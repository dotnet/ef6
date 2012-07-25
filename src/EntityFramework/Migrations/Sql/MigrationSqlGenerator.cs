// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.Sql
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Model;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Common base class for providers that convert provider agnostic migration 
    ///     operations into database provider specific SQL commands.
    /// </summary>
    [ContractClass(typeof(MigrationSqlGeneratorContracts))]
    public abstract class MigrationSqlGenerator
    {
        /// <summary>
        ///     Converts a set of migration operations into database provider specific SQL.
        /// </summary>
        /// <param name = "migrationOperations">The operations to be converted.</param>
        /// <param name = "providerManifestToken">Token representing the version of the database being targeted.</param>
        /// <returns>A list of SQL statements to be executed to perform the migration operations.</returns>
        public abstract IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken);

        #region Contracts

        [ContractClassFor(typeof(MigrationSqlGenerator))]
        internal abstract class MigrationSqlGeneratorContracts : MigrationSqlGenerator
        {
            public override IEnumerable<MigrationStatement> Generate(
                IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
            {
                Contract.Requires(migrationOperations != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(providerManifestToken));

                return default(IEnumerable<MigrationStatement>);
            }
        }

        #endregion
    }
}
