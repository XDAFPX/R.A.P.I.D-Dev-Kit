using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// Abstract modifier record for IStat, implementing IPet and IDisposable.
    /// </summary>
    [Serializable]
    public abstract class StatModifier<T> : IOwnable,
        IDisposable,
        IComparable<StatModifier<T>>
    {
        protected StatModifier(IEntity owner)
        {
            Owner = owner;
        }
        public abstract T Apply(T value);

        public abstract int Priority { get; }

        // IPet and IDisposable as before...

        public int CompareTo(StatModifier<T> other)
        {
            // Lower priority value means higher precedence
            return Priority.CompareTo(other.Priority);
        }

        // IPet implementation
        protected IEntity Owner;
        public virtual IEntity GetCurrentOwner()
        {

            return Owner;
        }

        public virtual void ChangeOwner(IEntity newOwner)
        {
            Owner = newOwner;
        }

        // IDisposable implementation
        public abstract void Dispose();
    }
}