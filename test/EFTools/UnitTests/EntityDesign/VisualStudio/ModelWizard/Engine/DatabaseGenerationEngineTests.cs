// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using DbProviderServices = System.Data.Entity.Core.Common.DbProviderServices;

    public class DatabaseGenerationEngineTests
    {
        [Fact]
        public void GetProviderManifestTokenConnected_returns_provider_manifest_token()
        {
            var providerServicesMock = new Mock<DbProviderServices>();
            providerServicesMock
                .Protected()
                .Setup<string>("GetDbProviderManifestToken", ItExpr.IsAny<DbConnection>())
                .Returns("FakeProviderManifestToken");

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(providerServicesMock.Object);

            Assert.Equal(
                "FakeProviderManifestToken",
                DatabaseGenerationEngine.GetProviderManifestTokenConnected(
                    mockResolver.Object,
                    "System.Data.SqlClient",
                    providerConnectionString: string.Empty));
        }
    }
}
