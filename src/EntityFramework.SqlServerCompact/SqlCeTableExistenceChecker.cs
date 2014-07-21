// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Text;

    internal class SqlCeTableExistenceChecker : TableExistenceChecker
    {
        public override bool AnyModelTableExistsInDatabase(
            ObjectContext context, DbConnection connection, IEnumerable<EntitySet> modelTables, string edmMetadataContextTableName)
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

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT Count(*)
FROM INFORMATION_SCHEMA.TABLES AS t
WHERE t.TABLE_NAME IN (" + modelTablesListBuilder + @")";

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
