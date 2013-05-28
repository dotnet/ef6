// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class UpdateDatabaseOperation : MigrationOperation
    {
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

            public string MigrationId
            {
                get { return _migrationId; }
            }

            public IList<MigrationOperation> Operations
            {
                get { return _operations; }
            }
        }

        private readonly IList<DbQueryCommandTree> _historyQueryTrees;
        private readonly IList<Migration> _migrations = new List<Migration>();

        public UpdateDatabaseOperation(IList<DbQueryCommandTree> historyQueryTrees)
            : base(null)
        {
            Check.NotNull(historyQueryTrees, "historyQueryTrees");

            _historyQueryTrees = historyQueryTrees;
        }

        public IList<DbQueryCommandTree> HistoryQueryTrees
        {
            get { return _historyQueryTrees; }
        }

        public IList<Migration> Migrations
        {
            get { return _migrations; }
        }

        public void AddMigration(string migrationId, IList<MigrationOperation> operations)
        {
            Check.NotEmpty(migrationId, "migrationId");
            Check.NotNull(operations, "operations");

            _migrations.Add(new Migration(migrationId, operations));
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
