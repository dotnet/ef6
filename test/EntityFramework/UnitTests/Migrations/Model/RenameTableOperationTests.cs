namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class RenameTableOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message, Assert.Throws<ArgumentException>(() => new RenameTableOperation(null, null)).Message);

            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("newName")).Message, Assert.Throws<ArgumentException>(() => new RenameTableOperation("N", null)).Message);
        }

        [Fact]
        public void Can_get_and_set_rename_properties()
        {
            var renameTableOperation = new RenameTableOperation("N", "N'");

            Assert.Equal("N", renameTableOperation.Name);
            Assert.Equal("N'", renameTableOperation.NewName);
        }

        [Fact]
        public void Inverse_should_produce_rename_column_operation()
        {
            var renameTableOperation = new RenameTableOperation("N", "N'");

            var inverse = (RenameTableOperation)renameTableOperation.Inverse;

            Assert.Equal("N'", inverse.Name);
            Assert.Equal("N", inverse.NewName);
        }
    }
}