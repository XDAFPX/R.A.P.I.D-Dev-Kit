using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityBecomePlayerEvent : IEntityEvent
    {
        public PlayerData Data;

        public IEntity Entity { get; }

        public OnEntityBecomePlayerEvent(IEntity ent, PlayerData data)
        {
            Entity = ent;
            Data = data;
        }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has become a player");
        }
    }
}