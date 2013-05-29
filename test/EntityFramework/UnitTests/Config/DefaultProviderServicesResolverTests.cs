// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DefaultProviderServicesResolverTests : TestBase
    {
        public class GetService : TestBase
        {
            [Fact]
            public void The_provider_services_resolver_throws_for_an_unknown_provider_name()
            {
                Assert.Equal(
                    Strings.EF6Providers_NoProviderFound("Don't.Come.Around.Here.No.More"),
                    Assert.Throws<InvalidOperationException>(
                        () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("Don't.Come.Around.Here.No.More"))
                        .Message);
            }

            [Fact]
            public void The_provider_services_resolver_throws_for_an_empty_provider_name()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetService<DbProviderServices>("")).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetService<DbProviderServices>(" ")).Message);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void The_provider_services_resolver_returns_the_empty_list_for_an_unknown_provider_name()
            {
                Assert.Empty(new DefaultProviderServicesResolver().GetServices<DbProviderServices>("Don't.Come.Around.Here.No.More"));
            }

            [Fact]
            public void The_provider_services_resolver_throws_for_an_empty_provider_name()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetServices<DbProviderServices>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetServices<DbProviderServices>("")).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultProviderServicesResolver().GetServices<DbProviderServices>(" ")).Message);
            }
        }
    }
}
