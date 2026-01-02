using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS;

namespace DAFP.TOOLS.ECS.Thinkers
{
    /// <summary>
    /// Wraps a BaseThinker so it can be used as a BTSM IState.
    /// Enter/Exit map to Initialize/Dispose, and Tick maps directly.
    /// </summary>
    public sealed class ThinkerWrapperState : StateBase
    {
        private readonly BaseThinker thinker;
        private readonly IEntity host;
        private readonly string stateName;

        public ThinkerWrapperState(string stateName, BaseThinker thinker, IEntity host,
            HashSet<IState._stateTags> tags = null)
        {
            this.stateName = stateName;
            this.thinker = thinker;
            this.host = host;
            StateTagsInternal = tags ?? new HashSet<IState._stateTags>();
        }

        public override string FullStateName => StateName;
        public override string StateName => stateName;

        // Thinkers generally represent continuous behaviours, so we keep it Running.
        public override BtStatus LastStatus { get; } = BtStatus.Running;

        public override void EnterState()
        {
            // Re-initialize on every entry to ensure fresh state
            thinker?.Initialize(host);
        }

        public override void Tick()
        {
            // Use the host's entity ticker by default
            thinker?.Tick(host, host.EntityTicker);
        }

        public override void ExitState()
        {
            thinker?.Dispose(host);
        }
    }
}
