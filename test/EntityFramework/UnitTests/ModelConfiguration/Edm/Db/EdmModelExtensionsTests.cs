// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public sealed class EdmModelExtensionsTests
    {
        [Fact]
        public void InitializeStore_should_set_latest_version()
        {
            var database = new EdmModel().InitializeStore();

            Assert.Equal(3.0, database.Version);
        }

        [Fact]
        public void AddTable_should_create_and_add_table_to_default_schema()
        {
            var database = new EdmModel().InitializeConceptual();
            var table = database.AddTable("T");

            
            Assert.True(database.EntityTypes.Contains(table));
            
            Assert.Equal("T", database.EntityTypes.First().Name);
        }

        [Fact]
        public void Can_get_and_set_provider_annotation()
        {
            var database = new EdmModel().InitializeConceptual();
            var providerInfo = new DbProviderInfo("Foo", "Bar");

            database.ProviderInfo = providerInfo;

            Assert.Same(providerInfo, database.ProviderInfo);
        }
    }
}
