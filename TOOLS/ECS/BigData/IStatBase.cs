using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatBase : IResetable
    {
        public bool SyncToBlackBoard { get; }
    }
}