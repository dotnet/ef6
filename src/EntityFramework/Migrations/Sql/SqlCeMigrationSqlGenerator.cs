namespace System.Data.Entity.Migrations.Sql
{
    using System.Data.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     Provider to convert provider agnostic migration operations into SQL commands 
    ///     that can be run against Microsoft SQL Server Compact Edition.
    /// </summary>
    public class SqlCeMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        /// <inheritdoc />
        protected override DbConnection CreateConnection()
        {
            return DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0").CreateConnection();
        }

        /// <inheritdoc />
        protected override void GenerateCreateSchema(string schema)
        {
        }

        /// <inheritdoc />
        protected override void Generate(RenameColumnOperation renameColumnOperation)
        {
            throw Error.SqlCeColumnRenameNotSupported();
        }

        /// <inheritdoc />
        protected override void Generate(RenameTableOperation renameTableOperation)
        {
            using (var writer = Writer())
            {
                writer.Write("EXECUTE sp_rename @objname = N'");
                writer.Write(renameTableOperation.Name.Split(new[] { '.' }, 2).Last());
                writer.Write("', @newname = N'");
                writer.Write(renameTableOperation.NewName);
                writer.Write("', @objtype = N'OBJECT'");

                Statement(writer);
            }
        }

        /// <inheritdoc />
        protected override void Generate(MoveTableOperation moveTableOperation)
        {
        }

        /// <inheritdoc />
        protected override void GenerateMakeSystemTable(CreateTableOperation createTableOperation)
        {
        }

        /// <inheritdoc />
        protected override void Generate(DropColumnOperation dropColumnOperation)
        {
            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(dropColumnOperation.Table));
                writer.Write(" DROP COLUMN ");
                writer.Write(Quote(dropColumnOperation.Name));

                Statement(writer);
            }
        }

        /// <inheritdoc />
        protected override void Generate(DropIndexOperation dropIndexOperation)
        {
            using (var writer = Writer())
            {
                writer.Write("DROP INDEX ");
                writer.Write(Name(dropIndexOperation.Table));
                writer.Write(".");
                writer.Write(Quote(dropIndexOperation.Name));

                Statement(writer);
            }
        }

        /// <inheritdoc />
        protected override void Generate(AlterColumnOperation alterColumnOperation)
        {
            var column = alterColumnOperation.Column;

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(alterColumnOperation.Table));
                writer.Write(" ALTER COLUMN ");
                writer.Write(Quote(column.Name));
                writer.Write(" ");
                writer.Write(BuildColumnType(column));

                if ((column.IsNullable != null)
                    && !column.IsNullable.Value)
                {
                    writer.Write(" NOT NULL");
                }

                Statement(writer);
            }

            if ((column.DefaultValue != null)
                || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
            {
                using (var writer = Writer())
                {
                    writer.Write("ALTER TABLE ");
                    writer.Write(Name(alterColumnOperation.Table));
                    writer.Write(" ALTER COLUMN ");
                    writer.Write(Quote(column.Name));
                    writer.Write(" SET DEFAULT ");
                    writer.Write(
                        (column.DefaultValue != null)
                            ? Generate((dynamic)column.DefaultValue)
                            : column.DefaultValueSql
                        );

                    Statement(writer);
                }
            }
        }

        /// <inheritdoc />
        protected override string Generate(DateTime defaultValue)
        {
            return "'" + defaultValue.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";
        }

        /// <inheritdoc />
        protected override string Name(string name)
        {
            return Quote(name.Split(new[] { '.' }, 2).Last());
        }
    }
}