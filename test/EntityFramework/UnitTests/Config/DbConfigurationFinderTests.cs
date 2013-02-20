// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using Moq;
    using Xunit;

    public class DbConfigurationFinderTests
    {
        private static readonly Type _normalConfigType = new Mock<DbConfiguration>().Object.GetType();

        public class TryFindConfigurationType
        {
            [Fact]
            public void TryFindConfigurationType_returns_the_configuration_pointed_to_by_the_DbConfigurationTypeAttribute_if_found()
            {
                Assert.Same(
                    typeof(FunctionalTestsConfiguration),
                    new DbConfigurationFinder().TryFindConfigurationType(
                        typeof(FakeContextWithAttribute),
                        new Type[0]));
            }

            [Fact]
            public void TryFindConfigurationType_throws_the_type_returned_by_DbConfigurationTypeAttribute_is_not_a_DbConfiguration()
            {
                Assert.Equal(
                    Strings.CreateInstance_BadDbConfigurationType(typeof(Random).ToString(), typeof(DbConfiguration).ToString()),
                    Assert.Throws<InvalidOperationException>(
                        () => new DbConfigurationFinder().TryFindConfigurationType(
                            typeof(FakeContextWithBadAttribute))).Message);
            }

            [Fact]
            public void TryFindConfigurationType_throws_if_types_list_contains_more_than_one_normal_DbConfigurations()
            {
                Assert.Equal(
                    Strings.MultipleConfigsInAssembly(_normalConfigType.Assembly, typeof(DbConfiguration).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => new DbConfigurationFinder().TryFindConfigurationType(
                            typeof(DbContext),
                            new[] { typeof(object), _normalConfigType, _normalConfigType })).Message);
            }

            [Fact]
            public void TryFindConfigurationType_returns_the_normal_DbConfiguration_if_found()
            {
                Assert.Same(
                    _normalConfigType,
                    new DbConfigurationFinder().TryFindConfigurationType(
                        typeof(DbContext),
                        new[] { typeof(object), _normalConfigType, typeof(object) }));
            }

            [Fact]
            public void TryFindConfigurationType_returns_null_if_no_DbConfiguration_is_found()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(typeof(DbContext), new[] { typeof(object), typeof(object) }));
            }

            [Fact]
            public void TryFindConfigurationType_excludes_generic_types()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(typeof(DbContext), new[] { typeof(GenericConfiguration<>) }));
            }

            [Fact]
            public void TryFindConfigurationType_excludes_abstract_types()
            {
                Assert.Null(
                    new DbConfigurationFinder().TryFindConfigurationType(typeof(DbContext), new[] { typeof(AbstractConfiguration) }));
            }
        }

        public abstract class AbstractConfiguration : DbConfiguration
        {
        }

        public class GenericConfiguration<T> : DbConfiguration
        {
        }

        [DbConfigurationType(typeof(FunctionalTestsConfiguration))]
        public class FakeContextWithAttribute : DbContext
        {
        }

        [DbConfigurationType(typeof(Random))]
        public class FakeContextWithBadAttribute : DbContext
        {
        }
    }
}
