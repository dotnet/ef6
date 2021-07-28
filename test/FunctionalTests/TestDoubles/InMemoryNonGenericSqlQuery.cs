// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;

    public class InMemoryNonGenericSqlQuery<T> : DbSqlQuery, IEnumerable
#if !NET40
                                                 , IDbAsyncEnumerable
#endif
        where T : class
    {
        private readonly IEnumerable<T> _data;

        public InMemoryNonGenericSqlQuery(IEnumerable<T> data)
        {
            _data = data;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

#if !NET40
        public IDbAsyncEnumerator GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<T>(_data.GetEnumerator());
        }
#endif

        public override string ToString()
        {
            return "An in-memory SqlQuery";
        }
    }
}
