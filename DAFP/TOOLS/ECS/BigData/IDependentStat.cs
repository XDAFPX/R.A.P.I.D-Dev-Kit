using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData.Health;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// Interface for a stat whose MaxValue and MinValue are determined by other stats.
    /// </summary>
    public interface IDependentStat<T> : IStat<T>
    {
        IStatDependent<T> MaxSource { get; set; }
        IStatDependent<T> MinSource { get; set; }
    }
}