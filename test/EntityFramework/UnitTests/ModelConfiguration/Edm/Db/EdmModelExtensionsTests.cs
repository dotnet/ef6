// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public sealed class EdmModelExtensionsTests
    {
        [Fact]
        public void InitializeStore_should_set_latest_version()
        {
            var database = new EdmModel(DataSpace.SSpace);

            Assert.Equal(3.0, database.Version);
        }

        [Fact]
        public void AddTable_should_create_and_add_table_to_default_schema()
        {
            var database = new EdmModel(DataSpace.SSpace);

            var table = database.AddTable("T");

            Assert.True(database.EntityTypes.Contains(table));
            Assert.Equal("T", database.EntityTypes.First().Name);
        }

        [Fact]
        public void Can_get_and_set_provider_annotation()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var providerInfo = new DbProviderInfo("Foo", "Bar");

            database.ProviderInfo = providerInfo;

            Assert.Same(providerInfo, database.ProviderInfo);
        }

        [Fact]
        public void AddFunction_should_create_and_add_function_to_model()
        {
            var database = new EdmModel(DataSpace.SSpace);

            var function = database.AddFunction("F", new EdmFunctionPayload());

            Assert.True(database.Functions.Contains(function));
            Assert.Equal("F", database.Functions.First().Name);
        }

        [Fact]
        public void AddFunction_should_uniquify_namee()
        {
            var database = new EdmModel(DataSpace.SSpace);
            database.AddFunction("F", new EdmFunctionPayload());

            var function = database.AddFunction("F", new EdmFunctionPayload());

            Assert.True(database.Functions.Contains(function));
            Assert.Equal("F1", database.Functions.Last().Name);
        }
    }
}
