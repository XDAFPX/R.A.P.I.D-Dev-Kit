using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnWorldInitializeEvent : IWorldEvent
    {
        public OnWorldInitializeEvent(World world)
        {
            this.World = world;
        }

        public World World { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(World, $"({World}) was initialized");
        }
    }
}