using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    [System.Serializable]
    public class DivideFloatModifier : StatModifier<float>
    {
        [SerializeField]private SerializableInterface<IStat<float>> _devider;
        private IStat<float> divider;

        public DivideFloatModifier() : base(null)
        {
        }
        public DivideFloatModifier(IEntity owner, IStat<float> divider) : base(owner)
        {
            this.divider = divider;
        }

        public override float Apply(float value)
        {
            if (_devider.TryGetValue(out var _stat))
                return value / _stat.Value;
            return value / divider.Value;
        }

        public override int Priority => 10;

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}