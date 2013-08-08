// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Instances of this class are used to create DbConnection objects for
    /// SQL Server Compact Edition based on a given database name or connection string.
    /// </summary>
    /// <remarks>
    /// It is necessary to provide the provider invariant name of the SQL Server Compact
    /// Edition to use when creating an instance of this class.  This is because different
    /// versions of SQL Server Compact Editions use different invariant names.
    /// An instance of this class can be set on the <see cref="Database" /> class to
    /// cause all DbContexts created with no connection information or just a database
    /// name or connection string to use SQL Server Compact Edition by default.
    /// This class is immutable since multiple threads may access instances simultaneously
    /// when creating connections.
    /// </remarks>
    public sealed class SqlCeConnectionFactory : IDbConnectionFactory
    {
        #region Constructors and fields

        // All fields should remain readonly since this is intended to be an immutable class.
        private readonly string _databaseDirectory;
        private readonly string _baseConnectionString;
        private readonly string _providerInvariantName;

        /// <summary>
        /// Creates a new connection factory with empty (default) DatabaseDirectory and BaseConnectionString
        /// properties.
        /// </summary>
        /// <param name="providerInvariantName"> The provider invariant name that specifies the version of SQL Server Compact Edition that should be used. </param>
        public SqlCeConnectionFactory(string providerInvariantName)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");

            _providerInvariantName = providerInvariantName;
            _databaseDirectory = "|DataDirectory|";
            _baseConnectionString = "";
        }

        /// <summary>
        /// Creates a new connection factory with the given DatabaseDirectory and BaseConnectionString properties.
        /// </summary>
        /// <param name="providerInvariantName"> The provider invariant name that specifies the version of SQL Server Compact Edition that should be used. </param>
        /// <param name="databaseDirectory"> The path to prepend to the database name that will form the file name used by SQL Server Compact Edition when it creates or reads the database file. An empty string means that SQL Server Compact Edition will use its default for the database file location. </param>
        /// <param name="baseConnectionString"> The connection string to use for options to the database other than the 'Data Source'. The Data Source will be prepended to this string based on the database name when CreateConnection is called. </param>
        public SqlCeConnectionFactory(
            string providerInvariantName, string databaseDirectory, string baseConnectionString)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(databaseDirectory, "databaseDirectory");
            Check.NotNull(baseConnectionString, "baseConnectionString");

            _providerInvariantName = providerInvariantName;
            _databaseDirectory = databaseDirectory;
            _baseConnectionString = baseConnectionString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The path to prepend to the database name that will form the file name used by
        /// SQL Server Compact Edition when it creates or reads the database file.
        /// The default value is "|DataDirectory|", which means the file will be placed
        /// in the designated data directory.
        /// </summary>
        public string DatabaseDirectory
        {
            get { return _databaseDirectory; }
        }

        /// <summary>
        /// The connection string to use for options to the database other than the 'Data Source'.
        /// The Data Source will be prepended to this string based on the database name when
        /// CreateConnection is called.
        /// The default is the empty string, which means no other options will be used.
        /// </summary>
        public string BaseConnectionString
        {
            get { return _baseConnectionString; }
        }

        /// <summary>
        /// The provider invariant name that specifies the version of SQL Server Compact Edition
        /// that should be used.
        /// </summary>
        public string ProviderInvariantName
        {
            get { return _providerInvariantName; }
        }

        #endregion

        #region CreateConnection

        /// <summary>
        /// Creates a connection for SQL Server Compact Edition based on the given database name or connection string.
        /// If the given string contains an '=' character then it is treated as a full connection string,
        /// otherwise it is treated as a database name only.
        /// </summary>
        /// <param name="nameOrConnectionString"> The database name or connection string. </param>
        /// <returns> An initialized DbConnection. </returns>
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            var factory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(ProviderInvariantName);

            Debug.Assert(factory != null, "Expected DbProviderFactories.GetFactory to throw if provider not found.");

            var connection = factory.CreateConnection();
            if (connection == null)
            {
                throw Error.DbContext_ProviderReturnedNullConnection();
            }

            if (DbHelpers.TreatAsConnectionString(nameOrConnectionString))
            {
                connection.ConnectionString = nameOrConnectionString;
            }
            else
            {
                if (!nameOrConnectionString.EndsWith(".sdf", ignoreCase: true, culture: null))
                {
                    nameOrConnectionString += ".sdf";
                }
                var dataPath = (DatabaseDirectory.StartsWith("|", StringComparison.Ordinal)
                                && DatabaseDirectory.EndsWith("|", StringComparison.Ordinal))
                                   ? DatabaseDirectory + nameOrConnectionString
                                   : Path.Combine(DatabaseDirectory, nameOrConnectionString);
                connection.ConnectionString = String.Format(
                    CultureInfo.InvariantCulture, "Data Source={0}; {1}", dataPath, BaseConnectionString);
            }

            return connection;
        }

        #endregion
    }
}
