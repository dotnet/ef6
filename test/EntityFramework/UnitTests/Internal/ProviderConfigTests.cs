// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Internal.ConfigFile;
    using Xunit;

    public class ProviderConfigTests
    {
        public class GetAllDbProviderServices : AppConfigTestBase
        {
            [Fact]
            public void GetAllDbProviderServices_returns_provider_entries()
            {
                var providerConfig =
                    CreateAppConfig(
                        new[] { Tuple.Create("Hy.Pro.Glo", "Hy.Pro.Glo.Type"), Tuple.Create("Potters.Field", "Potters.Field.Type") });

                Assert.Equal(
                    new[]
                        {
                            new ProviderElement
                                {
                                    ProviderTypeName = "Hy.Pro.Glo.Type",
                                    InvariantName = "Hy.Pro.Glo"
                                },
                            new ProviderElement
                                {
                                    ProviderTypeName = "Potters.Field.Type",
                                    InvariantName = "Potters.Field"
                                }
                        },
                    providerConfig.Providers.GetAllDbProviderServices());
            }
        }
    }
}
