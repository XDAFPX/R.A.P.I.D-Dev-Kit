using System;
using System.Collections;
using System.Collections.Generic;

namespace DAFP.TOOLS.Common.Utill
{
    public class NonEmptyList<T> : IList<T> where T : notnull
    {
        // Step 2: backing store
        private readonly List<T> _items;

        // Step 3: constructor with params
        public NonEmptyList(params T[] initialItems)
        {
            if (initialItems == null || initialItems.Length < 1)
                throw new ArgumentException("At least one element is required.", nameof(initialItems));

            foreach (var item in initialItems)
                if (item == null)
                    throw new ArgumentNullException(nameof(initialItems), "Null elements are not allowed.");

            _items = new List<T>(initialItems);
        }

        // Step 3: constructor with IEnumerable<T>
        public NonEmptyList(IEnumerable<T> initialItems)
        {
            if (initialItems == null)
                throw new ArgumentNullException(nameof(initialItems));

            _items = new List<T>(initialItems);

            if (_items.Count < 1)
                throw new ArgumentException("At least one element is required.", nameof(initialItems));

            if (_items.Exists(item => item == null!))
                throw new ArgumentException("Null elements are not allowed.", nameof(initialItems));
        }

        // IList<T> Implementation (Step 4 & 5)
        public T this[int index]
        {
            get => _items[index];
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _items[index] = value;
            }
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item)); // Step 5
            _items.Add(item);
        }

        public void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item)); // Step 5
            _items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (_items.Count <= 1)
                throw new InvalidOperationException("Cannot remove the last element."); // Step 5
            return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (_items.Count <= 1)
                throw new InvalidOperationException("Cannot remove the last element."); // Step 5
            _items.RemoveAt(index);
        }

        public void Clear()
        {
            throw new InvalidOperationException("Cannot clear all elements; at least one must remain."); // Step 5
        }

        public bool Contains(T item) => _items.Contains(item);
        public int IndexOf(T item) => _items.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}