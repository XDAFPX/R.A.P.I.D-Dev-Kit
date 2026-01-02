using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.Injection;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public class WireCircleDrawer : GenericDebugDrawer<IEntity>
    {
        private readonly Transform draw_target;
        private readonly float radius;
        private readonly Color color;

        public WireCircleDrawer(Transform draw_target,float radius, Color color)
        {
            this.draw_target = draw_target;
            this.radius = radius;
            this.color = color;
        }
        protected override string GetLayerName()
        {
            return DebugDrawLayer.DefaultDebugLayers.POSITIONS;
        }

        public override void DrawInternal()
        {
            Sys.Gizmos.DrawWireCircle2D(draw_target.position,radius,color);
        }
    }
}