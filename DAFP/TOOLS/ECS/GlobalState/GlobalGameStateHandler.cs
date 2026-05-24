using BDeshi.BTSM;
using DAFP.TOOLS.ECS.GlobalState.Events;
using DAFP.TOOLS.Injection;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 3) Your specialized handler now simply derives from the generic manager
    public abstract class GameStateHandler
        : GlobalStateHandler<IGameState>, IGameStateHandler
    {
        [Inject(Id = IVideoGame.GAME_BUS_NAME)]
        private IEventBus bus;
        protected override void OnTransition(IGameState previous, IGameState @new)
        {
            ((IEventBus)bus).Send(new OnGameStateChanged(){New = @new,Previous = previous});
        }
    }

    // 4) The game‐specific interfaces
    public interface IGameState : IState,IDefinedState
    {
    }

    public interface IGameStateHandler : Zenject.ITickable, IInitializable,
        IGlobalStateHandler<IGameState>
    {
    }
}