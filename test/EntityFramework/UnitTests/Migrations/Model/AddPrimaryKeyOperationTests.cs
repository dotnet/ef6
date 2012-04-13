namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class AddPrimaryKeyOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_name_and_columns()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation { Table = "T", Name = "Pk" };

            addPrimaryKeyOperation.Columns.Add("pk2");

            Assert.Equal("T", addPrimaryKeyOperation.Table);
            Assert.Equal("Pk", addPrimaryKeyOperation.Name);
            Assert.Equal("pk2", addPrimaryKeyOperation.Columns.Single());
            Assert.False(addPrimaryKeyOperation.HasDefaultName);
        }

        [Fact]
        public void Can_get_default_for_name()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation { Table = "T" };

            Assert.Equal("PK_T", addPrimaryKeyOperation.Name);
            Assert.True(addPrimaryKeyOperation.HasDefaultName);
        }

        [Fact]
        public void DefaultName_is_restricted_to_128_chars()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation { Table = new string('t', 150) };

            Assert.Equal(128, addPrimaryKeyOperation.DefaultName.Length);
        }

        [Fact]
        public void Inverse_should_return_drop_operation()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation { Table = "T", Name = "Pk" };

            addPrimaryKeyOperation.Columns.Add("pk2");

            var inverse = (DropPrimaryKeyOperation)addPrimaryKeyOperation.Inverse;

            Assert.Equal("T", inverse.Table);
            Assert.Equal("Pk", inverse.Name);
            Assert.Equal("pk2", inverse.Columns.Single());
        }
    }
}
