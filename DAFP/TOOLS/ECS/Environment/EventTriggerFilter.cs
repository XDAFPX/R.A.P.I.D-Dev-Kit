using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.Filters;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    [System.Serializable]
    public class EventTriggerFilter : ITriggerFilter
    {
        public TriggerEntity.TriggerEvent EventMask;


        public bool? LastStatus { set; get; } = null;

        public TriggerEntity.TriggerEvent Event { get; set; }


        public bool Evaluate(GameObject go)
        {
            var _val = (EventMask & Event) != 0;
            LastStatus = _val;
            return _val;
        }
    }
}