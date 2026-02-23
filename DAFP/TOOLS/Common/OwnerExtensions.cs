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
        public static TOwner GetRootOwner<TOwner>(this IOwnedBy<TOwner> ownedBy)
            where TOwner : class, IOwnerOf<TOwner>
        {
            if (ownedBy == null) return null;

            var current = ownedBy.GetCurrentOwner();
            if (current == null) return null;

            // climb while the current owner is also an ownable of the same owner type
            var guard = 0; // minimal cycle guard for pathological cases
            while (current is IOwnedBy<TOwner> up && up.GetCurrentOwner() != null)
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
        public static IEnumerable<TOwner> EnumerateOwnersUp<TOwner>(this IOwnedBy<TOwner> ownedBy)
            where TOwner : class, IOwnerOf<TOwner>
        {
            if (ownedBy == null) yield break;
            var current = ownedBy.GetCurrentOwner();
            while (current != null)
            {
                yield return current;
                if (current is IOwnedBy<TOwner> up)
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
        public static TOwner FindOwnerUp<TOwner>(this IOwnedBy<TOwner> ownedBy, Func<TOwner, bool> predicate)
            where TOwner : class, IOwnerOf<TOwner>
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            foreach (var owner in EnumerateOwnersUp(ownedBy))
            {
                if (predicate(owner)) return owner;
            }

            return null;
        }

        /// <summary>
        /// Finds the first owner up the chain of a specific type.
        /// </summary>
        public static TTarget FindOwnerUpOfType<TOwner, TTarget>(this IOwnedBy<TOwner> ownedBy)
            where TOwner : class, IOwnerOf<TOwner>
            where TTarget : class, TOwner
        {
            foreach (var owner in EnumerateOwnersUp(ownedBy))
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
        public static TOwner GetRootOwner<TOwner>(this IOwnerOf<TOwner> ownerOf)
            where TOwner : class, IOwnerOf<TOwner>
        {
            if (ownerOf == null) return null;
            if (ownerOf is IOwnedBy<TOwner> ownable)
                return ownable.GetRootOwner();
            return ownerOf as TOwner; // best-effort cast; typical implementation matches TOwner
        }

        /// <summary>
        /// Enumerates all pets in the subtree (DFS), starting from immediate pets.
        /// Includes nested pets for pets that are also owners.
        /// </summary>
        public static IEnumerable<IOwnedBy<TOwner>> EnumeratePetsDeep<TOwner>(this IOwnerOf<IOwnedBy<TOwner>> ownerOf)
            where TOwner : class
        {
            if (ownerOf == null) yield break;

            var stack = new Stack<IOwnerOf<IOwnedBy<TOwner>>>();
            var visitedOwners = new HashSet<object>();

            void PushOwner(IOwnerOf<IOwnedBy<TOwner>> o)
            {
                if (o == null) return;
                if (visitedOwners.Add(o)) stack.Push(o);
            }

            PushOwner(ownerOf);

            while (stack.Count > 0)
            {
                var currentOwner = stack.Pop();
                var pets = currentOwner?.Pets;
                if (pets == null) continue;

                foreach (var pet in pets)
                {
                    if (pet == null) continue;
                    yield return pet;

                    // If the pet is itself an owner of this pet type, explore recursively
                    if (pet is IOwnerOf<IOwnedBy<TOwner>> petAsOwner)
                    {
                        PushOwner(petAsOwner);
                    }
                }
            }
        }

        public static IEnumerable<TPet> EnumeratePetsDeep<TOwner, TPet>(this IOwnerOf<IOwnedBy<TOwner>> ownerOf)
            where TOwner : class
            where TPet : class, IOwnedBy<TOwner>
        {
            foreach (var pet in ownerOf.EnumeratePetsDeep()) // calls the DFS for IOwnable<TOwner>
            {
                if (pet is TPet t)
                    yield return t;
            }
        }
    }
}