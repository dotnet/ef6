// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class InitializerConfigTests
    {
        [Fact]
        public void TryGetInitializer_returns_null_when_no_initializer_is_registered_for_context()
        {
            Assert.Null(
                new InitializerConfig(new EntityFrameworkSection(), new KeyValueConfigurationCollection())
                    .TryGetInitializer(typeof(DbContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_null_when_context_section_exists_but_no_initializer_is_registered()
        {
            var contextElements = new List<ContextElement>
                {
                    new ContextElement()
                };
            var mockContextCollection = new Mock<ContextCollection>();
            mockContextCollection.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(contextElements.GetEnumerator());

            var mockEfSection = new Mock<EntityFrameworkSection>();
            mockEfSection.Setup(m => m.Contexts).Returns(mockContextCollection.Object);

            Assert.Null(
                new InitializerConfig(mockEfSection.Object, new KeyValueConfigurationCollection())
                    .TryGetInitializer(typeof(DbContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_NullInitializer_when_initializer_is_disabled_in_context_section()
        {
            Assert.IsType<NullDatabaseInitializer<FakeContext>>(
                new InitializerConfig(
                    CreateEfSection(typeof(FakeContext).AssemblyQualifiedName, null, initializerDisabled: true),
                    new KeyValueConfigurationCollection())
                    .TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_initializer_registered_in_config_section()
        {
            Assert.IsType<FakeInitializer<FakeContext>>(
                new InitializerConfig(
                    CreateEfSection(
                        typeof(FakeContext).AssemblyQualifiedName,
                        typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName,
                        initializerDisabled: false),
                    new KeyValueConfigurationCollection()).TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_initializer_created_with_given_parameters_when_registered_in_config_section()
        {
            var initializer = new InitializerConfig(
                CreateEfSection(
                    typeof(FakeContext).AssemblyQualifiedName,
                    typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName,
                    initializerDisabled: false,
                    initalizerParams: new object[] { "Wiggo", 1 }),
                new KeyValueConfigurationCollection()).TryGetInitializer(typeof(FakeContext));

            Assert.IsType<FakeInitializer<FakeContext>>(initializer);
            Assert.Equal("Wiggo", ((FakeInitializer<FakeContext>)initializer).Hero);
            Assert.Equal(1, ((FakeInitializer<FakeContext>)initializer).Wheredecome);
        }

        [Fact]
        public void TryGetInitializer_returns_NullInitializer_when_initializer_is_disabled_in_app_settings_section()
        {
            Assert.IsType<NullDatabaseInitializer<FakeContext>>(
                new InitializerConfig(
                    new EntityFrameworkSection(), new KeyValueConfigurationCollection
                        {
                            { "DatabaseInitializerForType " + typeof(FakeContext).AssemblyQualifiedName, "Disabled" }
                        })
                    .TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_NullInitializer_when_initializer_is_disabled_with_empty_string_in_app_settings_section()
        {
            Assert.IsType<NullDatabaseInitializer<FakeContext>>(
                new InitializerConfig(
                    new EntityFrameworkSection(), new KeyValueConfigurationCollection
                        {
                            { "DatabaseInitializerForType " + typeof(FakeContext).AssemblyQualifiedName, "" }
                        })
                    .TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_initializer_registered_in_app_settings_section()
        {
            Assert.IsType<FakeInitializer<FakeContext>>(
                new InitializerConfig(
                    new EntityFrameworkSection(), new KeyValueConfigurationCollection
                        {
                            {
                                "DatabaseInitializerForType " + typeof(FakeContext).AssemblyQualifiedName,
                                typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName
                            }
                        })
                    .TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_returns_initializer_set_in_context_section_in_preference_to_app_settings_section()
        {
            Assert.IsType<FakeInitializer<FakeContext>>(
                new InitializerConfig(
                    CreateEfSection(
                        typeof(FakeContext).AssemblyQualifiedName,
                        typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName,
                        initializerDisabled: false),
                    new KeyValueConfigurationCollection
                        {
                            {
                                "DatabaseInitializerForType " + typeof(FakeContext).AssemblyQualifiedName,
                                "Not.Me.Please"
                            }
                        }).TryGetInitializer(typeof(FakeContext)));
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_context_type_is_registered_in_context_section()
        {
            TestBadType("A.Bad.Context.Type", typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName, initializerDisabled: false);
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_context_type_for_disabled_initializer_is_registered_in_context_section()
        {
            TestBadType("A.Bad.Context.Type", typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName, initializerDisabled: true);
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_initializer_type_is_registered_in_context_section()
        {
            TestBadType(typeof(FakeContext).AssemblyQualifiedName, "A.Bad.Initializer.Type", initializerDisabled: false);
        }

        private static void TestBadType(string contextTypeName, string initializerTypeName, bool initializerDisabled)
        {
            var initializerConfig = new InitializerConfig(
                CreateEfSection(contextTypeName, initializerTypeName, initializerDisabled),
                new KeyValueConfigurationCollection());

            var exception = Assert.Throws<InvalidOperationException>(() => initializerConfig.TryGetInitializer(typeof(FakeContext)));

            Assert.Equal(
                Strings.Database_InitializeFromConfigFailed(
                    initializerDisabled ? "Disabled" : initializerTypeName,
                    contextTypeName), exception.Message);

            Assert.IsType<TypeLoadException>(exception.InnerException);
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_context_type_is_registered_in_app_settings_section()
        {
            TestBadTypeLegacy("A.Bad.Context.Type", typeof(FakeInitializer<FakeContext>).AssemblyQualifiedName, initializerDisabled: false);
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_context_type_for_disabled_initializer_is_registered_in_app_settings_section()
        {
            TestBadTypeLegacy("A.Bad.Context.Type", "Disabled", initializerDisabled: true);
        }

        [Fact]
        public void
            TryGetInitializer_throws_if_bad_context_type_for_disabled_initializer_by_empty_string_is_registered_in_app_settings_section()
        {
            TestBadTypeLegacy("A.Bad.Context.Type", "", initializerDisabled: true);
        }

        [Fact]
        public void TryGetInitializer_throws_if_bad_initializer_type_is_registered_in_app_settings_section()
        {
            TestBadTypeLegacy(typeof(FakeContext).AssemblyQualifiedName, "A.Bad.Initializer.Type", initializerDisabled: false);
        }

        private static void TestBadTypeLegacy(string contextTypeName, string configValue, bool initializerDisabled)
        {
            var configurationCollection = new KeyValueConfigurationCollection
                {
                    { "DatabaseInitializerForType " + contextTypeName, configValue }
                };

            var initializerConfig = new InitializerConfig(new EntityFrameworkSection(), configurationCollection);

            var exception = Assert.Throws<InvalidOperationException>(() => initializerConfig.TryGetInitializer(typeof(FakeContext)));

            Assert.Equal(
                Strings.Database_InitializeFromLegacyConfigFailed(
                    initializerDisabled ? "Disabled" : configValue,
                    contextTypeName), exception.Message);

            Assert.IsType<TypeLoadException>(exception.InnerException);
        }

        [Fact]
        public void TryGetInitializer_throws_if_null_or_empty_context_type_is_registered_in_app_settings_section()
        {
            var configurationCollection = new KeyValueConfigurationCollection
                {
                    { "DatabaseInitializerForType ", "Disabled" }
                };

            var initializerConfig = new InitializerConfig(new EntityFrameworkSection(), configurationCollection);

            Assert.Equal(
                Strings.Database_BadLegacyInitializerEntry("DatabaseInitializerForType ", "Disabled"),
                Assert.Throws<InvalidOperationException>(() => initializerConfig.TryGetInitializer(typeof(FakeContext))).Message);
        }

        private static EntityFrameworkSection CreateEfSection(
            string contextTypeName, string initializerTypeName, bool initializerDisabled, object[] initalizerParams = null)
        {
            var mockParameterCollection = new Mock<ParameterCollection>();
            mockParameterCollection.Setup(m => m.GetTypedParameterValues()).Returns(initalizerParams);

            var mockDatabaseInitializerElement = new Mock<DatabaseInitializerElement>();
            mockDatabaseInitializerElement.Setup(m => m.InitializerTypeName).Returns(initializerTypeName);
            mockDatabaseInitializerElement.Setup(m => m.Parameters).Returns(mockParameterCollection.Object);

            var mockContextElement = new Mock<ContextElement>();
            mockContextElement.Setup(m => m.IsDatabaseInitializationDisabled).Returns(initializerDisabled);
            mockContextElement.Setup(m => m.ContextTypeName).Returns(contextTypeName);
            mockContextElement.Setup(m => m.DatabaseInitializer).Returns(mockDatabaseInitializerElement.Object);

            var mockContextCollection = new Mock<ContextCollection>();
            mockContextCollection.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(
                new List<ContextElement>
                    {
                        mockContextElement.Object
                    }.GetEnumerator());

            var mockEfSection = new Mock<EntityFrameworkSection>();
            mockEfSection.Setup(m => m.Contexts).Returns(mockContextCollection.Object);

            return mockEfSection.Object;
        }

        public class FakeContext : DbContext
        {
        }

        public class FakeInitializer<TContext> : IDatabaseInitializer<TContext>
            where TContext : DbContext
        {
            public FakeInitializer()
            {
            }

            public FakeInitializer(string hero, int wheredecome)
            {
                Hero = hero;
                Wheredecome = wheredecome;
            }

            public string Hero { get; set; }
            public int Wheredecome { get; set; }

            public void InitializeDatabase(TContext context)
            {
            }
        }
    }
}
