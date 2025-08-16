using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class Vector3Board : WhiteBoard<Vector3>
    {
        public override bool SyncToBlackBoard { get; }

        public override void Randomize(float margin01)
        {
            Value = Value.Randomize(margin01);
        }

        protected override Vector3 GetValue(Vector3 ProcessedValue)
        {
            return ProcessedValue;
        }



        public override Vector3 MaxValue { get; set; }
        public override Vector3 MinValue { get; set; }
        public override Vector3 DefaultValue { get; set; }

        public override void SetToMax()
        {
            Value = MaxValue;
        }

        public override void SetToMin()
        {
            Value = MaxValue;

        }

        protected override void ResetInternal()
        {
            InternalValue = DefaultValue;
        }
    }
}