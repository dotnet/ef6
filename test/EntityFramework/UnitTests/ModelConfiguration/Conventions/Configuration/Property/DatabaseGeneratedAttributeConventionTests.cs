namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using Xunit;

    public sealed class DatabaseGeneratedAttributeConventionTests : UnitTestBase
    {
        [Fact]
        public void Apply_should_set_store_generated_pattern()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new DatabaseGeneratedAttributeConvention.DatabaseGeneratedAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new DatabaseGeneratedAttribute(DatabaseGeneratedOption.None));

            Assert.Equal(DatabaseGeneratedOption.None, propertyConfiguration.DatabaseGeneratedOption);
        }

        [Fact]
        public void Apply_should_ignore_attribute_if_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration { DatabaseGeneratedOption = DatabaseGeneratedOption.Computed };

            new DatabaseGeneratedAttributeConvention.DatabaseGeneratedAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new DatabaseGeneratedAttribute(DatabaseGeneratedOption.None));

            Assert.Equal(DatabaseGeneratedOption.Computed, propertyConfiguration.DatabaseGeneratedOption);
        }
    }
}