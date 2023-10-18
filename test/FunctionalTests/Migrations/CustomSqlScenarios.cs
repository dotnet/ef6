// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NETFRAMEWORK

namespace System.Data.Entity.Migrations
{
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CustomSqlScenarios : DbTestCase
    {
        public CustomSqlScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class CustomSqlMigration : DbMigration
        {
            public override void Up()
            {
                Sql("CREATE TABLE [Foo](Id [int])");
            }
        }

        [MigrationsTheory]
        public void Can_update_when_migration_contains_custom_sql()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CustomSqlMigration());

            migrator.Update();

            Assert.True(TableExists("Foo"));
        }

        private class CustomSqlWithGoMigration : DbMigration
        {
            public override void Up()
            {
                Sql("create table [SomeTable] (id int, msg nvarchar(100));");
                Sql("insert into [SomeTable] (id, msg) values (1, 'click here to go to the next page');");
            }
        }

        [MigrationsTheory]
        public void Can_update_when_migration_contains_custom_sql_with_go()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new CustomSqlWithGoMigration());

            migrator.Update();

            Assert.True(TableExists("SomeTable"));
        }
    }
}

#endif
