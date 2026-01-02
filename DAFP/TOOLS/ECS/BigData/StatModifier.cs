using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// Abstract modifier record for IStat, implementing IPet and IDisposable.
    /// </summary>
    public interface IStatModifier<T> : IComparable<IStatModifier<T>>, IEntityOwnable, IDisposable
    {
        T Apply(T value);
        int Priority { get; }
    }


// Abstract base remains but implements the interface
    public abstract class StatModifier<T> : IStatModifier<T>
    {
        protected StatModifier(IEntity owner)
        {
            Owner = owner;
        }

        public abstract T Apply(T value);
        public abstract int Priority { get; }

        public int CompareTo(IStatModifier<T> other)
        {
            return Priority.CompareTo(other.Priority);
        }

        // IEntityOwnable implementation
        protected IEntity Owner;

        public virtual IEntity GetCurrentOwner()
        {
            return Owner;
        }

        public virtual void ChangeOwner(IEntity newOwner)
        {
            Owner = newOwner;
        }

        public abstract void Dispose();
    }
}