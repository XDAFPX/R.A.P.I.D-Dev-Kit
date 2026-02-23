using DAFP.TOOLS.Common.Utill;
using NRandom;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    [System.Serializable]
    public class GenericWhiteBoard<T> : WhiteBoard<T>

    {
        // public GenericWhiteBoard(T val)
        // {
        //     Value = val;
        // }
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
            var _min = MinValue;
            var _max = MaxValue;
            Utils.NormalizeMinMax(ref _min,ref _max);
            MinValue = _min;
            MaxValue = _max;
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