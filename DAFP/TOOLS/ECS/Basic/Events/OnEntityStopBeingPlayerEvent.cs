using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityStopBeingPlayerEvent : IEntityEvent
    {
        public PlayerData Data;

        public IEntity Entity { get; }

        public OnEntityStopBeingPlayerEvent(IEntity ent, PlayerData data)
        {
            Entity = ent;
            Data = data;
        }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) has stopped being a player");
        }
    }
}