using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using PixelRouge.Colors;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public class CollidableFilterActionEntity<T> : FilterActionEntity<T>
    {
        protected Collider2D[] Cols2d;
        protected Collider[] Cols3d;
        protected virtual Color DebugColor => ColorsForUnity.Orange;

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            var _bnds = Cols2d.GetCombinedBounds().Add(Cols3d.GetCombinedBounds());
            return base.SetupDebugDrawers().Union(new[]
            {
                new ActionDebugDrawer(DebugDrawLayer.DefaultDebugLayers.TRIGGERS, (
                    gizmos =>
                    {
                        gizmos.DrawCube(_bnds.center, Quaternion.identity, _bnds.size,
                            DebugColor);
                    }))
            });
        }

        protected override Bounds CalculateBounds()
        {
            return get_combined_bounds();
        }

        private Bounds get_combined_bounds()
        {
            if (Cols2d == null || Cols3d==null)
                return default;

            Bounds _combined = default;
            bool _initialized = false;

            foreach (var _col in Cols2d)
            {
                var _b = _col.bounds;
                _b.size = new Vector3(_b.size.x, _b.size.y, 0f);
                if (!_initialized)
                {
                    _combined = _b;
                    _initialized = true;
                }
                else _combined.Encapsulate(_b);
            }

            foreach (var _col in Cols3d)
            {
                if (!_initialized)
                {
                    _combined = _col.bounds;
                    _initialized = true;
                }
                else _combined.Encapsulate(_col.bounds);
            }

            return _combined;
        }

        protected override void InitializeInternal()
        {
            Cols2d = GetComponentsInChildren<Collider2D>();
            Cols3d = GetComponentsInChildren<Collider>();

            // Make sure they don't interfere with physics
            foreach (var c in Cols2d) c.isTrigger = true;
            foreach (var c in Cols3d) c.isTrigger = true;
        }
    }
}