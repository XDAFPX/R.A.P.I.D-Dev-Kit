using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using TNRD;
using UnityEngine;
using Zenject.Internal;

namespace DAFP.TOOLS.ECS.BigData
{
    [System.Serializable]
    public class FloatStat : QuikStat<float>
    {
    }

    [System.Serializable]
    public class IntStat : QuikStat<float>
    {
    }

    [System.Serializable]
    public class QuikStat<T> : IStat<T>
    {
        [SerializeField] private T InspectorVal;
        private readonly Func<T> valFunc;
        private T value;

        protected readonly List<StatModifier<T>> StatModifiers = new();


        public QuikStat()
        {
        }

        public QuikStat(Func<T> valFunc)
        {
            this.valFunc = valFunc;
            Value = valFunc.Invoke();
        }

        public QuikStat(T value)
        {
            Value = value;
        }

        public string Name { get; set; }

        public void ResetToDefault()
        {
        }

        public bool SyncToBlackBoard => false;

        public object GetAbsoluteValue()
        {
            if (valFunc != null)
                return valFunc.Invoke();
            return value;
        }

        public event IStatBase.UpdateValueCallBack OnUpdateValue;

        public void Randomize(NRandom.IRandom rng, float margin01)
        {
        }

        public T Value
        {
            get
            {
                if (!EqualityComparer<T>.Default.Equals(InspectorVal, default))
                    return apply_stat_modifiers(InspectorVal);
                if (valFunc == null)
                    return apply_stat_modifiers(value);
                else
                {
                    return apply_stat_modifiers(valFunc.Invoke());
                }
            }
            set
            {
                this.value = value;
                OnUpdateValue?.Invoke(this);
            }
        }

        private T apply_stat_modifiers(T _value)
        {
            var temp = _value;
            StatModifiers.Sort();
            foreach (var modifier in StatModifiers)
                temp = modifier.Apply(temp);

            return temp;
        }

        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }

        public void SetToMax()
        {
        }

        public void SetToMin()
        {
        }

        public void AddModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;
            if (StatModifiers.Contains(modifier))
                return;
            StatModifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;
            if (!StatModifiers.Contains(modifier))
                return;
            StatModifiers.Remove(modifier);
        }

        public static implicit operator QuikStat<T>(T stat)
        {
            return new QuikStat<T>(stat);
        }

        public static implicit operator T(QuikStat<T> stat)
        {
            return stat.Value;
        }

        public List<IStatBase> Owners { get; } = new List<IStatBase>();
        public ISet<IOwnable<IStatBase>> Pets { get; } = new HashSet<IOwnable<IStatBase>>();
    }
}