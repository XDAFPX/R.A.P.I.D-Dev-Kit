using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RapidLib.DAFP.TOOLS.Common
{
    public class DelegateSet<T> : ISet<T>
    {
        private readonly Func<IEnumerable<T>> getElements;
        private readonly Func<T, bool> add;
        private readonly Func<T, bool> remove;
        private readonly Func<T, bool> contains;
        private readonly Action clear;

        public DelegateSet(
            Func<IEnumerable<T>> getElements,
            Func<T, bool> add,
            Func<T, bool> remove,
            Func<T, bool>? contains = null,
            Action? clear = null)
        {
            this.getElements = getElements ?? throw new ArgumentNullException(nameof(getElements));
            this.add = add ?? throw new ArgumentNullException(nameof(add));
            this.remove = remove ?? throw new ArgumentNullException(nameof(remove));
            this.contains = contains ?? (item => this.getElements().Contains(item));
            this.clear = clear ?? (() =>
            {
                foreach (var _item in this.getElements().ToList()) this.remove(_item);
            });
        }

        public bool Add(T item) => add(item);
        public bool Remove(T item) => remove(item);
        public bool Contains(T item) => contains(item);
        public void Clear() => clear();

        public int Count => getElements().Count();
        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator() => getElements().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item) => Add(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            getElements().ToList().CopyTo(array, arrayIndex);

        // ISet-specific methods
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var _item in other.ToList())
                Remove(_item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            var _otherSet = other.ToHashSet();
            foreach (var _item in getElements().ToList())
            {
                if (!_otherSet.Contains(_item))
                    Remove(_item);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) =>
            getElements().ToHashSet().IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) =>
            getElements().ToHashSet().IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) =>
            getElements().ToHashSet().IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) =>
            getElements().ToHashSet().IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) =>
            getElements().ToHashSet().Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) =>
            getElements().ToHashSet().SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var _otherSet = other.ToHashSet();
            var _current = getElements().ToHashSet();

            foreach (var _item in _otherSet)
            {
                if (_current.Contains(_item))
                    Remove(_item);
                else
                    Add(_item);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var _item in other)
                Add(_item);
        }
    }
}