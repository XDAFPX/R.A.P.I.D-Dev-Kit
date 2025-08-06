using System;
using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public abstract class StateBase: IState
    {
        public abstract void EnterState();
        public abstract void Tick();
        public abstract void ExitState();
        public string Prefix { get; set; }
        public virtual string FullStateName => Prefix +"_"+ GetParentChainName();
        public HashSet<IState._stateTags> StateTags => StateTagsInternal;
        protected HashSet<IState._stateTags> StateTagsInternal;
        public IState Parent { get; set; }
               
        public string Name => this.GetType().Name;

        public IState AsChildOf(IState p)
        {
            Parent = p;
            return this;
        }

        public String GetParentChainName()
        {
            string _chain = Name;
            var _p = Parent;
            while (_p != null)
            {
                _chain = _p.Name + "." + _chain;
                _p = _p.Parent;
            }
            return _chain;
        }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}