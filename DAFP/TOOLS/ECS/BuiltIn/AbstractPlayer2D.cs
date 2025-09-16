using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using DAFP.TOOLS.ECS.Components;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(MovementSpeedBoard))]
    [RequireComponent(typeof(GroundedBoard))]
    [RequireComponent(typeof(GravityBoard2D))]
    [RequireComponent(typeof(VelocityBoard2D))]
    [RequireComponent(typeof(AirModifierBoard))]
    [RequireComponent(typeof(UniversalMover2D))]
    [RequireComponent(typeof(UniversalJumper2D))]
    public abstract class AbstractPlayer2D : AbstractGamePlayer
    {
        [GetComponentCache] private GroundedBoard groundedBoard;
        [GetComponentCache] private AirModifierBoard airModifierBoard;
        [GetComponentCache] private AccelerationBoard accelerationBoard;
        [GetComponentCache] private DecelerationBoard decelerationBoard;
        [GetComponentCache] private GravityBoard2D gravityBoard;
        [GetComponentCache] private VelocityBoard2D velocityBoard;
        [GetComponentCache] private UniversalMover2D universalMover;
        [GetComponentCache] private UniversalJumper2D universalJumper;

        [Inject(Id = "DefaultUpdateEntityGameplayTicker")]
        public override ITicker<IEntity> EntityTicker { get; }

        [SerializeField] private float HangJumpThreshold;
        [SerializeField] private float HangJumpModifier;

        private StatModifier<float> AirModifier;
        private StatModifier<float> HangGravityModifier;

        // store last movement input for state transitions
        [SerializeField] protected Vector2 lastMovementInput = Vector2.zero;

        // Idle and moving states…
        private IState AirIdle => GetOrCreateState(null, null, AirIdleTick, "AirIdle");
        private IState GroundIdle => GetOrCreateState(null, null, GroundedIdleTick, "GroundIdle");
        private IState AirMoving => GetOrCreateState(null, null, AirMovingTick, "AirMoving");
        private IState GroundMoving => GetOrCreateState(null, null, GroundMovingTick, "GroundMoving");

        // Wrappers for ground/air sub-machines
        private IState AirTimeWrapper => GetOrCreateState(() => AirTimeStateMachine, "AirTimeWrapper");
        private IState GroundTimeWrapper => GetOrCreateState(() => GroundTimeStateMachine, "GroundTimeWrapper");

        private StateMachine<IState> AirTimeStateMachine =>
            GetOrCreateStateMachine(() => AirIdle, "InAirStateMachine") as StateMachine<IState>;
        private StateMachine<IState> GroundTimeStateMachine =>
            GetOrCreateStateMachine(() => GroundIdle, "OnGroundStateMachine") as StateMachine<IState>;

        protected override void SetInitialDataAfterBinds()
        {
            AirModifier = new MultiplyFloatModifier(airModifierBoard, this);
            HangGravityModifier = new MultiplyFloatModifier(new QuikStat<float>(HangJumpModifier), this);
        }

        protected override IState GetInitialState() => GroundTimeWrapper;

        protected override void AddInitialStates(ref StateMachine<IState> sm)
        {
            // top-level transitions between ground & air
            sm.AddTransition(
                GroundTimeWrapper, AirTimeWrapper,
                () => !groundedBoard.Value,
                () =>
                {
                    decelerationBoard.AddModifier(AirModifier);
                    accelerationBoard.AddModifier(AirModifier);
                });

            sm.AddTransition(
                AirTimeWrapper, GroundTimeWrapper,
                () => groundedBoard.Value,
                () =>
                {
                    decelerationBoard.RemoveModifier(AirModifier);
                    accelerationBoard.RemoveModifier(AirModifier);
                });

            // ground: idle <-> moving
            GroundTimeStateMachine.AddTransition(
                GroundIdle, GroundMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            GroundTimeStateMachine.AddTransition(
                GroundMoving, GroundIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);

            // air: idle <-> moving
            AirTimeStateMachine.AddTransition(
                AirIdle, AirMoving,
                () => lastMovementInput.sqrMagnitude > 0f, null);
            AirTimeStateMachine.AddTransition(
                AirMoving, AirIdle,
                () => lastMovementInput.sqrMagnitude <= 0f, null);
        }

        protected virtual void GroundedIdleTick()
        {
            universalJumper.TickGround();
        }

        protected virtual void AirIdleTick()
        {
            if (Mathf.Abs(velocityBoard.Value.y) < HangJumpThreshold)
                gravityBoard.AddModifier(HangGravityModifier);
            else
                gravityBoard.RemoveModifier(HangGravityModifier);

            universalJumper.TickAir();
        }

        protected virtual void AirMovingTick()
        {
            if (Mathf.Abs(velocityBoard.Value.y) < HangJumpThreshold)
                gravityBoard.AddModifier(HangGravityModifier);
            else
                gravityBoard.RemoveModifier(HangGravityModifier);

            universalJumper.TickAir();
            InputMovement(lastMovementInput);
        }

        protected virtual void GroundMovingTick()
        {
            universalJumper.TickGround();
            InputMovement(lastMovementInput);
        }

        [BindName("OnMove")]
        private void OnMove(InputAction.CallbackContext ctx)
        {
            lastMovementInput = ctx.ReadValue<Vector2>();
        }

        [BindName("OnJump")]
        private void OnJump(InputAction.CallbackContext ctx)
        {
            universalJumper.OnJump(ctx);
        }

        protected virtual void InputMovement(Vector2 input)
        {
            input.Normalize();
            universalMover.Input(input);
        }
    }
}