// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    ///     Base class for decorators that wrap the core <see cref = "DbMigrator" />
    /// </summary>
    [DebuggerStepThrough]
    public abstract class MigratorBase
    {
        private MigratorBase _this;

        /// <summary>
        ///     Initializes a new instance of the MigratorBase class.
        /// </summary>
        /// <param name = "innerMigrator">The migrator that this decorator is wrapping.</param>
        protected MigratorBase(MigratorBase innerMigrator)
        {
            if (innerMigrator == null)
            {
                _this = this;
            }
            else
            {
                _this = innerMigrator;

                var nextMigrator = innerMigrator;

                while (nextMigrator._this != innerMigrator)
                {
                    nextMigrator = nextMigrator._this;
                }

                nextMigrator._this = this;
            }
        }

        /// <summary>
        ///     Gets a list of the pending migrations that have not been applied to the database.
        /// </summary>
        /// <returns>List of migration Ids</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetPendingMigrations()
        {
            return _this.GetPendingMigrations();
        }

        /// <summary>
        ///     Gets the configuration being used for the migrations process.
        /// </summary>
        public virtual DbMigrationsConfiguration Configuration
        {
            get { return _this.Configuration; }
        }

        /// <summary>
        ///     Updates the target database to the latest migration.
        /// </summary>
        public void Update()
        {
            Update(null);
        }

        /// <summary>
        ///     Updates the target database to a given migration.
        /// </summary>
        /// <param name = "targetMigration">The migration to upgrade/downgrade to.</param>
        public virtual void Update(string targetMigration)
        {
            _this.Update(targetMigration);
        }

        internal virtual string GetMigrationId(string migration)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migration));
            Contract.Requires(migration != Strings.AutomaticMigration);

            return _this.GetMigrationId(migration);
        }

        /// <summary>
        ///     Gets a list of the migrations that are defined in the assembly.
        /// </summary>
        /// <returns>List of migration Ids</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetLocalMigrations()
        {
            return _this.GetLocalMigrations();
        }

        /// <summary>
        ///     Gets a list of the migrations that have been applied to the database.
        /// </summary>
        /// <returns>List of migration Ids</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDatabaseMigrations()
        {
            return _this.GetDatabaseMigrations();
        }

        internal virtual void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            Contract.Requires(targetModel != null);

            _this.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
        }

        internal virtual void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            Contract.Requires(migration != null);

            _this.ApplyMigration(migration, lastMigration);
        }

        internal virtual void EnsureDatabaseExists()
        {
            _this.EnsureDatabaseExists();
        }

        internal virtual void RevertMigration(string migrationId, DbMigration migration, XDocument sourceModel, XDocument targetModel)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));
            Contract.Requires(migration != null);
            Contract.Requires(sourceModel != null);
            Contract.Requires(targetModel != null);

            _this.RevertMigration(migrationId, migration, sourceModel, targetModel);
        }

        internal virtual void SeedDatabase()
        {
            _this.SeedDatabase();
        }

        internal virtual void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            Contract.Requires(migrationStatements != null);

            _this.ExecuteStatements(migrationStatements);
        }

        internal virtual void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            Contract.Requires(transaction != null);
            Contract.Requires(migrationStatement != null);

            _this.ExecuteSql(transaction, migrationStatement);
        }

        internal virtual void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
            Contract.Requires(pendingMigrations != null);

            _this.Upgrade(pendingMigrations, targetMigrationId, lastMigrationId);
        }

        internal virtual void Downgrade(IEnumerable<string> pendingMigrations)
        {
            Contract.Requires(pendingMigrations != null);
            Contract.Requires(pendingMigrations.Count() > 1);

            _this.Downgrade(pendingMigrations);
        }

        internal virtual void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
        {
            Contract.Requires(upgradeOperations != null);

            _this.UpgradeHistory(upgradeOperations);
        }

        internal virtual DbMigration GetMigration(string migrationId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(migrationId));

            return _this.GetMigration(migrationId);
        }

        internal virtual string TargetDatabase
        {
            get { return _this.TargetDatabase; }
        }
    }
}
