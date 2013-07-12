// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using Linq;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;    

    /// <summary>
    /// Specifies the means to create concrete DbMappingViewCache instances.
    /// </summary>
    public abstract class DbMappingViewCacheFactory
    {
        /// <summary>
        /// Creates a generated view cache instance for the container mapping specified by
        /// the names of the mapped containers.
        /// </summary>
        /// <param name="conceptualModelContainerName">The name of a container in the conceptual model.</param>
        /// <param name="storeModelContainerName">The name of a container in the store model.</param>
        /// <returns>A DbMappingViewCache that specifies the generated view cache.</returns>
        public abstract DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName);

        /// <summary>
        /// Creates a concrete DbMappingViewCache corresponding to the specified container mapping.
        /// </summary>
        /// <param name="mapping">A mapping between a container in the conceptual model and a container in 
        /// the store model.</param>
        /// <returns>A concrete DbMappingViewCache, or null if a creator was not found.</returns>
        internal DbMappingViewCache Create(StorageEntityContainerMapping mapping)
        {
            return Create(mapping.EdmEntityContainer.Name, mapping.StorageEntityContainer.Name);
        }
    }
}
