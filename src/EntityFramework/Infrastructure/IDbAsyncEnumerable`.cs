// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Asynchronous version of the <see cref="IEnumerable<>"/> interface that allows elements of the enumerable sequence to be retrieved asynchronously.
    /// </summary>
    /// <typeparam name="T"> Element type </typeparam>
    public interface IDbAsyncEnumerable<out T> : IDbAsyncEnumerable
    {
        /// <summary>
        ///     Gets an enumerator that can be used to asynchronously enumerate the sequence.
        /// </summary>
        /// <returns> Enumerator for asynchronous enumeration over the sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        new IDbAsyncEnumerator<T> GetAsyncEnumerator();
    }
}
