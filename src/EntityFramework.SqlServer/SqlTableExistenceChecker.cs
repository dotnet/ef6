// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    internal class SqlTableExistenceChecker : TableExistenceChecker
    {
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override bool AnyModelTableExistsInDatabase(
            ObjectContext context, DbConnection connection, IEnumerable<EntitySet> modelTables, string edmMetadataContextTableName)
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

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT Count(*)
FROM INFORMATION_SCHEMA.TABLES AS t
WHERE t.TABLE_SCHEMA + '.' + t.TABLE_NAME IN (" + modelTablesListBuilder + @")
    OR t.TABLE_NAME = '" + edmMetadataContextTableName + "'";

                var shouldClose = true;

                if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Open)
                {
                    shouldClose = false;

                    var entityTransaction = ((EntityConnection)context.Connection).CurrentTransaction;
                    if (entityTransaction != null)
                    {
                        command.Transaction = entityTransaction.StoreTransaction;
                    }
                }

                var executionStrategy = DbProviderServices.GetExecutionStrategy(connection);
                try
                {
                    return executionStrategy.Execute(
                        () =>
                        {
                            if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext)
                                == ConnectionState.Broken)
                            {
                                DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
                            }

                            if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext)
                                == ConnectionState.Closed)
                            {
                                DbInterception.Dispatch.Connection.Open(connection, context.InterceptionContext);
                            }

                            return (int)DbInterception.Dispatch.Command.Scalar(
                                command, new DbCommandInterceptionContext(context.InterceptionContext)) > 0;
                        });
                }
                finally
                {
                    if (shouldClose
                        && DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) != ConnectionState.Closed)
                    {
                        DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
                    }
                }
            }
        }
    }
}
