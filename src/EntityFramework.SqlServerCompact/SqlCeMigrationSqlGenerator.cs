// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if SQLSERVERCOMPACT35
namespace System.Data.Entity.SqlServerCompact.Legacy
#else
namespace System.Data.Entity.SqlServerCompact
#endif
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.Entity.SqlServerCompact.SqlGen;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provider to convert provider agnostic migration operations into SQL commands
    /// that can be run against a Microsoft SQL Server Compact Edition database.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class SqlCeMigrationSqlGenerator : MigrationSqlGenerator
    {
        private const string BatchTerminator = "GO";

        internal const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private List<MigrationStatement> _statements;

        /// <summary>
        /// Converts a set of migration operations into Microsoft SQL Server specific SQL.
        /// </summary>
        /// <param name="migrationOperations"> The operations to be converted. </param>
        /// <param name="providerManifestToken"> Token representing the version of SQL Server being targeted (i.e. "2005", "2008"). </param>
        /// <returns> A list of SQL statements to be executed to perform the migration operations. </returns>
        public override IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            Check.NotNull(migrationOperations, "migrationOperations");
            Check.NotNull(providerManifestToken, "providerManifestToken");

            _statements = new List<MigrationStatement>();

            InitializeProviderServices(providerManifestToken);
            GenerateStatements(migrationOperations);

            return _statements;
        }

        private void GenerateStatements(IEnumerable<MigrationOperation> migrationOperations)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            migrationOperations.Each<dynamic>(o => Generate(o));
        }

        private void InitializeProviderServices(string providerManifestToken)
        {
            Check.NotEmpty(providerManifestToken, "providerManifestToken");

            using (var connection = CreateConnection())
            {
                ProviderManifest
                    = DbProviderServices
                        .GetProviderServices(connection)
                        .GetProviderManifest(providerManifestToken);
            }
        }

        /// <summary>
        /// Generates the specified update database operation which represents applying a series of migrations.
        /// The generated script is idempotent, meaning it contains conditional logic to check if individual migrations 
        /// have already been applied and only apply the pending ones.
        /// </summary>
        /// <param name="updateDatabaseOperation">The update database operation.</param>
        protected virtual void Generate(UpdateDatabaseOperation updateDatabaseOperation)
        {
            Check.NotNull(updateDatabaseOperation, "updateDatabaseOperation");

            GenerateStatements(updateDatabaseOperation.Migrations.SelectMany(m => m.Operations));
        }

        /// <summary>
        /// Generates SQL for a <see cref="MigrationOperation" />.
        /// Allows derived providers to handle additional operation types.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="migrationOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(MigrationOperation migrationOperation)
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            throw Error.SqlServerMigrationSqlGenerator_UnknownOperation(GetType().Name, migrationOperation.GetType().FullName);
        }

        /// <summary>
        /// Creates an empty connection for the current provider.
        /// Allows derived providers to use connection other than <see cref="SqlConnection" />.
        /// </summary>
        /// <returns> An empty connection for the current provider. </returns>
        protected virtual DbConnection CreateConnection()
        {
            return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(SqlCeProviderManifest.ProviderInvariantName).CreateConnection();
        }

        /// <summary>
        /// Generates the specified create procedure operation.
        /// </summary>
        /// <param name="createProcedureOperation">The create procedure operation.</param>
        protected virtual void Generate(CreateProcedureOperation createProcedureOperation)
        {
        }

        /// <summary>
        /// Generates the specified alter procedure operation.
        /// </summary>
        /// <param name="alterProcedureOperation">The alter procedure operation.</param>
        protected virtual void Generate(AlterProcedureOperation alterProcedureOperation)
        {
        }

        /// <summary>
        /// Generates the specified drop procedure operation.
        /// </summary>
        /// <param name="dropProcedureOperation">The drop procedure operation.</param>
        protected virtual void Generate(DropProcedureOperation dropProcedureOperation)
        {
        }

        /// <summary>
        /// Generates SQL for a <see cref="CreateTableOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="createTableOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(CreateTableOperation createTableOperation)
        {
            Check.NotNull(createTableOperation, "createTableOperation");

            using (var writer = Writer())
            {
                WriteCreateTable(createTableOperation, writer);

                Statement(writer);
            }
        }

        private void WriteCreateTable(CreateTableOperation createTableOperation, IndentedTextWriter writer)
        {
            DebugCheck.NotNull(createTableOperation);
            DebugCheck.NotNull(writer);

            writer.WriteLine("CREATE TABLE " + Name(createTableOperation.Name) + " (");
            writer.Indent++;

            createTableOperation.Columns.Each(
                (c, i) =>
                {
                    Generate(c, writer);

                    if (i < createTableOperation.Columns.Count - 1)
                    {
                        writer.WriteLine(",");
                    }
                });

            if (createTableOperation.PrimaryKey != null)
            {
                writer.WriteLine(",");
                writer.Write("CONSTRAINT ");
                writer.Write(Quote(createTableOperation.PrimaryKey.Name));
                writer.Write(" PRIMARY KEY ");

                if (!createTableOperation.PrimaryKey.IsClustered)
                {
                    writer.Write("NONCLUSTERED ");
                }

                writer.Write("(");
                writer.Write(createTableOperation.PrimaryKey.Columns.Join(Quote));
                writer.WriteLine(")");
            }
            else
            {
                writer.WriteLine();
            }

            writer.Indent--;
            writer.Write(")");
        }

        /// <summary>
        /// Override this method to generate SQL when the definition of a table or its attributes are changed.
        /// The default implementation of this method does nothing.
        /// </summary>
        /// <param name="alterTableOperation"> The operation describing changes to the table. </param>
        protected internal virtual void Generate(AlterTableOperation alterTableOperation)
        {
            Check.NotNull(alterTableOperation, "alterTableOperation");

            // Nothing to do since there is no inherent semantics associated with annotations
        }
        
        /// <summary>
        /// Generates SQL to mark a table as a system table.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="createTableOperation"> The table to mark as a system table. </param>
        /// <param name="writer"> The <see cref='IndentedTextWriter' /> to write the generated SQL to. </param>
        protected virtual void GenerateMakeSystemTable(CreateTableOperation createTableOperation, IndentedTextWriter writer)
        {
        }

        /// <summary>
        /// Generates SQL for a <see cref="AddForeignKeyOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="addForeignKeyOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(AddForeignKeyOperation addForeignKeyOperation)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(addForeignKeyOperation.DependentTable));
                writer.Write(" ADD CONSTRAINT ");
                writer.Write(Quote(addForeignKeyOperation.Name));
                writer.Write(" FOREIGN KEY (");
                writer.Write(addForeignKeyOperation.DependentColumns.Select(Quote).Join());
                writer.Write(") REFERENCES ");
                writer.Write(Name(addForeignKeyOperation.PrincipalTable));
                writer.Write(" (");
                writer.Write(addForeignKeyOperation.PrincipalColumns.Select(Quote).Join());
                writer.Write(")");

                if (addForeignKeyOperation.CascadeDelete)
                {
                    writer.Write(" ON DELETE CASCADE");
                }

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="DropForeignKeyOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="dropForeignKeyOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(DropForeignKeyOperation dropForeignKeyOperation)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(dropForeignKeyOperation.DependentTable));
                writer.Write(" DROP CONSTRAINT ");
                writer.Write(Quote(dropForeignKeyOperation.Name));

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="CreateIndexOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="createIndexOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(CreateIndexOperation createIndexOperation)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");

            using (var writer = Writer())
            {
                writer.Write("CREATE ");

                if (createIndexOperation.IsUnique)
                {
                    writer.Write("UNIQUE ");
                }

                // Note: SQL CE only supports NONCLUSTERED, so ignore clustered config

                writer.Write("INDEX ");
                writer.Write(Quote(createIndexOperation.Name));
                writer.Write(" ON ");
                writer.Write(Name(createIndexOperation.Table));
                writer.Write("(");
                writer.Write(createIndexOperation.Columns.Join(Quote));
                writer.Write(")");

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="DropIndexOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="dropIndexOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(DropIndexOperation dropIndexOperation)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");

            using (var writer = Writer())
            {
                writer.Write("DROP INDEX ");
                writer.Write(Name(dropIndexOperation.Table));
                writer.Write(".");
                writer.Write(Quote(dropIndexOperation.Name));

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="AddPrimaryKeyOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="addPrimaryKeyOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(addPrimaryKeyOperation.Table));
                writer.Write(" ADD CONSTRAINT ");
                writer.Write(Quote(addPrimaryKeyOperation.Name));
                writer.Write(" PRIMARY KEY ");

                if (!addPrimaryKeyOperation.IsClustered)
                {
                    writer.Write("NONCLUSTERED ");
                }

                writer.Write("(");
                writer.Write(addPrimaryKeyOperation.Columns.Select(Quote).Join());
                writer.Write(")");

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="DropPrimaryKeyOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="dropPrimaryKeyOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(dropPrimaryKeyOperation.Table));
                writer.Write(" DROP CONSTRAINT ");
                writer.Write(Quote(dropPrimaryKeyOperation.Name));

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="AddColumnOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="addColumnOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(AddColumnOperation addColumnOperation)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(addColumnOperation.Table));
                writer.Write(" ADD ");

                var column = addColumnOperation.Column;

                Generate(column, writer);

                if ((column.IsNullable != null)
                    && !column.IsNullable.Value
                    && (column.DefaultValue == null)
                    && (string.IsNullOrWhiteSpace(column.DefaultValueSql))
                    && !column.IsIdentity
                    && !column.IsTimestamp
                    && !column.StoreType.EqualsIgnoreCase("rowversion")
                    && !column.StoreType.EqualsIgnoreCase("timestamp"))
                {
                    writer.Write(" DEFAULT ");

                    if (column.Type == PrimitiveTypeKind.DateTime)
                    {
                        writer.Write(Generate(DateTime.Parse("1900-01-01 00:00:00", CultureInfo.InvariantCulture)));
                    }
                    else
                    {
                        writer.Write(Generate((dynamic)column.ClrDefaultValue));
                    }
                }

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="AddOrUpdateOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="addOrUpdateOperation">The operation to produce SQL for.</param>
        protected virtual void Generate(AddOrUpdateOperation addOrUpdateOperation)
        {
            Check.NotNull(addOrUpdateOperation, "addOrUpdateOperation");

            var columns = addOrUpdateOperation.Columns.ToList();
            var identifiers = columns.Intersect(addOrUpdateOperation.Identifiers).ToList();
            var values = addOrUpdateOperation.Values.ToList();

            using (var writer = Writer())
            {
                writer.Write("IF object_id('");
                writer.Write(Name(addOrUpdateOperation.Table));
                writer.WriteLine("') IS NOT NULL");

                if (identifiers.Any())
                {
                    writer.Write(" IF EXISTS (SELECT 1 FROM ");
                    writer.Write(Name(addOrUpdateOperation.Table));
                    writer.Write(" WHERE ");

                    writer.Write(
                        identifiers.Join(
                            identifier =>
                                string.Join(
                                    " = ", Quote(identifier),
                                    Generate((dynamic)values[columns.IndexOf(identifier)])),
                            " AND "));

                    writer.Write(") UPDATE ");
                    writer.Write(Name(addOrUpdateOperation.Table));
                    writer.Write(" SET ");

                    writer.Write(
                        addOrUpdateOperation.Columns.Join(
                            column =>
                                string.Join(
                                    " = ", Quote(column),
                                    Generate((dynamic)values[columns.IndexOf(column)]))));

                    writer.Write(" WHERE ");

                    writer.Write(
                        identifiers.Join(
                            identifier =>
                                string.Join(
                                    " = ", Quote(identifier),
                                    Generate((dynamic)values[columns.IndexOf(identifier)])),
                            " AND "));

                    writer.WriteLine(" ELSE");
                }

                writer.Write(" INSERT INTO ");
                writer.Write(Name(addOrUpdateOperation.Table));
                writer.Write("(");
                writer.Write(columns.Join(Quote));
                writer.Write(") VALUES (");
                writer.Write(values.Join(value => Generate((dynamic)value)));
                writer.Write(")");

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="DeleteOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="deleteOperation">The operation to produce SQL for.</param>
        protected virtual void Generate(DeleteOperation deleteOperation)
        {
            Check.NotNull(deleteOperation, "deleteOperation");

            using (var writer = Writer())
            {
                writer.Write("IF object_id('");
                writer.Write(Name(deleteOperation.Table));
                writer.WriteLine("') IS NOT NULL");

                writer.Write(" DELETE FROM ");
                writer.Write(Name(deleteOperation.Table));
                writer.Write(" WHERE ");

                for (var i = 0; i < deleteOperation.Columns.Count(); i++)
                {
                    var column = deleteOperation.Columns[i];
                    var values = deleteOperation.Values[i];


                    var joinedValues = values.Any() ?
                        string.Join(" OR ", values.Select(value => string.Concat(Quote(column), value != null ? string.Concat(" = ", Generate((dynamic)value)) : " IS NULL")))
                        : string.Empty;

                    if (!string.IsNullOrEmpty(joinedValues))
                    {
                        if (i > 0) writer.Write(" AND ");

                        writer.Write("(");
                        writer.Write(joinedValues);
                        writer.Write(")");
                    }
                }

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="DropColumnOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="dropColumnOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(DropColumnOperation dropColumnOperation)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(dropColumnOperation.Table));
                writer.Write(" DROP COLUMN ");
                writer.Write(Quote(dropColumnOperation.Name));

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="AlterColumnOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="alterColumnOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(AlterColumnOperation alterColumnOperation)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

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
                    writer.Write(" NOT");
                }

                writer.Write(" NULL");

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
                    writer.Write(" DROP DEFAULT");

                    Statement(writer);
                }

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

        /// <summary>
        /// Generates SQL for a <see cref="DropTableOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="dropTableOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(DropTableOperation dropTableOperation)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");

            using (var writer = Writer())
            {
                writer.Write("DROP TABLE ");
                writer.Write(Name(dropTableOperation.Name));

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="SqlOperation" />.
        /// Generated SQL should be added using the Statement or StatementBatch methods.
        /// </summary>
        /// <param name="sqlOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(SqlOperation sqlOperation)
        {
            Check.NotNull(sqlOperation, "sqlOperation");

            StatementBatch(sqlOperation.Sql, sqlOperation.SuppressTransaction);
        }

        /// <summary>
        /// Generates SQL for a <see cref="RenameColumnOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="renameColumnOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(RenameColumnOperation renameColumnOperation)
        {
            throw Error.SqlCeColumnRenameNotSupported();
        }

        /// <summary>
        /// Generates SQL for a <see cref="RenameIndexOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="renameIndexOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(RenameIndexOperation renameIndexOperation)
        {
            throw Error.SqlCeIndexRenameNotSupported();
        }

        /// <summary>
        /// Generates SQL for a <see cref="RenameTableOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="renameTableOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(RenameTableOperation renameTableOperation)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");

            using (var writer = Writer())
            {
                writer.Write("EXECUTE sp_rename @objname = N'");
                writer.Write(Escape(DatabaseName.Parse(renameTableOperation.Name).Name));
                writer.Write("', @newname = N'");
                writer.Write(Escape(renameTableOperation.NewName));
                writer.Write("', @objtype = N'OBJECT'");

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates the specified rename procedure operation.
        /// </summary>
        /// <param name="renameProcedureOperation">The rename procedure operation.</param>
        protected virtual void Generate(RenameProcedureOperation renameProcedureOperation)
        {
        }

        /// <summary>
        /// Generates the specified move procedure operation.
        /// </summary>
        /// <param name="moveProcedureOperation">The move procedure operation.</param>
        protected virtual void Generate(MoveProcedureOperation moveProcedureOperation)
        {
        }

        /// <summary>
        /// Generates SQL for a <see cref="MoveTableOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="moveTableOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(MoveTableOperation moveTableOperation)
        {
        }

        /// <summary>
        /// Generates SQL for the given column model. This method is called by other methods that
        /// process columns and can be overridden to change the SQL generated.
        /// </summary>
        /// <param name="column">The column for which SQL is being generated.</param>
        /// <param name="writer">The writer to which generated SQL should be written.</param>
        protected internal void Generate(ColumnModel column, IndentedTextWriter writer)
        {
            Check.NotNull(column, "column");
            Check.NotNull(writer, "writer");

            writer.Write(Quote(column.Name));
            writer.Write(" ");
            writer.Write(BuildColumnType(column));

            if ((column.IsNullable != null)
                && !column.IsNullable.Value)
            {
                writer.Write(" NOT NULL");
            }

            if (column.DefaultValue != null)
            {
                writer.Write(" DEFAULT ");
                writer.Write(Generate((dynamic)column.DefaultValue));
            }
            else if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
            {
                writer.Write(" DEFAULT ");
                writer.Write(column.DefaultValueSql);
            }
            else if (column.IsIdentity)
            {
                if ((column.Type == PrimitiveTypeKind.Guid)
                    && (column.DefaultValue == null))
                {
                    writer.Write(" DEFAULT " + GuidColumnDefault);
                }
                else
                {
                    writer.Write(" IDENTITY");
                }
            }
        }

        /// <summary>
        /// Returns the column default value to use for store-generated GUID columns when
        /// no default value is explicitly specified in the migration.
        /// Always returns newid() for SQL Compact.
        /// </summary>
        /// <value>The string newid().</value>
        protected virtual string GuidColumnDefault
        {
            get { return "newid()"; }
        }

        /// <summary>
        /// Generates SQL for a <see cref="HistoryOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="historyOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(HistoryOperation historyOperation)
        {
            Check.NotNull(historyOperation, "historyOperation");

            using (var writer = Writer())
            {
                historyOperation.CommandTrees.Each(
                    commandTree =>
                    {
                        List<DbParameter> _;

                        switch (commandTree.CommandTreeKind)
                        {
                            case DbCommandTreeKind.Insert:

                                writer.Write(
                                    string.Join(
                                        Environment.NewLine,
                                        DmlSqlGenerator.GenerateInsertSql(
                                            (DbInsertCommandTree)commandTree,
                                            out _,
                                            isLocalProvider: true,
                                            upperCaseKeywords: true,
                                            createParameters: false)));
                                break;

                            case DbCommandTreeKind.Delete:
                                writer.Write(
                                    string.Join(
                                        Environment.NewLine,
                                        DmlSqlGenerator.GenerateDeleteSql(
                                            (DbDeleteCommandTree)commandTree,
                                            out _,
                                            isLocalProvider: true,
                                            upperCaseKeywords: true,
                                            createParameters: false)));
                                break;
                        }
                    });

                Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL to specify a constant byte[] default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(byte[] defaultValue)
        {
            Check.NotNull(defaultValue, "defaultValue");

            return "0x" + defaultValue.ToHexString();
        }

        /// <summary>
        /// Generates SQL to specify a constant bool default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(bool defaultValue)
        {
            return defaultValue ? "1" : "0";
        }

        /// <summary>
        /// Generates SQL to specify a constant DateTime default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(DateTime defaultValue)
        {
            return "'" + defaultValue.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant DateTimeOffset default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(DateTimeOffset defaultValue)
        {
            return "'" + defaultValue.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture) + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant Guid default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(Guid defaultValue)
        {
            return "'" + defaultValue + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant string default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(string defaultValue)
        {
            Check.NotNull(defaultValue, "defaultValue");

            return "'" + defaultValue + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant TimeSpan default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(TimeSpan defaultValue)
        {
            return "'" + defaultValue + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant geogrpahy default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(DbGeography defaultValue)
        {
            return "'" + defaultValue + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant geometry default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(DbGeometry defaultValue)
        {
            return "'" + defaultValue + "'";
        }

        /// <summary>
        /// Generates SQL to specify a constant default value being set on a column.
        /// This method just generates the actual value, not the SQL to set the default value.
        /// </summary>
        /// <param name="defaultValue"> The value to be set. </param>
        /// <returns> SQL representing the default value. </returns>
        protected virtual string Generate(object defaultValue)
        {
            Check.NotNull(defaultValue, "defaultValue");
            Debug.Assert(defaultValue.GetType().IsValueType);

            return string.Format(CultureInfo.InvariantCulture, "{0}", defaultValue);
        }

        /// <summary>
        /// Generates SQL to specify the data type of a column.
        /// This method just generates the actual type, not the SQL to create the column.
        /// </summary>
        /// <param name="columnModel"> The definition of the column. </param>
        /// <returns> SQL representing the data type. </returns>
        protected virtual string BuildColumnType(ColumnModel columnModel)
        {
            Check.NotNull(columnModel, "columnModel");

            if (columnModel.IsTimestamp)
            {
                return "rowversion";
            }

            return BuildPropertyType(columnModel);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private string BuildPropertyType(PropertyModel propertyModel)
        {
            DebugCheck.NotNull(propertyModel);

            var originalStoreTypeName = propertyModel.StoreType;
            var typeUsage = ProviderManifest.GetStoreType(propertyModel.TypeUsage);

            if (string.IsNullOrWhiteSpace(originalStoreTypeName))
            {
                originalStoreTypeName = typeUsage.EdmType.Name;
            }
            else
            {
                var storeTypeUsage = BuildStoreTypeUsage(originalStoreTypeName, propertyModel);

                typeUsage = storeTypeUsage ?? typeUsage;
            }

            var storeTypeName = originalStoreTypeName;

            const string MaxSuffix = "(max)";

            if (storeTypeName.EndsWith(MaxSuffix, StringComparison.Ordinal))
            {
                storeTypeName = Quote(storeTypeName.Substring(0, storeTypeName.Length - MaxSuffix.Length)) + MaxSuffix;
            }
            else
            {
                storeTypeName = Quote(storeTypeName);
            }

            switch (originalStoreTypeName)
            {
                case "decimal":
                case "numeric":
                    storeTypeName += "(" +
                                     (propertyModel.Precision ?? (byte)typeUsage.Facets[DbProviderManifest.PrecisionFacetName].Value)
                                     + ", " + (propertyModel.Scale ?? (byte)typeUsage.Facets[DbProviderManifest.ScaleFacetName].Value) + ")";
                    break;
                case "datetime2":
                case "datetimeoffset":
                case "time":
                    storeTypeName += "(" + (propertyModel.Precision ?? (byte)typeUsage.Facets[DbProviderManifest.PrecisionFacetName].Value) + ")";
                    break;
                case "binary":
                case "varbinary":
                case "nvarchar":
                case "varchar":
                case "char":
                case "nchar":
                    storeTypeName += "(" + (propertyModel.MaxLength ?? (int)typeUsage.Facets[DbProviderManifest.MaxLengthFacetName].Value) + ")";
                    break;
            }

            return storeTypeName;
        }

        /// <summary>
        /// Generates a quoted name. The supplied name may or may not contain the schema.
        /// </summary>
        /// <param name="name"> The name to be quoted. </param>
        /// <returns> The quoted name. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        protected virtual string Name(string name)
        {
            Check.NotEmpty(name, "name");

            return Quote(DatabaseName.Parse(name).Name);
        }

        /// <summary>
        /// Quotes an identifier for SQL Server.
        /// </summary>
        /// <param name="identifier"> The identifier to be quoted. </param>
        /// <returns> The quoted identifier. </returns>
        protected virtual string Quote(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return SqlGenerator.QuoteIdentifier(identifier);
        }

        private static string Escape(string s)
        {
            DebugCheck.NotEmpty(s);

            return s.Replace("'", "''");
        }

        /// <summary>
        /// Adds a new Statement to be executed against the database.
        /// </summary>
        /// <param name="sql"> The statement to be executed. </param>
        /// <param name="suppressTransaction"> Gets or sets a value indicating whether this statement should be performed outside of the transaction scope that is used to make the migration process transactional. If set to true, this operation will not be rolled back if the migration process fails. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected void Statement(string sql, bool suppressTransaction = false)
        {
            Check.NotEmpty(sql, "sql");

            _statements.Add(
                new MigrationStatement
                    {
                        Sql = sql,
                        SuppressTransaction = suppressTransaction,
                        BatchTerminator = BatchTerminator
                    });
        }

        /// <summary>
        /// Gets a new <see cref="IndentedTextWriter" /> that can be used to build SQL.
        /// This is just a helper method to create a writer. Writing to the writer will
        /// not cause SQL to be registered for execution. You must pass the generated
        /// SQL to the Statement method.
        /// </summary>
        /// <returns> An empty text writer to use for SQL generation. </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected static IndentedTextWriter Writer()
        {
            return new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a new Statement to be executed against the database.
        /// </summary>
        /// <param name="writer"> The writer containing the SQL to be executed. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected void Statement(IndentedTextWriter writer)
        {
            Check.NotNull(writer, "writer");

            Statement(writer.InnerWriter.ToString());
        }

        /// <summary>
        /// Breaks string into one or more statements, handling T-SQL utility statements as necessary.
        /// </summary>
        /// <param name="sqlBatch">The SQL to split into one ore more statements to be executed.</param>
        /// <param name="suppressTransaction"> Gets or sets a value indicating whether this statement should be performed outside of the transaction scope that is used to make the migration process transactional. If set to true, this operation will not be rolled back if the migration process fails. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected void StatementBatch(string sqlBatch, bool suppressTransaction = false)
        {
            Check.NotNull(sqlBatch, "sqlBatch");

            // Handle backslash utility statement (see http://technet.microsoft.com/en-us/library/dd207007.aspx)
            sqlBatch = Regex.Replace(sqlBatch, @"\\(\r\n|\r|\n)", "");

            // Handle batch splitting utility statement (see http://technet.microsoft.com/en-us/library/ms188037.aspx)
            var batches = Regex.Split(sqlBatch, 
                String.Format(CultureInfo.InvariantCulture, @"^\s*({0}[ \t]+[0-9]+|{0})(?:\s+|$)", BatchTerminator),
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            for (int i = 0; i < batches.Length; ++i)
            {
                // Skip batches that merely contain the batch terminator
                if (batches[i].StartsWith(BatchTerminator, StringComparison.OrdinalIgnoreCase) || 
                    (i == batches.Length - 1 && string.IsNullOrWhiteSpace(batches[i])))
                {
                    continue;
                }

                int repeatCount = 1;
                
                // Handle count parameter on the batch splitting utility statement
                if (batches.Length > i + 1 &&
                    batches[i + 1].StartsWith(BatchTerminator, StringComparison.OrdinalIgnoreCase) && 
                    ! batches[i + 1].EqualsIgnoreCase(BatchTerminator))
                {
                    repeatCount = int.Parse(Regex.Match(batches[i + 1], @"([0-9]+)").Value, CultureInfo.InvariantCulture);
                }

                for (int j = 0; j < repeatCount; ++j)
                    Statement(batches[i], suppressTransaction);
            }       
        }

    }
}
