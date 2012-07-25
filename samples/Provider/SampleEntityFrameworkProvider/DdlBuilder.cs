// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SampleEntityFrameworkProvider
{
    sealed class DdlBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly HashSet<EntitySet> ignoredEntitySets = new HashSet<EntitySet>();
                
        internal static string CreateObjectsScript(StoreItemCollection itemCollection)
        {
            DdlBuilder builder = new DdlBuilder();

            foreach (EntityContainer container in itemCollection.GetItems<EntityContainer>())
            {
                var entitySets = container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name);

                var schemas = new HashSet<string>(entitySets.Select(s => GetSchemaName(s)));
                foreach (string schema in schemas.OrderBy(s => s))
                {
                    // don't bother creating default schema
                    if (schema != "dbo")
                    {
                        builder.AppendCreateSchema(schema);
                    }
                }
                
                foreach (EntitySet entitySet in container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateTable(entitySet);
                }

                foreach (AssociationSet associationSet in container.BaseEntitySets.OfType<AssociationSet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateForeignKeys(associationSet);
                }
            }
            return builder.GetCommandText();
        }

        internal static string CreateDatabaseScript(string databaseName, string dataFileName, string logFileName)
        {
            var builder = new DdlBuilder();
            builder.AppendSql("create database ");
            builder.AppendIdentifier(databaseName);
            if (null != dataFileName)
            {                
                builder.AppendSql(" on primary ");
                builder.AppendFileName(dataFileName);
                builder.AppendSql(" log on ");
                builder.AppendFileName(logFileName);
            }
            return builder.stringBuilder.ToString();
        }

        internal static string CreateDatabaseExistsScript(string databaseName)
        {
            var builder = new DdlBuilder();
            builder.AppendSql("SELECT Count(*) FROM ");            
            builder.AppendSql("sys.databases");
            builder.AppendSql(" WHERE [name]=");
            builder.AppendStringLiteral(databaseName);
            return builder.stringBuilder.ToString();
        }

        internal static string DropDatabaseScript(string databaseName)
        {
            var builder = new DdlBuilder();
            builder.AppendSql("drop database ");
            builder.AppendIdentifier(databaseName);
            return builder.stringBuilder.ToString();
        }

        internal string GetCommandText()
        {
            return this.stringBuilder.ToString();
        }

        private static string GetSchemaName(EntitySet entitySet)
        {
            var schemaName = entitySet.MetadataProperties["Schema"].Value as string;
            return schemaName ?? entitySet.EntityContainer.Name;             
        }

        private static string GetTableName(EntitySet entitySet)
        {
            string tableName = entitySet.MetadataProperties["Table"].Value as string;
            return tableName ?? entitySet.Name;            
        }

        private void AppendCreateForeignKeys(AssociationSet associationSet)
        {
            var constraint = associationSet.ElementType.ReferentialConstraints.Single();
            var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];
            var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];

            // If any of the participating entity sets was skipped, skip the association too
            if (ignoredEntitySets.Contains(principalEnd.EntitySet) || ignoredEntitySets.Contains(dependentEnd.EntitySet))
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
                if (principalEnd.CorrespondingAssociationEndMember.DeleteBehavior == OperationAction.Cascade)
                {
                    AppendSql(" on delete cascade");
                }
                AppendSql(";");
            }
            AppendNewLine();
        }

        private void AppendCreateTable(EntitySet entitySet)
        {
            //If the entity set has defining query, skip it
            if (entitySet.MetadataProperties["DefiningQuery"].Value != null)
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

                foreach (EdmProperty column in entitySet.ElementType.Properties)
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

        private void AppendCreateSchema(string schema)
        {
            AppendSql("if (schema_id(");
            AppendStringLiteral(schema);
            AppendSql(") is null) exec(");

            // need to create a sub-command and escape it as a string literal as well...
            DdlBuilder schemaBuilder = new DdlBuilder();
            schemaBuilder.AppendSql("create schema ");
            schemaBuilder.AppendIdentifier(schema);

            AppendStringLiteral(schemaBuilder.stringBuilder.ToString());
            AppendSql(");");
            AppendNewLine();
        }

        private void AppendIdentifier(EntitySet table)
        {
            AppendIdentifier(table, AppendIdentifier);
        }

        private void AppendIdentifier(EntitySet table, Action<string> AppendIdentifierEscape)
        {
            string schemaName = GetSchemaName(table);
            string tableName = GetTableName(table);
            if (schemaName != null)
            {
                AppendIdentifierEscape(schemaName);
                AppendSql(".");
            }
            AppendIdentifierEscape(tableName);
        }

        private void AppendStringLiteral(string literalValue)
        {
            AppendSql("N'" + literalValue.Replace("'", "''") + "'");
        }

        private void AppendIdentifiers(IEnumerable<EdmProperty> properties)
        {
            AppendJoin(properties, p => AppendIdentifier(p.Name), ", ");
        }

        private void AppendIdentifier(string identifier)
        {
            AppendSql("[" + identifier.Replace("]", "]]") + "]");
        }

        private void AppendIdentifierEscapeNewLine(string identifier)
        {
            AppendIdentifier(identifier.Replace("\r", "\r--").Replace("\n", "\n--"));
        }

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
            bool first = true;
            foreach (T element in elements)
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

        private void AppendType(EdmProperty column)
        {
            TypeUsage type = column.TypeUsage;

            // check for rowversion-like configurations
            Facet storeGenFacet;
            bool isTimestamp = false;
            if (type.EdmType.Name == "binary" &&
                8 == type.GetMaxLength() &&
                column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                storeGenFacet.Value != null &&
                StoreGeneratedPattern.Computed == (StoreGeneratedPattern)storeGenFacet.Value)
            {
                isTimestamp = true;
                AppendIdentifier("rowversion");
            }
            else
            {
                string typeName = type.EdmType.Name;
                // Special case: the EDM treats 'nvarchar(max)' as a type name, but SQL Server treats
                // it as a type 'nvarchar' and a type qualifier. As such, we can't escape the entire
                // type name as the EDM sees it.
                const string maxSuffix = "(max)";
                if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && typeName.EndsWith(maxSuffix, StringComparison.Ordinal))
                {
                    //Debug.Assert(new[] { "nvarchar(max)", "varchar(max)", "varbinary(max)" }.Contains(typeName),
                    //    "no other known SQL Server primitive types types accept (max)");
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

            if (!isTimestamp && column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                storeGenFacet.Value != null)
            {
                StoreGeneratedPattern storeGenPattern = (StoreGeneratedPattern)storeGenFacet.Value;
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

        /// <summary>
        /// Appends raw SQL into the string builder.
        /// </summary>
        /// <param name="text">Raw SQL string to append into the string builder.</param>
        private void AppendSql(string text)
        {
            stringBuilder.Append(text);
        }

        /// <summary>
        /// Appends new line for visual formatting or for ending a comment.
        /// </summary>
        private void AppendNewLine()
        {
            stringBuilder.Append("\r\n");
        }

        /// <summary>
        /// Append raw SQL into the string builder with formatting options and invariant culture formatting.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        private void AppendSqlInvariantFormat(string format, params object[] args)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }        
    }

    internal static class MetadataHelper
    {
        internal static byte GetPrecision(this TypeUsage type)
        {
            return type.GetFacetValue<byte>("Precision");
        }

        internal static byte GetScale(this TypeUsage type)
        {
            return type.GetFacetValue<byte>("Scale");
        }

        internal static int GetMaxLength(this TypeUsage type)
        {
            return type.GetFacetValue<int>("MaxLength");
        }

        internal static T GetFacetValue<T>(this TypeUsage type, string facetName)
        {
            return (T)type.Facets[facetName].Value;
        }
    }

}
