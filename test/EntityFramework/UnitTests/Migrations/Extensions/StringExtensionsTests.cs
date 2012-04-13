namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Linq;
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void Break_should_break_long_strings()
        {
            var s = new string('a', 3000);

            var lines = s.Break();

            Assert.Equal(3, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 3001);

            lines = s.Break();

            Assert.Equal(4, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 2999);

            lines = s.Break();

            Assert.Equal(3, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 30);

            lines = s.Break(width: 1);

            Assert.Equal(30, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));
        }


        [Fact]
        public void EqualsIgnoreCase_should_ignore_case()
        {
            Assert.True("foo".EqualsIgnoreCase("Foo"));
            Assert.False("Bar".EqualsIgnoreCase("Foo"));
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
        public void ComesBefore_detects_order()
        {
            Assert.False("111111111111112_Foo".ComesBefore("111111111111111_Foo"));
            Assert.False("111111111111111_Foo".ComesBefore("111111111111111_Foo_AutomaticMigration"));
            Assert.True("111111111111111_Foo_AutomaticMigration".ComesBefore("111111111111111_Foo"));
            Assert.True("111111111111111_Foo".ComesBefore("111111111111112_Foo"));
        }

        [Fact]
        public void RestrictTo_should_limit_string_length()
        {
            Assert.Equal("123", "123".RestrictTo(5));
            Assert.Equal("123", "123".RestrictTo(3));
            Assert.Equal("123", "12345".RestrictTo(3));
            Assert.Equal("", "12345".RestrictTo(0));
            Assert.Equal("", "".RestrictTo(0));
            Assert.Equal(null, ((string)null).RestrictTo(0));
        }
    }
}
