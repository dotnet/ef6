// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Moq;
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
            // On .NET 4 the situation where we fail to get the invariant name is the same as not being
            // able to get the provider, so the exception message is different.
            Assert.Equal(
#if NET40
                Strings.ProviderNotFound("I Be A Bad Bad Connection Is What I Be."),
#else
                Strings.ProviderNameNotFound("Castle.Proxies.DbProviderFactoryProxy"),
#endif
                Assert.Throws<NotSupportedException>(() => new InvalidConnection().GetProviderInvariantName()).Message);
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
            Assert.Equal(
                Strings.ProviderNotFound("I Be A Bad Bad Connection Is What I Be."),
                Assert.Throws<NotSupportedException>(() => new InvalidConnection().GetProviderFactory()).Message);
        }
#endif

        public class InvalidConnection : DbConnection
        {
            protected override DbProviderFactory DbProviderFactory
            {
                get { return new Mock<DbProviderFactory>().Object; }
            }

            public override string ToString()
            {
                return "I Be A Bad Bad Connection Is What I Be.";
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                throw new NotImplementedException();
            }

            public override void ChangeDatabase(string databaseName)
            {
                throw new NotImplementedException();
            }

            public override void Close()
            {
                throw new NotImplementedException();
            }

            public override string ConnectionString
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotImplementedException();
            }

            public override string DataSource
            {
                get { throw new NotImplementedException(); }
            }

            public override string Database
            {
                get { throw new NotImplementedException(); }
            }

            public override void Open()
            {
                throw new NotImplementedException();
            }

            public override string ServerVersion
            {
                get { throw new NotImplementedException(); }
            }

            public override ConnectionState State
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
