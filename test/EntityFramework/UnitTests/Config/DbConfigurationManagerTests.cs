// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using FunctionalTests.TestHelpers;
    using Moq;
    using SimpleModel;
    using Xunit;

    public class DbConfigurationManagerTests : TestBase
    {
        public class Instance : TestBase
        {
            [Fact]
            public void Instance_returns_the_Singleton_instance()
            {
                Assert.NotNull(DbConfigurationManager.Instance);
                Assert.Same(DbConfigurationManager.Instance, DbConfigurationManager.Instance);
            }
        }

        public class GetConfiguration : TestBase
        {
            [Fact]
            public void GetConfiguration_returns_the_configuration_at_the_top_of_the_stack_if_overriding_configurations_have_been_pushed()
            {
                var manager = CreateManager();
                var configuration = new Mock<DbConfiguration>
                                        {
                                            CallBase = true
                                        }.Object;

                manager.SetConfiguration(configuration);
                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                var pushed1 = manager.GetConfiguration();
                Assert.NotSame(configuration, pushed1);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                var pushed2 = manager.GetConfiguration();
                Assert.NotSame(pushed1, pushed2);
                Assert.NotSame(configuration, pushed2);
            }

            [Fact]
            public void GetConfiguration_returns_the_previously_set_configuration()
            {
                var manager = CreateManager();
                var configuration = new Mock<DbConfiguration>().Object;

                manager.SetConfiguration(configuration);

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void GetConfiguration_sets_and_returns_a_new_configuration_if_none_was_previously_set()
            {
                var manager = CreateManager();

                var configuration = manager.GetConfiguration();

                Assert.NotNull(configuration);
                Assert.Same(configuration, manager.GetConfiguration());
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetConfiguration_for_default_value_can_be_called_from_multiple_threads_concurrently()
            {
                ConfigurationThreadTest(m => { }, m => { });
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetConfiguration_for_pushed_config_can_be_called_from_multiple_threads_concurrently()
            {
                ConfigurationThreadTest(m => m.PushConfiguration(AppConfig.DefaultInstance, typeof(SimpleModelContext)), m => { });
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void GetConfiguration_for_pushed_and_popped_config_can_be_called_from_multiple_threads_concurrently()
            {
                ConfigurationThreadTest(
                    m =>
                        {
                            m.PushConfiguration(AppConfig.DefaultInstance, typeof(SimpleModelContext));
                            m.PopConfiguration(AppConfig.DefaultInstance);
                        },
                    m => { });
            }
        }

        public class SetConfiguration : TestBase
        {
            [Fact]
            public void SetConfiguration_sets_and_locks_the_configuration_if_none_was_previously_set_and_config_file_has_none()
            {
                var manager = CreateManager();
                var mockConfiguration = new Mock<DbConfiguration>();

                manager.SetConfiguration(mockConfiguration.Object);

                Assert.Same(mockConfiguration.Object, manager.GetConfiguration());

                mockConfiguration.Verify(m => m.Lock());
            }

            [Fact]
            public void The_same_type_of_configuration_can_be_set_multiple_times()
            {
                var manager = CreateManager();
                var configuration1 = new Mock<DbConfiguration>().Object;
                var configuration2 = new Mock<DbConfiguration>().Object;

                manager.SetConfiguration(configuration1);
                manager.SetConfiguration(configuration2);

                Assert.Same(configuration1, manager.GetConfiguration());
            }

            [Fact]
            public void SetConfiguration_discards_the_given_configuration_and_uses_the_configuration_from_the_config_file_if_it_exists()
            {
                var mockLoadedConfig = new Mock<DbConfiguration>();
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(It.IsAny<AppConfig>())).Returns(mockLoadedConfig.Object);

                var manager = CreateManager(mockLoader);
                var mockConfiguration = new Mock<DbConfiguration>();

                manager.SetConfiguration(mockConfiguration.Object);

                Assert.Same(mockLoadedConfig.Object, manager.GetConfiguration());

                mockLoadedConfig.Verify(m => m.Lock());
                mockConfiguration.Verify(m => m.Lock(), Times.Never());
            }

            [Fact]
            public void SetConfiguration_throws_if_an_attempt_is_made_to_set_a_different_configuration_type()
            {
                var manager = CreateManager();
                var configuration1 = new FakeConfiguration();
                var configuration2 = new Mock<DbConfiguration>().Object;

                manager.SetConfiguration(configuration1);

                Assert.Equal(
                    Strings.ConfigurationSetTwice(configuration2.GetType().Name, configuration1.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => manager.SetConfiguration(configuration2)).Message);
            }

            [Fact]
            public void SetConfiguration_throws_if_an_attempt_is_made_to_set_a_configuration_after_the_default_has_already_been_used()
            {
                var manager = CreateManager();
                var configuration = new Mock<DbConfiguration>().Object;

                manager.GetConfiguration(); // Initialize default

                Assert.Equal(
                    Strings.DefaultConfigurationUsedBeforeSet(configuration.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => manager.SetConfiguration(configuration)).Message);
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void SetConfiguration_can_be_called_from_multiple_threads_concurrently_and_only_one_will_win()
            {
                ConfigurationThreadTest(
                    m => { },
                    m => m.SetConfiguration(new UnitTestsConfiguration()));
            }
        }

        public class EnsureLoadedForContext : TestBase
        {
            [Fact]
            public void EnsureLoadedForContext_loads_configuration_from_context_assembly_if_none_was_previously_used()
            {
                var foundConfiguration = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>())).Returns(foundConfiguration);

                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()));
                Assert.Same(foundConfiguration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_given_context_is_exactly_DbContext()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();

                CreateManager(null, mockFinder).EnsureLoadedForContext(typeof(DbContext));

                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Never());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_assembly_has_already_been_checked()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Once());

                manager.EnsureLoadedForContext(typeof(FakeContext));

                // Finder has not been used again
                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Once());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_an_override_configuration_has_been_pushed()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Once());

                manager.EnsureLoadedForContext(typeof(FakeContext));

                // Finder has not been used again
                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Once());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_none_was_previously_used_and_no_configuration_is_found_in_assembly()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()));

                var configuration = new Mock<DbConfiguration>().Object;
                manager.SetConfiguration(configuration);

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_one_was_previously_used_but_no_configuration_is_found_in_assembly()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                var configuration = manager.GetConfiguration();

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_one_was_previously_used_but_DbNullConfiguration_is_found_in_assembly()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>())).Returns(typeof(DbNullConfiguration));
                var manager = CreateManager(null, mockFinder);

                var configuration = manager.GetConfiguration();

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_configuration_in_assembly_is_the_same_as_was_previously_used()
            {
                var configuration = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>())).Returns(configuration.GetType());
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(configuration);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_throws_if_found_configuration_does_not_match_previously_used_configuration()
            {
                var configuration1 = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>())).Returns(typeof(FakeConfiguration));
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(configuration1);

                Assert.Equal(
                    Strings.SetConfigurationNotDiscovered(configuration1.GetType().Name, typeof(FakeContext).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => manager.EnsureLoadedForContext(typeof(FakeContext))).Message);
            }

            [Fact]
            public void EnsureLoadedForContext_throws_if_configuration_is_found_but_default_was_previously_used()
            {
                var configuration = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(It.IsAny<IEnumerable<Type>>())).Returns(configuration.GetType());
                var manager = CreateManager(null, mockFinder);

                manager.GetConfiguration();

                Assert.Equal(
                    Strings.ConfigurationNotDiscovered(configuration.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () => manager.EnsureLoadedForContext(typeof(FakeContext))).Message);
            }

            [Fact]
            public void EnsureLoadedForContext_throws_if_configuration_was_set_but_is_not_found_in_context_assembly()
            {
                var configuration = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(configuration);

                Assert.Equal(
                    Strings.SetConfigurationNotDiscovered(configuration.GetType().Name, typeof(FakeContext).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => manager.EnsureLoadedForContext(typeof(FakeContext))).Message);
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void EnsureLoadedForContext_can_be_called_from_multiple_threads_concurrently_before_configuration_has_been_used()
            {
                ConfigurationThreadTest(
                    m => { },
                    m => m.EnsureLoadedForContext(typeof(SimpleModelContext)));
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void EnsureLoadedForContext_can_be_called_from_multiple_threads_concurrently_after_push_and_pop()
            {
                ConfigurationThreadTest(
                    m =>
                        {
                            m.PushConfiguration(AppConfig.DefaultInstance, typeof(SimpleModelContext));
                            m.PopConfiguration(AppConfig.DefaultInstance);
                        },
                    m => m.EnsureLoadedForContext(typeof(SimpleModelContext)));
            }
        }

        public class PushConfiguration : TestBase
        {
            [Fact]
            public void PushConfugiration_pushes_and_locks_configuration_from_config_if_found()
            {
                var mockConfiguration = new Mock<DbConfiguration>();
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(mockConfiguration.Object);
                var mockFinder = new Mock<DbConfigurationFinder>();

                var manager = CreateManager(mockLoader, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockConfiguration.Verify(m => m.Lock());
                Assert.Same(mockConfiguration.Object, manager.GetConfiguration());
                mockLoader.Verify(m => m.TryLoadFromConfig(AppConfig.DefaultInstance));
                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()), Times.Never());
            }

            [Fact]
            public void PushConfugiration_pushes_and_locks_configuration_discovered_in_context_assembly_if_found()
            {
                var mockConfiguration = new Mock<DbConfiguration>();
                var mockLoader = new Mock<DbConfigurationLoader>();
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>())).Returns(mockConfiguration.Object);

                var manager = CreateManager(mockLoader, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockConfiguration.Verify(m => m.Lock());
                Assert.Same(mockConfiguration.Object, manager.GetConfiguration());
                mockLoader.Verify(m => m.TryLoadFromConfig(AppConfig.DefaultInstance));
                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()));
            }

            [Fact]
            public void PushConfugiration_pushes_default_configuration_if_no_other_found()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                var mockFinder = new Mock<DbConfigurationFinder>();

                var manager = CreateManager(mockLoader, mockFinder);

                var defaultConfiguration = manager.GetConfiguration();

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                Assert.NotSame(defaultConfiguration, manager.GetConfiguration());
                mockLoader.Verify(m => m.TryLoadFromConfig(AppConfig.DefaultInstance));
                mockFinder.Verify(m => m.TryCreateConfiguration(It.IsAny<IEnumerable<Type>>()));
            }

            [Fact]
            public void AppConfigResolver_is_added_to_pushed_configuration()
            {
                var mockConfiguration = new Mock<DbConfiguration>();
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(mockConfiguration.Object);

                CreateManager(mockLoader).PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockConfiguration.Verify(m => m.AddAppConfigResolver(It.IsAny<AppConfigDependencyResolver>()));
            }

            [Fact]
            public void PushConfugiration_switches_in_original_root_resolver()
            {
                var mockConfiguration = new Mock<DbConfiguration>();
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(mockConfiguration.Object);

                var manager = CreateManager(mockLoader);
                var defaultConfiguration = manager.GetConfiguration();

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockConfiguration.Verify(m => m.SwitchInRootResolver(defaultConfiguration.RootResolver));
            }

            /// <summary>
            ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
            ///     issues. As with any test of this type just because the test passes does not mean that the code is
            ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
            ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
            /// </summary>
            [Fact]
            public void Configurations_can_be_pushed_and_popped_from_multiple_threads_concurrently()
            {
                for (var i = 0; i < 30; i++)
                {
                    var manager = new DbConfigurationManager(new DbConfigurationLoader(), new DbConfigurationFinder());
                    var config = manager.GetConfiguration();

                    ExecuteInParallel(
                        () =>
                            {
                                var appConfig = new AppConfig(new ConnectionStringSettingsCollection());
                                manager.PushConfiguration(appConfig, typeof(SimpleModelContext));
                                manager.PopConfiguration(appConfig);
                            });

                    Assert.Same(config, manager.GetConfiguration());
                }
            }
        }

        public class PopConfiguration : TestBase
        {
            [Fact]
            public void PopConfiguration_removes_the_first_configuration_associated_with_the_given_AppConfig()
            {
                var manager = CreateManager();
                var configuration = new Mock<DbConfiguration>
                                        {
                                            CallBase = true
                                        }.Object;

                var appConfig1 = AppConfig.DefaultInstance;
                var appConfig2 = new AppConfig(ConfigurationManager.ConnectionStrings);

                manager.SetConfiguration(configuration);

                manager.PushConfiguration(appConfig1, typeof(DbContext));
                var pushed1 = manager.GetConfiguration();
                manager.PushConfiguration(appConfig2, typeof(DbContext));

                manager.PopConfiguration(appConfig2);
                Assert.Same(pushed1, manager.GetConfiguration());

                manager.PopConfiguration(appConfig1);
                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void PopConfiguration_does_nothing_if_no_configuration_is_associated_with_the_given_AppConfig()
            {
                var manager = CreateManager();

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));
                var pushed1 = manager.GetConfiguration();

                manager.PopConfiguration(new AppConfig(ConfigurationManager.ConnectionStrings));
                Assert.Same(pushed1, manager.GetConfiguration());
            }
        }

        private static DbConfigurationManager CreateManager(
            Mock<DbConfigurationLoader> mockLoader = null,
            Mock<DbConfigurationFinder> mockFinder = null)
        {
            mockLoader = mockLoader ?? new Mock<DbConfigurationLoader>();
            mockFinder = mockFinder ?? new Mock<DbConfigurationFinder>();
            return new DbConfigurationManager(mockLoader.Object, mockFinder.Object);
        }

        public class FakeConfiguration : DbConfiguration
        {
        }

        public class FakeContext : DbContext
        {
        }

        private static void ConfigurationThreadTest(Action<DbConfigurationManager> beforeThreads, Action<DbConfigurationManager> inThreads)
        {
            for (var i = 0; i < 30; i++)
            {
                var configurationBag = new ConcurrentBag<DbConfiguration>();
                var manager = new DbConfigurationManager(new DbConfigurationLoader(), new DbConfigurationFinder());
                manager.SetConfiguration(new FunctionalTestsConfiguration());
                beforeThreads(manager);

                ExecuteInParallel(
                    () =>
                        {
                            inThreads(manager);
                            configurationBag.Add(manager.GetConfiguration());
                        });

                Assert.Equal(20, configurationBag.Count);
                Assert.True(configurationBag.All(c => manager.GetConfiguration() == c));
            }
        }
    }
}
