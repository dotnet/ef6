// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using Xunit;

    public class NoTestInfraScenarios : TestBase
    {
        [Fact]
        public void Can_generate_migration_from_user_code()
        {
            var migrator
                = new DbMigrator(
                    new DbMigrationsConfiguration
                        {
                            ContextType = typeof(ShopContext_v1),
                            MigrationsAssembly = SystemComponentModelDataAnnotationsAssembly,
                            MigrationsNamespace = "Foo",
                            MigrationsDirectory = "Bar"
                        });

            var migration = new MigrationScaffolder(migrator.Configuration).Scaffold("Test");

            Assert.False(string.IsNullOrWhiteSpace(migration.DesignerCode));
            Assert.False(string.IsNullOrWhiteSpace(migration.Language));
            Assert.False(string.IsNullOrWhiteSpace(migration.MigrationId));
            Assert.False(string.IsNullOrWhiteSpace(migration.UserCode));
            Assert.False(string.IsNullOrWhiteSpace(migration.Directory));
        }
    }
}
