// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Sql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Migrations.Model;

    /// <summary>
    ///     Common base class for providers that convert provider agnostic migration
    ///     operations into database provider specific SQL commands.
    /// </summary>
    public abstract class MigrationSqlGenerator
    {
        /// <summary>
        ///     Converts a set of migration operations into database provider specific SQL.
        /// </summary>
        /// <param name="migrationOperations"> The operations to be converted. </param>
        /// <param name="providerManifestToken"> Token representing the version of the database being targeted. </param>
        /// <returns> A list of SQL statements to be executed to perform the migration operations. </returns>
        public abstract IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken);

        public virtual string GenerateProcedureBody(
            ICollection<DbModificationCommandTree> commandTrees,
            string rowsAffectedParameter,
            string providerManifestToken)
        {
            return null;
        }
    }
}
