// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Utilities;

    // <summary>
    // Provides a default DbMappingViewCacheFactory implementation that uses the cache type
    // specified by a DbMappingViewCacheTypeAttribute to create a concrete DbMappingViewCache.
    // The implementation assumes that the model has a single container mapping.
    // </summary>
    internal class DefaultDbMappingViewCacheFactory : DbMappingViewCacheFactory
    {
        private readonly DbMappingViewCacheTypeAttribute _attribute;

        // <summary>
        // Creates a new DefaultDbMappingViewCacheFactory instance.
        // </summary>
        // <param name="attribute">
        // A DbMappingViewCacheTypeAttribute
        // that specifies the cache type.
        // </param>
        public DefaultDbMappingViewCacheFactory(DbMappingViewCacheTypeAttribute attribute)
        {
            DebugCheck.NotNull(attribute);

            _attribute = attribute;
        }

        // <summary>
        // Creates a generated view cache instance for the single container mapping in the model
        // by instantiating the type specified by the associated DbMappingViewCacheTypeAttribute.
        // </summary>
        // <param name="conceptualModelContainerName">The name of a container in the conceptual model.</param>
        // <param name="storeModelContainerName">The name of a container in the store model.</param>
        // <returns>A DbMappingViewCache that specifies the generated view cache.</returns>
        public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
        {
            return (DbMappingViewCache)Activator.CreateInstance(_attribute.CacheType);
        }

        // <summary>
        // Specifies a hash function for the current type. Two different instances associated
        // with the same DbMappingViewCacheTypeAttribute have the same hash code.
        // </summary>
        // <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return (_attribute.GetHashCode() * 397) ^ typeof(DefaultDbMappingViewCacheFactory).GetHashCode();
        }

        // <summary>
        // Determines whether the specified object is equal to the current object.
        // </summary>
        // <param name="obj">An object to compare with the current object.</param>
        // <returns>
        // true if the specified object is an instance of DefaultDbMappingViewCacheFactory
        // and the associated DbMappingViewCacheTypeAttribute is the same, false otherwise.
        // </returns>
        public override bool Equals(object obj)
        {
            var factory = obj as DefaultDbMappingViewCacheFactory;
            return factory != null && ReferenceEquals(factory._attribute, _attribute);
        }
    }
}
