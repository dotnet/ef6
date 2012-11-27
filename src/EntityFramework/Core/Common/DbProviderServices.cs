// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    ///     The factory for building command definitions; use the type of this object
    ///     as the argument to the IServiceProvider.GetService method on the provider
    ///     factory;
    /// </summary>
    [CLSCompliant(false)]
    public abstract class DbProviderServices
    {
        private readonly Lazy<IDbDependencyResolver> _resolver;

        /// <summary>
        ///     Constructs an EF provider that will use the <see cref="IDbDependencyResolver" /> obtained from
        ///     the app domain <see cref="DbConfiguration" /> Singleton for resolving EF dependencies such
        ///     as the <see cref="DbSpatialServices" /> instance to use.
        /// </summary>
        protected DbProviderServices()
        {
            _resolver = new Lazy<IDbDependencyResolver>(() => DbConfiguration.DependencyResolver);
        }

        /// <summary>
        ///     Constructs an EF provider that will use the given <see cref="IDbDependencyResolver" /> for
        ///     resolving EF dependencies such as the <see cref="DbSpatialServices" /> instance to use.
        /// </summary>
        /// <param name="resolver"> The resolver to use. </param>
        protected DbProviderServices(IDbDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");

            _resolver = new Lazy<IDbDependencyResolver>(() => resolver);
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
            ValidateDataSpace(commandTree);
            var storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            Debug.Assert(storeMetadata.StoreProviderManifest != null, "StoreItemCollection has null StoreProviderManifest?");

            return CreateDbCommandDefinition(storeMetadata.StoreProviderManifest, commandTree);
        }

        /// <summary>
        ///     Create a Command Definition object given a command tree.
        /// </summary>
        /// <param name="commandTree"> command tree for the statement </param>
        /// <returns> an executable command definition object </returns>
        /// <remarks>
        ///     This method simply delegates to the provider's implementation of CreateDbCommandDefinition.
        /// </remarks>
        public DbCommandDefinition CreateCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
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

        /// <summary>
        ///     Create a Command Definition object, given the provider manifest and command tree
        /// </summary>
        /// <param name="connection"> provider manifest previously retrieved from the store provider </param>
        /// <param name="commandTree"> command tree for the statement </param>
        /// <returns> an executable command definition object </returns>
        protected abstract DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree);

        /// <summary>
        ///     Ensures that the data space of the specified command tree is the target (S-) space
        /// </summary>
        /// <param name="commandTree"> The command tree for which the data space should be validated </param>
        internal virtual void ValidateDataSpace(DbCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            if (commandTree.DataSpace
                != DataSpace.SSpace)
            {
                throw new ProviderIncompatibleException(Strings.ProviderRequiresStoreCommandTree);
            }
        }

        /// <summary>
        ///     Create a DbCommand object given a command tree.
        /// </summary>
        /// <param name="commandTree"> command tree for the statement </param>
        /// <returns> a command object </returns>
        internal virtual DbCommand CreateCommand(DbCommandTree commandTree)
        {
            var commandDefinition = CreateCommandDefinition(commandTree);
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

        /// <summary>
        ///     Retrieve the provider manifest token based on the specified connection.
        /// </summary>
        /// <param name="connection"> The connection for which the provider manifest token should be retrieved. </param>
        /// <returns> The provider manifest token that describes the specified connection, as determined by the provider. </returns>
        /// <remarks>
        ///     This method simply delegates to the provider's implementation of GetDbProviderManifestToken.
        /// </remarks>
        public string GetProviderManifestToken(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            try
            {
                var providerManifestToken = GetDbProviderManifestToken(connection);
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

        protected abstract string GetDbProviderManifestToken(DbConnection connection);

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

        protected abstract DbProviderManifest GetDbProviderManifest(string manifestToken);

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

        public DbSpatialServices GetSpatialServices(string manifestToken)
        {
            return GetSpatialServicesInternal(_resolver, manifestToken);
        }

        internal DbSpatialServices GetSpatialServicesInternal(Lazy<IDbDependencyResolver> resolver, string manifestToken)
        {
            // First check if spatial services can be resolved and only if this fails
            // go on to ask the provider for spatial services.
            var spatialProvider = resolver.Value.GetService<DbSpatialServices>();
            if (spatialProvider != null)
            {
                return spatialProvider;
            }

            try
            {
                spatialProvider = DbGetSpatialServices(manifestToken);
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices, e);
            }

            if (spatialProvider == null)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
            }

            return spatialProvider;
        }

        protected virtual DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            Check.NotNull(fromReader, "fromReader");

            // Must be a virtual method; abstract would break previous implementors of DbProviderServices
            throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
        }

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

        protected virtual void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(parameterType, "parameterType");

            parameter.Value = value;
        }

        /// <summary>
        ///     Create an instance of DbProviderServices based on the supplied DbConnection
        /// </summary>
        /// <param name="connection"> The DbConnection to use </param>
        /// <returns> An instance of DbProviderServices </returns>
        public static DbProviderServices GetProviderServices(DbConnection connection)
        {
            return GetProviderFactory(connection).GetProviderServices();
        }

        internal static DbProviderFactory GetProviderFactory(string providerInvariantName)
        {
            DebugCheck.NotNull(providerInvariantName);
            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(providerInvariantName);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(Strings.EntityClient_InvalidStoreProvider, e);
            }
            return factory;
        }

        /// <summary>
        ///     Retrieve the DbProviderFactory based on the specified DbConnection
        /// </summary>
        /// <param name="connection"> The DbConnection to use </param>
        /// <returns> An instance of DbProviderFactory </returns>
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
        internal static XmlReader GetConceptualSchemaDefinition(string csdlName)
        {
            return GetXmlResource("System.Data.Resources.DbProviderServices." + csdlName + ".csdl");
        }

        internal static XmlReader GetXmlResource(string resourceName)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var stream = executingAssembly.GetManifestResourceStream(resourceName);
            return XmlReader.Create(stream, null, resourceName);
        }

        /// <summary>
        ///     Generates a DDL script which creates schema objects (tables, primary keys, foreign keys)
        ///     based on the contents of the storeItemCollection and targeted for the version of the backend corresponding to
        ///     the providerManifestToken.
        ///     Individual statements should be separated using database-specific DDL command separator.
        ///     It is expected that the generated script would be executed in the context of existing database with
        ///     sufficient permissions, and it should not include commands to create the database, but it may include
        ///     commands to create schemas and other auxiliary objects such as sequences, etc.
        /// </summary>
        /// <param name="providerManifestToken"> The provider manifest token identifying the target version </param>
        /// <param name="storeItemCollection"> The collection of all store items based on which the script should be created </param>
        /// <returns> A DDL script which creates schema objects based on contents of storeItemCollection and targeted for the version of the backend corresponding to the providerManifestToken. </returns>
        public string CreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(providerManifestToken, "providerManifestToken");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            return DbCreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        protected virtual string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(providerManifestToken, "providerManifestToken");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabaseScript);
        }

        /// <summary>
        ///     Creates a database indicated by connection and creates schema objects
        ///     (tables, primary keys, foreign keys) based on the contents of storeItemCollection.
        /// </summary>
        /// <param name="connection"> Connection to a non-existent database that needs to be created and be populated with the store objects indicated by the storeItemCollection </param>
        /// <param name="commandTimeout"> Execution timeout for any commands needed to create the database. </param>
        /// <param name="storeItemCollection">
        ///     The collection of all store items based on which the script should be created < </param>
        public void CreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            DbCreateDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected virtual void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportCreateDatabase);
        }

        /// <summary>
        ///     Returns a value indicating whether given database exists on the server
        ///     and/or whether schema objects contained in teh storeItemCollection have been created.
        ///     If the provider can deduct the database only based on the connection, they do not need
        ///     to additionally verify all elements of the storeItemCollection.
        /// </summary>
        /// <param name="connection"> Connection to a database whose existence is checked by this method </param>
        /// <param name="commandTimeout"> Execution timeout for any commands needed to determine the existence of the database </param>
        /// <param name="storeItemCollection">
        ///     The collection of all store items contained in the database whose existence is determined by this method < </param>
        /// <returns> Whether the database indicated by the connection and the storeItemCollection exist </returns>
        public bool DatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            return DbDatabaseExists(connection, commandTimeout, storeItemCollection);
        }

        protected virtual bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            throw new ProviderIncompatibleException(Strings.ProviderDoesNotSupportDatabaseExists);
        }

        /// <summary>
        ///     Deletes all store objects specified in the store item collection from the database and the database itself.
        /// </summary>
        /// <param name="connection"> Connection to an existing database that needs to be deleted </param>
        /// <param name="commandTimeout"> Execution timeout for any commands needed to delete the database </param>
        /// <param name="storeItemCollection">
        ///     The collection of all store items contained in the database that should be deleted < </param>
        public void DeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(storeItemCollection, "storeItemCollection");

            DbDeleteDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected virtual void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
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
            if (!EntityUtil.GetFullPath(path).StartsWith(rootFolderPath, StringComparison.Ordinal))
            {
                throw new ArgumentException(Strings.ExpandingDataDirectoryFailed);
            }

            return path;
        }
    }
}
