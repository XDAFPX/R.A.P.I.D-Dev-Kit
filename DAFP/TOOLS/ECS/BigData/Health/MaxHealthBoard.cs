using System;
using DAFP.TOOLS.ECS.BigData.GlobalModifiers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Health
{
    [HealthModdable]
    public class MaxHealthBoard : FloatBoard, IStatDependent<float>
    {
        protected override void OnTick()
        {
        }


        protected override void OnStart()
        {
        }

        protected override void OnInitializeInternal()
        {
            
        }

        public override float MaxValue
        {
            get => Single.MaxValue;
            set {}
        }

        public override float MinValue
        {
            get => 0;
            set {}
        }

        public override float DefaultValue
        {
            get => Owner?.DefaultValue ?? 0;
            set { if(Owner!=null) Owner.DefaultValue = value; }
        }

        protected IDependentStat<float> Owner;

        public void Register(IDependentStat<float> owner)
        {
            Owner = owner;
            ResetToDefault();
        }

        public override void Randomize(float margin01)
        {
        }
    }
}