using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public class ScaleBoard : Vector3Board
    {
        protected override void OnInitializeInternal()
        {
            DefaultValue = transform.localScale;
            ResetToDefault();
        }

        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            return value;
        }

        public override Vector3 MaxValue => Vector3.one * 10000;
        public override Vector3 MinValue => Vector3.zero;
        

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
            Value = transform.localScale;
        }

    }
}