// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public sealed class DbConnectionExtensionsTests : TestBase
    {
        [Fact]
        public void GetProviderInvariantName_should_return_correct_name()
        {
            Assert.Equal("System.Data.SqlClient", new SqlConnection().GetProviderInvariantName());
        }

        [Fact]
        public void GetProviderInvariantName_throws_for_unknown_provider()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
            mockConnection.Setup(m => m.ToString()).Returns("I Be A Bad Bad Connection Is What I Be.");

            // On .NET 4 the situation where we fail to get the invariant name is the same as not being
            // able to get the provider, so the exception message is different.
            Assert.Equal(
#if NET40
                Strings.ProviderNotFound("I Be A Bad Bad Connection Is What I Be."),
#else
                Strings.ProviderNameNotFound("Castle.Proxies.DbProviderFactoryProxy"),
#endif
                Assert.Throws<NotSupportedException>(() => mockConnection.Object.GetProviderInvariantName()).Message);
        }

        [Fact]
        public void GetProviderFactory_should_return_correct_factory()
        {
            Assert.Equal(SqlClientFactory.Instance, new SqlConnection().GetProviderFactory());
        }

#if NET40
        [Fact]
        public void GetProviderFactory_throws_for_unknown_provider_on_net40()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
            mockConnection.Setup(m => m.ToString()).Returns("I Be A Bad Bad Connection Is What I Be.");

            Assert.Equal(
                Strings.ProviderNotFound("I Be A Bad Bad Connection Is What I Be."),
                Assert.Throws<NotSupportedException>(() => mockConnection.Object.GetProviderFactory()).Message);
        }
#endif
    }
}
