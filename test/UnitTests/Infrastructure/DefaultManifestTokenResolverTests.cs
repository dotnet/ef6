// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Moq;
    using Xunit;

    public class DefaultManifestTokenResolverTests
    {
        [Fact]
        public void ResolveManifestToken_gets_the_token_from_the_provider_of_the_given_connection()
        {
            Assert.Equal("1908", new DefaultManifestTokenResolver().ResolveManifestToken(new FakeSqlConnection("1908")));
        }

        [Fact]
        public void ResolveManifestToken_caches_based_on_connection_server_and_database()
        {
            var resolver = new DefaultManifestTokenResolver();

            Assert.Equal("1908", resolver.ResolveManifestToken(CreateConnection<FakeSqlConnection>("1908", "Cheese", "Pickle")));

            // The following call should still return 1908 because the cache is being used
            Assert.Equal("1908", resolver.ResolveManifestToken(CreateConnection<FakeSqlConnection>("2108", "Cheese", "Pickle")));

            // Each of the following calls should miss the cache and return the new manifest token value
            Assert.Equal("2110", resolver.ResolveManifestToken(CreateConnection<FakeSqlConnection>("2110", "Beer", "Pickle")));
            Assert.Equal("2111", resolver.ResolveManifestToken(CreateConnection<FakeSqlConnection>("2111", "Cheese", "Chips")));

#if !NET40
            // Only on .NET 4.5 because provider lookup will fail on .NET 4 unless we jump through hoops to register
            Assert.Equal("2109", resolver.ResolveManifestToken(CreateConnection<DerivedFakeSqlConnection>("2109", "Cheese", "Pickle")));
#endif
        }

        [Fact]
        public void ResolveManifestToken_uses_interception()
        {
            var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
            DbInterception.Add(dbConnectionInterceptorMock.Object);
            try
            {
                new DefaultManifestTokenResolver().ResolveManifestToken(CreateConnection<FakeSqlConnection>("1908", "Cheese", "Pickle"));
            }
            finally
            {
                DbInterception.Remove(dbConnectionInterceptorMock.Object);
            }
        }

    [Fact]
        public void ResolveManifestToken_throws_if_given_null_connection()
        {
            Assert.Equal(
                "connection",
                Assert.Throws<ArgumentNullException>(() => new DefaultManifestTokenResolver().ResolveManifestToken(null)).ParamName);
        }

        private static TConnection CreateConnection<TConnection>(string manifestToken, string database, string dataSource)
            where TConnection : FakeSqlConnection, new()
        {
            var originalConnection = new TConnection { ManifestToken = manifestToken };
            originalConnection.SetDatabase(database);
            originalConnection.SetDataSource(dataSource);
            return originalConnection;
        }

#if !NET40
        public class DerivedFakeSqlConnection : FakeSqlConnection
        {
        }
#endif
    }
}
