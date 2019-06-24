// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    /// Instances of this class are used to create DbConnection objects for
    /// SQL Server LocalDb based on a given database name or connection string.
    /// </summary>
    /// <remarks>
    /// An instance of this class can be set on the <see cref="Database" /> class or in the
    /// app.config/web.config for the application to cause all DbContexts created with no
    /// connection information or just a database name to use SQL Server LocalDb by default.
    /// This class is immutable since multiple threads may access instances simultaneously
    /// when creating connections.
    /// </remarks>
    public sealed class LocalDbConnectionFactory : IDbConnectionFactory
    {
        #region Fields and constructors

        // All fields should remain readonly since this is an immutable class.
        private readonly string _baseConnectionString;
        private readonly string _localDbVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDbConnectionFactory"/> class.
        /// </summary>
        public LocalDbConnectionFactory()
            : this("mssqllocaldb")
        {
        }

        /// <summary>
        /// Creates a new instance of the connection factory for the given version of LocalDb.
        /// For SQL Server 2012 LocalDb use "v11.0".
        /// For SQL Server 2014 and later LocalDb use "mssqllocaldb".
        /// </summary>
        /// <param name="localDbVersion"> The LocalDb version to use. </param>
        public LocalDbConnectionFactory(string localDbVersion)
        {
            Check.NotEmpty(localDbVersion, "localDbVersion");

            _localDbVersion = localDbVersion;
            _baseConnectionString = @"Integrated Security=True; MultipleActiveResultSets=True;";
        }

        /// <summary>
        /// Creates a new instance of the connection factory for the given version of LocalDb.
        /// For SQL Server 2012 LocalDb use "v11.0".
        /// For SQL Server 2014 and later LocalDb use "mssqllocaldb".
        /// </summary>
        /// <param name="localDbVersion"> The LocalDb version to use. </param>
        /// <param name="baseConnectionString"> The connection string to use for options to the database other than the 'Initial Catalog', 'Data Source', and 'AttachDbFilename'. The 'Initial Catalog' and 'AttachDbFilename' will be prepended to this string based on the database name when CreateConnection is called. The 'Data Source' will be set based on the LocalDbVersion argument. </param>
        public LocalDbConnectionFactory(string localDbVersion, string baseConnectionString)
        {
            Check.NotEmpty(localDbVersion, "localDbVersion");
            Check.NotNull(baseConnectionString, "baseConnectionString");

            _localDbVersion = localDbVersion;
            _baseConnectionString = baseConnectionString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The connection string to use for options to the database other than the 'Initial Catalog',
        /// 'Data Source', and 'AttachDbFilename'.
        /// The 'Initial Catalog' and 'AttachDbFilename' will be prepended to this string based on the
        /// database name when CreateConnection is called.
        /// The 'Data Source' will be set based on the LocalDbVersion argument.
        /// The default is 'Integrated Security=True;'.
        /// </summary>
        public string BaseConnectionString
        {
            get { return _baseConnectionString; }
        }

        #endregion

        #region IDbConnectionFactory implementation

        /// <summary>
        /// Creates a connection for SQL Server LocalDb based on the given database name or connection string.
        /// If the given string contains an '=' character then it is treated as a full connection string,
        /// otherwise it is treated as a database name only.
        /// </summary>
        /// <param name="nameOrConnectionString"> The database name or connection string. </param>
        /// <returns> An initialized DbConnection. </returns>
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            var attachDb = " ";
#if !NETSTANDARD2_1
            if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.GetData("DataDirectory") as string))
            {
                attachDb = string.Format(
                    CultureInfo.InvariantCulture, @" AttachDbFilename=|DataDirectory|{0}.mdf; ", nameOrConnectionString);
            }
#endif

            return new SqlConnectionFactory(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"Data Source=(localdb)\{1};{0};{2}",
                    _baseConnectionString,
                    _localDbVersion,
                    attachDb)).CreateConnection(nameOrConnectionString);
        }

        #endregion
    }
}
