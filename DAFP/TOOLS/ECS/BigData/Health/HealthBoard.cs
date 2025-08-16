using System;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData.GlobalModifiers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Health
{
    [HealthModdable]
    [RequireComponent(typeof(MaxHealthBoard))]
    public class HealthBoard : DependentResourceBoard<float, MaxHealthBoard, MaxHealthBoard>
    {
        [ReadOnly(OnlyWhilePlaying = true)] [Min(0)] [SerializeField]
        private float __defaultValue;



        public override float DefaultValue
        {
            get => __defaultValue;
            set => __defaultValue = value;
        }

        public override float Percentage => Value / MaxValue;

        public override string GetFormatedMaxValue()
        {
            return Math.Round(MaxValue, 1).ToString();
        }

        public override string GetFormatedCurValue()
        {
            return Math.Round(Value, 1).ToString();
        }

        public override float Add(float amount, bool forced = false)
        {
            Value = Value + amount;
            return Value;
        }

        public override float Remove(float amount, bool forced = false)
        {
            Value = Value - amount;
            return Value;
        }

        public override void Randomize(float margin01)
        {
            MaxSource.DefaultValue = MaxSource.DefaultValue.Randomize(margin01);
            MaxSource.ResetToDefault();
            ResetToDefault();
        }


        public override float MinValue => 0;
        public override bool SyncToBlackBoard => true;


        protected override void OnStart()
        {
            
        }
    }
}