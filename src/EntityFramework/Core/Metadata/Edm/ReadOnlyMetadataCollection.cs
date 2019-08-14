// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a read-only wrapper around MetadataCollection
    /// </summary>
    /// <typeparam name="T"> The type of items in this collection </typeparam>
    public class ReadOnlyMetadataCollection<T> : ReadOnlyCollection<T>
        where T : MetadataItem
    {
        internal ReadOnlyMetadataCollection()
            : base(new MetadataCollection<T>())
        {
        }

        internal ReadOnlyMetadataCollection(MetadataCollection<T> collection)
            : base(collection)
        {
        }

        internal ReadOnlyMetadataCollection(List<T> list)
            : base(MetadataCollection<T>.Wrap(list))
        {
        }

        // On the surface, this Enumerator doesn't do anything but delegating to the underlying enumerator

        /// <summary>
        /// The enumerator for MetadataCollection
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
        public struct Enumerator : IEnumerator<T>
        {
            // <summary>
            // Constructor for the enumerator
            // </summary>
            // <param name="collection"> The collection that this enumerator should enumerate on </param>
            internal Enumerator(IList<T> collection)
            {
                _parent = collection;
                _nextIndex = 0;
                _current = null;
            }

            private int _nextIndex;
            private readonly IList<T> _parent;
            private T _current;

            /// <summary>Gets the member at the current position. </summary>
            /// <returns>The member at the current position.</returns>
            public T Current
            {
                get { return _current; }
            }

            /// <summary>
            /// Gets the member at the current position
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>Disposes of this enumerator.</summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Moves to the next member in the collection of type
            /// <see
            ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1.Enumerator" />
            /// .
            /// </summary>
            /// <returns>
            /// true if the enumerator is moved in the collection of type
            /// <see
            ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1.EnumeratorCollection" />
            /// ; otherwise, false.
            /// </returns>
            public bool MoveNext()
            {
                if ((uint)_nextIndex
                    < (uint)_parent.Count)
                {
                    _current = _parent[_nextIndex];
                    _nextIndex++;
                    return true;
                }

                _current = null;
                return false;
            }

            /// <summary>
            /// Positions the enumerator before the first position in the collection of type
            /// <see
            ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" />
            /// .
            /// </summary>
            public void Reset()
            {
                _current = null;
                _nextIndex = 0;
            }
        }

        /// <summary>Gets a value indicating whether this collection is read-only.</summary>
        /// <returns>true if this collection is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>Gets an item from this collection by using the specified identity.</summary>
        /// <returns>An item from this collection.</returns>
        /// <param name="identity">The identity of the item to be searched for.</param>
        public virtual T this[string identity]
        {
            get { return (((MetadataCollection<T>)Items)[identity]); }
        }

        // <summary>
        // Returns the metadata collection over which this collection is the view
        // </summary>
        internal MetadataCollection<T> Source
        {
            get
            {
                // PERF: this code written this way since it's part of a hotpath, consider its performance when refactoring. See codeplex #2298.
                try
                {
                    return (MetadataCollection<T>)Items;
                }
                finally
                {
                    // local variable is used to avoid concurrency problems
                    var sae = SourceAccessed;
                    if (sae != null)
                    {
                        sae(this, null);
                    }
                }
            }
        }

        internal event EventHandler SourceAccessed;

        /// <summary>Retrieves an item from this collection by using the specified identity.</summary>
        /// <returns>An item from this collection.</returns>
        /// <param name="identity">The identity of the item to be searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false. </param>
        public virtual T GetValue(string identity, bool ignoreCase)
        {
            return ((MetadataCollection<T>)Items).GetValue(identity, ignoreCase);
        }

        /// <summary>Determines whether the collection contains an item with the specified identity.</summary>
        /// <returns>true if the collection contains the item to be searched for; otherwise, false. The default is false.</returns>
        /// <param name="identity">The identity of the item.</param>
        public virtual bool Contains(string identity)
        {
            return ((MetadataCollection<T>)Items).ContainsIdentity(identity);
        }

        /// <summary>Retrieves an item from this collection by using the specified identity.</summary>
        /// <returns>true if there is an item that matches the search criteria; otherwise, false. </returns>
        /// <param name="identity">The identity of the item to be searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false. </param>
        /// <param name="item">When this method returns, this output parameter contains an item from the collection. If there is no matched item, this output parameter contains null.</param>
        public virtual bool TryGetValue(string identity, bool ignoreCase, out T item)
        {
            return ((MetadataCollection<T>)Items).TryGetValue(identity, ignoreCase, out item);
        }

        /// <summary>Returns an enumerator that can iterate through this collection.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1.Enumerator" /> that can be used to iterate through this
        /// <see
        ///     cref="T:System.Data.Metadata.Edm.ReadOnlyMetadataCollection" />
        /// .
        /// </returns>
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(Items);
        }

        /// <summary>Returns the index of the specified value in this collection.</summary>
        /// <returns>The index of the specified value in this collection.</returns>
        /// <param name="value">A value to seek.</param>
        public new virtual int IndexOf(T value)
        {
            return base.IndexOf(value);
        }
    }
}
