using System;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public class GlobalTimeScaleBoard : WhiteBoard<float>
    {
         private float maxValue = 10;

        protected override void OnTick()
        {
            Time.timeScale = Value;
        }

        protected override void OnStart()
        {
        }

        public override bool SyncToBlackBoard => false;

        protected override void OnInitializeInternal()
        {
            Value = Time.timeScale;
            DefaultValue = Time.timeScale;
        }

        protected override float GetValue(float processedValue)
        {
            return processedValue;
        }

        protected override float ClampAndProcessValue(float value)
        {
            return Mathf.Clamp(value, 0, MaxValue);
        }

        public override float MaxValue
        {
            get => maxValue;
            set => maxValue = Mathf.Max(0,value);
        }

        public override float MinValue
        {
            get => 0; set{} }
        public override float DefaultValue { get; set; }

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
            InternalValue = InternalValue.Randomize(margin01);
        }
    }
}