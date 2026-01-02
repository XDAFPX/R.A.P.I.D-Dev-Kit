using DAFP.TOOLS.ECS.Thinkers;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class ThinkerDebugDrawer : GenericDebugDrawer<BaseThinker>
    {
        protected override string GetLayerName()
        {
            return "Thinkers";
        }

        /*public class ThinkerNameDrawer  : ThinkerDebugDrawer
        {
            public override void DrawInternal()
            {
                Sys.Gizmos.Text
            }
        }*/
    }
}