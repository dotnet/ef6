// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    public static class EnumeratorExtensions
    {
        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            Debug.Assert(enumerator != null);

            var resultList = new List<T>();

            while (enumerator.MoveNext())
            {
                resultList.Add(enumerator.Current);
            }

            return resultList;
        }

#if !NET40

        public static Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerator<T> enumerator)
        {
            return enumerator.ToListAsync(CancellationToken.None);
        }

        public static async Task<List<T>> ToListAsync<T>(this IDbAsyncEnumerator<T> enumerator, CancellationToken cancellationToken)
        {
            Debug.Assert(enumerator != null);

            var resultList = new List<T>();

            while (await enumerator.MoveNextAsync(cancellationToken))
            {
                resultList.Add(enumerator.Current);
            }

            return resultList;
        }

#endif
    }
}
