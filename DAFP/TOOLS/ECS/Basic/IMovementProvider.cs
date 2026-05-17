using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IMovementProvider
    {
        IMovingStat MovingState { get; }
    }

    public interface IMovingStat : IStatBase
    {
        bool IsMoving { get; }
        bool IsStanding => !IsMoving;
    }
}
