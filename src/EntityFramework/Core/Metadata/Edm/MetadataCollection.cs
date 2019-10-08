// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    // <summary>
    // Represents a collection of metadata objects.
    // </summary>
    // <typeparam name="T">The type of objects in the collection.</typeparam>
    internal class MetadataCollection<T> : IList<T>
        where T : MetadataItem
    {
        internal const int UseDictionaryCrossover = 8;

        private bool _readOnly;
        private List<T> _metadataList;
        private volatile Dictionary<string, T> _caseSensitiveDictionary;
        private volatile Dictionary<string, int> _caseInsensitiveDictionary;

        // <summary>
        // Creates an empty metadata collection.
        // </summary>
        internal MetadataCollection()
        {
            _metadataList = new List<T>();
        }

        // <summary>
        // Creates a metadata collection that contains the specified items. 
        // The items are copied into an internal list.
        // </summary>
        // <param name="items">An enumerable of items to be stored in the collection.</param>
        internal MetadataCollection(IEnumerable<T> items)
        {
            _metadataList = new List<T>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("items"));
                    }

                    AddInternal(item);
                }
            }
        }

        // <summary>
        // Creates a metadata collection that stores the specified list of items.
        // The list is wrapped into the collection as is. 
        // The explicit intention of wrapping the list must be expressed by calling 
        // the Wrap method. 
        // </summary>
        // <param name="items">A list of items to be stored in the collection.</param>
        private MetadataCollection(List<T> items)
        {
            DebugCheck.NotNull(items);
#if DEBUG
            foreach (var item in items)
            {
                Debug.Assert(item != null);
                Debug.Assert(!String.IsNullOrEmpty(item.Identity));
            }
#endif
            _metadataList = items;
        }

        // <summary>
        // Creates a metadata collection that wraps the specified list of items.
        // </summary>
        // <param name="items">A list of items to be stored in the collection.</param>
        internal static MetadataCollection<T> Wrap(List<T> items)
        {
            return new MetadataCollection<T>(items);
        }

        // <summary>
        // Gets the number of items in the collection.
        // </summary>
        public virtual int Count
        {
            get { return _metadataList.Count; }
        }

        // <summary>
        // Gets or sets the item at the specified index.
        // </summary>
        // <param name="index">The zero-based index of the item to get or set.</param>
        // <returns>The item at the specified index.</returns>
        // <exception cref="System.ArgumentOutOfRangeException">index is less than 0 or index is equal to 
        // or greater than Count.</exception>
        // <exception cref="System.InvalidOperationException">The collection is read only.</exception>
        public virtual T this[int index]
        {
            get { return _metadataList[index]; }

            set
            {
                ThrowIfReadOnly();
                DebugCheck.NotNull(value);
                Debug.Assert(!String.IsNullOrEmpty(value.Identity));

                // Update the list.
                var existingIdentity = _metadataList[index].Identity;
                _metadataList[index] = value;

                HandleIdentityChange(value, existingIdentity, validate: false);
            }
        }

        // <summary>
        // Method that must be called after the identity of an item in the collection has changed.
        // </summary>
        // <param name="item">The item whose identity has changed.</param>
        // <param name="initialIdentity">The initial identity of the item.</param>
        internal void HandleIdentityChange(T item, string initialIdentity)
        {
            HandleIdentityChange(item, initialIdentity, validate: true);
        }

        private void HandleIdentityChange(T item, string initialIdentity, bool validate)
        {
            DebugCheck.NotNull(item);
            DebugCheck.NotEmpty(initialIdentity);

            // Update the case sensitive dictionary.
            if (_caseSensitiveDictionary != null)
            {
                T existingItem;
                if (!validate 
                    || (_caseSensitiveDictionary.TryGetValue(initialIdentity, out existingItem) 
                        && ReferenceEquals(existingItem, item)))
                {
                    RemoveFromCaseSensitiveDictionary(initialIdentity);

                    var identity = item.Identity;
                    if (_caseSensitiveDictionary.ContainsKey(identity))
                    {
                        // Invalidate the case sensitive dictionary.
                        // The identities are rebuilt externally, uniquiness should be ensured by caller.
                        _caseSensitiveDictionary = null;
                    }
                    else
                    {
                        _caseSensitiveDictionary.Add(identity, item);
                    }
                }
            }

            // Invalidate the case insensitive dictionary.
            _caseInsensitiveDictionary = null;
        }

        // <summary>
        // Gets the item with the specified identity.
        // </summary>
        // <param name="identity">The identity of the item to find.</param>
        // <returns>The item with the specified identity.</returns>
        // <exception cref="System.ArgumentNullException">identity is null.</exception>
        // <exception cref="System.ArgumentException">An item with the specified identity was not found.</exception>
        // <exception cref="System.InvalidOperationException">Always thrown on setter.</exception>
        public virtual T this[string identity]
        {
            get { return GetValue(identity, false); }
            set { throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection); }
        }

        // <summary>
        // Gets the item with the specified identity.
        // </summary>
        // <param name="identity">The identity of the item to find.</param>
        // <param name="ignoreCase">A boolean that indicates whether to ignore the case of the strings being compared.</param>
        // <returns>The item with the specified identity.</returns>
        public virtual T GetValue(string identity, bool ignoreCase)
        {
            DebugCheck.NotEmpty(identity);

            T item;
            if (!TryGetValue(identity, ignoreCase, out item))
            {
                throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
            }

            return item;
        }

        // <summary>
        // Attempts to get the item with the specified identity.
        // </summary>
        // <param name="identity">The identity of the item to find.</param>
        // <param name="ignoreCase">A boolean that indicates whether to ignore the case of the strings being compared.</param>
        // <param name="item">The item with the specified identity, or null if not found.</param>
        // <returns>true if the item was found, false otherwise.</returns>
        public virtual bool TryGetValue(string identity, bool ignoreCase, out T item)
        {
            DebugCheck.NotEmpty(identity);

            return ignoreCase
                ? FindCaseInsensitive(identity, out item, false)
                : FindCaseSensitive(identity, out item);
        }

        // <summary>
        // Adds the specified item to the collection.
        // </summary>
        // <param name="item">The item to add.</param>
        // <exception cref="System.InvalidOperationException">The collection read only.</exception>
        // <exception cref="System.ArgumentException">An item with the same identity already exists.</exception>
        public virtual void Add(T item)
        {
            ThrowIfReadOnly();

            AddInternal(item);
        }

        // <summary>
        // Helper method to add the specified item to the collection.
        // </summary>
        // <param name="item">The item to add.</param>
        // <exception cref="System.InvalidOperationException">The collection is read only.</exception>
        // <exception cref="System.ArgumentException">An item with the same identity already exists.</exception>
        private void AddInternal(T item)
        {
            DebugCheck.NotNull(item);
            Debug.Assert(!String.IsNullOrEmpty(item.Identity));

            var identity = item.Identity;

            if (ContainsIdentityCaseSensitive(identity))
            {
                throw new ArgumentException(Strings.ItemDuplicateIdentity(identity), "item");
            }

            // Add to the list.
            _metadataList.Add(item);
            
            // Add to the case sensitive dictionary.
            if (_caseSensitiveDictionary != null)
            {
                _caseSensitiveDictionary.Add(identity, item);
            }

            // Invalidate the case insensitive dictionary.
            _caseInsensitiveDictionary = null;
        }

        // <summary>
        // Adds the specified items to the collection.
        // </summary>
        // <param name="items">The items to add to the collection.</param>
        // <exception cref="System.ArgumentException">An item to add is null.</exception>
        // <exception cref="System.ArgumentException">An item with the same identity already exists.</exception>
        // <returns>A boolean that indicates whether the operation was successful.</returns>
        internal void AddRange(IEnumerable<T> items)
        {
            Check.NotNull(items, "items");

            // Add the new items, this will also perform duplication check.
            foreach (var item in items)
            {
                if (item == null)
                {
                    throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("items"));
                }

                AddInternal(item);
            }
        }

        // <summary>
        // Removes the specified item from the collection.
        // </summary>
        // <param name="item">The item to be removed.</param>
        // <returns>true if the item was removed, false otherwise.</returns>
        internal bool Remove(T item)
        {
            ThrowIfReadOnly();
            DebugCheck.NotNull(item);

            // Remove from the list.
            if (!_metadataList.Remove(item))
            {
                return false;
            }

            // Remove from the case sensitive dictionary.
            if (_caseSensitiveDictionary != null)
            {
                RemoveFromCaseSensitiveDictionary(item.Identity);
            }

            // Invalidate the case insensitive dictionary.
            _caseInsensitiveDictionary = null;

            return true;
        }

        // <summary>
        // Returns the collection as ReadOnlyCollection<T>.
        // </summary>
        public virtual ReadOnlyCollection<T> AsReadOnly
        {
            get { return new ReadOnlyCollection<T>(_metadataList); }
        }

        // <summary>
        // Returns the collection as ReadOnlyMetadataCollection<T>.
        // </summary>
        public virtual ReadOnlyMetadataCollection<T> AsReadOnlyMetadataCollection()
        {
            return new ReadOnlyMetadataCollection<T>(this);
        }

        // <summary>
        // Gets a boolean indicating whether the collection is readonly.
        // </summary>
        public bool IsReadOnly
        {
            get { return _readOnly; }
        }

        // <summary>
        // Used in OneToOneMappingBuilder for the designer to workaround the circular 
        // dependency between EntityType and AssociationEndMember created when adding 
        // navigation properties. Must not be used in other context.
        // </summary>
        internal void ResetReadOnly()
        {
            _readOnly = false;
        }

        // <summary>
        // Makes the collection readonly.
        // </summary>
        public MetadataCollection<T> SetReadOnly()
        {
            for (var i = 0; i < _metadataList.Count; i++)
            {
                _metadataList[i].SetReadOnly();
            }

            _readOnly = true;

            _metadataList.TrimExcess();

            if (_metadataList.Count <= UseDictionaryCrossover)
            {
                _caseSensitiveDictionary = null;
                _caseInsensitiveDictionary = null;
            }

            return this;
        }

        // <summary>
        // Not supported, the collection is treated as read-only.
        // </summary>
        // <param name="index">The index where to insert the given item.</param>
        // <param name="item">The item to be inserted.</param>
        // <exception cref="System.InvalidOperationException">Thrown if the item passed in or the collection itself is in ReadOnly state.</exception>
        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
        }

        // <summary>
        // Not supported, the collection is treated as read-only.
        // </summary>
        // <param name="item">The item to be removed.</param>
        // <returns>true if the item is actually removed, false if the item is not in the list.</returns>
        // <exception cref="System.InvalidOperationException">Always thrown.</exception>
        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
        }

        // <summary>
        // Not supported, the collection is treated as read-only.
        // </summary>
        // <param name="index">The index at which the item is removed.</param>
        // <exception cref="System.InvalidOperationException">Always thrown.</exception>
        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
        }

        // <summary>
        // Not supported, the collection is treated as read-only.
        // </summary>
        // <exception cref="System.InvalidOperationException">Always thrown.</exception>
        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
        }

        // <summary>
        // Determines if this collection contains the given item.
        // </summary>
        // <param name="item">The item to check for.</param>
        // <returns> True if the collection contains the item </returns>
        // <exception cref="System.ArgumentNullException">Thrown if item argument passed in is null</exception>
        // <exception cref="System.ArgumentException">Thrown if the item passed in has null or String.Empty identity</exception>
        public bool Contains(T item)
        {
            DebugCheck.NotNull(item);

            T existingItem;
            return
                TryGetValue(item.Identity, false, out existingItem)
                && ReferenceEquals(existingItem, item);
        }

        // <summary>
        // Determines if this collection contains an item of the given identity
        // </summary>
        // <param name="identity"> The identity of the item to check for </param>
        // <returns> True if the collection contains the item with the given identity </returns>
        // <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        // <exception cref="System.ArgumentException">Thrown if identity argument passed in is empty string</exception>
        public virtual bool ContainsIdentity(string identity)
        {
            DebugCheck.NotEmpty(identity);

            return ContainsIdentityCaseSensitive(identity);
        }

        // <summary>
        // Find the index of an item
        // </summary>
        // <param name="item"> The item whose index is to be looked for </param>
        // <returns> The index of the found item, -1 if not found </returns>
        // <exception cref="System.ArgumentNullException">Thrown if item argument passed in is null</exception>
        // <exception cref="System.ArgumentException">Thrown if the item passed in has null or String.Empty identity</exception>
        public virtual int IndexOf(T item)
        {
            return _metadataList.IndexOf(item);
        }

        // <summary>
        // Copies the items in this collection to an array
        // </summary>
        // <param name="array"> The array to copy to </param>
        // <param name="arrayIndex"> The index in the array at which to start the copy </param>
        // <exception cref="System.ArgumentNullException">Thrown if array argument is null</exception>
        // <exception cref="System.ArgumentOutOfRangeException">Thrown if the arrayIndex is less than zero</exception>
        // <exception cref="System.ArgumentException">Thrown if the array argument passed in with respect to the arrayIndex passed in not big enough to hold the MetadataCollection being copied</exception>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            DebugCheck.NotNull(array);

            _metadataList.CopyTo(array, arrayIndex);
        }

        // <summary>
        // Gets an enumerator over this collection
        // </summary>
        public ReadOnlyMetadataCollection<T>.Enumerator GetEnumerator()
        {
            return new ReadOnlyMetadataCollection<T>.Enumerator(this);
        }

        // <summary>
        // Gets an enumerator over this collection.
        // </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        // <summary>
        // Gets an enumerator over this collection.
        // </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // <summary>
        // Invalidates the dictionaries.
        // </summary>
        internal void InvalidateCache()
        {
            _caseSensitiveDictionary = null;
            _caseInsensitiveDictionary = null;
        }

        #region Helper methods

        internal bool HasCaseSensitiveDictionary
        {
            get { return _caseSensitiveDictionary != null; }
        }

        internal bool HasCaseInsensitiveDictionary
        {
            get { return _caseInsensitiveDictionary != null; }
        }

        // <summary>
        // Gets the case sensitive dictionary.
        // </summary>
        // Internal for test purpose only, do not use outside this class.
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal Dictionary<string, T> GetCaseSensitiveDictionary()
        {
            if (_caseSensitiveDictionary == null
                && _metadataList.Count > UseDictionaryCrossover)
            {
                _caseSensitiveDictionary = CreateCaseSensitiveDictionary();
            }

            return _caseSensitiveDictionary;
        }

        // <summary>
        // Creates the case sensitive dictionary.
        // </summary>
        private Dictionary<string, T> CreateCaseSensitiveDictionary()
        {
            var caseSensitiveDictionary
                = new Dictionary<string, T>(_metadataList.Count, StringComparer.Ordinal);

            for (var i = 0; i < _metadataList.Count; i++)
            {
                var item = _metadataList[i];
                caseSensitiveDictionary.Add(item.Identity, item);
            }

            return caseSensitiveDictionary;
        }

        // <summary>
        // Gets the case insensitive dictionary.
        // </summary>
        // Internal for test purpose only, do not use outside this class.
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal Dictionary<string, int> GetCaseInsensitiveDictionary()
        {
            if (_caseInsensitiveDictionary == null
                && _metadataList.Count > UseDictionaryCrossover)
            {
                _caseInsensitiveDictionary = CreateCaseInsensitiveDictionary();
            }

            return _caseInsensitiveDictionary;
        }

        // <summary>
        // Creates the case insensitive dictionary.
        // </summary>
        private Dictionary<string, int> CreateCaseInsensitiveDictionary()
        {
            var caseInsensitiveDictionary
                = new Dictionary<string, int>(_metadataList.Count, StringComparer.OrdinalIgnoreCase) 
                    { { _metadataList[0].Identity, 0 } };

            for (var i = 1; i < _metadataList.Count; i++)
            {
                var identity = _metadataList[i].Identity;
                int index;

                if (!caseInsensitiveDictionary.TryGetValue(identity, out index))
                {
                    caseInsensitiveDictionary[identity] = i;
                }
                else if (index >= 0)
                {
                    caseInsensitiveDictionary[identity] = -1;
                }
            }

            return caseInsensitiveDictionary;
        }

        // <summary>
        // Determines if the collection contains an item with the specified identity.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <returns>true if found, false otherwise</returns>
        private bool ContainsIdentityCaseSensitive(string identity)
        {
            var caseSensitiveDictionary = GetCaseSensitiveDictionary();
            if (caseSensitiveDictionary != null)
            {
                return caseSensitiveDictionary.ContainsKey(identity);
            }

            return ListContainsIdentityCaseSensitive(identity);
        }

        // <summary>
        // Determines if the internal list contains an item with the specified identity.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <returns>true if found, false otherwise</returns>
        private bool ListContainsIdentityCaseSensitive(string identity)
        {
            for (var i = 0; i < _metadataList.Count; i++)
            {
                if (_metadataList[i].Identity.Equals(identity, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        // <summary>
        // Attempts to find an item with the specified identity in the collection, performing case sensitive string comparisons.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <param name="item">The item with the specified identity or null if not found.</param>
        // <returns>true if found, false otherwise</returns>
        private bool FindCaseSensitive(string identity, out T item)
        {
            var caseSensitiveDictionary = GetCaseSensitiveDictionary();
            if (caseSensitiveDictionary != null)
            {
                if (caseSensitiveDictionary.TryGetValue(identity, out item))
                {
                    return true;
                }

                return false;
            }

            return ListFindCaseSensitive(identity, out item);
        }

        // <summary>
        // Attempts to find an item with the specified identity in the internal list, performing case sensitive string comparisons.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <param name="item">The item with the specified identity or null if not found.</param>
        // <returns>true if found, false otherwise</returns>
        private bool ListFindCaseSensitive(string identity, out T item)
        {
            for (var i = 0; i < _metadataList.Count; i++)
            {
                var it = _metadataList[i];

                if (it.Identity.Equals(identity, StringComparison.Ordinal))
                {
                    item = it;
                    return true;
                }
            }

            item = null;
            return false;
        }

        // <summary>
        // Attempts to find an item with the specified identity in the collection, performing case insensitive string comparisons.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <param name="item">The item with the specified identity or null if not found.</param>
        // <param name="throwOnMultipleMatches">Boolean that indicates whether to throw exception if multiple matches are found.</param>
        // <returns>true if found, false otherwise</returns>
        private bool FindCaseInsensitive(string identity, out T item, bool throwOnMultipleMatches)
        {
            var caseInsensitiveDictionary = GetCaseInsensitiveDictionary();
            if (caseInsensitiveDictionary != null)
            {
                int index;
                if (caseInsensitiveDictionary.TryGetValue(identity, out index))
                {
                    if (index >= 0)
                    {
                        item = _metadataList[index];
                        return true;
                    }

                    if (throwOnMultipleMatches)
                    {
                        throw new InvalidOperationException(Strings.MoreThanOneItemMatchesIdentity(identity));
                    }
                }

                item = null;
                return false;
            }

            return ListFindCaseInsensitive(identity, out item, throwOnMultipleMatches);
        }

        // <summary>
        // Attempts to find an item with the specified identity in the internal list, performing case insensitive string comparisons.
        // </summary>
        // <param name="identity">The identity to find.</param>
        // <param name="item">The item with the specified identity or null if not found.</param>
        // <param name="throwOnMultipleMatches">Boolean that indicates whether to throw exception if multiple matches are found.</param>
        // <returns>true if found, false otherwise</returns>
        private bool ListFindCaseInsensitive(string identity, out T item, bool throwOnMultipleMatches)
        {
            var found = false;
            item = null;

            for (var i = 0; i < _metadataList.Count; i++)
            {
                var it = _metadataList[i];

                if (it.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase))
                {
                    if (found)
                    {
                        if (throwOnMultipleMatches)
                        {
                            throw new InvalidOperationException(Strings.MoreThanOneItemMatchesIdentity(identity));
                        }

                        item = null;
                        return false;
                    }

                    found = true;
                    item = it;
                }
            }

            return found;
        }

        // <summary>
        // Removes the item with the specified identity from the case sensitive dictionary.
        // </summary>
        // <param name="identity">The identity of the item to be removed.</param>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void RemoveFromCaseSensitiveDictionary(string identity)
        {
            Debug.Assert(_caseSensitiveDictionary != null);

            if (!_caseSensitiveDictionary.Remove(identity))
            {
                Debug.Fail("The list and the case sensitive dictionary are out of sync.");
            }
        }

        // <summary>
        // Throws InvalidOperationException if the collection is readonly.
        // </summary>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.OperationOnReadOnlyCollection);
            }
        }

        #endregion
    }
}
