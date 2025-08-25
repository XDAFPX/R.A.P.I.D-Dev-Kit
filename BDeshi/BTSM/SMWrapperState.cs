using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace BDeshi.BTSM
{
    public class SmWrapperState : StateBase
    {
        private readonly string stateName;
        public override string FullStateName => stateName;

        private IRunnableStateMachine machine; 
        
        public SmWrapperState( string stateName, [NotNull] IRunnableStateMachine machine, HashSet<IState._stateTags> tags = null)
        {
            StateTagsInternal = tags ?? new HashSet<IState._stateTags>();
            this.stateName = stateName;
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