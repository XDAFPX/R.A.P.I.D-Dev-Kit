using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDeshi.BTSM
{
    /// <summary>
    /// C# default interface issues so this is a copy of statebase with monobehaviour inheritance
    /// </summary>
    public abstract class MonoBehaviourStateBase: MonoBehaviour,IState
    {
        public abstract void EnterState();
        public abstract void Tick();
        public abstract void ExitState();
        public string Prefix { get; set; }
        public string FullStateName => Prefix +"_"+ GetParentChainName();
        public HashSet<IState._stateTags> StateTags { get; }
        public IState Parent { get; set; }
               
        public string Name => GetType().Name;

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