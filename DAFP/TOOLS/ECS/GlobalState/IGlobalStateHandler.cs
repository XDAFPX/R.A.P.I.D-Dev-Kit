using System;
using BDeshi.BTSM;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public interface IGlobalStateHandler<TState> : Zenject.ITickable, IInitializable, IGlobalStateHandlerBase
        where TState : class, IState
    {
        public abstract TState Default { get; }

        public TState Current { get; }
        public IGlobalStateHandler<TState> TransitionTo<TConcreteState>() where TConcreteState : TState, new();

        public IGlobalStateHandler<TState> TransitionToDefault();
    }
}