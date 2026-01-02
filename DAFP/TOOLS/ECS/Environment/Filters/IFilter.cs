using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    /// <summary>
    /// Base interface for reusable, composable GameObject-level filters.
    /// </summary>
    public interface IFilter<T >
    {
        /// <summary>
        /// Returns true if the provided GameObject passes the filter.
        /// </summary>
        bool Evaluate(T go);

        void Initialize(object self)
        {
            
        }
    }
}
