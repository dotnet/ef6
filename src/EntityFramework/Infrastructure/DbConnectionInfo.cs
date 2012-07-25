// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents information about a database connection.
    /// </summary>
    [Serializable]
    public class DbConnectionInfo
    {
        private readonly string _connectionName;
        private readonly string _connectionString;
        private readonly string _providerInvariantName;

        /// <summary>
        ///     Creates a new instance of DbConnectionInfo representing a connection that is specified in the application configuration file.
        /// </summary>
        /// <param name = "connectionName">The name of the connection string in the application configuration.</param>
        public DbConnectionInfo(string connectionName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionName));

            _connectionName = connectionName;
        }

        /// <summary>
        ///     Creates a new instance of DbConnectionInfo based on a connection string.
        /// </summary>
        /// <param name = "connectionString">The connection string to use for the connection.</param>
        /// <param name = "providerInvariantName">The name of the provider to use for the connection. Use 'System.Data.SqlClient' for SQL Server.</param>
        public DbConnectionInfo(string connectionString, string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionString));
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
        }

        /// <summary>
        ///     Gets the connection information represented by this instance.
        /// </summary>
        /// <param name = "config">Configuration to use if connection comes from the configuration file.</param>
        internal ConnectionStringSettings GetConnectionString(AppConfig config)
        {
            Contract.Requires(config != null);

            if (_connectionName != null)
            {
                var result = config.GetConnectionString(_connectionName);
                if (result == null)
                {
                    throw Error.DbConnectionInfo_ConnectionStringNotFound(_connectionName);
                }

                return result;
            }

            return new ConnectionStringSettings(null, _connectionString, _providerInvariantName);
        }

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
