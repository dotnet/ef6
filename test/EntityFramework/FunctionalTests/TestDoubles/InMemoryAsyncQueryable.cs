// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;

    public class InMemoryAsyncQueryable<T> : IOrderedQueryable<T>
#if !NET40
        , IDbAsyncEnumerable<T>
#endif
    {
        private readonly IQueryable<T> _queryable;
        private readonly Action<string, IEnumerable> _include;

        public InMemoryAsyncQueryable(IQueryable<T> queryable, Action<string, IEnumerable> include = null)
        {
            _queryable = queryable;
            _include = include;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return _queryable.Expression; }
        }

        public Type ElementType
        {
            get { return _queryable.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return new InMemoryAsyncQueryProvider(_queryable.Provider, _include); }
        }

        public IQueryable<T> Include(string path)
        {
            if (_include != null)
            {
                _include(path, _queryable);
            }
            return this;
        }

#if !NET40
        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<T>(GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }
#endif
    }
}
