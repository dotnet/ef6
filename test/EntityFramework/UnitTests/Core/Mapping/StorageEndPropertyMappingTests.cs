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
            var endPropertyMapping = new StorageEndPropertyMapping();

            Assert.Empty(endPropertyMapping.PropertyMappings);

            var scalarPropertyMapping 
                = new StorageScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C"));

            endPropertyMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.PropertyMappings.Single());
        }

        [Fact]
        public void Can_get_properties()
        {
            var endPropertyMapping = new StorageEndPropertyMapping();

            Assert.Empty(endPropertyMapping.Properties);

            var scalarPropertyMapping
                = new StorageScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C"));

            endPropertyMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.Properties.Single());            
        }

        [Fact]
        public void Can_set_and_get_end_member()
        {
            var endPropertyMapping = new StorageEndPropertyMapping();

            Assert.Null(endPropertyMapping.EndMember);

            var endMember = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            endPropertyMapping.EndMember = endMember;

            Assert.Same(endMember, endPropertyMapping.EndMember);
        }

    }
}
