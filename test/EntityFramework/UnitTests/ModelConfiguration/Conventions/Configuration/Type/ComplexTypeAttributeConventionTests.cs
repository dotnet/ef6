namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;

    public sealed class ComplexTypeAttributeConventionTests
    {
        [Fact]
        public void Apply_should_inline_type()
        {
            var mockType = new MockType();
            var modelConfiguration = new ModelConfiguration();

            new ComplexTypeAttributeConvention.ComplexTypeAttributeConventionImpl()
                .Apply(mockType, modelConfiguration, new ComplexTypeAttribute());

            Assert.True(modelConfiguration.IsComplexType(mockType));
        }
    }
}