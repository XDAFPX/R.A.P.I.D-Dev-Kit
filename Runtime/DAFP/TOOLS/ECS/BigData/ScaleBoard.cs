using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public class ScaleBoard : Vector3Board
    {
        protected override void OnInitialize()
        {
            DefaultValue = transform.localScale;
            SetValue(transform.localScale);
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnTick()
        {

            SetValue(transform.localScale);
        }

        protected override void SetValue(Vector3 value)
        {
            InternalValue = value;
        }
    }
}