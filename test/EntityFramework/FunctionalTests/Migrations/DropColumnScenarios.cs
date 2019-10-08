// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Migrations
{
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NET452
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DropColumnScenarios : DbTestCase
    {
        public DropColumnScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class DropColumnMigration : DbMigration
        {
            public override void Up()
            {
                DropColumn("MigrationsProducts", "Name");
            }
        }

        [MigrationsTheory]
        public void Can_drop_column()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            Assert.True(ColumnExists("MigrationsProducts", "Name"));

            migrator = CreateMigrator<ShopContext_v1>(new DropColumnMigration());

            migrator.Update();

            Assert.False(ColumnExists("MigrationsProducts", "Name"));
        }
    }
}

#endif
