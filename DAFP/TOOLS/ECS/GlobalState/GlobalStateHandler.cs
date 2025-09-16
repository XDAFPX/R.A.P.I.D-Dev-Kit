using System;
using System.Collections.Generic;
using System.Linq;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using UnityEventBus;
using Object = System.Object;

namespace DAFP.TOOLS.ECS.GlobalState
{
    // 1) A generic request container
    public class StateChangeRequest<T> where T : class, IState
    {
        public T State;
        public int Priority;
        public string Author;

        public StateChangeRequest(T state, int priority, string author)
        {
            State = state;
            Priority = priority;
            Author = author;
        }

        public virtual void Apply(StateMachine<T> sm)
        {
            sm.ForceTakeTransition(new SimpleTransition<T>(State), true);
        }
    }

    public struct GlobalStateChangedEvent<T> : IGlobalStateChanged where T : IState
    {
        public T Old;
        public T New;

        public GlobalStateChangedEvent(T @new, T old, object author)
        {
            New = @new;
            Old = old;
            Author = author;
        }

        public object Author { get; }
        public IState OldState => Old;
        public IState NewState => New;
    }

    public interface IGlobalStateChanged
    {
        public object Author { get; }
        public IState OldState { get; }
        public IState NewState { get; }
    }

    // 2) The generic state‐manager
    public abstract class GlobalStateHandler<T> : Zenject.ITickable, Zenject.IInitializable, IGlobalStateHandlerBase,
        ISavable where T : class, IState
    {
        protected virtual IEventBus CustomBus => null;
        private T[] States { get; }
        public T Default { get; }
        protected StateMachine<T> StateMachine;
        protected readonly List<StateChangeRequest<T>> Queue;

        public T GetState(string name)
        {
            return States.FirstOrDefault((state => state.StateName == name));
        }

        protected abstract T[] GetPreBuildStates();

        protected GlobalStateHandler(string defaultState, GlobalStates states)
        {
            states.Register(this);
            States = GetPreBuildStates();
            Default = States.FirstOrDefault((state => state.StateName == defaultState));
            if (Default == null)
                throw new Exception(
                    $"YOUR DEFAULT STATE ({defaultState}) IS NOT FOUND IN PREBUILD STATES AAAAAAAAAAAAAAA");
            Queue = new List<StateChangeRequest<T>>();
            StateMachine = new StateMachine<T>(Default, "Global");
        }

        public StateChangeRequest<T> PushState(StateChangeRequest<T> request)
        {
            if (request == null) return null;
            Queue.Add(request);
            ApplyTopState();
            return request;
        }

        public void PopState(StateChangeRequest<T> request)
        {
            if (request == null) return;
            Queue.Remove(request);
            ApplyTopState();
        }

        protected virtual void ApplyTopState()
        {
            T old = Current();
            if (Queue.Count > 0)
            {
                var top = Queue.OrderByDescending(r => r.Priority).First();
                top.Apply(StateMachine);
            }
            else

            {
                StateMachine.TransitionToInitialState(true);
            }

            T current = Current();

            if (CustomBus != null && current != old)
            {
                CustomBus.Send(new GlobalStateChangedEvent<T>(current, old, this) as IGlobalStateChanged);
            }
        }

        public T Current()
        {
            return StateMachine.CurState as T;
        }


        public void Tick()
        {
            StateMachine.Tick();
        }

        public void Initialize()
        {
            StateMachine.Enter();
        }

        public abstract Dictionary<string, object> Save();
        public abstract void Load(Dictionary<string, object> save);

        public void ResetToDefault()
        {
            Queue.Clear();
            ApplyTopState();
        }
    }
}