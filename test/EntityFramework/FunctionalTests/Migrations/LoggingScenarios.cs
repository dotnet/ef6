// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Migrations
{
    using Moq;
    using System.Data.Entity.Migrations.Infrastructure;
    
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class LoggingScenarios : DbTestCase
    {
        public LoggingScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class WarningMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "PkTooLong",
                    t => new
                             {
                                 Id = t.String(maxLength: 451)
                             })
                    .PrimaryKey(t => t.Id);
            }
        }

        [MigrationsTheory]
        public void Can_log_provider_execution_warnings()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var mockLogger = new Mock<MigrationsLogger>();

            migrator = CreateMigrator<ShopContext_v1>(new WarningMigration());

            var migratorLoggingDecorator
                = new MigratorLoggingDecorator(
                    migrator,
                    mockLogger.Object);

            migratorLoggingDecorator.Update();

            // Not checking actual string anymore since it changes based on version of SqlClient used
            mockLogger.Verify(ml => ml.Warning(It.IsAny<string>()), Times.Once());
        }
    }
}

#endif
