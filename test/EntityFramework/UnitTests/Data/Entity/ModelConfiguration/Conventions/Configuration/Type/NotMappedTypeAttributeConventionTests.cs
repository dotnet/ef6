namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;

    public sealed class NotMappedTypeAttributeConventionTests : UnitTestBase
    {
        [Fact]
        public void Apply_should_ignore_type()
        {
            var mockType = new MockType();
            var modelConfiguration = new ModelConfiguration();

            new NotMappedTypeAttributeConvention.NotMappedTypeAttributeConventionImpl()
                .Apply(mockType, modelConfiguration, new NotMappedAttribute());

            Assert.True(modelConfiguration.IsIgnoredType(mockType));
        }
    }
}