// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Transactions;
    using System.Xml;

    /// <summary>
    ///     The factory for building command definitions; use the type of this object
    ///     as the argument to the IServiceProvider.GetService method on the provider
    ///     factory;
    /// </summary>
    [CLSCompliant(false)]
    public abstract class DbProviderServices : IDbDependencyResolver
    {
        private readonly Lazy<IDbDependencyResolver> _resolver;
        private readonly Lazy<DbCommandTreeDispatcher> _treeDispatcher;

        private static readonly ConcurrentDictionary<DbProviderInfo, DbSpatialServices> _spatialServices =
            new ConcurrentDictionary<DbProviderInfo, DbSpatialServices>();

        private static readonly ConcurrentDictionary<ExecutionStrategyKey, Func<IExecutionStrategy>>
            _executionStrategyFactories =
                new ConcurrentDictionary<ExecutionStrategyKey, Func<IExecutionStrategy>>();

        private readonly ResolverChain _resolvers = new ResolverChain();

        /// <summary>
        ///     Constructs an EF provider that will use the <see cref="IDbDependencyResolver" /> obtained from
        ///     the app domain <see cref="DbConfiguration" /> Singleton for resolving EF dependencies such
        ///     as the <see cref="DbSpatialServices" /> instance to use.
        /// </summary>
        protected DbProviderServices()
            : this(() => DbConfiguration.DependencyResolver)
        {
        }

        /// <summary>
        ///     Constructs an EF provider that will use the given <see cref="IDbDependencyResolver" /> for
        ///     resolving EF dependencies such as the <see cref="DbSpatialServices" /> instance to use.
        /// </summary>
        /// <param name="resolver"> The resolver to use. </param>
        protected DbProviderServices(Func<IDbDependencyResolver> resolver)
            : this(resolver, new Lazy<DbCommandTreeDispatcher>(() => Interception.Dispatch.CommandTree))
        {
        }

        internal DbProviderServices(Func<IDbDependencyResolver> resolver, Lazy<DbCommandTreeDispatcher> treeDispatcher)
        {
            Check.NotNull(resolver, "resolver");
            DebugCheck.NotNull(treeDispatcher);

            _resolver = new Lazy<IDbDependencyResolver>(resolver);
            _treeDispatcher = treeDispatcher;
        }

        /// <summary>
        ///     Create a Command Definition object given a command tree.
        /// </summary>
        /// <param name="commandTree"> command tree for the statement </param>
        /// <returns> an executable command definition object </returns>
        /// <remarks>
        ///     This method simply delegates to the provider's implementation of CreateDbCommandDefinition.
        /// </remarks>
        public DbCommandDefinition CreateCommandDefinition(DbCommandTree commandTree)
        {
            Check.NotNull(commandTree, "commandTree");

            return CreateCommandDefinition(commandTree, new DbInterceptionContext());
        }

        internal DbCommandDefinition CreateCommandDefinition(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(interceptionContext);

            ValidateDataSpace(commandTree);

            var storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);

            Debug.Assert(
                storeMetadata.StoreProviderManifest != null,
                "StoreItemCollection has null StoreProviderManifest?");

            commandTree = _treeDispatcher.Value.Created(commandTree, new DbCommandTreeInterceptionContext(interceptionContext));

            return CreateDbCommandDefinition(storeMetadata.StoreProviderManifest, commandTree, interceptionContext);
        }

        internal virtual DbCommandDefinition CreateDbCommandDefinition(
            DbProviderManifest providerManifest,
            DbCommandTree commandTree,
            DbInterceptionContext interceptionContext)
        {
            return CreateDbCommandDefinition(providerManifest, commandTree);
        }

        /// <summary>Creates command definition from specified manifest and command tree.</summary>
        /// <returns>The created command definition.</returns>
        /// <param name="providerManifest">The manifest.</param>
        /// <param name="commandTree">The command tree.</param>
        public DbCommandDefinition CreateCommandDefinition(
            DbProviderManifest providerManifest,
            DbCommandTree commandTree)
        {
            Check.NotNull(providerManifest, "providerManifest");
            Check.NotNull(commandTree, "commandTree");

            try
            {
                return CreateDbCommandDefinition(providerManifest, commandTree);
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotCreateACommandDefinition, e);
                }
                throw;
            }
        }

        /// <summary>Creates a command definition object for the specified provider manifest and command tree.</summary>
        /// <returns>An executable command definition object.</returns>
        /// <param name="providerManifest">Provider manifest previously retrieved from the store provider.</param>
        /// <param name="commandTree">Command tree for the statement.</param>
        protected abstract DbCommandDefinition CreateDbCommandDefinition(
            DbProviderManifest providerManifest,
            DbCommandTree commandTree);

        /// <summary>
        ///     Ensures that the data space of the specified command tree is the target (S-) space
        /// </summary>
        /// <param name="commandTree"> The command tree for which the data space should be validated </param>
        internal virtual void ValidateDataSpace(DbCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            if (commandTree.DataSpace != DataSpace.SSpace)
            {
                throw new ProviderIncompatibleException(Strings.ProviderRequiresStoreCommandTree);
            }
        }

        internal virtual DbCommand CreateCommand(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(interceptionContext);

            var commandDefinition = CreateCommandDefinition(commandTree, interceptionContext);
            var command = commandDefinition.CreateCommand();
            return command;
        }

        /// <summary>
        ///     Create the default DbCommandDefinition object based on the prototype command
        ///     This method is intended for provider writers to build a default command definition
        ///     from a command.
        ///     Note: This will clone the prototype
        /// </summary>
        /// <param name="prototype"> the prototype command </param>
        /// <returns> an executable command definition object </returns>
        public virtual DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return DbCommandDefinition.CreateCommandDefinition(prototype);
        }

        /// <summary>Returns provider manifest token given a connection.</summary>
        /// <returns>The provider manifest token.</returns>
        /// <param name="connection">Connection to provider.</param>
        public string GetProviderManifestToken(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            try
            {
                string providerManifestToken;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    providerManifestToken = GetDbProviderManifestToken(connection);
                }

                if (providerManifestToken == null)
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifestToken);
                }

                return providerManifestToken;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifestToken, e);
                }
                throw;
            }
        }

        /// <summary>Returns provider manifest token given a connection.</summary>
        /// <returns>The provider manifest token for the specified connection.</returns>
        /// <param name="connection">Connection to provider.</param>
        protected abstract string GetDbProviderManifestToken(DbConnection connection);

        /// <summary>Returns the provider manifest by using the specified version information.</summary>
        /// <returns>The provider manifest by using the specified version information.</returns>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        public DbProviderManifest GetProviderManifest(string manifestToken)
        {
            Check.NotNull(manifestToken, "manifestToken");

            try
            {
                var providerManifest = GetDbProviderManifest(manifestToken);
                if (providerManifest == null)
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifest);
                }

                return providerManifest;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnAProviderManifest, e);
                }
                throw;
            }
        }

        /// <summary>When overridden in a derived class, returns an instance of a class that derives from the DbProviderManifest.</summary>
        /// <returns>A DbProviderManifest object that represents the provider manifest.</returns>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        protected abstract DbProviderManifest GetDbProviderManifest(string manifestToken);

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategy" /> that will be used to execute methods that use the specified connection.
        /// </summary>
        /// <param name="connection">The database connection</param>
        /// <returns>
        ///     A new instance of <see cref="ExecutionStrategyBase" />
        /// </returns>
        public static IExecutionStrategy GetExecutionStrategy(DbConnection connection)
        {
            return GetExecutionStrategy(connection, GetProviderFactory(connection));
        }

        /// <summary>
        ///     Gets the <see cref="IExecutionStrategy" /> that will be used to execute methods that use the specified connection.
        ///     Uses MetadataWorkspace for faster lookup.
        /// </summary>
        /// <param name="connection">The database connection</param>
        /// <returns>
        ///     A new instance of <see cref="ExecutionStrategyBase" />
        /// </returns>
        internal static IExecutionStrategy GetExecutionStrategy(
            DbConnection connection,
            MetadataWorkspace metadataWorkspace)
        {
            var storeMetadata = (StoreItemCollection)metadataWorkspace.GetItemCollection(DataSpace.SSpace);

            return GetExecutionStrategy(connection, storeMetadata.StoreProviderFactory);
        }

        private static IExecutionStrategy GetExecutionStrategy(
            DbConnection connection,
            DbProviderFactory providerFactory)
        {
            var entityConnection = connection as EntityConnection;
            if (entityConnection != null)
            {
                connection = entityConnection.StoreConnection;
            }

            // Using the type name of DbProviderFactory implementation instead of the provider invariant name for performance
            var cacheKey = new ExecutionStrategyKey(providerFactory.GetType().FullName, connection.DataSource);

            var factory = _executionStrategyFactories.GetOrAdd(
                cacheKey,
                k =>
                DbConfiguration.GetService<Func<IExecutionStrategy>>(
                    new ExecutionStrategyKey(
                    DbConfiguration.GetService<IProviderInvariantName>(providerFactory).Name,
                    connection.DataSource)));
            return factory();
        }

        /// <summary>
        ///     Gets the spatial data reader for the <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" />.
        /// </summary>
        /// <returns>The spatial data reader.</returns>
        /// <param name="fromReader">The reader where the spatial data came from.</param>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        public DbSpatialDataReader GetSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            try
            {
                var spatialReader = GetDbSpatialDataReader(fromReader, manifestToken);
                if (spatialReader == null)
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
                }

                return spatialReader;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices, e);
                }
                throw;
            }
        }

        /// <summary>
        ///     Gets the spatial services for the <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" />.
        /// </summary>
        /// <returns>The spatial services.</returns>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        [Obsolete(
            "Use GetSpatialServices(DbProviderInfo) or DbConfiguration to ensure the configured spatial services are used. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information."
            )]
        public DbSpatialServices GetSpatialServices(string manifestToken)
        {
            return GetSpatialServicesInternal(manifestToken, throwIfNotImplemented: true);
        }

        private DbSpatialServices GetSpatialServicesInternal(string manifestToken, bool throwIfNotImplemented)
        {
            DbSpatialServices spatialProvider;
            try
            {
#pragma warning disable 612,618
                spatialProvider = DbGetSpatialServices(manifestToken);
#pragma warning restore 612,618
            }
            catch (ProviderIncompatibleException)
            {
                if (throwIfNotImplemented)
                {
                    throw;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices, e);
            }

            if (throwIfNotImplemented && spatialProvider == null)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
            }

            return spatialProvider;
        }

        internal static DbSpatialServices GetSpatialServices(IDbDependencyResolver resolver, EntityConnection connection)
        {
            DebugCheck.NotNull(resolver);
            DebugCheck.NotNull(connection);

            var storeItemCollection = (StoreItemCollection)connection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
            var key = new DbProviderInfo(
                storeItemCollection.StoreProviderInvariantName, storeItemCollection.StoreProviderManifestToken);

            return GetSpatialServices(resolver, key, () => GetProviderServices(connection.StoreConnection));
        }

        public DbSpatialServices GetSpatialServices(DbProviderInfo key)
        {
            DebugCheck.NotNull(key);

            return GetSpatialServices(_resolver.Value, key, () => this);
        }

        private static DbSpatialServices GetSpatialServices(
            IDbDependencyResolver resolver,
            DbProviderInfo key,
            Func<DbProviderServices> providerServices) // Delegate use to avoid lookup when not needed
        {
            DebugCheck.NotNull(resolver);
            DebugCheck.NotNull(key);
            DebugCheck.NotNull(providerServices);

            var services = _spatialServices.GetOrAdd(
                key,
                k =>
                resolver.GetService<DbSpatialServices>(k)
                ?? providerServices().GetSpatialServicesInternal(k.ProviderManifestToken, throwIfNotImplemented: false)
                ?? resolver.GetService<DbSpatialServices>());

            if (services == null)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
            }
            return services;
        }

        /// <summary>
        ///     Gets the spatial data reader for the <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" />.
        /// </summary>
        /// <returns>The spatial data reader.</returns>
        /// <param name="fromReader">The reader where the spatial data came from.</param>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        protected virtual DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            Check.NotNull(fromReader, "fromReader");

            // Must be a virtual method; abstract would break previous implementors of DbProviderServices
            throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
        }

        /// <summary>
        ///     Gets the spatial services for the <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" />.
        /// </summary>
        /// <returns>The spatial services.</returns>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        [Obsolete(
            "Return DbSpatialServices from the GetService method. See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.")]
        protected virtual DbSpatialServices DbGetSpatialServices(string manifestToken)
        {
            // Must be a virtual method; abstract would break previous implementors of DbProviderServices
            throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
        }

        internal void SetParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            DebugCheck.NotNull(parameter);
            DebugCheck.NotNull(parameterType);

            SetDbParameterValue(parameter, parameterType, value);
        }

        /// <summary>
        ///     Sets the parameter values for the <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" />.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        protected virtual void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(parameterType, "parameterType");

            parameter.Value = value;
        }

        /// <summary>Returns providers given a connection.</summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.DbProviderServices" /> instanced based on the specified connection.
        /// </returns>
        /// <param name="connection">Connection to provider.</param>
        public static DbProviderServices GetProviderServices(DbConnection connection)
        {
            return GetProviderFactory(connection).GetProviderServices();
        }

        /// <summary>Retrieves the DbProviderFactory based on the specified DbConnection.</summary>
        /// <returns>The retrieved DbProviderFactory.</returns>
        /// <param name="connection">The connection to use.</param>
        public static DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            Check.NotNull(connection, "connection");
            var factory = connection.GetProviderFactory();
            if (factory == null)
            {
                throw new ProviderIncompatibleException(
                    Strings.EntityClient_ReturnedNullOnProviderMethod(
                        "get_ProviderFactory",
                        connection.GetType().ToString()));
            }
            return factory;
        }

        /// <summary>
        ///     Return an XML reader which represents the CSDL description
        /// </summary>
        /// <returns> An XmlReader that represents the CSDL description </returns>
        public static XmlReader GetConceptualSchemaDefinition(string csdlName)
        {
            Check.NotEmpty(csdlName, "csdlName");

            return GetXmlResource("System.Data.Resources.DbProviderServices." + csdlName + ".csdl");
        }

        internal static XmlReader GetXmlResource(string resourceName)
        {
            DebugCheck.NotEmpty(resourceName);

            var executingAssembly = Assembly.GetExecutingAssembly();
            var stream = executingAssembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw Error.InvalidResourceName(resourceName);
            }

            return XmlReader.Create(stream, null, resourceName);
        }

        /// <summary>Generates a data definition language (DDL script that creates schema objects (tables, primary keys, foreign keys) based on the contents of the StoreItemCollection parameter and targeted for the version of the database corresponding to the provider manifest token.</summary>
        /// <remarks>
        ///     Individual statements should be separated using database-specific DDL command separator.
        ///     It is expected that the generated script would be executed in the context of existing database with
        ///     sufficient permissions, and it should not include commands to create the database, but it may include
        ///     commands to create schemas and other auxiliary objects such as sequences, etc.
        /// </remarks>
        /// <returns>A DDL script that creates schema objects based on the contents of the StoreItemCollection parameter and targeted for the version of the database corresponding to the provider manifest token.</returns>
        /// <param name="providerManifestToken">The provider manifest token identifying the target version.</param>
        /// <param name="storeItemCollection">The structure of the database.</param>
        public string CreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(providerManifestToken, "providerManifestToken");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            return DbCreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        /// <summary>Generates a data definition language (DDL script that creates schema objects (tables, primary keys, foreign keys) based on the contents of the StoreItemCollection parameter and targeted for the version of the database corresponding to the provider manifest token.</summary>
        /// <remarks>
        ///     Individual statements should be separated using database-specific DDL command separator.
        ///     It is expected that the generated script would be executed in the context of existing database with
        ///     sufficient permissions, and it should not include commands to create the database, but it may include
        ///     commands to create schemas and other auxiliary objects such as sequences, etc.
        /// </remarks>
        /// <returns>A DDL script that creates schema objects based on the contents of the StoreItemCollection parameter and targeted for the version of the database corresponding to the provider manifest token.</returns>
        /// <param name="providerManifestToken">The provider manifest token identifying the target version.</param>
        /// <param name="storeItemCollection">The structure of the database.</param>
        protected virtual string DbCreateDatabaseScript(
            string providerManifestToken,
            StoreItemCollection storeItemCollection)
        {
            Check.NotNull(providerManifestToken, "providerManifestToken");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabaseScript);
        }

        /// <summary>
        ///     Creates a database indicated by connection and creates schema objects
        ///     (tables, primary keys, foreign keys) based on the contents of storeItemCollection.
        /// </summary>
        /// <param name="connection">Connection to a non-existent database that needs to be created and populated with the store objects indicated with the storeItemCollection parameter.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to create the database.</param>
        /// <param name="storeItemCollection">The collection of all store items based on which the script should be created.</param>
        public void CreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            DbCreateDatabase(connection, commandTimeout, storeItemCollection);
        }

        /// <summary>Creates a database indicated by connection and creates schema objects (tables, primary keys, foreign keys) based on the contents of a StoreItemCollection.</summary>
        /// <param name="connection">Connection to a non-existent database that needs to be created and populated with the store objects indicated with the storeItemCollection parameter.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to create the database.</param>
        /// <param name="storeItemCollection">The collection of all store items based on which the script should be created.</param>
        protected virtual void DbCreateDatabase(
            DbConnection connection, int? commandTimeout,
            StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabase);
        }

        /// <summary>Returns a value indicating whether a given database exists on the server.</summary>
        /// <returns>True if the provider can deduce the database only based on the connection.</returns>
        /// <param name="connection">Connection to a database whose existence is checked by this method.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to determine the existence of the database.</param>
        /// <param name="storeItemCollection">The collection of all store items from the model. This parameter is no longer used for determining database existence.</param>
        public bool DatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                return DbDatabaseExists(connection, commandTimeout, storeItemCollection);
            }
        }

        /// <summary>Returns a value indicating whether a given database exists on the server.</summary>
        /// <returns>True if the provider can deduce the database only based on the connection.</returns>
        /// <param name="connection">Connection to a database whose existence is checked by this method.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to determine the existence of the database.</param>
        /// <param name="storeItemCollection">The collection of all store items from the model. This parameter is no longer used for determining database existence.</param>
        protected virtual bool DbDatabaseExists(
            DbConnection connection, int? commandTimeout,
            StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportDatabaseExists);
        }

        /// <summary>Deletes the specified database.</summary>
        /// <param name="connection">Connection to an existing database that needs to be deleted.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to delete the database.</param>
        /// <param name="storeItemCollection">The collection of all store items from the model. This parameter is no longer used for database deletion.</param>
        public void DeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            DbDeleteDatabase(connection, commandTimeout, storeItemCollection);
        }

        /// <summary>Deletes the specified database.</summary>
        /// <param name="connection">Connection to an existing database that needs to be deleted.</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to delete the database.</param>
        /// <param name="storeItemCollection">The collection of all store items from the model. This parameter is no longer used for database deletion.</param>
        protected virtual void DbDeleteDatabase(
            DbConnection connection, int? commandTimeout,
            StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportDeleteDatabase);
        }

        /// <summary>
        ///     Expands |DataDirectory| in the given path if it begins with |DataDirectory| and returns the expanded path,
        ///     or returns the given string if it does not start with |DataDirectory|.
        /// </summary>
        /// <param name="path"> The path to expand. </param>
        /// <returns> The expanded path. </returns>
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        public static string ExpandDataDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)
                || !path.StartsWith(DbConnectionOptions.DataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            // find the replacement path
            var rootFolderObject = AppDomain.CurrentDomain.GetData("DataDirectory");
            var rootFolderPath = rootFolderObject as string;
            if ((null != rootFolderObject)
                && (null == rootFolderPath))
            {
                throw new InvalidOperationException(Strings.ADP_InvalidDataDirectory);
            }

            if (rootFolderPath == String.Empty)
            {
                rootFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (null == rootFolderPath)
            {
                rootFolderPath = String.Empty;
            }

            // Make sure that the paths have exactly one "\" between them
            path = path.Substring(DbConnectionOptions.DataDirectory.Length);
            if (path.StartsWith(@"\", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            var fixedRoot = rootFolderPath.EndsWith(@"\", StringComparison.Ordinal)
                                ? rootFolderPath
                                : rootFolderPath + @"\";

            path = fixedRoot + path;

            // Verify root folder path is a real path without unexpected "..\"
            if (rootFolderPath.Contains(".."))
            {
                throw new ArgumentException(Strings.ExpandingDataDirectoryFailed);
            }

            return path;
        }

        /// <summary>
        ///     Adds an <see cref="IDbDependencyResolver" /> that will be used to resolve secondary provider
        ///     services when a derived type is registered as an EF provider either using an entry in the application's
        ///     config file or through code-based registration in <see cref="DbConfiguration" />.
        /// </summary>
        /// <param name="resolver">The resolver to add.</param>
        protected void AddDependencyResolver(IDbDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");

            _resolvers.Add(resolver);
        }

        /// <summary>
        ///     Called to resolve secondary provider services when a derived type is registered as an
        ///     EF provider either using an entry in the application's config file or through code-based
        ///     registration in <see cref="DbConfiguration" />. The implementation of this method in this
        ///     class uses the resolvers added with the AddDependencyResolver method to resolve
        ///     dependencies.
        /// </summary>
        /// <remarks>
        ///     Use this method to set, add, or change other provider-related services. Note that this method
        ///     will only be called for such services if they are not already explicitly configured in some
        ///     other way by the application. This allows providers to set default services while the
        ///     application is still able to override and explicitly configure each service if required.
        ///     See <see cref="IDbDependencyResolver" /> and <see cref="DbConfiguration" /> for more details.
        /// </remarks>
        /// <param name="type">The type of the service to be resolved.</param>
        /// <param name="key">An optional key providing additional information for resolving the service.</param>
        /// <returns>An instance of the given type, or null if the service could not be resolved.</returns>
        public virtual object GetService(Type type, object key)
        {
            return _resolvers.GetService(type, key);
        }

        /// <summary>
        ///     Called to resolve secondary provider services when a derived type is registered as an
        ///     EF provider either using an entry in the application's config file or through code-based
        ///     registration in <see cref="DbConfiguration" />. The implementation of this method in this
        ///     class uses the resolvers added with the AddDependencyResolver method to resolve
        ///     dependencies.
        /// </summary>
        /// <param name="type">The type of the service to be resolved.</param>
        /// <param name="key">An optional key providing additional information for resolving the service.</param>
        /// <returns>All registered services that satisfy the given type and key, or an empty enumeration if there are none.
        public virtual IEnumerable<object> GetServices(Type type, object key)
        {
            return _resolvers.GetServices(type, key);
        }
    }
}
