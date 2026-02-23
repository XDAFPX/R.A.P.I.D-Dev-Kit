namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public class LockModifier<T> : StatModifier<T>
    {
        private readonly T lockedValue;
        private readonly int setPriority;

        public LockModifier(IEntity owner, T lockedValue, int priority) : base(owner)
        {
            this.lockedValue = lockedValue;
            setPriority = priority;
        }

        public override T Apply(T value)
        {
            return lockedValue;
        }

        public override int Priority
        {
            get => 1000 + setPriority;
            set
            {
            }
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}