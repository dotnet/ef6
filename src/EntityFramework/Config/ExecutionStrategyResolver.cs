// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     An <see cref="IDbDependencyResolver" /> implementation used for resolving <see cref="IExecutionStrategy" />
    ///     factories.
    /// </summary>
    /// <remarks>
    ///     This class can be used by <see cref="DbProviderServices" /> to aid in the resoloving
    ///     of <see cref="IExecutionStrategy" /> factories as a secondary service.
    /// </remarks>
    /// <typeparam name="T">The type of execution strategy that is resolved.</typeparam>
    public class ExecutionStrategyResolver<T> : IDbDependencyResolver
        where T : IExecutionStrategy
    {
        private readonly Func<T> _getExecutionStrategy;
        private readonly string _providerInvariantName;
        private readonly string _serverName;

        /// <summary>
        ///     Initializes a new instance of <see cref="ExecutionStrategyResolver{T}" />
        /// </summary>
        /// <param name="providerInvariantName"> The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this execution strategy will be used. </param>
        /// <param name="serverName">
        ///     A string that will be matched against the server name in the connection string. <c>null</c> will match anything.
        /// </param>
        /// <param name="getExecutionStrategy">A function that returns a new instance of an execution strategy.</param>
        public ExecutionStrategyResolver(string providerInvariantName, string serverName, Func<T> getExecutionStrategy)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(getExecutionStrategy, "getExecutionStrategy");

            _providerInvariantName = providerInvariantName;
            _serverName = serverName;
            _getExecutionStrategy = getExecutionStrategy;
        }

        /// <summary>
        ///     If the given type is <see cref="Func{IExecutionStrategy}" />, then this resolver will attempt
        ///     to return the service to use, otherwise it will return null. When the given type is
        ///     Func{IExecutionStrategy}, then the key is expected to be an <see cref="ExecutionStrategyKey" />.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        /// <param name="key">A key used to make a determination of the service to return.</param>
        /// <returns>
        ///     An <see cref="Func{IExecutionStrategy}" />, or null.
        /// </returns>
        public object GetService(Type type, object key)
        {
            if (type == typeof(Func<IExecutionStrategy>))
            {
                var executionStrategyKey = key as ExecutionStrategyKey;
                if (executionStrategyKey == null)
                {
                    throw new ArgumentException(
                        Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"));
                }

                if (!executionStrategyKey.ProviderInvariantName.Equals(_providerInvariantName, StringComparison.Ordinal))
                {
                    return null;
                }

                if (_serverName != null
                    && !executionStrategyKey.ServerName.Equals(_serverName, StringComparison.Ordinal))
                {
                    return null;
                }

                return _getExecutionStrategy;
            }

            return null;
        }
    }
}
