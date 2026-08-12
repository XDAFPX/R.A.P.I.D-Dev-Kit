using DAFP.TOOLS.ECS.GlobalState;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class TransitionToGameStateEnt<T> : EmptyEntity where T : IGameState, new()
    {
        [Inject] private IGameStateHandler handler;
        protected override void InitializeInternal()
        {
            
            handler.TryTransitionTo<T>();
            base.InitializeInternal();
        }
    }
}