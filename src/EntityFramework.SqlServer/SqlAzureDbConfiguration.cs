// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    ///     A class derived from this class can be placed in the same assembly as a class derived from
    ///     <see cref="DbContext" /> to enable the SQL Azure retry policy for it.
    /// </summary>
    public class SqlAzureDbConfiguration : DbConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="SqlAzureDbConfiguration"/> and sets
        ///     <see cref="SqlAzureExecutionStrategy"/> as the default execution strategy.
        /// </summary>
        public SqlAzureDbConfiguration()
        {
            AddExecutionStrategy(() => new SqlAzureExecutionStrategy(), null);
        }

        /// <summary>
        ///     Adds an <see cref="IExecutionStrategy"/> factory method for a particular server name.
        ///     The order in which the factories are added is important. The last one takes precedence.
        /// </summary>
        /// <param name="getExecutionStrategy">A function that returns a new instance of an execution strategy.</param>
        /// <param name="serverName">A string that will be matched against the server name in the connection string. <c>null</c> will match anything.</param>
        protected void AddExecutionStrategy(Func<IExecutionStrategy> getExecutionStrategy, string serverName)
        {
            AddDependencyResolver(new SqlExecutionStrategyResolver(getExecutionStrategy, serverName));
        }
    }
}
