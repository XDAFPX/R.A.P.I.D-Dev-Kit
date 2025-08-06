namespace BDeshi.BTSM
{
    public class FailTillComplete : BtSingleDecorator
    {

        public override void Enter()
        {
            
        }

        public override BtStatus InternalTick()
        {
            var _result = Child.Tick();
            return _result == BtStatus.Success ? BtStatus.Failure : BtStatus.Success;
        }

        public override void Exit()
        {
            
        }

        public FailTillComplete(BtNodeBase child) : base(child)
        {
        }
    }
}