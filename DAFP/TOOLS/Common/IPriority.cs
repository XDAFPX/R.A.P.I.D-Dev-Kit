using System;

namespace DAFP.TOOLS.Common
{
    public interface IPriority<in T> : IComparable<T>,IPrioritized where T : IPrioritized
    {
        int IComparable<T>.CompareTo(T other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}