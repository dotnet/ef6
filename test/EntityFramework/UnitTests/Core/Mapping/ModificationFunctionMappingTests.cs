// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class ModificationFunctionMappingTests
    {
        [Fact]
        public void Can_get_rows_affected_parameter_name()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            var entityContainer = new EntityContainer("EC", DataSpace.SSpace);

            entityContainer.AddEntitySetBase(entitySet);

            var storageModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload()),
                    Enumerable.Empty<ModificationFunctionParameterBinding>(),
                    new FunctionParameter("rows_affected", new TypeUsage(), ParameterMode.Out),
                    null);

            Assert.Equal("rows_affected", storageModificationFunctionMapping.RowsAffectedParameterName);
        }
    }
}
