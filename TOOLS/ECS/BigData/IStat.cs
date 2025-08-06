using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// Abstract modifier record for IStat, implementing IPet and IDisposable.
    /// </summary>
    public abstract record StatModifier<T> : IPet,
        IDisposable,
        IComparable<StatModifier<T>>
    {
        public abstract T Apply(T value);

        public abstract int Priority { get; }

        // IPet and IDisposable as before...

        public int CompareTo(StatModifier<T> other)
        {
            // Lower priority value means higher precedence
            return Priority.CompareTo(other.Priority);
        }

        // IPet implementation
        public abstract List<Entity> Owners { get; }
        public abstract Entity GetCurrentOwner();
        public abstract Entity GetExOwner();
        public abstract void ChangeOwner(Entity newOwner);

        // IDisposable implementation
        public abstract void Dispose();
    }

    public interface IStat<T> : INameable, IStatBase, IRandomizeable
    
    {
        public T Value { get; set; }
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }
        public void SetToMax();
        public void SetToMin();

        void AddModifier(StatModifier<T> modifier);
        void RemoveModifier(StatModifier<T> modifier);
    }
}