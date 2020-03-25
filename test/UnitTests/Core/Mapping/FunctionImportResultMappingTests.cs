// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class FunctionImportResultMappingTests
    {
        [Fact]
        public void Can_add_get_remove_type_mapping()
        {
            var resultMapping = new FunctionImportResultMapping();

            Assert.Empty(resultMapping.TypeMappings);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>(),
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>());

            resultMapping.AddTypeMapping(typeMapping);
            
            Assert.Equal(1, resultMapping.TypeMappings.Count);
            Assert.Same(typeMapping, resultMapping.TypeMappings[0]);

            resultMapping.RemoveTypeMapping(typeMapping);

            Assert.Empty(resultMapping.TypeMappings);
        }

        [Fact]
        public void Cannot_add_type_mapping_if_read_only()
        {
            var resultMapping = new FunctionImportResultMapping();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>(),
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>());

            resultMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => resultMapping.AddTypeMapping(typeMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_type_mapping_if_read_only()
        {
            var resultMapping = new FunctionImportResultMapping();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>(),
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>());

            resultMapping.AddTypeMapping(typeMapping);
            resultMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => resultMapping.RemoveTypeMapping(typeMapping)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var resultMapping = new FunctionImportResultMapping();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var typeMapping = new FunctionImportEntityTypeMapping(
                Enumerable.Empty<EntityType>(),
                new[] { entityType },
                new Collection<FunctionImportReturnTypePropertyMapping>(),
                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>());

            resultMapping.AddTypeMapping(typeMapping);

            Assert.False(typeMapping.IsReadOnly);
            resultMapping.SetReadOnly();
            Assert.True(typeMapping.IsReadOnly);
        }
    }
}
