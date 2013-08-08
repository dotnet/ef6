// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Scaffolds code-based migrations to apply pending model changes to the database.
    /// </summary>
    public class MigrationScaffolder
    {
        private readonly DbMigrator _migrator;
        private string _namespace;
        private bool _namespaceSpecified;

        /// <summary>
        /// Initializes a new instance of the MigrationScaffolder class.
        /// </summary>
        /// <param name="migrationsConfiguration"> Configuration to be used for scaffolding. </param>
        public MigrationScaffolder(DbMigrationsConfiguration migrationsConfiguration)
        {
            Check.NotNull(migrationsConfiguration, "migrationsConfiguration");

            _migrator = new DbMigrator(migrationsConfiguration);
        }

        /// <summary>
        /// Gets or sets the namespace used in the migration's generated code.
        /// By default, this is the same as MigrationsNamespace on the migrations
        /// configuration object passed into the constructor. For VB.NET projects, this
        /// will need to be updated to take into account the project's root namespace.
        /// </summary>
        public string Namespace
        {
            get
            {
                return _namespaceSpecified
                           ? _namespace
                           : _migrator.Configuration.MigrationsNamespace;
            }
            set
            {
                _namespaceSpecified = _migrator.Configuration.MigrationsNamespace != value;
                _namespace = value;
            }
        }

        /// <summary>
        /// Scaffolds a code based migration to apply any pending model changes to the database.
        /// </summary>
        /// <param name="migrationName"> The name to use for the scaffolded migration. </param>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration Scaffold(string migrationName)
        {
            Check.NotEmpty(migrationName, "migrationName");

            return _migrator.Scaffold(migrationName, Namespace, ignoreChanges: false);
        }

        /// <summary>
        /// Scaffolds a code based migration to apply any pending model changes to the database.
        /// </summary>
        /// <param name="migrationName"> The name to use for the scaffolded migration. </param>
        /// <param name="ignoreChanges"> Whether or not to include model changes. </param>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration Scaffold(string migrationName, bool ignoreChanges)
        {
            Check.NotEmpty(migrationName, "migrationName");

            return _migrator.Scaffold(migrationName, Namespace, ignoreChanges);
        }

        /// <summary>
        /// Scaffolds the initial code-based migration corresponding to a previously run database initializer.
        /// </summary>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration ScaffoldInitialCreate()
        {
            return _migrator.ScaffoldInitialCreate(Namespace);
        }
    }
}
