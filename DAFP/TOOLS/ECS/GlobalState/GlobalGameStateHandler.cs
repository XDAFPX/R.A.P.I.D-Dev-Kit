using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState.Events;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using DAFP.TOOLS.Injection;
using MessagePipe;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 3) Your specialized handler now simply derives from the generic manager
    public abstract class GameStateHandler
        : GlobalStateHandler<IGameState>, IGameStateHandler
    {
        [Inject] private IPublisher<OnGameStateChangedEvent> e;

        protected override void OnTransition(IGameState previous, IGameState @new)
        {
            e.Publish(new OnGameStateChangedEvent(this,@new,previous));
        }

        public void TransitionToCriticalFailureState()
        {
            var _pstate = StateMachine.CurTypedState;
            var _state = Injector.Instantiate<ErrorGameState>();

            StateMachine.ForceTakeTransition(new SimpleTransition<IGameState>(_state));
            OnTransition(_pstate, _state);
        }
    }

    internal class ErrorGameState : StateBase, IGameState
    {
        public override string StateName { get; } = "Error";
        public override BtStatus LastStatus { get; } = BtStatus.Failure;
        [Inject] private IMusicMan man;
        [Inject] private IAudioSystem audio;
        [Inject] private World world;
        [Inject] private ControllerManager controls;

        public bool CanTransitionTo(IState state)
        {
            return false;
        }

        public override void EnterState()
        {
            Debug.LogError("Critical failure state encountered!");
            man.Stop();
            world.Shutdown();
            controls.Controllers.DisableAll();
        }

        public override void Tick()
        {
        }

        public override void ExitState()
        {
        }
    }

    // 4) The game‐specific interfaces
    public interface IGameState : IState, IDefinedState
    {
    }

    public interface IGameStateHandler : Zenject.ITickable, IInitializable,
        IGlobalStateHandler<IGameState>
    {
        public void TransitionToCriticalFailureState();
    }
}