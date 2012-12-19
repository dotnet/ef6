// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    ///     Base class for decorators that wrap the core <see cref="DbMigrator" />
    /// </summary>
    [DebuggerStepThrough]
    public abstract class MigratorBase
    {
        private MigratorBase _this;

        /// <summary>
        ///     Initializes a new instance of the MigratorBase class.
        /// </summary>
        /// <param name="innerMigrator"> The migrator that this decorator is wrapping. </param>
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
        /// <returns> List of migration Ids </returns>
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
        /// <param name="targetMigration"> The migration to upgrade/downgrade to. </param>
        public virtual void Update(string targetMigration)
        {
            _this.Update(targetMigration);
        }

        internal virtual string GetMigrationId(string migration)
        {
            DebugCheck.NotEmpty(migration);
            Debug.Assert(migration != Strings.AutomaticMigration);

            return _this.GetMigrationId(migration);
        }

        /// <summary>
        ///     Gets a list of the migrations that are defined in the assembly.
        /// </summary>
        /// <returns> List of migration Ids </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetLocalMigrations()
        {
            return _this.GetLocalMigrations();
        }

        /// <summary>
        ///     Gets a list of the migrations that have been applied to the database.
        /// </summary>
        /// <returns> List of migration Ids </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDatabaseMigrations()
        {
            return _this.GetDatabaseMigrations();
        }

        internal virtual void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            Check.NotNull(targetModel, "targetModel");

            _this.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
        }

        internal virtual void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            DebugCheck.NotNull(migration);

            _this.ApplyMigration(migration, lastMigration);
        }

        internal virtual void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
        {
            _this.EnsureDatabaseExists(mustSucceedToKeepDatabase);
        }

        internal virtual void RevertMigration(string migrationId, DbMigration migration, XDocument sourceModel, XDocument targetModel)
        {
            DebugCheck.NotEmpty(migrationId);
            DebugCheck.NotNull(migration);
            DebugCheck.NotNull(sourceModel);
            DebugCheck.NotNull(targetModel);

            _this.RevertMigration(migrationId, migration, sourceModel, targetModel);
        }

        internal virtual void SeedDatabase()
        {
            _this.SeedDatabase();
        }

        internal virtual void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            DebugCheck.NotNull(migrationStatements);

            _this.ExecuteStatements(migrationStatements);
        }

        internal virtual void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            DebugCheck.NotNull(transaction);
            DebugCheck.NotNull(migrationStatement);

            _this.ExecuteSql(transaction, migrationStatement);
        }

        internal virtual void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
            DebugCheck.NotNull(pendingMigrations);

            _this.Upgrade(pendingMigrations, targetMigrationId, lastMigrationId);
        }

        internal virtual void Downgrade(IEnumerable<string> pendingMigrations)
        {
            DebugCheck.NotNull(pendingMigrations);
            Debug.Assert(pendingMigrations.Count() > 1);

            _this.Downgrade(pendingMigrations);
        }

        internal virtual void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
        {
            DebugCheck.NotNull(upgradeOperations);

            _this.UpgradeHistory(upgradeOperations);
        }

        internal virtual string TargetDatabase
        {
            get { return _this.TargetDatabase; }
        }

        internal virtual bool HistoryExists()
        {
            return _this.HistoryExists();
        }
    }
}
