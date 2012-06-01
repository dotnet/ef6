namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;
    using Xunit;

    public sealed class PluralizingTableNameConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_pluralized_table_name_as_identitier()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var table = new DbTableMetadata { DatabaseIdentifier = "Customer" };
            database.Schemas.Single().Tables.Add(table);

            ((IDbConvention<DbTableMetadata>)new PluralizingTableNameConvention()).Apply(table, database);

            Assert.Equal("Customers", table.DatabaseIdentifier);
        }

        [Fact]
        public void Apply_should_ignored_configured_tables()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var table = new DbTableMetadata { DatabaseIdentifier = "Customer" };
            table.SetTableName(new DatabaseName("Foo"));
            database.Schemas.Single().Tables.Add(table);

            ((IDbConvention<DbTableMetadata>)new PluralizingTableNameConvention()).Apply(table, database);

            Assert.Equal("Customer", table.DatabaseIdentifier);
            Assert.Equal("Foo", table.GetTableName().Name);
        }

        [Fact]
        public void Apply_should_ignore_current_table()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var table = new DbTableMetadata { DatabaseIdentifier = "Customers" };
            database.Schemas.Single().Tables.Add(table);

            ((IDbConvention<DbTableMetadata>)new PluralizingTableNameConvention()).Apply(table, database);

            Assert.Equal("Customers", table.DatabaseIdentifier);
        }

        [Fact]
        public void Apply_should_uniquify_names()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var tableA = new DbTableMetadata { DatabaseIdentifier = "Customers" };
            var tableB = new DbTableMetadata { DatabaseIdentifier = "Customer" };
            database.Schemas.Single().Tables.Add(tableA);
            database.Schemas.Single().Tables.Add(tableB);

            ((IDbConvention<DbTableMetadata>)new PluralizingTableNameConvention()).Apply(tableB, database);

            Assert.Equal("Customers1", tableB.DatabaseIdentifier);
        }
    }
}