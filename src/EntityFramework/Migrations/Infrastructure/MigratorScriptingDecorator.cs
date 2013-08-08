// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Decorator to produce a SQL script instead of applying changes to the database.
    /// Using this decorator to wrap <see cref="DbMigrator" /> will prevent <see cref="DbMigrator" />
    /// from applying any changes to the target database.
    /// </summary>
    public class MigratorScriptingDecorator : MigratorBase
    {
        private readonly StringBuilder _sqlBuilder = new StringBuilder();

        private UpdateDatabaseOperation _updateDatabaseOperation;

        /// <summary>
        /// Initializes a new instance of the  MigratorScriptingDecorator class.
        /// </summary>
        /// <param name="innerMigrator"> The migrator that this decorator is wrapping. </param>
        public MigratorScriptingDecorator(MigratorBase innerMigrator)
            : base(innerMigrator)
        {
            Check.NotNull(innerMigrator, "innerMigrator");
        }

        /// <summary>
        /// Produces a script to update the database.
        /// </summary>
        /// <param name="sourceMigration">
        /// The migration to update from. If null is supplied, a script to update the
        /// current database will be produced.
        /// </param>
        /// <param name="targetMigration">
        /// The migration to update to. If null is supplied,
        /// a script to update to the latest migration will be produced.
        /// </param>
        /// <returns> The generated SQL script. </returns>
        public string ScriptUpdate(string sourceMigration, string targetMigration)
        {
            _sqlBuilder.Clear();

            if (string.IsNullOrWhiteSpace(sourceMigration))
            {
                Update(targetMigration);
            }
            else
            {
                if (sourceMigration.IsAutomaticMigration())
                {
                    throw Error.AutoNotValidForScriptWindows(sourceMigration);
                }

                var sourceMigrationId = GetMigrationId(sourceMigration);
                var pendingMigrations = GetLocalMigrations().Where(m => string.CompareOrdinal(m, sourceMigrationId) > 0);

                string targetMigrationId = null;

                if (!string.IsNullOrWhiteSpace(targetMigration))
                {
                    if (targetMigration.IsAutomaticMigration())
                    {
                        throw Error.AutoNotValidForScriptWindows(targetMigration);
                    }

                    targetMigrationId = GetMigrationId(targetMigration);

                    if (string.CompareOrdinal(sourceMigrationId, targetMigrationId) > 0)
                    {
                        throw Error.DownScriptWindowsNotSupported();
                    }

                    pendingMigrations = pendingMigrations.Where(m => string.CompareOrdinal(m, targetMigrationId) <= 0);
                }

                _updateDatabaseOperation
                    = sourceMigration == DbMigrator.InitialDatabase
                          ? new UpdateDatabaseOperation(base.CreateDiscoveryQueryTrees().ToList())
                          : null;

                Upgrade(pendingMigrations, targetMigrationId, sourceMigrationId);

                if (_updateDatabaseOperation != null)
                {
                    ExecuteStatements(base.GenerateStatements(new[] { _updateDatabaseOperation }, null));
                }
            }

            return _sqlBuilder.ToString();
        }

        internal override IEnumerable<MigrationStatement> GenerateStatements(
            IList<MigrationOperation> operations, string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            if (_updateDatabaseOperation == null)
            {
                return base.GenerateStatements(operations, migrationId);
            }

            _updateDatabaseOperation.AddMigration(migrationId, operations);

            return Enumerable.Empty<MigrationStatement>();
        }

        internal override void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
        {
            mustSucceedToKeepDatabase();
        }

        internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            foreach (var migrationStatement in migrationStatements)
            {
                if (!string.IsNullOrWhiteSpace(migrationStatement.Sql))
                {
                    if (!string.IsNullOrWhiteSpace(migrationStatement.BatchTerminator)
                        && (_sqlBuilder.Length > 0))
                    {
                        _sqlBuilder.AppendLine(migrationStatement.BatchTerminator);
                        _sqlBuilder.AppendLine();
                    }

                    _sqlBuilder.AppendLine(migrationStatement.Sql);
                }
            }
        }

        internal override void SeedDatabase()
        {
        }

        internal override bool HistoryExists()
        {
            return false;
        }
    }
}
