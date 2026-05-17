using JetBrains.Annotations;
using Optional;
using RapidLib.DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityTakeDamageEvent
    {
        public OnEntityTakeDamageEvent([NotNull] IEntity receiver, [NotNull] IDamage damage)
        {
            Receiver = receiver;
            Damage = damage;
        }

        public IEntity Receiver { get; }
        public Option<IEntity> Source => Damage.Info.Source.Author;
        public IDamage Damage { get; }
    }
}
