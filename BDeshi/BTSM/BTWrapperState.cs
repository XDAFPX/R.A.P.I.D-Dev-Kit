using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public interface IBtWrapperState
    {
         IBtNode BtRoot { get; }
         public BtStatus LastStatus{ get; }

    }
    public class BtWrapperState: StateBase, IBtWrapperState
    {
        public IBtNode BtRoot { get; private set; }
        private BtStatus lastStatus;
        public override BtStatus LastStatus => lastStatus;

        /// <summary>
        /// Creates a transition to a state when the root BT node is complete.
        /// DOES NOT automagically go to statemachine.
        /// Do that yourself.
        /// </summary>
        /// <returns>Newly Created transition.</returns>

        public override string FullStateName => StateName;
        public override string StateName { get; }

        public BtCompleteTransition<TState> CreateRootSuccessTransition<TState>(TState to)
        where  TState:IState
        {
            return new BtCompleteTransition<TState>(this.BtRoot, to);
        }

        public BtWrapperState(BtNodeBase btRoot,string stateName, HashSet<IState._stateTags> tags= null)
        {
            StateTagsInternal = tags ?? new HashSet<IState._stateTags>();
            this.BtRoot = btRoot;
            this.StateName = stateName;
        }
        

        public override void EnterState()
        {
            BtRoot.Enter();
        }

        public override void Tick()
        {
            lastStatus = BtRoot.Tick();
        }

        public override void ExitState()
        {
            BtRoot.Exit();
        }
    }
}