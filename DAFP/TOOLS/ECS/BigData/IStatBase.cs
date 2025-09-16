using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatBase : IResetable, INameable
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

        public delegate void UpdateValueCallBack(IStatBase stat);

        public event UpdateValueCallBack OnUpdateValue;
    }
}