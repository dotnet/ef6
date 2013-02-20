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
                var mockInternalConfiguration = new Mock<InternalConfiguration>
                    {
                        CallBase = true
                    };

                var mockDbConfiguration = new Mock<DbConfiguration>();
                mockDbConfiguration.Setup(m => m.InternalConfiguration).Returns(mockInternalConfiguration.Object);
                mockInternalConfiguration.Setup(m => m.Owner).Returns(mockDbConfiguration.Object);

                var configuration = mockInternalConfiguration.Object;

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
                var mockInternalConfiguration = CreateMockInternalConfiguration();

                manager.SetConfiguration(mockInternalConfiguration.Object);

                Assert.Same(mockInternalConfiguration.Object, manager.GetConfiguration());
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
                var mockInternalConfiguration = CreateMockInternalConfiguration();

                manager.SetConfiguration(mockInternalConfiguration.Object);

                Assert.Same(mockInternalConfiguration.Object, manager.GetConfiguration());

                mockInternalConfiguration.Verify(m => m.Lock());
            }

            [Fact]
            public void The_same_type_of_configuration_can_be_set_multiple_times()
            {
                var manager = CreateManager();
                var mockInternalConfiguration1 = CreateMockInternalConfiguration();
                var mockInternalConfiguration2 = CreateMockInternalConfiguration();

                manager.SetConfiguration(mockInternalConfiguration1.Object);
                manager.SetConfiguration(mockInternalConfiguration2.Object);

                Assert.Same(mockInternalConfiguration1.Object, manager.GetConfiguration());
            }

            [Fact]
            public void SetConfiguration_discards_the_given_configuration_and_uses_the_configuration_from_the_config_file_if_it_exists()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);
                mockLoader.Setup(m => m.TryLoadFromConfig(It.IsAny<AppConfig>())).Returns(typeof(FakeConfiguration));

                var manager = CreateManager(mockLoader);
                var mockConfiguration = CreateMockInternalConfiguration();

                manager.SetConfiguration(mockConfiguration.Object);

                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
                Assert.NotSame(mockConfiguration, manager.GetConfiguration());

                AssertIsLocked(manager.GetConfiguration());
                AssertIsNotLocked(mockConfiguration.Object);
            }

            [Fact]
            public void SetConfiguration_throws_if_an_attempt_is_made_to_set_a_different_configuration_type()
            {
                var manager = CreateManager();

                var configuration1 = new FakeConfiguration();
                var mockInternalConfiguration1 = CreateMockInternalConfiguration(configuration1);

                var configuration2 = new Mock<DbConfiguration>().Object;
                var mockInternalConfiguration2 = CreateMockInternalConfiguration(configuration2);

                manager.SetConfiguration(mockInternalConfiguration1.Object);

                Assert.Equal(
                    Strings.ConfigurationSetTwice(configuration2.GetType().Name, configuration1.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => manager.SetConfiguration(mockInternalConfiguration2.Object)).Message);
            }

            [Fact]
            public void SetConfiguration_throws_if_an_attempt_is_made_to_set_a_configuration_after_the_default_has_already_been_used()
            {
                var manager = CreateManager();

                var dbConfiguration = new Mock<DbConfiguration>().Object;
                var mockInternalConfiguration = CreateMockInternalConfiguration(dbConfiguration);

                manager.GetConfiguration(); // Initialize default

                Assert.Equal(
                    Strings.DefaultConfigurationUsedBeforeSet(dbConfiguration.GetType().Name),
                    Assert.Throws<InvalidOperationException>(() => manager.SetConfiguration(mockInternalConfiguration.Object)).Message);
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
                    m => m.SetConfiguration(new FunctionalTestsConfiguration().InternalConfiguration));
            }
        }

        public class EnsureLoadedForContext : TestBase
        {
            [Fact]
            public void EnsureLoadedForContext_loads_configuration_from_context_assembly_if_none_was_previously_used()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>())).Returns(
                    typeof(FakeConfiguration));

                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));
                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
            }

            [Fact]
            public void EnsureLoadedForContext_doesnt_get_recursively_called_even_if_config_constructor_calls_EnsureLoadedForContext()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>())).Returns(
                    typeof(FakeConfigurationWithEnsures));

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));
                Assert.IsType<FakeConfigurationWithEnsures>(manager.GetConfiguration().Owner);
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_given_context_is_exactly_DbContext()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();

                CreateManager(null, mockFinder).EnsureLoadedForContext(typeof(DbContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()), Times.Never());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_assembly_has_already_been_checked()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()), Times.Once());

                manager.EnsureLoadedForContext(typeof(FakeContext));

                // Finder has not been used again
                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()), Times.Once());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_an_override_configuration_has_been_pushed()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()), Times.Once());

                manager.EnsureLoadedForContext(typeof(FakeContext));

                // Finder has not been used again
                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()), Times.Once());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_none_was_previously_used_and_no_configuration_is_found_in_assembly()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));

                var mockInternalConfiguration = CreateMockInternalConfiguration();
                manager.SetConfiguration(mockInternalConfiguration.Object);

                Assert.Same(mockInternalConfiguration.Object, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_one_was_previously_used_but_no_configuration_is_found_in_assembly()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                var configuration = manager.GetConfiguration();

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_one_was_previously()
            {
                var mockFinder = new Mock<DbConfigurationFinder>();

                var manager = CreateManager(null, mockFinder);

                var configuration = manager.GetConfiguration();

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_configuration_in_assembly_is_the_same_as_was_previously_used()
            {
                var mockInternalConfiguration = CreateMockInternalConfiguration();

                var configuration = mockInternalConfiguration.Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()))
                          .Returns(configuration.Owner.GetType());
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(configuration);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_throws_if_found_configuration_does_not_match_previously_used_configuration()
            {
                var mockInternalConfiguration = CreateMockInternalConfiguration();

                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()))
                          .Returns(typeof(FakeConfiguration));
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.SetConfigurationNotDiscovered(mockInternalConfiguration.Object.Owner.GetType().Name, typeof(FakeContext).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => manager.EnsureLoadedForContext(typeof(FakeContext))).Message);
            }

            [Fact]
            public void EnsureLoadedForContext_throws_if_configuration_is_found_but_default_was_previously_used()
            {
                var configuration = new Mock<DbConfiguration>().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(FakeContext), It.IsAny<IEnumerable<Type>>()))
                          .Returns(configuration.GetType());
                var manager = CreateManager(null, mockFinder);

                manager.GetConfiguration();

                Assert.Equal(
                    Strings.ConfigurationNotDiscovered(configuration.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () => manager.EnsureLoadedForContext(typeof(FakeContext))).Message);
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_configuration_was_set_but_is_not_found_in_context_assembly()
            {
                var configuration = CreateMockInternalConfiguration().Object;
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(null, mockFinder);

                manager.SetConfiguration(configuration);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                Assert.Same(configuration, manager.GetConfiguration());
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_if_configuration_is_specified_in_config_file()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);
                mockLoader.Setup(m => m.TryLoadFromConfig(It.IsAny<AppConfig>())).Returns(typeof(FakeConfiguration));

                var mockInternalConfiguration = CreateMockInternalConfiguration();
                var mockFinder = new Mock<DbConfigurationFinder>();
                var manager = CreateManager(mockLoader, mockFinder);

                manager.SetConfiguration(mockInternalConfiguration.Object);

                manager.EnsureLoadedForContext(typeof(FakeContext));
                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
            }

            [Fact]
            public void EnsureLoadedForContext_does_not_throw_even_if_finder_would_throw_if_configuration_is_specified_in_config_file()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);
                mockLoader.Setup(m => m.TryLoadFromConfig(It.IsAny<AppConfig>())).Returns(typeof(FakeConfiguration));

                var mockInternalConfiguration = CreateMockInternalConfiguration();

                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder
                    .Setup(m => m.TryFindConfigurationType(It.IsAny<Type>(), It.IsAny<IEnumerable<Type>>()))
                    .Throws<InvalidOperationException>();

                var manager = CreateManager(mockLoader, mockFinder);
                manager.SetConfiguration(mockInternalConfiguration.Object);

                manager.EnsureLoadedForContext(typeof(FakeContext));

                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
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
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(typeof(FakeConfiguration));
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);
                var mockFinder = new Mock<DbConfigurationFinder>();

                var manager = CreateManager(mockLoader, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
                AssertIsLocked(manager.GetConfiguration());
                mockLoader.Verify(m => m.TryLoadFromConfig(AppConfig.DefaultInstance));
                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()), Times.Never());
            }

            [Fact]
            public void PushConfugiration_pushes_and_locks_configuration_discovered_in_context_assembly_if_found()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                var mockFinder = new Mock<DbConfigurationFinder>();
                mockFinder.Setup(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>())).Returns(
                    typeof(FakeConfiguration));

                var manager = CreateManager(mockLoader, mockFinder);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                Assert.IsType<FakeConfiguration>(manager.GetConfiguration().Owner);
                AssertIsLocked(manager.GetConfiguration());
                mockLoader.Verify(m => m.TryLoadFromConfig(AppConfig.DefaultInstance));
                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()));
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
                mockFinder.Verify(m => m.TryFindConfigurationType(typeof(DbContext), It.IsAny<IEnumerable<Type>>()));
            }

            internal class DbConfigurationWithMockInternals : DbConfiguration
            {
                private readonly InternalConfiguration _internalConfiguration = new Mock<InternalConfiguration>().Object;

                internal override InternalConfiguration InternalConfiguration
                {
                    get { return _internalConfiguration; }
                }
            }

            [Fact]
            public void AppConfigResolver_is_added_to_pushed_configuration()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(typeof(DbConfigurationWithMockInternals));
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);
                var manager = CreateManager(mockLoader);

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                Mock.Get(manager.GetConfiguration()).Verify(m => m.AddAppConfigResolver(It.IsAny<AppConfigDependencyResolver>()));
            }

            [Fact]
            public void PushConfugiration_switches_in_original_root_resolver()
            {
                var mockLoader = new Mock<DbConfigurationLoader>();
                mockLoader.Setup(m => m.TryLoadFromConfig(AppConfig.DefaultInstance)).Returns(typeof(DbConfigurationWithMockInternals));
                mockLoader.Setup(m => m.AppConfigContainsDbConfigurationType(It.IsAny<AppConfig>())).Returns(true);

                var manager = CreateManager(mockLoader);
                var defaultConfiguration = manager.GetConfiguration();

                manager.PushConfiguration(AppConfig.DefaultInstance, typeof(DbContext));

                Mock.Get(manager.GetConfiguration()).Verify(m => m.SwitchInRootResolver(defaultConfiguration.RootResolver));
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
                var mockInternalConfiguration = new Mock<InternalConfiguration>
                    {
                        CallBase = true
                    };

                var mockDbConfiguration = new Mock<DbConfiguration>();
                mockDbConfiguration.Setup(m => m.InternalConfiguration).Returns(mockInternalConfiguration.Object);
                mockInternalConfiguration.Setup(m => m.Owner).Returns(mockDbConfiguration.Object);

                var appConfig1 = AppConfig.DefaultInstance;
                var appConfig2 = new AppConfig(ConfigurationManager.ConnectionStrings);

                manager.SetConfiguration(mockInternalConfiguration.Object);

                manager.PushConfiguration(appConfig1, typeof(DbContext));
                var pushed1 = manager.GetConfiguration();
                manager.PushConfiguration(appConfig2, typeof(DbContext));

                manager.PopConfiguration(appConfig2);
                Assert.Same(pushed1, manager.GetConfiguration());

                manager.PopConfiguration(appConfig1);
                Assert.Same(mockInternalConfiguration.Object, manager.GetConfiguration());
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

        public class AddOnLockingHandler
        {
            [Fact]
            public void Adding_handler_after_configuration_is_in_use_throws()
            {
                var manager = CreateManager();
                manager.GetConfiguration();

                Assert.Equal(
                    Strings.AddHandlerToInUseConfiguration,
                    Assert.Throws<InvalidOperationException>(() => manager.AddOnLockingHandler((_, __) => { })).Message);
            }
        }

        public class OnLocking
        {
            private readonly IDbDependencyResolver _snapshot;
            private readonly Mock<InternalConfiguration> _configuration;

            public OnLocking()
            {
                _snapshot = new Mock<IDbDependencyResolver>().Object;
                _configuration = CreateMockInternalConfiguration(null, _snapshot);
            }

            [Fact]
            public void OnLocking_calls_all_added_handlers_and_passes_in_correct_configuration()
            {
                var manager = CreateManager();

                manager.OnLocking(_configuration.Object); // No throw when no handlers registered

                manager.AddOnLockingHandler(Handler1);
                manager.AddOnLockingHandler(Handler2);
                manager.AddOnLockingHandler(Handler3);

                manager.OnLocking(_configuration.Object);
                Assert.Equal(1, Handler1Called);
                Assert.Equal(1, Handler2Called);
                Assert.Equal(1, Handler3Called);

                manager.RemoveOnLockingHandler(Handler2);

                manager.OnLocking(_configuration.Object);
                Assert.Equal(2, Handler1Called);
                Assert.Equal(1, Handler2Called);
                Assert.Equal(2, Handler3Called);
            }

            private int Handler1Called { get; set; }

            private void Handler1(object sender, DbConfigurationEventArgs args)
            {
                Assert.Same(_configuration.Object.Owner, sender);
                Assert.Same(_snapshot, args.ResolverSnapshot);
                Handler1Called++;
            }

            private int Handler2Called { get; set; }

            private void Handler2(object sender, DbConfigurationEventArgs args)
            {
                Assert.Same(_configuration.Object.Owner, sender);
                Assert.Same(_snapshot, args.ResolverSnapshot);
                Handler2Called++;
            }

            private int Handler3Called { get; set; }

            private void Handler3(object sender, DbConfigurationEventArgs args)
            {
                Assert.Same(_configuration.Object.Owner, sender);
                Assert.Same(_snapshot, args.ResolverSnapshot);
                Handler3Called++;
            }
        }

        private static DbConfigurationManager CreateManager(
            Mock<DbConfigurationLoader> mockLoader = null,
            Mock<DbConfigurationFinder> mockFinder = null)
        {
            return new DbConfigurationManager(
                (mockLoader ?? new Mock<DbConfigurationLoader>()).Object,
                (mockFinder ?? new Mock<DbConfigurationFinder>()).Object);
        }

        public class FakeConfiguration : DbConfiguration
        {
        }

        public class FakeConfigurationWithEnsures : DbConfiguration
        {
            public FakeConfigurationWithEnsures()
            {
                DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(FakeContext));
            }
        }

        public class FakeContext : DbContext
        {
        }

        private static Mock<InternalConfiguration> CreateMockInternalConfiguration(
            DbConfiguration dbConfiguration = null, IDbDependencyResolver snapshot = null)
        {
            var mockInternalConfiguration = new Mock<InternalConfiguration>();

            if (dbConfiguration == null)
            {
                var mockDbConfiguration = new Mock<DbConfiguration>();
                mockDbConfiguration.Setup(m => m.InternalConfiguration).Returns(mockInternalConfiguration.Object);
                dbConfiguration = mockDbConfiguration.Object;
            }

            mockInternalConfiguration.Setup(m => m.Owner).Returns(dbConfiguration);
            mockInternalConfiguration.Setup(m => m.ResolverSnapshot).Returns(snapshot);

            return mockInternalConfiguration;
        }

        private static void ConfigurationThreadTest(Action<DbConfigurationManager> beforeThreads, Action<DbConfigurationManager> inThreads)
        {
            for (var i = 0; i < 30; i++)
            {
                var configurationBag = new ConcurrentBag<InternalConfiguration>();
                var manager = new DbConfigurationManager(new DbConfigurationLoader(), new DbConfigurationFinder());
                manager.SetConfiguration(new FunctionalTestsConfiguration().InternalConfiguration);
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

        private static void AssertIsLocked(InternalConfiguration internalConfiguration)
        {
            Assert.Throws<InvalidOperationException>(() => internalConfiguration.CheckNotLocked("Foo"));
        }

        private static void AssertIsNotLocked(InternalConfiguration internalConfiguration)
        {
            Assert.DoesNotThrow(() => internalConfiguration.CheckNotLocked("Foo"));
        }
    }
}
