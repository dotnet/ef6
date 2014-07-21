// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Implemented by Entity Framework providers and used to check whether or not tables exist
    ///     in a given database. This is used by database initializers when determining whether or not to
    ///     treat an existing database as empty such that tables should be created.
    /// </summary>
    public abstract class TableExistenceChecker
    {
        /// <summary>
        ///     When overridden in a derived class checks where the given tables exist in the database
        ///     for the given connection.
        /// </summary>
        /// <param name="context">
        ///     The context for which table checking is being performed, usually used to obtain an appropriate
        ///     <see cref="DbInterceptionContext" />.
        /// </param>
        /// <param name="connection">
        ///     A connection to the database. May be open or closed; should be closed again if opened. Do not
        ///     dispose.
        /// </param>
        /// <param name="modelTables">The tables to check for existence.</param>
        /// <param name="edmMetadataContextTableName">The name of the EdmMetadata table to check for existence.</param>
        /// <returns>True if any of the model tables or EdmMetadata table exists.</returns>
        public abstract bool AnyModelTableExistsInDatabase(
            ObjectContext context, DbConnection connection, IEnumerable<EntitySet> modelTables, string edmMetadataContextTableName);

        /// <summary>
        ///     Helper method to get the table name for the given s-space <see cref="EntitySet" />.
        /// </summary>
        /// <param name="modelTable">The s-space entity set for the table.</param>
        /// <returns>The table name.</returns>
        protected virtual string GetTableName(EntitySet modelTable)
        {
            return modelTable.MetadataProperties.Contains("Table")
                   && modelTable.MetadataProperties["Table"].Value != null
                ? (string)modelTable.MetadataProperties["Table"].Value
                : modelTable.Name;
        }
    }
}
