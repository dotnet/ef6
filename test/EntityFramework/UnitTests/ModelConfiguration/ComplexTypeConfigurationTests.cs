namespace System.Data.Entity.ModelConfiguration.UnitTests
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Xunit;

    public sealed class ComplexTypeConfigurationTests
    {
        [Fact]
        public void Configuration_should_return_internal_configuration()
        {
            var complexTypeConfiguration = new ComplexTypeConfiguration<object>();

            Assert.NotNull(complexTypeConfiguration.Configuration);
            Assert.Equal(typeof(ComplexTypeConfiguration), complexTypeConfiguration.Configuration.GetType());
        }
    }
}