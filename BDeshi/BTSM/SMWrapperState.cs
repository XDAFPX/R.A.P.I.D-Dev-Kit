using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace BDeshi.BTSM
{
    public class SmWrapperState : StateBase
    {
        public override string FullStateName => $"{StateName}.{machine?.CurState?.FullStateName}";
        public override string StateName { get; }

        private IRunnableStateMachine machine; 
        
        public SmWrapperState( string StateName, [NotNull] IRunnableStateMachine machine, HashSet<IState._stateTags> tags = null)
        {
            StateTagsInternal = tags ?? new HashSet<IState._stateTags>();
            this.StateName = StateName;
            this.machine = machine;
        }

        public override void EnterState()
        {
            machine.Enter();
        }

        public override void Tick()
        {
            machine.Tick();
        }

        public override void ExitState()
        {
            machine.Cleanup();
        }
    }
}