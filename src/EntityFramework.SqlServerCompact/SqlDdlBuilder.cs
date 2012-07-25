// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServerCompact
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Class for generating scripts to create schema objects.
    /// </summary>
    internal sealed class SqlDdlBuilder
    {
        // List of command strings for Creating tables.
        private readonly List<string> objects = new List<string>();

        // list of command strings for constraints.
        // Not this has been kep separate from objectBuilder because in the script constraints should always appear in the end.
        private readonly List<string> constraints = new List<string>();

        // List of command strings for ignored objects.
        // Note that, we shouldn't execute these and should be returned only as warnings.
        private readonly List<string> ignoredObjects = new List<string>();

        // List of ignored entities.
        private readonly List<EntitySet> ignoredEntitySets = new List<EntitySet>();

        /// <summary>
        /// Helper function for generating the scripts for tables & constraints.
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <returns></returns> 
        internal static List<string> CreateObjectsScript(StoreItemCollection itemCollection, bool returnWarnings)
        {
            var builder = new SqlDdlBuilder();

            // Iterate over the container.
            foreach (var container in itemCollection.GetItems<EntityContainer>())
            {
                // Generate create table statements.
                foreach (var set in container.BaseEntitySets)
                {
                    // If it is a type of entitySet, generate Create Table statements.
                    var entitySet = set as EntitySet;
                    if (entitySet != null)
                    {
                        builder.AppendCreateTable(entitySet);
                    }
                }

                // Generate Foreign Key constraints.
                foreach (var set in container.BaseEntitySets)
                {
                    // If it is association set, generate Foreign Key constraints.
                    var associationSet = set as AssociationSet;
                    if (associationSet != null)
                    {
                        builder.AppendCreateForeignKeys(associationSet);
                    }
                }
            }

            // Return the final command text.
            return builder.GetCommandText(returnWarnings);
        }

        /// <summary>
        /// Function that returns final command text by appending Constraints to ObjectBuilder.
        /// </summary>
        /// <returns></returns> 
        internal List<string> GetCommandText(bool returnWarnings)
        {
            objects.AddRange(constraints);
            if (returnWarnings)
            {
                objects.AddRange(ignoredObjects);
            }
            return objects;
        }

        private static string GetTableName(EntitySet entitySet)
        {
            return ((string)(entitySet.MetadataProperties["Table"].Value) ?? entitySet.Name);
        }

        /// <summary>
        /// Function for generating foreign key constraints.
        /// </summary>
        /// <param name="associationSet"></param>
        private void AppendCreateForeignKeys(AssociationSet associationSet)
        {
            var constraintBuilder = new StringBuilder();
            Debug.Assert(associationSet.ElementType.ReferentialConstraints.Count == 1);
            // Get the constraint.
            var constraint = associationSet.ElementType.ReferentialConstraints[0];

            // Principal (referenced) end of the constraint.
            var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];

            // Dependent (referencing) end of constraint.
            var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];

            //If any of the participating entity sets was skipped, skip the association too
            if (ignoredEntitySets.Contains(principalEnd.EntitySet)
                || ignoredEntitySets.Contains(dependentEnd.EntitySet))
            {
                constraintBuilder.Append("-- Ignoring association set with participating entity set with defining query: ");
                AppendIdentifierEscapeNewLine(constraintBuilder, associationSet.Name);
                constraintBuilder.AppendLine();
                ignoredObjects.Add(constraintBuilder.ToString());
            }
            else
            {
                constraintBuilder.Append("ALTER TABLE ");
                AppendIdentifier(constraintBuilder, GetTableName(dependentEnd.EntitySet));
                constraintBuilder.Append(" ADD CONSTRAINT ");
                AppendIdentifier(constraintBuilder, associationSet.Name);
                constraintBuilder.Append(" FOREIGN KEY (");
                AppendIdentifiers(constraintBuilder, constraint.ToProperties); // List of referencing columns.
                constraintBuilder.Append(") REFERENCES ");
                AppendIdentifier(constraintBuilder, GetTableName(principalEnd.EntitySet));
                constraintBuilder.Append("(");
                AppendIdentifiers(constraintBuilder, constraint.FromProperties); // List of referenced columns.
                constraintBuilder.Append(")");

                // Append cascade action if it exists.
                if (principalEnd.CorrespondingAssociationEndMember.DeleteBehavior
                    == OperationAction.Cascade)
                {
                    constraintBuilder.Append(" ON DELETE CASCADE");
                }
                constraintBuilder.Append(";");
                constraintBuilder.AppendLine();
                constraints.Add(constraintBuilder.ToString());
            }
        }

        /// <summary>
        /// Function for generating create table statements.
        /// </summary>
        /// <param name="entitySet"></param>
        private void AppendCreateTable(EntitySet entitySet)
        {
            var objectBuilder = new StringBuilder();
            // Ignore the set if it is a defining query.
            if (entitySet.MetadataProperties["DefiningQuery"].Value != null)
            {
                ignoredEntitySets.Add(entitySet);
                objectBuilder.Append(" -- Ignoring entity set with defining query: "); // Adding a comment.
                AppendIdentifierEscapeNewLine(objectBuilder, GetTableName(entitySet));
                objectBuilder.AppendLine();
                ignoredObjects.Add(objectBuilder.ToString());
            }
            else
            {
                objectBuilder.Append("CREATE TABLE ");
                AppendIdentifier(objectBuilder, GetTableName(entitySet));
                objectBuilder.Append(" (");
                objectBuilder.AppendLine();

                // Column information.
                foreach (var column in entitySet.ElementType.Properties)
                {
                    objectBuilder.Append("    ");
                    AppendIdentifier(objectBuilder, column.Name);
                    objectBuilder.Append(" ");
                    AppendType(objectBuilder, column);
                    objectBuilder.Append(",");
                    objectBuilder.AppendLine();
                }

                if (entitySet.ElementType.KeyMembers.Count > 0)
                {
                    // Primary key information.
                    objectBuilder.Append("    PRIMARY KEY (");
                    var first = true;
                    foreach (var keyMember in entitySet.ElementType.KeyMembers)
                    {
                        // VSTS Bug ID: 845968
                        // Throw an exception if key member is of server generated guid type.
                        // This is because DML operations won't succeed if key column is of server generated with GUID type.
                        // TODO: Remove this once the DML is fixed to retrieve back server generated GUID column values.
                        if (IsServerGeneratedGuid(keyMember))
                        {
                            throw ADP1.ServerGeneratedGuidKeyNotSupportedException(keyMember.Name);
                        }
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            objectBuilder.Append(",");
                        }
                        AppendIdentifier(objectBuilder, keyMember.Name);
                    }

                    objectBuilder.Append(")");
                    objectBuilder.AppendLine();
                }

                objectBuilder.Append(");");
                objectBuilder.AppendLine();
                objects.Add(objectBuilder.ToString());
            }
        }

        // Helper function for appending list of EdmProperties separated using ','.
        private static void AppendIdentifiers(StringBuilder builder, IEnumerable<EdmProperty> properties)
        {
            var first = true;
            foreach (var property in properties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(",");
                }
                AppendIdentifier(builder, property.Name);
            }
        }

        private static void AppendIdentifierEscapeNewLine(StringBuilder builder, string identifier)
        {
            AppendIdentifier(builder, identifier.Replace("\r", "\r--").Replace("\n", "\n--"));
        }

        private static void AppendType(StringBuilder builder, EdmProperty column)
        {
            var type = column.TypeUsage;

            // check for rowversion-like configurations
            Facet storeGenFacet;
            var isTimestamp = false;
            if (type.EdmType.Name == "binary" &&
                8 == (int)type.Facets["MaxLength"].Value &&
                column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                storeGenFacet.Value != null
                &&
                StoreGeneratedPattern.Computed == (StoreGeneratedPattern)storeGenFacet.Value)
            {
                isTimestamp = true;
                builder.Append("rowversion");
            }
            else
            {
                var typeName = type.EdmType.Name;

                builder.Append(typeName);
                switch (type.EdmType.Name)
                {
                    case "decimal":
                    case "numeric":
                        AppendSqlInvariantFormat(builder, "({0}, {1})", type.Facets["Precision"].Value, type.Facets["Scale"].Value);
                        break;
                    case "binary":
                    case "varbinary":
                    case "nvarchar":
                    case "varchar":
                    case "char":
                    case "nchar":
                        AppendSqlInvariantFormat(builder, "({0})", type.Facets["MaxLength"].Value);
                        break;
                    default:
                        break;
                }
            }
            builder.Append(column.Nullable ? " null" : " not null");

            if (!isTimestamp && column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet)
                &&
                storeGenFacet.Value != null)
            {
                var storeGenPattern = (StoreGeneratedPattern)storeGenFacet.Value;
                if (storeGenPattern == StoreGeneratedPattern.Identity)
                {
                    if (type.EdmType.Name == "uniqueidentifier")
                    {
                        builder.Append(" default newid()");
                    }
                    else
                    {
                        builder.Append(" identity");
                    }
                }
                else if (storeGenPattern == StoreGeneratedPattern.Computed)
                {
                    if (type.EdmType.Name != "timestamp"
                        && type.EdmType.Name != "rowversion")
                    {
                        // if "IsComputed" is applied to store types that are not intrinsically store generated, throw
                        //throw EntityUtil.NotSupported(Strings.SqlProvider_DdlGeneration_StoreGeneratedPatternNotSupported(Enum.GetName(typeof(StoreGeneratedPattern), storeGenPattern)));
                        throw ADP1.ComputedColumnsNotSupportedException();
                    }
                }
            }
        }

        // Function to check whether the given column is of server generated guid type.
        private static bool IsServerGeneratedGuid(EdmMember column)
        {
            Facet storeGenFacet;
            return (column.TypeUsage.EdmType.Name == "uniqueidentifier" &&
                    column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                    storeGenFacet.Value != null &&
                    (StoreGeneratedPattern)storeGenFacet.Value == StoreGeneratedPattern.Identity);
        }

        private static void AppendIdentifier(StringBuilder builder, string name)
        {
            builder.Append("\"" + name.Replace("\"", "\"\"") + "\"");
        }

        /// <summary>
        /// Append raw SQL into the string builder with formatting options and invariant culture formatting.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        private static void AppendSqlInvariantFormat(StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }
    }
}
