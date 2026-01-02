using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityEngine.Serialization;
using NRandom;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    [Serializable]
    public abstract class WhiteBoard<T> : IStat<T>, IInitializable, ITickable, IEntityPet
    {
        [field: SerializeField] public string Name { get; set; }

        public abstract bool SyncToBlackBoard { get; }


        public abstract T MaxValue { get; set; }
        public abstract T MinValue { get; set; }
        public abstract T DefaultValue { get; set; }

        protected IEntity Host => ((IEntityPet)this).GetCurrentOwner();


#if UNITY_EDITOR
        [ReadOnly] [SerializeField] private T RealValue;
#endif
        [ReadOnly] [SerializeField] protected T InternalValue;


        [SerializeField] [ReadOnly(OnlyWhilePlaying = true)]
        protected SerializableInterface<IStatModifier<T>>[] Modifiers;

        public List<SerializableInterface<IStatBase>> Children;

        public delegate void ValueChangedCallBack(T newValue, T oldValue);

        public delegate void ModifierAddedCallBack(StatModifier<T> newModifier);

        public delegate void ModifierRemovedCallBack(StatModifier<T> oldModifier);

        public event ValueChangedCallBack OnValueChanged;
        public event ModifierAddedCallBack OnModifierAdded;
        public event ModifierRemovedCallBack OnModifierRemoved;

        public event IStatBase.UpdateValueCallBack OnUpdateValue;

        public T Value
        {
            get => internal_get_value();
            set => set_value(value);
        }

        public void Initialize()
        {
            StatModifiers = StatModifiers
                .Union(Modifiers.Select((@interface => @interface.Value)).OfType<StatModifier<T>>()).ToList();

            ResetToDefault();
            OnInitializeInternal();
        }

        protected abstract void OnInitializeInternal();

        // Simplified: just apply this board’s modifiers, return through GetValue,
        // but do NOT write back into InternalValue or re-clamp here.
        private T internal_get_value()
        {
            var _temp = InternalValue;
            StatModifiers.Sort();
            foreach (var _modifier in StatModifiers)
                _temp = _modifier.Apply(_temp);

            return GetValue(ClampAndProcessValue(_temp));
        }

        protected abstract T GetValue(T processedValue);

        private void set_value(T value)
        {
            OnValueChanged?.Invoke(value, InternalValue);
            InternalValue = ClampAndProcessValue(value);
            OnUpdateValue?.Invoke(this);
        }

        protected abstract T ClampAndProcessValue(T value);

        public abstract void SetToMax();
        public abstract void SetToMin();


        protected List<StatModifier<T>> StatModifiers = new();
        private List<IEntity> owners = new List<IEntity>();

        public void AddModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;
            if (StatModifiers.Contains(modifier))
                return;
            OnModifierAdded?.Invoke(modifier);
            StatModifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;
            if (!StatModifiers.Contains(modifier))
                return;
            OnModifierRemoved?.Invoke(modifier);
            StatModifiers.Remove(modifier);
        }

        protected abstract void ResetInternal();

        public void ResetToDefault()
        {
            RemoveAllModifiers();
            var _old = Value;
            ResetInternal();
            OnValueChanged?.Invoke(Value, _old);
            OnUpdateValue?.Invoke(this);
        }

        public virtual object GetAbsoluteValue()
        {
            return InternalValue;
        }

        public void RemoveAllModifiers()
        {
            StatModifiers.Clear();
        }

        public void RemoveAllModifiersFrom(IEntity owner)
        {
            StatModifiers.RemoveAll(modifier => modifier.GetCurrentOwner() == owner);
        }

        public override string ToString()
        {
            return Value.ToString();
        }


        public virtual void SetAbsoluteValue(object value)
        {
            try
            {
                InternalValue = (T)value;
            }
            catch
            {
                InternalValue = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public abstract void Randomize(NRandom.IRandom rng, float margin01);


        public List<IStatBase> Owners { get; } = new List<IStatBase>();

        ISet<IOwnable<IStatBase>> IOwner<IStatBase>.Pets => new DelegateSet<IOwnable<IStatBase>>(
            (() => Children?.ToValues() ?? Array.Empty<IStatBase>()), ownable =>
            {
                var stat = (IStatBase)ownable;
                if (Children.ToValues().FindByName((stat).Name) == null)
                    return false;
                stat.ChangeOwner(this);
                Children.Add(new SerializableInterface<IStatBase>(stat));
                return true;
            }, (ownable =>
            {
                var _stat = ((IStatBase)ownable);
                if (Children.ToValues().FindByName(_stat.Name) != null)
                    return false;
                _stat.ChangeOwner(null);
                Children.RemoveAll((@interface => @interface.Value.Name != _stat.Name));
                return true;
            }), (ownable => Children.ToValues().FindByName(((IStatBase)ownable).Name) != null), (() => Children.Clear())
        );

        List<IEntity> IPet<IEntity>.Owners => owners;

        public virtual void OnStart()
        {
        }

        public virtual void Tick()
        {
        }
    }
}