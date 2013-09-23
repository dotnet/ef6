// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents information about a database connection.
    /// </summary>
    [Serializable]
    public class DbConnectionInfo
    {
        private readonly string _connectionName;
        private readonly string _connectionString;
        private readonly string _providerInvariantName;

        /// <summary>
        /// Creates a new instance of DbConnectionInfo representing a connection that is specified in the application configuration file.
        /// </summary>
        /// <param name="connectionName"> The name of the connection string in the application configuration. </param>
        public DbConnectionInfo(string connectionName)
        {
            Check.NotEmpty(connectionName, "connectionName");

            _connectionName = connectionName;
        }

        /// <summary>
        /// Creates a new instance of DbConnectionInfo based on a connection string.
        /// </summary>
        /// <param name="connectionString"> The connection string to use for the connection. </param>
        /// <param name="providerInvariantName"> The name of the provider to use for the connection. Use 'System.Data.SqlClient' for SQL Server. </param>
        public DbConnectionInfo(string connectionString, string providerInvariantName)
        {
            Check.NotEmpty(connectionString, "connectionString");
            Check.NotEmpty(providerInvariantName, "providerInvariantName");

            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
        }

        // <summary>
        // Gets the connection information represented by this instance.
        // </summary>
        // <param name="config"> Configuration to use if connection comes from the configuration file. </param>
        internal ConnectionStringSettings GetConnectionString(AppConfig config)
        {
            DebugCheck.NotNull(config);

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

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
