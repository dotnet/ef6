// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ProviderCollectionTests
    {
        private class ProviderCollectionInvoker : ProviderCollection
        {
            public void InvokeBaseAdd(ConfigurationElement element)
            {
                BaseAdd(element);
            }

            public void InvokeBaseAdd(int index, ConfigurationElement element)
            {
                BaseAdd(index, element);
            }
        }

        [Fact]
        public void ProviderCollection_ignores_duplicate_provider_entries()
        {
            var providerCollectionInvoker = new ProviderCollectionInvoker();
            providerCollectionInvoker
                .InvokeBaseAdd(
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });

            Assert.Equal(1, providerCollectionInvoker.Count);

            providerCollectionInvoker
                .InvokeBaseAdd(
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });

            Assert.Equal(1, providerCollectionInvoker.Count);
        }

        [Fact]
        public void ProviderCollection_ignores_duplicate_provider_entries_index_overload()
        {
            var providerCollectionInvoker = new ProviderCollectionInvoker();
            providerCollectionInvoker
                .InvokeBaseAdd(
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });

            Assert.Equal(1, providerCollectionInvoker.Count);

            providerCollectionInvoker
                .InvokeBaseAdd(1,
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });

            Assert.Equal(1, providerCollectionInvoker.Count);
        }

        [Fact]
        public void Cannot_change_type_for_registered_provider()
        {
            var providerCollectionInvoker = new ProviderCollectionInvoker();
            providerCollectionInvoker
                .InvokeBaseAdd(
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });
            
            Assert.Equal(
                Strings.ProviderInvariantRepeatedInConfig("All.Sql"),
                Assert.Throws<InvalidOperationException>(
                    () => providerCollectionInvoker
                            .InvokeBaseAdd(
                                new ProviderElement
                                {
                                    InvariantName = "All.Sql", 
                                    ProviderTypeName = "No.Sql.Provider"
                                })).Message);
        }

        [Fact]
        public void Cannot_change_type_for_registered_provider_index_overload()
        {
            var providerCollectionInvoker = new ProviderCollectionInvoker();
            providerCollectionInvoker
                .InvokeBaseAdd(
                    new ProviderElement { InvariantName = "All.Sql", ProviderTypeName = "All.Sql.Provider" });
            
            Assert.Equal(
                Strings.ProviderInvariantRepeatedInConfig("All.Sql"),
                Assert.Throws<InvalidOperationException>(
                    () => providerCollectionInvoker
                            .InvokeBaseAdd(1, 
                                new ProviderElement
                                {
                                    InvariantName = "All.Sql", 
                                    ProviderTypeName = "No.Sql.Provider"
                                })).Message);
        }
    }
}
