// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// A key used for resolving dependencies based on the target store. It consists of the ADO.NET provider invariant name
    /// and the database server name as specified in the connection string.
    /// </summary>
    public class StoreKey
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StoreKey" />
        /// </summary>
        /// <param name="providerInvariantName">
        /// The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this dependency will be used.
        /// </param>
        /// <param name="serverName"> A string that will be matched against the server name in the connection string. </param>
        public StoreKey(string providerInvariantName, string serverName)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");

            ProviderInvariantName = providerInvariantName;
            ServerName = serverName;
        }

        /// <summary>
        /// The ADO.NET provider invariant name indicating the type of ADO.NET connection for which this dependency will be used.
        /// </summary>
        public string ProviderInvariantName { get; private set; }

        /// <summary>
        /// A string that will be matched against the server name in the connection string.
        /// </summary>
        public string ServerName { get; private set; }
    }
}
