using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatBase : IResetable,INameable
    {
        public bool SyncToBlackBoard { get; }
        public object GetAbsoluteValue();

        public delegate void UpdateValueCallBack(IStatBase stat);
        public event UpdateValueCallBack OnUpdateValue;
    }
}