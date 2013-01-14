// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Xunit;

    public class DefaultInvariantNameResolverTests
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
                Strings.DbProviderFactoryNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultInvariantNameResolver().GetService<IProviderInvariantName>(null)).Message);

            Assert.Equal(
                Strings.DbProviderFactoryNotPassedToResolver,
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
}
