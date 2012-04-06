namespace System.Data.Entity.Migrations
{
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AlterColumnScenarios : DbTestCase
    {
        private class AlterColumnWithDefault : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(defaultValue: "Bill"));
            }
        }

        [MigrationsTheory]
        public void Can_change_column_to_have_default_value()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithDefault());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.True(column.Default.Contains("'Bill'"));
        }

        private class AlterColumnWithIdentityMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "CustomerNumber", c => c.Long(identity: true));
            }
        }

        [MigrationsTheory] // TODO: Can't handle this yet (table rebuild)
        public void Can_change_column_to_identity_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithIdentityMigration());

            migrator.Update();

            // TODO: Assert column is identity
        }

        private class AlterColumnMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(nullable: false));
            }
        }

        [MigrationsTheory]
        public void Can_change_column_to_non_nullable_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnMigration());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("NO", column.IsNullable);
        }

        private class AlterColumnWithDefaultMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(nullable: false, defaultValue: string.Empty));
            }
        }

        // UNDONE: Can't handle this yet (table rebuild)
        // [MigrationsTheory]
        public void Can_change_colum_to_non_nullable_column_with_default_value_when_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.Add(
                    new MigrationsCustomer
                        {
                            HomeAddress = new MigrationsAddress(),
                            WorkAddress = new MigrationsAddress(),
                            DateOfBirth = DateTime.Now
                        });
                context.SaveChanges();
            }

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithDefaultMigration());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("NO", column.IsNullable);
            Assert.True(column.Default.Contains("''"));
        }
    }
}
