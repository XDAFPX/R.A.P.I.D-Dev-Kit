using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class MeleeDamageBoard : DamageBoard
    {
        [field: SerializeField] public override Damage DefaultValue { get; set; }

        protected override void OnTick()
        {
            
        }

        protected override void OnStart()
        {
        }


        public override bool SyncToBlackBoard => true;
        protected override void OnInitializeInternal()
        {
            
        }
    }
}