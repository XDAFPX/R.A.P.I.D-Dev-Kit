using System.Collections.Generic;
using BDeshi.BTSM;
using UnityEngine;

namespace DAFP.TOOLS.ECS.GlobalState.CursorSates
{
    public class NormalCursorState : BasicCursorState
    {
        public NormalCursorState() : base(null, new CursorSettings(CursorLockMode.Confined,false), "Normal")
        {
        }
    }
}