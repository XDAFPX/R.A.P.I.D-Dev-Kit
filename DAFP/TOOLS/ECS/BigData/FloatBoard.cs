using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class FloatBoard : WhiteBoard<float>
    {
        public override void Randomize(float margin01)
        {
            DefaultValue = DefaultValue.Randomize(margin01);
            Value = DefaultValue;
        }

        protected override void ResetInternal()
        {
            InternalValue = DefaultValue;
        }

        public override bool SyncToBlackBoard => true;

        protected override float GetValue(float ProcessedValue)
        {
            return ProcessedValue;
        }

        protected override float ClampAndProcessValue(float value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }


        public override void SetToMax()
        {
            Value = MaxValue;
        }

        public override void SetToMin()
        {
            Value = MaxValue;
        }
    }
}