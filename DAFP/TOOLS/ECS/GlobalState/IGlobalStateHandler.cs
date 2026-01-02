using System;
using BDeshi.BTSM;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public interface IGlobalStateHandler<TState> : Zenject.ITickable, IInitializable, IGlobalStateHandlerBase
        where TState : class, IState
    {
        TState Default { get; }
        StateChangeRequest<TState> PushState(StateChangeRequest<TState> request);
        void PopState(StateChangeRequest<TState> request);
        TState Current();
        TState GetState(string name);

        Type GetStateType()
        {
            return typeof(TState);
        }
    }
}