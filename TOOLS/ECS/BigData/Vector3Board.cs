using DAFP.TOOLS.Common;
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

        protected override Vector3 GetValue()
        {
            return InternalValue;
        }


        protected override void SetValue(Vector3 value)
        {
            InternalValue = Vector3.ClampMagnitude(value, MaxValue.magnitude);
        }

        [field: SerializeField] public override Vector3 MaxValue { get; set; }
        [field: SerializeField] public override Vector3 MinValue { get; set; }
        [field: SerializeField] public override Vector3 DefaultValue { get; set; }

        public override void SetToMax()
        {
            SetValue(MaxValue);
        }

        public override void SetToMin()
        {
            SetValue(MinValue);
        }

        protected override void ResetInternal()
        {
            SetValue(DefaultValue);
        }
    }
}