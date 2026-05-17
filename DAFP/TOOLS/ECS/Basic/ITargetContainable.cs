using Optional;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface ITargetContainable : IEntity
    {
        ITargetOf<IEntity> Target { get; set; }
        void ResetTarget() => Target.Value = Option.None<IEntity>();
    }
}
