using DAFP.TOOLS.ECS.Environment.Filters;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public interface ITriggerFilter : IFilter<TriggerContext>,IFilter<GameObject>
    {
        public TriggerEntity.TriggerEvent Event { get; set; }

        bool IFilter<TriggerContext>.Evaluate(TriggerContext go)
        {
            var _val = Evaluate(go.Target.GameObject);
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