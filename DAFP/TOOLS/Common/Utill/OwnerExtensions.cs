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

            var _current = ownedBy.GetCurrentOwner();
            if (_current == null) return null;

            // climb while the current owner is also an ownable of the same owner type
            var _guard = 0; // minimal cycle guard for pathological cases
            while (_current is IOwnedBy<TOwner> _up && _up.GetCurrentOwner() != null)
            {
                var _next = _up.GetCurrentOwner();
                if (ReferenceEquals(_next, _current)) break; // self-cycle defense
                _current = _next;
                if (++_guard > 1_000_000) break; // hard guard to avoid infinite loops in corrupted graphs
            }

            return _current;
        }

        /// <summary>
        /// Enumerates the chain of owners from the immediate owner up to the root owner.
        /// </summary>
        public static IEnumerable<TOwner> EnumerateOwnersUp<TOwner>(this IOwnedBy<TOwner> ownedBy)
            where TOwner : class, IOwnerOf<TOwner>
        {
            if (ownedBy == null) yield break;
            var _current = ownedBy.GetCurrentOwner();
            while (_current != null)
            {
                yield return _current;
                if (_current is IOwnedBy<TOwner> _up)
                {
                    var _next = _up.GetCurrentOwner();
                    if (ReferenceEquals(_next, _current)) yield break; // self-cycle defense
                    _current = _next;
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
            foreach (var _owner in EnumerateOwnersUp(ownedBy))
            {
                if (predicate(_owner)) return _owner;
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
            foreach (var _owner in EnumerateOwnersUp(ownedBy))
            {
                if (_owner is TTarget _t) return _t;
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
            if (ownerOf is IOwnedBy<TOwner> _ownable)
                return _ownable.GetRootOwner();
            return ownerOf as TOwner; // best-effort cast; typical implementation matches TOwner
        }

        public static IEnumerable<object> AllPets<T>(this T owner) where T : IOwnerBase
        {
            foreach (var _pet in owner.AbsolutePets)
            {
                yield return _pet;

                if (_pet is IOwnerBase _nestedOwner)
                {
                    foreach (var _nestedPet in _nestedOwner.AllPets())
                    {
                        yield return _nestedPet;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all pets in the subtree (DFS), starting from immediate pets.
        /// Includes nested pets for pets that are also owners.
        /// </summary>
        public static IEnumerable<IOwnedBy<TOwner>> EnumeratePetsDeep<TOwner>(this IOwnerOf<IOwnedBy<TOwner>> ownerOf)
            where TOwner : class
        {
            if (ownerOf == null) yield break;

            var _stack = new Stack<IOwnerOf<IOwnedBy<TOwner>>>();
            var _visitedOwners = new HashSet<object>();

            void push_owner(IOwnerOf<IOwnedBy<TOwner>> o)
            {
                if (o == null) return;
                if (_visitedOwners.Add(o)) _stack.Push(o);
            }

            push_owner(ownerOf);

            while (_stack.Count > 0)
            {
                var _currentOwner = _stack.Pop();
                var _pets = _currentOwner?.Pets;
                if (_pets == null) continue;

                foreach (var _pet in _pets)
                {
                    if (_pet == null) continue;
                    yield return _pet;

                    // If the pet is itself an owner of this pet type, explore recursively
                    if (_pet is IOwnerOf<IOwnedBy<TOwner>> _petAsOwner)
                    {
                        push_owner(_petAsOwner);
                    }
                }
            }
        }

        public static IEnumerable<TPet> EnumeratePetsDeep<TOwner, TPet>(this IOwnerOf<IOwnedBy<TOwner>> ownerOf)
            where TOwner : class
            where TPet : class, IOwnedBy<TOwner>
        {
            foreach (var _pet in ownerOf.EnumeratePetsDeep()) // calls the DFS for IOwnable<TOwner>
            {
                if (_pet is TPet _t)
                    yield return _t;
            }
        }
    }
}