using DAFP.TOOLS.ECS.BigData.GlobalModifiers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class DecelerationBoard : FloatBoard
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

        [field: SerializeField] public override float MaxValue { get; set; }

        public override float MinValue
        {
            get => 0;
            set { }
        }

        [field: SerializeField] public override float DefaultValue { get; set; }
    }
}