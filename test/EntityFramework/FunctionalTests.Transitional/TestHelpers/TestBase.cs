// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Xml.Linq;

    public class TestBase : MarshalByRefObject
    {
        static TestBase()
        {
            DbConfiguration.SetConfiguration(new FunctionalTestsConfiguration());

            // Uncomment below to log all test generated SQL to the console.
            //DbInterception.Add(new DatabaseLogFormatter(Console.Write));
        }

        internal DbDatabaseMapping BuildMapping(DbModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
            // Build and clone to check for idempotency issues.

            modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var clone = modelBuilder.Clone();

            return clone.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
        }

        protected virtual void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // For fixture wide configuration
        }

        internal DbDatabaseMapping BuildCeMapping(DbModelBuilder modelBuilder)
        {
            // Build and clone to check for idempotency issues.

            modelBuilder.Build(ProviderRegistry.SqlCe4_ProviderInfo);

            var clone = modelBuilder.Clone();

            return clone.Build(ProviderRegistry.SqlCe4_ProviderInfo).DatabaseMapping;
        }

        protected static DataRow CreateProviderRow(string name, string invariantName, string assemblyQualifiedName)
        {
            var table = new DataTable();
            table.Columns.AddRange(
                new[]
                    {
                        new DataColumn("Name", typeof(string)),
                        new DataColumn("Description", typeof(string)),
                        new DataColumn("InvariantName", typeof(string)),
                        new DataColumn("AssemblyQualifiedName", typeof(string))
                    });

            var row = table.NewRow();
            row["Name"] = name;
            row["Description"] = "Name: " + name + " Invariant: " + invariantName;
            row["InvariantName"] = invariantName;
            row["AssemblyQualifiedName"] = assemblyQualifiedName;
            return row;
        }

        protected static void RunTestWithTempMetadata(string csdl, string ssdl, string msl, Action<IEnumerable<string>> test)
        {
            var paths = new[]
                {
                    Path.GetTempFileName() + ".ssdl",
                    Path.GetTempFileName() + ".csdl",
                    Path.GetTempFileName() + ".msl"
                };
            var metadata = new[]
                {
                    ssdl,
                    csdl,
                    msl
                };
            try
            {
                for (var i = 0; i < metadata.Length; i++)
                {
                    using (var file = File.CreateText(paths[i]))
                    {
                        file.Write(metadata[i]);
                    }
                }
                test(paths);
            }
            finally
            {
                foreach (var path in paths)
                {
                    try
                    {
                        File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
                        File.Delete(path);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
            }
        }

        #region Assemblies and exceptions

        /// <summary>
        /// The assembly containing Code First and the Productivity API.
        /// </summary>
        public static Assembly EntityFrameworkAssembly
        {
            get { return typeof(DbModelBuilder).Assembly(); }
        }

        /// <summary>
        /// System.ComponentModel.DataAnnotations.
        /// </summary>
        public static Assembly SystemComponentModelDataAnnotationsAssembly
        {
            get { return typeof(ValidationAttribute).Assembly(); }
        }

        /// <summary>
        /// EntityFramework.SqlServer
        /// </summary>
        public static Assembly EntityFrameworkSqlServerAssembly
        {
            get { return typeof(SqlProviderServices).Assembly(); }
        }

        /// <summary>
        /// Gets an embedded resource string from the specified assembly
        /// </summary>
        public static string LookupString(Assembly assembly, string resourceTable, string resourceKey)
        {
            return new AssemblyResourceLookup(assembly, resourceTable).LookupString(resourceKey);
        }

        /// <summary>
        /// Executes the given delegate and returns the exception that it throws.
        /// </summary>
        protected Exception GenerateException(Action willThrow)
        {
            return ExceptionHelpers.GenerateException(willThrow);
        }

        /// <summary>
        /// Executes the given test multiple times in parallel.
        /// </summary>
        /// <param name="test"> The test. </param>
        /// <param name="count"> The number of copies to run in parallel. </param>
        protected static void ExecuteInParallel(Action test, int count = 20)
        {
            var tests = new Action[count];
            for (var i = 0; i < count; i++)
            {
                tests[i] = test;
            }
            Parallel.Invoke(tests);
        }

        #endregion

        #region GetObjectContext

        /// <summary>
        /// Returns the ObjectContext for the given DbContext.
        /// </summary>
        public static ObjectContext GetObjectContext(DbContext context)
        {
            return ((IObjectContextAdapter)context).ObjectContext;
        }

        #endregion

        #region State entry helpers

        /// <summary>
        /// Gets all GetStateEntries for the given DbContext,.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <returns> All state entries in the ObjectStateManager. </returns>
        protected static IEnumerable<ObjectStateEntry> GetStateEntries(DbContext dbContext)
        {
            return ModelHelpers.GetStateEntries(dbContext);
        }

        /// <summary>
        /// Gets all GetStateEntries for the given ObjectContext,.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <returns> All state entries in the ObjectStateManager. </returns>
        protected static IEnumerable<ObjectStateEntry> GetStateEntries(ObjectContext objectContext)
        {
            return ModelHelpers.GetStateEntries(objectContext);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        /// <returns> The ObjectStateEntry. </returns>
        protected static ObjectStateEntry GetStateEntry(DbContext dbContext, object entity)
        {
            return ModelHelpers.GetStateEntry(dbContext, entity);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        /// <returns> The ObjectStateEntry. </returns>
        protected static ObjectStateEntry GetStateEntry(ObjectContext objectContext, object entity)
        {
            return ModelHelpers.GetStateEntry(objectContext, entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        public static void AssertNoStateEntry(DbContext dbContext, object entity)
        {
            ModelHelpers.AssertNoStateEntry(dbContext, entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        public static void AssertNoStateEntry(ObjectContext objectContext, object entity)
        {
            ModelHelpers.AssertNoStateEntry(objectContext, entity);
        }

        #endregion

        #region Connection helpers

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine with the given database name.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <returns> The connection string. </returns>
        protected static string SimpleConnectionString(string databaseName)
        {
            return ModelHelpers.SimpleConnectionString(databaseName);
        }

        /// <summary>
        /// Returns a simple SQL Server connection string with the specified credentials.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        protected static string SimpleConnectionStringWithCredentials(
            string databaseName,
            string userId,
            string password,
            bool persistSecurityInfo = false)
        {
            return ModelHelpers.SimpleConnectionStringWithCredentials(
                databaseName,
                userId,
                password,
                persistSecurityInfo);
        }

        /// <summary>
        /// Returns the default name that will be created for the context of the given type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a name for. </typeparam>
        /// <returns> The name. </returns>
        protected static string DefaultDbName<TContext>() where TContext : DbContext
        {
            return ModelHelpers.DefaultDbName<TContext>();
        }

        /// <summary>
        /// Returns the transaction count from the server.
        /// </summary>
        /// <param name="connection"> Database connection to connect against. </param>
        /// <returns> The transaction count. </returns>
        protected int GetTransactionCount(DbConnection connection)
        {
            var closeconn = false;

            if (connection.State
                != ConnectionState.Open)
            {
                connection.Open();
                closeconn = true;
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = "select @@TranCount";
            var trancount = (int)cmd.ExecuteScalar();

            if (closeconn)
            {
                connection.Close();
            }

            return trancount;
        }

        /// <summary>
        /// Returns a local transaction.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns> </returns>
        protected DbTransaction BeginLocalTransaction(DbContext context)
        {
            return OpenEntityConnection(context).BeginTransaction();
        }

        /// <summary>
        /// Opens the underlying <see cref="EntityConnection" /> obtained from the underlying
        /// <see cref="ObjectContext" /> and creates a new <see cref="CommittableTransaction" />.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns> </returns>
        protected CommittableTransaction BeginCommittableTransaction(DbContext context)
        {
            OpenEntityConnection(context);

            return new CommittableTransaction();
        }

        /// <summary>
        /// Opens the underlying <see cref="EntityConnection" /> obtained from the underlying
        /// <see cref="ObjectContext" /> and returns it.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> The connection. </returns>
        private static EntityConnection OpenEntityConnection(DbContext context)
        {
            var connection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;
            if (connection.State
                != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection;
        }

        /// <summary>
        /// Closes the underlying <see cref="EntityConnection" /> obtained from the underlying
        /// <see cref="ObjectContext" /> if the connection is open.
        /// </summary>
        protected void CloseEntityConnection(DbContext context)
        {
            var connection = ((IObjectContextAdapter)context).ObjectContext.Connection;
            if (connection.State
                == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection string for. </typeparam>
        /// <returns> The connection string. </returns>
        protected static string SimpleConnectionString<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleConnectionString<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine for the given context type
        /// with the specified credentials.
        /// </summary>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleConnectionStringWithCredentials<TContext>(
            string userId,
            string password,
            bool persistSecurityInfo = false)
            where TContext : DbContext
        {
            return ModelHelpers.SimpleConnectionStringWithCredentials<TContext>(
                userId,
                password,
                persistSecurityInfo);
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using attached database
        /// for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection string for. </typeparam>
        /// <param name="useInitialCatalog">
        /// Specifies whether the InitialCatalog should be created from the context name.
        /// </param>
        /// <returns> The connection string. </returns>
        protected static string SimpleAttachConnectionString<TContext>(bool useInitialCatalog = true) where TContext : DbContext
        {
            return ModelHelpers.SimpleAttachConnectionString<TContext>(useInitialCatalog);
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using an attachable database
        /// for the given context type with the specified credentials.
        /// </summary>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleAttachConnectionStringWithCredentials<TContext>(
            string userId,
            string password,
            bool persistSecurityInfo = false) where TContext : DbContext
        {
            return ModelHelpers.SimpleAttachConnectionStringWithCredentials<TContext>(
                userId,
                password,
                persistSecurityInfo);
        }

        /// <summary>
        /// Returns a simple SQLCE connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context. </typeparam>
        /// <returns> The connection string. </returns>
        protected static string SimpleCeConnectionString<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleCeConnectionString<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL Server connection to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection for. </typeparam>
        /// <returns> The connection. </returns>
        protected static SqlConnection SimpleConnection<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleConnection<TContext>();
        }

        /// <summary>
        /// Returns a simple SQL CE connection for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection for. </typeparam>
        /// <returns> The connection. </returns>
        protected static DbConnection SimpleCeConnection<TContext>() where TContext : DbContext
        {
            return ModelHelpers.SimpleCeConnection<TContext>();
        }

        #endregion

        #region Entity set name helpers

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="dbContext"> The context to look in. </param>
        /// <param name="clrType"> The type to lookup. </param>
        /// <returns> The entity set name. </returns>
        protected static string GetEntitySetName(DbContext dbContext, Type clrType)
        {
            return ModelHelpers.GetEntitySetName(dbContext, clrType);
        }

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="objetContext"> The context to look in. </param>
        /// <param name="clrType"> The type to lookup. </param>
        /// <returns> The entity set name. </returns>
        protected static string GetEntitySetName(ObjectContext objetContext, Type clrType)
        {
            return ModelHelpers.GetEntitySetName(objetContext, clrType);
        }

        #endregion

        #region Entity Type helpers

        /// <summary>
        /// Gets the EntityType of the given CLR type
        /// </summary>
        /// <param name="dbContext"> The context to look in. </param>
        /// <param name="clrType"> The CLR type </param>
        /// <returns> The entity type corresponding to the CLR type </returns>
        protected static EntityType GetEntityType(DbContext dbContext, Type clrType)
        {
            return ModelHelpers.GetEntityType(dbContext, clrType);
        }

        /// <summary>
        /// Gets the EntityType of the given CLR type
        /// </summary>
        /// <param name="objectContext"> The context to look in. </param>
        /// <param name="clrType"> The CLR type </param>
        /// <returns> The entity type corresponding to the CLR type </returns>
        protected static EntityType GetEntityType(ObjectContext objectContext, Type clrType)
        {
            return ModelHelpers.GetStructuralType<EntityType>(objectContext, clrType);
        }

        #endregion

        #region Creating config documents

        public static Configuration CreateEmptyConfig()
        {
            var tempFileName = Path.GetTempFileName();
            var doc = new XDocument(new XElement("configuration"));
            doc.Save(tempFileName);

            var config = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = tempFileName
                    },
                ConfigurationUserLevel.None);

            config.Sections.Add("entityFramework", new EntityFrameworkSection());

            return config;
        }

        #endregion
    }
}
