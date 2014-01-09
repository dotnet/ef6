// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbProviderServicesExtensionsTests
    {
        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_MVC4_connection_string_is_used()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=(localdb)\v11.0", Strings.BadLocalDBDatabaseName);
        }

        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_general_bad_connection_string_is_used()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=WotNoServer", Strings.FailedToGetProviderInformation);
        }

        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_correct_LocalDB_name_is_used_but_it_still_fails()
        {
            Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                "Data Source=(localdb)\\v11.0", Strings.FailedToGetProviderInformation);
        }

        [Fact]
        public void GetProviderManifestTokenChecked_uses_interception()
        {
            var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
            DbInterception.Add(dbConnectionInterceptorMock.Object);
            try
            {
                Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
                    "Data Source=WotNoServer", Strings.FailedToGetProviderInformation);
            }
            finally
            {
                DbInterception.Remove(dbConnectionInterceptorMock.Object);
            }

            dbConnectionInterceptorMock.Verify(
                m => m.DataSourceGetting(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
            dbConnectionInterceptorMock.Verify(
                m => m.DataSourceGot(It.IsAny<DbConnection>(), It.IsAny<DbConnectionInterceptionContext<string>>()),
                Times.Once());
        }

        private void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked_if_bad_connection_string_is_used(
            string connectionString, string expectedMessage)
        {
            var fakeConnection = new SqlConnection(connectionString);
            var innerException = new ProviderIncompatibleException();

            var mockProviderServices = new Mock<DbProviderServices>();
            mockProviderServices
                .Protected()
                .Setup("GetDbProviderManifestToken", fakeConnection)
                .Throws(innerException);

            var ex =
                Assert.Throws<ProviderIncompatibleException>(
                    () => mockProviderServices.Object.GetProviderManifestTokenChecked(fakeConnection));
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Same(innerException, ex.InnerException);
        }
    }
}
