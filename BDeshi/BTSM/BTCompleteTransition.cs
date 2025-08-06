namespace BDeshi.BTSM
{
    public class BtCompleteTransition<TState> : ITransition<TState>
    where TState:IState
    {
        private IBtNode node;
        public IState SuccessState => SuccessTypedState;
        public bool TakenLastTime { get; set; }
        public bool TransitionToSameState { get; set; }
        public System.Action OnTaken { get; }

        public TState SuccessTypedState { get; private set; }

        public BtCompleteTransition(IBtNode node, TState typedState)
        {
            this.node = node;
            this.SuccessTypedState = typedState;
            // Debug.Log("??" + (successState == null?"nnnnnulll":successState.EditorName));
        }

        public bool Evaluate()
        {
            if(node.LastStatus==BtStatus.NotRunYet)
                node.Enter();
            var status = node.Tick() == BtStatus.Success;
            if(node.LastStatus!=BtStatus.Running)
                node.Enter();
            node.Exit();
            return status ;
        }

        public override string ToString()
        {
            return $"{node.EditorName}.complete->{SuccessState.FullStateName}";
        }
    }
}