using System;
using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(FsmRunner))]
    public abstract class StateDrivenEntity : Entity
    {
        protected FsmRunner Fsm;

        protected StateMachine<IState> StateMachine;

        protected sealed override void TickInternal()
        {
            Fsm.ManualTick();
        }


        protected sealed override void InitializeInternal()
        {
            Fsm = GetComponent<FsmRunner>();

            Fsm.InitializeData(Memory);
            SetInitialData();
            var _sm = new StateMachine<IState>(GetInitialState(), "RootObj");
            StateMachine = _sm;

            RegisterInitialStates(ref _sm);
            Fsm.Initialize(_sm);
            Fsm.ShouldTickAutomatically = false;
            StateDrivenEntityInitialize();
        }

        protected abstract void SetInitialData();


        protected abstract void RegisterInitialStates(ref StateMachine<IState> sm);
        protected abstract IState GetInitialState();
        protected abstract void StateDrivenEntityInitialize();
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
    }
}