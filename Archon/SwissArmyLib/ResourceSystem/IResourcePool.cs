using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// Interface for resource pool functionality.
    /// </summary>
    public interface IResourcePool<T> : IPercentageProvider
    {
        /// <summary>
        /// Gets how full the resource is percentage-wise (0 to 1).
        /// </summary>

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets whether the pool is completely full.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Gets the (scaled) time since this pool was last empty.
        /// </summary>
        float TimeSinceEmpty { get; }

        /// <summary>
        /// Adds the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        T Add(T amount, bool forced = false);

        /// <summary>
        /// Removes the specified amount of resource from the pool.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        T Remove(T amount, bool forced = false);

        /// <summary>
        /// Completely empties the pool.
        /// </summary>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        T Empty(bool forced = false);

        /// <summary>
        /// Fully fills the pool.
        /// </summary>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        T Fill(bool forced = false);

        /// <summary>
        /// Fills the pool to the specified amount.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        T Fill(T toValue, bool forced = false);

    }
}