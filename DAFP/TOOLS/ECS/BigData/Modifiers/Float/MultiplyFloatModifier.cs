using Archon.SwissArmyLib.Utils.Editor;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    [System.Serializable]
    public class MultiplyFloatModifier : StatModifier<float>
    {
        [SerializeField] private SerializableInterface<IStat<float>> Multiplier;
        private IStat<float> multiplier;

        private IStat<ITickerBase> multiplierTick;

        public MultiplyFloatModifier() : base(null)
        {
            
        }
        public MultiplyFloatModifier(IStat<ITickerBase> multiplier, IEntity owner) : base(owner)
        {
            multiplierTick = multiplier;
        }

        public MultiplyFloatModifier(IStat<float> multiplier, IEntity owner) : base(owner)
        {
            this.multiplier = multiplier;
        }

        public override float Apply(float value)
        {
            if (Multiplier.TryGetValue(out var _stat))
                return value * _stat.Value;
            return value * (multiplier?.Value ?? 1) * (multiplierTick?.Value?.DeltaTime ?? 1);
        }

        public override int Priority => 10;

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}