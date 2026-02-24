using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData.Modifiers;
using ModestTree;
using UnityEngine;
using UnityEngine.Serialization;
using NRandom;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    [Serializable]
    public abstract class WhiteBoard<T> : IStat<T>, IInitializable, ITickable, IPetOf<IEntity, IStatBase>
    {
        [SerializeField] protected List<SerializableInterface<IPegStatModifier<T>>> PegModifiers = new();
        [field: SerializeField] public string Name { get; set; }

        public abstract bool SyncToBlackBoard { get; }


        public abstract T MaxValue { get; set; }
        public abstract T MinValue { get; set; }
        public abstract T DefaultValue { get; set; }

        protected IEntity Host => ((IPetOf<IEntity,IStatBase>)this).GetCurrentOwner();


#if UNITY_EDITOR
        [ReadOnly] [SerializeField] private T RealValue;
#endif
        [ReadOnly] [SerializeField] protected T InternalValue;


        [SerializeField] protected List<SerializableInterface<IStatModifier<T>>> Modifiers = new();

        [FormerlySerializedAs("Children")]
        public List<SerializableInterface<IStatBase>> ChildrenStats = new List<SerializableInterface<IStatBase>>();


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
            configure_stat_owner();
            configure_peg_modifiers();

            ResetToDefault();
            OnInitializeInternal();
        }

        private void configure_stat_owner()
        {
            var children = ChildrenStats.ToValues().ToArray();
            foreach (var _statBase in children)
            {
                _statBase.ChangeOwner(this);
                if (_statBase is IInitializable _tickable)
                {
                    _tickable.Initialize();
                }
            }
        }

        private void configure_peg_modifiers()
        {
            var _own = ((IOwnedBy<IStatBase>)this).GetCurrentOwner();
            foreach (var _statBase in PegModifiers.ToValues())
            {
                _statBase.Peg = _own;
            }
        }

        protected abstract void OnInitializeInternal();

        private T internal_get_value()
        {
            var _temp = InternalValue;
            List<IStatModifier<T>> _mods = new List<IStatModifier<T>>();
            List<IPegStatModifier<T>> _pegs = new();
            if (!PegModifiers.IsEmpty())
            {
                _pegs = PegModifiers.ToValues().ToList();
                _pegs.Sort();
            }

            if (!Modifiers.IsEmpty())
            {
                _mods = Modifiers.ToValues().ToList();
                _mods.Sort();
            }

            _temp = _pegs.Aggregate(_temp, (current, modifier) => modifier.Apply(current));
            _temp = _mods.Aggregate(_temp, (current, modifier) => modifier.Apply(current));

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


        private List<IEntity> owners = new List<IEntity>();
        private List<IStatBase> owners1;
        private List<IEntity> owners2;

        public void AddModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;

            var _mods = Modifiers.ToValues().ToList();
            if (_mods.Contains(modifier))
                return;
            OnModifierAdded?.Invoke(modifier);
            Modifiers.Add(new SerializableInterface<IStatModifier<T>>(modifier));
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            if (modifier == null)
                return;
            var _mods = Modifiers.ToValues().ToList();
            if (!_mods.Contains(modifier))
                return;
            OnModifierRemoved?.Invoke(modifier);
            Modifiers.RemoveAll((@interface => @interface.Value.Equals(modifier)));
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
            Modifiers.Clear();
        }

        public void RemoveAllModifiersFrom(IEntity owner)
        {
            Modifiers.RemoveAll(modifier => modifier.Value.GetCurrentOwner() == owner);
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


        public virtual void OnStart()
        {
        }

        public virtual void Tick()
        {
        }

        public List<IStatBase> Children => ChildrenStats.ToValues().ToList();

        public void AddPet(IStatBase pet)
        {
            if (pet == null) return;
            if (Children.Contains(pet)) return;
            ChildrenStats.Add(new SerializableInterface<IStatBase>(pet));
        }

        public bool RemovePet(IStatBase pet)
        {
            if (pet == null) return false;
            if (!Children.Contains(pet)) return false;
            ChildrenStats.Remove(new SerializableInterface<IStatBase>(pet));
            return true;
        }

        List<IStatBase> IPetOwnerTreeOf<IStatBase>.Owners => owners1;

        List<IEntity> IPetOf<IEntity, IStatBase>.Owners => owners2;
    }
}