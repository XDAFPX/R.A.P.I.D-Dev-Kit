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
    [RequireComponent(typeof(GravityBoard2D))]
    [RequireComponent(typeof(VelocityBoard2D))]
    [RequireComponent(typeof(AirModifierBoard))]
    [RequireComponent(typeof(UniversalMover2D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public abstract class AbstractPlayer2D : AbstractGamePlayer
    {
        [GetComponentCache] private GroundedBoard groundedBoard;
        [GetComponentCache] private AirModifierBoard airModifierBoard;
        [GetComponentCache] private AccelerationBoard accelerationBoard;
        [GetComponentCache] private DecelerationBoard decelerationBoard;
        [GetComponentCache] private GravityBoard2D gravityBoard;
        [GetComponentCache] private JumpStrengthBoard jumpStrengthBoard;
        [GetComponentCache] private VelocityBoard2D velocityBoard;
        [GetComponentCache] private UniversalMover2D universalMover;
        [GetComponentCache] private UniversalCooldownController universalCooldownController;
        public override ITicker<IEntity> EntityTicker => World.UpdateTicker;

        [SerializeField] private float HangJumpThreshold;
        [SerializeField] private float HangJumpModifier;

        private StatModifier<float> AirModifier;
        private StatModifier<float> HangGravityModifier;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private Cooldown CoyoteTime;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private Cooldown JumpBuffer;

        // store last movement input for state transitions
        private Vector2 lastMovementInput = Vector2.zero;

        // Idle ticks
        private IState AirIdle => GetOrCreateState(null, null, AirIdleTick, "AirIdle");
        private IState GroundIdle => GetOrCreateState(null, null, GroundedIdleTick, "GroundIdle");

        // Moving ticks (delegates to the same logic as idle)
        private IState AirMoving => GetOrCreateState(null, null, AirIdleTick, "AirMoving");
        private IState GroundMoving => GetOrCreateState(null, null, GroundedIdleTick, "GroundMoving");

        // Wrappers that contain the per‐mode state machines
        private IState AirTimeWrapper   => GetOrCreateState(() => AirTimeStateMachine,   "AirTimeWrapper");
        private IState GroundTimeWrapper=> GetOrCreateState(() => GroundTimeStateMachine,"GroundTimeWrapper");

        private StateMachine<IState> AirTimeStateMachine =>
            GetOrCreateStateMachine(() => AirIdle,   "InAirStateMachine")   as StateMachine<IState>;
        private StateMachine<IState> GroundTimeStateMachine =>
            GetOrCreateStateMachine(() => GroundIdle,"OnGroundStateMachine") as StateMachine<IState>;

        protected override void SetInitialData()
        {
            AirModifier         = new MultiplyFloatModifier(airModifierBoard, this);
            HangGravityModifier = new MultiplyFloatModifier(new QuikStat<float>(HangJumpModifier), this);
            universalCooldownController.RegisterCooldown(CoyoteTime);
            universalCooldownController.RegisterCooldown(JumpBuffer);
        }

        protected override IState GetInitialState() => GroundTimeWrapper;

        protected override void AddInitialStates(ref StateMachine<IState> sm)
        {
            // top‐level transitions between ground & air
            sm.AddTransition(GroundTimeWrapper, AirTimeWrapper,
                () => !groundedBoard.Value,
                () =>
                {
                    decelerationBoard.AddModifier(AirModifier);
                    accelerationBoard.AddModifier(AirModifier);
                });

            sm.AddTransition(AirTimeWrapper, GroundTimeWrapper,
                () => groundedBoard.Value,
                () =>
                {
                    decelerationBoard.RemoveModifier(AirModifier);
                    accelerationBoard.RemoveModifier(AirModifier);
                });

            // inside ground machine: idle <-> moving
            GroundTimeStateMachine.AddTransition(GroundIdle, GroundMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            GroundTimeStateMachine.AddTransition(GroundMoving, GroundIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);

            // inside air machine: idle <-> moving
            AirTimeStateMachine.AddTransition(AirIdle, AirMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            AirTimeStateMachine.AddTransition(AirMoving, AirIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);
        }

        private void GroundedIdleTick()
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
                universalMover.DoJump(transform.up * jumpStrengthBoard.Value);
            }
        }

        public virtual void InputCutJump()
        {
            CoyoteTime.SetToMax();
            universalMover.DoCutJump(transform.up, 2f);
        }

        public virtual void InputJump()
        {
            JumpBuffer.SetToMin();
        }

        // 1) capture movement input for state transitions
        public virtual void InputMovement(Vector2 input)
        {
            input.Normalize();
            lastMovementInput = input;
            universalMover.Input(input);
        }
    }
}