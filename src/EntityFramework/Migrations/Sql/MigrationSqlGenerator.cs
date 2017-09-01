// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Sql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;

    /// <summary>
    /// Common base class for providers that convert provider agnostic migration
    /// operations into database provider specific SQL commands.
    /// </summary>
    public abstract class MigrationSqlGenerator
    {
        /// <summary>
        /// Gets or sets the provider manifest.
        /// </summary>
        /// <value>
        /// The provider manifest.
        /// </value>
        protected DbProviderManifest ProviderManifest { get; set; }

        /// <summary>
        /// Converts a set of migration operations into database provider specific SQL.
        /// </summary>
        /// <param name="migrationOperations"> The operations to be converted. </param>
        /// <param name="providerManifestToken"> Token representing the version of the database being targeted. </param>
        /// <returns> A list of SQL statements to be executed to perform the migration operations. </returns>
        public abstract IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken);


        /// <summary>
        /// Generates the SQL body for a stored procedure.
        /// </summary>
        /// <param name="commandTrees">The command trees representing the commands for an insert, update or delete operation.</param>
        /// <param name="rowsAffectedParameter">The rows affected parameter name.</param>
        /// <param name="providerManifestToken">The provider manifest token.</param>
        /// <returns>The SQL body for the stored procedure.</returns>
        public virtual string GenerateProcedureBody(
            ICollection<DbModificationCommandTree> commandTrees,
            string rowsAffectedParameter,
            string providerManifestToken)
        {
            return null;
        }

        /// <summary>
        /// Determines if a provider specific exception corresponds to a database-level permission denied error.
        /// </summary>
        /// <param name="exception">The database exception.</param>
        /// <returns> true if the supplied exception corresponds to a database-level permission denied error; otherwise false. </returns>
        public virtual bool IsPermissionDeniedError(Exception exception)
        {
            return false; // Default is unknown
        }

        /// <summary>
        /// Builds the store type usage for the specified <paramref name="storeTypeName"/> using the facets from the specified <paramref name="propertyModel"/>.
        /// </summary>
        /// <param name="storeTypeName">Name of the store type.</param>
        /// <param name="propertyModel">The target property.</param>
        /// <returns>A store-specific TypeUsage</returns>
        protected virtual TypeUsage BuildStoreTypeUsage(string storeTypeName, PropertyModel propertyModel)
        {
            var storeType = ProviderManifest.GetStoreTypes()
                                   .SingleOrDefault(p => string.Equals(p.Name, storeTypeName, StringComparison.OrdinalIgnoreCase));
            
            return storeType == null
                ? null
                : TypeUsage.Create(storeType, propertyModel.ToFacetValues());
        }
    }
}
