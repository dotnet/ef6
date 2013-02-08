// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class ManyToManyNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Map_should_set_configuration()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());
            var manyToManyNavigationPropertyConfiguration
                = new ManyToManyNavigationPropertyConfiguration(navigationPropertyConfiguration);

            manyToManyNavigationPropertyConfiguration.Map(c => c.ToTable("Foo"));

            Assert.NotNull(navigationPropertyConfiguration.AssociationMappingConfiguration);
            Assert.IsType<ManyToManyAssociationMappingConfiguration>(
                navigationPropertyConfiguration.AssociationMappingConfiguration);
        }
    }
}
