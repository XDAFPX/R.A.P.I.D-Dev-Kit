using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using UGizmo;
using UGizmo.Internal;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class EntityDebugDrawer : IDebugDrawer
    {
        public List<IDebugDrawable> Children { get; } = new();
        public List<IDebugDrawable> Owners { get; } = new();

        public DebugDrawLayer Layer { get; set; }

        protected IDebugSys<IGlobalGizmos, IConsoleMessenger> Sys;
        protected IEntity Host => ((IOwnedBy<IDebugDrawable>)this).GetCurrentOwner() as IEntity;
        protected abstract string GetLayerName();

        public void InitilizeDebugDrawer(IDebugSys<IGlobalGizmos, IConsoleMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(GetLayerName()) as DebugDrawLayer ?? Sys.GetSharedLayer;
            OnInit();
        }

        protected virtual void OnInit()
        {
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
                if (Host.CachedBounds.size != Vector3.zero)
                    Sys.Gizmos.DrawWireBox2D(Host.CachedBounds.center, 0, Host.CachedBounds.size, color);
            }
        }

        public class HealthDrawer : EntityDebugDrawer, IListener<ICommonEntityEvent.EntityTakeDamageEvent>
        {
            private Color color;
            private readonly Color secoundColor;
            private readonly float height;
            private readonly float width;

            public HealthDrawer(Color color = default, Color secoundColor = default, float height = 0.37f, float width = 2.5f)
            {
                if (secoundColor == default)
                    secoundColor = Color.softRed;
                if (color == default)
                    color = Color.lightSeaGreen;
                this.color = color;
                this.secoundColor = secoundColor;
                this.height = height;
                this.width = width;
            }

            private float health01 = 1;

            protected override string GetLayerName()
            {
                return DebugDrawLayer.DefaultDebugLayers.ENT_INFO;
            }

            protected override void OnInit()
            {
                if (extract_and_update_health(Host)) return;
                Host.Bus.Subscribe(this);
            }


            public override void DrawInternal()
            {
                if (!Host.Stats.Has("Health"))
                    return;
                draw_bar();
            }

            private void draw_bar()
            {
                bool is2D = Host.CachedBounds.size.z == 0f;
                var offset = 0.4f;
                Vector3 barPosition = is2D
                    ? new Vector3(Host.CachedBounds.min.x, Host.CachedBounds.max.y + offset, 0f)
                    : new Vector3(Host.CachedBounds.min.x, Host.CachedBounds.max.y + offset, Host.CachedBounds.center.z);

                Sys.Gizmos.DrawBox2D(barPosition, 0, new Vector2(width, height), secoundColor);
                Sys.Gizmos.DrawBox2D(barPosition, 0, new Vector2(width * health01, height), color);
            }


            public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem => Sys;

            public void React(in ICommonEntityEvent.EntityTakeDamageEvent e)
            {
                if (e.Host != Host)
                    return;
                extract_and_update_health(Host);
            }


            private bool extract_and_update_health(IEntity entity)
            {
                var _health = entity.Stats.Get<uint>("Health", () => null);
                if (_health == null) return true;
                OnHostTakeDamage((float)_health.Value / (float)_health.MaxValue);
                return false;
            }

            public void OnHostTakeDamage(float updatedHealth)
            {
                health01 = Mathf.Clamp01(updatedHealth);
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

        public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem => Sys;
    }
}