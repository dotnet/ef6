// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    public class DbAssociationSetMapping : DbMappingMetadataItem
    {
        private readonly List<DbColumnCondition> columnConditions = new List<DbColumnCondition>();

        /// <summary>
        ///     Gets an <see cref="AssociationSet" /> value representing the association set that is being mapped.
        /// </summary>
        public virtual AssociationSet AssociationSet { get; set; }

        /// <summary>
        ///     Gets a <see cref="DbTableMetadata" /> value representing the table to which the entity type's properties are being mapped.
        /// </summary>
        public virtual DbTableMetadata Table { get; set; }

        public virtual DbAssociationEndMapping SourceEndMapping { get; set; }

        public virtual DbAssociationEndMapping TargetEndMapping { get; set; }

        /// <summary>
        ///     Gets the collection of <see cref="DbColumnCondition" /> s that specifies the constant or null values that columns in <see
        ///      cref="Table" /> must have for this type mapping to apply.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbColumnCondition> ColumnConditions
        {
            get { return columnConditions; }
        }

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.AssociationSetMapping;
        }
    }
}
