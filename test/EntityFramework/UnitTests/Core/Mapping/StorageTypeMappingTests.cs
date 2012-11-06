// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageTypeMappingTests
    {
        [Fact]
        public void Can_add_remove_mapping_fragment()
        {
            var storageSetMapping
                = new StorageEntitySetMapping(
                    new EntitySet(),
                    new StorageEntityContainerMapping(new EntityContainer()));

            var storageTypeMapping
                = new StorageEntityTypeMapping(storageSetMapping);

            Assert.Empty(storageTypeMapping.MappingFragments);

            var mappingFragment = new StorageMappingFragment(new EntitySet(), storageTypeMapping, false);

            storageTypeMapping.AddFragment(mappingFragment);

            Assert.Same(mappingFragment, storageTypeMapping.MappingFragments.Single());

            storageTypeMapping.RemoveFragment(mappingFragment);

            Assert.Empty(storageTypeMapping.MappingFragments);
        }
    }
}
