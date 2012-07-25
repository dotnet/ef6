// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db.Mapping
{
    /// <summary>
    ///     Allows the construction and modification of a condition for a column in a database table.
    /// </summary>
    internal class DbColumnCondition
        : DbMappingMetadataItem
    {
        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.ColumnCondition;
        }

        /// <summary>
        ///     Gets or sets a <see cref = "DbTableColumnMetadata" /> value representing the table column which must contain <see cref = "Value" /> for this condition to hold.
        /// </summary>
        public virtual DbTableColumnMetadata Column { get; set; }

        /// <summary>
        ///     Gets or sets the value that <see cref = "Column" /> must contain for this condition to hold.
        /// </summary>
        public virtual object Value { get; set; }

        public virtual bool? IsNull { get; set; }
    }
}
