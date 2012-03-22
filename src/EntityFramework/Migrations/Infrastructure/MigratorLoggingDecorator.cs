namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    /// <summary>
    ///     Decorator to provide logging during migrations operations..
    /// </summary>
    public class MigratorLoggingDecorator : MigratorBase
    {
        private static readonly Regex _historyInsertRegex
            = new Regex(
                @"^INSERT INTO \[" + HistoryContext.TableName + @"\].*$",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex _historyDeleteRegex
            = new Regex(
                @"^DELETE.* \[" + HistoryContext.TableName + @"\].*$",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex _metadataDeleteRegex
            = new Regex(
                @"DELETE.* \[EdmMetadata\].*((\r?\n)?|$)",
                RegexOptions.Compiled);

        private static readonly Regex _metadataInsertRegex
            = new Regex(
                @"INSERT.* \[EdmMetadata\].*((\r?\n)?|$)",
                RegexOptions.Compiled);

        private readonly MigrationsLogger _logger;

        /// <summary>
        ///     Initializes a new instance of the MigratorLoggingDecorator class.
        /// </summary>
        /// <param name = "innerMigrator">The migrator that this decorator is wrapping.</param>
        /// <param name = "logger">The logger to write messages to.</param>
        public MigratorLoggingDecorator(MigratorBase innerMigrator, MigrationsLogger logger)
            : base(innerMigrator)
        {
            Contract.Requires(innerMigrator != null);
            Contract.Requires(logger != null);

            _logger = logger;
            _logger.Verbose(Strings.LoggingTargetDatabase(base.TargetDatabase));
        }

        internal override void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            _logger.Info(
                downgrading
                    ? Strings.LoggingRevertAutoMigrate(migrationId)
                    : Strings.LoggingAutoMigrate(migrationId));

            base.AutoMigrate(migrationId, sourceModel, targetModel, downgrading);
        }

        internal override void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            var cleanSql = _historyInsertRegex.Replace(migrationStatement.Sql, Strings.LoggingHistoryInsert);
            cleanSql = _historyDeleteRegex.Replace(cleanSql, Strings.LoggingHistoryDelete);
            cleanSql = _metadataDeleteRegex.Replace(cleanSql, Strings.LoggingMetadataUpdate);
            cleanSql = _metadataInsertRegex.Replace(cleanSql, string.Empty);

            _logger.Verbose(cleanSql.Trim());

            base.ExecuteSql(transaction, migrationStatement);
        }

        internal override void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
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
            _logger.Info(Strings.LoggingApplyMigration(((IMigrationMetadata)migration).Id));

            base.ApplyMigration(migration, lastMigration);
        }

        internal override void RevertMigration(string migrationId, DbMigration migration, XDocument targetModel)
        {
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
