// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Asynchronous version of the <see cref="System.Collections.Generic.IEnumerable{T}" /> interface that allows elements of the enumerable sequence to be retrieved asynchronously.
    /// This interface is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    /// <typeparam name="T"> The type of objects to enumerate. </typeparam>
    public interface IDbAsyncEnumerable<out T> : IDbAsyncEnumerable
    {
        /// <summary>
        /// Gets an enumerator that can be used to asynchronously enumerate the sequence.
        /// </summary>
        /// <returns> Enumerator for asynchronous enumeration over the sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        new IDbAsyncEnumerator<T> GetAsyncEnumerator();
    }
}

#endif
