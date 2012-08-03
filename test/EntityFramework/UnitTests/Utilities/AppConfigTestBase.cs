// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using Moq;

    public class AppConfigTestBase : TestBase
    {
        internal static AppConfig CreateAppConfig(string invariantName = null, string typeName = null, string sqlGeneratorName = null)
        {
            var mockEFSection = new Mock<EntityFrameworkSection>();
            mockEFSection.Setup(m => m.DefaultConnectionFactory).Returns(new DefaultConnectionFactoryElement());

            var providers = new ProviderCollection();
            if (!string.IsNullOrEmpty(invariantName))
            {
                var providerElement = providers.AddProvider(invariantName, typeName);
                if (sqlGeneratorName != null)
                {
                    providerElement.SqlGeneratorElement = new MigrationSqlGeneratorElement
                        {
                            SqlGeneratorTypeName = sqlGeneratorName
                        };
                }
            }
            mockEFSection.Setup(m => m.Providers).Returns(providers);

            return new AppConfig(new ConnectionStringSettingsCollection(), null, mockEFSection.Object);
        }
    }
}
