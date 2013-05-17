// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;

    public class InMemorySqlQuery<T> : DbSqlQuery<T>
#if !NET40
                                       , IDbAsyncEnumerable<T>
#endif
        where T : class
    {
        private readonly IEnumerable<T> _data;

        public InMemorySqlQuery(IEnumerable<T> data)
        {
            _data = data;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

#if !NET40
        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
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
