using System.Collections.Generic;
using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Services
{
    /// <summary>
    /// Minimal spatial partitioning interface for querying nearby entities efficiently.
    /// </summary>
    public interface ISpacePartitioningSystem
    {
        /// <summary>
        /// Adds an entity to the spatial index.
        /// </summary>
        void Add(IEntity entity);

        /// <summary>
        /// Removes an entity from the spatial index.
        /// </summary>
        void Remove(IEntity entity);

        /// <summary>
        /// Updates entity's position in the index. Call when entity moves.
        /// </summary>
        void Update(IEntity entity);

        /// <summary>
        /// Removes all entities.
        /// </summary>
        void Clear();

        /// <summary>
        /// Queries entities that may overlap with given bounds.
        /// </summary>
        IEnumerable<IEntity> Query(Bounds bounds);

        /// <summary>
        /// Queries entities within a radius of a point (best-effort based on index granularity).
        /// </summary>
        IEnumerable<IEntity> Query(Vector3 center, float radius);
    }
}
