using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class ExcludeSelfFilter : IFilter<IEntity>, IFilter<GameObject>
    {
        private static (GameObject go, IEntity ent) ResolveSelf(IFilterContext ctx)
        {
            switch (ctx)
            {
                case EntityFilterContext ectx:
                    return (ectx.SelfGO, ectx.Self);
                case GameObjectFilterContext gctx:
                    return (gctx.Self, null);
                default:
                    return (null, null);
            }
        }

        
        public bool Evaluate(IEntity go, IFilterContext ctx)
        {
            
            
            var (selfGO, selfEnt) = ResolveSelf(ctx);
            if (selfGO == null && selfEnt == null)
            {
                Debug.LogError($"ExcludeSelfFilter used without self in context. go={go}");
                return false;
            }

            if (selfEnt != null)
                return go.GetWorldRepresentation() != selfEnt.GetWorldRepresentation();
            if (selfGO != null)
                return go.GetWorldRepresentation() != selfGO;
            return false;
        }

        public bool Evaluate(GameObject go, IFilterContext ctx)
        {
            var (selfGO, selfEnt) = ResolveSelf(ctx);
            if (selfGO == null && selfEnt == null)
            {
                Debug.LogWarning($"ExcludeSelfFilter used without self in context. go={go}");
                return false;
            }

            if (selfGO != null)
                return go != selfGO;
            if (selfEnt != null)
                return go != selfEnt.GetWorldRepresentation();
            return false;
        }
    }
}