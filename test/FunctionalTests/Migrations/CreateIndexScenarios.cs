// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NET452
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CreateIndexScenarios : DbTestCase
    {
        public CreateIndexScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class CreateSimpleIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", "OrderId", unique: true, name: "IX_Custom_Name");
            }
        }

#if NET452
        [MigrationsTheory]
        public void Can_create_simple_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateSimpleIndexMigration());

            migrator.Update();
        }
#endif

        private class CreateCompositeIndexMigration : DbMigration
        {
            public override void Up()
            {
                CreateIndex("OrderLines", new[] { "ProductId", "Sku" }, true);
            }
        }

        [MigrationsTheory]
        public void Can_create_composite_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1, CreateCompositeIndexMigration>();

            migrator.Update();
        }

        [MigrationsTheory]
        public void Bug_49966_should_not_generate_duplicate_foreign_keys()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ProcessedTransactionContext>();

            migrator.Update();
        }

        private class CreateClusteredIndexMigration : DbMigration
        {
            public override void Up()
            {
                AddColumn("dbo.OrderLines", "Int", c => c.Int(name: "Boo"));

                var dropKey = new DropPrimaryKeyOperation { Table = "dbo.OrderLines" };
                dropKey.Columns.Add("Id");
                this.GetOperations().Add(dropKey);

                CreateIndex("OrderLines", "OrderId", name: "IX_Custom_Name", clustered: true);
            }
        }

#if NET452
        [MigrationsTheory]
        public void Can_create_clustered_index()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CreateClusteredIndexMigration());

            migrator.Update();
        }
#endif
    }
}
