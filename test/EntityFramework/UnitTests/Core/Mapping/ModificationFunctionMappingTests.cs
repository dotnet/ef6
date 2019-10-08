// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public class ModificationFunctionMappingTests
    {
        [Fact]
        public void Cannot_create_with_null_argument()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", "T", "Q", entityType);
            var function = new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload());
            var parameterBindings = Enumerable.Empty<ModificationFunctionParameterBinding>();
            var rowsAffectedParameter = new FunctionParameter("rows_affected", new TypeUsage(), ParameterMode.Out);
            var resultBindings = Enumerable.Empty<ModificationFunctionResultBinding>();

            Assert.Equal(
                "entitySet",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionMapping(
                        null, entityType, function, 
                        parameterBindings, rowsAffectedParameter, resultBindings)).ParamName);

            Assert.Equal(
                "function",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionMapping(
                        entitySet, entityType, null,
                        parameterBindings, rowsAffectedParameter, resultBindings)).ParamName);

            Assert.Equal(
                "parameterBindings",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionMapping(
                        entitySet, entityType, function,
                        null, rowsAffectedParameter, resultBindings)).ParamName);
        }

        [Fact]
        public void Can_retrieve_properties()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", "T", "Q", entityType);
            var entityContainer = new EntityContainer("EC", DataSpace.SSpace);

            entityContainer.AddEntitySetBase(entitySet);

            var function = new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload());
            var parameterBindings
                = new[]
                  {
                      new ModificationFunctionParameterBinding(
                          new FunctionParameter(), 
                          new ModificationFunctionMemberPath(Enumerable.Empty<EdmMember>(), null),
                          true)
                  };
            var rowsAffectedParameter = new FunctionParameter("rows_affected", new TypeUsage(), ParameterMode.Out);

            var resultBindings
                = new[]
                  {
                      new ModificationFunctionResultBinding(
                          "C", 
                          EdmProperty.CreatePrimitive(
                              "P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))
                  };

            var mapping
                = new ModificationFunctionMapping(
                    entitySet, entityType, function,
                    parameterBindings, rowsAffectedParameter, resultBindings);

            Assert.Same(rowsAffectedParameter, mapping.RowsAffectedParameter);
            Assert.Same(function, mapping.Function);
            Assert.Equal(parameterBindings, mapping.ParameterBindings);
            Assert.Equal(resultBindings, mapping.ResultBindings);
        }

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

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", "T", "Q", entityType);
            var entityContainer = new EntityContainer("EC", DataSpace.SSpace);

            entityContainer.AddEntitySetBase(entitySet);

            var function = new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload());
            var parameterBindings
                = new[]
                  {
                      new ModificationFunctionParameterBinding(
                          new FunctionParameter(), 
                          new ModificationFunctionMemberPath(Enumerable.Empty<EdmMember>(), null),
                          true)
                  };
            var rowsAffectedParameter = new FunctionParameter("rows_affected", new TypeUsage(), ParameterMode.Out);

            var resultBindings
                = new[]
                  {
                      new ModificationFunctionResultBinding(
                          "C", 
                          EdmProperty.CreatePrimitive(
                              "P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))
                  };

            var mapping
                = new ModificationFunctionMapping(
                    entitySet, entityType, function,
                    parameterBindings, rowsAffectedParameter, resultBindings);

            Assert.False(mapping.IsReadOnly);
            parameterBindings.Each(b => Assert.False(b.IsReadOnly));
            resultBindings.Each(b => Assert.False(b.IsReadOnly));
            mapping.SetReadOnly();
            Assert.True(mapping.IsReadOnly);
            parameterBindings.Each(b => Assert.True(b.IsReadOnly));
            resultBindings.Each(b => Assert.True(b.IsReadOnly));
        }
    }
}
