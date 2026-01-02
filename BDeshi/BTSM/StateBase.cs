using System;
using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public abstract class StateBase : IState
    {
        public abstract void EnterState();
        public abstract void Tick();
        public abstract void ExitState();
        public string Prefix { get; set; }
        public virtual string FullStateName => Prefix + "_" + GetParentChainName();
        public abstract string StateName { get; }
        public HashSet<IState._stateTags> StateTags => StateTagsInternal;
        protected HashSet<IState._stateTags> StateTagsInternal;
        public IState Parent { get; set; }
        public abstract BtStatus LastStatus { get; }  

        public string Name => this.GetType().Name;

        public IState AsChildOf(IState p)
        {
            Parent = p;
            return this;
        }

        public static string GetParentChainName(IState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var chain = state.Name;
            var current = state.Parent;
            while (current != null)
            {
                chain = current.Name + "." + chain;
                current = current.Parent;
            }

            return chain;
        }

        public string GetParentChainName()
        {
            return GetParentChainName(this);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}