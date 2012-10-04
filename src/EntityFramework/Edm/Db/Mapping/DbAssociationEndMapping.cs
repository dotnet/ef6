// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents the mapping of an EDM association end ( <see cref="AssociationEndMember" /> ) as a collection of property mappings ( <see
    ///      cref="DbEdmPropertyMapping" /> ).
    /// </summary>
    public class DbAssociationEndMapping : DbMappingMetadataItem
    {
        private readonly List<DbEdmPropertyMapping> propertyMappings = new List<DbEdmPropertyMapping>();

        /// <summary>
        ///     Gets an <see cref="AssociationEndMember" /> value representing the association end that is being mapped.
        /// </summary>
        public virtual AssociationEndMember AssociationEnd { get; set; }

        /// <summary>
        ///     Gets the collection of <see cref="DbEdmPropertyMapping" /> s that specifies how the association end key properties are mapped to the table.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbEdmPropertyMapping> PropertyMappings
        {
            get { return propertyMappings; }
        }

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.AssociationEndMapping;
        }
    }
}
