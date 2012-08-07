// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using System.Linq;
    using Xunit;

    public class DefaultProviderServicesResolverTests : TestBase
    {
        [Fact]
        public void The_provider_services_resolver_returns_SqlProviderServices_type_for_SqlClient_invariant_name()
        {
            Assert.Same(
                SqlProviderServices.Instance,
                new DefaultProviderServicesResolver().GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void The_provider_services_resolver_returns_SqlCeProviderServices_type_for_Sql_Compact_invariant_name()
        {
            Assert.Same(
                SqlCeProviderServices.Instance,
                new DefaultProviderServicesResolver().GetService<DbProviderServices>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void The_provider_services_resolver_throws_for_an_unknown_provider_name()
        {
            Assert.Equal(
                Strings.EF6Providers_NoProviderFound("Don't.Come.Around.Here.No.More"),
                Assert.Throws<InvalidOperationException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("Don't.Come.Around.Here.No.More")).Message);
        }

        [Fact]
        public void The_provider_services_resolver_throws_for_an_empty_provider_name()
        {
            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(null)).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("")).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(" ")).Message);
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new DefaultProviderServicesResolver().Release(new object());
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void GetService_can_be_accessed_from_multiple_threads_concurrently()
        {
            for (var i = 0; i < 30; i++)
            {
                var bag = new ConcurrentBag<DbProviderServices>();
                var resolver = new DefaultProviderServicesResolver();

                ExecuteInParallel(() => bag.Add(resolver.GetService<DbProviderServices>("System.Data.SqlClient")));

                Assert.Equal(20, bag.Count);
                Assert.True(bag.All(c => SqlProviderServices.Instance == c));
            }
        }
    }
}
