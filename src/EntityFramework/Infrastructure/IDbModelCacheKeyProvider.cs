// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Implement this interface on your context to use custom logic to calculate the key used to lookup an already created model in the cache.
    /// This interface allows you to have a single context type that can be used with different models in the same AppDomain, 
    /// or multiple context types that use the same model.
    /// </summary>
    public interface IDbModelCacheKeyProvider
    {
        /// <summary>Gets the cached key associated with the provider.</summary>
        /// <returns>The cached key associated with the provider.</returns>
        string CacheKey { get; }
    }
}
