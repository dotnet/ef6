// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbProviderServicesTests
    {
        public class CreateCommandDefinition
        {
            [Fact]
            public void Dispatches_to_interception()
            {
                var dispatcher = new DbCommandTreeDispatcher();
                var mockCommandTreeInterceptor = new Mock<IDbCommandTreeInterceptor>();
                dispatcher.InternalDispatcher.Add(mockCommandTreeInterceptor.Object);

                var providerServices
                    = new Mock<DbProviderServices>(
                        (Func<IDbDependencyResolver>)(() => new Mock<IDbDependencyResolver>().Object),
                        new Lazy<DbCommandTreeDispatcher>(() => dispatcher))
                        {
                            CallBase = true
                        }.Object;

                var mockCommandTree
                    = new Mock<DbCommandTree>
                        {
                            DefaultValue = DefaultValue.Mock
                        };

                mockCommandTree.SetupGet(m => m.DataSpace).Returns(DataSpace.SSpace);

                var mockStoreItemCollection
                    = new Mock<StoreItemCollection>
                        {
                            DefaultValue = DefaultValue.Mock
                        };

                mockCommandTree
                    .Setup(m => m.MetadataWorkspace.GetItemCollection(DataSpace.SSpace))
                    .Returns(mockStoreItemCollection.Object);
                var commandTree = mockCommandTree.Object;

                providerServices.CreateCommandDefinition(commandTree);

                mockCommandTreeInterceptor.Verify(
                    m => m.TreeCreated(
                        It.Is<DbCommandTreeInterceptionContext>(c => c.Result == commandTree && c.OriginalResult == commandTree)), Times.Once());
            }
        }

        public class ExpandDataDirectory
        {
            [Fact]
            public void ExpandDataDirectory_returns_the_given_string_if_it_does_not_start_with_DataDirectory_ident()
            {
                Assert.Null(DbProviderServices.ExpandDataDirectory(null));
                Assert.Equal("", DbProviderServices.ExpandDataDirectory(""));
                Assert.Equal("It's Late", DbProviderServices.ExpandDataDirectory("It's Late"));
            }

            [Fact]
            public void ExpandDataDirectory_throws_if_expansion_is_needed_but_DataDirectory_is_not_a_string()
            {
                TestWithDataDirectory(
                    new Random(),
                    () => Assert.Equal(
                        Strings.ADP_InvalidDataDirectory,
                        Assert.Throws<InvalidOperationException>(() => DbProviderServices.ExpandDataDirectory("|DataDirectory|")).Message));
            }

            [Fact]
            public void ExpandDataDirectory_uses_app_domain_base_directory_if_data_directory_is_set_but_empty()
            {
                TestWithDataDirectory(
                    "",
                    () => Assert.Equal(
                        AppDomain.CurrentDomain.BaseDirectory + @"\",
                        DbProviderServices.ExpandDataDirectory("|DataDirectory|")));
            }

            [Fact]
            public void ExpandDataDirectory_uses_empty_string_if_DataDirectory_is_not_set()
            {
                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\", DbProviderServices.ExpandDataDirectory("|DataDirectory|")));
            }

            [Fact]
            public void ExpandDataDirectory_correctly_concatenates_paths_with_correct_number_of_slashes()
            {
                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () => Assert.Equal(@"C:\MelancholyBlues\", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|")));

                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    null,
                    () => Assert.Equal(@"\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () =>
                    Assert.Equal(
                        @"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () =>
                    Assert.Equal(
                        @"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () =>
                    Assert.Equal(
                        @"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () =>
                    Assert.Equal(
                        @"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));
            }

            [Fact]
            public void ExpandDataDirectory_throws_if_the_result_is_not_a_fully_expanded_path()
            {
                TestWithDataDirectory(
                    @"C:\MelancholyBlues\..\",
                    () => Assert.Equal(
                        Strings.ExpandingDataDirectoryFailed,
                        Assert.Throws<ArgumentException>(() => DbProviderServices.ExpandDataDirectory("|DataDirectory|")).Message));
            }

            private static void TestWithDataDirectory(object dataDirectory, Action test)
            {
                var previousDataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
                try
                {
                    AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
                    test();
                }
                finally
                {
                    AppDomain.CurrentDomain.SetData("DataDirectory", previousDataDirectory);
                }
            }
        }

        public class GetProviderServices : TestBase
        {
            [Fact]
            public void GetProviderServices_returns_SQL_Server_provider_by_convention()
            {
                Assert.Same(
                    SqlProviderServices.Instance,
                    DbProviderServices.GetProviderServices(new SqlConnection()));
            }

            [Fact]
            public void GetProviderServices_returns_provider_registered_in_app_config()
            {
                Assert.Same(
                    FakeSqlProviderServices.Instance,
                    DbProviderServices.GetProviderServices(new FakeSqlConnection()));
            }
        }

        public class GetExecutionStrategy : TestBase
        {
            [Fact]
            public void Static_method_returns_the_ExecutionStrategy_from_resolver()
            {
                var connectionMock = new Mock<DbConnection>();
                connectionMock.Setup(m => m.DataSource).Returns("FooSource");

                var model = EdmModel.CreateStoreModel(
                    new DbProviderInfo("System.Data.FakeSqlClient", "2008"),
                    new SqlProviderManifest("2008"));

                var storeItemCollectionMock = new Mock<StoreItemCollection>(model) { CallBase = true };

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollectionMock.Object);

                var entityConnection = new EntityConnection(
                    workspace: null, connection: connectionMock.Object, skipInitialization: true, entityConnectionOwnsStoreConnection: false);

                var mockExecutionStrategy = new Mock<IDbExecutionStrategy>().Object;
                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(
                    k =>
                        {
                            var key = k as ExecutionStrategyKey;
                            Assert.Equal("System.Data.FakeSqlClient", key.ProviderInvariantName);
                            Assert.Equal("FooSource", key.ServerName);
                            return (Func<IDbExecutionStrategy>)(() => mockExecutionStrategy);
                        });

                var providerFactoryServiceMock = new Mock<IDbProviderFactoryResolver>();
                providerFactoryServiceMock.Setup(m => m.ResolveProviderFactory(It.IsAny<DbConnection>()))
                    .Returns(FakeSqlProviderFactory.Instance);

                MutableResolver.AddResolver<IDbProviderFactoryResolver>(k => providerFactoryServiceMock.Object);
                try
                {
                    Assert.Same(mockExecutionStrategy, DbProviderServices.GetExecutionStrategy(connectionMock.Object));
                    Assert.Same(
                        mockExecutionStrategy, DbProviderServices.GetExecutionStrategy(entityConnection, metadataWorkspaceMock.Object));
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }
            }
        }

        public class GetSpatialServices
        {
            [Fact]
            public void GetSpatialServices_uses_resolver_to_obtain_provider_specific_spatial_services()
            {
                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();
                var key = new DbProviderInfo("Pefect.Day", "Lou");
                mockResolver.Setup(m => m.GetService(typeof(DbSpatialServices), key)).Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object,
                    new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object)).Object.GetSpatialServices(key));
            }

            [Fact]
            public void GetSpatialServices_gets_services_from_provider_if_resolver_returns_null_for_provider_specific_services()
            {
                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "Walk")
                    .Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object,
                    testProvider.Object.GetSpatialServices(new DbProviderInfo("Wild.Side", "Walk")));
            }

            [Fact]
            public void GetSpatialServices_uses_default_spatial_services_if_no_provider_specific_services_found()
            {
                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();
                mockResolver.Setup(m => m.GetService(typeof(DbSpatialServices), null)).Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object,
                    new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object)).Object
                        .GetSpatialServices(new DbProviderInfo("Satellite.Of", "Love")));
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "Love")
                    .Throws(new Exception("Fail"));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(
                        () => testProvider.Object
                                  .GetSpatialServices(new DbProviderInfo("Andy's.Chest", "Love"))).Message);
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws_ProviderIncompatibleException()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "Love")
                    .Throws(new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(
                        () => testProvider.Object
                                  .GetSpatialServices(new DbProviderInfo("HanginRound", "Love"))).Message);
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_returns_null()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "Love")
                    .Returns((DbSpatialServices)null);

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(
                        () => testProvider.Object
                                  .GetSpatialServices(new DbProviderInfo("Make.Up", "Love"))).Message);
            }

            [Fact]
            public void Obsolete_GetSpatialServices_gets_services_from_provider_evem_if_resolver_is_configured()
            {
                var mockResolverServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();
                mockResolver.Setup(
                    m => m.GetService(
                        typeof(DbSpatialServices),
                        new DbProviderInfo("Wagon.Wheel", "Lou"))).Returns(mockResolverServices.Object);

                var mockProviderServices = new Mock<DbSpatialServices>();
                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "Lou")
                    .Returns(mockProviderServices.Object);

#pragma warning disable 612,618
                Assert.Same(mockProviderServices.Object, testProvider.Object.GetSpatialServices("Lou"));
#pragma warning restore 612,618
            }

            [Fact]
            public void Obsolete_GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws()
            {
                var testProvider = new Mock<DbProviderServices>();
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Throws(new Exception("Fail"));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
#pragma warning disable 612,618
                    Assert.Throws<ProviderIncompatibleException>(() => testProvider.Object.GetSpatialServices("X")).Message);
#pragma warning restore 612,618
            }

            [Fact]
            public void Obsolete_GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws_ProviderIncompatibleException()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Throws(new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
#pragma warning disable 612,618
                    Assert.Throws<ProviderIncompatibleException>(() => testProvider.Object.GetSpatialServices("X")).Message);
#pragma warning restore 612,618
            }

            [Fact]
            public void Obsolete_GetSpatialServices_returns_null_if_resolver_returns_null_and_provider_returns_null()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>((Func<IDbDependencyResolver>)(() => mockResolver.Object));
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Returns((DbSpatialServices)null);

#pragma warning disable 612,618
                Assert.Null(testProvider.Object.GetSpatialServices("X"));
#pragma warning restore 612,618
            }

            [Fact]
            public void GetSpatialServices_with_EntityConnection_uses_store_manifest_to_build_key()
            {
                var mockItemCollection = new Mock<StoreItemCollection>();
                mockItemCollection.Setup(m => m.ProviderInvariantName).Returns("New.York");
                mockItemCollection.Setup(m => m.ProviderManifestToken).Returns("Conversation");

                var mockWorkspace = new Mock<MetadataWorkspace>();
                mockWorkspace.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(mockItemCollection.Object);

                var mockConnection = new Mock<EntityConnection>();
                mockConnection.Setup(m => m.GetMetadataWorkspace()).Returns(mockWorkspace.Object);

                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();
                mockResolver.Setup(
                    m => m.GetService(
                        typeof(DbSpatialServices),
                        new DbProviderInfo("New.York", "Conversation"))).Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object,
                    DbProviderServices.GetSpatialServices(mockResolver.Object, mockConnection.Object));
            }

            [Fact]
            public void GetSpatialServices_with_EntityConnection_uses_provider_from_store_connection_if_resolver_returns_null()
            {
                var mockItemCollection = new Mock<StoreItemCollection>();
                mockItemCollection.Setup(m => m.ProviderInvariantName).Returns("I'm.So.Free");
                mockItemCollection.Setup(m => m.ProviderManifestToken).Returns("2008");

                var mockWorkspace = new Mock<MetadataWorkspace>();
                mockWorkspace.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(mockItemCollection.Object);

                var mockConnection = new Mock<EntityConnection>();
                mockConnection.Setup(m => m.GetMetadataWorkspace()).Returns(mockWorkspace.Object);
                mockConnection.Setup(m => m.StoreConnection).Returns(new SqlConnection());

                Assert.Same(
                    SqlSpatialServices.Instance,
                    DbProviderServices.GetSpatialServices(new Mock<IDbDependencyResolver>().Object, mockConnection.Object));
            }
        }

        public class GetConceptualSchemaDefinition
        {
            [Fact]
            public void GetConceptualSchemaDefinition_throws_ArgumentNullException_for_null_or_empty_resource_name()
            {
                foreach (var csdlName in new[] { null, string.Empty })
                {
                    Assert.Throws<ArgumentException>(
                        () => DbProviderServices.GetConceptualSchemaDefinition(csdlName));
                }
            }

            [Fact]
            public void GetConceptualSchemaDefinition_throws_ArgumentException_for_invalid_resource_name()
            {
                Assert.Equal(
                    string.Format(Strings.InvalidResourceName("resource")),
                    Assert.Throws<ArgumentException>(
                        () => DbProviderServices.GetXmlResource("resource")).Message);
            }

            [Fact]
            public void GetConceptualSchemaDefinition_returns_non_null_XmlReader_for_valid_resource_names()
            {
                foreach (var csdlName in new[] { "ConceptualSchemaDefinition", "ConceptualSchemaDefinitionVersion3" })
                {
                    using (var reader = DbProviderServices.GetConceptualSchemaDefinition(csdlName))
                    {
                        Assert.NotNull(reader);
                    }
                }
            }
        }

        public class GetService
        {
            [Fact]
            public void GetService_returns_null()
            {
                Assert.Null(new FakeSqlProviderServices().GetService(null, null));
            }

            [Fact]
            public void GetService_returns_services_registered_with_AddDependencyResolver()
            {
                var services = new FakeSqlProviderServices();
                services.AddDependencyResolver(new SingletonDependencyResolver<string>("Cheese", "Please"));

                Assert.Equal("Cheese", services.GetService<string>("Please"));
            }
        }

        public class GetServices
        {
            [Fact]
            public void GetServices_returns_empty_list()
            {
                Assert.Empty(new FakeSqlProviderServices().GetServices(null, null));
            }

            [Fact]
            public void GetServices_returns_services_registered_with_AddDependencyResolver()
            {
                var services = new FakeSqlProviderServices();
                services.AddDependencyResolver(new SingletonDependencyResolver<string>("Cheese", "Please"));

                Assert.Equal("Cheese", services.GetServices<string>("Please").Single());
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_passed_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new FakeSqlProviderServices().AddDependencyResolver(null)).ParamName);
            }
        }
    }
}
