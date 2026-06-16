using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnWorldInitEvent //--stuff that happens right after the entities init
    {
        public World world;

        public OnWorldInitEvent(World world)
        {
            this.world = world;
        }
    }
}