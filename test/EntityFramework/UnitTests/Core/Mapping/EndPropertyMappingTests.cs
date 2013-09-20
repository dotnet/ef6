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

            Assert.Empty(endPropertyMapping.Properties);

            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            endPropertyMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.Properties.Single());
        }

        [Fact]
        public void Can_add_get_remove_properties()
        {
            var endPropertyMapping = new EndPropertyMapping();

            Assert.Empty(endPropertyMapping.Properties);

            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            endPropertyMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, endPropertyMapping.Properties.Single());

            endPropertyMapping.RemoveProperty(scalarPropertyMapping);

            Assert.Empty(endPropertyMapping.Properties);
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
                    () => mapping.AddProperty(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_read_only()
        {
            var associationEnd = new AssociationEndMember("E", new EntityType("E", "N", DataSpace.CSpace));
            var mapping = new EndPropertyMapping(associationEnd);
            var scalarPropertyMapping
                = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));
            mapping.AddProperty(scalarPropertyMapping);
            mapping.SetReadOnly();

            Assert.True(mapping.IsReadOnly);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mapping.RemoveProperty(scalarPropertyMapping)).Message);
        }
    }
}
