// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using System.Reflection;

    public static class DbMigratorExtensions
    {
        private static readonly PropertyInfo _operationsProperty
            = typeof(DbMigration).GetProperty("Operations", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IList<MigrationOperation> GetOperations(this DbMigration migration)
        {
            var migrationOperations = (IList<MigrationOperation>)_operationsProperty.GetValue(migration, null);

            if (!migrationOperations.Any())
            {
                migration.Up();
            }

            return migrationOperations;
        }
    }
}
