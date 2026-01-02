using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public class ModularState: StateBase
    {
        private System.Action onEnter;
        private System.Action onTick;
        private System.Action onExit;
        private BtStatus lastStatus = BtStatus.NotRunYet;
        public override string FullStateName => StateName;

        public ModularState(string stateName, System.Action onEnter = null, System.Action onTick = null, System.Action onExit = null,HashSet<IState._stateTags> tags = null)
        {
            if(tags!=null)
                StateTagsInternal = tags;
            StateTagsInternal = new HashSet<IState._stateTags>();
            StateName = stateName;
            this.onEnter = onEnter;
            this.onTick = onTick;
            this.onExit = onExit;
        }

        public override string StateName { get; }

        public override BtStatus LastStatus => lastStatus;

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