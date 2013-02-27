// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class FunctionParameterTests
    {
        [Fact]
        public void Can_get_type_name()
        {
            var function
                = new FunctionParameter(
                    "P",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.InOut);

            Assert.Equal("String", function.TypeName);
        }

        [Fact]
        public void Can_get_and_set_name()
        {
            var function
                = new FunctionParameter(
                    "P",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.InOut);

            Assert.Equal("P", function.Name);

            function.Name = "Foo";

            Assert.Equal("Foo", function.Name);
        }

        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var parameter = 
                FunctionParameter.Create(
                    "param", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), ParameterMode.In);

            Assert.Equal("param", parameter.Name);
            Assert.Same(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), parameter.TypeUsage.EdmType);
            Assert.Equal(ParameterMode.In, parameter.Mode);
            Assert.True(parameter.IsReadOnly);
        }
    }
}
