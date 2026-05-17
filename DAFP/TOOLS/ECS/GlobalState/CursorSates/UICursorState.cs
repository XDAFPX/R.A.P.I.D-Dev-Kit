using UnityEngine;

namespace DAFP.TOOLS.ECS.GlobalState.CursorSates
{
    public class UICursorState : BasicCursorState
    {
        public UICursorState() : base(null, new CursorSettings(CursorLockMode.None, true), "UI")
        {
        }
    }
}