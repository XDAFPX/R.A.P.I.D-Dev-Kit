using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IPathFindable : ITargetContainable
    {
        IVector PathFindToTarget();
    }
}
