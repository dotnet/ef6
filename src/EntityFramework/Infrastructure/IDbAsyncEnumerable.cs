// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Asynchronous version of the <see cref="IEnumerable" /> interface that allows elements to be retrieved asynchronously.
    /// This interface is used to interact with Entity Framework queries and shouldn't be implemented by custom classes.
    /// </summary>
    public interface IDbAsyncEnumerable
    {
        /// <summary>
        /// Gets an enumerator that can be used to asynchronously enumerate the sequence.
        /// </summary>
        /// <returns> Enumerator for asynchronous enumeration over the sequence. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IDbAsyncEnumerator GetAsyncEnumerator();
    }
}

#endif
