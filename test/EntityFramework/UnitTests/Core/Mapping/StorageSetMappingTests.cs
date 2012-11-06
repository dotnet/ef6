// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageSetMappingTests
    {
        [Fact]
        public void Can_add_remove_type_mapping()
        {
            var storageSetMapping 
                = new StorageEntitySetMapping(
                    new EntitySet(), 
                    new StorageEntityContainerMapping(new EntityContainer()));

            Assert.Empty(storageSetMapping.TypeMappings);

            var entityTypeMapping = new StorageEntityTypeMapping(storageSetMapping);

            storageSetMapping.AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, storageSetMapping.TypeMappings.Single());

            storageSetMapping.RemoveTypeMapping(entityTypeMapping);

            Assert.Empty(storageSetMapping.TypeMappings);
        } 
    }
}