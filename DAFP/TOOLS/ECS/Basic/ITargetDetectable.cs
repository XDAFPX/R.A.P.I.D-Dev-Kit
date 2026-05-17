using DAFP.TOOLS.ECS.Environment.Filters;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface ITargetDetectable : ITargetContainable
    {
        ITargetOf<IEntity> ScanForTarget(IFilter<IEntity> filter);
    }
}
