// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using Xunit;

    public sealed class ManyToManyNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Map_should_set_configuration()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(AType1), "N"));
            var manyToManyNavigationPropertyConfiguration
                = new ManyToManyNavigationPropertyConfiguration<string, string>(navigationPropertyConfiguration);

            manyToManyNavigationPropertyConfiguration.Map(c => c.ToTable("Foo"));

            Assert.NotNull(navigationPropertyConfiguration.AssociationMappingConfiguration);
            Assert.IsType<ManyToManyAssociationMappingConfiguration>(
                navigationPropertyConfiguration.AssociationMappingConfiguration);
        }

        [Fact]
        public void MapToStoredProcedures_when_no_configuration_should_create_empty_configuration()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(AType1), "N"));
            var manyToManyNavigationPropertyConfiguration
                = new ManyToManyNavigationPropertyConfiguration<string, string>(navigationPropertyConfiguration);

            manyToManyNavigationPropertyConfiguration.MapToStoredProcedures();

            Assert.NotNull(navigationPropertyConfiguration.ModificationStoredProceduresConfiguration);
        }

        [Fact]
        public void MapToStoredProcedures_when_configuration_should_assign_configuration_to_nav_prop_configuration()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(AType1), "N"));
            var manyToManyNavigationPropertyConfiguration
                = new ManyToManyNavigationPropertyConfiguration<string, string>(navigationPropertyConfiguration);

            var called = false;

            manyToManyNavigationPropertyConfiguration.MapToStoredProcedures(m => { called = true; });

            Assert.True(called);
            Assert.NotNull(navigationPropertyConfiguration.ModificationStoredProceduresConfiguration);
        }

        public class AType1
        {
        }
    }
}
