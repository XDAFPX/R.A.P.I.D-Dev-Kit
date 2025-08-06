using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    [RequireComponent(typeof(HpBoard))]
    public class MaxHpBoard : FloatBoard
    {
        public override float MaxValue => Mathf.Infinity;
        public override float MinValue => 0;
        public override float DefaultValue => board.DefaultValue;
        
        private HpBoard board;
        protected override void OnTick()
        {
            
        }

        protected override void OnInitialize()
        {
            board = Host.GetEntComponent<HpBoard>();
        }

        protected override void OnStart()
        {
        }
    }
}