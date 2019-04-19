// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;

    public class ExtendedSqlAzureExecutionStrategy : SqlAzureExecutionStrategy
    {
        public ExtendedSqlAzureExecutionStrategy()
            : base(maxRetryCount: 6, maxDelay: TimeSpan.FromMinutes(1))
        {
        }

        public static void ExecuteNew(Action action)
        {
            if (DatabaseTestHelpers.IsSqlAzure(ModelHelpers.BaseConnectionString))
            {
                var executionStrategy = new ExtendedSqlAzureExecutionStrategy();
                executionStrategy.Execute(action);
            }
            else
            {
                action();
            }
        }

        public static T ExecuteNew<T>(Func<T> func)
        {
            if (DatabaseTestHelpers.IsSqlAzure(ModelHelpers.BaseConnectionString))
            {
                var executionStrategy = new ExtendedSqlAzureExecutionStrategy();
                return executionStrategy.Execute(func);
            }
            else
            {
                return func();
            }
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            var sqlException = exception as SqlException;
            if (sqlException != null)
            {
                // Enumerate through all errors found in the exception.
                foreach (SqlError err in sqlException.Errors)
                {
                    switch (err.Number)
                    {
                            // This exception can be thrown even if the operation completed succesfully, so it's safer to let the application fail.
                            // DBNETLIB Error Code: -2
                            // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated. 
                        case -2:
                            return true;
                    }
                }
            }
            return base.ShouldRetryOn(exception);
        }
    }
}
