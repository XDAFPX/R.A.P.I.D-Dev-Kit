using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [System.Serializable]
    public class LayerFilter :   ITriggerFilter,IFilter<IEntity>
    {
        [Tooltip("GameObject must be on one of these layers to pass. Empty mask means no restriction.")]
        public LayerMask layers;

        public bool Evaluate(GameObject go)
        {
            if (go == null) return false;
            if (layers.value == 0) return true; // No restriction
            int layer = go.layer;
            return (layers.value & (1 << layer)) != 0;
        }

        public TriggerEntity.TriggerEvent Event { get; set; }
        public bool? LastStatus { get; set; }
        public bool Evaluate(IEntity go)
        {
            return Evaluate(go.GetWorldRepresentation());
        }
    }
}
