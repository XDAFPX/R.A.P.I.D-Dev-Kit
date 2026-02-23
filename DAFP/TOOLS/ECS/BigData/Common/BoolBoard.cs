using NRandom;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class BoolBoard : WhiteBoard<bool>
    {
        public override bool SyncToBlackBoard { get; }
        public override bool MaxValue
        {
            get => true; set{} }
        public override bool MinValue
        {
            get => false; set{} }
        [field : SerializeField]public override bool DefaultValue { get; set; }
        protected override void OnInitializeInternal()
        {
        }

        protected override bool GetValue(bool processedValue)
        {
            return processedValue;
        }

        protected override bool ClampAndProcessValue(bool value)
        {
            return value;
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

        public override void Randomize(IRandom rng, float margin01)
        {
            Value = rng.NextBool();
        }
    }
}