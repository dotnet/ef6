namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Linq;
    using Xunit;

    public sealed class ColumnOrderingConventionTests
    {
        [Fact]
        public void Apply_should_order_by_annotation_if_given()
        {
            var table = new DbTableMetadata();
            table.AddColumn("C").SetOrder(2);
            table.AddColumn("Id").SetOrder(1);

            ((IDbConvention<DbTableMetadata>)new ColumnOrderingConvention()).Apply(table, new DbDatabaseMetadata());

            Assert.Equal(2, table.Columns.Count);
            Assert.Equal("Id", table.Columns.First().Name);
        }

        [Fact]
        public void Apply_should_sort_annotated_before_unannotated()
        {
            var table = new DbTableMetadata();
            table.AddColumn("C").SetOrder(2);
            table.AddColumn("Id");

            ((IDbConvention<DbTableMetadata>)new ColumnOrderingConvention()).Apply(table, new DbDatabaseMetadata());

            Assert.Equal(2, table.Columns.Count);
            Assert.Equal("C", table.Columns.First().Name);
        }

        [Fact]
        public void Apply_should_sort_unannotated_in_given_order()
        {
            var table = new DbTableMetadata();
            table.AddColumn("C");
            table.AddColumn("Id");

            ((IDbConvention<DbTableMetadata>)new ColumnOrderingConvention()).Apply(table, new DbDatabaseMetadata());

            Assert.Equal(2, table.Columns.Count);
            Assert.Equal("C", table.Columns.First().Name);
        }
    }
}
