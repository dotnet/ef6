// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common
{
    using System.Data.Common;

    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common.CommandTrees;

    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;

    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbProviderServicesTests
    {
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
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|SheerHeartAttack")));

                TestWithDataDirectory(
                    @"C:\MelancholyBlues\",
                    () => Assert.Equal(@"C:\MelancholyBlues\SheerHeartAttack", DbProviderServices.ExpandDataDirectory(@"|DataDirectory|\SheerHeartAttack")));
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

        public class GetProviderServices
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
                var mockConnection = new Mock<DbConnection>();
                mockConnection.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(FakeSqlProviderFactory.Instance);

                Assert.Same(
                    FakeSqlProviderServices.Instance,
                    DbProviderServices.GetProviderServices(mockConnection.Object));
            }
        }

        public class GetSpatialServices
        {
            [Fact]
            public void GetSpatialServices_uses_resolver_to_obtain_spatial_services()
            {
                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();
                mockResolver
                    .Setup(m => m.GetService(typeof(DbSpatialServices), It.IsAny<string>()))
                    .Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object, 
                    new Mock<DbProviderServices>(mockResolver.Object).Object.GetSpatialServices("X"));
            }

            [Fact]
            public void GetSpatialServices_gets_services_from_provider_if_resolver_returns_null()
            {
                var mockSpatialServices = new Mock<DbSpatialServices>();
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>(mockResolver.Object);
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Returns(mockSpatialServices.Object);

                Assert.Same(
                    mockSpatialServices.Object,
                    testProvider.Object.GetSpatialServices("X"));
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>(mockResolver.Object);
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Throws(new Exception("Fail"));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(() => testProvider.Object.GetSpatialServices("X")).Message);
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_throws_ProviderIncompatibleException()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>(mockResolver.Object);
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Throws(new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices));

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(() => testProvider.Object.GetSpatialServices("X")).Message);
            }

            [Fact]
            public void GetSpatialServices_throws_if_resolver_returns_null_and_provider_returns_null()
            {
                var mockResolver = new Mock<IDbDependencyResolver>();

                var testProvider = new Mock<DbProviderServices>(mockResolver.Object);
                testProvider.Protected()
                    .Setup<DbSpatialServices>("DbGetSpatialServices", "X")
                    .Returns((DbSpatialServices)null);

                Assert.Equal(
                    Strings.ProviderDidNotReturnSpatialServices,
                    Assert.Throws<ProviderIncompatibleException>(() => testProvider.Object.GetSpatialServices("X")).Message);
            }
        }
    }
}
