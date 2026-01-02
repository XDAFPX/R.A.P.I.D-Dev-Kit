using BDeshi.BTSM;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 3) Your specialized handler now simply derives from the generic manager
    public abstract class GlobalGameStateHandler
        : GlobalStateHandler<IGlobalGameState>, IGlobalGameStateHandler
    {
        protected IGlobalCursorStateHandler cursorStateHandler;

        protected GlobalGameStateHandler(string defaultState, IGlobalCursorStateHandler cursorStateHandler,
            IEventBus bus
        ) : base(
            defaultState, bus)
        {
            this.cursorStateHandler = cursorStateHandler;
        }
    }

    // 4) The game‐specific interfaces
    public interface IGlobalGameState : IState
    {
    }

    public interface IGlobalGameStateHandler : Zenject.ITickable, IInitializable,
        IGlobalStateHandler<IGlobalGameState>
    {
    }
}