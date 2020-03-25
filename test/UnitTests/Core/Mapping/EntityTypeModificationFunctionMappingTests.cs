// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class EntityTypeModificationFunctionMappingTests
    {
        [Fact]
        public void PrimaryParameterBindings_should_omit_original_value_parameters_when_update()
        {
            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("C", DataSpace.CSpace));

            var storageModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new[]
                        {
                            new ModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new ModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("M")
                                    },
                                null),
                                false)
                        },
                    null,
                    null);

            var storageEntityTypeModificationFunctionMapping
                = new EntityTypeModificationFunctionMapping(
                    new EntityType("E", "N", DataSpace.CSpace),
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping);

            Assert.Equal(2, storageEntityTypeModificationFunctionMapping.PrimaryParameterBindings.Count());
        }

        [Fact]
        public void Can_retrieve_properties_and_set_read_only()
        {
            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("C", DataSpace.CSpace));

            var deleteModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new[]
                        {
                            new ModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new ModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("M")
                                    },
                                null),
                                false)
                        },
                    null,
                    null);

            var insertModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new[]
                        {
                            new ModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new ModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("M")
                                    },
                                null),
                                false)
                        },
                    null,
                    null);

            var updateModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new[]
                        {
                            new ModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new ModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("M")
                                    },
                                null),
                                false)
                        },
                    null,
                    null);

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entityTypeModificationFunctionMapping
                = new EntityTypeModificationFunctionMapping(
                    entityType,
                    deleteModificationFunctionMapping,
                    insertModificationFunctionMapping,
                    updateModificationFunctionMapping);

            Assert.Same(entityTypeModificationFunctionMapping.EntityType, entityType);
            Assert.Same(entityTypeModificationFunctionMapping.DeleteFunctionMapping, deleteModificationFunctionMapping);
            Assert.Same(entityTypeModificationFunctionMapping.InsertFunctionMapping, insertModificationFunctionMapping);
            Assert.Same(entityTypeModificationFunctionMapping.UpdateFunctionMapping, updateModificationFunctionMapping);

            Assert.False(entityTypeModificationFunctionMapping.IsReadOnly);
            Assert.False(deleteModificationFunctionMapping.IsReadOnly);
            Assert.False(insertModificationFunctionMapping.IsReadOnly);
            Assert.False(updateModificationFunctionMapping.IsReadOnly);

            entityTypeModificationFunctionMapping.SetReadOnly();

            Assert.True(entityTypeModificationFunctionMapping.IsReadOnly);
            Assert.True(deleteModificationFunctionMapping.IsReadOnly);
            Assert.True(insertModificationFunctionMapping.IsReadOnly);
            Assert.True(updateModificationFunctionMapping.IsReadOnly);
        }
    }
}
