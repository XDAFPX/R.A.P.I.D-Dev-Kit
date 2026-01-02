using DAFP.TOOLS.Common.Utill;
using NRandom;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public class GenericWhiteBoard<T> : WhiteBoard<T>

    {
        public override bool SyncToBlackBoard => true;
        [field: SerializeField] public override T MaxValue { get; set; }
        [field: SerializeField] public override T MinValue { get; set; }
        [field: SerializeField] public override T DefaultValue { get; set; }

        protected override void OnInitializeInternal()
        {
        }

        protected override T GetValue(T processedValue)
        {
            return processedValue;
        }

        protected override T ClampAndProcessValue(T value)
        {
            return Utils.Clamp(value, MinValue, MaxValue);
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
            
        }
    }
}