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
            var parameter
                = FunctionParameter.Create(
                    "param", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), ParameterMode.In);

            Assert.Equal("param", parameter.Name);
            Assert.Same(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), parameter.TypeUsage.EdmType);
            Assert.Equal(ParameterMode.In, parameter.Mode);
            Assert.True(parameter.IsReadOnly);
        }

        [Fact]
        public void Can_get_facets_via_property_wrappers()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[]
                        {
                            CreateConstFacet("Precision", PrimitiveTypeKind.Byte, (byte)10),
                            CreateConstFacet("Scale", PrimitiveTypeKind.Byte, (byte)10),
                            CreateConstFacet("MaxLength", PrimitiveTypeKind.Int32, 200)
                        });

            var functionParameter = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.Equal(200, functionParameter.MaxLength.Value);
            Assert.Equal(10, functionParameter.Precision.Value);
            Assert.Equal(10, functionParameter.Scale.Value);
        }

        [Fact]
        public void IsPrecisionConstant_returns_true_for_const_Precision_facet_and_value_cannot_be_changed()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("Precision", PrimitiveTypeKind.Byte, (byte)10) });

            var functionParameter = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.True(functionParameter.IsPrecisionConstant);
            Assert.Equal(10, (byte)functionParameter.Precision);
        }

        [Fact]
        public void IsPrecisionConstant_returns_false_if_Precision_facet_not_present_and_value_is_null()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.False(property.IsPrecisionConstant);
            Assert.Null(property.Precision);
        }

        [Fact]
        public void IsScaleConstant_returns_true_for_const_Scale_facet_and_value_cannot_be_changed()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("Scale", PrimitiveTypeKind.Byte, (byte)10) });

            var property = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.True(property.IsScaleConstant);
            Assert.Equal(10, (byte)property.Scale);
        }

        [Fact]
        public void IsScaleConstant_returns_false_if_Scale_facet_not_present_and_value_is_null()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.False(property.IsScaleConstant);
            Assert.Null(property.Scale);
        }

        [Fact]
        public void IsMaxLengthConstant_returns_true_for_const_MaxLength_facet_and_value_cannot_be_changed()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new[] { CreateConstFacet("MaxLength", PrimitiveTypeKind.Int32, 200) });

            var property = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.True(property.IsMaxLengthConstant);
            Assert.Equal(200, property.MaxLength);
        }

        [Fact]
        public void IsMaxLengthConstant_returns_false_if_MaxLength_facet_not_present_and_value_is_null()
        {
            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), new Facet[0]);

            var property = new FunctionParameter("P", typeUsage, ParameterMode.In);

            Assert.False(property.IsMaxLengthConstant);
            Assert.Null(property.MaxLength);
        }

        private static Facet CreateConstFacet(string facetName, PrimitiveTypeKind facetTypeKind, object value)
        {
            return
                Facet.Create(
                    new FacetDescription(
                        facetName,
                        PrimitiveType.GetEdmPrimitiveType(facetTypeKind),
                        null,
                        null,
                        value,
                        true,
                        null),
                    value);
        }
    }
}
