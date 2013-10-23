// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Data.Core;
    using Moq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class DataConnectionUtilsTests
    {
        [Fact]
        public void HasEntityFrameworkProvider_returns_true_when_has_legacy_provider()
        {
            var provider = new Mock<IVsDataProvider>();
            provider.Setup(p => p.GetProperty("InvariantName")).Returns("System.Data.SqlClient");
            var providerGuid = Guid.NewGuid();
            var providers = new Dictionary<Guid, IVsDataProvider> { { providerGuid, provider.Object } };
            var dataProviderManager = new Mock<IVsDataProviderManager>();
            dataProviderManager.SetupGet(m => m.Providers).Returns(providers);
            var dte = new MockDTE(".NETFramework,Version=v4.5");

            Assert.True(
                DataConnectionUtils.HasEntityFrameworkProvider(
                    dataProviderManager.Object,
                    providerGuid,
                    dte.Project,
                    dte.ServiceProvider));
        }

        [Fact]
        public void HasEntityFrameworkProvider_returns_false_when_no_adonet_provider_or_ef_reference()
        {
            var provider = new Mock<IVsDataProvider>();
            provider.Setup(p => p.GetProperty("InvariantName")).Returns("My.Fake.Provider");
            var providerGuid = Guid.NewGuid();
            var providers = new Dictionary<Guid, IVsDataProvider> { { providerGuid, provider.Object } };
            var dataProviderManager = new Mock<IVsDataProviderManager>();
            dataProviderManager.SetupGet(m => m.Providers).Returns(providers);
            var dte = new MockDTE(".NETFramework,Version=v4.5", references: Enumerable.Empty<Reference>());

            Assert.False(
                DataConnectionUtils.HasEntityFrameworkProvider(
                    dataProviderManager.Object,
                    providerGuid,
                    dte.Project,
                    dte.ServiceProvider));
        }

        [Fact]
        public void HasEntityFrameworkProvider_returns_false_when_no_legacy_provider_or_ef_reference()
        {
            var provider = new Mock<IVsDataProvider>();
            provider.Setup(p => p.GetProperty("InvariantName")).Returns("System.Data.OleDb");
            var providerGuid = Guid.NewGuid();
            var providers = new Dictionary<Guid, IVsDataProvider> { { providerGuid, provider.Object } };
            var dataProviderManager = new Mock<IVsDataProviderManager>();
            dataProviderManager.SetupGet(m => m.Providers).Returns(providers);
            var dte = new MockDTE(".NETFramework,Version=v4.5", references: Enumerable.Empty<Reference>());

            Assert.False(
                DataConnectionUtils.HasEntityFrameworkProvider(
                    dataProviderManager.Object,
                    providerGuid,
                    dte.Project,
                    dte.ServiceProvider));
        }
    }
}
