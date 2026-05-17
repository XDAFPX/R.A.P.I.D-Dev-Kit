using System;
using System.Collections.Generic;
using System.Linq;
using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEventBus;
using Zenject;
using Object = System.Object;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 1) A generic request container


    // 2) The generic state‐manager
    public abstract class GlobalStateHandler<T> : Zenject.ITickable, Zenject.IInitializable,
        IGlobalStateHandler<T>, ISavable where T : class, IState
    {
        public abstract T Default { get; }
        public T Current => StateMachine.CurTypedState;
        protected StateMachine<T> StateMachine;
        [Inject] private DiContainer injector;

        public void Initialize()
        {
            StateMachine = new StateMachine<T>(Default, this.GetType().Name);
            StateMachine.Enter();
        }

        public void Tick()
        {
            StateMachine.Tick();
        }


        public void ResetToDefault()
        {
            StateMachine.ExitCurState();
            Initialize();
        }

        public ISaveData Save()
        {
            // return new GenericSaveData(new Dictionary<string, object>() { { "index", States[(T)StateMachine.CurState] } });
            return new GenericSaveData(new Dictionary<string, object>()
                { { "CurrentState", StateMachine.CurState.GetType().FullName } });
        }

        public void Load(ISaveData saveData)
        {
            //--TODO Implement
        }

        public IGlobalStateHandler<T> TransitionTo<TConcreteState>() where TConcreteState : T, new()
        {
            var _pstate = StateMachine.CurTypedState;
            var _state = injector.Instantiate<TConcreteState>();
            StateMachine.ForceTakeTransition(new SimpleTransition<T>(_state));
            OnTransition(_pstate, _state);
            return this;
        }

        public IGlobalStateHandler<T> TransitionToDefault()
        {
            var _pstate = StateMachine.CurTypedState;
            StateMachine.TransitionToInitialState();
            OnTransition(_pstate, StateMachine.CurTypedState);
            return this;
        }

        protected virtual void OnTransition(T previous, T @new)
        {
        }
    }
}