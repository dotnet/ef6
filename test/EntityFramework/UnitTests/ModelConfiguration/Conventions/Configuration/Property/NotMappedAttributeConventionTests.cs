namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public sealed class NotMappedAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_ignore_property()
        {
            var mockPropertyInfo = new MockPropertyInfo();
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            new NotMappedPropertyAttributeConvention.NotMappedPropertyAttributeConventionImpl()
                .Apply(mockPropertyInfo, entityTypeConfiguration, new NotMappedAttribute());

            Assert.True(entityTypeConfiguration.IgnoredProperties.Contains(mockPropertyInfo));
        }
    }
}