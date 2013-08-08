// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Asynchronous version of the <see cref="System.Collections.Generic.IEnumerator{T}" />  interface that allows elements to be retrieved asynchronously.
    /// This interface is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    /// <typeparam name="T"> The type of objects to enumerate. </typeparam>
    public interface IDbAsyncEnumerator<out T> : IDbAsyncEnumerator
    {
        /// <summary>
        /// Gets the current element in the iteration.
        /// </summary>
        new T Current { get; }
    }
}

#endif
