using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class GroundedBoard : BooleanBoard
    {
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

        [field : SerializeField]public override bool DefaultValue { get; set; }
    }
}