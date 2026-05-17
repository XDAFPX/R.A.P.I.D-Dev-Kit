using System.Collections.Generic;
using UnityEngine;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;
using TNRD;

namespace DAFP.TOOLS.Common
{
    [System.Serializable]
    public class Cooldown : SafeFiniteTimer, IStat<float>
    {
        [SerializeField] private SerializableInterface<IStat<float>> maxStat;

        // ensure Unity serializes the list so it never stays null

        [SerializeField] private float timer;


        private Cooldown()
        {
        }

        public Cooldown(string name, IStat<float> stat)
        {
            Name = name;
            this.maxStat = new SerializableInterface<IStat<float>>(stat);
        }

        public Cooldown(string name, float stat)
        {
            Name = name;
            this.maxStat = new SerializableInterface<IStat<float>>(new QuikStat<float>(stat));
        }

        [field: SerializeField] public string Name { get; set; }

        public void ResetToDefault()
        {
            Reset();
        }

        public bool SyncToBlackBoard => false;

        public object GetAbsoluteValue()
        {
            return Value;
        }

        public event IStatBase.UpdateValueCallBack OnUpdateValue;

        public void Randomize(NRandom.IRandom rng, float margin01)
        {
            throw new System.NotImplementedException();
        }

        public override float Timer
        {
            get => timer;
            set
            {
                timer = value;
                OnUpdateValue?.Invoke(this);
            }
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
                if (maxStat == null) return 0;
                return maxStat.Value.Value;
            }
            set
            {
                if (maxStat == null) return;
                maxStat.Value.Value = value;
            }
        }

        public float MinValue
        {
            get => DefaultValue;
            set { }
        }

        public float DefaultValue
        {
            get => 0;
            set { }
        }

        public void SetToMax()
        {
            complete();
        }

        public void SetToMin()
        {
            Reset();
        }

        public void AddModifier(StatModifier<float> modifier)
        {
            maxStat.Value.AddModifier(modifier);
        }

        public void RemoveModifier(StatModifier<float> modifier)
        {
            maxStat.Value.RemoveModifier(modifier);
        }

        public void RemoveModifier(string name)
        {
            maxStat.Value.RemoveModifier(name);
        }

        public float TrueCoolDownTime => ReverseRatio * MaxValue;
        public bool IsOnCooldown => !isComplete;

        // ISerializationCallbackReceiver


        public override string ToString()
        {
            return $"[{Name}] left : {MaxValue - Timer}s";
        }

        public List<IStatBase> Children { get; } = new();
        public List<IStatBase> Owners { get; } = new List<IStatBase>();

        List<IEntity> IPetOf<IEntity, IStatBase>.Owners { get; } = new();
    }
}