using System;

namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public class MaxLockModifier<T> : StatModifier<T>
    {
        public enum StatValue
        {
            Max,
            Min,
            Default
        }

        private readonly StatValue lockedValue;
        private readonly IStat<T> parent;
        private readonly int setPriority;

        public MaxLockModifier(IEntity owner, StatValue lockedValue, IStat<T> parent, string name, int priority=default) : base(
            owner, name)
        {
            this.lockedValue = lockedValue;
            this.parent = parent;
            setPriority = priority;
        }

        public override T Apply(T value)
        {
            switch (lockedValue)
            {
                case StatValue.Max:
                    return parent.MaxValue;
                case StatValue.Min:
                    return parent.MinValue;
                case StatValue.Default:
                    return parent.DefaultValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int Priority
        {
            get => 1000 + setPriority;
            set { }
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }

    public class LockModifier<T> : StatModifier<T>
    {
        private readonly T lockedValue;
        private readonly int setPriority;

        public LockModifier(IEntity owner, T lockedValue, string name, int priority = default) : base(owner, name)
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
            set { }
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}