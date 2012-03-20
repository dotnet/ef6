namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Base class for code-based migrations.
    /// </summary>
    public abstract class DbMigration
    {
        private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

        /// <summary>
        ///     Operations to be performed during the upgrade process.
        /// </summary>
        public abstract void Up();

        /// <summary>
        ///     Operations to be performed during the downgrade process.
        /// </summary>
        public virtual void Down()
        {
        }

        /// <summary>
        ///     Adds an operation to create a new table.
        /// </summary>
        /// <typeparam name = "TColumns">
        ///     The columns in this create table operation. 
        ///     You do not need to specify this type, it will be inferred from the columnsAction parameter you supply.
        /// </typeparam>
        /// <param name = "name">The name of the table. Schema name is optional, if no schema is specified then dbo is assumed.</param>
        /// <param name = "columnsAction">
        ///     An action that specifies the columns to be included in the table.
        ///     i.e. t => new { Id = t.Int(identity: true), Name = t.String() }</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns>An object that allows further configuration of the table creation operation.</returns>
        protected internal TableBuilder<TColumns> CreateTable<TColumns>(
            string name, Func<ColumnBuilder, TColumns> columnsAction, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(columnsAction != null);

            var createTableOperation = new CreateTableOperation(name, anonymousArguments);

            AddOperation(createTableOperation);

            var columns = columnsAction(new ColumnBuilder());

            columns.GetType().GetProperties()
                .Each(
                    (p, i) =>
                        {
                            var columnModel = p.GetValue(columns, null) as ColumnModel;

                            if (columnModel != null)
                            {
                                if (string.IsNullOrWhiteSpace(columnModel.Name))
                                {
                                    columnModel.Name = p.Name;
                                }

                                createTableOperation.Columns.Add(columnModel);
                            }
                        });

            return new TableBuilder<TColumns>(createTableOperation, this);
        }

        /// <summary>
        ///     Adds an operation to create a new foreign key constraint.
        /// </summary>
        /// <param name = "dependentTable">
        ///     The table that contains the foreign key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "dependentColumn">The foreign key column.</param>
        /// <param name = "principalTable">
        ///     The table that contains the column this foreign key references.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "principalColumn">
        ///     The column this foreign key references. 
        ///     If no value is supplied the primary key of the principal table will be referenced.
        /// </param>
        /// <param name = "cascadeDelete">
        ///     A value indicating if cascade delete should be configured for the foreign key relationship.
        ///     If no value is supplied, cascade delete will be off.
        /// </param>
        /// <param name = "name">
        ///     The name of the foreign key constraint in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AddForeignKey(
            string dependentTable,
            string dependentColumn,
            string principalTable,
            string principalColumn = null,
            bool cascadeDelete = false,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentTable));
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentColumn));
            Contract.Requires(!string.IsNullOrWhiteSpace(principalTable));

            AddForeignKey(
                dependentTable,
                new[] { dependentColumn },
                principalTable,
                principalColumn != null ? new[] { principalColumn } : null,
                cascadeDelete,
                name,
                anonymousArguments);
        }

        /// <summary>
        ///     Adds an operation to create a new foreign key constraint.
        /// </summary>
        /// <param name = "dependentTable">
        ///     The table that contains the foreign key columns.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "dependentColumns">The foreign key columns.</param>
        /// <param name = "principalTable">
        ///     The table that contains the columns this foreign key references.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "principalColumns">
        ///     The columns this foreign key references. 
        ///     If no value is supplied the primary key of the principal table will be referenced.
        /// </param>
        /// <param name = "cascadeDelete">
        ///     A value indicating if cascade delete should be configured for the foreign key relationship.
        ///     If no value is supplied, cascade delete will be off.
        /// </param>
        /// <param name = "name">
        ///     The name of the foreign key constraint in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AddForeignKey(
            string dependentTable,
            string[] dependentColumns,
            string principalTable,
            string[] principalColumns = null,
            bool cascadeDelete = false,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentTable));
            Contract.Requires(dependentColumns != null);
            Contract.Requires(dependentColumns.Any());
            Contract.Requires(!string.IsNullOrWhiteSpace(principalTable));

            var addForeignKeyOperation
                = new AddForeignKeyOperation(anonymousArguments)
                    {
                        DependentTable = dependentTable,
                        PrincipalTable = principalTable,
                        CascadeDelete = cascadeDelete,
                        Name = name
                    };

            dependentColumns.Each(c => addForeignKeyOperation.DependentColumns.Add(c));

            if (principalColumns != null)
            {
                principalColumns.Each(c => addForeignKeyOperation.PrincipalColumns.Add(c));
            }

            AddOperation(addForeignKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to drop a foreign key constraint based on its name.
        /// </summary>
        /// <param name = "dependentTable">
        ///     The table that contains the foreign key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the foreign key constraint in the database.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropForeignKey(string dependentTable, string name, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentTable));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(anonymousArguments)
                    {
                        DependentTable = dependentTable,
                        Name = name
                    };

            AddOperation(dropForeignKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to drop a foreign key constraint based on the column it targets.
        /// </summary>
        /// <param name = "dependentTable">
        ///     The table that contains the foreign key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "dependentColumn">The foreign key column.</param>
        /// <param name = "principalTable">
        ///     The table that contains the column this foreign key references.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "principalColumn">The columns this foreign key references.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropForeignKey(
            string dependentTable,
            string dependentColumn,
            string principalTable,
            string principalColumn = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentTable));
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentColumn));
            Contract.Requires(!string.IsNullOrWhiteSpace(principalTable));

            DropForeignKey(
                dependentTable,
                new[] { dependentColumn },
                principalTable,
                anonymousArguments);
        }

        /// <summary>
        ///     Adds an operation to drop a foreign key constraint based on the columns it targets.
        /// </summary>
        /// <param name = "dependentTable">
        ///     The table that contains the foreign key columns.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "dependentColumns">The foreign key columns.</param>
        /// <param name = "principalTable">
        ///     The table that contains the columns this foreign key references.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "principalColumns">The columns this foreign key references.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropForeignKey(
            string dependentTable,
            string[] dependentColumns,
            string principalTable,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dependentTable));
            Contract.Requires(dependentColumns != null);
            Contract.Requires(dependentColumns.Any());
            Contract.Requires(!string.IsNullOrWhiteSpace(principalTable));

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(anonymousArguments)
                    {
                        DependentTable = dependentTable,
                        PrincipalTable = principalTable
                    };

            dependentColumns.Each(c => dropForeignKeyOperation.DependentColumns.Add(c));

            AddOperation(dropForeignKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to drop a table.
        /// </summary>
        /// <param name = "name">
        ///     The name of the table to be dropped.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropTable(string name, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            AddOperation(new DropTableOperation(name, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to move a table to a new schema.
        /// </summary>
        /// <param name = "name">
        ///     The name of the table to be moved.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "newSchema">The schema the table is to be moved to.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void MoveTable(string name, string newSchema, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            AddOperation(new MoveTableOperation(name, newSchema, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to rename a table. To change the schema of a table use MoveTable
        /// </summary>
        /// <param name = "name">
        ///     The name of the table to be renamed.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "newName">
        ///     The new name for the table.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void RenameTable(string name, string newName, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(newName));

            AddOperation(new RenameTableOperation(name, newName, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to rename a column.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table that contains the column to be renamed.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the column to be renamed.</param>
        /// <param name = "newName">The new name for the column.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void RenameColumn(string table, string name, string newName, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(newName));

            AddOperation(new RenameColumnOperation(table, name, newName, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to add a column to an existing table.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to add the column to.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">
        ///     The name of the column to be added.
        /// </param>
        /// <param name = "columnAction">
        ///     An action that specifies the column to be added.
        ///     i.e. c => c.Int(nullable: false, defaultValue: 3)
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AddColumn(
            string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(columnAction != null);

            var columnModel = columnAction(new ColumnBuilder());

            columnModel.Name = name;

            AddOperation(new AddColumnOperation(table, columnModel, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to drop an existing column.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to drop the column from.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the column to be dropped.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropColumn(
            string table, string name, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            AddOperation(new DropColumnOperation(table, name, anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to alter the definition of an existing column.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table the column exists in.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the column to be changed.</param>
        /// <param name = "columnAction">
        ///     An action that specifies the new definition for the column.
        ///     i.e. c => c.String(nullable: false, defaultValue: "none")
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AlterColumn(
            string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(columnAction != null);

            var columnModel = columnAction(new ColumnBuilder());

            columnModel.Name = name;

            AddOperation(new AlterColumnOperation(table, columnModel, isDestructiveChange: false, anonymousArguments: anonymousArguments));
        }

        /// <summary>
        ///     Adds an operation to create a new primary key.
        /// </summary>
        /// <param name = "table">
        ///     The table that contains the primary key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "column">The primary key column.</param>
        /// <param name = "name">
        ///     The name of the primary key in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AddPrimaryKey(
            string table,
            string column,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(column));

            AddPrimaryKey(table, new[] { column }, name, anonymousArguments);
        }

        /// <summary>
        ///     Adds an operation to create a new primary key based on multiple columns.
        /// </summary>
        /// <param name = "table">
        ///     The table that contains the primary key columns.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "columns">The primary key columns.</param>
        /// <param name = "name">
        ///     The name of the primary key in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void AddPrimaryKey(
            string table,
            string[] columns,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(columns != null);
            Contract.Requires(columns.Any());

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation(anonymousArguments)
                    {
                        Table = table,
                        Name = name
                    };

            columns.Each(c => addPrimaryKeyOperation.Columns.Add(c));

            AddOperation(addPrimaryKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to drop an existing primary key that does not have the default name.
        /// </summary>
        /// <param name = "table">
        ///     The table that contains the primary key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the primary key to be dropped.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropPrimaryKey(string table, string name, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation(anonymousArguments)
                    {
                        Table = table,
                        Name = name,
                    };

            AddOperation(dropPrimaryKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to drop an existing primary key that was created with the default name.
        /// </summary>
        /// <param name = "table">
        ///     The table that contains the primary key column.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropPrimaryKey(string table, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation(anonymousArguments)
                    {
                        Table = table,
                    };

            AddOperation(dropPrimaryKeyOperation);
        }

        /// <summary>
        ///     Adds an operation to create an index on a single column.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to create the index on.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "column">The name of the column to create the index on.</param>
        /// <param name = "unique">
        ///     A value indicating if this is a unique index.
        ///     If no value is supplied a non-unique index will be created.
        /// </param>
        /// <param name = "name">
        ///     The name to use for the index in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void CreateIndex(
            string table,
            string column,
            bool unique = false,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(column));

            CreateIndex(table, new[] { column }, unique, name, anonymousArguments);
        }

        /// <summary>
        ///     Adds an operation to create an index on multiple columns.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to create the index on.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "columns">The name of the columns to create the index on.</param>
        /// <param name = "unique">
        ///     A value indicating if this is a unique index.
        ///     If no value is supplied a non-unique index will be created.
        /// </param>
        /// <param name = "name">
        ///     The name to use for the index in the database.
        ///     If no value is supplied a unique name will be generated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void CreateIndex(
            string table,
            string[] columns,
            bool unique = false,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(columns != null);
            Contract.Requires(columns.Any());

            var createIndexOperation
                = new CreateIndexOperation(anonymousArguments)
                    {
                        Table = table,
                        IsUnique = unique,
                        Name = name
                    };

            columns.Each(c => createIndexOperation.Columns.Add(c));

            AddOperation(createIndexOperation);
        }

        /// <summary>
        ///     Adds an operation to drop an index based on its name.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to drop the index from.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "name">The name of the index to be dropped.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropIndex(
            string table,
            string name,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var dropIndexOperation
                = new DropIndexOperation(anonymousArguments)
                    {
                        Table = table,
                        Name = name,
                    };

            AddOperation(dropIndexOperation);
        }

        /// <summary>
        ///     Adds an operation to drop an index based on the columns it targets.
        /// </summary>
        /// <param name = "table">
        ///     The name of the table to drop the index from.
        ///     Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name = "columns">The name of the column(s) the index targets.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void DropIndex(
            string table,
            string[] columns,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(columns != null);
            Contract.Requires(columns.Any());

            var dropIndexOperation
                = new DropIndexOperation(anonymousArguments)
                    {
                        Table = table,
                    };

            columns.Each(c => dropIndexOperation.Columns.Add(c));

            AddOperation(dropIndexOperation);
        }

        /// <summary>
        ///     Adds an operation to execute a SQL command.
        /// </summary>
        /// <param name = "sql">The SQL to be executed.</param>
        /// <param name = "suppressTransaction">
        ///     A value indicating if the SQL should be executed outside of the 
        ///     transaction being used for the migration process.
        ///     If no value is supplied the SQL will be executed within the transaction.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected internal void Sql(string sql, bool suppressTransaction = false, object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));

            AddOperation(new SqlOperation(sql, anonymousArguments) { SuppressTransaction = suppressTransaction });
        }

        internal void AddOperation(MigrationOperation migrationOperation)
        {
            Contract.Requires(migrationOperation != null);

            _operations.Add(migrationOperation);
        }

        internal IEnumerable<MigrationOperation> Operations
        {
            get { return _operations; }
        }

        internal void Reset()
        {
            _operations.Clear();
        }

        #region Hide object members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected new object MemberwiseClone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}