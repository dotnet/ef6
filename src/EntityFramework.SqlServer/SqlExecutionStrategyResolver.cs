// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer.Utilities;

    public class SqlExecutionStrategyResolver : IDbDependencyResolver
    {
        private readonly Func<IExecutionStrategy> _getExecutionStrategy;
        private readonly string _serverName;

        /// <summary>
        ///     Initializes a new instance of <see cref="SqlExecutionStrategyResolver"/>
        /// </summary>
        /// <param name="getExecutionStrategy">A function that returns a new instance of an execution strategy.</param>
        /// <param name="serverName">A string that will be matched against the server name in the connection string. <c>null</c> will match anything.</param>
        public SqlExecutionStrategyResolver(Func<IExecutionStrategy> getExecutionStrategy, string serverName)
        {
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _getExecutionStrategy = getExecutionStrategy;
            _serverName = serverName;
        }

        public object GetService(Type type, object key)
        {
            if (type == typeof(IExecutionStrategy))
            {
                var executionStrategyKey = key as ExecutionStrategyKey;
                if (executionStrategyKey == null)
                {
                    return null;
                }

                if (!executionStrategyKey.InvariantProviderName.Equals("System.Data.SqlClient", StringComparison.Ordinal))
                {
                    return null;
                }

                if (_serverName != null
                    &&
                    !executionStrategyKey.DataSourceName.Equals(_serverName, StringComparison.Ordinal))
                {
                    return null;
                }

                return _getExecutionStrategy();
            }

            return null;
        }
    }
}
