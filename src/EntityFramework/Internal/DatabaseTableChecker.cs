// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Transactions;

    internal class DatabaseTableChecker
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DatabaseExistenceState AnyModelTableExists(InternalContext internalContext)
        {
            using (var clonedObjectContext = internalContext.CreateObjectContextForDdlOps())
            {
                var exists = internalContext.DatabaseOperations.Exists(clonedObjectContext.ObjectContext);

                if (!exists)
                {
                    return DatabaseExistenceState.DoesNotExist;
                }

                try
                {
                    if (internalContext.CodeFirstModel == null)
                    {
                        // If not Code First, then assume tables created in some other way
                        return DatabaseExistenceState.Exists;
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
                            // If we can't check for tables, then assume they exist as we did in older versions
                            return DatabaseExistenceState.Exists;
                    }

                    var modelTables = GetModelTables(internalContext).ToList();

                    if (!modelTables.Any())
                    {
                        // If this is an empty model, then all tables that can exist (0) do exist
                        return DatabaseExistenceState.Exists;
                    }

                    if (QueryForTableExistence(provider, clonedObjectContext, modelTables))
                    {
                        // If any table exists, then assume that this is a non-empty database
                        return DatabaseExistenceState.Exists;
                    }

                    // At this point we know no model tables exist. If the history table exists and has an entry
                    // for this context, then treat this as a non-empty database, otherwise treat is as existing
                    // but empty.
                    return internalContext.HasHistoryTableEntry()
                               ? DatabaseExistenceState.Exists
                               : DatabaseExistenceState.ExistsConsideredEmpty;
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message, ex.ToString());

                    // Revert to previous behavior on error
                    return DatabaseExistenceState.Exists;
                }
            }
        }

        public virtual bool QueryForTableExistence(
            IPseudoProvider provider, ClonedObjectContext clonedObjectContext, List<EntitySet> modelTables)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                if (provider.AnyModelTableExistsInDatabase(
                    clonedObjectContext.ObjectContext,
                    clonedObjectContext.Connection,
                    modelTables,
                    EdmMetadataContext.TableName))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual IEnumerable<EntitySet> GetModelTables(InternalContext internalContext)
        {
            return internalContext.ObjectContext.MetadataWorkspace
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

        internal interface IPseudoProvider
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

                var dbCommand = connection.CreateCommand();
                using (var command = new InterceptableDbCommand(dbCommand, context.InterceptionContext))
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
                                    if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Broken)
                                    {
                                        DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
                                    }

                                    if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Closed)
                                    {
                                        DbInterception.Dispatch.Connection.Open(connection, context.InterceptionContext);
                                    }

                                    return (int)command.ExecuteScalar() > 0;
                                });
                    }
                    finally
                    {
                        if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) != ConnectionState.Closed)
                        {
                            DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
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

                var dbCommand = connection.CreateCommand();
                using (var command = new InterceptableDbCommand(dbCommand, context.InterceptionContext))
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
                                    if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Broken)
                                    {
                                        DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
                                    }

                                    if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Closed)
                                    {
                                        DbInterception.Dispatch.Connection.Open(connection, context.InterceptionContext);
                                    }

                                    return (int)command.ExecuteScalar() > 0;
                                });
                    }
                    finally
                    {
                        if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) != ConnectionState.Closed)
                        {
                            DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
                        }
                    }
                }
            }
        }
    }
}
