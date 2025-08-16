using System;
using Archon.SwissArmyLib.ResourceSystem;
using Bdeshi.Helpers.Utility;
using PixelRouge.CsharpHelperMethods;
using PixelRouge.Inspector.Utilities;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class ResourceBoard<T> : WhiteBoard<T> , IResourcePool<T> where T : IComparable<T>
    {
        private float _TimeInSecondsSinceEmpty;
        protected override void OnTick()
        {
            if (IsEmpty)
                _TimeInSecondsSinceEmpty += Host.EntityTicker.DeltaTime;
        }

        protected override void OnStart()
        {
        }

        protected override void OnInitializeInternal()
        {
        }

        protected override T GetValue(T ProcessedValue)
        {
            return ProcessedValue;
        }


        protected override T ClampAndProcessValue(T value)
        {
            
            return  CsharpHelper.Clamp(value, MinValue, MaxValue);

        }

        public override T DefaultValue { get; set; }
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
            
            InternalValue = DefaultValue;
            
        }


        public abstract float Percentage { get; }
        public abstract string GetFormatedMaxValue();
        public abstract string GetFormatedCurValue();
        public bool IsEmpty => Value.Equals(MinValue);
        public bool IsFull => Value.Equals(MaxValue);
        public float TimeSinceEmpty => _TimeInSecondsSinceEmpty;

        public abstract T Add(T amount, bool forced = false);

        public abstract T Remove(T amount, bool forced = false);

        public T Empty(bool forced = false)
        {
            SetToMin();
            return Value;
        }

        public T Fill(bool forced = false)
        {
            SetToMax();
            return Value;
        }

        public T Fill(T toValue, bool forced = false)
        {
            Value = toValue;
            return Value;
        }
    }
}