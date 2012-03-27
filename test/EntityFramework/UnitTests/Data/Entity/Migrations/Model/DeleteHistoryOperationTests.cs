namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DeleteHistoryOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var deleteHistoryOperation
                = new DeleteHistoryOperation("Foo", "Migration1");

            Assert.Equal("Foo", deleteHistoryOperation.Table);
            Assert.Equal("Migration1", deleteHistoryOperation.MigrationId);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message, Assert.Throws<ArgumentException>(() => new DeleteHistoryOperation(null, "Migration1")).Message);

            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("migrationId")).Message, Assert.Throws<ArgumentException>(() => new DeleteHistoryOperation("Foo", null)).Message);
        }
    }
}