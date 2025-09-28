using DAFP.TOOLS.ECS.BigData;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components.GrabController
{
    public class CarryWeightBoard : FloatBoard
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
        [field: SerializeField] public override float MinValue { get; set; }
        [field: SerializeField] public override float DefaultValue { get; set; }
    }
}