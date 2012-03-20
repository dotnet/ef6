namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     InternalConnection objects manage DbConnections.
    ///     Two concrete base classes of this abstract interface exist:<see cref = "LazyInternalConnection" />
    ///     and <see cref = "EagerInternalConnection" />.
    /// </summary>
    internal abstract class InternalConnection : IInternalConnection
    {
        private string _key;
        private string _providerName;
        private string _originalConnectionString;
        private string _originalDatabaseName;
        private string _originalDataSource;

        /// <summary>
        ///     Returns the underlying DbConnection.
        /// </summary>
        public virtual DbConnection Connection
        {
            get
            {
                Contract.Assert(UnderlyingConnection != null, "UnderlyingConnection should have been initialized before getting here.");

                var asEntityConnection = UnderlyingConnection as EntityConnection;
                return asEntityConnection != null ? asEntityConnection.StoreConnection : UnderlyingConnection;
            }
        }

        /// <summary>
        ///     Returns a key consisting of the connection type and connection string.
        ///     If this is an EntityConnection then the metadata path is included in the key returned.
        /// </summary>
        /// <value></value>
        public virtual string ConnectionKey
        {
            get
            {
                Contract.Assert(UnderlyingConnection != null, "UnderlyingConnection should have been initialized before getting here.");

                return _key
                       ??
                       (_key =
                        String.Format(
                            CultureInfo.InvariantCulture, "{0};{1}", UnderlyingConnection.GetType(), UnderlyingConnection.ConnectionString));
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the connection is an EF connection which therefore contains
        ///     metadata specifying the model, or instead is a store connection, in which case it contains no
        ///     model info.
        /// </summary>
        /// <value><c>true</c> if the connection contains model info; otherwise, <c>false</c>.</value>
        public virtual bool ConnectionHasModel
        {
            get
            {
                Contract.Assert(UnderlyingConnection != null, "UnderlyingConnection should have been initialized before getting here.");

                return UnderlyingConnection is EntityConnection;
            }
        }

        /// <summary>
        ///     Returns the origin of the underlying connection string.
        /// </summary>
        public abstract DbConnectionStringOrigin ConnectionStringOrigin { get; }

        /// <summary>
        ///     Gets or sets an object representing a config file used for looking for DefaultConnectionFactory entries
        ///     and connection strins.
        /// </summary>
        public virtual AppConfig AppConfig { get; set; }

        /// <summary>
        ///     Gets or sets the provider to be used when creating the underlying connection.
        /// </summary>
        public virtual string ProviderName
        {
            get { return _providerName ?? (_providerName = UnderlyingConnection == null ? null : Connection.GetProviderInvariantName()); }
            set { _providerName = value; }
        }

        /// <summary>
        ///     Gets the name of the underlying connection string.
        /// </summary>
        public virtual string ConnectionStringName
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets the original connection string.
        /// </summary>
        public string OriginalConnectionString
        {
            get
            {
                Contract.Assert(UnderlyingConnection != null);

                // Reset the original connection string if it has been changed.
                // This helps in trying to use the correct connection if the connection string is mutated after it has
                // been created.
                if (!string.Equals(_originalDatabaseName, UnderlyingConnection.Database, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(_originalDataSource, UnderlyingConnection.DataSource, StringComparison.OrdinalIgnoreCase))
                {
                    OnConnectionInitialized();
                }

                return _originalConnectionString;
            }
        }

        /// <summary>
        ///     Creates an <see cref = "ObjectContext" /> from metadata in the connection.  This method must
        ///     only be called if ConnectionHasModel returns true.
        /// </summary>
        /// <returns>The newly created context.</returns>
        public virtual ObjectContext CreateObjectContextFromConnectionModel()
        {
            Contract.Assert(UnderlyingConnection != null, "UnderlyingConnection should have been initialized before getting here.");
            Contract.Assert(UnderlyingConnection is EntityConnection, "Cannot create context from connection for non-EntityConnection.");

            var objectContext = new ObjectContext((EntityConnection)UnderlyingConnection);

            var containers = objectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
            if (containers.Count == 1)
            {
                objectContext.DefaultContainerName = containers.Single().Name;
            }

            return objectContext;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Gets or sets the underlying <see cref = "DbConnection" /> object.  No initialization is done when the
        ///     connection is obtained, and it can also be set to null.
        /// </summary>
        /// <value>The underlying connection.</value>
        protected DbConnection UnderlyingConnection { get; set; }

        /// <summary>
        ///     Called after the connection is initialized for the first time.
        /// </summary>
        protected void OnConnectionInitialized()
        {
            Contract.Assert(UnderlyingConnection != null);

            _originalConnectionString = AddAppNameCookieToConnectionString(UnderlyingConnection);

            try
            {
                _originalDatabaseName = UnderlyingConnection.Database;
            }
            catch (NotImplementedException) { }

            try
            {
                _originalDataSource = UnderlyingConnection.DataSource;
            }
            catch (NotImplementedException) { }
        }

        /// <summary>
        ///     Adds a tracking cookie to the connection string for SqlConnections. Returns the
        ///     possibly modified store connection string.
        /// </summary>
        public static string AddAppNameCookieToConnectionString(DbConnection connection)
        {
            Contract.Assert(connection != null);

            var connectionString = connection.ConnectionString;

            var entityConnection = connection as EntityConnection;

            if (entityConnection != null)
            {
                connection = entityConnection.StoreConnection;
                connectionString = (connection != null) ? connection.ConnectionString : null;
            }

            if ((connection is SqlConnection)
                && (connection.State == ConnectionState.Closed))
            {
                var connectionStringBuilder
                    = new SqlConnectionStringBuilder(connection.ConnectionString);

                const string defaultAppName = ".Net SqlClient Data Provider";

                if ((string.IsNullOrWhiteSpace(connectionStringBuilder.ApplicationName)
                     || string.Equals(connectionStringBuilder.ApplicationName, defaultAppName, StringComparison.OrdinalIgnoreCase))
                    && (connectionStringBuilder.IntegratedSecurity
                        || !string.IsNullOrEmpty(connectionStringBuilder.Password)))
                {
                    connectionStringBuilder.ApplicationName = "EntityFrameworkMUE";
                    connection.ConnectionString = connectionStringBuilder.ToString();
                    connectionString = connection.ConnectionString;
                }
            }

            return connectionString;
        }
    }
}