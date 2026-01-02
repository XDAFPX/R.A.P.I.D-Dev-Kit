using System;
using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public abstract class StateDrivenThinker : BaseThinker
    {
        protected StateMachine<IState> StateMachine;

        protected sealed override void InternalInitialize(IEntity host)
        {
            StateMachine = new StateMachine<IState>(GetInitialState(host), "RootBrain");
            RegisterInitialStates(ref StateMachine, host);
        }

        protected sealed override void InternalTick(IEntity host, ITickerBase ticker)
        {
            StateMachine.Tick();
        }

        protected sealed override void InternalDispose(IEntity host)
        {
            StateMachine = null;
        }

        protected abstract void RegisterInitialStates(ref StateMachine<IState> sm, IEntity host);
        protected abstract IState GetInitialState(IEntity host);

        private List<IState> btStateCache = new();
        private List<IRunnableStateMachine> smCache = new();

        protected BtWrapperState GetOrCreateState(Func<BtNodeBase> root, string state_name)
        {
            return BehaviourTreeUtil.GetOrCreateState(root, state_name, btStateCache);
        }

        protected ModularState GetOrCreateState(
            Action Enter, Action Exit, Action Tick,
            string stateMachineName,
            HashSet<IState._stateTags> tags = null)
        {
            return BehaviourTreeUtil.GetOrCreateState(Enter, Exit, Tick, stateMachineName, btStateCache, tags);
        }

        protected SmWrapperState GetOrCreateState(Func<IRunnableStateMachine> root, string state_name)
        {
            return BehaviourTreeUtil.GetOrCreateState(root, state_name, btStateCache);
        }

        protected IRunnableStateMachine GetOrCreateStateMachine(Func<IState> root, string state_name)
        {
            return BehaviourTreeUtil.GetOrCreateStateMachine(root, state_name, smCache);
        }

        protected ThinkerWrapperState GetOrCreateState(string stateName, BaseThinker thinker, IEntity host,
            HashSet<IState._stateTags> tags = null)
        {
            return BehaviourTreeUtil.GetOrCreateState(stateName, thinker, host, btStateCache, tags);
        }
    }
}