// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SampleEntityFrameworkProvider;
using Xunit;

namespace ProviderTests
{
    public class ProviderModelTests : TestBase
    {
        [Fact]
        public void Verify_provider_can_be_created_with_factory()
        {
            Assert.NotNull(DbProviderFactories.GetFactory(SampleProviderName));
        }

        [Fact]
        public void Verify_factory_created_by_provider_factories_and_connection_are_same()
        {
            var providerFactory = DbProviderFactories.GetFactory(SampleProviderName);
            Assert.NotNull(providerFactory);
            Assert.Same(typeof(SampleFactory), providerFactory.GetType());

            var providerFactoryFromConnection = ((SampleConnection)providerFactory.CreateConnection()).ProviderFactory;
            Assert.Same(providerFactory.GetType(), providerFactoryFromConnection.GetType());
        }

        [Fact]
        public void Verify_SampleCommand_implements_ICloneable()
        {
            var providerFactory = DbProviderFactories.GetFactory(SampleProviderName);
            Assert.NotNull(providerFactory);
            
            var command = providerFactory.CreateCommand();

            var cloneable = command as ICloneable;
            Assert.NotNull(cloneable);

            var clonedCommand = cloneable.Clone();
            Assert.NotNull(cloneable);
        }

        [Fact]
        public void Verify_provider_supports_DbProviderServices()
        {
            var providerFactory = DbProviderFactories.GetFactory(SampleProviderName);
            Assert.NotNull(providerFactory);
            
            var serviceprovider = providerFactory as IServiceProvider;
            Assert.NotNull(serviceprovider);

            Assert.NotNull(serviceprovider.GetService(typeof(DbProviderServices)));
        }

        [Fact]
        public void Verify_provider_services_returns_provider_manifest()
        {
            var factory = DbProviderFactories.GetFactory(SampleProviderName);
            var providerServices = (DbProviderServices)((IServiceProvider)factory).GetService(typeof(DbProviderServices));
            var providerManifest = providerServices.GetProviderManifest("2005");
            Assert.NotNull(providerManifest);
        }

        [Fact]
        public void Verify_provider_manifest_token_returned_by_provider_services_is_correct()
        {
            var factory = DbProviderFactories.GetFactory(SampleProviderName);

            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = NorthwindDirectConnectionString;

                var providerServices = (DbProviderServices)((IServiceProvider)factory).GetService(typeof(DbProviderServices));
                var providerManifestToken = providerServices.GetProviderManifestToken(connection);
                Assert.True(providerManifestToken == "2005" || providerManifestToken == "2008");
            }
        }
    }
}
