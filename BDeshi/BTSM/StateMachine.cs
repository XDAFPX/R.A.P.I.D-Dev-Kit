
using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.BTs;
using UnityEngine;

namespace BDeshi.BTSM
{
    public interface IRunnableStateMachine
    {
        /// <summary>
        /// Transitions list for this state
        /// </summary>
        IEnumerable<ITransitionBase> ActiveTransitions { get; }

        /// <summary>
        /// Transitions that are always active
        /// </summary>
        IEnumerable<ITransitionBase> GlobalTransitions { get; }
        /// <summary>
        /// Transitions that are not evaluated by the fsm
        /// But can be made manually
        /// and will be tracked
        /// You can change states without this
        /// But that won't be shown in the UI
        /// </summary>
        IEnumerable<ITransitionBase> ManualTransition { get; }
        public BlackBoard BlackBoard { get; set; }
        public bool ShouldLog { get; set; }
        IState CurState { get; }
        IState[] GetAllStates();
        public GameObject DebugContext { get; set; }
        void Enter(bool callEnter = true);
        void Tick();
        void Cleanup();

        bool TryGetTransitionsForState(IState state, out IEnumerable<ITransitionBase> transitionsForState);
    }

    public class StateMachine<TState> : IRunnableStateMachine
    where TState : class, IState
    {
        public GameObject DebugContext{ get; set; } = null;

        public IState CurState => CurTypedState;
        public TState CurTypedState { get; private set; }

        public TState StartingState;


        public IEnumerable<ITransitionBase> ActiveTransitions => ActiveTransitionsL;

        /// <summary>
        /// Transitions list for this state
        /// </summary>
        public List<ITransition<TState>> ActiveTransitionsL { get; protected set; }

        /// <summary>
        /// This is there in case current state does not have transitions
        /// and so that we don't have to create a new list
        /// </summary>
        public static readonly List<ITransition<TState>> EMPTY_TRANSITIONS =new List<ITransition<TState>>();
        
        /// <summary>
        /// Transitions from a state to another
        /// </summary>
        public Dictionary<TState, List<ITransition<TState>>> Transitions = new Dictionary<TState, List<ITransition<TState>>>();

        public IEnumerable<ITransitionBase> GlobalTransitions => GlobalTransitionsL;

        /// <summary>
        /// Transitions that are always active
        /// </summary>
        public List<ITransition<TState>> GlobalTransitionsL = new List<ITransition<TState>>();

        public IEnumerable<ITransitionBase> ManualTransition => ManualTransitions;
        public List<ITransition<TState>> ManualTransitions = new List<ITransition<TState>>();
        private IState[] states = null;
        private Action<TState, TState> onStateTransitioned;
        public bool ShouldLog { get; set; } = false;

        private BlackBoard blackBoard ;

        BlackBoard IRunnableStateMachine.BlackBoard { get => blackBoard; set => blackBoard = value; }

        //hack
        private IState[] CreateAllStatesList()
        {
            HashSet<TState> _statesHash = new HashSet<TState>();
            foreach (var _p in Transitions)
            {
                _statesHash.Add(_p.Key);

                foreach (var _transition in _p.Value)
                {
                    _statesHash.Add(_transition.SuccessTypedState);
                }
            }

            foreach (var _transition in GlobalTransitionsL)
            {
                _statesHash.Add(_transition.SuccessTypedState);
            }

            foreach (var _transition in ManualTransitions)
            {
                _statesHash.Add(_transition.SuccessTypedState);
            }

            return _statesHash.ToArray();
        }
        public IState[] GetAllStates()
        {
            if (states == null)
            {
                states = CreateAllStatesList();
            }

            return states;
        }
        
        
        public StateMachine(TState startingState)
        {
            this.StartingState = startingState;
        }

        public void Enter(bool callEnter = true)
        {
            TransitionToInitialState(callEnter);
        }

        public void TransitionToInitialState(bool callEnter = true)
        {
            TransitionTo(StartingState, callEnter);
        }

        public void ExitCurState()
        {
            IState _cur = CurState;
            while (_cur != null)
            {
                _cur.ExitState();
                _cur = _cur.Parent;
            }
        }

        public void ForceTakeTransition(ITransition<TState> t, bool reEnter = false)
        {

            #if UNITY_EDITOR
                foreach (var _transition in GlobalTransitionsL)
                {
                      _transition.TakenLastTime = false;
                }

                foreach (var _activeTransition in GlobalTransitionsL)
                {
                    _activeTransition.TakenLastTime = false;
                }
                
                foreach (var _transition in ManualTransitions)
                {
                    _transition.TakenLastTime = false;
                }
            #endif
            

            t.OnTaken?.Invoke();
            TransitionTo(t.SuccessTypedState, true, reEnter);
            
            //do this after transition 
            //otherwise it would get overwritten if we transitioned to same state
            t.TakenLastTime = true;
        }
        public void Tick()
        {
            //Debug.Log(GetAllStates().Length);
            CurState.Tick();

            IState _newState = null;
            ITransition<TState> _takenTransition = null;
            foreach (var _activeTransition in ActiveTransitionsL)
            {
                //Debug.Log(ActiveTransitionsL);
                if (_activeTransition.Evaluate())
                {
                    _takenTransition = _activeTransition;
                    break;
                }
            }

            if (_newState == null)
            {
                foreach (var _activeTransition in GlobalTransitionsL)
                {
                    if (_activeTransition.Evaluate())
                    {
                        _takenTransition = _activeTransition;
                        LOG("global transition from " + (CurState.FullStateName) + " -> " +  (_takenTransition.SuccessTypedState.FullStateName));

                        break;
                    }
                }
            }
            if(_takenTransition != null)
                ForceTakeTransition(_takenTransition);
        }

        void LOG(string text)
        {
            if (ShouldLog)
                Debug.Log(text, DebugContext);
        }

        /// <summary>
        /// Transitions to a state given that it is null
        /// Calls oldstate.exit() if it is not null
        /// Then sets up newState via newState.enter()
        /// Handles recursion.
        /// </summary>
        /// <param name="newState">
        /// Limit this to states this Dictionary knows about. Otherwise, the Actions/Transitions will not work
        /// </param>
        /// <param name="callEnter">
        /// If true, call the enter function in the state(s) transitioned to
        /// Usecase: initialize curState without calling enter
        /// </param>
        /// <param name="forceEnterIfSameState"></param>
        public void TransitionTo(TState newState, bool callEnter = true, bool forceEnterIfSameState = false)
        {
            if (newState != null && (newState != CurTypedState || forceEnterIfSameState))
            {
                var _prevState = CurTypedState;

                LOG((CurState == null?"null": CurState.FullStateName)  + " -> " + newState.FullStateName);

                if(callEnter)
                {
                    if (forceEnterIfSameState && newState == CurState)
                    {
                        if(CurState != null)
                            CurState.ExitState();
                        CurTypedState = newState;
                        CurState.EnterState();
                    }
                    else
                    {
                        // recursiveTransitionToState(newState);
                        if(CurState != null)
                                CurState.ExitState();
                        CurTypedState = newState;
                        CurState.EnterState();
                    }
                }
                else
                {
                    CurTypedState = newState;
                }
                

                HandleTransitioned(_prevState, newState);
            }
        }

        /// <summary>
        /// Set transitions list to curState's.
        /// </summary>
        protected virtual void HandleTransitioned(TState prevState, TState newState)
        {
            ActiveTransitionsL = Transitions.GetValueOrDefault(CurTypedState, EMPTY_TRANSITIONS);
            //need to clear active transition list of new cur state
#if UNITY_EDITOR
            foreach (var _activeTransition in ActiveTransitionsL)
            {
                _activeTransition.TakenLastTime = false;
            }
#endif
            onStateTransitioned?.Invoke(prevState, newState);
        }


        void RecursiveTransitionToState(TState to)
        {
            var _cur = CurState;

            IState _commonParent = null;
            while (_cur != null)
            {
                var _toParent = to;
                while (_toParent != null)
                {
                    if (_toParent == _cur)
                    {
                        _commonParent = _cur;
                        break;
                    }
                    _toParent = _toParent.Parent as TState;
                }
                

                if (_commonParent != null)
                    break;
                
                _cur.ExitState();
                _cur = _cur.Parent;
            }
            // Debug.Log( curState?.FullStateName + "->"+ to?.FullStateName+" to "  +commonParent?.FullStateName);
            CallEnterRecursive(to, _commonParent);
            CurTypedState = to;
        }
        /// <summary>
        /// Recurse to some parent and call enterstate of all childs recursively down to the passed one
        /// </summary>
        /// <param name="child"> The child we start recursing from. DO NOT MAKE THIS == PARENT</param>
        /// <param name="limitParent">The parent we won't call enter on. </param>
        void CallEnterRecursive(IState child, IState limitParent)
        {
            if(child == null || child == limitParent)
                return;

            CallEnterRecursive(child.Parent, limitParent);
            child.EnterState();
        }

        public ITransition<TState> AddTransition(TState from, ITransition<TState> t)
        {
            if(Transitions.TryGetValue(from, out var _l))
                _l.Add(t);
            else
            {
                Transitions.Add(from, new List<ITransition<TState>>(){t});
            }

            return t;
        }

        
        public ITransition<TState> AddTransition(TState from, TState to, Func<bool> condition, System.Action onTaken = null )
        {
            return AddTransition(from, new SimpleTransition<TState>(to, condition, onTaken));
        }
        
        public ITransition<TState> AddGlobalTransition(TState to, Func<bool> condition, System.Action onTaken = null )
        {
            return AddGlobalTransition(new SimpleTransition<TState>(to, condition, onTaken));
        }
        /// <summary>
        /// fsm never checks these during tick
        /// But can be used via forceTakeTransition()
        /// And also shows up in Editor
        /// </summary>
        /// <param name="to"></param>
        /// <param name="onTaken"></param>
        /// <returns></returns>
        public ITransition<TState> AddManualTransitionTo(TState to, System.Action onTaken = null )
        {
            var _t = new SimpleTransition<TState>(to, null, onTaken);
            ManualTransitions.Add(_t);
            return _t;
        }

        public ITransition <TState>AddGlobalTransition(ITransition<TState> t)
        {
            GlobalTransitionsL.Add(t);
            return t;
        }
        

        public void Cleanup()
        {
            if (CurState != null)
            {
                CurState.ExitState();
            }
        }

        public bool TryGetTransitionsForState(IState state, out IEnumerable<ITransitionBase> transitionsForState)
        {
            if (state is TState _tstate)
            {
                if (Transitions.TryGetValue(_tstate, out var _v))
                {
                    transitionsForState = _v;
                    return true;
                }
            }

            transitionsForState = default(IEnumerable<ITransitionBase>);
            return false;
        }
    }
}