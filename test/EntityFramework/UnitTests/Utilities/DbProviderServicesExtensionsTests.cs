// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbProviderServicesExtensionsTests
    {
        [Fact]
        public void Useful_exception_is_thrown_by_GetProviderManifestTokenChecked()
        {
            var fakeConnection = new SqlConnection("Data Source=AnyConnectionString");
            var innerException = new ProviderIncompatibleException();

            var mockProviderServices = new Mock<DbProviderServices>();
            mockProviderServices
                .Protected()
                .Setup("GetDbProviderManifestToken", fakeConnection)
                .Throws(innerException);

            var ex =
                Assert.Throws<ProviderIncompatibleException>(
                    () => mockProviderServices.Object.GetProviderManifestTokenChecked(fakeConnection));
            Assert.Equal(Strings.FailedToGetProviderInformation, ex.Message);
            Assert.Same(innerException, ex.InnerException);
        }
    }
}
