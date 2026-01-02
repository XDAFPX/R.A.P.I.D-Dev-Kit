using System;
using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Thinkers;
using UnityEditor;

namespace DAFP.TOOLS.Common.Utill
{
    public static class BehaviourTreeUtil
    {
        public static SmWrapperState GetOrCreateState(
            Func<IRunnableStateMachine> root,
            string stateMachineName, List<IState> btStateCache,
            HashSet<IState._stateTags> tags = null)
        {
            // Try to find existing state
            var foundState = btStateCache.Find(s => s.StateName == stateMachineName);
            if (foundState != null)
            {
                if (foundState is SmWrapperState state)
                    return state;
                else
                    throw new Exception("YOU MESSED WITH THE STATE NAMES BRO!!!!");
            }

            // Create, cache, and return new state
            var newState = new SmWrapperState(stateMachineName, root.Invoke(), tags);
            btStateCache.Add(newState);
            return newState;
        }

        public static ModularState GetOrCreateState(
            Action Enter, Action Exit, Action Tick,
            string stateName, List<IState> btStateCache,
            HashSet<IState._stateTags> tags = null)
        {
            // Try to find existing state
            var foundState = btStateCache.Find(s => s.StateName == stateName);
            if (foundState != null)
            {
                if (foundState is ModularState state)
                    return state;
                else
                    throw new Exception("YOU MESSED WITH THE STATE NAMES BRO!!!!");
            }

            // Create, cache, and return new state
            var newState = new ModularState(stateName, Enter, Tick, Exit, tags);
            btStateCache.Add(newState);
            return newState;
        }

        public static IRunnableStateMachine GetOrCreateStateMachine(
            Func<IState> root,
            string stateMachineName, List<IRunnableStateMachine> stateMachinesCache
        )
        {
            // Try to find existing state
            var foundState = stateMachinesCache.Find(s =>
                s.Name == stateMachineName);
            if (foundState != null) return foundState;

            // Create, cache, and return new state
            var newState = new StateMachine<IState>(root.Invoke(), stateMachineName);
            stateMachinesCache.Add(newState);
            return newState;
        }

        public static BtWrapperState GetOrCreateState(
            Func<BtNodeBase> root,
            string stateMachineName, List<IState> btStateCache,
            HashSet<IState._stateTags> tags = null)
        {
            // Try to find existing state
            var foundState = btStateCache.Find(s => s.StateName == stateMachineName);
            if (foundState != null)
            {
                if (foundState is BtWrapperState state)
                    return state;
                else
                    throw new Exception("YOU MESSED WITH THE STATE NAMES BRO!!!!");
            }


            // Create, cache, and return new state
            var newState = new BtWrapperState(root.Invoke(), stateMachineName, tags);
            btStateCache.Add(newState);
            return newState;
        }

        public static ThinkerWrapperState GetOrCreateState(
            string stateName,
            BaseThinker thinker,
            IEntity host,
            List<IState> btStateCache,
            HashSet<IState._stateTags> tags = null)
        {
            // Try to find existing state
            var foundState = btStateCache.Find(s => s.StateName == stateName);
            if (foundState != null)
            {
                if (foundState is ThinkerWrapperState state)
                    return state;
                else
                    throw new Exception("YOU MESSED WITH THE STATE NAMES BRO!!!!");
            }

            // Create, cache, and return new state
            var newState = new ThinkerWrapperState(stateName, thinker, host, tags);
            btStateCache.Add(newState);
            return newState;
        }
    }
}