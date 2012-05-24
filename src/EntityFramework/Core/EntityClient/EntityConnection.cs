namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using IsolationLevel = System.Data.IsolationLevel;

    /// <summary>
    /// Class representing a connection for the conceptual layer. An entity connection may only
    /// be initialized once (by opening the connection). It is subsequently not possible to change
    /// the connection string, attach a new store connection, or change the store connection string.
    /// </summary>
    public sealed class EntityConnection : DbConnection
    {
        private bool _disposed = false;
        private readonly InternalEntityConnection _internalEntityConnection;

        /// <summary>
        /// Constructs the EntityConnection object with a connection not yet associated to a particular store
        /// </summary>
        [ResourceExposure(ResourceScope.None)] //We are not exposing any resource
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        //For EntityConnection constructor. But since the connection string we pass in is an Empty String,
        //we consume the resource and do not expose it any further.        
        public EntityConnection()
            : this(new InternalEntityConnection(String.Empty))
        {
        }

        /// <summary>
        /// Constructs the EntityConnection object with a connection string
        /// </summary>
        /// <param name="connectionString">The connection string, may contain a list of settings for the connection or
        /// just the name of the connection to use</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        //For ChangeConnectionString method call. But the paths are not created in this method.        
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        public EntityConnection(string connectionString)
            : this(new InternalEntityConnection(connectionString))
        {
        }

        /// <summary>
        /// Constructs the EntityConnection from Metadata loaded in memory
        /// </summary>
        /// <param name="workspace">Workspace containing metadata information.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "Object is in fact passed to property of the class and gets Disposed properly in the Dispose() method.")]
        public EntityConnection(MetadataWorkspace workspace, DbConnection connection)
            : this(new InternalEntityConnection(workspace, connection))
        {
        }

        internal EntityConnection(InternalEntityConnection internalEntityConnection)
        {
            _internalEntityConnection = internalEntityConnection;
            _internalEntityConnection.EntityConnectionWrapper = this;
        }

        /// <summary>
        /// Get or set the entity connection string associated with this connection object
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string ConnectionString
        {
            get { return _internalEntityConnection.ConnectionString; }
            [ResourceExposure(ResourceScope.Machine)] // Exposes the file names as part of ConnectionString which are a Machine resource
            [ResourceConsumption(ResourceScope.Machine)]
            // For ChangeConnectionString method call. But the paths are not created in this method.
                set { _internalEntityConnection.ConnectionString = value; }
        }

        /// <summary>
        /// Formats provider string to replace " with \" so it can be appended within quotation marks "..."
        /// </summary>
        private static string FormatProviderString(string providerString)
        {
            return providerString.Trim().Replace("\"", "\\\"");
        }

        /// <summary>
        /// Get the time to wait when attempting to establish a connection before ending the try and generating an error
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int ConnectionTimeout
        {
            get { return _internalEntityConnection.ConnectionTimeout; }
        }

        /// <summary>
        /// Get the name of the current database or the database that will be used after a connection is opened
        /// </summary>
        public override string Database
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// Gets the ConnectionState property of the EntityConnection
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override ConnectionState State
        {
            get { return _internalEntityConnection.State; }
        }

        /// <summary>
        /// Gets the name or network address of the data source to connect to
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string DataSource
        {
            get { return _internalEntityConnection.DataSource; }
        }

        /// <summary>
        /// Gets a string that contains the version of the data store to which the client is connected
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string ServerVersion
        {
            get { return _internalEntityConnection.ServerVersion; }
        }

        /// <summary>
        /// Gets the provider factory associated with EntityConnection
        /// </summary>
        protected override DbProviderFactory DbProviderFactory
        {
            get { return EntityProviderFactory.Instance; }
        }

        /// <summary>
        /// Gets the DbProviderFactory for the underlying provider
        /// </summary>
        internal DbProviderFactory StoreProviderFactory
        {
            get { return _internalEntityConnection.StoreProviderFactory; }
        }

        /// <summary>
        /// Gets the DbConnection for the underlying provider connection
        /// </summary>
        public DbConnection StoreConnection
        {
            get { return _internalEntityConnection.StoreConnection; }
        }

        /// <summary>
        /// Gets the metadata workspace used by this connection
        /// </summary>
        [CLSCompliant(false)]
        public MetadataWorkspace GetMetadataWorkspace()
        {
            return _internalEntityConnection.GetMetadataWorkspace();
        }

        [ResourceExposure(ResourceScope.None)] // The resource( path name) is not exposed to the callers of this method
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        //For SplitPaths call and we pick the file names from class variable.
        internal MetadataWorkspace GetMetadataWorkspace(bool initializeAllCollections)
        {
            return _internalEntityConnection.GetMetadataWorkspace(initializeAllCollections);
        }

        /// <summary>
        /// Gets the current transaction that this connection is enlisted in
        /// </summary>
        internal EntityTransaction CurrentTransaction
        {
            get { return _internalEntityConnection.CurrentTransaction; }
        }

        /// <summary>
        /// Whether the user has enlisted in transaction using EnlistTransaction method
        /// </summary>
        internal bool EnlistedInUserTransaction
        {
            get { return _internalEntityConnection.EnlistedInUserTransaction; }
        }

        /// <summary>
        /// Establish a connection to the data store by calling the Open method on the underlying data provider
        /// </summary>
        public override void Open()
        {
            _internalEntityConnection.Open();
        }

        /// <summary>
        /// An asynchronous version of Open, which
        /// establishes a connection to the data store by calling the Open method on the underlying data provider
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return _internalEntityConnection.OpenAsync(cancellationToken);
        }

        /// <summary>
        /// Create a new command object that uses this connection object.
        /// </summary>
        public new EntityCommand CreateCommand()
        {
            return new EntityCommand(null, this);
        }

        /// <summary>
        /// Create a new command object that uses this connection object
        /// </summary>
        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        /// <summary>
        /// Close the connection to the data store
        /// </summary>
        public override void Close()
        {
            _internalEntityConnection.Close();
        }

        /// <summary>
        /// Changes the current database for this connection
        /// </summary>
        /// <param name="databaseName">The name of the database to change to</param>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <returns>An object representing the new transaction</returns>
        public new EntityTransaction BeginTransaction()
        {
            return base.BeginTransaction() as EntityTransaction;
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction</param>
        /// <returns>An object representing the new transaction</returns>
        public new EntityTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return base.BeginTransaction(isolationLevel) as EntityTransaction;
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction</param>
        /// <returns>An object representing the new transaction</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _internalEntityConnection.BeginDbTransaction(isolationLevel);
        }

        /// <summary>
        /// Enlist in the given transaction
        /// </summary>
        /// <param name="transaction">The transaction object to enlist into</param>
        public override void EnlistTransaction(Transaction transaction)
        {
            _internalEntityConnection.EnlistTransaction(transaction);
        }

        internal void InternalOnStageChange(StateChangeEventArgs stateChange)
        {
            OnStateChange(stateChange);
        }

        /// <summary>
        /// Cleans up this connection object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_currentTransaction")]
        [ResourceExposure(ResourceScope.None)] //We are not exposing any resource
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        //For ChangeConnectionString method call. But since the connection string we pass in is an Empty String,
        //we consume the resource and do not expose it any further.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _internalEntityConnection.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears the current DbTransaction for this connection
        /// </summary>
        internal void ClearCurrentTransaction()
        {
            _internalEntityConnection.ClearCurrentTransaction();
        }
    }
}
