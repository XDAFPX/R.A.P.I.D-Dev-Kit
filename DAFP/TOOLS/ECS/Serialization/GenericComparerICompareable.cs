using System;
using System.Collections.Generic;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    /// <summary>
    /// Fully generic comparer for any T; if an item implements IComparable&lt;T&gt; or IComparable,
    /// that is used, otherwise items are treated as equal (0).
    /// </summary>
    public class GenericComparerICompareable<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            // Step 2: Same reference or both null => equal
            if (ReferenceEquals(x, y))
                return 0;

            // Treat null as less than any non-null
            if (x is null)
                return y is null ? 0 : -1;
            if (y is null)
                return 1;

            // Step 3: x implements IComparable<T>
            if (x is IComparable<T> genX)
            {
                try { return genX.CompareTo(y); }
                catch { return 0; }
            }

            // x implements non-generic IComparable
            if (x is IComparable cmpX)
            {
                try { return cmpX.CompareTo(y); }
                catch { return 0; }
            }

            // Step 4: y implements IComparable<T> (invert result)
            if (y is IComparable<T> genY)
            {
                try { return -genY.CompareTo(x); }
                catch { return 0; }
            }

            // y implements non-generic IComparable (invert result)
            if (y is IComparable cmpY)
            {
                try { return -cmpY.CompareTo(x); }
                catch { return 0; }
            }

            // Step 5: neither side comparable => equal
            return 0;
        }
    }
}
