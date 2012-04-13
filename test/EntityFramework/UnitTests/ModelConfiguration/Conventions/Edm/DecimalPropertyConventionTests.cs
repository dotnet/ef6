namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class DecimalPropertyConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_decimal()
        {
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Decimal;

            ((IEdmConvention<EdmProperty>)new DecimalPropertyConvention())
                .Apply(property, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal((byte)18, primitiveTypeFacets.Precision);
            Assert.Equal((byte)2, primitiveTypeFacets.Scale);
        }

        [Fact]
        public void Apply_should_not_set_defaults_for_configured_precision()
        {
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Decimal;
            property.PropertyType.PrimitiveTypeFacets.Precision = 22;

            ((IEdmConvention<EdmProperty>)new DecimalPropertyConvention())
                .Apply(property, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal((byte)22, primitiveTypeFacets.Precision);
            Assert.Equal((byte)2, primitiveTypeFacets.Scale);
        }

        [Fact]
        public void Apply_should_not_set_defaults_for_configured_scale()
        {
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Decimal;
            property.PropertyType.PrimitiveTypeFacets.Scale = 4;

            ((IEdmConvention<EdmProperty>)new DecimalPropertyConvention())
                .Apply(property, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal((byte)18, primitiveTypeFacets.Precision);
            Assert.Equal((byte)4, primitiveTypeFacets.Scale);
        }
    }
}