namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class AddForeignKeyOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation
                {
                    PrincipalTable = "P",
                    DependentTable = "D",
                    CascadeDelete = true,
                    Name = "Foo"
                };

            addForeignKeyOperation.PrincipalColumns.Add("pk");
            addForeignKeyOperation.DependentColumns.Add("fk");

            Assert.Equal("P", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("D", addForeignKeyOperation.DependentTable);
            Assert.Equal("pk", addForeignKeyOperation.PrincipalColumns.Single());
            Assert.Equal("fk", addForeignKeyOperation.DependentColumns.Single());
            Assert.True(addForeignKeyOperation.CascadeDelete);
            Assert.Equal("Foo", addForeignKeyOperation.Name);
            Assert.Equal("FK_D_P_fk", addForeignKeyOperation.DefaultName);
            Assert.False(addForeignKeyOperation.HasDefaultName);
        }

        [Fact]
        public void DefaultName_is_restricted_to_128_chars()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation
            {
                PrincipalTable = "P",
                DependentTable = "D"
            };

            addForeignKeyOperation.DependentColumns.Add(new string('c', 150));

            Assert.Equal(128, addForeignKeyOperation.DefaultName.Length);
        }

        [Fact]
        public void Inverse_should_produce_drop_foreign_key_operation()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation
                {
                    PrincipalTable = "P",
                    DependentTable = "D",
                    Name = "Foo"
                };

            addForeignKeyOperation.PrincipalColumns.Add("pk");
            addForeignKeyOperation.DependentColumns.Add("fk");

            var dropForeignKeyOperation = (DropForeignKeyOperation)addForeignKeyOperation.Inverse;

            Assert.Equal("P", dropForeignKeyOperation.PrincipalTable);
            Assert.Equal("D", dropForeignKeyOperation.DependentTable);
            Assert.Equal("fk", dropForeignKeyOperation.DependentColumns.Single());
            Assert.Equal("Foo", dropForeignKeyOperation.Name);
            Assert.Equal("FK_D_P_fk", dropForeignKeyOperation.DefaultName);
        }

        [Fact]
        public void CreateCreateIndexOperation_should_return_corresponding_create_index_operation()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation
                {
                    PrincipalTable = "P",
                    DependentTable = "D",
                    Name = "Foo"
                };
            addForeignKeyOperation.DependentColumns.Add("fk");

            var createIndexOperation = addForeignKeyOperation.CreateCreateIndexOperation();

            Assert.Equal(createIndexOperation.DefaultName, createIndexOperation.Name);
            Assert.Equal("D", createIndexOperation.Table);
            Assert.Equal("fk", createIndexOperation.Columns.Single());
        }
    }
}
