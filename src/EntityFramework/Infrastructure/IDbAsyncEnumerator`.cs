// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Asynchronous version of the <see cref="IEnumerator<>"/>  interface that allows elements to be retrieved asynchronously.
    ///     It is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    /// <typeparam name="T"> Element type </typeparam>
    public interface IDbAsyncEnumerator<out T> : IDbAsyncEnumerator
    {
        /// <summary>
        ///     Gets the current element in the iteration.
        /// </summary>
        new T Current { get; }
    }
}

#endif
