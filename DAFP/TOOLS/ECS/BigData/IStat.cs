using System;
using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStat<T> : INameable, IStatBase, IRandomizeable

    {
        public T Value { get; set; }
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }

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
            value ??= default(T);
            try
            {
                MaxValue = (T)value;
            }
            catch
            {
                try
                {
                    MaxValue = (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception e)
                {
                    Debug.LogError("[IStat] :: Tried and failed to convert the desired value from SetAbsolute");
                }
            }
        }

        void IStatBase.SetAbsoluteMin(object value)
        {
            value ??= default(T);
            try
            {
                MinValue = (T)value;
            }
            catch
            {
                try
                {
                    MinValue = (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception e)
                {
                    Debug.LogError("[IStat] :: Tried and failed to convert the desired value from SetAbsolute");
                }
            }
        }

        void IStatBase.SetAbsoluteDefault(object value)
        {
            value ??= default(T);
            try
            {
                DefaultValue = (T)value;
            }
            catch
            {
                try
                {
                    DefaultValue = (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[IStat] :: Tried and failed to convert the desired value from SetAbsoluteDefault. from : {value.GetType()}, to : {typeof(T)}");
                }
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

        public delegate void OnUpdateValueConcreteCallback(IStat<T> stat, T pvalue);

        public event OnUpdateValueConcreteCallback OnUpdateValue;
    }
}