// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using Xunit;

    public sealed class ForeignKeyNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Has_independent_key_should_execute_lambda_and_set_configuration()
        {
            var invoked = false;
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new ForeignKeyNavigationPropertyConfiguration(navigationPropertyConfiguration)
                .Map(c => { invoked = true; });

            Assert.True(invoked);
            Assert.NotNull(navigationPropertyConfiguration.Constraint);
            Assert.NotNull(navigationPropertyConfiguration.AssociationMappingConfiguration);
        }
    }
}
