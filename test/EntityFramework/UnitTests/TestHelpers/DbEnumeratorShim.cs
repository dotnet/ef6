// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Threading;
    using System.Threading.Tasks;

    public class DbEnumeratorShim<TElement> : IDbEnumerator<TElement>
    {
        private readonly IEnumerator<TElement> _enumerator;

        public DbEnumeratorShim(IEnumerator<TElement> enumerator)
        {
            _enumerator = enumerator;
        }

        public TElement Current
        {
            get { return _enumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return _enumerator.Current; }
        }

        object Infrastructure.IDbAsyncEnumerator.Current
        {
            get { return _enumerator.Current; }
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_enumerator.MoveNext());
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
