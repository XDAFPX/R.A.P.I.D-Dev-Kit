namespace BDeshi.BTSM
{
    public class IgnoreStatus : BtSingleDecorator
    {
        private BtStatus statusToOverrideWith;

        public IgnoreStatus(BtNodeBase child, BtStatus statusToOverrideWith = BtStatus.Ignore) : base(child)
        {
            this.statusToOverrideWith = statusToOverrideWith;
        }

        public override void Enter()
        {

        }

        public override BtStatus InternalTick()
        {
            Child.Tick();
            return statusToOverrideWith;
        }

        public override void Exit()
        {

        }
        
    }
}