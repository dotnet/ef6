namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    internal class DbEntityTypeMappingFragment : DbMappingMetadataItem
    {
        private readonly BackingList<DbEdmPropertyMapping> propertyMappings = new BackingList<DbEdmPropertyMapping>();
        private readonly BackingList<DbColumnCondition> columnConditions = new BackingList<DbColumnCondition>();

        /// <summary>
        ///     Gets a <see cref = "DbTableMetadata" /> value representing the table to which the entity type's properties are being mapped.
        /// </summary>
        public virtual DbTableMetadata Table { get; set; }

        /// <summary>
        ///     Gets the collection of <see cref = "DbEdmPropertyMapping" /> s that specifies how the type's properties are mapped to the table.
        /// </summary>
        public virtual IList<DbEdmPropertyMapping> PropertyMappings
        {
            get { return propertyMappings.EnsureValue(); }
            set { propertyMappings.SetValue(value); }
        }

        /// <summary>
        ///     Gets the collection of <see cref = "DbColumnCondition" /> s that specifies the constant or null values that columns in <see cref = "Table" /> must have for this type mapping fragment to apply.
        /// </summary>
        public virtual IList<DbColumnCondition> ColumnConditions
        {
            get { return columnConditions.EnsureValue(); }
            set { columnConditions.SetValue(value); }
        }

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntityTypeMappingFragment;
        }
    }
}
