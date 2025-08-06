using Bdeshi.Helpers.Utility;
using UnityEngine;

namespace BDeshi.BTSM
{
    /// <summary>
    /// Run child for x secs
    /// </summary>
    public class TimeoutNode: BtSingleDecorator
    {
        public FiniteTimer Timer;
      
        public override void Enter()
        {
            Timer.reset();
        }

        public override BtStatus InternalTick()
        {
            if (Timer.isComplete)
            {
                return BtStatus.Success;
            }
            else
            {
                Timer.safeUpdateTimer(Time.deltaTime);
                return Child.Tick();
            }
        }

        public override void Exit()
        {
            
        }

        public TimeoutNode(BtNodeBase child, float timerDuration) : base(child)
        {
            Timer = new FiniteTimer(timerDuration);
        }
    }
}