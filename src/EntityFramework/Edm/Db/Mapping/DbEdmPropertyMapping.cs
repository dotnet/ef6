// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents the mapping of an entity property to a column in a database table.
    /// </summary>
    public class DbEdmPropertyMapping
        : DbMappingMetadataItem
    {
        private IList<EdmProperty> propertyPathList = new List<EdmProperty>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EdmPropertyMapping;
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="EdmProperty" /> instances that defines the mapped property, beginning from a property declared by the mapped entity type and optionally proceeding through properties of complex property result types.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmProperty> PropertyPath
        {
            get { return propertyPathList; }
            set { propertyPathList = value; }
        }

        /// <summary>
        ///     Gets or sets a <see cref="EdmProperty" /> value representing the table column to which the entity property is being mapped.
        /// </summary>
        public virtual EdmProperty Column { get; set; }
    }
}
