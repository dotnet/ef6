// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class AssociationSetModificationFunctionMappingTests
    {
        [Fact]
        public void Can_retrieve_properties_and_set_read_only()
        {
            var associationType = new AssociationType("A", "N", false, DataSpace.CSpace);
            var associationSet = new AssociationSet("AS", associationType);

            associationSet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("C", DataSpace.CSpace));

            var deleteModificationFunctionMapping
                = new ModificationFunctionMapping(
                    associationSet,
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
                    associationSet,
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


            var associationSetModificationFunctionMapping
                = new AssociationSetModificationFunctionMapping(
                    associationSet,
                    deleteModificationFunctionMapping,
                    insertModificationFunctionMapping);

            Assert.Same(associationSetModificationFunctionMapping.AssociationSet, associationSet);
            Assert.Same(associationSetModificationFunctionMapping.DeleteFunctionMapping, deleteModificationFunctionMapping);
            Assert.Same(associationSetModificationFunctionMapping.InsertFunctionMapping, insertModificationFunctionMapping);

            Assert.False(associationSetModificationFunctionMapping.IsReadOnly);
            Assert.False(deleteModificationFunctionMapping.IsReadOnly);
            Assert.False(insertModificationFunctionMapping.IsReadOnly);

            associationSetModificationFunctionMapping.SetReadOnly();

            Assert.True(associationSetModificationFunctionMapping.IsReadOnly);
            Assert.True(deleteModificationFunctionMapping.IsReadOnly);
            Assert.True(insertModificationFunctionMapping.IsReadOnly);
        }
    }
}
