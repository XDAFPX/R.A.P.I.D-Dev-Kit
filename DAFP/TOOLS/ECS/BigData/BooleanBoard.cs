using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class BooleanBoard : WhiteBoard<bool>
    {

        protected override bool GetValue(bool processedValue)
        {
            return processedValue;
        }

        protected override bool ClampAndProcessValue(bool value)
        {
            return value;
        }

        public override bool MaxValue { get; set; }
        public override bool MinValue { get; set; }
        public override void SetToMax()
        {
            Value = true;
        }

        public override void SetToMin()
        {
            Value = false;
        }

        protected override void ResetInternal()
        {
            Value = DefaultValue;
        }

        public override void Randomize(float margin01)
        {
            
        }
    }
}