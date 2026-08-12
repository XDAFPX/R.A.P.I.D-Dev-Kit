using DAFP.TOOLS.ECS; // for IEntity
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    /// <summary>
    /// Marker for filter types (stateless by design).
    /// </summary>
    public interface IFilter { }

    /// <summary>
    /// Context passed to filters during evaluation. Implementations may carry references like "self".
    /// </summary>
    public interface IFilterContext { }

    /// <summary>
    /// A context that carries no data.
    /// </summary>
    public readonly struct EmptyFilterContext : IFilterContext
    {
        public static readonly EmptyFilterContext Instance = new EmptyFilterContext();
    }

    /// <summary>
    /// Context carrying a reference to a GameObject considered as "self".
    /// </summary>
    public readonly struct GameObjectFilterContext : IFilterContext
    {
        public readonly GameObject Self;
        public GameObjectFilterContext(GameObject self) { Self = self; }
    }

    /// <summary>
    /// Context carrying a reference to an IEntity considered as "self".
    /// </summary>
    public readonly struct EntityFilterContext : IFilterContext
    {
        public readonly IEntity Self;
        public EntityFilterContext(IEntity self) { Self = self; }
        public GameObject SelfGO => Self?.GetWorldRepresentation();
    }

    /// <summary>
    /// Base interface for reusable, composable filters.
    /// </summary>
    public interface IFilter<in T> : IFilter
    {
        /// <summary>
        /// Returns true if the provided target passes the filter under the given context.
        /// </summary>
        bool Evaluate(T go, IFilterContext ctx);
    }
}