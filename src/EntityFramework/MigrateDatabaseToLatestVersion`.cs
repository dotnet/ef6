namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// An implementation of <see cref="IDatabaseInitializer{TContext}"/> that will use Code First Migrations
    /// to update the database to the latest version.
    /// </summary>
    public class MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration> : IDatabaseInitializer<TContext>
        where TContext : DbContext
        where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        private readonly DbMigrationsConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the MigrateDatabaseToLatestVersion class. 
        /// </summary>
        public MigrateDatabaseToLatestVersion()
        {
            _config = new TMigrationsConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the MigrateDatabaseToLatestVersion class that will
        /// use a specific connection string from the configuration file to connect to
        /// the database to perform the migration.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string to use for migration.</param>
        public MigrateDatabaseToLatestVersion(string connectionStringName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(connectionStringName));

            _config = new TMigrationsConfiguration
                          {
                              TargetDatabase = new DbConnectionInfo(connectionStringName)
                          };
        }

        /// <inheritdoc/>
        public void InitializeDatabase(TContext context)
        {
            var migrator = new DbMigrator(_config);
            migrator.Update();
        }
    }
}
