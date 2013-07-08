// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Interface implemented by objects that create an <see cref="IDbModelCacheKey"/> for a context.
    /// </summary>
    public interface IDbModelCacheKeyFactory
    {
        /// <summary>Creates a model cache key for a given context.</summary>
        /// <returns>A model cache key for a given context.</returns>
        /// <param name="context">The given context.</param>
        IDbModelCacheKey Create(DbContext context);
    }
}
