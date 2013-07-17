// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Handles creating databases either using the core provider or the Migrations pipeline.
    /// </summary>
    internal class DatabaseCreator
    {
        private readonly IDbDependencyResolver _resolver;
        private readonly MigrationsChecker _migrationsChecker;

        public DatabaseCreator()
            : this(DbConfiguration.DependencyResolver)
        {
        }

        public DatabaseCreator(IDbDependencyResolver resolver, MigrationsChecker migrationsChecker = null)
        {
            DebugCheck.NotNull(resolver);

            _resolver = resolver;
            _migrationsChecker = migrationsChecker ?? new MigrationsChecker();
        }

        /// <summary>
        ///     Creates a database using the core provider (i.e. ObjectContext.CreateDatabase) or
        ///     by using Code First Migrations <see cref="DbMigrator" /> to create an empty database
        ///     and the perform an automatic migration to the current model.
        /// </summary>
        public virtual void CreateDatabase(
            InternalContext internalContext,
            Func<DbMigrationsConfiguration, DbContext, MigratorBase> createMigrator,
            ObjectContext objectContext)
        {
            DebugCheck.NotNull(internalContext);
            DebugCheck.NotNull(createMigrator);
            // objectContext may be null when testing.

            if (internalContext.CodeFirstModel != null
                && _resolver.GetService<Func<MigrationSqlGenerator>>(internalContext.ProviderName) != null)
            {
                if (!_migrationsChecker.IsMigrationsConfigured(internalContext, () => false))
                {
                    var migrator = createMigrator(
                        GetMigrationsConfiguration(internalContext),
                        internalContext.Owner);

                    migrator.Update();
                }
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

        public static DbMigrationsConfiguration GetMigrationsConfiguration(InternalContext internalContext)
        {
            DebugCheck.NotNull(internalContext);

            var contextType = internalContext.Owner.GetType();

            return new DbMigrationsConfiguration
                {
                    ContextType = contextType,
                    AutomaticMigrationsEnabled = true,
                    MigrationsAssembly = contextType.Assembly,
                    MigrationsNamespace = contextType.Namespace,
                    ContextKey = internalContext.ContextKey,
                    TargetDatabase = new DbConnectionInfo(internalContext.OriginalConnectionString, internalContext.ProviderName),
                    CommandTimeout = internalContext.CommandTimeout
                };
        }
    }
}
