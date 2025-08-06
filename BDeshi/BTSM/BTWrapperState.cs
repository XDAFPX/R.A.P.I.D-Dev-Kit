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
        public BtStatus LastStatus => lastStatus;

        /// <summary>
        /// Creates a transition to a state when the root BT node is complete.
        /// DOES NOT automagically go to statemachine.
        /// Do that yourself.
        /// </summary>
        /// <returns>Newly Created transition.</returns>
        private readonly string stateName;
        public override string FullStateName => $"<{stateName}>";
        
        public BtCompleteTransition<TState> CreateRootSuccessTransition<TState>(TState to)
        where  TState:IState
        {
            return new BtCompleteTransition<TState>(this.BtRoot, to);
        }

        public BtWrapperState(BtNodeBase btRoot,string stateName, HashSet<IState._stateTags> tags= null)
        {
            StateTagsInternal = tags ?? new HashSet<IState._stateTags>();
            this.BtRoot = btRoot;
            this.stateName = stateName;
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