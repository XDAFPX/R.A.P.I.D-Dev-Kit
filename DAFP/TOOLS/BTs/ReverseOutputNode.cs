using System;
using BDeshi.BTSM;

namespace DAFP.TOOLS.BTs
{
    public class ReverseOutputNode : BtSingleDecorator
    {
        public ReverseOutputNode(IBtNode child) : base(child)
        {
        }

        public override void Enter()
        {
            Child.Enter();
        }

        public override void Exit()
        {
            Child.Exit();
        }

        public override BtStatus InternalTick()
        {
            var output = Child.Tick();
            return output switch
            {
                BtStatus.Success => BtStatus.Failure,
                BtStatus.Failure => BtStatus.Success,
                _ => output
            };
        }
    }
}