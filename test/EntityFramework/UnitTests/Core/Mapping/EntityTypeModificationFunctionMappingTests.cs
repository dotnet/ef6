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
    }
}
