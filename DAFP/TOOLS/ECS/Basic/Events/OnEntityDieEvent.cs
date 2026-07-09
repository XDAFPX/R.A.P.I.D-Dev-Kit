using DAFP.TOOLS.Common.Utill;
using Optional;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityDieEvent : IEntityEvent
    {
        public OnEntityDieEvent(IEntity receiver, IDamage lethal)
        {
            Entity = receiver;
            Lethal = lethal;
        }

        public IEntity Entity { get; }
        public IDamage Lethal { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has killed by ({Lethal.Info.Source}) . R.I.P");
        }
    }
}