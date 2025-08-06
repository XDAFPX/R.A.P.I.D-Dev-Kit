namespace BDeshi.BTSM
{
    public class EnterExitDecorator: BtSingleDecorator
    {
        public System.Action OnEnter;
        public System.Action OnExit;

        public EnterExitDecorator(BtNodeBase child, System.Action onEnter, System.Action onExit = null) : base(child)
        {
            OnEnter = onEnter;
            OnExit = onExit;
        }

        public override void Enter()
        {
            OnEnter?.Invoke();
            Child.Enter();
        }

        public override BtStatus InternalTick()
        {
            return Child.Tick();
        }

        public override void Exit()
        {
            OnExit?.Invoke();
            Child.Exit();
        }
    }
}