namespace System.Data.Entity.ModelConfiguration.Utilities.UnitTests
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class ObjectExtensionsTests
    {
        [Fact]
        public void AsEnumerable_should_return_empty_enumeration_when_null()
        {
            Assert.Equal(0, ObjectExtensions.AsEnumerable<object>(null).Count());
        }

        [Fact]
        public void AsEnumerable_should_return_simple_enumeration()
        {
            Assert.Equal(1, 1.AsEnumerable<object>().Count());
        }

        [Fact]
        public void ParseQualifiedTableName_parses_table_name()
        {
            string table = null;
            string schema = null;
            ObjectExtensions.ParseQualifiedTableName("A", out schema, out table);
            Assert.Equal(null, schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_parses_schema_dot_table_name()
        {
            string table = null;
            string schema = null;
            ObjectExtensions.ParseQualifiedTableName("S.A", out schema, out table);
            Assert.Equal("S", schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_parses_doted_schema_andtable_name()
        {
            string table = null;
            string schema = null;
            ObjectExtensions.ParseQualifiedTableName("S1.S2.A", out schema, out table);
            Assert.Equal("S1.S2", schema);
            Assert.Equal("A", table);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_table()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidTableName("A."), Assert.Throws<ArgumentException>(() =>
                                                                                                        ObjectExtensions.ParseQualifiedTableName("A.", out schema, out table)).Message);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_schema()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidSchemaName(".A"), Assert.Throws<ArgumentException>(() =>
                                                                                                         ObjectExtensions.ParseQualifiedTableName(".A", out schema, out table)).Message);
        }

        [Fact]
        public void ParseQualifiedTableName_throws_for_empty_table_and_schema()
        {
            string table = null;
            string schema = null;
            Assert.Equal(Strings.ToTable_InvalidSchemaName("."), Assert.Throws<ArgumentException>(() =>
                                                                                                        ObjectExtensions.ParseQualifiedTableName(".", out schema, out table)).Message);
        }
    }
}