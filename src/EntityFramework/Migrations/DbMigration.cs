// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for code-based migrations.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class DbMigration : IDbMigration
    {
        private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public abstract void Up();

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public virtual void Down()
        {
        }

        /// <summary>
        /// Adds an operation to create a new stored procedure.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the stored procedure. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="body">The body of the stored procedure.</param>
        /// <param name="anonymousArguments">
        /// The additional arguments that may be processed by providers. Use anonymous type syntax
        /// to specify arguments. For example, 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void CreateStoredProcedure(string name, string body, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            CreateStoredProcedure<object>(name, _ => new { }, body, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to create a new stored procedure.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the stored procedure. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="parametersAction">The action that specifies the parameters of the stored procedure.</param>
        /// <param name="body">The body of the stored procedure.</param>
        /// <param name="anonymousArguments">
        /// The additional arguments that may be processed by providers. Use anonymous type syntax
        /// to specify arguments. For example, 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <typeparam name="TParameters">
        /// The parameters in this create stored procedure operation. You do not need to specify this
        /// type, it will be inferred from the <paramref name="parametersAction" /> parameter you supply.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void CreateStoredProcedure<TParameters>(
            string name,
            Func<ParameterBuilder, TParameters> parametersAction,
            string body,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(parametersAction, "parametersAction");

            var createProcedureOperation = new CreateProcedureOperation(name, body, anonymousArguments);

            AddOperation(createProcedureOperation);

            var parameters = parametersAction(new ParameterBuilder());

            parameters.GetType().GetNonIndexerProperties()
                .Each(
                    (p, i) =>
                    {
                        var parameterModel = p.GetValue(parameters, null) as ParameterModel;

                        if (parameterModel != null)
                        {
                            if (string.IsNullOrWhiteSpace(parameterModel.Name))
                            {
                                parameterModel.Name = p.Name;
                            }

                            createProcedureOperation.Parameters.Add(parameterModel);
                        }
                    });
        }

        /// <summary>
        /// Adds an operation to alter a stored procedure.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the stored procedure. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="body">The body of the stored procedure.</param>
        /// <param name="anonymousArguments">
        /// The additional arguments that may be processed by providers. Use anonymous type syntax
        /// to specify arguments. For example, 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void AlterStoredProcedure(string name, string body, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            AlterStoredProcedure<object>(name, _ => new { }, body, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to alter a stored procedure.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <typeparam name="TParameters">
        /// The parameters in this alter stored procedure operation. You do not need to specify this
        /// type, it will be inferred from the <paramref name="parametersAction" /> parameter you supply.
        /// </typeparam>
        /// <param name="name">
        /// The name of the stored procedure. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="parametersAction">The action that specifies the parameters of the stored procedure.</param>
        /// <param name="body">The body of the stored procedure.</param>
        /// <param name="anonymousArguments">
        /// The additional arguments that may be processed by providers. Use anonymous type syntax
        /// to specify arguments. For example, 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void AlterStoredProcedure<TParameters>(
            string name,
            Func<ParameterBuilder, TParameters> parametersAction,
            string body,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(parametersAction, "parametersAction");

            var alterProcedureOperation = new AlterProcedureOperation(name, body, anonymousArguments);

            AddOperation(alterProcedureOperation);

            var parameters = parametersAction(new ParameterBuilder());

            parameters.GetType().GetNonIndexerProperties()
                .Each(
                    (p, i) =>
                    {
                        var parameterModel = p.GetValue(parameters, null) as ParameterModel;

                        if (parameterModel != null)
                        {
                            if (string.IsNullOrWhiteSpace(parameterModel.Name))
                            {
                                parameterModel.Name = p.Name;
                            }

                            alterProcedureOperation.Parameters.Add(parameterModel);
                        }
                    });
        }

        /// <summary>
        /// Adds an operation to drop an existing stored procedure with the specified name.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the procedure to drop. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// The additional arguments that may be processed by providers. Use anonymous type syntax
        /// to specify arguments. For example, 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void DropStoredProcedure(
            string name,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            AddOperation(new DropProcedureOperation(name, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to create a new table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <typeparam name="TColumns">
        /// The columns in this create table operation. You do not need to specify this type, it will
        /// be inferred from the columnsAction parameter you supply.
        /// </typeparam>
        /// <param name="name"> The name of the table. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="columnsAction">
        /// An action that specifies the columns to be included in the table. i.e. t => new { Id =
        /// t.Int(identity: true), Name = t.String() }
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns> An object that allows further configuration of the table creation operation. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal TableBuilder<TColumns> CreateTable<TColumns>(
            string name, Func<ColumnBuilder, TColumns> columnsAction, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columnsAction, "columnsAction");

            return CreateTable(name, columnsAction, null, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to create a new table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <typeparam name="TColumns">
        /// The columns in this create table operation. You do not need to specify this type, it will
        /// be inferred from the columnsAction parameter you supply.
        /// </typeparam>
        /// <param name="name"> The name of the table. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="columnsAction">
        /// An action that specifies the columns to be included in the table. i.e. t => new { Id =
        /// t.Int(identity: true), Name = t.String() }
        /// </param>
        /// <param name="annotations">Custom annotations that exist on the table to be created. May be null or empty.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns> An object that allows further configuration of the table creation operation. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal TableBuilder<TColumns> CreateTable<TColumns>(
            string name, 
            Func<ColumnBuilder, TColumns> columnsAction, 
            IDictionary<string, object> annotations, 
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columnsAction, "columnsAction");

            var createTableOperation = new CreateTableOperation(name, annotations, anonymousArguments);

            AddOperation(createTableOperation);

            AddColumns(columnsAction(new ColumnBuilder()), createTableOperation.Columns);

            return new TableBuilder<TColumns>(createTableOperation, this);
        }

        /// <summary>
        /// Adds an operation to handle changes in the annotations defined on tables.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <typeparam name="TColumns">
        /// The columns in this operation. You do not need to specify this type, it will
        /// be inferred from the columnsAction parameter you supply.
        /// </typeparam>
        /// <param name="name"> The name of the table. Schema name is optional, if no schema is specified then dbo is assumed. </param>
        /// <param name="columnsAction">
        /// An action that specifies the columns to be included in the table. i.e. t => new { Id =
        /// t.Int(identity: true), Name = t.String() }
        /// </param>
        /// <param name="annotations">The custom annotations on the table that have changed.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AlterTableAnnotations<TColumns>(
            string name,
            Func<ColumnBuilder, TColumns> columnsAction,
            IDictionary<string, AnnotationValues> annotations,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columnsAction, "columnsAction");

            var operation = new AlterTableOperation(name, annotations, anonymousArguments);

            AddColumns(columnsAction(new ColumnBuilder()), operation.Columns);

            AddOperation(operation);
        }

        private static void AddColumns<TColumns>(TColumns columns, ICollection<ColumnModel> columnModels)
        {
            columns.GetType().GetNonIndexerProperties()
                .Each(
                    (p, i) =>
                    {
                        var columnModel = p.GetValue(columns, null) as ColumnModel;

                        if (columnModel != null)
                        {
                            columnModel.ApiPropertyInfo = p;

                            if (string.IsNullOrWhiteSpace(columnModel.Name))
                            {
                                columnModel.Name = p.Name;
                            }

                            columnModels.Add(columnModel);
                        }
                    });
        }

        /// <summary>
        /// Adds an operation to create a new foreign key constraint.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key column. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="dependentColumn"> The foreign key column. </param>
        /// <param name="principalTable">
        /// The table that contains the column this foreign key references. Schema name is optional,
        /// if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="principalColumn">
        /// The column this foreign key references. If no value is supplied the primary key of the
        /// principal table will be referenced.
        /// </param>
        /// <param name="cascadeDelete">
        /// A value indicating if cascade delete should be configured for the foreign key
        /// relationship. If no value is supplied, cascade delete will be off.
        /// </param>
        /// <param name="name">
        /// The name of the foreign key constraint in the database. If no value is supplied a unique name will
        /// be generated.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AddForeignKey(
            string dependentTable,
            string dependentColumn,
            string principalTable,
            string principalColumn = null,
            bool cascadeDelete = false,
            string name = null,
            object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotEmpty(dependentColumn, "dependentColumn");
            Check.NotEmpty(principalTable, "principalTable");

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
        /// Adds an operation to create a new foreign key constraint.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key columns. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="dependentColumns"> The foreign key columns. </param>
        /// <param name="principalTable">
        /// The table that contains the columns this foreign key references. Schema name is optional,
        /// if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="principalColumns">
        /// The columns this foreign key references. If no value is supplied the primary key of the
        /// principal table will be referenced.
        /// </param>
        /// <param name="cascadeDelete">
        /// A value indicating if cascade delete should be configured for the foreign key
        /// relationship. If no value is supplied, cascade delete will be off.
        /// </param>
        /// <param name="name">
        /// The name of the foreign key constraint in the database. If no value is supplied a unique name will
        /// be generated.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AddForeignKey(
            string dependentTable,
            string[] dependentColumns,
            string principalTable,
            string[] principalColumns = null,
            bool cascadeDelete = false,
            string name = null,
            object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotNull(dependentColumns, "dependentColumns");
            Check.NotEmpty(principalTable, "principalTable");

            if (!dependentColumns.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("dependentColumns", "AddForeignKey"));
            }

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
        /// Adds an operation to drop a foreign key constraint based on its name.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key column. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the foreign key constraint in the database. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropForeignKey(string dependentTable, string name, object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotEmpty(name, "name");

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(anonymousArguments)
                  {
                      DependentTable = dependentTable,
                      Name = name
                  };

            AddOperation(dropForeignKeyOperation);
        }

        /// <summary>
        /// Adds an operation to drop a foreign key constraint based on the column it targets.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key column. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="dependentColumn"> The foreign key column. </param>
        /// <param name="principalTable">
        /// The table that contains the column this foreign key references. Schema name is optional,
        /// if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropForeignKey(
            string dependentTable,
            string dependentColumn,
            string principalTable,
            object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotEmpty(dependentColumn, "dependentColumn");
            Check.NotEmpty(principalTable, "principalTable");

            DropForeignKey(
                dependentTable,
                new[] { dependentColumn },
                principalTable,
                anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop a foreign key constraint based on the column it targets.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key column.
        /// Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="dependentColumn">The foreign key column.</param>
        /// <param name="principalTable">
        /// The table that contains the column this foreign key references.
        /// Schema name is optional, if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="principalColumn">The columns this foreign key references.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers.
        /// Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "principalColumn")]
        [Obsolete("The principalColumn parameter is no longer required and can be removed.")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropForeignKey(
            string dependentTable,
            string dependentColumn,
            string principalTable,
            string principalColumn,
            object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotEmpty(dependentColumn, "dependentColumn");
            Check.NotEmpty(principalTable, "principalTable");

            DropForeignKey(
                dependentTable,
                new[] { dependentColumn },
                principalTable,
                anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop a foreign key constraint based on the columns it targets.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="dependentTable">
        /// The table that contains the foreign key columns. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="dependentColumns"> The foreign key columns. </param>
        /// <param name="principalTable">
        /// The table that contains the columns this foreign key references. Schema name is optional,
        /// if no schema is specified then dbo is assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropForeignKey(
            string dependentTable,
            string[] dependentColumns,
            string principalTable,
            object anonymousArguments = null)
        {
            Check.NotEmpty(dependentTable, "dependentTable");
            Check.NotNull(dependentColumns, "dependentColumns");
            Check.NotEmpty(principalTable, "principalTable");

            if (!dependentColumns.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("dependentColumns", "DropForeignKey"));
            }

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
        /// Adds an operation to drop a table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be dropped. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropTable(string name, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            DropTable(name, null, null, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop a table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be dropped. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="removedColumnAnnotations">Custom annotations that exist on columns of the table that is being dropped. May be null or empty.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected internal void DropTable(
            string name,
            IDictionary<string, IDictionary<string, object>> removedColumnAnnotations,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            DropTable(name, null, removedColumnAnnotations, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop a table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be dropped. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="removedAnnotations">Custom annotations that exist on the table that is being dropped. May be null or empty.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropTable(
            string name,
            IDictionary<string, object> removedAnnotations,
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            DropTable(name, removedAnnotations, null, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop a table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be dropped. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="removedAnnotations">Custom annotations that exist on the table that is being dropped. May be null or empty.</param>
        /// <param name="removedColumnAnnotations">Custom annotations that exist on columns of the table that is being dropped. May be null or empty.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected internal void DropTable(
            string name,
            IDictionary<string, object> removedAnnotations,
            IDictionary<string, IDictionary<string, object>> removedColumnAnnotations, 
            object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            AddOperation(new DropTableOperation(name, removedAnnotations, removedColumnAnnotations, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to move a table to a new schema.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be moved. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="newSchema"> The schema the table is to be moved to. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void MoveTable(string name, string newSchema, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            AddOperation(new MoveTableOperation(name, newSchema, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to move a stored procedure to a new schema.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the stored procedure to be moved. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="newSchema"> The schema the stored procedure is to be moved to. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void MoveStoredProcedure(string name, string newSchema, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");

            AddOperation(new MoveProcedureOperation(name, newSchema, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to rename a table. To change the schema of a table use MoveTable.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be renamed. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="newName">
        /// The new name for the table. Schema name is optional, if no schema is specified then dbo is
        /// assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void RenameTable(string name, string newName, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            AddOperation(new RenameTableOperation(name, newName, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to rename a stored procedure. To change the schema of a stored procedure use MoveStoredProcedure
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">
        /// The name of the stored procedure to be renamed. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="newName">
        /// The new name for the stored procedure. Schema name is optional, if no schema is specified then
        /// dbo is assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void RenameStoredProcedure(string name, string newName, object anonymousArguments = null)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            AddOperation(new RenameProcedureOperation(name, newName, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to rename a column.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table that contains the column to be renamed. Schema name is optional, if no
        /// schema is specified then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the column to be renamed. </param>
        /// <param name="newName"> The new name for the column. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void RenameColumn(
            string table, string name, string newName, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            AddOperation(new RenameColumnOperation(table, name, newName, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to add a column to an existing table.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to add the column to. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the column to be added. </param>
        /// <param name="columnAction">
        /// An action that specifies the column to be added. i.e. c => c.Int(nullable: false,
        /// defaultValue: 3)
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AddColumn(
            string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");
            Check.NotNull(columnAction, "columnAction");

            var columnModel = columnAction(new ColumnBuilder());

            columnModel.Name = name;

            AddOperation(new AddColumnOperation(table, columnModel, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to drop an existing column.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to drop the column from. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the column to be dropped. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropColumn(string table, string name, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");

            DropColumn(table, name, null, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to drop an existing column.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to drop the column from. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the column to be dropped. </param>
        /// <param name="removedAnnotations">Custom annotations that exist on the column that is being dropped. May be null or empty.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropColumn(
            string table, string name, IDictionary<string, object> removedAnnotations, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");

            AddOperation(new DropColumnOperation(table, name, removedAnnotations, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to alter the definition of an existing column.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table the column exists in. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the column to be changed. </param>
        /// <param name="columnAction">
        /// An action that specifies the new definition for the column. i.e. c => c.String(nullable:
        /// false, defaultValue: "none")
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AlterColumn(
           string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");
            Check.NotNull(columnAction, "columnAction");

            var columnModel = columnAction(new ColumnBuilder());

            columnModel.Name = name;

            AddOperation(
                new AlterColumnOperation(
                    table, columnModel, isDestructiveChange: false, anonymousArguments: anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to create a new primary key.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The table that contains the primary key column. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="column"> The primary key column. </param>
        /// <param name="name">
        /// The name of the primary key in the database. If no value is supplied a unique name will be
        /// generated.
        /// </param>
        /// <param name="clustered"> A value indicating whether or not this is a clustered primary key. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AddPrimaryKey(
            string table,
            string column,
            string name = null,
            bool clustered = true,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(column, "column");

            AddPrimaryKey(table, new[] { column }, name, clustered, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to create a new primary key based on multiple columns.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The table that contains the primary key columns. Schema name is optional, if no schema is
        /// specified then dbo is assumed.
        /// </param>
        /// <param name="columns"> The primary key columns. </param>
        /// <param name="name">
        /// The name of the primary key in the database. If no value is supplied a unique name will be
        /// generated.
        /// </param>
        /// <param name="clustered"> A value indicating whether or not this is a clustered primary key. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void AddPrimaryKey(
            string table,
            string[] columns,
            string name = null,
            bool clustered = true,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(columns, "columns");

            if (!columns.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("columns", "AddPrimaryKey"));
            }

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation(anonymousArguments)
                  {
                      Table = table,
                      Name = name,
                      IsClustered = clustered
                  };

            columns.Each(c => addPrimaryKeyOperation.Columns.Add(c));

            AddOperation(addPrimaryKeyOperation);
        }

        /// <summary>
        /// Adds an operation to drop an existing primary key that does not have the default name.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The table that contains the primary key column. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the primary key to be dropped. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropPrimaryKey(string table, string name, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation(anonymousArguments)
                  {
                      Table = table,
                      Name = name,
                  };

            AddOperation(dropPrimaryKeyOperation);
        }

        /// <summary>
        /// Adds an operation to drop an existing primary key that was created with the default name.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The table that contains the primary key column. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropPrimaryKey(string table, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation(anonymousArguments)
                  {
                      Table = table,
                  };

            AddOperation(dropPrimaryKeyOperation);
        }

        /// <summary>
        /// Adds an operation to create an index on a single column.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to create the index on. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="column"> The name of the column to create the index on. </param>
        /// <param name="unique">
        /// A value indicating if this is a unique index. If no value is supplied a non-unique index will be
        /// created.
        /// </param>
        /// <param name="name">
        /// The name to use for the index in the database. If no value is supplied a unique name will be
        /// generated.
        /// </param>
        /// <param name="clustered"> A value indicating whether or not this is a clustered index. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void CreateIndex(
            string table,
            string column,
            bool unique = false,
            string name = null,
            bool clustered = false,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(column, "column");

            CreateIndex(table, new[] { column }, unique, name, clustered, anonymousArguments);
        }

        /// <summary>
        /// Adds an operation to create an index on multiple columns.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to create the index on. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="columns"> The name of the columns to create the index on. </param>
        /// <param name="unique">
        /// A value indicating if this is a unique index. If no value is supplied a non-unique index will be
        /// created.
        /// </param>
        /// <param name="name">
        /// The name to use for the index in the database. If no value is supplied a unique name will be
        /// generated.
        /// </param>
        /// <param name="clustered"> A value indicating whether or not this is a clustered index. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void CreateIndex(
            string table,
            string[] columns,
            bool unique = false,
            string name = null,
            bool clustered = false,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(columns, "columns");

            if (!columns.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("columns", "CreateIndex"));
            }

            var createIndexOperation
                = new CreateIndexOperation(anonymousArguments)
                  {
                      Table = table,
                      IsUnique = unique,
                      Name = name,
                      IsClustered = clustered
                  };

            columns.Each(c => createIndexOperation.Columns.Add(c));

            AddOperation(createIndexOperation);
        }

        /// <summary>
        /// Adds an operation to drop an index based on its name.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to drop the index from. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the index to be dropped. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropIndex(
            string table,
            string name,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");

            var dropIndexOperation
                = new DropIndexOperation(anonymousArguments)
                  {
                      Table = table,
                      Name = name,
                  };

            AddOperation(dropIndexOperation);
        }

        /// <summary>
        /// Adds an operation to drop an index based on the columns it targets.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table to drop the index from. Schema name is optional, if no schema is specified
        /// then dbo is assumed.
        /// </param>
        /// <param name="columns"> The name of the column(s) the index targets. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void DropIndex(
            string table,
            string[] columns,
            object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(columns, "columns");

            if (!columns.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("columns", "DropIndex"));
            }

            var dropIndexOperation
                = new DropIndexOperation(anonymousArguments)
                  {
                      Table = table,
                  };

            columns.Each(c => dropIndexOperation.Columns.Add(c));

            AddOperation(dropIndexOperation);
        }

        /// <summary>
        /// Adds an operation to rename an index.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table">
        /// The name of the table that contains the index to be renamed. Schema name is optional, if no
        /// schema is specified then dbo is assumed.
        /// </param>
        /// <param name="name"> The name of the index to be renamed. </param>
        /// <param name="newName"> The new name for the index. </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected internal void RenameIndex(
            string table, string name, string newName, object anonymousArguments = null)
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            AddOperation(new RenameIndexOperation(table, name, newName, anonymousArguments));
        }

        /// <summary>
        /// Adds an operation to execute a SQL command or set of SQL commands.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="sql"> The SQL to be executed. </param>
        /// <param name="suppressTransaction">
        /// A value indicating if the SQL should be executed outside of the transaction being
        /// used for the migration process. If no value is supplied the SQL will be executed within the transaction.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        protected internal void Sql(string sql, bool suppressTransaction = false, object anonymousArguments = null)
        {
            Check.NotEmpty(sql, "sql");

            AddOperation(
                new SqlOperation(sql, anonymousArguments)
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// Adds an operation to execute a SQL file.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="sqlFile"> 
        /// The SQL file to be executed.  Relative paths are assumed to be relative to the current AppDomain's BaseDirectory.
        /// </param>
        /// <param name="suppressTransaction">
        /// A value indicating if the SQL should be executed outside of the transaction being
        /// used for the migration process. If no value is supplied the SQL will be executed within the transaction.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        protected internal void SqlFile(string sqlFile, bool suppressTransaction = false, object anonymousArguments = null)
        {
            Check.NotEmpty(sqlFile, "sqlFile");

            if (!Path.IsPathRooted(sqlFile))
            {
                sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqlFile);
            }

            AddOperation(
                new SqlOperation(File.ReadAllText(sqlFile), anonymousArguments)
                {
                    SuppressTransaction = suppressTransaction
                });
        }

        /// <summary>
        /// Adds an operation to execute a SQL resource file.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="sqlResource"> The manifest resource name of the SQL resource file to be executed. </param>
        /// <param name="resourceAssembly">
        /// The assembly containing the resource file. The calling assembly is assumed if not provided.
        /// </param>
        /// <param name="suppressTransaction">
        /// A value indicating if the SQL should be executed outside of the transaction being
        /// used for the migration process. If no value is supplied the SQL will be executed within the transaction.
        /// </param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        protected internal void SqlResource(string sqlResource, Assembly resourceAssembly = null, bool suppressTransaction = false, object anonymousArguments = null)
        {
            Check.NotEmpty(sqlResource, "sqlResource");

            resourceAssembly = resourceAssembly ?? Assembly.GetCallingAssembly();

            if (!resourceAssembly.GetManifestResourceNames().Contains(sqlResource))
            {
                throw new ArgumentException(Strings.UnableToLoadEmbeddedResource(resourceAssembly.FullName, sqlResource));
            }

            using (var resourceStream = resourceAssembly.GetManifestResourceStream(sqlResource))
            {
                using (var textStream = new StreamReader(resourceStream))
                {
                    AddOperation(
                        new SqlOperation(textStream.ReadToEnd(), anonymousArguments)
                        {
                            SuppressTransaction = suppressTransaction
                        });
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IDbMigration.AddOperation(MigrationOperation migrationOperation)
        {
            AddOperation(migrationOperation);
        }

        internal void AddOperation(MigrationOperation migrationOperation)
        {
            Check.NotNull(migrationOperation, "migrationOperation");

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

        internal VersionedModel GetSourceModel()
        {
            return GetModel(mm => mm.Source);
        }

        internal VersionedModel GetTargetModel()
        {
            return GetModel(mm => mm.Target);
        }

        private VersionedModel GetModel(Func<IMigrationMetadata, string> modelAccessor)
        {
            var migrationMetadata = (IMigrationMetadata)this;

            var modelData = modelAccessor(migrationMetadata);

            if (string.IsNullOrWhiteSpace(modelData))
            {
                return null;
            }

            var generatedCodeAttribute
                = GetType().GetCustomAttributes<GeneratedCodeAttribute>(inherit: false)
                    .SingleOrDefault();

            var version
                = generatedCodeAttribute != null
                  && !string.IsNullOrWhiteSpace(generatedCodeAttribute.Version)
                    ? generatedCodeAttribute.Version
                    : typeof(DbMigration).Assembly().GetInformationalVersion();

            return new VersionedModel(
                new ModelCompressor().Decompress(Convert.FromBase64String(modelData)),
                version);
        }

        #region Hide object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected new object MemberwiseClone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}
