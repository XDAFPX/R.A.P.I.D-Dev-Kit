using BDeshi.BTSM;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 3) Your specialized handler now simply derives from the generic manager
    public abstract class GlobalGameStateHandler
        : GlobalStateManager<IGlobalGameState>, IGlobalGameStateHandler
    {
        protected IGlobalCursorStateHandler cursorStateHandler;

        protected GlobalGameStateHandler(string defaultState, IGlobalCursorStateHandler cursorStateHandler) : base(
            defaultState)
        {
            this.cursorStateHandler = cursorStateHandler;
        }
    }

    // 4) The game‐specific interfaces
    public interface IGlobalGameState : IState
    {
    }

    public interface IGlobalGameStateHandler : Zenject.ITickable, Zenject.IInitializable
    {
        IGlobalGameState Default { get; }
        void PushState(StateChangeRequest<IGlobalGameState> request);
        void PopState(StateChangeRequest<IGlobalGameState> request);
        IGlobalGameState Current();
        IGlobalGameState GetState(string name);
    }
}