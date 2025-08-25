using System.Data;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class AirModifierBoard : FloatBoard
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
            get => 1; set{} }
        public override float MinValue
        {
            get => 0; set{} }
        [field: SerializeField] public override float DefaultValue { get; set; }
    }
}