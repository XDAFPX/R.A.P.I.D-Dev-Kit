using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public class ModularState: StateBase
    {
        private System.Action onEnter;
        private System.Action onTick;
        private System.Action onExit;

        public ModularState(System.Action onEnter = null, System.Action onTick = null, System.Action onExit = null,HashSet<IState._stateTags> tags = null)
        {
            if(tags!=null)
                StateTagsInternal = tags;
            StateTagsInternal = new HashSet<IState._stateTags>();
            this.onEnter = onEnter;
            this.onTick = onTick;
            this.onExit = onExit;
        }

        public override void EnterState()
        {
            onEnter?.Invoke();
        }

        public override void Tick()
        {
            onTick?.Invoke();
        }

        public override void ExitState()
        {
            onExit?.Invoke();
        }
    }
}