// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class DbPermissionsScenarios : DbTestCase
    {
        public DbPermissionsScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        [MigrationsTheory]
        public void GetDatabaseMigrations_when_not_permissioned()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            CreateNotPermissionedConfig(migrator);

            migrator = new DbMigrator(migrator.Configuration);

            Assert.Throws<EntityCommandExecutionException>(() => migrator.GetDatabaseMigrations());
        }

        [MigrationsTheory]
        public void Update_when_not_permissioned()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            CreateNotPermissionedConfig(migrator);

            migrator = new DbMigrator(migrator.Configuration);

            Assert.Throws<EntityCommandExecutionException>(() => migrator.Update());
        }

        private void CreateNotPermissionedConfig(DbMigrator migrator)
        {
            TestDatabase.ExecuteNonQuery(
                @"IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'EFDDLAdminOnly')
BEGIN
    CREATE LOGIN [EFDDLAdminOnly] WITH PASSWORD=N'PLACEHOLDER', DEFAULT_DATABASE=[MigrationsTest]
END

USE [MigrationsTest]

IF USER_ID('EFDDLAdminOnly') IS NULL
BEGIN
    CREATE USER [EFDDLAdminOnly] FOR LOGIN [EFDDLAdminOnly]
    ALTER AUTHORIZATION ON SCHEMA::[db_ddladmin] TO [EFDDLAdminOnly]
END
");

            var connectionStringBuilder
                = new SqlConnectionStringBuilder(TestDatabase.ConnectionString)
                {
                    IntegratedSecurity = false,
                    UserID = "EFDDLAdminOnly",
                    Password = "PLACEHOLDER"
                };

            migrator.Configuration.TargetDatabase
                = new DbConnectionInfo(connectionStringBuilder.ToString(), TestDatabase.ProviderName);
        }
    }
}
