// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public sealed class DbDatabaseMetadataExtensionsTests
    {
        [Fact]
        public void DbInitialize_should_set_latest_version()
        {
            var database = new EdmModel().DbInitialize();

            Assert.Equal(3.0, database.Version);
        }

        [Fact]
        public void AddTable_should_create_and_add_table_to_default_schema()
        {
            var database = new EdmModel().Initialize();
            var table = database.AddTable("T");

            Assert.True(database.GetEntityTypes().Contains(table));
            Assert.Equal("T", database.GetEntityTypes().First().Name);
        }

        [Fact]
        public void Can_get_and_set_provider_annotation()
        {
            var database = new EdmModel().Initialize();
            var providerInfo = new DbProviderInfo("Foo", "Bar");

            database.SetProviderInfo(providerInfo);

            Assert.Same(providerInfo, database.GetProviderInfo());
        }
    }
}
