// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using Xunit;

    public class DefaultProviderFactoryResolverTests
    {
        [Fact]
        public void GetService_returns_null_for_non_DbProviderFactory_types()
        {
            Assert.Null(new DefaultProviderFactoryResolver().GetService<Random>());
        }

        [Fact]
        public void GetService_throws_for_null_or_incorrect_key_type()
        {
            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderFactoryResolver().GetService<DbProviderFactory>(null)).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderFactoryResolver().GetService<DbProviderFactory>("")).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderFactoryResolver().GetService<DbProviderFactory>(" ")).Message);

            Assert.Equal(
                Strings.ProviderInvariantNotPassedToResolver,
                Assert.Throws<ArgumentException>(
                    () => new DefaultProviderFactoryResolver().GetService<DbProviderFactory>(new Random())).Message);
        }

        [Fact]
        public void GetService_returns_correct_provider_factory_given_an_invariant_name()
        {
            Assert.Same(
                SqlClientFactory.Instance,
                new DefaultProviderFactoryResolver().GetService<DbProviderFactory>("System.Data.SqlClient"));
        }

        [Fact]
        public void GetService_wraps_argument_exception_when_provider_is_not_found()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new DefaultProviderFactoryResolver().GetService<DbProviderFactory>("Oh.No.Not.Again"));

            Assert.Equal(Strings.EntityClient_InvalidStoreProvider, exception.Message);
            Assert.NotNull(exception.InnerException);
        }
    }
}
