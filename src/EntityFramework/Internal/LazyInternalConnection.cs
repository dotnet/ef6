// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    // <summary>
    // A LazyInternalConnection object manages information that can be used to create a DbConnection object and
    // is responsible for creating that object and disposing it.
    // </summary>
    internal class LazyInternalConnection : InternalConnection
    {
        #region Fields and constructors

        // Info used for creating the connection.
        private readonly string _nameOrConnectionString;
        private DbConnectionStringOrigin _connectionStringOrigin = DbConnectionStringOrigin.Convention;
        private string _connectionStringName;
        private readonly DbConnectionInfo _connectionInfo;
        private bool? _hasModel;

        // <summary>
        // Creates a new LazyInternalConnection using convention to calculate the connection.
        // The DbConnection object will be created lazily on demand and will be disposed when the LazyInternalConnection is disposed.
        // </summary>
        // <param name="nameOrConnectionString"> Either the database name or a connection string. </param>
        public LazyInternalConnection(string nameOrConnectionString)
            : this(null, nameOrConnectionString)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LazyInternalConnection(DbContext context, string nameOrConnectionString)
            : base(context == null
                ? null
                : new DbInterceptionContext().WithDbContext(context))
        {
            DebugCheck.NotEmpty(nameOrConnectionString);

            _nameOrConnectionString = nameOrConnectionString;
            AppConfig = AppConfig.DefaultInstance;
        }

        // <summary>
        // Creates a new LazyInternalConnection targeting a specific database.
        // The DbConnection object will be created lazily on demand and will be disposed when the LazyInternalConnection is disposed.
        // </summary>
        // <param name="connectionInfo"> The connection to target. </param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LazyInternalConnection(DbContext context, DbConnectionInfo connectionInfo)
            : base(new DbInterceptionContext().WithDbContext(context))
        {
            DebugCheck.NotNull(connectionInfo);

            _connectionInfo = connectionInfo;
            AppConfig = AppConfig.DefaultInstance;
        }

        #endregion

        #region Connection

        // <summary>
        // Returns the underlying DbConnection, creating it first if it does not already exist.
        // </summary>
        public override DbConnection Connection
        {
            get
            {
                Initialize();
                return base.Connection;
            }
        }

        // <summary>
        // Returns the origin of the underlying connection string.
        // </summary>
        public override DbConnectionStringOrigin ConnectionStringOrigin
        {
            get
            {
                Initialize();
                return _connectionStringOrigin;
            }
        }

        // <summary>
        // Gets the name of the underlying connection string.
        // </summary>
        public override string ConnectionStringName
        {
            get
            {
                Initialize();
                return _connectionStringName;
            }
        }

        // <inheritdoc />
        public override string ConnectionKey
        {
            get
            {
                Initialize();
                return base.ConnectionKey;
            }
        }

        // <inheritdoc />
        public override string OriginalConnectionString
        {
            get
            {
                Initialize();
                return base.OriginalConnectionString;
            }
        }

        // <inheritdoc />
        public override string ProviderName
        {
            get
            {
                Initialize();
                return base.ProviderName;
            }
            set { base.ProviderName = value; }
        }

        #endregion

        #region EF connection string handling

        // <summary>
        // Gets a value indicating whether the connection is an EF connection which therefore contains
        // metadata specifying the model, or instead is a store connection, in which case it contains no
        // model info.
        // </summary>
        // <value>
        // <c>true</c> if connection contain model info; otherwise, <c>false</c> .
        // </value>
        public override bool ConnectionHasModel
        {
            get
            {
                if (!_hasModel.HasValue)
                {
                    // Avoid initializing the connection just to work out if it is an EF connection
                    if (UnderlyingConnection == null)
                    {
                        var connectionString = _nameOrConnectionString;
                        string name;
                        if (_connectionInfo != null)
                        {
                            connectionString = _connectionInfo.GetConnectionString(AppConfig).ConnectionString;
                        }
                        else if (DbHelpers.TryGetConnectionName(_nameOrConnectionString, out name))
                        {
                            var setting = FindConnectionInConfig(name, AppConfig);

                            // If the connection string is of the form name=, but the name was not found in the config file
                            if (setting == null
                                && DbHelpers.TreatAsConnectionString(_nameOrConnectionString))
                            {
                                throw Error.DbContext_ConnectionStringNotFound(name);
                            }

                            if (setting != null)
                            {
                                connectionString = setting.ConnectionString;
                            }
                        }

                        _hasModel = DbHelpers.IsFullEFConnectionString(connectionString);
                    }
                    else
                    {
                        _hasModel = UnderlyingConnection is EntityConnection;
                    }
                }

                return _hasModel.Value;
            }
        }

        // <summary>
        // Creates an <see cref="ObjectContext" /> from metadata in the connection.  This method must
        // only be called if ConnectionHasModel returns true.
        // </summary>
        // <returns> The newly created context. </returns>
        public override ObjectContext CreateObjectContextFromConnectionModel()
        {
            Initialize();
            return base.CreateObjectContextFromConnectionModel();
        }

        #endregion

        #region Dispose

        // <summary>
        // Disposes the underlying DbConnection.
        // Note that dispose actually puts the LazyInternalConnection back to its initial state such that
        // it can be used again.
        // </summary>
        public override void Dispose()
        {
            if (UnderlyingConnection != null)
            {
                if (UnderlyingConnection is EntityConnection)
                {
                    UnderlyingConnection.Dispose();
                }
                else
                {
                    DbInterception.Dispatch.Connection.Dispose(UnderlyingConnection, InterceptionContext);
                }
                UnderlyingConnection = null;
            }
        }

        #endregion

        #region Initialization

        // <summary>
        // Gets a value indicating if the lazy connection has been initialized.
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal bool IsInitialized
        {
            get { return UnderlyingConnection != null; }
        }

        // <summary>
        // Creates the underlying <see cref="DbConnection" /> (which may actually be an <see cref="EntityConnection" />)
        // if it does not already exist.
        // </summary>
        private void Initialize()
        {
            if (UnderlyingConnection == null)
            {
                Debug.Assert(AppConfig != null);

                string name;
                if (_connectionInfo != null)
                {
                    var connection = _connectionInfo.GetConnectionString(AppConfig);
                    InitializeFromConnectionStringSetting(connection);

                    _connectionStringOrigin = DbConnectionStringOrigin.DbContextInfo;
                    _connectionStringName = connection.Name;
                }
                    // If the name or connection string is a simple name or is in the form "name=foo" then use
                    // that name to try to load from the app/web config file. 
                else if (!DbHelpers.TryGetConnectionName(_nameOrConnectionString, out name)
                         || !TryInitializeFromAppConfig(name, AppConfig))
                {
                    // If the connection string is of the form name=, but the name was not found in the config file
                    // then always throw since we always interpret name= to mean find in the config file only.
                    if (name != null
                        && DbHelpers.TreatAsConnectionString(_nameOrConnectionString))
                    {
                        throw Error.DbContext_ConnectionStringNotFound(name);
                    }

                    // If the name or connection string is a full EF connection string, then create an EntityConnection from it.
                    if (DbHelpers.IsFullEFConnectionString(_nameOrConnectionString))
                    {
                        UnderlyingConnection = new EntityConnection(_nameOrConnectionString);
                    }
                    else
                    {
                        if (base.ProviderName != null)
                        {
                            CreateConnectionFromProviderName(base.ProviderName);
                        }
                        else
                        {
                            // Otherwise figure out the connection factory to use (either the default,
                            // the one set in code, or one provided by DbContextInfo via the AppSettings property
                            UnderlyingConnection = DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>()
                                .CreateConnection(name ?? _nameOrConnectionString);

                            if (UnderlyingConnection == null)
                            {
                                throw Error.DbContext_ConnectionFactoryReturnedNullConnection();
                            }
                        }
                    }

                    if (name != null)
                    {
                        _connectionStringOrigin = DbConnectionStringOrigin.Convention;
                        _connectionStringName = name;
                    }
                    else
                    {
                        _connectionStringOrigin = DbConnectionStringOrigin.UserCode;
                    }
                }

                OnConnectionInitialized();
            }

            Debug.Assert(UnderlyingConnection != null, "Connection should have been initialized by some mechanism.");
        }

        // <summary>
        // Searches the app.config/web.config file for a connection that matches the given name.
        // The connection might be a store connection or an EF connection.
        // </summary>
        // <param name="name"> The connection name. </param>
        // <returns> True if a connection from the app.config file was found and used. </returns>
        private bool TryInitializeFromAppConfig(string name, AppConfig config)
        {
            DebugCheck.NotNull(config);

            var appConfigConnection = FindConnectionInConfig(name, config);
            if (appConfigConnection != null)
            {
                InitializeFromConnectionStringSetting(appConfigConnection);
                _connectionStringOrigin = DbConnectionStringOrigin.Configuration;
                _connectionStringName = appConfigConnection.Name;

                return true;
            }

            return false;
        }

        // <summary>
        // Attempts to locate a connection entry in the configuration based on the supplied context name.
        // </summary>
        // <param name="name"> The name to search for. </param>
        // <param name="config"> The configuration to search in. </param>
        // <returns> Connection string if found, otherwise null. </returns>
        private static ConnectionStringSettings FindConnectionInConfig(string name, AppConfig config)
        {
            // Build a list of candidate names that might be found in the app.config/web.config file.
            // The first entry is the full name.
            var candidates = new List<string>
            {
                name
            };

            // Second entry is full name with namespace stripped out.
            var lastDot = name.LastIndexOf('.');
            if (lastDot >= 0
                && lastDot + 1 < name.Length)
            {
                candidates.Add(name.Substring(lastDot + 1));
            }

            // Now go through each candidate.  As soon as we find one that matches, stop.
            var appConfigConnection = (from c in candidates
                where config.GetConnectionString(c) != null
                select config.GetConnectionString(c)).FirstOrDefault();
            return appConfigConnection;
        }

        // <summary>
        // Initializes the connection based on a connection string.
        // </summary>
        // <param name="appConfigConnection"> The settings to initialize from. </param>
        private void InitializeFromConnectionStringSetting(ConnectionStringSettings appConfigConnection)
        {
            var providerInvariantName = appConfigConnection.ProviderName;
            if (String.IsNullOrWhiteSpace(providerInvariantName))
            {
                throw Error.DbContext_ProviderNameMissing(appConfigConnection.Name);
            }

            if (String.Equals(providerInvariantName, "System.Data.EntityClient", StringComparison.OrdinalIgnoreCase))
            {
                UnderlyingConnection = new EntityConnection(appConfigConnection.ConnectionString);
            }
            else
            {
                CreateConnectionFromProviderName(providerInvariantName);

                DbInterception.Dispatch.Connection.SetConnectionString(
                    UnderlyingConnection,
                    new DbConnectionPropertyInterceptionContext<string>().WithValue(appConfigConnection.ConnectionString));
            }
        }

        private void CreateConnectionFromProviderName(string providerInvariantName)
        {
            var factory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInvariantName);
            Debug.Assert(factory != null, "Expected DbProviderFactories.GetFactory to throw if provider not found.");

            UnderlyingConnection = factory.CreateConnection();

            if (UnderlyingConnection == null)
            {
                throw Error.DbContext_ProviderReturnedNullConnection();
            }
        }

        #endregion
    }
}
