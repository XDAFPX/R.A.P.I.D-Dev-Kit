using System.Collections.Generic;
using UnityEngine;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.Common
{
    [System.Serializable]
    public class Cooldown : SafeFiniteTimer, IStat<float>, ISerializationCallbackReceiver
    {
        [SerializeField]private float maxValue;

        // ensure Unity serializes the list so it never stays null
        [SerializeField] 
        private List<StatModifier<float>> modifiers;

        [SerializeField]private float timer;

        public Cooldown(string name, float maxValue)
        {
            Name = name;
            modifiers = new List<StatModifier<float>>();
            this.maxValue = maxValue;
        }

        [field: SerializeField] public string Name { get; set; }

        public void ResetToDefault() => Reset();
        public bool SyncToBlackBoard => false;
        public object GetAbsoluteValue()
        {
            return Value;
        }

        public event IStatBase.UpdateValueCallBack OnUpdateValue;

        public void Randomize(float margin01) => throw new System.NotImplementedException();

        public override float Timer
        {
            get => timer;
            set { timer = value; OnUpdateValue?.Invoke(this); }
        }

        public float Value
        {
            get => Timer;
            set { }
        }

        public override float MaxValue
        {
            get
            {
                // safety fallback in case something went wrong
                if (modifiers == null)
                    modifiers = new List<StatModifier<float>>();

                float temp = maxValue;
                modifiers.Sort();
                foreach (var modifier in modifiers)
                    temp = modifier.Apply(temp);
                return temp;
            }
            set => maxValue = value;
        }

        public float MinValue { get => DefaultValue; set { } }
        public float DefaultValue { get => 0; set { } }

        public void SetToMax() => complete();
        public void SetToMin() => Reset();

        public void AddModifier(StatModifier<float> modifier)
        {
            if (modifiers == null) modifiers = new List<StatModifier<float>>();
            modifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier<float> modifier)
        {
            if (modifiers == null) return;
            modifiers.Remove(modifier);
        }

        public float TrueCoolDownTime => ReverseRatio * MaxValue;

        // ISerializationCallbackReceiver
        void ISerializationCallbackReceiver.OnBeforeSerialize() { /* nothing */ }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (modifiers == null)
                modifiers = new List<StatModifier<float>>();
        }


        public override string ToString()
        {
            return $"[{Name}] left : {MaxValue-Timer}s";
        }
    }
}