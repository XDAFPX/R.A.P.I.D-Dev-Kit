using System;
using System.Collections.Generic;
using System.Linq;
using BDeshi.BTSM;

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

    // 2) The generic state‐manager
        public abstract class GlobalStateManager<T> : Zenject.ITickable,Zenject.IInitializable where T : class, IState
        {
            private T[] States { get; }
            public T Default { get; }
            protected StateMachine<T> StateMachine;
            protected readonly List<StateChangeRequest<T>> Queue;

            public T GetState(string name)
            {
                return States.FirstOrDefault((state => state.StateName == name));
            }

            protected abstract T[] GetPreBuildStates();

            protected GlobalStateManager(string defaultState)
            {
                States = GetPreBuildStates();
                Default = States.FirstOrDefault((state => state.StateName == defaultState));
                if (Default == null)
                    throw new Exception("YOUR DEFAULT STATE IS NOT FOUND IN PREBUILD STATES AAAAAAAAAAAAAAA");
                Queue = new List<StateChangeRequest<T>>();
                StateMachine = new StateMachine<T>(Default, "Global");
            }

            public void PushState(StateChangeRequest<T> request)
            {
                if (request == null) return;
                Queue.Add(request);
                ApplyTopState();
            }

            public void PopState(StateChangeRequest<T> request)
            {
                if (request == null) return;
                Queue.Remove(request);
                ApplyTopState();
            }

            protected virtual void ApplyTopState()
            {
                if (Queue.Count > 0)
                {
                    var top = Queue.OrderByDescending(r => r.Priority).First();
                    if (!Object.Equals(StateMachine.CurState, top.State))
                        top.Apply(StateMachine);
                }
                else

                {
                    StateMachine.TransitionToInitialState(true);
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
        }
    }