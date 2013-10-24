// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// An <see cref="IDbDependencyResolver" /> implementation used for resolving <see cref="TransactionContext" />
    /// factories.
    /// </summary>
    public class TransactionContextResolver : IDbDependencyResolver
    {
        private readonly Func<DbConnection, TransactionContext> _transactionContextFactory;
        private readonly string _providerInvariantName;
        private readonly string _serverName;

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionContextResolver" />
        /// </summary>
        /// <param name="transactionContextFactory">A function that returns a new instance of a transaction context.</param>
        /// <param name="providerInvariantName">
        /// The ADO.NET provider invariant name indicating the type of ADO.NET connection for which the transaction context will be used.
        /// <c>null</c> will match anything.
        /// </param>
        /// <param name="serverName">
        /// A string that will be matched against the server name in the connection string. <c>null</c> will match anything.
        /// </param>
        public TransactionContextResolver(Func<DbConnection, TransactionContext> transactionContextFactory, string providerInvariantName, string serverName)
        {
            Check.NotNull(transactionContextFactory, "transactionContextFactory");

            _providerInvariantName = providerInvariantName;
            _serverName = serverName;
            _transactionContextFactory = transactionContextFactory;
        }

        /// <summary>
        /// If the given type is <see cref="Func{DbConnection, TransactionContext}" />, then this method will attempt
        /// to return the service to use, otherwise it will return <c>null</c>. When the given type is
        /// <see cref="Func{DbConnection, TransactionContext}" />, then the key is expected to be a <see cref="StoreKey" />.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        /// <param name="key">A key used to make a determination of the service to return.</param>
        /// <returns>
        /// An <see cref="Func{DbConnection, TransactionContext}" />, or null.
        /// </returns>
        public object GetService(Type type, object key)
        {
            if (type == typeof(Func<DbConnection, TransactionContext>))
            {
                var transactionContextKey = key as StoreKey;
                if (transactionContextKey == null)
                {
                    throw new ArgumentException(
                        Strings.DbDependencyResolver_InvalidKey(
                            typeof(StoreKey).Name, "Func<DbConnection, TransactionContext>"));
                }

                if (_providerInvariantName != null
                    && !transactionContextKey.ProviderInvariantName.Equals(_providerInvariantName, StringComparison.Ordinal))
                {
                    return null;
                }

                if (_serverName != null
                    && !_serverName.Equals(transactionContextKey.ServerName, StringComparison.Ordinal))
                {
                    return null;
                }

                return _transactionContextFactory;
            }

            return null;
        }

        /// <summary>
        /// If the given type is <see cref="Func{DbConnection, TransactionContext}" />, then this resolver will attempt
        /// to return the service to use, otherwise it will return an empty enumeration. When the given type is
        /// <see cref="Func{DbConnection, TransactionContext}" />, then the key is expected to be an <see cref="StoreKey" />.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        /// <param name="key">A key used to make a determination of the service to return.</param>
        /// <returns>
        /// An enumerable of <see cref="Func{DbConnection, TransactionContext}" />, or an empty enumeration.
        /// </returns>
        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
