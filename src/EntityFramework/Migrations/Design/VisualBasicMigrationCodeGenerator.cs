// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.VisualBasic;

    /// <summary>
    ///     Generates VB.Net code for a code-based migration.
    /// </summary>
    public class VisualBasicMigrationCodeGenerator : MigrationCodeGenerator
    {
        private IEnumerable<Tuple<CreateTableOperation, AddForeignKeyOperation>> _newTableForeignKeys;
        private IEnumerable<Tuple<CreateTableOperation, CreateIndexOperation>> _newTableIndexes;

        /// <inheritdoc />
        public override ScaffoldedMigration Generate(
            string migrationId,
            IEnumerable<MigrationOperation> operations,
            string sourceModel,
            string targetModel,
            string @namespace,
            string className)
        {
            className = ScrubName(className);

            _newTableForeignKeys
                = (from ct in operations.OfType<CreateTableOperation>()
                   from cfk in operations.OfType<AddForeignKeyOperation>()
                   where ct.Name.EqualsIgnoreCase(cfk.DependentTable)
                   select Tuple.Create(ct, cfk)).ToList();

            _newTableIndexes
                = (from ct in operations.OfType<CreateTableOperation>()
                   from cfk in operations.OfType<CreateIndexOperation>()
                   where ct.Name.EqualsIgnoreCase(cfk.Table)
                   select Tuple.Create(ct, cfk)).ToList();

            var generatedMigration
                = new ScaffoldedMigration
                      {
                          MigrationId = migrationId,
                          Language = "vb",
                          UserCode = Generate(operations, @namespace, className),
                          DesignerCode = Generate(migrationId, sourceModel, targetModel, @namespace, className)
                      };

            if (!string.IsNullOrWhiteSpace(sourceModel))
            {
                generatedMigration.Resources.Add("Source", sourceModel);
            }

            generatedMigration.Resources.Add("Target", targetModel);

            return generatedMigration;
        }

        /// <summary>
        ///     Generates the primary code file that the user can view and edit.
        /// </summary>
        /// <param name="operations"> Operations to be performed by the migration. </param>
        /// <param name="namespace"> Namespace that code should be generated in. </param>
        /// <param name="className"> Name of the class that should be generated. </param>
        /// <returns> The generated code. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected virtual string Generate(
            IEnumerable<MigrationOperation> operations, string @namespace, string className)
        {
            Contract.Requires(operations != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(className));

            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var writer = new IndentedTextWriter(stringWriter, "    "))
                {
                    WriteClassStart(
                        @namespace, className, writer, "Inherits DbMigration", designer: false,
                        namespaces: GetNamespaces(operations));

                    writer.WriteLine("Public Overrides Sub Up()");
                    writer.Indent++;

                    operations
                        .Except(_newTableForeignKeys.Select(t => t.Item2))
                        .Except(_newTableIndexes.Select(t => t.Item2))
                        .Each<dynamic>(o => Generate(o, writer));

                    writer.Indent--;
                    writer.WriteLine("End Sub");

                    writer.WriteLine();

                    writer.WriteLine("Public Overrides Sub Down()");
                    writer.Indent++;

                    operations
                        .Select(o => o.Inverse)
                        .Where(o => o != null)
                        .Reverse()
                        .Each<dynamic>(o => Generate(o, writer));

                    writer.Indent--;
                    writer.WriteLine("End Sub");

                    WriteClassEnd(@namespace, writer);
                }

                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     Generates the code behind file with migration metadata.
        /// </summary>
        /// <param name="migrationId"> Unique identifier of the migration. </param>
        /// <param name="sourceModel"> Source model to be stored in the migration metadata. </param>
        /// <param name="targetModel"> Target model to be stored in the migration metadata. </param>
        /// <param name="namespace"> Namespace that code should be generated in. </param>
        /// <param name="className"> Name of the class that should be generated. </param>
        /// <returns> The generated code. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected virtual string Generate(
            string migrationId, string sourceModel, string targetModel, string @namespace, string className)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Requires(!string.IsNullOrWhiteSpace(targetModel));
            Contract.Requires(!string.IsNullOrWhiteSpace(className));

            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var writer = new IndentedTextWriter(stringWriter, "    "))
                {
                    writer.WriteLine("' <auto-generated />");

                    WriteClassStart(@namespace, className, writer, "Implements IMigrationMetadata", designer: true);

                    writer.Write("Private ReadOnly Resources As New ResourceManager(GetType(");
                    writer.Write(className);
                    writer.WriteLine("))");
                    writer.WriteLine();

                    WriteProperty("Id", Quote(migrationId), writer);
                    writer.WriteLine();
                    WriteProperty(
                        "Source",
                        sourceModel == null
                            ? null
                            : "Resources.GetString(\"Source\")",
                        writer);
                    writer.WriteLine();
                    WriteProperty("Target", "Resources.GetString(\"Target\")", writer);

                    WriteClassEnd(@namespace, writer);
                }

                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     Generates a property to return the source or target model in the code behind file.
        /// </summary>
        /// <param name="name"> Name of the property. </param>
        /// <param name="value"> Value to be returned. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void WriteProperty(string name, string value, IndentedTextWriter writer)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(writer != null);

            writer.Write("Private ReadOnly Property IMigrationMetadata_");
            writer.Write(name);
            writer.Write("() As String Implements IMigrationMetadata.");
            writer.WriteLine(name);
            writer.Indent++;
            writer.WriteLine("Get");
            writer.Indent++;
            writer.Write("Return ");

            if (value == null)
            {
                writer.WriteLine("Nothing");
            }
            else
            {
                writer.WriteLine(value);
            }

            writer.Indent--;
            writer.WriteLine("End Get");
            writer.Indent--;
            writer.WriteLine("End Property");
        }

        /// <summary>
        ///     Generates a namespace, using statements and class definition.
        /// </summary>
        /// <param name="namespace"> Namespace that code should be generated in. </param>
        /// <param name="className"> Name of the class that should be generated. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        /// <param name="base"> Base class for the generated class. </param>
        /// <param name="designer"> A value indicating if this class is being generated for a code-behind file. </param>
        /// <param name="namespaces"> Namespaces for which Imports directives will be added. If null, then the namespaces returned from GetDefaultNamespaces will be used. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "base")]
        protected virtual void WriteClassStart(
            string @namespace, string className, IndentedTextWriter writer, string @base, bool designer = false,
            IEnumerable<string> namespaces = null)
        {
            Contract.Requires(writer != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(className));
            Contract.Requires(!string.IsNullOrWhiteSpace(@base));

            (namespaces ?? GetDefaultNamespaces(designer)).Each(n => writer.WriteLine("Imports " + n));

            writer.WriteLine();

            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                writer.Write("Namespace ");
                writer.WriteLine(@namespace);
                writer.Indent++;
            }

            writer.Write("Public ");

            if (designer)
            {
                writer.Write("NotInheritable ");
            }

            writer.Write("Partial Class ");
            writer.Write(className);

            writer.WriteLine();
            writer.Indent++;
            writer.WriteLine(@base);
            writer.Indent--;

            writer.WriteLine();
            writer.Indent++;
        }

        /// <summary>
        ///     Generates the closing code for a class that was started with WriteClassStart.
        /// </summary>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace")]
        protected virtual void WriteClassEnd(string @namespace, IndentedTextWriter writer)
        {
            Contract.Requires(writer != null);

            writer.Indent--;
            writer.WriteLine("End Class");

            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                writer.Indent--;
                writer.WriteLine("End Namespace");
            }
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="addColumnOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(AddColumnOperation addColumnOperation, IndentedTextWriter writer)
        {
            Contract.Requires(addColumnOperation != null);
            Contract.Requires(writer != null);

            writer.Write("AddColumn(");
            writer.Write(Quote(addColumnOperation.Table));
            writer.Write(", ");
            writer.Write(Quote(addColumnOperation.Column.Name));
            writer.Write(", Function(c)");
            Generate(addColumnOperation.Column, writer);
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="DropColumnOperation" />.
        /// </summary>
        /// <param name="dropColumnOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(DropColumnOperation dropColumnOperation, IndentedTextWriter writer)
        {
            Contract.Requires(dropColumnOperation != null);
            Contract.Requires(writer != null);

            writer.Write("DropColumn(");
            writer.Write(Quote(dropColumnOperation.Table));
            writer.Write(", ");
            writer.Write(Quote(dropColumnOperation.Name));
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AlterColumnOperation" />.
        /// </summary>
        /// <param name="alterColumnOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(AlterColumnOperation alterColumnOperation, IndentedTextWriter writer)
        {
            Contract.Requires(alterColumnOperation != null);
            Contract.Requires(writer != null);

            writer.Write("AlterColumn(");
            writer.Write(Quote(alterColumnOperation.Table));
            writer.Write(", ");
            writer.Write(Quote(alterColumnOperation.Column.Name));
            writer.Write(", Function(c)");
            Generate(alterColumnOperation.Column, writer);
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="createTableOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(CreateTableOperation createTableOperation, IndentedTextWriter writer)
        {
            Contract.Requires(createTableOperation != null);
            Contract.Requires(writer != null);

            writer.WriteLine("CreateTable(");
            writer.Indent++;
            writer.WriteLine(Quote(createTableOperation.Name) + ",");

            writer.WriteLine("Function(c) New With");
            writer.Indent++;
            writer.WriteLine("{");
            writer.Indent++;

            var columnCount = createTableOperation.Columns.Count();

            createTableOperation.Columns.Each(
                (c, i) =>
                    {
                        var scrubbedName = ScrubName(c.Name);

                        writer.Write(".");
                        writer.Write(scrubbedName);
                        writer.Write(" =");
                        Generate(c, writer, !string.Equals(c.Name, scrubbedName, StringComparison.Ordinal));

                        if (i < columnCount - 1)
                        {
                            writer.Write(",");
                        }

                        writer.WriteLine();
                    });

            writer.Indent--;
            writer.Write("}");
            writer.Indent--;
            writer.Write(")");

            GenerateInline(createTableOperation.PrimaryKey, writer);

            _newTableForeignKeys
                .Where(t => t.Item1 == createTableOperation)
                .Each(t => GenerateInline(t.Item2, writer));

            _newTableIndexes
                .Where(t => t.Item1 == createTableOperation)
                .Each(t => GenerateInline(t.Item2, writer));

            writer.WriteLine();
            writer.Indent--;
            writer.WriteLine();
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AddPrimaryKeyOperation" /> as part of a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="addPrimaryKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void GenerateInline(AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(writer != null);

            if (addPrimaryKeyOperation != null)
            {
                writer.WriteLine(" _");
                writer.Write(".PrimaryKey(");

                Generate(addPrimaryKeyOperation.Columns, writer);

                if (!addPrimaryKeyOperation.HasDefaultName)
                {
                    writer.Write(", name := ");
                    writer.Write(Quote(addPrimaryKeyOperation.Name));
                }

                writer.Write(")");
            }
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AddForeignKeyOperation" /> as part of a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="addForeignKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void GenerateInline(AddForeignKeyOperation addForeignKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(addForeignKeyOperation != null);
            Contract.Requires(writer != null);

            writer.WriteLine(" _");
            writer.Write(".ForeignKey(" + Quote(addForeignKeyOperation.PrincipalTable) + ", ");
            Generate(addForeignKeyOperation.DependentColumns, writer);

            if (addForeignKeyOperation.CascadeDelete)
            {
                writer.Write(", cascadeDelete := True");
            }

            writer.Write(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="CreateIndexOperation" /> as part of a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="createIndexOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void GenerateInline(CreateIndexOperation createIndexOperation, IndentedTextWriter writer)
        {
            Contract.Requires(createIndexOperation != null);
            Contract.Requires(writer != null);

            writer.WriteLine(" _");
            writer.Write(".Index(");
            Generate(createIndexOperation.Columns, writer);
            writer.Write(")");
        }

        /// <summary>
        ///     Generates code to specify a set of column names using a lambda expression.
        /// </summary>
        /// <param name="columns"> The columns to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(IEnumerable<string> columns, IndentedTextWriter writer)
        {
            Contract.Requires(columns != null);
            Contract.Requires(writer != null);

            writer.Write("Function(t) ");

            if (columns.Count() == 1)
            {
                writer.Write("t." + columns.Single());
            }
            else
            {
                writer.Write("New With { " + columns.Join(c => "t." + c) + " }");
            }
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AddForeignKeyOperation" />.
        /// </summary>
        /// <param name="addForeignKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(AddForeignKeyOperation addForeignKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(addForeignKeyOperation != null);
            Contract.Requires(writer != null);

            writer.Write("AddForeignKey(");
            writer.Write(Quote(addForeignKeyOperation.DependentTable));
            writer.Write(", ");

            var compositeKey = addForeignKeyOperation.DependentColumns.Count() > 1;

            if (compositeKey)
            {
                writer.Write("New String() { ");
            }

            writer.Write(addForeignKeyOperation.DependentColumns.Join(Quote));

            if (compositeKey)
            {
                writer.Write(" }");
            }

            writer.Write(", ");
            writer.Write(Quote(addForeignKeyOperation.PrincipalTable));

            if (addForeignKeyOperation.PrincipalColumns.Any())
            {
                writer.Write(", ");

                if (compositeKey)
                {
                    writer.Write("New String() { ");
                }

                writer.Write(addForeignKeyOperation.PrincipalColumns.Join(Quote));

                if (compositeKey)
                {
                    writer.Write(" }");
                }
            }

            if (addForeignKeyOperation.CascadeDelete)
            {
                writer.Write(", cascadeDelete := True");
            }

            if (!addForeignKeyOperation.HasDefaultName)
            {
                writer.Write(", name := ");
                writer.Write(Quote(addForeignKeyOperation.Name));
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="DropForeignKeyOperation" />.
        /// </summary>
        /// <param name="dropForeignKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(DropForeignKeyOperation dropForeignKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(dropForeignKeyOperation != null);
            Contract.Requires(writer != null);

            writer.Write("DropForeignKey(");
            writer.Write(Quote(dropForeignKeyOperation.DependentTable));
            writer.Write(", ");

            if (!dropForeignKeyOperation.HasDefaultName)
            {
                writer.Write(Quote(dropForeignKeyOperation.Name));
            }
            else
            {
                var compositeKey = dropForeignKeyOperation.DependentColumns.Count() > 1;

                if (compositeKey)
                {
                    writer.Write("New String() { ");
                }

                writer.Write(dropForeignKeyOperation.DependentColumns.Join(Quote));

                if (compositeKey)
                {
                    writer.Write(" }");
                }

                writer.Write(", ");
                writer.Write(Quote(dropForeignKeyOperation.PrincipalTable));
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform an <see cref="AddPrimaryKeyOperation" />.
        /// </summary>
        /// <param name="addPrimaryKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(addPrimaryKeyOperation != null);
            Contract.Requires(writer != null);

            writer.Write("AddPrimaryKey(");
            writer.Write(Quote(addPrimaryKeyOperation.Table));
            writer.Write(", ");

            var compositeIndex = addPrimaryKeyOperation.Columns.Count() > 1;

            if (compositeIndex)
            {
                writer.Write("New String() { ");
            }

            writer.Write(addPrimaryKeyOperation.Columns.Join(Quote));

            if (compositeIndex)
            {
                writer.Write(" }");
            }

            if (!addPrimaryKeyOperation.HasDefaultName)
            {
                writer.Write(", name := ");
                writer.Write(Quote(addPrimaryKeyOperation.Name));
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="DropPrimaryKeyOperation" />.
        /// </summary>
        /// <param name="dropPrimaryKeyOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation, IndentedTextWriter writer)
        {
            Contract.Requires(dropPrimaryKeyOperation != null);
            Contract.Requires(writer != null);

            writer.Write("DropPrimaryKey(");
            writer.Write(Quote(dropPrimaryKeyOperation.Table));
            writer.Write(", ");

            if (!dropPrimaryKeyOperation.HasDefaultName)
            {
                writer.Write(Quote(dropPrimaryKeyOperation.Name));
            }
            else
            {
                writer.Write("New String() { ");
                writer.Write(dropPrimaryKeyOperation.Columns.Join(Quote));
                writer.Write(" }");
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="CreateIndexOperation" />.
        /// </summary>
        /// <param name="createIndexOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(CreateIndexOperation createIndexOperation, IndentedTextWriter writer)
        {
            Contract.Requires(createIndexOperation != null);
            Contract.Requires(writer != null);

            writer.Write("CreateIndex(");
            writer.Write(Quote(createIndexOperation.Table));
            writer.Write(", ");

            var compositeIndex = createIndexOperation.Columns.Count() > 1;

            if (compositeIndex)
            {
                writer.Write("New String() { ");
            }

            writer.Write(createIndexOperation.Columns.Join(Quote));

            if (compositeIndex)
            {
                writer.Write(" }");
            }

            if (createIndexOperation.IsUnique)
            {
                writer.Write(", unique := True");
            }

            if (!createIndexOperation.HasDefaultName)
            {
                writer.Write(", name := ");
                writer.Write(Quote(createIndexOperation.Name));
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="DropIndexOperation" />.
        /// </summary>
        /// <param name="dropIndexOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(DropIndexOperation dropIndexOperation, IndentedTextWriter writer)
        {
            Contract.Requires(dropIndexOperation != null);
            Contract.Requires(writer != null);

            writer.Write("DropIndex(");
            writer.Write(Quote(dropIndexOperation.Table));
            writer.Write(", ");

            if (!dropIndexOperation.HasDefaultName)
            {
                writer.Write(Quote(dropIndexOperation.Name));
            }
            else
            {
                writer.Write("New String() { ");
                writer.Write(dropIndexOperation.Columns.Join(Quote));
                writer.Write(" }");
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to specify the definition for a <see cref="ColumnModel" />.
        /// </summary>
        /// <param name="column"> The column definition to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        /// <param name="emitName"> A value indicating whether to include the column name in the definition. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        protected virtual void Generate(ColumnModel column, IndentedTextWriter writer, bool emitName = false)
        {
            Contract.Requires(column != null);
            Contract.Requires(writer != null);

            writer.Write(" c.");
            writer.Write(TranslateColumnType(column.Type));
            writer.Write("(");

            var args = new List<string>();

            if (emitName)
            {
                args.Add("name := " + Quote(column.Name));
            }

            if (column.IsNullable == false)
            {
                args.Add("nullable := False");
            }

            if (column.MaxLength != null)
            {
                args.Add("maxLength := " + column.MaxLength);
            }

            if (column.Precision != null)
            {
                args.Add("precision := " + column.Precision);
            }

            if (column.Scale != null)
            {
                args.Add("scale := " + column.Scale);
            }

            if (column.IsFixedLength != null)
            {
                args.Add("fixedLength := " + column.IsFixedLength.ToString().ToLowerInvariant());
            }

            if (column.IsUnicode != null)
            {
                args.Add("unicode := " + column.IsUnicode.ToString().ToLowerInvariant());
            }

            if (column.IsIdentity)
            {
                args.Add("identity := True");
            }

            if (column.DefaultValue != null)
            {
                args.Add("defaultValue := " + Generate((dynamic)column.DefaultValue));
            }

            if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
            {
                args.Add("defaultValueSql := " + Quote(column.DefaultValueSql));
            }

            if (column.IsTimestamp)
            {
                args.Add("timestamp := True");
            }

            if (!string.IsNullOrWhiteSpace(column.StoreType))
            {
                args.Add("storeType := " + Quote(column.StoreType));
            }

            writer.Write(args.Join());
            writer.Write(")");
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="T:byte[]" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(byte[] defaultValue)
        {
            return "New Byte() {" + defaultValue.Join() + "}";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="DateTime" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(DateTime defaultValue)
        {
            return "New DateTime(" + defaultValue.Ticks + ", DateTimeKind."
                   + Enum.GetName(typeof(DateTimeKind), defaultValue.Kind) + ")";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="DateTimeOffset" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(DateTimeOffset defaultValue)
        {
            return "New DateTimeOffset(" + defaultValue.Ticks + ", new TimeSpan("
                   + defaultValue.Offset.Ticks + "))";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="decimal" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(decimal defaultValue)
        {
            return defaultValue.ToString(CultureInfo.InvariantCulture) + "D";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="Guid" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(Guid defaultValue)
        {
            return "New Guid(\"" + defaultValue + "\")";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="long" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(long defaultValue)
        {
            return defaultValue.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="float" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(float defaultValue)
        {
            return defaultValue.ToString(CultureInfo.InvariantCulture) + "F";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="string" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(string defaultValue)
        {
            return Quote(defaultValue);
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="TimeSpan" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(TimeSpan defaultValue)
        {
            return "New TimeSpan(" + defaultValue.Ticks + ")";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="DbGeography" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(DbGeography defaultValue)
        {
            return "DbGeography.FromText(\"" + defaultValue.AsText() + "\", " + defaultValue.CoordinateSystemId + ")";
        }

        /// <summary>
        ///     Generates code to specify the default value for a <see cref="DbGeometry" /> column.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        protected virtual string Generate(DbGeometry defaultValue)
        {
            return "DbGeometry.FromText(\"" + defaultValue.AsText() + "\", " + defaultValue.CoordinateSystemId + ")";
        }

        /// <summary>
        ///     Generates code to specify the default value for a column of unknown data type.
        /// </summary>
        /// <param name="defaultValue"> The value to be used as the default. </param>
        /// <returns> Code representing the default value. </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        protected virtual string Generate(object defaultValue)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", defaultValue).ToLowerInvariant();
        }

        /// <summary>
        ///     Generates code to perform a <see cref="DropTableOperation" />.
        /// </summary>
        /// <param name="dropTableOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(DropTableOperation dropTableOperation, IndentedTextWriter writer)
        {
            Contract.Requires(dropTableOperation != null);
            Contract.Requires(writer != null);

            writer.Write("DropTable(");
            writer.Write(Quote(dropTableOperation.Name));
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="MoveTableOperation" />.
        /// </summary>
        /// <param name="moveTableOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(MoveTableOperation moveTableOperation, IndentedTextWriter writer)
        {
            Contract.Requires(moveTableOperation != null);
            Contract.Requires(writer != null);

            writer.Write("MoveTable(name := ");
            writer.Write(Quote(moveTableOperation.Name));
            writer.Write(", newSchema := ");
            writer.Write(
                string.IsNullOrWhiteSpace(moveTableOperation.NewSchema)
                    ? "Nothing"
                    : Quote(moveTableOperation.NewSchema));
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="RenameTableOperation" />.
        /// </summary>
        /// <param name="renameTableOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(RenameTableOperation renameTableOperation, IndentedTextWriter writer)
        {
            Contract.Requires(renameTableOperation != null);
            Contract.Requires(writer != null);

            writer.Write("RenameTable(name := ");
            writer.Write(Quote(renameTableOperation.Name));
            writer.Write(", newName := ");
            writer.Write(Quote(renameTableOperation.NewName));
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="RenameColumnOperation" />.
        /// </summary>
        /// <param name="renameColumnOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(RenameColumnOperation renameColumnOperation, IndentedTextWriter writer)
        {
            Contract.Requires(renameColumnOperation != null);
            Contract.Requires(writer != null);

            writer.Write("RenameColumn(table := ");
            writer.Write(Quote(renameColumnOperation.Table));
            writer.Write(", name := ");
            writer.Write(Quote(renameColumnOperation.Name));
            writer.Write(", newName := ");
            writer.Write(Quote(renameColumnOperation.NewName));
            writer.WriteLine(")");
        }

        /// <summary>
        ///     Generates code to perform a <see cref="SqlOperation" />.
        /// </summary>
        /// <param name="sqlOperation"> The operation to generate code for. </param>
        /// <param name="writer"> Text writer to add the generated code to. </param>
        protected virtual void Generate(SqlOperation sqlOperation, IndentedTextWriter writer)
        {
            Contract.Requires(sqlOperation != null);
            Contract.Requires(writer != null);

            writer.Write("Sql(");
            writer.Write(Quote(sqlOperation.Sql));

            if (sqlOperation.SuppressTransaction)
            {
                writer.Write(", suppressTransaction := True");
            }

            writer.WriteLine(")");
        }

        /// <summary>
        ///     Removes any invalid characters from the name of an database artifact.
        /// </summary>
        /// <param name="name"> The name to be scrubbed. </param>
        /// <returns> The scrubbed name. </returns>
        [SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule")]
        protected virtual string ScrubName(string name)
        {
            var invalidChars
                = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

            name = invalidChars.Replace(name, string.Empty);

            using (var codeProvider = new VBCodeProvider())
            {
                if (!char.IsLetter(name[0])
                    || !codeProvider.IsValidIdentifier(name))
                {
                    name = "_" + name;
                }
            }

            return name;
        }

        /// <summary>
        ///     Gets the type name to use for a column of the given data type.
        /// </summary>
        /// <param name="primitiveTypeKind"> The data type to translate. </param>
        /// <returns> The type name to use in the generated migration. </returns>
        protected virtual string TranslateColumnType(PrimitiveTypeKind primitiveTypeKind)
        {
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Int16:
                    return "Short";
                case PrimitiveTypeKind.Int32:
                    return "Int";
                case PrimitiveTypeKind.Int64:
                    return "Long";
                default:
                    return Enum.GetName(typeof(PrimitiveTypeKind), primitiveTypeKind);
            }
        }

        /// <summary>
        ///     Quotes an identifier using appropriate escaping to allow it to be stored in a string.
        /// </summary>
        /// <param name="identifier"> The identifier to be quoted. </param>
        /// <returns> The quoted identifier. </returns>
        protected virtual string Quote(string identifier)
        {
            return "\"" + identifier + "\"";
        }
    }
}
