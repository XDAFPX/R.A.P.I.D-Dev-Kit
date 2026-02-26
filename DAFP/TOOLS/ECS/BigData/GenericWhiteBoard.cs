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
            if (underflow_fix(value, out var _value)) return _value;


            // InternalValue is the original
            var _min = MinValue;
            var _max = MaxValue;
            Utils.NormalizeMinMax(ref _min, ref _max);
            MinValue = _min;
            MaxValue = _max;


            return Utils.Clamp(value, MinValue, MaxValue);
        }

        private bool underflow_fix(T value, out T clampAndProcessValue)
        {
            if (value is uint _newUint && InternalValue is uint _oldUint)
            {
                bool _greaterThanBefore = _newUint > _oldUint;
                bool _suspiciouslyHuge = _newUint > (uint.MaxValue / 2);
                if (_greaterThanBefore && _suspiciouslyHuge)
                {
                    clampAndProcessValue = MinValue;
                    return true;
                }
            }

            clampAndProcessValue = default;
            return false;
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