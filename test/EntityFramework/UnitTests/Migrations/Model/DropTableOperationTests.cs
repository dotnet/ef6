namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DropTableOperationTests
    {
        [Fact]
        public void Can_get_and_set_table()
        {
            var dropTableOperation = new DropTableOperation("T");

            Assert.Equal("T", dropTableOperation.Name);
        }

        [Fact]
        public void Inverse_should_produce_create_table_operation()
        {
            var inverse = new CreateTableOperation("T");
            var dropTableOperation = new DropTableOperation("T", inverse);

            Assert.Equal("T", dropTableOperation.Name);
            Assert.Same(inverse, dropTableOperation.Inverse);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message, Assert.Throws<ArgumentException>(() => new DropTableOperation(null)).Message);
        }
    }
}