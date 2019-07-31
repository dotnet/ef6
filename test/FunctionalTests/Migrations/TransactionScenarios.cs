// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Migrations
{
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Security.Principal;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class TransactionScenarios : DbTestCase
    {
        public TransactionScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

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
        public SqlClientTransactionScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class MigrationWithNonTransactionalSql : DbMigration
        {
            public override void Up()
            {
                var account = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null).Translate(typeof(NTAccount)).Value;
                Sql(string.Format("EXEC sp_grantlogin N'{0}'", account), suppressTransaction: true);
            }
        }

        [MigrationsTheory(SkipForSqlAzure = true, Justification = "sp_grantlogin is not supported on Sql Azure")]
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

#endif
