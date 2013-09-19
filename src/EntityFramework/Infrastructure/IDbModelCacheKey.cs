// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Represents a key value that uniquely identifies an Entity Framework model that has been loaded into memory.
    /// </summary>
    public interface IDbModelCacheKey
    {
        /// <summary>Determines whether the current cached model key is equal to the specified cached model key.</summary>
        /// <returns>true if the current cached model key is equal to the specified cached model key; otherwise, false.</returns>
        /// <param name="other">The cached model key to compare to the current cached model key. </param>
        bool Equals(object other);

        /// <summary>Returns the hash function for this cached model key.</summary>
        /// <returns>The hash function for this cached model key.</returns>
        int GetHashCode();
    }
}
