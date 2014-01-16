// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EndPropertyMappingTests
    {
        [Fact]
        public void Can_get_property_mappings()
        {
            var endPropertyMapping = new EndPropertyMapping();

            Assert.Empty(endPropertyMapping.PropertyMappings);

            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            endPropertyMapping.AddPropertyMapping(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.PropertyMappings.Single());
        }

        [Fact]
        public void Can_add_get_remove_properties()
        {
            var endPropertyMapping = new EndPropertyMapping();

            Assert.Empty(endPropertyMapping.PropertyMappings);

            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            endPropertyMapping.AddPropertyMapping(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.PropertyMappings.Single());

            endPropertyMapping.RemovePropertyMapping(scalarPropertyMapping);

            Assert.Empty(endPropertyMapping.PropertyMappings);
        }

        [Fact]
        public void Can_set_and_get_end_member()
        {
            var endPropertyMapping = new EndPropertyMapping();

            Assert.Null(endPropertyMapping.AssociationEnd);

            var endMember = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            endPropertyMapping.AssociationEnd = endMember;

            Assert.Same(endMember, endPropertyMapping.AssociationEnd);
        }

        [Fact]
        public void Can_create_mapping_and_get_association_end()
        {
            var associationEnd = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            var mapping = new EndPropertyMapping(associationEnd);

            Assert.Same(associationEnd, mapping.AssociationEnd);
        }

        [Fact]
        public void Cannot_add_property_when_read_only()
        {
            var associationEnd = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            var mapping = new EndPropertyMapping(associationEnd);
            mapping.SetReadOnly();

            Assert.True(mapping.IsReadOnly);

            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mapping.AddPropertyMapping(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_read_only()
        {
            var associationEnd = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            var mapping = new EndPropertyMapping(associationEnd);
            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));
            mapping.AddPropertyMapping(scalarPropertyMapping);
            mapping.SetReadOnly();

            Assert.True(mapping.IsReadOnly);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mapping.RemovePropertyMapping(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var associationEnd = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            var mapping = new EndPropertyMapping(associationEnd);
            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));
            mapping.AddPropertyMapping(scalarPropertyMapping);

            Assert.False(scalarPropertyMapping.IsReadOnly);
            mapping.SetReadOnly();
            Assert.True(scalarPropertyMapping.IsReadOnly);
        }
    }
}
