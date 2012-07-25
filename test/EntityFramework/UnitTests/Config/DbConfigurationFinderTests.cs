// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Config
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using FunctionalTests.TestHelpers;
    using Moq;
    using Xunit;

    public class DbConfigurationFinderTests
    {
        private static readonly Type _nullConfigType = new Mock<DbNullConfiguration>().Object.GetType();
        private static readonly Type _proxyConfigType = new Mock<DbConfigurationProxy>().Object.GetType();
        private static readonly Type _normalConfigType = new Mock<DbConfiguration>().Object.GetType();

        public class TryFindConfigurationType
        {
            [Fact]
            public void TryFindConfigurationType_returns_a_DbNullConfiguration_if_found()
            {
                Assert.Same(
                    _nullConfigType,
                    new DbConfigurationFinder().TryFindConfigurationType(
                        new[] { _normalConfigType, _normalConfigType, _proxyConfigType, _proxyConfigType, _nullConfigType, _nullConfigType }));
            }

            [Fact]
            public void TryFindConfigurationType_throws_if_types_list_contains_more_than_one_DbConfigurationProxy()
            {
                Assert.Equal(
                    Strings.MultipleConfigsInAssembly(_proxyConfigType.Assembly, typeof(DbConfigurationProxy).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => new DbConfigurationFinder().TryFindConfigurationType(
                            new[] { typeof(object), _proxyConfigType, _normalConfigType, _normalConfigType, _proxyConfigType })).Message);
            }

            [Fact]
            public void TryFindConfigurationType_returns_the_configuration_pointed_to_by_the_DbConfigurationProxy_if_found()
            {
                Assert.Same(
                    typeof(FunctionalTestsConfiguration),
                    new DbConfigurationFinder().TryFindConfigurationType(
                        new[] { typeof(object), _normalConfigType, _normalConfigType, typeof(UnitTestsConfiguration) }));
            }

            [Fact]
            public void TryFindConfigurationType_throws_if_types_list_contains_more_than_one_normal_DbConfigurations()
            {
                Assert.Equal(
                    Strings.MultipleConfigsInAssembly(_proxyConfigType.Assembly, typeof(DbConfiguration).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => new DbConfigurationFinder().TryFindConfigurationType(
                            new[] { typeof(object), _normalConfigType, _normalConfigType })).Message);
            }

            [Fact]
            public void TryFindConfigurationType_returns_the_normal_DbConfiguration_if_found()
            {
                Assert.Same(
                    _normalConfigType,
                    new DbConfigurationFinder().TryFindConfigurationType(
                        new[] { typeof(object), _normalConfigType, typeof(object) }));
            }

            [Fact]
            public void TryFindConfigurationType_returns_null_if_no_DbConfiguration_is_found()
            {
                Assert.Null(new DbConfigurationFinder().TryFindConfigurationType(new[] { typeof(object), typeof(object) }));
            }

            [Fact]
            public void TryFindConfigurationType_excludes_DbConfigurationProxy_and_DbNullConfiguration()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(new[] { typeof(DbConfiguration), typeof(DbConfigurationProxy) }));
            }

            [Fact]
            public void TryFindConfigurationType_excludes_generic_types()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(new[] { typeof(GenericConfiguration<>) }));
            }

            [Fact]
            public void TryFindConfigurationType_excludes_abstract_types()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(new[] { typeof(AbstractConfiguration) }));
            }
        }

        public class TryCreateConfiguration
        {
            [Fact]
            public void TryCreateConfiguration_returns_null_if_TryFindConfigurationType_returns_null()
            {
                Assert.Null(new DbConfigurationFinder().TryCreateConfiguration(new[] { typeof(object), typeof(object) }));
            }

            [Fact]
            public void TryCreateConfiguration_returns_null_if_TryFindConfigurationType_returns_DbNullConfiguration()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryCreateConfiguration(new[] { _nullConfigType }));
            }

            [Fact]
            public void TryCreateConfiguration_creates_instance_of_type_returned_by_TryFindConfigurationType()
            {
                Assert.IsType<FunctionalTestsConfiguration>(
                    new DbConfigurationFinder().TryCreateConfiguration(new[] { typeof(FunctionalTestsConfiguration) }));
            }

            [Fact]
            public void TryCreateConfiguration_creates_instance_of_type_returned_by_TryFindConfigurationType_when_proxied()
            {
                Assert.IsType<FunctionalTestsConfiguration>(
                    new DbConfigurationFinder().TryCreateConfiguration(new[] { typeof(UnitTestsConfiguration) }));
            }
        }

        public abstract class AbstractConfiguration : DbConfiguration
        {
        }

        public class GenericConfiguration<T> : DbConfiguration
        {
        }
    }
}
