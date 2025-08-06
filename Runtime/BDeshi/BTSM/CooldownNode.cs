using Bdeshi.Helpers.Utility;
using UnityEngine;

namespace BDeshi.BTSM
{
    /// <summary>
    /// prevent running child for x secs after it has run || it started running
    /// </summary>
   public class CooldownNode : BtSingleDecorator
    {
        private FiniteTimer cooldownTimer;
        public float WaitDuration = 5;

        private bool isRunning;
        private CoolDownResetType resetType;
        private readonly BtStatus cooldownFailStatus;

        public enum CoolDownResetType
        {
            ResetOnStart,
            ResetOnExit,
            Dont
        }

        public CooldownNode(BtNodeBase child, float waitDuration, BtStatus cooldownFailStatus = BtStatus.Failure, CoolDownResetType resetType = CoolDownResetType.ResetOnExit, bool shouldWaitAtStart = false)
            : base(child)
        {
            this.WaitDuration = waitDuration;
            this.cooldownFailStatus = cooldownFailStatus;
            this.resetType = resetType;
            this.isRunning = !shouldWaitAtStart;
            cooldownTimer = new FiniteTimer(waitDuration, shouldWaitAtStart);
        }

        public void ResetCoolDown()
        {
            cooldownTimer.reset(WaitDuration);
        }

        public void ResetCoolDown(float duration)
        {
            cooldownTimer.reset(duration);
        }

        public void SkipCoolDown()
        {
            cooldownTimer.Timer = cooldownTimer.MaxValue;
        }

        public bool IsOnCooldown()
        {
            return !cooldownTimer.isComplete;
        }

        public override void Enter()
        {
            isRunning = false;
            cooldownTimer.MaxValue = WaitDuration;

            if (resetType == CoolDownResetType.ResetOnStart || lastStatus == BtStatus.NotRunYet)
            {
                cooldownTimer.reset();
            }
        }

        private void StartRunning()
        {
            isRunning = true;
            Child.Enter();
        }

        public override BtStatus InternalTick()
        {
            UpdateManual();

            if (isRunning)
            {
                var _childStatus = Child.Tick();
                if (_childStatus == BtStatus.Success)
                {
                    cooldownTimer.reset(WaitDuration);
                    isRunning = false;
                }
                return _childStatus;
            }
            else if (!IsOnCooldown())
            {
                if (Child == null)
                {
                    cooldownTimer.reset(WaitDuration);
                    return BtStatus.Success;
                }
                else
                {

                    StartRunning();
                    var _childStatus = Child.Tick();
                    if (_childStatus == BtStatus.Success)
                    {
                        cooldownTimer.reset(WaitDuration);
                        isRunning = false;
                    }
                    return _childStatus;
                }
            }
            else
            {
                return cooldownFailStatus;
            }
        }

        public FiniteTimer GetTimer() => cooldownTimer;
        public void UpdateManual()
        {
            cooldownTimer.updateTimer(Time.deltaTime);
        }
        public void UpdateManual(float deltaTime)
        {
            cooldownTimer.updateTimer(deltaTime);
        }

        public override void Exit()
        {
            if (isRunning)
            {
                Child.Exit();
            }

            if (resetType == CoolDownResetType.ResetOnExit)
            {
                cooldownTimer.reset(WaitDuration);
            }
        }

        public override string EditorName => $"{base.EditorName} [{cooldownTimer.remaingValue():0.00}s left]";
    }
}
