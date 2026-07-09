using DAFP.TOOLS.Common.Utill;
using JetBrains.Annotations;
using Optional;
using RapidLib.DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityTakeDamageEvent : IHealthChangeEvent
    {
        public OnEntityTakeDamageEvent(IEntity entity, IDamage damage)
        {
            Entity = entity;
            Damage = damage;
        }

        public IDamage Damage { get; }
        public IEntity Entity { get; }

        IHealthChange IHealthChangeEvent.Change => Damage;

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has received damage ({Damage.Info.Amount.Value}) by ({Damage.Info.Source}) . ");
        }
    }
}