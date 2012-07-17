namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Sql;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Handles creating databases either using the core provider or the Migrations pipeline.
    /// </summary>
    internal class DatabaseCreator
    {
        private readonly Lazy<IDbDependencyResolver> _resolver;

        public DatabaseCreator()
            : this(new Lazy<IDbDependencyResolver>(() => DbConfiguration.Instance.DependencyResolver))
        {
        }

        public DatabaseCreator(Lazy<IDbDependencyResolver> resolver)
        {
            Contract.Requires(resolver != null);

            _resolver = resolver;
        }

        /// <summary>
        ///     Creates a database using the core provider (i.e. ObjectContext.CreateDatabase) or
        ///     by using Code First Migrations <see cref = "DbMigrator" /> to create an empty database
        ///     and the perform an automatic migration to the current model.
        ///     Migrations is used if Code First is being used and the EF provider is for SQL Server
        ///     or SQL Compact. The core is used for non-Code First models and for other providers even
        ///     when using Code First.
        /// </summary>
        public virtual void CreateDatabase(
            InternalContext internalContext,
            Func<DbMigrationsConfiguration, DbContext, DbMigrator> createMigrator,
            ObjectContext objectContext)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(createMigrator != null);
            // objectContext may be null when testing.

            var sqlGenerator = _resolver.Value.GetService<MigrationSqlGenerator>(internalContext.ProviderName);

            if (internalContext.CodeFirstModel != null
                && sqlGenerator != null)
            {
                var contextType = internalContext.Owner.GetType();

                var migrator = createMigrator(
                    new DbMigrationsConfiguration
                        {
                            ContextType = contextType,
                            AutomaticMigrationsEnabled = true,
                            MigrationsAssembly = contextType.Assembly,
                            MigrationsNamespace = contextType.Namespace,
                            TargetDatabase =
                                new DbConnectionInfo(
                                    internalContext.OriginalConnectionString, internalContext.ProviderName)
                        },
                    internalContext.Owner);

                migrator.Update();
            }
            else
            {
                internalContext.DatabaseOperations.Create(objectContext);
                internalContext.SaveMetadataToDatabase();
            }

            // If the database is created explicitly, then this is treated as overriding the
            // database initialization strategy, so make it as already run.
            internalContext.MarkDatabaseInitialized();
        }
    }
}
