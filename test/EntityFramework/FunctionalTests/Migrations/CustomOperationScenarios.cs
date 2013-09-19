// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.CustomOperations;
    using System.Data.Entity.Migrations.Infrastructure;
    using Xunit;

    public class CustomOperationScenarios : FunctionalTestBase
    {
        [Fact]
        public void Can_use_custom_operations()
        {
            var configuration = new DbMigrationsConfiguration<EmptyModel>
                {
                    MigrationsAssembly = typeof(CustomOperationMigration).Assembly,
                    MigrationsNamespace = typeof(CustomOperationMigration).Namespace
                };
            configuration.SetSqlGenerator("System.Data.SqlClient", new CustomSqlGenerator());

            var migrator = new DbMigrator(configuration);
            var scriptor = new MigratorScriptingDecorator(migrator);

            var sql = scriptor.ScriptUpdate(DbMigrator.InitialDatabase, null);

            Assert.Contains("-- This is a test.", sql);
        }
    }
}
