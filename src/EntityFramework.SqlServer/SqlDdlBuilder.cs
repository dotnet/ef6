// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal sealed class SqlDdlBuilder
    {
        private readonly StringBuilder unencodedStringBuilder = new StringBuilder();
        private readonly HashSet<EntitySet> ignoredEntitySets = new HashSet<EntitySet>();

        #region Public Surface

        internal static string CreateObjectsScript(StoreItemCollection itemCollection, bool createSchemas)
        {
            var builder = new SqlDdlBuilder();

            foreach (var container in itemCollection.GetItems<EntityContainer>())
            {
                var entitySets = container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name);

                if (createSchemas)
                {
                    var schemas = new HashSet<string>(entitySets.Select(s => GetSchemaName(s)));
                    foreach (var schema in schemas.OrderBy(s => s))
                    {
                        // don't bother creating default schema
                        if (schema != "dbo")
                        {
                            builder.AppendCreateSchema(schema);
                        }
                    }
                }

                foreach (var entitySet in container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateTable(entitySet);
                }

                foreach (var associationSet in container.BaseEntitySets.OfType<AssociationSet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateForeignKeys(associationSet);
                }
            }
            return builder.GetCommandText();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string CreateDatabaseScript(string databaseName, string dataFileName, string logFileName)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("create database ");
            builder.AppendIdentifier(databaseName);
            if (null != dataFileName)
            {
                Debug.Assert(logFileName != null, "must specify log file with data file");
                builder.AppendSql(" on primary ");
                builder.AppendFileName(dataFileName);
                builder.AppendSql(" log on ");
                builder.AppendFileName(logFileName);
            }

            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EngineEdition")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "serverproperty")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "spexecutesql")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string SetDatabaseOptionsScript(SqlVersion sqlVersion, string databaseName)
        {
            if (sqlVersion < SqlVersion.Sql9)
            {
                return String.Empty;
            }

            var builder = new SqlDdlBuilder();

            // Set READ_COMMITTED_SNAPSHOT ON, if SQL Server 2005 and up, and not SQLAzure.
            builder.AppendSql("if serverproperty('EngineEdition') <> 5 execute sp_executesql ");
            builder.AppendStringLiteral(SetReadCommittedSnapshotScript(databaseName));

            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "readcommittedsnapshot")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private static string SetReadCommittedSnapshotScript(string databaseName)
        {
            var builder = new SqlDdlBuilder();

            builder.AppendSql("alter database ");
            builder.AppendIdentifier(databaseName);
            builder.AppendSql(" set read_committed_snapshot on");

            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string CreateDatabaseExistsScript(string databaseName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT Count(*) FROM ");
            AppendSysDatabases(builder, useDeprecatedSystemTable);
            builder.AppendSql(" WHERE [name]=");
            builder.AppendStringLiteral(databaseName);
            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sysdatabases")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private static void AppendSysDatabases(SqlDdlBuilder builder, bool useDeprecatedSystemTable)
        {
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("sysdatabases");
            }
            else
            {
                builder.AppendSql("sys.databases");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "physicalname")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "databaseid")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "masterfiles")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string CreateGetDatabaseNamesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT [d].[name] FROM ");
            AppendSysDatabases(builder, useDeprecatedSystemTable);
            builder.AppendSql(" AS [d] ");
            if (!useDeprecatedSystemTable)
            {
                builder.AppendSql("INNER JOIN sys.master_files AS [f] ON [f].[database_id] = [d].[database_id]");
            }
            builder.AppendSql(" WHERE [");
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("filename");
            }
            else
            {
                builder.AppendSql("f].[physical_name");
            }
            builder.AppendSql("]=");
            builder.AppendStringLiteral(databaseFileName);
            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "physicalname")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "masterfiles")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sysdatabases")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string CreateCountDatabasesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT Count(*) FROM ");

            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("sysdatabases");
            }
            if (!useDeprecatedSystemTable)
            {
                builder.AppendSql("sys.master_files");
            }
            builder.AppendSql(" WHERE [");
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("filename");
            }
            else
            {
                builder.AppendSql("physical_name");
            }
            builder.AppendSql("]=");
            builder.AppendStringLiteral(databaseFileName);
            return builder.unencodedStringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        internal static string DropDatabaseScript(string databaseName)
        {
            var builder = new SqlDdlBuilder();

            builder.AppendSql("drop database ");
            builder.AppendIdentifier(databaseName);

            return builder.unencodedStringBuilder.ToString();
        }

        internal string GetCommandText()
        {
            return unencodedStringBuilder.ToString();
        }

        internal static string GetSchemaName(EntitySet entitySet)
        {
            return entitySet.GetMetadataPropertyValue<string>("Schema") ?? entitySet.EntityContainer.Name;
        }

        internal static string GetTableName(EntitySet entitySet)
        {
            return entitySet.GetMetadataPropertyValue<string>("Table") ?? entitySet.Name;
        }

        #endregion

        #region Private Methods

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendCreateForeignKeys(AssociationSet associationSet)
        {
            var constraint = associationSet.ElementType.ReferentialConstraints.Single();
            var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];
            var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];

            // If any of the participating entity sets was skipped, skip the association too
            if (ignoredEntitySets.Contains(principalEnd.EntitySet)
                || ignoredEntitySets.Contains(dependentEnd.EntitySet))
            {
                AppendSql("-- Ignoring association set with participating entity set with defining query: ");
                AppendIdentifierEscapeNewLine(associationSet.Name);
            }
            else
            {
                AppendSql("alter table ");
                AppendIdentifier(dependentEnd.EntitySet);
                AppendSql(" add constraint ");
                AppendIdentifier(associationSet.Name);
                AppendSql(" foreign key (");
                AppendIdentifiers(constraint.ToProperties);
                AppendSql(") references ");
                AppendIdentifier(principalEnd.EntitySet);
                AppendSql("(");
                AppendIdentifiers(constraint.FromProperties);
                AppendSql(")");
                if (principalEnd.CorrespondingAssociationEndMember.DeleteBehavior
                    == OperationAction.Cascade)
                {
                    AppendSql(" on delete cascade");
                }
                AppendSql(";");
            }
            AppendNewLine();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendCreateTable(EntitySet entitySet)
        {
            // If the entity set has defining query, skip it
            if (entitySet.GetMetadataPropertyValue<string>("DefiningQuery") != null)
            {
                AppendSql("-- Ignoring entity set with defining query: ");
                AppendIdentifier(entitySet, AppendIdentifierEscapeNewLine);
                ignoredEntitySets.Add(entitySet);
            }
            else
            {
                AppendSql("create table ");
                AppendIdentifier(entitySet);
                AppendSql(" (");
                AppendNewLine();

                foreach (var column in entitySet.ElementType.Properties)
                {
                    AppendSql("    ");
                    AppendIdentifier(column.Name);
                    AppendSql(" ");
                    AppendType(column);
                    AppendSql(",");
                    AppendNewLine();
                }

                AppendSql("    primary key (");
                AppendJoin(entitySet.ElementType.KeyMembers, k => AppendIdentifier(k.Name), ", ");
                AppendSql(")");
                AppendNewLine();

                AppendSql(");");
            }
            AppendNewLine();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "schemaid")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendCreateSchema(string schema)
        {
            AppendSql("if (schema_id(");
            AppendStringLiteral(schema);
            AppendSql(") is null) exec(");

            // need to create a sub-command and escape it as a string literal as well...
            var schemaBuilder = new SqlDdlBuilder();
            schemaBuilder.AppendSql("create schema ");
            schemaBuilder.AppendIdentifier(schema);

            AppendStringLiteral(schemaBuilder.unencodedStringBuilder.ToString());
            AppendSql(");");
            AppendNewLine();
        }

        private void AppendIdentifier(EntitySet table)
        {
            AppendIdentifier(table, AppendIdentifier);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendIdentifier(EntitySet table, Action<string> AppendIdentifierEscape)
        {
            var schemaName = GetSchemaName(table);
            var tableName = GetTableName(table);
            if (schemaName != null)
            {
                AppendIdentifierEscape(schemaName);
                AppendSql(".");
            }
            AppendIdentifierEscape(tableName);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendStringLiteral(string literalValue)
        {
            AppendSql("N'" + literalValue.Replace("'", "''") + "'");
        }

        private void AppendIdentifiers(IEnumerable<EdmProperty> properties)
        {
            AppendJoin(properties, p => AppendIdentifier(p.Name), ", ");
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendIdentifier(string identifier)
        {
            AppendSql("[" + identifier.Replace("]", "]]") + "]");
        }

        private void AppendIdentifierEscapeNewLine(string identifier)
        {
            AppendIdentifier(identifier.Replace("\r", "\r--").Replace("\n", "\n--"));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendFileName(string path)
        {
            AppendSql("(name=");
            AppendStringLiteral(Path.GetFileName(path));
            AppendSql(", filename=");
            AppendStringLiteral(path);
            AppendSql(")");
        }

        private void AppendJoin<T>(IEnumerable<T> elements, Action<T> appendElement, string unencodedSeparator)
        {
            var first = true;
            foreach (var element in elements)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    AppendSql(unencodedSeparator);
                }
                appendElement(element);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "newid")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.SqlServer.SqlDdlBuilder.AppendSql(System.String)")]
        private void AppendType(EdmProperty column)
        {
            var type = column.TypeUsage;

            // check for rowversion-like configurations
            Facet storeGenFacet;
            var isTimestamp = false;
            if (type.EdmType.Name == "binary"
                &&
                8 == type.GetMaxLength()
                &&
                column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet)
                &&
                storeGenFacet.Value != null
                &&
                StoreGeneratedPattern.Computed == (StoreGeneratedPattern)storeGenFacet.Value)
            {
                isTimestamp = true;
                AppendIdentifier("rowversion");
            }
            else
            {
                var typeName = type.EdmType.Name;
                // Special case: the EDM treats 'nvarchar(max)' as a type name, but SQL Server treats
                // it as a type 'nvarchar' and a type qualifier. As such, we can't escape the entire
                // type name as the EDM sees it.
                const string maxSuffix = "(max)";
                if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType
                    && typeName.EndsWith(maxSuffix, StringComparison.Ordinal))
                {
                    Debug.Assert(
                        new[] { "nvarchar(max)", "varchar(max)", "varbinary(max)" }.Contains(typeName),
                        "no other known SQL Server primitive types types accept (max)");
                    AppendIdentifier(typeName.Substring(0, typeName.Length - maxSuffix.Length));
                    AppendSql("(max)");
                }
                else
                {
                    AppendIdentifier(typeName);
                }
                switch (type.EdmType.Name)
                {
                    case "decimal":
                    case "numeric":
                        AppendSqlInvariantFormat("({0}, {1})", type.GetPrecision(), type.GetScale());
                        break;
                    case "datetime2":
                    case "datetimeoffset":
                    case "time":
                        AppendSqlInvariantFormat("({0})", type.GetPrecision());
                        break;
                    case "binary":
                    case "varbinary":
                    case "nvarchar":
                    case "varchar":
                    case "char":
                    case "nchar":
                        AppendSqlInvariantFormat("({0})", type.GetMaxLength());
                        break;
                    default:
                        break;
                }
            }
            AppendSql(column.Nullable ? " null" : " not null");

            if (!isTimestamp
                && column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet)
                &&
                storeGenFacet.Value != null)
            {
                var storeGenPattern = (StoreGeneratedPattern)storeGenFacet.Value;
                if (storeGenPattern == StoreGeneratedPattern.Identity)
                {
                    if (type.EdmType.Name == "uniqueidentifier")
                    {
                        AppendSql(" default newid()");
                    }
                    else
                    {
                        AppendSql(" identity");
                    }
                }
            }
        }

        #region Access to underlying string builder

        // <summary>
        // Appends raw SQL into the string builder.
        // </summary>
        // <param name="text"> Raw SQL string to append into the string builder. </param>
        private void AppendSql(string text)
        {
            unencodedStringBuilder.Append(text);
        }

        // <summary>
        // Appends new line for visual formatting or for ending a comment.
        // </summary>
        private void AppendNewLine()
        {
            unencodedStringBuilder.Append("\r\n");
        }

        // <summary>
        // Append raw SQL into the string builder with formatting options and invariant culture formatting.
        // </summary>
        // <param name="format"> A composite format string. </param>
        // <param name="args"> An array of objects to format. </param>
        private void AppendSqlInvariantFormat(string format, params object[] args)
        {
            unencodedStringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }

        #endregion

        #endregion
    }
}
