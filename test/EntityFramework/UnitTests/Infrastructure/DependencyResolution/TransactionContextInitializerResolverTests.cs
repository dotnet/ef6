// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class TransactionContextInitializerResolverTests
    {
        public class GetService
        {
            [Fact]
            public void GetService_returns_null_for_non_IDatabaseInitializer_types()
            {
                Assert.Null(new TransactionContextInitializerResolver().GetService<Random>());
            }

            [Fact]
            public void GetService_returns_null_for_an_unparametrized_IDatabaseInitializer()
            {
                Assert.Null(
                    new TransactionContextInitializerResolver()
                        .GetService(typeof(IDatabaseInitializer<>), null));
            }

            [Fact]
            public void GetService_returns_null_for_a_non_TransactionContext_context_type()
            {
                Assert.Null(
                    new TransactionContextInitializerResolver()
                        .GetService<IDatabaseInitializer<DbContext>>());
            }

            [Fact]
            public void GetService_returns_the_initializer_for_TransactionContext()
            {
                Assert.IsType<TransactionContextInitializer<TransactionContext>>(
                    new TransactionContextInitializerResolver()
                        .GetService<IDatabaseInitializer<TransactionContext>>());
            }
        }
        
        public class GetServices
        {
            [Fact]
            public void GetServices_returns_the_initializer_for_TransactionContext()
            {
                Assert.IsType<TransactionContextInitializer<TransactionContext>>(
                    new TransactionContextInitializerResolver()
                        .GetServices<IDatabaseInitializer<TransactionContext>>().Single());
            }
        }
    }
}
