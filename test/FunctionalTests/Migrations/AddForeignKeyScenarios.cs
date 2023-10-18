// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NETFRAMEWORK

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AddForeignKeyScenarios : DbTestCase
    {
        public AddForeignKeyScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class AddSimpleForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", "OrderId", "ordering.Orders", "OrderId", name: "FK_Custom_Name");
            }
        }

        [MigrationsTheory]
        public void Can_add_simple_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddSimpleForeignKeyMigration());

            migrator.Update();

            var foreignKey = Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(rc => rc.Name == "FK_Custom_Name");
            Assert.NotNull(foreignKey);
            Assert.Equal(1, foreignKey.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.KeyColumnUsages.Any(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "OrderId"));
            Assert.Equal(1, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            var keyColumnUsage =
                foreignKey.UniqueConstraint.KeyColumnUsages.SingleOrDefault(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "Orders" && kcu.ColumnName == "OrderId");
            Assert.NotNull(keyColumnUsage);
            WhenNotSqlCe(() => Assert.Equal("ordering", keyColumnUsage.ColumnTableSchema));
        }

        private class AddCompositeForeignKeyMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts", new[] { "ProductId", "Sku" });
            }
        }

        [MigrationsTheory]
        public void Can_add_composite_foreign_key_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddCompositeForeignKeyMigration());

            migrator.Update();

            var foreignKey =
                Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(
                    rc => rc.Name == "FK_OrderLines_MigrationsProducts_ProductId_Sku");
            Assert.NotNull(foreignKey);
            Assert.Equal(2, foreignKey.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.KeyColumnUsages.Any(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "ProductId"));
            Assert.True(
                foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "Sku"));
            Assert.Equal(2, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "ProductId"));
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(
                    kcu => kcu.Position == 2 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "Sku"));
        }

        private class AddForeignKeyNoPrincipalMigration : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("OrderLines", new[] { "ProductId", "Sku" }, "MigrationsProducts");
            }
        }

        [MigrationsTheory]
        public void Can_add_composite_foreign_key_constraint_when_principal_columns_not_specified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AddForeignKeyNoPrincipalMigration());

            migrator.Update();

            var foreignKey =
                Info.TableConstraints.OfType<ReferentialConstraintInfo>().SingleOrDefault(
                    rc => rc.Name == "FK_OrderLines_MigrationsProducts_ProductId_Sku");
            Assert.NotNull(foreignKey);
            Assert.Equal(2, foreignKey.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.KeyColumnUsages.Any(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "ProductId"));
            Assert.True(
                foreignKey.KeyColumnUsages.Any(kcu => kcu.Position == 2 && kcu.ColumnTableName == "OrderLines" && kcu.ColumnName == "Sku"));
            Assert.Equal(2, foreignKey.UniqueConstraint.KeyColumnUsages.Count());
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(
                    kcu => kcu.Position == 1 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "ProductId"));
            Assert.True(
                foreignKey.UniqueConstraint.KeyColumnUsages.Any(
                    kcu => kcu.Position == 2 && kcu.ColumnTableName == "MigrationsProducts" && kcu.ColumnName == "Sku"));
        }

        private class CustomSchemaContext : DbContext
        {
            private class Foo
            {
                public int Id { get; set; }
            }

            private class Bar
            {
                public int Id { get; set; }
                public int FooId { get; set; }
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("");
                modelBuilder.Entity<Foo>();
                modelBuilder.Entity<Bar>();
            }
        }

        private class AddFKCustomSchema : DbMigration
        {
            public override void Up()
            {
                AddForeignKey("Bars", new[] { "FooId" }, "Foos");
            }
        }

        [MigrationsTheory]
        public void Can_add_composite_foreign_key_constraint_when_principal_columns_not_specified_when_default_schema()
        {
            ResetDatabase();

            var migrator = CreateMigrator<CustomSchemaContext>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration1");

            migrator = CreateMigrator<CustomSchemaContext>(scaffoldedMigrations: generatedMigration);

            migrator.Update();

            migrator = CreateMigrator<CustomSchemaContext>(new AddFKCustomSchema());

            migrator.Update();
        }
    }
}

#endif
