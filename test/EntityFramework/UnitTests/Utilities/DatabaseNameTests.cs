// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
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
        public void Parse_parses_table_name()
        {
            var databaseName = DatabaseName.Parse("A");

            Assert.Equal(null, databaseName.Schema);
            Assert.Equal("A", databaseName.Name);
        }

        [Fact]
        public void Parse_parses_schema_dot_table_name()
        {
            var databaseName = DatabaseName.Parse("S.A");

            Assert.Equal("S", databaseName.Schema);
            Assert.Equal("A", databaseName.Name);
        }

        [Fact]
        public void Parse_throws_when_too_many_parts()
        {
            Assert.Equal(
                Strings.InvalidDatabaseName("S1.S2.A"),
                Assert.Throws<ArgumentException>(() => DatabaseName.Parse("S1.S2.A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table()
        {
            Assert.Equal(
                Strings.InvalidDatabaseName("A."),
                Assert.Throws<ArgumentException>(() => DatabaseName.Parse("A.")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_schema()
        {
            Assert.Equal(
                Strings.InvalidDatabaseName(".A"),
                Assert.Throws<ArgumentException>(() => DatabaseName.Parse(".A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table_and_schema()
        {
            Assert.Equal(
                Strings.InvalidDatabaseName("."),
                Assert.Throws<ArgumentException>(() => DatabaseName.Parse(".")).Message);
        }
    }
}
