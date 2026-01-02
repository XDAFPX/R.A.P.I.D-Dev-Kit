using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStat<T> : INameable, IStatBase, IRandomizeable 

    {
        public T Value { get; set; }
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }
        public void SetToMax();
        public void SetToMin();

        void IStatBase.SetAbsoluteValue(object value)
        {
            try
            {
                Value = (T)value;
            }
            catch
            {
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        void IStatBase.SetAbsoluteMax(object value)
        {
            try
            {
                MaxValue = (T)value;
            }
            catch
            {
                MaxValue = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        void IStatBase.SetAbsoluteMin(object value)
        {
            try
            {
                MinValue = (T)value;
            }
            catch
            {
                MinValue = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        void IStatBase.SetAbsoluteDefault(object value)
        {
            try
            {
                DefaultValue = (T)value;
            }
            catch
            {
                DefaultValue = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        object IStatBase.GetAbsoluteMax()
        {
            return MaxValue;
        }

        object IStatBase.GetAbsoluteMin()
        {
            return MinValue;
        }

        object IStatBase.GetAbsoluteDefault()
        {
            return DefaultValue;
        }

        void AddModifier(StatModifier<T> modifier);
        void RemoveModifier(StatModifier<T> modifier);

    }
}