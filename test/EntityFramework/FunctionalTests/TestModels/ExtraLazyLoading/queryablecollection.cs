// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace LazyUnicorns
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    public class QueryableCollection<T> : ICollection<T>, IQueryable<T>, IHasIsLoaded
    {
        private readonly ICollection<T> _collection;
        private readonly IQueryable<T> _query;

        public QueryableCollection(ICollection<T> collection, IQueryable<T> query)
        {
            _collection = collection ?? new HashSet<T>();
            _query = query;
        }

        public IQueryable<T> Query
        {
            get { return _query; }
        }

        public bool IsLoaded { get; set; }

        public void Add(T item)
        {
            _collection.Add(item);
        }

        public void Clear()
        {
            LazyLoad();
            _collection.Clear();
        }

        public bool Contains(T item)
        {
            LazyLoad();
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            LazyLoad();
            _collection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return IsLoaded ? _collection.Count : _query.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _collection.IsReadOnly;
            }
        }

        public bool Remove(T item)
        {
            LazyLoad();
            return _collection.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            LazyLoad();
            return _collection.GetEnumerator();
        }

        public bool TestLoadInEnumerator { get; set; }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Calling lazy load here would previously cause EF to throw--see Dev11 205813
            // It no longer throws, but we still can't call Load here because DetectChanges uses
            // this method to determine if there are any entities in the collection, and so calling
            // load here will cause DetectChanges to lazily load the collection.
            // It's generally okay not to call load here because the normal case of enumerating
            // the collection is through the generic enumerator

            if (TestLoadInEnumerator)
            {
                LazyLoad();
            }
            return ((IEnumerable)_collection).GetEnumerator();
        }

        private void LazyLoad()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                _query.Load();
            }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }
    }
}