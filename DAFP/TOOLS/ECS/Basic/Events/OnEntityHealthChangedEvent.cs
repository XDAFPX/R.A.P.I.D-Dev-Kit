using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityHealthChangedEvent : IEntityEvent, IHealthChangeEvent
    {
        public OnEntityHealthChangedEvent(IEntity entity, IHealthChange change)
        {
            Entity = entity;
            Change = change;
        }

        public IEntity Entity { get; }
        public IHealthChange Change { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has changed its health");
        }
    }
}