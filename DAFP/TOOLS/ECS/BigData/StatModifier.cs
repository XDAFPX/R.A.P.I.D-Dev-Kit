using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// Abstract modifier record for IStat, implementing IPet and IDisposable.
    /// </summary>
    public interface IStatModifierBase : IDisposable, IPrioritized,IOwnedBy<IEntity>
    {
    }

    public interface IStatModifier<T> : IComparable<IStatModifier<T>>, IPetOf<IEntity,IStatModifierBase>, IStatModifierBase
    {
        T Apply(T value);

        int IComparable<IStatModifier<T>>.CompareTo(IStatModifier<T> other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }


// Abstract base remains but implements the interface
    public abstract class StatModifier<T> : IStatModifier<T>
    {
        protected StatModifier(IEntity owner)
        {
            Owners = new List<IEntity>() { owner };
        }

        public abstract T Apply(T value);
        public abstract int Priority { get; set; }

        public int CompareTo(IStatModifier<T> other)
        {
            return Priority.CompareTo(other.Priority);
        }

        // IEntityOwnable implementation

        public List<IEntity> Owners { get; } = new List<IEntity>();


        public abstract void Dispose();
    }
}