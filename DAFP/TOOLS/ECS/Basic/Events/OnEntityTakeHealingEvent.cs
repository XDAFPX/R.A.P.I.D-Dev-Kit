using JetBrains.Annotations;
using Optional;
using RapidLib.DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityTakeHealingEvent
    {
        public OnEntityTakeHealingEvent([NotNull] IEntity receiver, [NotNull] IHealing healing)
        {
            Receiver = receiver;
            Healing = healing;
        }

        public IEntity Receiver { get; }
        public Option<IEntity> Source => Healing.Info.Source.Author;
        public IHealing Healing;
    }
}
