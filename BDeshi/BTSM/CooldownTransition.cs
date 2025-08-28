using BDeshi.BTSM;

namespace DAFP.TOOLS.BTs
{
    public class CooldownTransition<TState> : ITransition<TState>
        where TState:IState
    {
        private CooldownNode node;
        public IState SuccessState => SuccessTypedState;
        public bool TakenLastTime { get; set; }
        public bool TransitionToSameState { get; set; }
        public System.Action OnTaken { get; }

        public TState SuccessTypedState { get; private set; }

        public CooldownTransition(CooldownNode node, TState typedState)
        {
            this.node = node;
            this.SuccessTypedState = typedState;
            // Debug.Log("??" + (successState == null?"nnnnnulll":successState.EditorName));
        }

        public bool Evaluate()
        {
            return !node.IsOnCooldown();
        }

        public override string ToString()
        {
            return $"cooldown(left:{node.GetTimer().remaingValue()}s).complete->{SuccessState.FullStateName}";
        }
    }
}