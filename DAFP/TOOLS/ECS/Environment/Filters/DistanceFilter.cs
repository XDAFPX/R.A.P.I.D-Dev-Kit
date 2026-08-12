using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class DistanceFilter3D : IFilter<IEntity>, IFilter<GameObject>
    {
        [SerializeField] private float Distance;

        public DistanceFilter3D(float distance)
        {
            Distance = distance;
        }

        public DistanceFilter3D()
        {
        }

        private static GameObject ResolveOwner(IFilterContext ctx)
        {
            switch (ctx)
            {
                case EntityFilterContext ectx:
                    return ectx.SelfGO;
                case GameObjectFilterContext gctx:
                    return gctx.Self;
                default:
                    return null;
            }
        }

        public bool Evaluate(IEntity go, IFilterContext ctx)
        {
            return Evaluate(go.GetWorldRepresentation(), ctx);
        }

        public bool Evaluate(GameObject go, IFilterContext ctx)
        {
            var owner = ResolveOwner(ctx);
            if (owner == null || go == null)
                return false;
            return Vector3.Distance(go.transform.position, owner.transform.position) < Distance;
        }
    }

    [Serializable]
    public class DistanceFilter2D : IFilter<IEntity>, IFilter<GameObject>
    {
        [SerializeField] private float Distance;

        public DistanceFilter2D(float distance)
        {
            Distance = distance;
        }

        public DistanceFilter2D()
        {
        }

        private static GameObject ResolveOwner(IFilterContext ctx)
        {
            switch (ctx)
            {
                case EntityFilterContext ectx:
                    return ectx.SelfGO;
                case GameObjectFilterContext gctx:
                    return gctx.Self;
                default:
                    return null;
            }
        }

        public bool Evaluate(IEntity go, IFilterContext ctx)
        {
            return Evaluate(go.GetWorldRepresentation(), ctx);
        }

        public bool Evaluate(GameObject go, IFilterContext ctx)
        {
            var owner = ResolveOwner(ctx);
            if (owner == null || go == null)
                return false;
            return Vector2.Distance(go.transform.position, owner.transform.position) < Distance;
        }
    }
}