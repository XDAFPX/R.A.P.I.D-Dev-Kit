using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class GravityBoard3D : WhiteBoard<float>
    {
        private GravityScale3D gs3D;

        public override bool SyncToBlackBoard => false;
        [field: SerializeField] public override float MaxValue { get; set; }
        [field: SerializeField] public override float MinValue { get; set; }
        public override float DefaultValue { get; set; }

        protected override void OnInitializeInternal()
        {
            gs3D = Host.GetWorldRepresentation().GetComponent<GravityScale3D>();
            if(!gs3D)
                return;
            DefaultValue = gs3D.gravityScale;
            Value = gs3D.gravityScale;
        }


        public override void Tick()
        {
            if(!gs3D)
                return;
            gs3D.gravityScale = Value;
        }

        protected override float GetValue(float processedValue)
        {
            return processedValue;
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