using System;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class GravityBoard2D : WhiteBoard<float>
    {
        public override void Tick()
        {
            if (!rb)
                return;
            rb.gravityScale = Value;
        }


        public override bool SyncToBlackBoard => false;
        private Rigidbody2D rb;

        protected override void OnInitializeInternal()
        {
            rb = Host.GetWorldRepresentation().GetComponent<Rigidbody2D>();
            if (!rb)
                return;
            DefaultValue = rb.gravityScale;
            Value = rb.gravityScale;
        }

        protected override float GetValue(float processedValue)
        {
            return processedValue;
        }

        protected override float ClampAndProcessValue(float value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }

        [field: SerializeField] public override float MaxValue { get; set; }
        [field: SerializeField] public override float MinValue { get; set; }
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

        public override void Randomize(NRandom.IRandom rng, float margin01)
        {
            Value = Value.Randomize(margin01);
        }
    }
}