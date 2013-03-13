// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public class MigrationsConfigurationFinderTests : TestBase
    {
        [Fact]
        public void FindMigrationsConfiguration_returns_type_with_given_name_if_specified()
        {
            Assert.IsType<DiscoverableConfig>(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMigrations).Assembly))
                    .FindMigrationsConfiguration(null, typeof(DiscoverableConfig).FullName));
        }

        [Fact]
        public void FindMigrationsConfiguration_returns_single_matching_type_for_specific_context_if_no_type_name_is_specified()
        {
            Assert.IsType<DiscoverableConfig>(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMigrations).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithMigrations),
                        null));
        }

        public class ContextWithMigrations : DbContext
        {
        }

        public class DiscoverableConfig : DbMigrationsConfiguration<ContextWithMigrations>
        {
        }

        [Fact]
        public void FindMigrationsConfiguration_filters_generic_types()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithGenericConfig).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithGenericConfig),
                        null));
        }

        public class ContextWithGenericConfig : DbContext
        {
        }

        public class GenericConfig<T> : DbMigrationsConfiguration<ContextWithGenericConfig>
        {
        }

        [Fact]
        public void FindMigrationsConfiguration_filters_abstract_types()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithAbstractConfig).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithAbstractConfig),
                        null));
        }

        public class ContextWithAbstractConfig : DbContext
        {
        }

        public abstract class AbstractConfig : DbMigrationsConfiguration<ContextWithAbstractConfig>
        {
        }

        [Fact]
        public void FindMigrationsConfiguration_filters_types_without_a_parameterless_constructor()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithNoConstructorConfig).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithNoConstructorConfig),
                        null));
        }

        public class ContextWithNoConstructorConfig : DbContext
        {
        }

        public class NoConstructorConfig : DbMigrationsConfiguration<ContextWithNoConstructorConfig>
        {
            public NoConstructorConfig(int _)
            {
            }
        }

        [Fact]
        public void FindMigrationsConfiguration_can_throw_if_no_type_matching_name_is_found()
        {
            Assert.Equal(
                "EntityFramework.UnitTests Bad_Config_Type_Name",
                Assert.Throws<InvalidOperationException>(
                    () => new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithNoConfig).Assembly))
                              .FindMigrationsConfiguration(
                                  typeof(ContextWithNoConfig),
                                  "Bad_Config_Type_Name",
                                  noTypeWithName: (t, a) => new InvalidOperationException(a + " " + t))).Message);
        }

        [Fact]
        public void FindMigrationsConfiguration_can_return_null_if_no_type_matching_name_is_found()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithNoConfig).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithNoConfig),
                        "Bad_Config_Type_Name"));
        }

        public class ContextWithNoConfig : DbContext
        {
        }

        [Fact]
        public void FindMigrationsConfiguration_can_throw_if_multiple_types_matching_name_are_found()
        {
            Assert.Equal(
                "EntityFramework.UnitTests MultipleConfig",
                Assert.Throws<InvalidOperationException>(
                    () => new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMultipleConfigs).Assembly))
                              .FindMigrationsConfiguration(
                                  typeof(ContextWithMultipleConfigs),
                                  "MultipleConfig",
                                  multipleTypesWithName: (t, a) => new InvalidOperationException(a + " " + t))).Message);
        }

        [Fact]
        public void FindMigrationsConfiguration_can_return_null_if_multiple_types_matching_name_are_found()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMultipleConfigs).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithMultipleConfigs),
                        "MultipleConfig"));
        }

        public class ContextWithMultipleConfigs : DbContext
        {
        }

        public class MultipleConfig : DbMigrationsConfiguration<ContextWithMultipleConfigs>
        {
        }

        public class Outer
        {
            public class MultipleConfig : DbMigrationsConfiguration<ContextWithMultipleConfigs>
            {
            }
        }

        [Fact]
        public void FindMigrationsConfiguration_can_throw_if_filter_returns_no_types()
        {
            Assert.Equal(
                "EntityFramework.UnitTests",
                Assert.Throws<InvalidOperationException>(
                    () => new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithNoConfig).Assembly))
                              .FindMigrationsConfiguration(
                                  typeof(ContextWithNoConfig),
                                  null,
                                  noType: a => new InvalidOperationException(a))).Message);
        }

        [Fact]
        public void FindMigrationsConfiguration_can_return_null_if_filter_returns_no_types()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithNoConfig).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithNoConfig),
                        null));
        }

        [Fact]
        public void FindMigrationsConfiguration_can_throw_if_filter_returns_many_types()
        {
            Assert.Equal(
                "MultipleConfig MultipleConfig EntityFramework.UnitTests",
                Assert.Throws<InvalidOperationException>(
                    () => new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMultipleConfigs).Assembly))
                              .FindMigrationsConfiguration(
                                  typeof(ContextWithMultipleConfigs),
                                  null,
                                  multipleTypes:
                              (a, t) => new InvalidOperationException(t.First().Name + " " + t.Skip(1).First().Name + " " + a))).Message);
        }

        [Fact]
        public void FindMigrationsConfiguration_can_return_null_if_filter_returns_many_types()
        {
            Assert.Null(
                new MigrationsConfigurationFinder(new TypeFinder(typeof(ContextWithMultipleConfigs).Assembly))
                    .FindMigrationsConfiguration(
                        typeof(ContextWithMultipleConfigs),
                        null));
        }

        [Fact]
        public void FindMigrationsConfiguration_unwraps_and_preserves_stack_for_invocation_exceptions_thrown_when_constructing_object()
        {
            var exception =
                Assert.Throws<MigrationsException>(
                    () => new MigrationsConfigurationFinder(
                              new TypeFinder(typeof(ContextWithBadConfig).Assembly))
                              .FindMigrationsConfiguration(typeof(ContextWithBadConfig), null));

            Assert.Equal(Strings.DbMigrationsConfiguration_RootedPath(@"\Test"), exception.Message);
            Assert.Contains("set_MigrationsDirectory", exception.StackTrace);
        }

        public class ContextWithBadConfig : DbContext
        {
        }

        public class BadConfig : DbMigrationsConfiguration<ContextWithBadConfig>
        {
            public BadConfig()
            {
                MigrationsDirectory = @"\Test";
            }
        }
    }
}
