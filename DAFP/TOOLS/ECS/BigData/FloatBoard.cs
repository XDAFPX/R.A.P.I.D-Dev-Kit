using System;
using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class FloatBoard : WhiteBoard<float>
    {
        public override void Randomize(float margin01)
        {
            Value = Value.Randomize(margin01);
        }

        protected override void ResetInternal()
        {
            InternalValue = DefaultValue;
        }

        public override bool SyncToBlackBoard => true;

        protected override float GetValue()
        {
            return InternalValue;
        }

        protected override void SetValue(float value)
        {
            InternalValue = Math.Clamp(value, MinValue, MaxValue);
        }

        [field: SerializeField] public override float MaxValue { get; set; }
        [field: SerializeField] public override float MinValue { get; set; }
        [field: SerializeField] public override float DefaultValue { get; set; }

        public override void SetToMax()
        {
            SetValue(MaxValue);
        }

        public override void SetToMin()
        {
            SetValue(MinValue);
        }
    }
}