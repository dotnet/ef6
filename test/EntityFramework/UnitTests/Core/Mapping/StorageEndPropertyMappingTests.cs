// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class StorageEndPropertyMappingTests
    {
        [Fact]
        public void Can_get_property_mappings()
        {
            var endPropertyMapping = new StorageEndPropertyMapping(new EdmProperty("P"));

            Assert.Empty(endPropertyMapping.PropertyMappings);

            var scalarPropertyMapping 
                = new StorageScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C"));

            endPropertyMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.PropertyMappings.Single());
        }
    }
}
