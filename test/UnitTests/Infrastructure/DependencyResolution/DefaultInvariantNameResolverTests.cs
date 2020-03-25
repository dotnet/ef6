// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    public class DefaultInvariantNameResolverTests
    {
        public class GetService : TestBase
        {
            [Fact]
            public void GetService_returns_null_for_non_IProviderInvariantName_types()
            {
                Assert.Null(new DefaultInvariantNameResolver().GetService<Random>());
            }

            [Fact]
            public void GetService_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultInvariantNameResolver().GetService<IProviderInvariantName>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultInvariantNameResolver().GetService<IProviderInvariantName>("Oh No!")).Message);
            }

            [Fact]
            public void GetService_returns_the_invariant_name_obtained_from_the_given_DbProviderFactory()
            {
                Assert.Equal(
                    "System.Data.SqlClient",
                    new DefaultInvariantNameResolver().GetService<IProviderInvariantName>(SqlClientFactory.Instance).Name);
            }
        }

        public class GetServices : TestBase
        {
            [Fact]
            public void GetServices_returns_empty_list_for_non_IProviderInvariantName_types()
            {
                Assert.Empty(new DefaultInvariantNameResolver().GetServices<Random>());
            }

            [Fact]
            public void GetServices_throws_for_null_or_incorrect_key_type()
            {
                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultInvariantNameResolver().GetServices<IProviderInvariantName>(null)).Message);

                Assert.Equal(
                    Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)),
                    Assert.Throws<ArgumentException>(
                        () => new DefaultInvariantNameResolver().GetServices<IProviderInvariantName>("Oh No!")).Message);
            }

            [Fact]
            public void GetServices_returns_the_invariant_name_obtained_from_the_given_DbProviderFactory()
            {
                Assert.Equal(
                    "System.Data.SqlClient",
                    new DefaultInvariantNameResolver().GetServices<IProviderInvariantName>(SqlClientFactory.Instance).Single().Name);
            }
        }
    }
}
