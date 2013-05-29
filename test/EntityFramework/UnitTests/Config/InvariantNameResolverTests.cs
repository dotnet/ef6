// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class InvariantNameResolverTests
    {
        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_null_for_non_IProviderInvariantName_types()
            {
                Assert.Null(new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW").GetService<Random>());
            }

            [Fact]
            public void GetService_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                                  .GetService<IProviderInvariantName>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                                  .GetService<IProviderInvariantName>("Oh No!")).Message);
            }

            [Fact]
            public void GetService_returns_the_invariant_name_registered_for_the_given_DbProviderFactory()
            {
                var factory = new Mock<DbProviderFactory>().Object;

                Assert.Equal(
                    "920.FTW",
                    new InvariantNameResolver(factory, "920.FTW").GetService<IProviderInvariantName>(factory).Name);
            }

            [Fact]
            public void GetService_returns_null_for_a_different_DbProviderFactory()
            {
                Assert.Null(
                    new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                        .GetService<IProviderInvariantName>(SqlClientFactory.Instance));
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetService_returns_empty_list_for_non_IProviderInvariantName_types()
            {
                Assert.Empty(new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW").GetServices<Random>());
            }

            [Fact]
            public void GetServices_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                                  .GetServices<IProviderInvariantName>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                                  .GetServices<IProviderInvariantName>("Oh No!")).Message);
            }

            [Fact]
            public void GetServices_returns_the_invariant_name_registered_for_the_given_DbProviderFactory()
            {
                var factory = new Mock<DbProviderFactory>().Object;

                Assert.Equal(
                    "920.FTW",
                    new InvariantNameResolver(factory, "920.FTW").GetServices<IProviderInvariantName>(factory).Single().Name);
            }

            [Fact]
            public void GetServices_returns_empty_list_for_a_different_DbProviderFactory()
            {
                Assert.Empty(
                    new InvariantNameResolver(new Mock<DbProviderFactory>().Object, "920.FTW")
                        .GetServices<IProviderInvariantName>(SqlClientFactory.Instance));
            }
        }
    }
}
