namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class IndependentNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Has_independent_key_should_execute_lambda_and_set_configuration()
        {
            var invoked = false;
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new ForeignKeyNavigationPropertyConfiguration(navigationPropertyConfiguration)
                .Map(c => { invoked = true; });

            Assert.True(invoked);
            Assert.NotNull(navigationPropertyConfiguration.Constraint);
            Assert.NotNull(navigationPropertyConfiguration.AssociationMappingConfiguration);
        }
    }
}