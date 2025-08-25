using System;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GravityBoard2D : WhiteBoard<float>
    {
        protected override void OnTick()
        {
            rb.gravityScale = Value;
        }

        protected override void OnStart()
        {
        }

        public override bool SyncToBlackBoard => false;
        private Rigidbody2D rb;

        protected override void OnInitializeInternal()
        {
            rb = GetComponent<Rigidbody2D>();
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

        public override void Randomize(float margin01)
        {
            Value = Value.Randomize(margin01);
        }
    }
}