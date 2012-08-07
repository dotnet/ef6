// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     DbAliasedMetadataItem provides the base type for all Database Metadata types that can have an optional <see
    ///      cref="DatabaseIdentifier" /> that should be used instead of the item's <see cref="DbNamedMetadataItem.Name" /> when referring to the item in the database.
    /// </summary>
    internal abstract class DbAliasedMetadataItem
        : DbNamedMetadataItem
    {
        /// <summary>
        ///     Gets an optional alternative identifier that should be used when referring to this item in the database.
        /// </summary>
        public virtual string DatabaseIdentifier { get; set; }
    }
}
