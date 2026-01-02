using System;
using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using NRandom;
using Optional;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Damage
{
    public class DamageBoard : WhiteBoard<uint>, IDamageBoard
    {

        public override bool SyncToBlackBoard { get; } = true;

        protected override void OnInitializeInternal()
        {
        }

        protected override uint GetValue(uint processedValue)
        {
            return processedValue;
        }

        protected override uint ClampAndProcessValue(uint value)
        {
            return Math.Clamp(value, MinValue, MaxValue);
        }

        [field: SerializeField] public override uint MaxValue { get; set; }

        public override uint MinValue
        {
            get => 0;
            set { }
        }

        [field: SerializeField] public override uint DefaultValue { get; set; }
        [SerializeField] private GameplayTagContainer TagContainer;

        public override void SetToMax()
        {
            Value = MaxValue;
        }

        public override void SetToMin()
        {
            Value = MinValue;
        }

        protected override void ResetInternal()
        {
            Value = DefaultValue;
        }

        public override void Randomize(IRandom rng, float margin01)
        {
            Value = Value.Randomize(margin01);
        }


        public IDamage Construct(Option<IEntity> ent, Option<IVectorBase> vec, IStat<uint> stat)
        {
            return new Environment.DamageSys.Damage(new DamageInfo(stat, new DamageSource(ent, vec), Tag));
        }

        public IHaveGameplayTag Tag => TagContainer;
    }
}