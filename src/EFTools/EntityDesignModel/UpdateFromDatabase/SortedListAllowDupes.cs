// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Represents a Systems.Collections.Generic.List which is
    ///     always kept in sorted order and which allows duplicates.
    /// </summary>
    internal class SortedListAllowDupes<T> : ICollection<T>
    {
        private readonly List<T> _list;
        private readonly IComparer<T> _comparer;

        internal SortedListAllowDupes(IComparer<T> comparer)
        {
            _list = new List<T>();
            _comparer = comparer;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (0 == _list.Count)
            {
                _list.Add(item);
            }
            else
            {
                var index = _list.BinarySearch(item, _comparer);
                if (index < 0)
                {
                    // index < 0 implies index to insert is at ~index
                    _list.Insert(~index, item);
                }
                else
                {
                    // index >=0 implies a dupe of this item already exists
                    // make sure we are at the last one before either the end
                    // of the list or an item which is greater
                    var itemAtIndex = _list[index];
                    var indexToInsert = index + 1;
                    while (indexToInsert < _list.Count)
                    {
                        var compare = _comparer.Compare(itemAtIndex, _list[indexToInsert]);
                        if (0 == compare)
                        {
                            indexToInsert++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    _list.Insert(indexToInsert, item);
                }
            }
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        internal bool ContainsAll(ICollection<T> collectionOfItems)
        {
            foreach (var item in collectionOfItems)
            {
                if (false == Contains(item))
                {
                    // collectionOfItems contains at least 1 item which is not in _list
                    return false;
                }
            }

            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        #endregion

        internal static int CompareListContents(SortedListAllowDupes<T> a, SortedListAllowDupes<T> b)
        {
            var enumA = a.GetEnumerator();
            var enumB = b.GetEnumerator();

            var aHasNext = enumA.MoveNext();
            var bHasNext = enumB.MoveNext();

            while (aHasNext && bHasNext)
            {
                var compVal = a._comparer.Compare(enumA.Current, enumB.Current);
                if (compVal != 0)
                {
                    return compVal;
                }

                aHasNext = enumA.MoveNext();
                bHasNext = enumB.MoveNext();
            }

            if (aHasNext && !bHasNext)
            {
                return 1;
            }
            else if (!aHasNext && bHasNext)
            {
                return -1;
            }

            // they are equals
            return 0;
        }
    }
}
