// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.SqlServer;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper;
    using Xunit;

    public class DependencyResolverTests
    {
        [Fact]
        public void DependencyResolver_resolves_IDbProviderServicesFactory()
        {
            Assert.NotNull(DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void DependencyResolver_resolves_IPluralizationService()
        {
            Assert.NotNull(DependencyResolver.GetService<IPluralizationService>());
        }

        [Fact]
        public void DependencyResolver_does_not_resolve_Object()
        {
            Assert.Null(DependencyResolver.GetService<object>());
        }

        [Fact]
        public void DependencyResolver_can_register_unregister_provider()
        {
            DependencyResolver.RegisterProvider(typeof(SqlProviderServices), "System.Data.SqlClient");
            try
            {
                Assert.Same(
                    SqlProviderServices.Instance,
                    DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
            }
            finally
            {
                DependencyResolver.UnregisterProvider("System.Data.SqlClient");
            }

            Assert.IsType<LegacyDbProviderServicesWrapper>(
                DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void EnsureProvider_registers_provider()
        {
            DependencyResolver.EnsureProvider("System.Data.SqlClient", typeof(SqlProviderServices));
            try
            {
                Assert.Same(
                    SqlProviderServices.Instance,
                    DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
            }
            finally
            {
                DependencyResolver.UnregisterProvider("System.Data.SqlClient");
            }
        }

        [Fact]
        public void EnsureProvider_unregisters_provider_when_null()
        {
            DependencyResolver.RegisterProvider(typeof(SqlProviderServices), "System.Data.SqlClient");

            DependencyResolver.EnsureProvider("System.Data.SqlClient", null);

            Assert.IsType<LegacyDbProviderServicesWrapper>(
                DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
        }
    }
}
