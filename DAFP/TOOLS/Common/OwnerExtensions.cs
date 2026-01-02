using System;
using System.Collections.Generic;
using System.Linq;

namespace DAFP.TOOLS.Common
{
    /// <summary>
    /// Tree helper extensions for IOwnable/IOwner chains.
    /// </summary>
    public static class OwnerExtensions
    {
        // ===================== IOwnable helpers (walk up) =====================

        /// <summary>
        /// Returns the top-most owner in the chain for this ownable.
        /// If there is no owner, returns null.
        /// </summary>
        public static TOwner GetRootOwner<TOwner>(this IOwnable<TOwner> ownable)
            where TOwner : class, IOwner<TOwner>
        {
            if (ownable == null) return null;

            var current = ownable.GetCurrentOwner();
            if (current == null) return null;

            // climb while the current owner is also an ownable of the same owner type
            var guard = 0; // minimal cycle guard for pathological cases
            while (current is IOwnable<TOwner> up && up.GetCurrentOwner() != null)
            {
                var next = up.GetCurrentOwner();
                if (ReferenceEquals(next, current)) break; // self-cycle defense
                current = next;
                if (++guard > 1_000_000) break; // hard guard to avoid infinite loops in corrupted graphs
            }
            return current;
        }

        /// <summary>
        /// Enumerates the chain of owners from the immediate owner up to the root owner.
        /// </summary>
        public static IEnumerable<TOwner> EnumerateOwnersUp<TOwner>(this IOwnable<TOwner> ownable)
            where TOwner : class, IOwner<TOwner>
        {
            if (ownable == null) yield break;
            var current = ownable.GetCurrentOwner();
            while (current != null)
            {
                yield return current;
                if (current is IOwnable<TOwner> up)
                {
                    var next = up.GetCurrentOwner();
                    if (ReferenceEquals(next, current)) yield break; // self-cycle defense
                    current = next;
                }
                else
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Finds the first owner up the chain that matches the predicate. Returns null if not found.
        /// </summary>
        public static TOwner FindOwnerUp<TOwner>(this IOwnable<TOwner> ownable, Func<TOwner, bool> predicate)
            where TOwner : class, IOwner<TOwner>
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            foreach (var owner in EnumerateOwnersUp(ownable))
            {
                if (predicate(owner)) return owner;
            }
            return null;
        }

        /// <summary>
        /// Finds the first owner up the chain of a specific type.
        /// </summary>
        public static TTarget FindOwnerUpOfType<TOwner, TTarget>(this IOwnable<TOwner> ownable)
            where TOwner : class, IOwner<TOwner>
            where TTarget : class, TOwner
        {
            foreach (var owner in EnumerateOwnersUp(ownable))
            {
                if (owner is TTarget t) return t;
            }
            return null;
        }

        // ===================== IOwner helpers (walk down) =====================

        /// <summary>
        /// If this owner also participates in an ownership chain as an ownable, returns the root owner of that chain;
        /// otherwise returns the owner itself (as TOwner).
        /// </summary>
        public static TOwner GetRootOwner<TOwner>(this IOwner<TOwner> owner)
            where TOwner : class, IOwner<TOwner>
        {
            if (owner == null) return null;
            if (owner is IOwnable<TOwner> ownable)
                return ownable.GetRootOwner();
            return owner as TOwner; // best-effort cast; typical implementation matches TOwner
        }

        /// <summary>
        /// Enumerates all pets in the subtree (DFS), starting from immediate pets.
        /// Includes nested pets for pets that are also owners.
        /// </summary>
        public static IEnumerable<IOwnable<TOwner>> EnumeratePetsDeep<TOwner>(this IOwner<TOwner> owner)
            where TOwner : class, IOwner<TOwner>
        {
            if (owner == null) yield break;

            var stack = new Stack<IOwner<TOwner>>();
            var visitedOwners = new HashSet<object>();

            void PushOwner(IOwner<TOwner> o)
            {
                if (o == null) return;
                if (visitedOwners.Add(o)) stack.Push(o);
            }

            PushOwner(owner);

            while (stack.Count > 0)
            {
                var currentOwner = stack.Pop();
                var pets = currentOwner?.Pets;
                if (pets == null) continue;

                foreach (var pet in pets)
                {
                    if (pet == null) continue;
                    yield return pet;
                    if (pet is IOwner<TOwner> petAsOwner)
                    {
                        PushOwner(petAsOwner);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the first pet in the subtree matching the predicate. DFS order.
        /// </summary>
        public static IOwnable<TOwner> FindPetDeep<TOwner>(this IOwner<TOwner> owner, Func<IOwnable<TOwner>, bool> predicate)
            where TOwner : class, IOwner<TOwner>
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            foreach (var pet in EnumeratePetsDeep(owner))
            {
                if (predicate(pet)) return pet;
            }
            return null;
        }

        /// <summary>
        /// Finds the first pet of the requested type in the subtree. Optional predicate to further filter.
        /// </summary>
        public static TPet FindPetDeep<TOwner, TPet>(this IOwner<TOwner> owner, Func<TPet, bool> predicate = null)
            where TOwner : class, IOwner<TOwner>
            where TPet : class, IOwnable<TOwner>
        {
            foreach (var pet in EnumeratePetsDeep(owner))
            {
                if (pet is TPet t && (predicate == null || predicate(t)))
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Checks if the specified target pet exists anywhere in the owner's subtree.
        /// </summary>
        public static bool HasPetInSubtree<TOwner>(this IOwner<TOwner> owner, IOwnable<TOwner> target)
            where TOwner : class, IOwner<TOwner>
        {
            if (owner == null || target == null) return false;
            foreach (var pet in EnumeratePetsDeep(owner))
            {
                if (ReferenceEquals(pet, target)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns all pets of type TPet in the subtree.
        /// </summary>
        public static IEnumerable<TPet> EnumeratePetsDeep<TOwner, TPet>(this IOwner<TOwner> owner)
            where TOwner : class, IOwner<TOwner>
            where TPet : class, IOwnable<TOwner>
        {
            foreach (var pet in EnumeratePetsDeep(owner))
            {
                if (pet is TPet t) yield return t;
            }
        }
    }
}
