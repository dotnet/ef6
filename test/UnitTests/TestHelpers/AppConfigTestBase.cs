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
        internal static AppConfig CreateAppConfig(string invariantName = null, string typeName = null)
        {
            return CreateAppConfig(new[] { Tuple.Create(invariantName, typeName) });
        }

        internal static AppConfig CreateAppConfig(IEnumerable<Tuple<string, string>> providerNamesAndTypes)
        {
            var mockEFSection = new Mock<EntityFrameworkSection>();
            mockEFSection.Setup(m => m.DefaultConnectionFactory).Returns(new DefaultConnectionFactoryElement());

            var providers = new ProviderCollection();

            foreach (var nameAndType in providerNamesAndTypes.Where(n => n.Item1 != null))
            {
                providers.AddProvider(nameAndType.Item1, nameAndType.Item2);
            }
            mockEFSection.Setup(m => m.Providers).Returns(providers);

            return new AppConfig(new ConnectionStringSettingsCollection(), null, mockEFSection.Object);
        }
    }
}
