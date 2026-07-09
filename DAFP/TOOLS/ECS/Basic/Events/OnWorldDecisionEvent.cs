using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using UnityEngine.LightTransport;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnWorldDecisionEvent : IWorldEvent
    {
        public OnWorldDecisionEvent(World world)
        {
            World = world;
        }

        public World World { get; }
        public override string ToString()
        {
            return GameUtils.FormatLog(World, $"A World decision was made");
        }
    }
}