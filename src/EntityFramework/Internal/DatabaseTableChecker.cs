// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Transactions;

    internal class DatabaseTableChecker
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool AnyModelTableExists(InternalContext internalContext)
        {
            using (var clonedObjectContext = internalContext.CreateObjectContextForDdlOps())
            {
                var exists = internalContext.DatabaseOperations.Exists(clonedObjectContext.ObjectContext);

                if (!exists)
                {
                    return false;
                }

                try
                {
                    if (internalContext.CodeFirstModel == null)
                    {
                        return true;
                    }

                    var providerName = internalContext.ProviderName;
                    IPseudoProvider provider;

                    switch (providerName)
                    {
                        case "System.Data.SqlClient":
                            provider = new SqlPseudoProvider();
                            break;

                        case "System.Data.SqlServerCe.4.0":
                            provider = new SqlCePseudoProvider();
                            break;

                        default:
                            return true;
                    }

                    var modelTables = GetModelTables(internalContext.ObjectContext.MetadataWorkspace).ToList();

                    if (!modelTables.Any())
                    {
                        return true;
                    }

                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        if (provider.AnyModelTableExistsInDatabase(
                            clonedObjectContext.ObjectContext, clonedObjectContext.Connection, modelTables,
                            EdmMetadataContext.TableName))
                        {
                            return true;
                        }
                    }

                    return internalContext.HasHistoryTableEntry();
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message, ex.ToString());

                    // Revert to previous behavior on error
                    return true;
                }
            }
        }

        private static IEnumerable<EntitySet> GetModelTables(MetadataWorkspace workspace)
        {
            return workspace
                .GetItemCollection(DataSpace.SSpace)
                .GetItems<EntityContainer>()
                .Single()
                .BaseEntitySets
                .OfType<EntitySet>()
                .Where(
                    s => !s.MetadataProperties.Contains("Type")
                         || (string)s.MetadataProperties["Type"].Value == "Tables");
        }

        private static string GetTableName(EntitySet modelTable)
        {
            return modelTable.MetadataProperties.Contains("Table")
                   && modelTable.MetadataProperties["Table"].Value != null
                       ? (string)modelTable.MetadataProperties["Table"].Value
                       : modelTable.Name;
        }

        private interface IPseudoProvider
        {
            bool AnyModelTableExistsInDatabase(
                ObjectContext context, DbConnection connection, List<EntitySet> modelTables, string edmMetadataContextTableName);
        }

        private class SqlPseudoProvider : IPseudoProvider
        {
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
            public bool AnyModelTableExistsInDatabase(
                ObjectContext context, DbConnection connection, List<EntitySet> modelTables, string edmMetadataContextTableName)
            {
                var modelTablesListBuilder = new StringBuilder();
                foreach (var modelTable in modelTables)
                {
                    modelTablesListBuilder.Append("'");
                    modelTablesListBuilder.Append((string)modelTable.MetadataProperties["Schema"].Value);
                    modelTablesListBuilder.Append(".");
                    modelTablesListBuilder.Append(GetTableName(modelTable));
                    modelTablesListBuilder.Append("',");
                }
                modelTablesListBuilder.Remove(modelTablesListBuilder.Length - 1, 1);

                using (var command = new InterceptableDbCommand(
                    connection.CreateCommand(), context.InterceptionContext))
                {
                    command.CommandText = @"
SELECT Count(*)
FROM INFORMATION_SCHEMA.TABLES AS t
WHERE t.TABLE_TYPE = 'BASE TABLE'
    AND (t.TABLE_SCHEMA + '.' + t.TABLE_NAME IN (" + modelTablesListBuilder + @")
        OR t.TABLE_NAME = '" + edmMetadataContextTableName + "')";

                    var executionStrategy = DbProviderServices.GetExecutionStrategy(connection);
                    try
                    {
                        return executionStrategy.Execute(
                            () =>
                                {
                                    if (connection.State == ConnectionState.Broken)
                                    {
                                        connection.Close();
                                    }

                                    if (connection.State == ConnectionState.Closed)
                                    {
                                        connection.Open();
                                    }

                                    return (int)command.ExecuteScalar() > 0;
                                });
                    }
                    finally
                    {
                        if (connection.State != ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        private class SqlCePseudoProvider : IPseudoProvider
        {
            public bool AnyModelTableExistsInDatabase(
                ObjectContext context, DbConnection connection, List<EntitySet> modelTables, string edmMetadataContextTableName)
            {
                var modelTablesListBuilder = new StringBuilder();
                foreach (var modelTable in modelTables)
                {
                    modelTablesListBuilder.Append("'");
                    modelTablesListBuilder.Append(GetTableName(modelTable));
                    modelTablesListBuilder.Append("',");
                }

                modelTablesListBuilder.Append("'");
                modelTablesListBuilder.Append("edmMetadataContextTableName");
                modelTablesListBuilder.Append("'");

                using (var command = new InterceptableDbCommand(
                    connection.CreateCommand(), context.InterceptionContext))
                {
                    command.CommandText = @"
SELECT Count(*)
FROM INFORMATION_SCHEMA.TABLES AS t
WHERE t.TABLE_TYPE = 'TABLE'
    AND t.TABLE_NAME IN (" + modelTablesListBuilder + @")";

                    var executionStrategy = DbProviderServices.GetExecutionStrategy(connection);
                    try
                    {
                        return executionStrategy.Execute(
                            () =>
                                {
                                    if (connection.State == ConnectionState.Broken)
                                    {
                                        connection.Close();
                                    }

                                    if (connection.State == ConnectionState.Closed)
                                    {
                                        connection.Open();
                                    }

                                    return (int)command.ExecuteScalar() > 0;
                                });
                    }
                    finally
                    {
                        if (connection.State != ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        private class IgnoreSchemaComparer : IEqualityComparer<Tuple<string, string>>
        {
            public bool Equals(Tuple<string, string> x, Tuple<string, string> y)
            {
                return EqualityComparer<string>.Default.Equals(x.Item2, y.Item2);
            }

            public int GetHashCode(Tuple<string, string> obj)
            {
                return EqualityComparer<string>.Default.GetHashCode(obj.Item2);
            }
        }
    }
}
