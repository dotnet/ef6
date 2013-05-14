// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Resources;

    /// <summary>
    ///     An implementation of <see cref="ISet{T}" /> that wraps an existing set but makes
    ///     it read-only.
    /// </summary>
    internal class ReadOnlySet<T> : ISet<T>
    {
        #region Constructors and fields

        private readonly ISet<T> _set;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlySet{T}" /> class wrapped around
        ///     another existing set.
        /// </summary>
        /// <param name="set"> The existing set. </param>
        public ReadOnlySet(ISet<T> set)
        {
            _set = set;
        }

        #endregion

        #region ISet<> implementation

        public bool Add(T item)
        {
            throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _set.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _set.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _set.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _set.UnionWith(other);
        }

        void ICollection<T>.Add(T item)
        {
            throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
        }

        public void Clear()
        {
            throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _set.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw Error.DbPropertyValues_PropertyValueNamesAreReadonly();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_set).GetEnumerator();
        }

        #endregion
    }
}
