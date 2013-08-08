// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Decorator to provide logging during migrations operations..
    /// </summary>
    public class MigratorLoggingDecorator : MigratorBase
    {
        private readonly MigrationsLogger _logger;
        private string _lastInfoMessage;

        /// <summary>
        /// Initializes a new instance of the MigratorLoggingDecorator class.
        /// </summary>
        /// <param name="innerMigrator"> The migrator that this decorator is wrapping. </param>
        /// <param name="logger"> The logger to write messages to. </param>
        public MigratorLoggingDecorator(MigratorBase innerMigrator, MigrationsLogger logger)
            : base(innerMigrator)
        {
            Check.NotNull(innerMigrator, "innerMigrator");
            Check.NotNull(logger, "logger");

            _logger = logger;
            _logger.Verbose(Strings.LoggingTargetDatabase(base.TargetDatabase));
        }

        internal override void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            DebugCheck.NotEmpty(migrationId);

            _logger.Info(
                downgrading
                    ? Strings.LoggingRevertAutoMigrate(migrationId)
                    : Strings.LoggingAutoMigrate(migrationId));

            base.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
        }

        internal override void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            DebugCheck.NotNull(transaction);
            DebugCheck.NotNull(migrationStatement);

            _logger.Verbose(migrationStatement.Sql);

            var providerServices
                = DbProviderServices.GetProviderServices(transaction.Connection);

            if (providerServices != null)
            {
                providerServices.RegisterInfoMessageHandler(
                    transaction.Connection,
                    message =>
                        {
                            if (!string.Equals(message, _lastInfoMessage, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.Warning(message);

                                // simple duplicate filtering
                                _lastInfoMessage = message;
                            }
                        });
            }

            base.ExecuteSql(transaction, migrationStatement);
        }

        internal override void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
            DebugCheck.NotNull(pendingMigrations);

            var count = pendingMigrations.Count();

            _logger.Info(
                (count > 0)
                    ? Strings.LoggingPendingMigrations(count, pendingMigrations.Join())
                    : string.IsNullOrWhiteSpace(targetMigrationId)
                          ? Strings.LoggingNoExplicitMigrations
                          : Strings.LoggingAlreadyAtTarget(targetMigrationId));

            base.Upgrade(pendingMigrations, targetMigrationId, lastMigrationId);
        }

        internal override void Downgrade(IEnumerable<string> pendingMigrations)
        {
            DebugCheck.NotNull(pendingMigrations);

            var loggableMigrations
                = pendingMigrations.Take(pendingMigrations.Count() - 1);

            _logger.Info(
                Strings.LoggingPendingMigrationsDown(
                    loggableMigrations.Count(),
                    loggableMigrations.Join()));

            base.Downgrade(pendingMigrations);
        }

        internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            DebugCheck.NotNull(migration);

            _logger.Info(Strings.LoggingApplyMigration(((IMigrationMetadata)migration).Id));

            base.ApplyMigration(migration, lastMigration);
        }

        internal override void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
        {
            DebugCheck.NotEmpty(migrationId);

            _logger.Info(Strings.LoggingRevertMigration(migrationId));

            base.RevertMigration(migrationId, migration, targetModel);
        }

        internal override void SeedDatabase()
        {
            _logger.Info(Strings.LoggingSeedingDatabase);

            base.SeedDatabase();
        }

        internal override void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
        {
            _logger.Info(Strings.UpgradingHistoryTable);

            base.UpgradeHistory(upgradeOperations);
        }
    }
}
