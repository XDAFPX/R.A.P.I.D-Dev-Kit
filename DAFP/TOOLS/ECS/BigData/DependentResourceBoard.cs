using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.ResourceSystem;
using PixelRouge.CsharpHelperMethods;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// A ResourceBoard whose Min and Max values are dependent on other WhiteBoards.
    /// </summary>
    public abstract class DependentResourceBoard<T, TMax, TMin> : DependentWhiteBoard<T, TMax, TMin>, IResourcePool<T>
        where TMin : WhiteBoard<T>, IStatDependent<T>
        where TMax : WhiteBoard<T>, IStatDependent<T> 
        where T : IComparable<T>
        
    {
        private float _timeInSecondsSinceEmpty;

        protected override void OnTick()
        {
            
            if (IsEmpty )
                _timeInSecondsSinceEmpty += Host.EntityTicker.DeltaTime;
        }



        protected override T GetValue(T processedValue)
        {
            return processedValue;
        }


        protected override T ClampAndProcessValue(T value)
        {

            return CsharpHelper.Clamp(value, MinValue, MaxValue);

        }
        // DefaultValue is now abstract in base class, so do not redeclare here

        public override void SetToMax()
        {
            Value = MaxValue;
        }

        public override void SetToMin()
        {
            Value = MinValue;
        }


        public abstract float Percentage { get; }
        public abstract string GetFormatedMaxValue();
        public abstract string GetFormatedCurValue();

        public bool IsEmpty => Value.Equals(MinValue);
        public bool IsFull => Value.Equals(MaxValue);
        public float TimeSinceEmpty => _timeInSecondsSinceEmpty;

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