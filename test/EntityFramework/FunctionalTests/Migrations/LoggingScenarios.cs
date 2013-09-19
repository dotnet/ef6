// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using Moq;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.TestHelpers;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class LoggingScenarios : DbTestCase
    {
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

            if (LocalizationTestHelpers.IsEnglishLocale())
            {
                mockLogger
                    .Verify(
                        ml => ml.Warning(
                            "Warning! The maximum key length is 900 bytes. The index 'PK_PkTooLong' has maximum length of 902 bytes. For some combination of large values, the insert/update operation will fail."),
                        Times.Once());
            }
            else
            {
                mockLogger.Verify(ml => ml.Warning(It.IsAny<string>()), Times.Once());
            }
        }
    }
}
