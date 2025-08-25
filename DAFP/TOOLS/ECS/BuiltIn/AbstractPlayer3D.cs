using System.Collections;
using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using DAFP.TOOLS.ECS.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(JumpStrengthBoard))]
    [RequireComponent(typeof(MovementSpeedBoard))]
    [RequireComponent(typeof(GroundedBoard))]
    [RequireComponent(typeof(GravityBoard3D))]
    [RequireComponent(typeof(VelocityBoard3D))]
    [RequireComponent(typeof(AirModifierBoard))]
    [RequireComponent(typeof(UniversalMover3D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public abstract class AbstractPlayer3D : AbstractGamePlayer
    {
        public override ITicker<IEntity> EntityTicker => World.UpdateTicker;

        // component caches
        [GetComponentCache] private GroundedBoard groundedBoard;
        [GetComponentCache] private AirModifierBoard airModifierBoard;
        [GetComponentCache] private GravityBoard3D gravityBoard;
        [GetComponentCache] private JumpStrengthBoard jumpStrengthBoard;
        [GetComponentCache] private VelocityBoard3D velocityBoard;
        [GetComponentCache] private UniversalMover3D mover;
        [GetComponentCache] private UniversalCooldownController cooldowns;
        [GetComponentCache] private AccelerationBoard accelerationBoard;
        [GetComponentCache] private DecelerationBoard decelerationBoard;

        // tuning fields
        [Header("Hang-jump")] [SerializeField] private float HangJumpThreshold = 1f;
        [SerializeField] private float HangJumpGravity = 0.5f;

        [Header("Double Jump")] [SerializeField]
        private int DoubleJumps = 1;

        [SerializeField] [ReadOnly(OnlyWhilePlaying = true)]
        private Cooldown JumpBuffer;

        [SerializeField] [ReadOnly(OnlyWhilePlaying = true)]
        private Cooldown CoyoteTime;

        [Header("Dash")] [SerializeField] private int DashCharges = 1;
        [SerializeField] private Cooldown dashReload;
        [SerializeField] private Cooldown dashUse;
        [SerializeField] private float DashDistance = 5f;
        [SerializeField] private float DashSpeedMultiplier = 2f;
        [SerializeField] private bool RequireGroundToReload = true;

        [Header("Slide")] [SerializeField] private float SlideSpeedBoost = 5f;
        [SerializeField] private float SlideDragMultiplier = 0.3f;

        [Header("Wall-run")] [SerializeField] private float WallRunSpeed = 5f;
        [SerializeField] private float WallRunStickForce = 2f;
        [SerializeField] private float WallRunDetectionDist = 1f;

        [Header("Grapple")] [SerializeField] private Transform GrappleStartPoint;
        [SerializeField] private float GrappleSpring = 8f;
        [SerializeField] private float GrappleDamp = 5f;
        [SerializeField] private float GrappleMinPct = 0.1f;
        [SerializeField] private float GrappleMaxPct = 0.4f;

        // runtime state
        private int jumpsLeft, dashLeft;
        private MultiplyFloatModifier HangGravityModifier;
        private SpringJoint grappleJoint;
        private Transform grappleAnchor;
        private RaycastHit currentWall;
        private Vector2 lastMovementInput = Vector2.zero;

        // states
        private IState AirIdle => GetOrCreateState(null, null, AirIdleTick, "AirIdle");
        private IState GroundIdle => GetOrCreateState(null, null, GroundIdleTick, "GroundIdle");

        // Moving ticks (delegates to the same logic as idle)
        private IState AirMoving => GetOrCreateState(null, null, AirIdleTick, "AirMoving");
        private IState GroundMoving => GetOrCreateState(null, null, GroundIdleTick, "GroundMoving");


        private IState AirStateWrapper => GetOrCreateState(() => AirMachine, "Air3D");
        private IState GroundWrapper => GetOrCreateState(() => GroundMachine, "Ground3D");

        private StateMachine<IState> AirMachine =>
            GetOrCreateStateMachine(() => AirIdle, "InAirSM") as StateMachine<IState>;

        private StateMachine<IState> GroundMachine =>
            GetOrCreateStateMachine(() => GroundIdle, "OnGroundSM") as StateMachine<IState>;

        protected override void SetInitialData()
        {
            // jump gravity modifier
            HangGravityModifier = new MultiplyFloatModifier(new QuikStat<float>(HangJumpGravity), this);
            AirModifier = new MultiplyFloatModifier(airModifierBoard, this);
            // cooldowns
            cooldowns.RegisterCooldown(JumpBuffer);
            cooldowns.RegisterCooldown(CoyoteTime);
            cooldowns.RegisterCooldown(dashReload);
            cooldowns.RegisterCooldown(dashUse);

            // initial counters
            jumpsLeft = DoubleJumps;
            dashLeft = DashCharges;
        }

        public MultiplyFloatModifier AirModifier { get; set; }

        protected override IState GetInitialState() => GroundWrapper;

        protected override void AddInitialStates(ref StateMachine<IState> sm)
        {
            // ground → air
            sm.AddTransition(GroundWrapper, AirStateWrapper,
                () => !groundedBoard.Value,
                () =>
                {
                    decelerationBoard.AddModifier(AirModifier);
                    accelerationBoard.AddModifier(AirModifier);
                });

            // air → ground
            sm.AddTransition(AirStateWrapper, GroundWrapper,
                () => groundedBoard.Value,
                () =>
                {
                    jumpsLeft = DoubleJumps;
                    if (!RequireGroundToReload) dashLeft = DashCharges;
                    decelerationBoard.RemoveModifier(AirModifier);
                    accelerationBoard.RemoveModifier(AirModifier);
                });
            GroundMachine.AddTransition(GroundIdle, GroundMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            GroundMachine.AddTransition(GroundMoving, GroundIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);

            // inside air machine: idle <-> moving
            AirMachine.AddTransition(AirIdle, AirMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            AirMachine.AddTransition(AirMoving, AirIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);
        }

        private void GroundIdleTick()
        {
            CoyoteTime.SetToMin();
            CheckForJumping();
        }

        private void AirIdleTick()
        {
            if (Mathf.Abs(velocityBoard.Value.y) < HangJumpThreshold)
                gravityBoard.AddModifier(HangGravityModifier);
            else
                gravityBoard.RemoveModifier(HangGravityModifier);

            CheckForJumping();
        }

        protected void CheckForJumping()
        {
            if (!JumpBuffer.isComplete && !CoyoteTime.isComplete)
            {
                JumpBuffer.SetToMax();

                mover.DoJump(transform.up * jumpStrengthBoard.Value);
            }
        }

        // input API
        public virtual void InputMovement(Vector2 v)
        {
            var dir = new Vector3(v.x, 0, v.y).normalized;
            Vector3 worldMove = transform.TransformDirection(dir).normalized;
            lastMovementInput = dir;
            mover.Input(worldMove);
        }

        public virtual void InputJump()
        {
            JumpBuffer.Reset();
            CoyoteTime.SetToMax();
        }

        public virtual void InputCutJump() => mover.DoCutJump(transform.up, 2f);
        public virtual void InputDash() => dashUse.Reset();

        public virtual void InputSlide()
        {
            /* handled every frame */
        }

        public virtual void InputGrapple()
        {
            /* handled every frame */
        }
    }
}