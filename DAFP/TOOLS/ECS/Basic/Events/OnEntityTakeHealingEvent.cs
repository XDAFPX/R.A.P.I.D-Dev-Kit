using DAFP.TOOLS.Common.Utill;
using JetBrains.Annotations;
using Optional;
using RapidLib.DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityTakeHealingEvent : IEntityEvent, IHealthChangeEvent
    {
        public IHealing Healing;

        public OnEntityTakeHealingEvent(IEntity entity, IHealing healing)
        {
            Entity = entity;
            Healing = healing;
        }

        public IEntity Entity { get; }
        public IHealthChange Change => Healing;

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has received healing ({Healing.Info.Amount.Value}) by ({Healing.Info.Source}) . ");
        }
    }
}