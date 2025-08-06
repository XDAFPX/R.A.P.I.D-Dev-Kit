using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class WhiteBoard<T> : EntityComponent, IStat<T>
    {
        public string Name { get; set; } = typeof(WhiteBoard<T>).FullName;
        public abstract bool SyncToBlackBoard { get; }
        [field: SerializeField] protected T InternalValue;

        public T Value
        {
            get => InternalGetValue();
            set => SetValue(value);
        }

        private T InternalGetValue()
        {
            T temp = Value;
            _modifiers.Sort();
            foreach (var _modifier in _modifiers)
            {
                _modifier.Apply(temp);
            }

            SetValue(temp);
            return GetValue();

        }
        

        protected abstract T GetValue();
        protected abstract void SetValue(T value);
        public abstract T MaxValue { get; set; }
        public abstract T MinValue { get; set; }
        public abstract T DefaultValue { get; set; }
        public abstract void SetToMax();
        public abstract void SetToMin();
        private readonly List<StatModifier<T>> _modifiers = new();

        public void AddModifier(StatModifier<T> modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            _modifiers.Add(modifier);
        }

        protected abstract void ResetInternal();

        public void ResetToDefault()
        {
            _modifiers.Clear();
            ResetInternal();
        }

        public abstract void Randomize(float margin01);
    }
}