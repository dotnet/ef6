namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class DatabaseNameTests
    {
        [Fact]
        public void ToString_returns_table_name_when_no_schema_specified()
        {
            var databaseName = new DatabaseName("T");

            Assert.Equal("T", databaseName.ToString());
        }

        [Fact]
        public void ToString_returns_schema_and_table_name_when_schema_specified()
        {
            var databaseName = new DatabaseName("T", "S");

            Assert.Equal("S.T", databaseName.ToString());
        }

        [Fact]
        public void Equals_returns_true_when_names_equal_and_no_schema_specified()
        {
            var databaseName1 = new DatabaseName("T");
            var databaseName2 = new DatabaseName("T");

            Assert.Equal(databaseName1, databaseName2);
        }

        [Fact]
        public void Equals_returns_false_when_names_equal_and_schemas_not_equal()
        {
            var databaseName1 = new DatabaseName("T", "S1");
            var databaseName2 = new DatabaseName("T", "S2");

            Assert.NotEqual(databaseName1, databaseName2);
        }

        [Fact]
        public void ParseQualifiedTableName_parses_table_name()
        {
            string table = null;
            string schema = null;
            DatabaseName.ParseQualifiedTableName("A", out schema, out table);
            Assert.Equal(null, schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_parses_schema_dot_table_name()
        {
            string table = null;
            string schema = null;
            DatabaseName.ParseQualifiedTableName("S.A", out schema, out table);
            Assert.Equal("S", schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_parses_doted_schema_andtable_name()
        {
            string table = null;
            string schema = null;
            DatabaseName.ParseQualifiedTableName("S1.S2.A", out schema, out table);
            Assert.Equal("S1.S2", schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_table()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidTableName("A."), Assert.Throws<ArgumentException>(() =>
                DatabaseName.ParseQualifiedTableName("A.", out schema, out table)).Message);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_schema()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidSchemaName(".A"), Assert.Throws<ArgumentException>(() =>
                DatabaseName.ParseQualifiedTableName(".A", out schema, out table)).Message);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_table_and_schema()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidSchemaName("."), Assert.Throws<ArgumentException>(() =>
                DatabaseName.ParseQualifiedTableName(".", out schema, out table)).Message);
        }
    }
}