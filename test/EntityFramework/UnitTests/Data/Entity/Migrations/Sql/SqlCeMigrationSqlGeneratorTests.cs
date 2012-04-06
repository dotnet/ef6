namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class SqlCeMigrationSqlGeneratorTests
    {
        [Fact(Skip = "No CE Provider")]
        public void Generate_should_throw_when_column_rename()
        {
            var migrationProvider = new SqlCeMigrationSqlGenerator();

            var renameColumnOperation = new RenameColumnOperation("T", "c", "c'");

            Assert.Equal(Strings.SqlCeColumnRenameNotSupported, Assert.Throws<MigrationsException>(() => migrationProvider.Generate(new[] { renameColumnOperation }, "4.0").ToList()).Message);
        }
    }
}