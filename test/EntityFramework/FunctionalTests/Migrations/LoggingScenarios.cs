// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;
    using Moq;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class LoggingScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void Can_log_provider_execution_warnings()
        {
            ResetDatabase();

            var mockLogger = new Mock<MigrationsLogger>();
            var migrator = CreateMigrator<ShopContext_v1>();

            var migratorLoggingDecorator
                = new MigratorLoggingDecorator(
                    migrator,
                    mockLogger.Object);

            migratorLoggingDecorator.Update();

            mockLogger
                .Verify(
                    ml => ml.Warning(
                        "Warning! The maximum key length is 900 bytes. The index 'PK_dbo.__MigrationHistory' has maximum length of 1534 bytes. For some combination of large values, the insert/update operation will fail."),
                    Times.Once());
        }
    }
}
