// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Used when scripting an update database operation to store the operations that would have been performed against the database.
    /// </summary>
    public class UpdateDatabaseOperation : MigrationOperation
    {
        /// <summary>
        /// Represents a migration to be applied to the database.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class Migration
        {
            private readonly string _migrationId;
            private readonly IList<MigrationOperation> _operations;

            internal Migration(string migrationId, IList<MigrationOperation> operations)
            {
                DebugCheck.NotEmpty(migrationId);
                DebugCheck.NotNull(operations);

                _migrationId = migrationId;
                _operations = operations;
            }

            /// <summary>
            /// Gets the id of the migration.
            /// </summary>
            /// <value>
            /// The id of the migration.
            /// </value>
            public string MigrationId
            {
                get { return _migrationId; }
            }

            /// <summary>
            /// Gets the individual operations applied by this migration.
            /// </summary>
            /// <value>
            /// The individual operations applied by this migration.
            /// </value>
            public IList<MigrationOperation> Operations
            {
                get { return _operations; }
            }
        }

        private readonly IList<DbQueryCommandTree> _historyQueryTrees;
        private readonly IList<Migration> _migrations = new List<Migration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDatabaseOperation"/> class.
        /// </summary>
        /// <param name="historyQueryTrees">
        /// The queries used to determine if this migration needs to be applied to the database. 
        /// This is used to generate an idempotent SQL script that can be run against a database at any version.
        /// </param>
        public UpdateDatabaseOperation(IList<DbQueryCommandTree> historyQueryTrees)
            : base(null)
        {
            Check.NotNull(historyQueryTrees, "historyQueryTrees");

            _historyQueryTrees = historyQueryTrees;
        }

        /// <summary>
        /// The queries used to determine if this migration needs to be applied to the database. 
        /// This is used to generate an idempotent SQL script that can be run against a database at any version.
        /// </summary>
        public IList<DbQueryCommandTree> HistoryQueryTrees
        {
            get { return _historyQueryTrees; }
        }

        /// <summary>
        /// Gets the migrations applied during the update database operation.
        /// </summary>
        /// <value>
        /// The migrations applied during the update database operation.
        /// </value>
        public IList<Migration> Migrations
        {
            get { return _migrations; }
        }

        /// <summary>
        /// Adds a migration to this update database operation.
        /// </summary>
        /// <param name="migrationId">The id of the migration.</param>
        /// <param name="operations">The individual operations applied by the migration.</param>
        public void AddMigration(string migrationId, IList<MigrationOperation> operations)
        {
            Check.NotEmpty(migrationId, "migrationId");
            Check.NotNull(operations, "operations");

            _migrations.Add(new Migration(migrationId, operations));
        }

        /// <summary>
        /// Gets a value indicating if any of the operations may result in data loss.
        /// </summary>
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
