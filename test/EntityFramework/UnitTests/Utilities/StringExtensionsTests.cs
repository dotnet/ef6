// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void EqualsIgnoreCase_should_ignore_case()
        {
            Assert.True("foo".EqualsIgnoreCase("Foo"));
            Assert.False("Bar".EqualsIgnoreCase("Foo"));
        }

        [Fact]
        public void ToDatabaseName_returns_database_name()
        {
            var databaseName = "dbo.Customers".ToDatabaseName();

            Assert.Equal("dbo", databaseName.Schema);
            Assert.Equal("Customers", databaseName.Name);
        }

        [Fact]
        public void IsValid_should_correctly_validate_ids()
        {
            Assert.False("Foo".IsValidMigrationId());
            Assert.False("11111111111111_Foo".IsValidMigrationId());
            Assert.False("11111111111111Foo".IsValidMigrationId());
            Assert.True("111111111111111_Foo".IsValidMigrationId());
            Assert.True(DbMigrator.InitialDatabase.IsValidMigrationId());
        }

        [Fact]
        public void IsAutomatic_detects_automatic_migration_names()
        {
            Assert.True("111111111111111_AutomaticMigration".IsAutomaticMigration());
            Assert.True("111111111111111_Foo_AutomaticMigration".IsAutomaticMigration());
            Assert.False("111111111111111_Foo".IsAutomaticMigration());
        }

        [Fact]
        public void RestrictTo_should_limit_string_length()
        {
            Assert.Equal("123", "123".RestrictTo(5));
            Assert.Equal("123", "123".RestrictTo(3));
            Assert.Equal("123", "12345".RestrictTo(3));
            Assert.Equal("", "12345".RestrictTo(0));
            Assert.Equal("", "".RestrictTo(0));
            Assert.Equal(null, StringExtensions.RestrictTo(null, 0));
        }

        [Fact]
        public void ToAutomaticMigrationId_should_rewind_timestamp_and_append_auto_string()
        {
            Assert.Equal("201205054534555_Foo_AutomaticMigration", "201205054534556_Foo".ToAutomaticMigrationId());
            Assert.Equal("111111111111109_Foo_AutomaticMigration", "111111111111110_Foo".ToAutomaticMigrationId());
        }
    }
}
