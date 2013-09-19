// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    /// An interface implemented by <see cref="InternalQuery{TElement}" />.
    /// </summary>
    /// <typeparam name="TElement"> The type of the element. </typeparam>
    internal interface IInternalQuery<out TElement> : IInternalQuery
    {
        IInternalQuery<TElement> Include(string path);
        IInternalQuery<TElement> AsNoTracking();
        IInternalQuery<TElement> AsStreaming();

#if !NET40
        new IDbAsyncEnumerator<TElement> GetAsyncEnumerator();
#endif

        new IEnumerator<TElement> GetEnumerator();
    }
}
