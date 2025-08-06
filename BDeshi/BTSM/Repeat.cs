namespace BDeshi.BTSM
{
    /// <summary>
    /// Repeats child regardless of result, itself returning running
    /// After N times, return success
    /// If n < 0, keep ticking,
    /// </summary>
    public class Repeat:BtSingleDecorator
    {
        private int n = 0;
        private int c = 0;

        public Repeat(int n, BtNodeBase child) : base(child)
        {
            this.n = n;
        }

        
        public Repeat(BtNodeBase child) : base(child)
        {
            this.n = -1;
        }

        public override void Enter()
        {
            c = n;
            Child.Enter();
        }

        public override BtStatus InternalTick()
        {
            if (c == 0)
            {
                return BtStatus.Success;
            }

            var _r = Child.Tick();
            if (_r == BtStatus.Success || _r == BtStatus.Failure)
            {
                
                Child.Exit();

                if (c > 0)
                {
                    c--;
                    if (c != 0)
                    {
                        Child.Enter();
                    }
                }
                else 
                {
                    Child.Enter();
                }

                return BtStatus.Running;
            }

            return BtStatus.Running;
        }

        public override void Exit()
        {
            Child.Exit();
        }
    }
}