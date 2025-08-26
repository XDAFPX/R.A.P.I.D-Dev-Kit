using System.Collections;
using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
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
        [GetComponentCache] protected GroundedBoard groundedBoard;
        [GetComponentCache] protected AirModifierBoard airModifierBoard;
        [GetComponentCache] protected GravityBoard3D gravityBoard;
        [GetComponentCache] protected JumpStrengthBoard jumpStrengthBoard;
        [GetComponentCache] protected VelocityBoard3D velocityBoard;
        [GetComponentCache] protected UniversalMover3D mover;
        [GetComponentCache] protected UniversalCooldownController cooldowns;
        [GetComponentCache] protected AccelerationBoard accelerationBoard;
        [GetComponentCache] protected DecelerationBoard decelerationBoard;

        // tuning fields
        [Header("Hang-jump")] [SerializeField] protected float HangJumpThreshold = 1f;
        [SerializeField] protected float HangJumpGravity = 0.5f;

        [Header("Double Jump")] [SerializeField]
        protected int DoubleJumps = 1;

        [SerializeField] [ReadOnly(OnlyWhilePlaying = true)]
        protected Cooldown JumpBuffer;

        [SerializeField] [ReadOnly(OnlyWhilePlaying = true)]
        protected Cooldown CoyoteTime;

        [Header("Dash")] [SerializeField] protected int DashCharges = 1;
        [SerializeField] protected Cooldown dashReload;
        [SerializeField] protected Cooldown dashUse;
        [SerializeField] protected float DashDistance = 5f;
        [SerializeField] protected float DashSpeedMultiplier = 2f;
        [SerializeField] protected bool RequireGroundToReload = true;

        [Header("Slide")] [SerializeField] protected float SlideSpeedBoost = 5f;
        [SerializeField] protected float SlideDragMultiplier = 0.3f;

        [Header("Wall-run")] [SerializeField] protected float WallRunSpeed = 5f;
        [SerializeField] protected float WallRunStickForce = 2f;
        [SerializeField] protected float WallRunDetectionDist = 1f;

        [Header("Grapple")] [SerializeField] protected Transform GrappleStartPoint;
        [SerializeField] protected float GrappleSpring = 8f;
        [SerializeField] protected float GrappleDamp = 5f;
        [SerializeField] protected float GrappleMinPct = 0.1f;
        [SerializeField] protected float GrappleMaxPct = 0.4f;

        // runtime state
        protected int jumpsLeft, dashLeft;
        protected MultiplyFloatModifier HangGravityModifier;
        protected SpringJoint grappleJoint;
        protected Transform grappleAnchor;
        protected RaycastHit currentWall;
        protected Vector2 lastMovementInput = Vector2.zero;

        // states
        protected IState AirIdle => GetOrCreateState(null, null, AirIdleTick, "AirIdle");
        protected IState GroundIdle => GetOrCreateState(null, null, GroundIdleTick, "GroundIdle");

        // Moving ticks (delegates to the same logic as idle)
        protected IState AirMoving => GetOrCreateState(null, null, AirMovingTick, "AirMoving");
        protected IState GroundMoving => GetOrCreateState(null, null, GroundMovingTick, "GroundMoving");


        protected IState AirStateWrapper => GetOrCreateState(() => AirMachine, "Air3D");
        protected IState GroundWrapper => GetOrCreateState(() => GroundMachine, "Ground3D");

        protected StateMachine<IState> AirMachine =>
            GetOrCreateStateMachine(() => AirIdle, "InAirSM") as StateMachine<IState>;

        protected StateMachine<IState> GroundMachine =>
            GetOrCreateStateMachine(() => GroundIdle, "OnGroundSM") as StateMachine<IState>;

        protected override void SetInitialDataAfterBinds()
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
                () => OnAirTimeStart());

            // air → ground
            sm.AddTransition(AirStateWrapper, GroundWrapper,
                () => groundedBoard.Value,
                () => OnGroundTimeStart());
            GroundMachine.AddTransition(GroundIdle, GroundMoving,
                () => lastMovementInput.sqrMagnitude > 0f, (OnMovementStart));
            GroundMachine.AddTransition(GroundMoving, GroundIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, (OnMovementStop));

            // inside air machine: idle <-> moving
            AirMachine.AddTransition(AirIdle, AirMoving,
                () => lastMovementInput.sqrMagnitude > 0f, OnMovementStart);
            AirMachine.AddTransition(AirMoving, AirIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, OnMovementStop);
        }

        protected virtual void OnMovementStop()
        {
            
        }

        protected virtual void OnMovementStart()
        {
            
        }

        protected virtual void OnGroundTimeStart()
        {
            jumpsLeft = DoubleJumps;
            if (!RequireGroundToReload) dashLeft = DashCharges;
            decelerationBoard.RemoveModifier(AirModifier);
            accelerationBoard.RemoveModifier(AirModifier);
        }

        protected virtual void OnAirTimeStart()
        {
            decelerationBoard.AddModifier(AirModifier);
            accelerationBoard.AddModifier(AirModifier);
        }

        protected virtual void GroundMovingTick()
        {
            CoyoteTime.SetToMin();
            CheckForJumping();
            InputMovement(lastMovementInput);
        }

        protected virtual void AirMovingTick()
        {
            if (Mathf.Abs(velocityBoard.Value.y) < HangJumpThreshold)
                gravityBoard.AddModifier(HangGravityModifier);
            else
                gravityBoard.RemoveModifier(HangGravityModifier);

            CheckForJumping();
            InputMovement(lastMovementInput);
        }

        protected virtual void GroundIdleTick()
        {
            CoyoteTime.SetToMin();
            CheckForJumping();
        }

        protected virtual void AirIdleTick()
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

        [BindName("OnMove")]
        protected virtual void OnMove(InputAction.CallbackContext ctx)
        {
            lastMovementInput = ctx.ReadValue<Vector2>();
        }

        [BindName("OnJump")]
        protected virtual void  OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled)
                InputCutJump();
            if (ctx.started)
                InputJump();
        }

        protected virtual void InputMovement(Vector2 v)
        {
            var dir = new Vector3(v.x, 0, v.y).normalized;
            Vector3 worldMove = transform.TransformDirection(dir).normalized;
            mover.Input(worldMove);
        }

        protected virtual void InputJump()
        {
            JumpBuffer.Reset();
            CoyoteTime.SetToMax();
        }

        protected virtual void InputCutJump() => mover.DoCutJump(transform.up, 2f);
        protected virtual void InputDash() => dashUse.Reset();

        protected virtual void InputSlide()
        {
            /* handled every frame */
        }

        protected virtual void InputGrapple()
        {
            /* handled every frame */
        }
    }
}