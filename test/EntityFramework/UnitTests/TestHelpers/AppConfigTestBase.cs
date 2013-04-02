// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Linq;
    using Moq;

    public class AppConfigTestBase : TestBase
    {
        internal static AppConfig CreateAppConfigWithSpatial(string spatialProviderType = null)
        {
            return CreateAppConfig((string)null, null, null, spatialProviderType);
        }

        internal static AppConfig CreateAppConfig(
            string invariantName = null, string typeName = null, string sqlGeneratorName = null, string spatialProviderType = null)
        {
            return CreateAppConfig(new[] { Tuple.Create(invariantName, typeName) }, sqlGeneratorName, spatialProviderType);
        }

        internal static AppConfig CreateAppConfig(
            IEnumerable<Tuple<string, string>> providerNamesAndTypes,
            string sqlGeneratorName = null, 
            string spatialProviderType = null,
            string defaultInvariantName = null)
        {
            var mockEFSection = new Mock<EntityFrameworkSection>();
            mockEFSection.Setup(m => m.DefaultConnectionFactory).Returns(new DefaultConnectionFactoryElement());
            mockEFSection.Setup(m => m.SpatialProviderTypeName).Returns(spatialProviderType);

            var providers = new ProviderCollection();

            if (defaultInvariantName != null)
            {
                providers.DefaultInvariantName = defaultInvariantName;
            }

            foreach (var nameAndType in providerNamesAndTypes.Where(n => n.Item1 != null))
            {
                var providerElement = providers.AddProvider(nameAndType.Item1, nameAndType.Item2);
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
