using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Services
{
    /// <summary>
    /// Simple uniform-grid spatial partitioning system.
    /// Not thread-safe. Intended for read-mostly usage each frame.
    /// </summary>
    public class GridSpacePartitioningSystem : ISpacePartitioningSystem
    {
        private readonly float cellSize;

        // Map from grid cell to entities within it
        private readonly Dictionary<Vector3Int, HashSet<IEntity>> grid = new();

        // Track last known cell per-entity for fast updates
        private readonly Dictionary<IEntity, Vector3Int> entityCells = new();

        public GridSpacePartitioningSystem(IEnumerable<IEntity> build, float cellSize = 5f)
        {
            this.cellSize = Mathf.Max(0.01f, cellSize);
            foreach (var _entity in build)
            {
                Add(_entity);
            }
        }

        public GridSpacePartitioningSystem(float cellSize = 5f)
        {
            this.cellSize = Mathf.Max(0.01f, cellSize);
        }

        public void Add(IEntity entity)
        {
            if (entity == null) return;
            var cell = WorldToCell(GetEntityPosition(entity));
            entityCells[entity] = cell;
            if (!grid.TryGetValue(cell, out var set))
            {
                set = new HashSet<IEntity>();
                grid[cell] = set;
            }

            set.Add(entity);
        }

        public void Remove(IEntity entity)
        {
            if (entity == null) return;
            if (entityCells.TryGetValue(entity, out var cell))
            {
                if (grid.TryGetValue(cell, out var set))
                {
                    set.Remove(entity);
                    if (set.Count == 0) grid.Remove(cell);
                }

                entityCells.Remove(entity);
            }
            else
            {
                // Fallback: attempt to remove from its current position's cell
                var c = WorldToCell(GetEntityPosition(entity));
                if (grid.TryGetValue(c, out var set))
                {
                    set.Remove(entity);
                    if (set.Count == 0) grid.Remove(c);
                }
            }
        }

        public void Update(IEntity entity)
        {
            if (entity == null) return;
            var newCell = WorldToCell(GetEntityPosition(entity));
            if (entityCells.TryGetValue(entity, out var oldCell) && oldCell == newCell)
                return; // no cell change

            // Remove from old cell
            if (entityCells.TryGetValue(entity, out oldCell))
            {
                if (grid.TryGetValue(oldCell, out var oldSet))
                {
                    oldSet.Remove(entity);
                    if (oldSet.Count == 0) grid.Remove(oldCell);
                }
            }

            // Add to new cell
            entityCells[entity] = newCell;
            if (!grid.TryGetValue(newCell, out var newSet))
            {
                newSet = new HashSet<IEntity>();
                grid[newCell] = newSet;
            }

            newSet.Add(entity);
        }

        public void Clear()
        {
            grid.Clear();
            entityCells.Clear();
        }

        public IEnumerable<IEntity> Query(Bounds bounds)
        {
            var min = WorldToCell(bounds.min);
            var max = WorldToCell(bounds.max);
            var results = new HashSet<IEntity>();
            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
            {
                var cell = new Vector3Int(x, y, z);
                if (grid.TryGetValue(cell, out var set))
                {
                    foreach (var e in set)
                    {
                        // Precise check using entity bounds overlap
                        if (e.Bounds.Intersects(bounds))
                            results.Add(e);
                    }
                }
            }

            return results;
        }

        public IEnumerable<IEntity> Query(Vector3 center, float radius)
        {
            var b = new Bounds(center, Vector3.one * (radius * 2f));
            var approx = Query(b);
            var r2 = radius * radius;
            return approx.Where(e => (GetEntityPosition(e) - center).sqrMagnitude <= r2);
        }

        private Vector3 GetEntityPosition(IEntity e)
        {
            var t = e.GetWorldRepresentation()?.transform;
            return t ? t.position : e.Bounds.center;
        }

        private Vector3Int WorldToCell(Vector3 world)
        {
            return new Vector3Int(
                Mathf.FloorToInt(world.x / cellSize),
                Mathf.FloorToInt(world.y / cellSize),
                Mathf.FloorToInt(world.z / cellSize)
            );
        }
    }
}