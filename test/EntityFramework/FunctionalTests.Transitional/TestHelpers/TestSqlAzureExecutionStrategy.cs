using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;

    class TestSqlAzureExecutionStrategy : SqlAzureExecutionStrategy
    {
        protected override bool ShouldRetryOn(Exception ex)
        {
            if (FunctionalTestsConfiguration.SuspendExecutionStrategy)
            {
                return false;
            }

            var sqlException = ex as SqlException;
            if (sqlException != null)
            {
                // Enumerate through all errors found in the exception.
                foreach (SqlError err in sqlException.Errors)
                {
                    switch (err.Number)
                    {
                            // This case is the only difference between the testing strategy and production strategy 
                            // DBNETLIB Error Code: -2
                            // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated. 
                        case -2:
                            return true;
                    }
                }
            }

            return base.ShouldRetryOn(ex);
        }

        public new bool RetriesOnFailure 
        {
            get { return !FunctionalTestsConfiguration.SuspendExecutionStrategy; }
        }
    }
}
