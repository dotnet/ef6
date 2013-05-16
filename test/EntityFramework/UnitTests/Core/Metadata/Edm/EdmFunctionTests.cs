// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;
    using System.Linq;

    public class EdmFunctionTests
    {
        [Fact]
        public void Can_get_and_set_schema()
        {
            var function
                = new EdmFunction("F", "N", DataSpace.SSpace)
                      {
                          Schema = "Foo"
                      };

            Assert.Equal("Foo", function.Schema);
        }

        [Fact]
        public void Can_get_and_set_store_function_name_attribute()
        {
            var function
                = new EdmFunction("F", "N", DataSpace.SSpace)
                {
                    StoreFunctionNameAttribute = "Foo"
                };

            Assert.Equal("Foo", function.StoreFunctionNameAttribute);
        }

        [Fact]
        public void Can_get_full_name()
        {
            var function = new EdmFunction("F", "N", DataSpace.SSpace);

            Assert.Equal("N.F", function.FullName);

            function.Name = "Foo";

            Assert.Equal("N.Foo", function.FullName);
        }

        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var stringTypeUsage =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var function =
                EdmFunction.Create(
                    "foo",
                    "namespace",
                    DataSpace.SSpace,
                    new EdmFunctionPayload()
                        {
                            Schema = "dbo",
                            StoreFunctionName = "storeFunction",
                            IsAggregate = false,
                            IsNiladic = true,
                            ReturnParameters = new[] { new FunctionParameter("ReturnParam", stringTypeUsage, ParameterMode.ReturnValue) }
                        },
                    new[] { new MetadataProperty("TestProperty", stringTypeUsage, "bar") });

            Assert.NotNull(function);
            Assert.Equal("namespace.foo", function.FullName);
            Assert.Equal("dbo", function.Schema);
            Assert.Equal("storeFunction", function.StoreFunctionNameAttribute);
            Assert.False(function.AggregateAttribute);
            Assert.True(function.NiladicFunctionAttribute);
            Assert.Equal(new[] {"ReturnParam"}, function.ReturnParameters.Select(p => p.Name));

            var metadataProperty = function.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("bar", metadataProperty.Value);

            Assert.True(function.IsReadOnly);
        }

        [Fact]
        public void Cannot_create_function_with_input_parameter_with_ReturnValue_mode()
        {
            Assert.Equal(
                Resources.Strings.ReturnParameterInInputParameterCollection,
                Assert.Throws<ArgumentException>(
                    () => new EdmFunction(
                              "foo",
                              "bar",
                              DataSpace.CSpace,
                              new EdmFunctionPayload()
                                  {
                                      Parameters =
                                          new[]
                                              {
                                                  new FunctionParameter(
                                                      "returnParam",
                                                      TypeUsage.CreateDefaultTypeUsage(
                                                          PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                                      ParameterMode.ReturnValue)
                                              }
                                  })).Message);
        }

        [Fact]
        public void Cannot_create_function_with_null_return_parameter()
        {
            Assert.Equal(
                Resources.Strings.ADP_CollectionParameterElementIsNull("ReturnParameters"),
                Assert.Throws<ArgumentException>(
                    () => new EdmFunction(
                              "foo",
                              "bar",
                              DataSpace.CSpace,
                              new EdmFunctionPayload()
                                  {
                                      ReturnParameters = new FunctionParameter[] { null }
                                  })).Message);
        }

        [Fact]
        public void Cannot_create_function_with_return_parameter_whose_mode_is_not_ReturnValue()
        {
            foreach (var mode in new [] { ParameterMode.In, ParameterMode.InOut, ParameterMode.Out })
            {
                Assert.Equal(
                    Resources.Strings.NonReturnParameterInReturnParameterCollection,
                    Assert.Throws<ArgumentException>(
                        () => new EdmFunction(
                                  "foo",
                                  "bar",
                                  DataSpace.CSpace,
                                  new EdmFunctionPayload()
                                      {
                                          ReturnParameters =
                                              new[]
                                                  {
                                                      new FunctionParameter(
                                                          "param",
                                                          TypeUsage.CreateDefaultTypeUsage(
                                                              PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                                          mode)
                                                  }
                                      })).Message);
            }
        }

        [Fact]
        public void Cannot_create_function_if_entity_sets_not_null_an_not_matching_number_of_return_parameters()
        {
            var entitySet = new EntitySet("set", null, null, null, new EntityType("entity", "ns", DataSpace.CSpace));

            Assert.Equal(
                Resources.Strings.NumberOfEntitySetsDoesNotMatchNumberOfReturnParameters,
                Assert.Throws<ArgumentException>(
                    () => new EdmFunction(
                              "foo",
                              "bar",
                              DataSpace.CSpace,
                              new EdmFunctionPayload()
                                  {
                                      EntitySets = new[] { entitySet, entitySet },
                                      ReturnParameters =
                                          new[]
                                              {
                                                  new FunctionParameter(
                                                      "param",
                                                      TypeUsage.CreateDefaultTypeUsage(
                                                          PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                                      ParameterMode.ReturnValue)
                                                  ,
                                              }
                                  })).Message);
        }

        [Fact]
        public void Cannot_create_function_with_multiple_resultsets_and_null_entity_sets()
        {
            Assert.Equal(
                Resources.Strings.NullEntitySetsForFunctionReturningMultipleResultSets,
                Assert.Throws<ArgumentException>(
                    () => new EdmFunction(
                              "foo",
                              "bar",
                              DataSpace.CSpace,
                              new EdmFunctionPayload()
                              {
                                  ReturnParameters =
                                      new[]
                                              {
                                                  new FunctionParameter(
                                                      "param1",
                                                      TypeUsage.CreateDefaultTypeUsage(
                                                          PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                                      ParameterMode.ReturnValue),
                                                  new FunctionParameter(
                                                      "param2",
                                                      TypeUsage.CreateDefaultTypeUsage(
                                                          PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                                      ParameterMode.ReturnValue)
                                              }
                              })).Message);
        }

    }
}