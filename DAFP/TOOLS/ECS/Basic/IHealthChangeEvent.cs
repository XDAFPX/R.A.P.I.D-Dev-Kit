using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IHealthChangeEvent : IEntityEvent
    {
        public IHealthChange Change { get; }
    }
}