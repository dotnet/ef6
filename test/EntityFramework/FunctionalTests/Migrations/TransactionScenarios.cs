// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class TransactionScenarios : DbTestCase
    {
        private class MigrationWithError : DbMigration
        {
            public override void Up()
            {
                Sql("CREATE TABLE [Foo](Id [int])");
                Sql("CREATE TABLE [Foo](Id [int])");
            }
        }

        [MigrationsTheory]
        public void Update_when_error_should_be_transactional()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new MigrationWithError());

            try
            {
                migrator.Update();
            }
            catch (SqlException)
            {
            }
            catch (SqlCeException)
            {
            }

            Assert.False(TableExists("Foo"));
        }
    }

    public class SqlClientTransactionScenarios : DbTestCase
    {
        private class MigrationWithNonTransactionalSql : DbMigration
        {
            public override void Up()
            {
                Sql("EXEC sp_grantlogin N'NT AUTHORITY\\NETWORK SERVICE'", suppressTransaction: true);
            }
        }

        [MigrationsTheory]
        public void Can_run_custom_sql_outside_of_transaction()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new MigrationWithNonTransactionalSql());

            migrator.Update();
        }
    }
}
