// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// An implementation of <see cref="IDatabaseInitializer{TContext}" /> that will use Code First Migrations
    /// to update the database to the latest version.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TMigrationsConfiguration">The type of the migrations configuration to use during initialization.</typeparam>
    public class MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration> : IDatabaseInitializer<TContext>
        where TContext : DbContext
        where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        private readonly DbMigrationsConfiguration _config;
        private readonly bool _useSuppliedContext;

        static MigrateDatabaseToLatestVersion()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        /// Initializes a new instance of the MigrateDatabaseToLatestVersion class that will use
        /// the connection information from a context constructed using the default constructor 
        /// or registered factory if applicable
        /// </summary>
        public MigrateDatabaseToLatestVersion()
            : this(useSuppliedContext: false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the MigrateDatabaseToLatestVersion class specifying whether to
        /// use the connection information from the context that triggered initialization to perform the migration.
        /// </summary>
        /// <param name="useSuppliedContext">
        /// If set to <c>true</c> the initializer is run using the connection information from the context that 
        /// triggered initialization. Otherwise, the connection information will be taken from a context constructed 
        /// using the default constructor or registered factory if applicable. 
        /// </param>
        public MigrateDatabaseToLatestVersion(bool useSuppliedContext)
        {
            _config = new TMigrationsConfiguration();
            _useSuppliedContext = useSuppliedContext;
        }

        /// <summary>
        /// Initializes a new instance of the MigrateDatabaseToLatestVersion class that will
        /// use a specific connection string from the configuration file to connect to
        /// the database to perform the migration.
        /// </summary>
        /// <param name="connectionStringName"> The name of the connection string to use for migration. </param>
        public MigrateDatabaseToLatestVersion(string connectionStringName)
        {
            Check.NotEmpty(connectionStringName, "connectionStringName");

            _config = new TMigrationsConfiguration
                {
                    TargetDatabase = new DbConnectionInfo(connectionStringName)
                };
        }

        /// <inheritdoc />
        public virtual void InitializeDatabase(TContext context)
        {
            Check.NotNull(context, "context");

            var migrator = new DbMigrator(_config, _useSuppliedContext ? context : null);
            migrator.Update();
        }
    }
}
