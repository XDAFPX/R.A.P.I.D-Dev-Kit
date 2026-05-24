using System;
using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatBase : IResetable, INameable, IPetOwnerTreeOf<IStatBase>, IPetOf<IHaveStats, IStatBase>
    {
        public bool SyncToBlackBoard { get; }
        public object GetAbsoluteValue();

        public object GetAbsoluteMax();
        public object GetAbsoluteMin();
        public object GetAbsoluteDefault();

        public void SetAbsoluteValue(object value);

        public void SetAbsoluteMax(object value);
        public void SetAbsoluteMin(object value);
        public void SetAbsoluteDefault(object value);

        void RemoveModifier(string name);
        bool HasModifier(string name);

        public void SetToMax();
        public void SetToMin();
        public void ForceUpdate();
        public delegate void UpdateValueCallBack(IStatBase stat, object pvalue);


        public event UpdateValueCallBack OnValueUpdateGeneric;
    }
}