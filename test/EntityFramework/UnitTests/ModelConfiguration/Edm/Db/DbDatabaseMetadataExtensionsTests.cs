namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public sealed class DbDatabaseMetadataExtensionsTests
    {
        [Fact]
        public void Initialize_should_create_default_schema()
        {
            var database = new DbDatabaseMetadata().Initialize();

            Assert.Equal(3.0, database.Version);
        }

        [Fact]
        public void AddTable_should_create_and_add_table_to_default_schema()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var table = database.AddTable("T");

            Assert.True(database.Schemas.First().Tables.Contains(table));
            Assert.Equal("T", database.Schemas.First().Tables.First().Name);
        }

        [Fact]
        public void Can_get_and_set_provider_annotation()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var providerInfo = new DbProviderInfo("Foo", "Bar");

            database.SetProviderInfo(providerInfo);

            Assert.Same(providerInfo, database.GetProviderInfo());
        }
    }
}