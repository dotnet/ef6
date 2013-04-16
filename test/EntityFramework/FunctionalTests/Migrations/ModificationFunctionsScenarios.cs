// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class ModificationFunctionsScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void Auto_migration_when_functions_and_model_not_current_should_throw()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v2>();

            var generatedMigration = new MigrationScaffolder(migrator.Configuration).Scaffold("Migration");

            migrator = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: generatedMigration);

            ResetDatabase();

            Assert.Throws<MigrationsException>(() => migrator.Update())
                  .ValidateMessage("AutomaticStaleFunctions");
        }
    }
}
