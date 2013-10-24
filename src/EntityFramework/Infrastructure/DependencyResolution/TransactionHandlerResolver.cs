// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// An <see cref="IDbDependencyResolver" /> implementation used for resolving <see cref="TransactionHandler" />
    /// factories.
    /// </summary>
    public class TransactionHandlerResolver : IDbDependencyResolver
    {
        private readonly Func<TransactionHandler> _transactionHandlerFactory;
        private readonly string _providerInvariantName;
        private readonly string _serverName;

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionHandlerResolver" />
        /// </summary>
        /// <param name="transactionHandlerFactory">A function that returns a new instance of a transaction handler.</param>
        /// <param name="providerInvariantName">
        /// The ADO.NET provider invariant name indicating the type of ADO.NET connection for which the transaction handler will be used.
        /// <c>null</c> will match anything.
        /// </param>
        /// <param name="serverName">
        /// A string that will be matched against the server name in the connection string. <c>null</c> will match anything.
        /// </param>
        public TransactionHandlerResolver(
            Func<TransactionHandler> transactionHandlerFactory, string providerInvariantName, string serverName)
        {
            Check.NotNull(transactionHandlerFactory, "transactionHandlerFactory");

            _providerInvariantName = providerInvariantName;
            _serverName = serverName;
            _transactionHandlerFactory = transactionHandlerFactory;
        }

        /// <summary>
        /// If the given type is <see cref="Func{TransactionHandler}" />, then this method will attempt
        /// to return the service to use, otherwise it will return <c>null</c>. When the given type is
        /// <see cref="Func{TransactionHandler}" />, then the key is expected to be a <see cref="StoreKey" />.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        /// <param name="key">A key used to make a determination of the service to return.</param>
        /// <returns>
        /// An <see cref="Func{TransactionHandler}" />, or null.
        /// </returns>
        public object GetService(Type type, object key)
        {
            if (type == typeof(Func<TransactionHandler>))
            {
                var transactionHandlerKey = key as StoreKey;
                if (transactionHandlerKey == null)
                {
                    throw new ArgumentException(
                        Strings.DbDependencyResolver_InvalidKey(
                            typeof(StoreKey).Name, "Func<TransactionHandler>"));
                }

                if (_providerInvariantName != null
                    && !transactionHandlerKey.ProviderInvariantName.Equals(_providerInvariantName, StringComparison.Ordinal))
                {
                    return null;
                }

                if (_serverName != null
                    && !_serverName.Equals(transactionHandlerKey.ServerName, StringComparison.Ordinal))
                {
                    return null;
                }

                return _transactionHandlerFactory;
            }

            return null;
        }

        /// <summary>
        /// If the given type is <see cref="Func{TransactionHandler}" />, then this resolver will attempt
        /// to return the service to use, otherwise it will return an empty enumeration. When the given type is
        /// <see cref="Func{TransactionHandler}" />, then the key is expected to be an <see cref="StoreKey" />.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        /// <param name="key">A key used to make a determination of the service to return.</param>
        /// <returns>
        /// An enumerable of <see cref="Func{TransactionHandler}" />, or an empty enumeration.
        /// </returns>
        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
