using DAFP.TOOLS.ECS.Environment.Filters;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ITriggerFilter : IFilter<TriggerCollider>,IFilter<GameObject>
    {
        public TriggerEntity.TriggerEvent Event { get; set; }

        bool IFilter<TriggerCollider>.Evaluate(TriggerCollider go)
        {
            var _val = Evaluate(go.GameObject);
            LastStatus = _val;
            return _val;

        }

        public ITriggerFilter SetEvent(TriggerEntity.TriggerEvent triggerEvent)
        {
            Event = triggerEvent;
            return this;
        }

        public bool? LastStatus { get; set; }
    }
}