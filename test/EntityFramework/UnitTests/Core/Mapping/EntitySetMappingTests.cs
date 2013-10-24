// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EntitySetMappingTests
    {
        [Fact]
        public void Cannot_create_entity_set_mapping_with_null_entity_set()
        {
            Assert.Equal(
                "entitySet",
                Assert.Throws<ArgumentNullException>(() => new EntitySetMapping(null, null)).ParamName);
        }

        [Fact]
        public void Can_get_entity_type_mappings()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySetMapping = new EntitySetMapping(new EntitySet(), entityContainerMapping);

            Assert.Empty(entitySetMapping.EntityTypeMappings);

            var entityTypeMapping
                = new EntityTypeMapping(
                    new EntitySetMapping(new EntitySet(), entityContainerMapping));

            entitySetMapping.AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, entitySetMapping.EntityTypeMappings.Single());
        }

        [Fact]
        public void Can_get_entity_set()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySet = new EntitySet();
            var entitySetMapping = new EntitySetMapping(entitySet, entityContainerMapping);

            Assert.Same(entitySet, entitySetMapping.EntitySet);
        }

        [Fact]
        public void Can_clear_modification_function_mappings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("S", "N", null, null, entityType);
            var function = new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload());

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddEntitySetBase(entitySet);

            var entitySetMapping =
                new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(container));

            var functionMapping =
                new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    null,
                    null);

            var entityFunctionMappings =
                new EntityTypeModificationFunctionMapping(entityType, functionMapping, null, null);

            entitySetMapping.AddModificationFunctionMapping(entityFunctionMappings);

            Assert.Equal(1, entitySetMapping.ModificationFunctionMappings.Count());

            entitySetMapping.ClearModificationFunctionMappings();

            Assert.Equal(0, entitySetMapping.ModificationFunctionMappings.Count());
        }

        [Fact]
        public void Can_add_get_remove_modification_function_mappings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("S", "N", null, null, entityType);
            var function = new EdmFunction(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                    {
                        IsFunctionImport = true
                    });

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddEntitySetBase(entitySet);
            container.AddFunctionImport(function);

            var entitySetMapping =
                new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(container));

            var functionMapping =
                new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    null,
                    null);

            var entityFunctionMappings =
                new EntityTypeModificationFunctionMapping(entityType, functionMapping, null, null);

            entitySetMapping.AddModificationFunctionMapping(entityFunctionMappings);

            Assert.Same(entityFunctionMappings, entitySetMapping.ModificationFunctionMappings.Single());

            entitySetMapping.RemoveModificationFunctionMapping(entityFunctionMappings);

            Assert.Empty(entitySetMapping.ModificationFunctionMappings);
        }

        [Fact]
        public void Cannot_add_type_mapping_when_read_only()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySetMapping = new EntitySetMapping(new EntitySet(), entityContainerMapping);
            var entityTypeMapping = new EntityTypeMapping(entitySetMapping);

            entitySetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entitySetMapping.AddTypeMapping(entityTypeMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_type_mapping_when_read_only()
        {
            var entityContainerMapping = new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace));
            var entitySetMapping = new EntitySetMapping(new EntitySet(), entityContainerMapping);
            var entityTypeMapping = new EntityTypeMapping(entitySetMapping);

            entitySetMapping.AddTypeMapping(entityTypeMapping);
            entitySetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entitySetMapping.RemoveTypeMapping(entityTypeMapping)).Message);
        }

        [Fact]
        public void Cannot_add_modification_function_mapping_when_read_only()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("S", "N", null, null, entityType);
            var function = new EdmFunction(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsFunctionImport = true
                });

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddEntitySetBase(entitySet);
            container.AddFunctionImport(function);

            var entitySetMapping =
                new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(container));

            var functionMapping =
                new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    null,
                    null);

            var entityFunctionMappings =
                new EntityTypeModificationFunctionMapping(entityType, functionMapping, null, null);

            entitySetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entitySetMapping.AddModificationFunctionMapping(entityFunctionMappings)).Message);
        }

        [Fact]
        public void Cannot_remove_modification_function_mapping_when_read_only()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("S", "N", null, null, entityType);
            var function = new EdmFunction(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsFunctionImport = true
                });

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddEntitySetBase(entitySet);
            container.AddFunctionImport(function);

            var entitySetMapping =
                new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(container));

            var functionMapping =
                new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    null,
                    null);

            var entityFunctionMappings =
                new EntityTypeModificationFunctionMapping(entityType, functionMapping, null, null);

            entitySetMapping.AddModificationFunctionMapping(entityFunctionMappings);
            entitySetMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => entitySetMapping.RemoveModificationFunctionMapping(entityFunctionMappings)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("S", "N", null, null, entityType);
            var function = new EdmFunction(
                "F", "N", DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsFunctionImport = true
                });

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddEntitySetBase(entitySet);
            container.AddFunctionImport(function);

            var entitySetMapping =
                new EntitySetMapping(
                    entitySet,
                    new EntityContainerMapping(container));

            var functionMapping =
                new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    function,
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    null,
                    null);

            var entityFunctionMapping =
                new EntityTypeModificationFunctionMapping(entityType, functionMapping, null, null);

            entitySetMapping.AddModificationFunctionMapping(entityFunctionMapping);

            var entityTypeMapping = new EntityTypeMapping(entitySetMapping);

            entitySetMapping.AddTypeMapping(entityTypeMapping);

            Assert.False(entityTypeMapping.IsReadOnly);
            Assert.False(entityFunctionMapping.IsReadOnly);
            entitySetMapping.SetReadOnly();
            Assert.True(entityTypeMapping.IsReadOnly);
            Assert.True(entityFunctionMapping.IsReadOnly);
        }
    }
}
