// Copyright (c) 2008, 2021, Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of XG hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// XG.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of XG Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using XuguClient;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Migrations.Utilities;
using System.Linq;
using System.Text;
using Xugu.Data.EntityFramework.Utilities;

namespace Xugu.Data.EntityFramework
{

    /// <summary>
    /// Class used to customized code generation
    /// to avoid SYSDBA. prefix added on table names.
    /// </summary>
    public class XGMigrationCodeGenerator : CSharpMigrationCodeGenerator
    {
        private IEnumerable<KeyValuePair<CreateTableOperation, AddForeignKeyOperation>> _foreignKeys;
        private IEnumerable<KeyValuePair<CreateTableOperation, CreateIndexOperation>> _tableIndexes;

        private string TrimSchemaPrefix(string table)
        {
            if (table.StartsWith("SYSDBA."))
                return table.Replace("SYSDBA.", "");

            return table;
        }

        private string PrepareSql(string sql, bool removeNonXGChars)
        {
            var sqlResult = sql;
            if (removeNonXGChars)
            {
                sqlResult = sql.Replace("[", "").Replace("]", "").Replace("@", "");
            }
            sqlResult = sqlResult.Replace("SYSDBA.", "");
            return sqlResult;
        }

        private IEnumerable<MigrationOperation> ReorderOperations(IEnumerable<MigrationOperation> operations)
        {
            if (operations.Where(operation => operation.GetType() == typeof(AddPrimaryKeyOperation)).Count() > 0 &&
                operations.Where(operation => operation.GetType() == typeof(DropPrimaryKeyOperation)).Count() > 0)
            {
                List<MigrationOperation> reorderedOpes = new List<MigrationOperation>();
                reorderedOpes.AddRange(operations.Where(operation => operation.GetType() == typeof(AlterColumnOperation)));
                reorderedOpes.AddRange(operations.Where(operation => operation.GetType() == typeof(DropPrimaryKeyOperation)));
                reorderedOpes.AddRange(operations.Where(operation => operation.GetType() != typeof(DropPrimaryKeyOperation) && operation.GetType() != typeof(AlterColumnOperation)));
                return reorderedOpes;
            }
            return operations;
        }

        public override ScaffoldedMigration Generate(string migrationId, IEnumerable<MigrationOperation> operations, string sourceModel, string targetModel, string @namespace, string className)
        {
            _foreignKeys = (from tbl in operations.OfType<CreateTableOperation>()
                            from fk in operations.OfType<AddForeignKeyOperation>()
                            where tbl.Name.Equals(fk.DependentTable, StringComparison.InvariantCultureIgnoreCase)
                            select new KeyValuePair<CreateTableOperation, AddForeignKeyOperation>(tbl, fk)).ToList();

            _tableIndexes = (from tbl in operations.OfType<CreateTableOperation>()
                             from idx in operations.OfType<CreateIndexOperation>()
                             where tbl.Name.Equals(idx.Table, StringComparison.InvariantCultureIgnoreCase)
                             select new KeyValuePair<CreateTableOperation, CreateIndexOperation>(tbl, idx)).ToList();

            return base.Generate(migrationId, ReorderOperations(operations), sourceModel, targetModel, @namespace, className);
        }

        protected override void Generate(AddColumnOperation addColumnOperation, IndentedTextWriter writer)
        {
            var add = new AddColumnOperation(TrimSchemaPrefix(addColumnOperation.Table), addColumnOperation.Column);
            base.Generate(add, writer);
        }

        protected override void Generate(AddForeignKeyOperation addForeignKeyOperation, IndentedTextWriter writer)
        {
            addForeignKeyOperation.PrincipalTable = TrimSchemaPrefix(addForeignKeyOperation.PrincipalTable);
            addForeignKeyOperation.DependentTable = TrimSchemaPrefix(addForeignKeyOperation.DependentTable);
            addForeignKeyOperation.Name = PrepareSql(addForeignKeyOperation.Name, false);
            base.Generate(addForeignKeyOperation, writer);
        }

        protected override void GenerateInline(AddForeignKeyOperation addForeignKeyOperation, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.Write(".ForeignKey(\"" + TrimSchemaPrefix(addForeignKeyOperation.PrincipalTable) + "\", ");
            Generate(addForeignKeyOperation.DependentColumns, writer);
            writer.Write(addForeignKeyOperation.CascadeDelete ? ", cascadeDelete: true)" : ")");
        }

        protected override void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedTextWriter writer)
        {
            addPrimaryKeyOperation.Table = TrimSchemaPrefix(addPrimaryKeyOperation.Table);
            base.Generate(addPrimaryKeyOperation, writer);
        }

        protected override void Generate(AlterColumnOperation alterColumnOperation, IndentedTextWriter writer)
        {
            AlterColumnOperation alter = null;
            if (alterColumnOperation.Inverse != null)
                alter = new AlterColumnOperation(TrimSchemaPrefix(alterColumnOperation.Table), alterColumnOperation.Column, alterColumnOperation.IsDestructiveChange, (AlterColumnOperation)alterColumnOperation.Inverse);
            else
                alter = new AlterColumnOperation(TrimSchemaPrefix(alterColumnOperation.Table), alterColumnOperation.Column, alterColumnOperation.IsDestructiveChange);

            if (alter != null)
                base.Generate(alter, writer);
            else
                base.Generate(alterColumnOperation);
        }

        protected override void Generate(CreateIndexOperation createIndexOperation, IndentedTextWriter writer)
        {
            createIndexOperation.Table = TrimSchemaPrefix(createIndexOperation.Table);
            base.Generate(createIndexOperation, writer);
        }

        protected override void GenerateInline(CreateIndexOperation createIndexOperation, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.Write(".Index(");

            Generate(createIndexOperation.Columns, writer);

            writer.Write(createIndexOperation.IsUnique ? ", unique: true" : "");
            writer.Write(!createIndexOperation.HasDefaultName ? string.Format(", name: {0}", TrimSchemaPrefix(createIndexOperation.Name)) : "");

            writer.Write(")");
        }

        protected override void Generate(CreateTableOperation createTableOperation, IndentedTextWriter writer)
        {
            var create = new CreateTableOperation(TrimSchemaPrefix(createTableOperation.Name));

            foreach (var item in createTableOperation.Columns)
                create.Columns.Add(item);

            create.PrimaryKey = createTableOperation.PrimaryKey;

            base.Generate(create, writer);

            System.IO.StringWriter innerWriter = writer.InnerWriter as System.IO.StringWriter;
            if (innerWriter != null)
            {
                innerWriter.GetStringBuilder().Remove(innerWriter.ToString().LastIndexOf(";"), innerWriter.ToString().Length - innerWriter.ToString().LastIndexOf(";"));
                writer.Indent++;
                _foreignKeys.Where(tbl => tbl.Key == createTableOperation).ToList().ForEach(fk => GenerateInline(fk.Value, writer));
                _tableIndexes.Where(tbl => tbl.Key == createTableOperation).ToList().ForEach(idx => GenerateInline(idx.Value, writer));
                writer.WriteLine(";");
                writer.Indent--;
                writer.WriteLine();
            }
        }

        protected override void Generate(DropColumnOperation dropColumnOperation, IndentedTextWriter writer)
        {
            var drop = new DropColumnOperation(TrimSchemaPrefix(dropColumnOperation.Table), dropColumnOperation.Name);
            base.Generate(drop, writer);
        }

        protected override void Generate(DropForeignKeyOperation dropForeignKeyOperation, IndentedTextWriter writer)
        {
            dropForeignKeyOperation.PrincipalTable = TrimSchemaPrefix(dropForeignKeyOperation.PrincipalTable);
            dropForeignKeyOperation.DependentTable = TrimSchemaPrefix(dropForeignKeyOperation.DependentTable);
            dropForeignKeyOperation.Name = PrepareSql(dropForeignKeyOperation.Name, false);
            base.Generate(dropForeignKeyOperation, writer);
        }

        protected override void Generate(DropIndexOperation dropIndexOperation, IndentedTextWriter writer)
        {
            dropIndexOperation.Table = TrimSchemaPrefix(dropIndexOperation.Table);
            base.Generate(dropIndexOperation, writer);
        }

        protected override void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation, IndentedTextWriter writer)
        {
            dropPrimaryKeyOperation.Table = TrimSchemaPrefix(dropPrimaryKeyOperation.Table);
            base.Generate(dropPrimaryKeyOperation, writer);
        }

        protected override void Generate(DropTableOperation dropTableOperation, IndentedTextWriter writer)
        {
            var drop = new DropTableOperation(TrimSchemaPrefix(dropTableOperation.Name));
            base.Generate(drop, writer);
        }

        protected override void Generate(MoveTableOperation moveTableOperation, IndentedTextWriter writer)
        {
            var move = new MoveTableOperation(TrimSchemaPrefix(moveTableOperation.Name), moveTableOperation.NewSchema);
            base.Generate(move, writer);
        }

        protected override void Generate(RenameColumnOperation renameColumnOperation, IndentedTextWriter writer)
        {
            var rename = new RenameColumnOperation(TrimSchemaPrefix(renameColumnOperation.Table), renameColumnOperation.Name, renameColumnOperation.NewName);
            base.Generate(rename, writer);
        }

        protected override void Generate(RenameTableOperation renameTableOperation, IndentedTextWriter writer)
        {
            var rename = new RenameTableOperation(TrimSchemaPrefix(renameTableOperation.Name), renameTableOperation.NewName);
            base.Generate(rename, writer);
        }
    }


    /// <summary>
    /// Implementation of a XG's Sql generator for EF 4.3 data migrations.
    /// </summary>
    public class XGMigrationSqlGenerator : MigrationSqlGenerator
    {
        private List<MigrationStatement> _specialStmts = new List<MigrationStatement>();
        private DbProviderManifest _providerManifest;
        private Dictionary<string, OpDispatcher> _dispatcher = new Dictionary<string, OpDispatcher>();
        private List<string> _generatedTables { get; set; }
        private string _tableName { get; set; }
        private string _providerManifestToken;
        private List<string> autoIncrementCols { get; set; }
        private List<string> primaryKeyCols { get; set; }
        private IEnumerable<AddPrimaryKeyOperation> _pkOperations = new List<AddPrimaryKeyOperation>();

        delegate MigrationStatement OpDispatcher(MigrationOperation op);

        private HashSet<string> _generatedSchemas;


        public XGMigrationSqlGenerator()
        {

            _dispatcher.Add("AddColumnOperation", (OpDispatcher)((op) => { return Generate(op as AddColumnOperation); }));
            _dispatcher.Add("AddForeignKeyOperation", (OpDispatcher)((op) => { return Generate(op as AddForeignKeyOperation); }));
            _dispatcher.Add("AddPrimaryKeyOperation", (OpDispatcher)((op) => { return Generate(op as AddPrimaryKeyOperation); }));
            _dispatcher.Add("AlterColumnOperation", (OpDispatcher)((op) => { return Generate(op as AlterColumnOperation); }));
            _dispatcher.Add("CreateIndexOperation", (OpDispatcher)((op) => { return Generate(op as CreateIndexOperation); }));
            _dispatcher.Add("CreateTableOperation", (OpDispatcher)((op) => { return Generate(op as CreateTableOperation); }));
            _dispatcher.Add("DropColumnOperation", (OpDispatcher)((op) => { return Generate(op as DropColumnOperation); }));
            _dispatcher.Add("DropForeignKeyOperation", (OpDispatcher)((op) => { return Generate(op as DropForeignKeyOperation); }));
            _dispatcher.Add("DropIndexOperation", (OpDispatcher)((op) => { return Generate(op as DropIndexOperation); }));
            _dispatcher.Add("DropPrimaryKeyOperation", (OpDispatcher)((op) => { return Generate(op as DropPrimaryKeyOperation); }));
            _dispatcher.Add("DropTableOperation", (OpDispatcher)((op) => { return Generate(op as DropTableOperation); }));
            _dispatcher.Add("MoveTableOperation", (OpDispatcher)((op) => { return Generate(op as MoveTableOperation); }));
            _dispatcher.Add("RenameColumnOperation", (OpDispatcher)((op) => { return Generate(op as RenameColumnOperation); }));
            _dispatcher.Add("RenameTableOperation", (OpDispatcher)((op) => { return Generate(op as RenameTableOperation); }));
            _dispatcher.Add("SqlOperation", (OpDispatcher)((op) => { return Generate(op as SqlOperation); }));
            autoIncrementCols = new List<string>();
            primaryKeyCols = new List<string>();
            _dispatcher.Add("HistoryOperation", (OpDispatcher)((op) => { return Generate(op as HistoryOperation); }));
            _dispatcher.Add("CreateProcedureOperation", (OpDispatcher)((op) => { return Generate(op as CreateProcedureOperation); }));
            _dispatcher.Add("UpdateDatabaseOperation", (OpDispatcher)((op) => { return Generate(op as UpdateDatabaseOperation); }));
            _generatedSchemas = new HashSet<string>();
        }

        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            XGConnection con = new XGConnection();
            List<MigrationStatement> stmts = new List<MigrationStatement>();
            _providerManifestToken = providerManifestToken;
            _providerManifest = DbProviderServices.GetProviderServices(con).GetProviderManifest(providerManifestToken);

            //verify if there is one or more add/alter column operation, if there is then look for primary key operations. Alter in case that the user wants to change the current PK column
            if ((from cols in migrationOperations.OfType<AddColumnOperation>() select cols).Count() > 0 || (from cols in migrationOperations.OfType<AlterColumnOperation>() select cols).Count() > 0)
                _pkOperations = (from pks in migrationOperations.OfType<AddPrimaryKeyOperation>() select pks).ToList();

            foreach (MigrationOperation op in migrationOperations)
            {
                var ct = op as CreateTableOperation;
                if (ct != null)
                {
                    var databaseName = DatabaseName.Parse(ct.Name);

                    if (!string.IsNullOrWhiteSpace(databaseName.Schema))
                    {
                        if (!databaseName.Schema.EqualsIgnoreCase("SYSDBA")
                            && !_generatedSchemas.Contains(databaseName.Schema))
                        {
                            stmts.Add(new MigrationStatement() { Sql = $"BEGIN IF (SELECT COUNT(*) FROM ALL_SCHEMAS WHERE SCHEMA_NAME='{databaseName.Schema}')=0 THEN CREATE SCHEMA `{databaseName.Schema}`; END IF; END;" });

                            _generatedSchemas.Add(databaseName.Schema);
                        }
                    }
                }
                if (!_dispatcher.ContainsKey(op.GetType().Name))
                    throw new NotImplementedException(op.GetType().Name);
                OpDispatcher opdis = _dispatcher[op.GetType().Name];
                stmts.Add(opdis(op));
            }
            if (_specialStmts.Count > 0)
            {
                foreach (var item in _specialStmts)
                    stmts.Add(item);
            }
            return stmts;
        }

        private MigrationStatement Generate(UpdateDatabaseOperation updateDatabaseOperation)
        {
            if (updateDatabaseOperation == null)
                throw new ArgumentNullException("UpdateDatabaseOperation");

            MigrationStatement statement = new MigrationStatement();
            StringBuilder sql = new StringBuilder();
            const string idempotentScriptName = "_idempotent_script";
            SelectGenerator generator = new SelectGenerator();

            if (!updateDatabaseOperation.Migrations.Any())
                return statement;

            sql.AppendFormat("DROP PROCEDURE IF EXISTS `{0}`;", idempotentScriptName);
            sql.AppendLine();
            sql.AppendLine();
            sql.AppendLine();

            sql.AppendFormat("CREATE PROCEDURE `{0}`() AS", idempotentScriptName);
            sql.AppendLine("  CurrentMigration TEXT;");
            sql.AppendLine("BEGIN");
            sql.AppendLine();
            sql.AppendLine("  IF (SELECT COUNT(t.*) FROM ALL_TABLES t LEFT JOIN ALL_SCHEMAS s ON(t.SCHEMA_ID=s.SCHEMA_ID AND s.SCHEMA_NAME=CURRENT_SCHEMA) ");
            sql.AppendLine("  WHERE t.TABLE_NAME = '__MigrationHistory')>0 THEN ");

            foreach (var historyQueryTree in updateDatabaseOperation.HistoryQueryTrees)
            {
                string historyQuery = generator.GenerateSQL(historyQueryTree);
                ReplaceParemeters(ref historyQuery, generator.Parameters);
                sql.AppendLine(@"    CurrentMigration := (" + historyQuery + ");");
                sql.AppendLine("  END IF;");
                sql.AppendLine();
            }

            sql.AppendLine("  IF CurrentMigration IS NULL THEN");
            sql.AppendLine("    CurrentMigration := '0';");
            sql.AppendLine("  END IF;");
            sql.AppendLine();

            // Migrations
            foreach (var migration in updateDatabaseOperation.Migrations)
            {
                if (migration.Operations.Count == 0)
                    continue;

                sql.AppendLine("  IF CurrentMigration < '" + migration.MigrationId + "' THEN ");
                var statements = Generate(migration.Operations, _providerManifestToken);
                foreach (var migrationStatement in statements)
                {
                    string sqlStatement = migrationStatement.Sql;
                    if (!sqlStatement.EndsWith(";"))
                        sqlStatement += ";";
                    sql.AppendLine(sqlStatement);
                }
                sql.AppendLine("  END IF;");
                sql.AppendLine();
            }

            sql.AppendLine("END;");
            sql.AppendLine();
            sql.AppendFormat("CALL `{0}`();", idempotentScriptName);
            sql.AppendLine();
            sql.AppendLine();
            sql.AppendFormat("DROP PROCEDURE IF EXISTS `{0}`;", idempotentScriptName);
            sql.AppendLine();

            statement.Sql = sql.ToString();

            return statement;
        }

        protected virtual MigrationStatement Generate(HistoryOperation op)
        {
            if (op == null) return null;

            MigrationStatement stmt = new MigrationStatement();

            var cmdStr = "";
            SqlGenerator generator = new SelectGenerator();
            foreach (var commandTree in op.CommandTrees)
            {
                switch (commandTree.CommandTreeKind)
                {
                    case DbCommandTreeKind.Insert:
                        generator = new InsertGenerator();
                        break;
                    case DbCommandTreeKind.Delete:
                        generator = new DeleteGenerator();
                        break;
                    case DbCommandTreeKind.Update:
                        generator = new UpdateGenerator();
                        break;
                    case DbCommandTreeKind.Query:
                        generator = new SelectGenerator();
                        break;
                    case DbCommandTreeKind.Function:
                        generator = new FunctionGenerator();
                        break;
                    default:
                        throw new NotImplementedException(commandTree.CommandTreeKind.ToString());
                }
                cmdStr = generator.GenerateSQL(commandTree);

                ReplaceParemeters(ref cmdStr, generator.Parameters);
                stmt.Sql += cmdStr.Replace("SYSDBA.", "") + ";";
            }
            return stmt;
        }

        private void ReplaceParemeters(ref string sql, IList<XGParameters> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.DbType == System.Data.DbType.String)
                    sql = sql.Replace(parameter.ParameterName, "'" + parameter.Value.ToString() + "'");
                else if (parameter.DbType == System.Data.DbType.Binary)
                    sql = sql.Replace(parameter.ParameterName, "'" + BitConverter.ToString((byte[])parameter.Value).Replace("-", "") + "'");
                else
                    sql = sql.Replace(parameter.ParameterName, parameter.Value.ToString());
            }
        }

        private string ReconfigurationTableName(string name)
        {
            var list = name.Split('.');
            if (list.Length >= 2)
            {
                return /*(list[0] != "dbo" && list[0] != "SYSDBA") ? */string.Format("`{0}`.`{1}`", list[0], list[1]) /*: $"`{list[1]}`"*/;
            }
            return "`" + name + "`";
        }

        public override string GenerateProcedureBody(ICollection<DbModificationCommandTree> commandTrees, string rowsAffectedParameter, string providerManifestToken)
        {
            XGConnection con = new XGConnection();
            MigrationStatement stmt = new MigrationStatement();
            _providerManifest = DbProviderServices.GetProviderServices(con).GetProviderManifest(providerManifestToken);

            var cmdStr = "";
            SqlGenerator generator = new SelectGenerator();
            foreach (var commandTree in commandTrees)
            {
                switch (commandTree.CommandTreeKind)
                {
                    case DbCommandTreeKind.Insert:
                        generator = new InsertGenerator();
                        cmdStr = generator.GenerateSQL(commandTree);
                        break;
                    case DbCommandTreeKind.Delete:
                        generator = new DeleteGenerator();
                        cmdStr = generator.GenerateSQL(commandTree);
                        break;
                    case DbCommandTreeKind.Update:
                        generator = new UpdateGenerator();
                        cmdStr = generator.GenerateSQL(commandTree);
                        break;
                    case DbCommandTreeKind.Query:
                        generator = new SelectGenerator();
                        cmdStr = generator.GenerateSQL(commandTree);
                        break;
                    case DbCommandTreeKind.Function:
                        generator = new FunctionGenerator();
                        cmdStr = generator.GenerateSQL(commandTree);
                        break;
                }
                stmt.Sql += cmdStr.Replace("SYSDBA.", "") + ";";
            }
            return stmt.Sql;
        }

        protected virtual MigrationStatement Generate(CreateProcedureOperation op)
        {
            MigrationStatement stmt = new MigrationStatement();
            stmt.Sql = GenerateProcedureCmd(op);
            return stmt;
        }

        private string GenerateProcedureCmd(CreateProcedureOperation po)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(string.Format("CREATE PROCEDURE {0}({1}) AS", ReconfigurationTableName(po.Name) /*po.Name.Replace("SYSDBA.", "")*/, GenerateParamSentence(po.Parameters)));
            sql.AppendLine("BEGIN ");
            sql.AppendLine(po.BodySql);
            sql.AppendLine(" END");
            return sql.ToString().Replace("@", "");
        }

        private string GenerateParamSentence(IList<ParameterModel> Parameters)
        {
            StringBuilder sql = new StringBuilder();
            foreach (ParameterModel param in Parameters)
            {
                sql.AppendFormat("{0} {1} {2},",
                                 (param.IsOutParameter ? "OUT" : ""),
                                 param.Name,
                                 BuildParamType(param));
            }

            return sql.ToString().Substring(0, sql.ToString().LastIndexOf(","));
        }

        private string BuildParamType(ParameterModel param)
        {
            string type = XGProviderServices.Instance.GetColumnType(_providerManifest.GetStoreType(param.TypeUsage));
            StringBuilder sb = new StringBuilder();
            sb.Append(type);

            if (new string[] { "char", "varchar" }.Contains(type.ToLower()))
            {
                if (param.MaxLength.HasValue)
                {
                    sb.AppendFormat("({0}) ", param.MaxLength.Value);
                }
            }

            if (param.Precision.HasValue && param.Scale.HasValue)
            {
                sb.AppendFormat("( {0}, {1} ) ", param.Precision.Value, param.Scale.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a migration operation to add a column.
        /// </summary>
        /// <param name="op">The operation that represents a column being added to a table.</param>
        /// <returns>A migration operation to add a column.</returns>
        protected virtual MigrationStatement Generate(AddColumnOperation op)
        {
            if (op == null) return null;

            _tableName = op.Table;

            MigrationStatement stmt = new MigrationStatement();
            //verify if there is any "AddPrimaryKeyOperation" related with the column that will be added and if it is defined as identity (auto_increment)
            bool uniqueAttr = (from pkOpe in _pkOperations
                               where (from col in pkOpe.Columns
                                      where col == op.Column.Name
                                      select col).Count() > 0
                               select pkOpe).Count() > 0 & op.Column.IsIdentity;

            // if the column to be added is PK as well as identity we need to specify the column as unique to avoid the error: 
            // "Incorrect table definition there can be only one auto column and it must be defined as a key", 
            // since unique and PK are almost equivalent we'll be able to add the new column and later add the PK related to it, 
            // this because the "AddPrimaryKeyOperation" is executed after the column is added
            stmt.Sql = EndsWithSemicolon(string.Format("alter table {0} add column `{1}` {2} {3}", ReconfigurationTableName(TrimSchemaPrefix(op.Table)),
              op.Column.Name, Generate(op.Column), (uniqueAttr ? " unique " : "")));

            return stmt;
        }

        /// <summary>
        /// Generates a migration operation to drop a column.
        /// </summary>
        /// <param name="op">The operation that represents a column being dropped from a table.</param>
        /// <returns>The migration operation to drop a column.</returns>
        protected virtual MigrationStatement Generate(DropColumnOperation op)
        {
            if (op == null) return null;

            MigrationStatement stmt = new MigrationStatement();
            stmt.Sql = EndsWithSemicolon(string.Format("alter table {0} drop column `{1}` cascade",
              ReconfigurationTableName(TrimSchemaPrefix(op.Table)), op.Name));
            return stmt;
        }

        /// <summary>
        /// Generates a migration operation to alter a column.
        /// </summary>
        /// <param name="op">The operation that represents altering an existing column.</param>
        /// <returns>A migration operation to alter a column.</returns>
        protected virtual MigrationStatement Generate(AlterColumnOperation op)
        {
            if (op == null) return null;

            ColumnModel column = op.Column;
            StringBuilder sb = new StringBuilder();
            _tableName = op.Table;

            //verify if there is any "AddPrimaryKeyOperation" related with the column that will be added and if it is defined as identity (auto_increment)
            bool uniqueAttr = (from pkOpe in _pkOperations
                               where (from col in pkOpe.Columns
                                      where col == op.Column.Name
                                      select col).Count() > 0
                               select pkOpe).Count() > 0 & op.Column.IsIdentity;

            // for existing columns
            sb.Append("alter table " + ReconfigurationTableName(TrimSchemaPrefix(op.Table)) + " modify `" + column.Name + "` ");

            // add definition
            sb.Append(Generate(column) + (uniqueAttr ? " unique " : ""));
            sb.Append(" cascade");

            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to rename a column.
        /// </summary>
        /// <param name="op">The operation that represents a column being renamed.</param>
        /// <returns>A migration operation to rename a column.</returns>
        protected virtual MigrationStatement Generate(RenameColumnOperation op)
        {
            if (op == null) return null;

            StringBuilder sb = new StringBuilder();


            sb.AppendFormat("ALTER TABLE `{0}` RENAME `{1}` TO `{2}` CASCADE", TrimSchemaPrefix(op.Table),op.Name, op.NewName);
            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to add a foreign key.
        /// </summary>
        /// <param name="op">the operation that represents a foreing key constraint being added to a table.</param>
        /// <returns>A migration operation to add a foreign key constraint.</returns>
        protected virtual MigrationStatement Generate(AddForeignKeyOperation op)
        {

            StringBuilder sb = new StringBuilder();
            string fkName = op.Name;
            if (fkName.Length > 64)
            {
                fkName = "FK_" + Guid.NewGuid().ToString().Replace("-", "");
            }
            sb.Append("alter table " + ReconfigurationTableName(TrimSchemaPrefix(op.DependentTable)) + " add constraint `" + fkName + "` " +
                       " foreign key ");

            sb.Append("(" + string.Join(",", op.DependentColumns.Select(c => "`" + c + "`")) + ") ");
            sb.Append("references " + ReconfigurationTableName(op.PrincipalTable) + " ( " + string.Join(",", op.PrincipalColumns.Select(c => "`" + c + "`")) + ") ");

            if (op.CascadeDelete)
            {
                sb.Append(" on update cascade on delete cascade ");
            }

            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates an SQL statement of a column model.
        /// </summary>
        /// <param name="op">The model that represents a column.</param>
        /// <returns>A string containing an SQL statement of a column model.</returns>
        protected virtual string Generate(ColumnModel op)
        {
            TypeUsage typeUsage = _providerManifest.GetStoreType(op.TypeUsage);
            StringBuilder sb = new StringBuilder();
            string type = op.StoreType;
            if (type == null)
            {
                type = XGProviderServices.Instance.GetColumnType(typeUsage);
            }

            sb.Append(type);

            if (!type.EndsWith(")", StringComparison.InvariantCulture))
            {
                if ((op.ClrType == typeof(string)) ||
                   ((op.ClrType == typeof(byte) || op.ClrType == typeof(byte[])) && op.ClrType.IsArray))
                {
                    if (op.MaxLength.HasValue)
                    {
                        sb.AppendFormat("({0}) ", op.MaxLength.Value);
                    }
                }
                if (op.Precision.HasValue && op.Scale.HasValue)
                {
                    sb.AppendFormat("( {0}, {1} ) ", op.Precision.Value, op.Scale.Value);
                }
                else
                {
                    if (type == "datetime" || type == "timestamp" || type == "time")
                    {
                        if (op.Precision.HasValue && op.Precision.Value >= 1)
                        {
                            sb.AppendFormat("({0}) ", op.Precision.Value <= 6 ? op.Precision.Value : 6);
                        }
                        if (op.IsIdentity && (String.Compare(type, "datetime", true) == 0 || String.Compare(type, "timestamp", true) == 0))
                        {
                            sb.AppendFormat(" DEFAULT CURRENT_TIMESTAMP{0}", op.Precision.HasValue && op.Precision.Value >= 1 ? "( " + op.Precision.Value.ToString() + " )" : "");
                        }
                    }
                }
            }

            op.StoreType = type;

            if (op.IsIdentity && (new string[] { "tinyint", "smallint", "int", "int", "bigint" }).Contains(type.ToLower()))
            {
                sb.Append(" IDENTITY ");
                autoIncrementCols.Add(op.Name);
            }
            else
            {
                // nothing
            }
            if (!(op.IsNullable ?? true))
            {
                sb.Append(string.Format("{0} not null ",
                  ((!primaryKeyCols.Contains(op.Name) && op.IsIdentity && op.Type != PrimitiveTypeKind.Guid) ? "" :
                  ((op.Type == PrimitiveTypeKind.Guid) ? " default null " : ""))));
            }

            if (!string.IsNullOrEmpty(op.DefaultValueSql))
            {
                sb.Append(string.Format(" default {0}", op.DefaultValueSql));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a migration operation to drop a foreign key constraint from a table.
        /// </summary>
        /// <param name="op">The operation that represents a foreign key being added from a table.</param>
        /// <returns>A migration operation to drop a foreign key.</returns>
        protected virtual MigrationStatement Generate(DropForeignKeyOperation op)
        {
            StringBuilder sb = new StringBuilder();
            sb = sb.AppendFormat("alter table {0} drop constraint `{1}` cascade", ReconfigurationTableName(TrimSchemaPrefix(op.DependentTable)), op.Name);
            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to create a database index.
        /// </summary>
        /// <param name="op">The operation that represents creating a database index.</param>
        /// <returns>A migration operation to create a database index.</returns>
        protected virtual MigrationStatement Generate(CreateIndexOperation op)
        {
            StringBuilder sb = new StringBuilder();

            sb = sb.Append("CREATE ");

            if (op.IsUnique)
            {
                sb.Append("UNIQUE ");
            }

            //index_col_name specification can end with ASC or DESC.
            // sort order are permitted for future extensions for specifying ascending or descending index value storage
            //Currently, they are parsed but ignored; index values are always stored in ascending order.

            object sort;
            op.AnonymousArguments.TryGetValue("Sort", out sort);
            var sortOrder = sort != null && sort.ToString() == "Ascending" ?
                            "ASC" : "DESC";

            sb.AppendFormat("index  `{0}` on {1} (", op.Name, ReconfigurationTableName(TrimSchemaPrefix(op.Table)));
            sb.Append(string.Join(",", op.Columns.Select(c => "`" + c + "` " + sortOrder)) + ") ");

            return new MigrationStatement() { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to drop an existing database index.
        /// </summary>
        /// <param name="op">The operation that represents dropping am existing database index.</param>
        /// <returns>A migration operation to drop an existing database index.</returns>
        protected virtual MigrationStatement Generate(DropIndexOperation op)
        {
            return new MigrationStatement()
            {
                Sql = EndsWithSemicolon(string.Format("drop index {0}.`{1}`",
                ReconfigurationTableName(TrimSchemaPrefix(op.Table)), op.Name))
            };
        }

        /// <summary>
        /// Generates a migration operation to create a table.
        /// </summary>
        /// <param name="op">The operation that represents creating a table.</param>
        /// <returns>A migration operation to create a table.</returns>
        protected virtual MigrationStatement Generate(CreateTableOperation op)
        {
            StringBuilder sb = new StringBuilder();
            string tableName = ReconfigurationTableName(TrimSchemaPrefix(op.Name));
            primaryKeyCols.Clear();
            autoIncrementCols.Clear();
            if (_generatedTables == null)
                _generatedTables = new List<string>();

            if (!_generatedTables.Contains(tableName))
            {
                _generatedTables.Add(tableName);
            }
            sb.Append("create table " + tableName + " (");

            _tableName = op.Name;

            if (op.PrimaryKey != null)
            {
                op.PrimaryKey.Columns.ToList().ForEach(col => primaryKeyCols.Add(col));
            }

            //columns
            sb.Append(string.Join(",", op.Columns.Select(c => "`" + c.Name + "` " + Generate(c))));

            // Determine columns that are GUID & identity
            List<ColumnModel> guidCols = new List<ColumnModel>();
            ColumnModel guidPK = null;
            foreach (ColumnModel opCol in op.Columns)
            {
                if (opCol.Type == PrimitiveTypeKind.Guid && opCol.IsIdentity/* && String.Compare(opCol.StoreType, "CHAR(36) BINARY", true) == 0*/)
                {
                    if (primaryKeyCols.Contains(opCol.Name))
                        guidPK = opCol;
                    guidCols.Add(opCol);
                }
            }

            if (guidCols.Count != 0)
            {
                var names = _tableName.Split('.');
                string typeName = names.Length > 1 ? names[1] : _tableName;
                string schemaName = names.Length > 1 ? names[0] : "SYSDBA";
                var createTrigger = new StringBuilder();
                createTrigger.AppendLine(string.Format("DROP TABLE IF EXISTS `{0}`.`tmpIdentity_{1}`;", schemaName, typeName));
                createTrigger.AppendLine(string.Format("CREATE TABLE `{0}`.`tmpIdentity_{1}` (`guid` guid);", schemaName, typeName));
                createTrigger.AppendLine(string.Format("DROP TRIGGER IF EXISTS `{0}`.`{1}_IdentityTgr`;", schemaName, typeName));
                createTrigger.AppendLine(string.Format("CREATE TRIGGER `{0}`.`{1}_IdentityTgr` BEFORE INSERT ON `{0}`.`{1}`", schemaName, typeName));
                createTrigger.AppendLine("FOR EACH ROW BEGIN");
                for (int i = 0; i < guidCols.Count; i++)
                {
                    ColumnModel opCol = guidCols[i];
                    createTrigger.AppendLine(string.Format("NEW.{0} := sys_guid();", opCol.Name));
                }
                if(guidPK!=null)createTrigger.AppendLine(string.Format("INSERT INTO `{0}`.`tmpIdentity_{1}` VALUES(New.{2});", schemaName, typeName, guidPK.Name));
                createTrigger.AppendLine("END;");
                var sqlOp = new SqlOperation(createTrigger.ToString());
                _specialStmts.Add(Generate(sqlOp));
            }

            if (op.PrimaryKey != null)// && !sb.ToString().Contains("primary key"))
            {
                sb.Append(",");
                sb.Append("primary key ( " + string.Join(",", op.PrimaryKey.Columns.Select(c => "`" + c + "`")) + ") ");
            }

            //string keyFields = ",";
            //autoIncrementCols.ForEach(col => keyFields += (!primaryKeyCols.Contains(col) ? string.Format(" KEY (`{0}`),", col) : ""));
            //sb.Append(keyFields.Substring(0, keyFields.LastIndexOf(",")));
            sb.Append(");");

            return new MigrationStatement() { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to drop an existing table.
        /// </summary>
        /// <param name="op">The operation that represents dropping an existing table.</param>
        /// <returns>A migration operation to drop an existing table.</returns>
        protected virtual MigrationStatement Generate(DropTableOperation op)
        {
            return new MigrationStatement() { Sql = EndsWithSemicolon("drop table " + ReconfigurationTableName(TrimSchemaPrefix(op.Name))) };
        }

        /// <summary>
        /// Generates a migration operation to add a primary key to a table.
        /// </summary>
        /// <param name="op">The operation that represents adding a primary key to a table.</param>
        /// <returns>A migration operation to add a primary key to a table.</returns>
        protected virtual MigrationStatement Generate(AddPrimaryKeyOperation op)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("alter table " + ReconfigurationTableName(TrimSchemaPrefix(op.Table)) + " add primary key ");
            sb.Append(" `" + op.Name + "` ");

            if (op.Columns.Count > 0)
                sb.Append("( " + string.Join(",", op.Columns.Select(c => "`" + c + "`")) + ") ");

            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Generates a migration operation to drpo an existing primary key.
        /// </summary>
        /// <param name="op">The operation that represents dropping an existing primary key.</param>
        /// <returns>A migration operation to drop an existing primary key.</returns>
        protected virtual MigrationStatement Generate(DropPrimaryKeyOperation op)
        {
            object obj2;
            bool deleteAutoIncrement = false;
            StringBuilder sb = new StringBuilder();


            op.AnonymousArguments.TryGetValue("DeleteAutoIncrement", out obj2);
            if (obj2 != null)
                bool.TryParse(obj2.ToString(), out deleteAutoIncrement);

            if (deleteAutoIncrement && op.Columns.Count == 1)
            {
                var newColumn = new ColumnModel(PrimitiveTypeKind.Int32, null);
                newColumn.Name = op.Columns[0];
                var alterColumn = new AlterColumnOperation(op.Table, newColumn, false);
                var ms = Generate(alterColumn);
                sb.Append(ms.Sql);
            }

            //return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString() + " EXECUTE IMMEDIATE 'ALTER TABLE `"+op.Table+"` DROP CONSTRAINT '||(SELECT CASE WHEN EXISTS(SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE CONS_TYPE = 'P' AND TABLE_ID = (SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME = '"+op.Table+"' LIMIT 1) LIMIT 1) THEN (SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE CONS_TYPE = 'P' AND TABLE_ID = (SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME = '"+op.Table+"' LIMIT 1) LIMIT 1) ELSE CONCAT('Random_', FLOOR(RAND() * 10000)) END AS Result FROM DUAL); ") };
            return new MigrationStatement { Sql = sb.ToString() };
        }

        /// <summary>
        /// Generates a migration operation to rename an existing table.
        /// </summary>
        /// <param name="op">The operation that represents renaming an existing table.</param>
        /// <returns>A migration operation to rename an existing table.</returns>
        protected virtual MigrationStatement Generate(RenameTableOperation op)
        {
            if (op == null) return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("rename table `{0}` to `{1}`", op.Name, op.NewName);
            return new MigrationStatement { Sql = EndsWithSemicolon(sb.ToString()) };
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="op">NA</param>
        /// <returns>NA</returns>
        protected virtual MigrationStatement Generate(MoveTableOperation op)
        {
            return new MigrationStatement { Sql = "" }; // TODO :check if we'll suppport this operation
        }

        private string EndsWithSemicolon(string str)
        {
            //remove last linefeed or whitespace end of string 
            string strSemiColon = str.TrimEnd(new char[] { ' ', '\r', '\n', ';' });
            strSemiColon += ";";
            return strSemiColon;
        }

        /// <summary>
        /// Generates a migration operation with a XG statement to be executed.
        /// </summary>
        /// <param name="op">The operation representing a XG statement to be executed directly against the database.</param>
        /// <returns>A migration operation with a XG statement to be executed.</returns>
        protected virtual MigrationStatement Generate(SqlOperation op)
        {
            return new MigrationStatement { Sql = op.Sql, SuppressTransaction = op.SuppressTransaction };
        }

        private string TrimSchemaPrefix(string table)
        {
            /*if (table.StartsWith("dbo.") || table.Contains("dbo."))
            {
                return table.Replace("dbo.", "");
            }
            else */if (table.StartsWith("SYSDBA.") || table.Contains("SYSDBA."))
            {
                return table.Replace("SYSDBA.", "");
            }
            return table;
        }
    }
}


