// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Base.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ReadOnlyCollection<T> : ICollection<T>
    {
        private readonly ICollection<T> _proxy;

        internal ReadOnlyCollection(ICollection<T> collection)
        {
            _proxy = collection;
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new InvalidOperationException("This is a read-only collection");
        }

        public void Clear()
        {
            throw new InvalidOperationException("This is a read-only collection");
        }

        public bool Contains(T item)
        {
            return _proxy.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _proxy.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _proxy.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException("This is a read-only collection");
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _proxy.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _proxy.GetEnumerator();
        }

        #endregion
    }
}
