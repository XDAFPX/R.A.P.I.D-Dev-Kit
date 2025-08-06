using Bdeshi.Helpers.Utility;
using UnityEngine;

namespace BDeshi.BTSM
{
    public class WaitNode: BtNodeBase
    {
        protected FiniteTimer Timer;

        public WaitNode(float waitTime = 1.9f)
        {
            Timer = new FiniteTimer(waitTime);
        }

        public override void Enter()
        {
            Timer.reset();
        }

        public override BtStatus InternalTick()
        {
            if (Timer.tryCompleteTimer(Time.deltaTime))
            {
                return BtStatus.Success;
            }

            return BtStatus.Running;
        }

        public override void Exit()
        {
            Timer.reset();
        }
    }
}