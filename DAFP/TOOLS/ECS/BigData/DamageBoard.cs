using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData.GlobalModifiers;
using PixelRouge.Inspector;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    [DamageModdable]
    public abstract class DamageBoard : WhiteBoard<Damage>
    {
        protected override Damage GetValue(Damage processedValue)
        {
            return processedValue;
        }

        protected override Damage ClampAndProcessValue(Damage value)
        {
            return Damage.Clamp(value, MinValue, MaxValue);
        }

        public override Damage MaxValue
        {
            get => new Damage(99999, DamageType.Debug, null, Vector3.zero);
            set {}
        }

        public override Damage MinValue
        {
            get => new Damage(0, DamageType.Debug, null, Vector3.zero);
            set {}
        }

        public override void SetToMax()
        {
            Value = MaxValue;
        }

        public override void SetToMin()
        {
            Value = MinValue;
        }

        protected override void ResetInternal()
        {
            Value = DefaultValue;
        }

        public override void Randomize(float margin01)
        {
            Value = Value.Randomize(margin01);
        }
    }
}