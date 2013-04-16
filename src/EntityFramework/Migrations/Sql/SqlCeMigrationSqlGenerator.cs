// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Sql
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    ///     Provider to convert provider agnostic migration operations into SQL commands
    ///     that can be run against Microsoft SQL Server Compact Edition.
    /// </summary>
    [DbProviderName("System.Data.SqlServerCe.4.0")]
    public class SqlCeMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        /// <inheritdoc />
        protected override DbConnection CreateConnection()
        {
            return DbConfiguration.GetService<DbProviderFactory>("System.Data.SqlServerCe.4.0").CreateConnection();
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
                writer.Write(renameTableOperation.Name.ToDatabaseName().Name);
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
        protected override void GenerateMakeSystemTable(CreateTableOperation createTableOperation, IndentedTextWriter writer)
        {
        }

        /// <inheritdoc />
        protected override void Generate(CreateProcedureOperation createProcedureOperation)
        {
        }

        /// <inheritdoc />
        protected override void Generate(DropProcedureOperation dropProcedureOperation)
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
            return "'" + defaultValue.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";
        }

        /// <inheritdoc />
        protected override string Name(string name)
        {
            return Quote(name.ToDatabaseName().Name);
        }

        /// <summary>
        ///     Returns the column default value to use for store-generated GUID columns when
        ///     no default value is explicitly specified in the migration.
        ///     Always returns newid() for SQL Compact.
        /// </summary>
        /// <value>The string newid().</value>
        protected override string GuidColumnDefault
        {
            get { return "newid()"; }
        }
    }
}
