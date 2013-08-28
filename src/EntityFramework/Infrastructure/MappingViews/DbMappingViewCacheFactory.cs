// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Core.Mapping;

    /// <summary>
    /// Specifies the means to create concrete <see cref="DbMappingViewCache" /> instances.
    /// </summary>
    public abstract class DbMappingViewCacheFactory
    {
        /// <summary>
        /// Creates a generated view cache instance for the container mapping specified by
        /// the names of the mapped containers.
        /// </summary>
        /// <param name="conceptualModelContainerName">The name of a container in the conceptual model.</param>
        /// <param name="storeModelContainerName">The name of a container in the store model.</param>
        /// <returns>
        /// A <see cref="DbMappingViewCache" /> that specifies the generated view cache.
        /// </returns>
        public abstract DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName);

        /// <summary>
        /// Creates a concrete <see cref="DbMappingViewCache" /> corresponding to the specified container mapping.
        /// </summary>
        /// <param name="mapping">
        /// A mapping between a container in the conceptual model and a container in
        /// the store model.
        /// </param>
        /// <returns>
        /// A concrete <see cref="DbMappingViewCache" />, or null if a creator was not found.
        /// </returns>
        internal DbMappingViewCache Create(EntityContainerMapping mapping)
        {
            return Create(mapping.EdmEntityContainer.Name, mapping.StorageEntityContainer.Name);
        }
    }
}
