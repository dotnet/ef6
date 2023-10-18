// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NETFRAMEWORK
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DatabaseGeneratedScenarios : DbTestCase
    {
        public DatabaseGeneratedScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        [MigrationsTheory]
        public void Can_auto_migrate_when_string_column_with_identity_database_generated_option()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v4>();

            migrator.Update();
        }
    }
}
