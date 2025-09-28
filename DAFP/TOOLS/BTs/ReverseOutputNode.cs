using System;
using BDeshi.BTSM;

namespace DAFP.TOOLS.BTs
{
    public class ReverseOutputNode  : BtSingleDecorator
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
            switch (output)
            {
                case BtStatus.Success:
                    return BtStatus.Failure;
                    break;
                case BtStatus.Failure:
                    return BtStatus.Success;
                    break;
                default:
                    return output;
            }
        }
    }
}