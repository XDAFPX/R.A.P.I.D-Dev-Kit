using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IVelocityProvider : IEntity
    {
        IVelocityStat Velocity { get; }
    }

    public interface IVelocityStat : IStatBase
    {
        IVector Value { get; }
    }
}
