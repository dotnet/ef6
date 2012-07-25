// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using Xunit;

    public sealed class DbProviderFactoryExtensionsTests
    {
        [Fact]
        public void GetProviderServices_returns_EntityProviderServices_from_EntityProviderFactory()
        {
            Assert.Same(EntityProviderServices.Instance, EntityProviderFactory.Instance.GetProviderServices());
        }

        [Fact]
        public void GetProviderServices_returns_SQL_Server_provider_by_convention()
        {
            Assert.Same(
                SqlProviderServices.Instance,
                SqlClientFactory.Instance.GetProviderServices());
        }

        [Fact]
        public void GetProviderServices_returns_provider_registered_in_app_config()
        {
            Assert.Same(
                FakeSqlProviderServices.Instance,
                FakeSqlProviderFactory.Instance.GetProviderServices());
        }
    }
}