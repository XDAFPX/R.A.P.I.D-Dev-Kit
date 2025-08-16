using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class WhiteBoard<T> : EntityComponent, IStat<T>
    {
        public string Name { get; set; } = typeof(WhiteBoard<T>).FullName;
        public abstract bool SyncToBlackBoard { get; }

#if UNITY_EDITOR
        [ReadOnly] [SerializeField] private T RealValue;
#endif
        [ReadOnly] [SerializeField] protected T InternalValue;

#if UNITY_EDITOR
        [ReadOnly] [SerializeField] private List<string> _Modifiers = new List<string>();

        private void Update()
        {
            RealValue = Value;
            _Modifiers.Clear();
            foreach (var mod in StatModifiers)
                _Modifiers.Add(mod.GetType().Name + $"  From : {mod.GetCurrentOwner()}");
        }
#endif

        public T Value
        {
            get => InternalGetValue();
            set => SetValue(value);
        }

        protected sealed override void OnInitialize()
        {
            ResetToDefault();
            OnInitializeInternal();
        }

        protected abstract void OnInitializeInternal();

        // Simplified: just apply this board’s modifiers, return through GetValue,
        // but do NOT write back into InternalValue or re-clamp here.
        private T InternalGetValue()
        {
            T temp = InternalValue;
            StatModifiers.Sort();
            foreach (var modifier in StatModifiers)
                temp = modifier.Apply(temp);

            return GetValue(ClampAndProcessValue(temp));
        }

        protected abstract T GetValue(T processedValue);

        private void SetValue(T value)
        {
            InternalValue = ClampAndProcessValue(value);
        }

        protected abstract T ClampAndProcessValue(T value);

        public abstract T MaxValue { get; set; }
        public abstract T MinValue { get; set; }
        public abstract T DefaultValue { get; set; }
        public abstract void SetToMax();
        public abstract void SetToMin();

        protected readonly List<StatModifier<T>> StatModifiers = new();

        public void AddModifier(StatModifier<T> modifier)
        {
            if (StatModifiers.Contains(modifier))
                return;
            StatModifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            if (!StatModifiers.Contains(modifier))
                return;
            StatModifiers.Remove(modifier);
        }

        protected abstract void ResetInternal();

        public void ResetToDefault()
        {
            StatModifiers.Clear();
            ResetInternal();
        }

        public abstract void Randomize(float margin01);
    }
}