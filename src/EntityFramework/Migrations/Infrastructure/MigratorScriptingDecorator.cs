namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Decorator to produce a SQL script instead of applying changes to the database.
    ///     Using this decorator to wrap <see cref = "DbMigrator" /> will prevent <see cref = "DbMigrator" /> 
    ///     from applying any changes to the target database.
    /// </summary>
    public class MigratorScriptingDecorator : MigratorBase
    {
        private readonly StringBuilder _sqlBuilder = new StringBuilder();

        /// <summary>
        ///     Initializes a new instance of the  MigratorScriptingDecorator class.
        /// </summary>
        /// <param name = "innerMigrator">The migrator that this decorator is wrapping.</param>
        public MigratorScriptingDecorator(MigratorBase innerMigrator)
            : base(innerMigrator)
        {
            Contract.Requires(innerMigrator != null);
        }

        /// <summary>
        ///     Produces a script to update the database.
        /// </summary>
        /// <param name = "sourceMigration">
        ///     The migration to update from. 
        ///     If null is supplied, a script to update the current database will be produced.
        /// </param>
        /// <param name = "targetMigration">
        ///     The migration to update to.
        ///     If null is supplied, a script to update to the latest migration will be produced.
        /// </param>
        /// </param>
        /// <returns>The generated SQL script.</returns>
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

                Upgrade(pendingMigrations, targetMigrationId, sourceMigrationId);
            }

            return _sqlBuilder.ToString();
        }

        internal override void EnsureDatabaseExists()
        {
        }

        internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            foreach (var migrationStatement in migrationStatements)
            {
                if (!string.IsNullOrWhiteSpace(migrationStatement.Sql))
                {
                    _sqlBuilder.AppendLine(migrationStatement.Sql);
                }
            }
        }

        internal override void SeedDatabase()
        {
        }

        internal override bool IsFirstMigrationIncludingAutomatics(string migrationId)
        {
            if (migrationId.IsAutomaticMigration())
            {
                return true;
            }

            var migration = base.GetMigration(migrationId);
            var migrationMetadata = (IMigrationMetadata)migration;

            return (migrationMetadata.Source == null);
        }
    }
}
