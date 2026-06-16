using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnWorldLoadEvent //--All stuff that's supposed to happen before the full initialization of the world 
    {
        public World world;

        public OnWorldLoadEvent(World world)
        {
            this.world = world;
        }
    }
}