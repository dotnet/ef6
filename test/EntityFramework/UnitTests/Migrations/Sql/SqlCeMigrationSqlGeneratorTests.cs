// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Sql
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class SqlCeMigrationSqlGeneratorTests
    {
        [Fact]
        public void Generate_should_throw_when_column_rename()
        {
            var migrationProvider = new SqlCeMigrationSqlGenerator();

            var renameColumnOperation = new RenameColumnOperation("T", "c", "c'");

            Assert.Equal(
                Strings.SqlCeColumnRenameNotSupported,
                Assert.Throws<MigrationsException>(() => migrationProvider.Generate(new[] { renameColumnOperation }, "4.0").ToList()).
                    Message);
        }

        [Fact]
        public void Generate_throws_when_operation_unknown()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();
            var unknownOperation = new Mock<MigrationOperation>(null).Object;

            var ex = Assert.Throws<InvalidOperationException>(
                () => migrationSqlGenerator.Generate(new[] { unknownOperation }, "4.0"));

            Assert.Equal(
                Strings.SqlServerMigrationSqlGenerator_UnknownOperation(typeof(SqlCeMigrationSqlGenerator).Name, unknownOperation.GetType().FullName),
                ex.Message);
        }
    }
}
