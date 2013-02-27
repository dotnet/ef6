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
    }
}