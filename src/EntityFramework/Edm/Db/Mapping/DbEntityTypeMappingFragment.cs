// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    public class DbEntityTypeMappingFragment : DbMappingMetadataItem
    {
        private readonly List<DbEdmPropertyMapping> propertyMappings = new List<DbEdmPropertyMapping>();
        private readonly List<DbColumnCondition> columnConditions = new List<DbColumnCondition>();

        /// <summary>
        ///     Gets a <see cref="EntityType" /> value representing the table to which the entity type's properties are being mapped.
        /// </summary>
        public virtual EntityType Table { get; set; }

        /// <summary>
        ///     Gets the collection of <see cref="DbEdmPropertyMapping" /> s that specifies how the type's properties are mapped to the table.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbEdmPropertyMapping> PropertyMappings
        {
            get { return propertyMappings; }
        }

        /// <summary>
        ///     Gets the collection of <see cref="DbColumnCondition" /> s that specifies the constant or null values that columns in <see
        ///      cref="Table" /> must have for this type mapping fragment to apply.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbColumnCondition> ColumnConditions
        {
            get { return columnConditions; }
        }

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntityTypeMappingFragment;
        }
    }
}
