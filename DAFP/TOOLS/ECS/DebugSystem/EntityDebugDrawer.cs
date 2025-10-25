using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using UGizmo;
using UGizmo.Internal;
using UnityEditor.Graphs;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class EntityDebugDrawer : IDebugDrawer
    {
        public List<IDebugDrawable> Owners { get; } = new List<IDebugDrawable>();

        public DebugDrawLayer Layer { get; set; }

        protected IDebugSys<IGlobalGizmos, IMessenger> Sys;
        protected IEntity Host => ((IOwnable<IDebugDrawable>)this).GetCurrentOwner() as IEntity;
        protected abstract string GetLayerName();
        public void Initilize(IDebugSys<IGlobalGizmos, IMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(GetLayerName()) as DebugDrawLayer ?? Sys.GetSharedLayer;
        }

        public abstract void DrawInternal();


        public class BoundingBoxDrawer : EntityDebugDrawer
        {
            private Color color;

            public BoundingBoxDrawer(Color color = default)
            {
                if (color == default)
                    color = Color.green;
                this.color = color;
            }

            protected override string GetLayerName()
            {
                return "BoundingBoxes";
            }

            public override void DrawInternal()
            {
                if (Host.Bounds.size != Vector3.zero)
                    Sys.Gizmos.DrawWireBox2D(Host.Bounds.center, 0, Host.Bounds.size, color);
            }
        }

        public class PositionDrawer : EntityDebugDrawer
        {
            private Color color;

            public PositionDrawer(Color color = default)
            {
                if (color == default)
                    color = Color.blue;
                this.color = color;
            }

            protected override string GetLayerName()
            {
                return "Positions";
            }

            public override void DrawInternal()
            {
                Sys.Gizmos.DrawPoint(Host.GetWorldRepresentation().transform.position, 0.14f, color);
            }
        }
    }
}