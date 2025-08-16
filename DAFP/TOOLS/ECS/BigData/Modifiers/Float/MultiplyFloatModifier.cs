using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    [System.Serializable]
    public class MultiplyFloatModifier : StatModifier<float>
    {
        private IStat<float> Multiplier;

        private IStat<ITickerBase> MultiplierTick;

        public MultiplyFloatModifier(IStat<ITickerBase> multiplier, IEntity owner) : base(owner)
        {
            MultiplierTick = multiplier;
        }

        public MultiplyFloatModifier(IStat<float> multiplier, IEntity owner) : base(owner)
        {
            Multiplier = multiplier;
        }

        public override float Apply(float value)
        {
            return value * (Multiplier?.Value ?? 1) *(MultiplierTick?.Value?.DeltaTime ?? 1);
        }

        public override int Priority => 10;

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}