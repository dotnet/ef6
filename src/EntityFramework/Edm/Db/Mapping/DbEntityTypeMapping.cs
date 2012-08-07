// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    ///     Allows the construction and modification of a complete or partial mapping of an EDM entity type ( <see
    ///      cref="EdmEntityType" /> ) or type hierarchy to a specific database table ( <see cref="DbTableMetadata" /> ).
    /// </summary>
    internal class DbEntityTypeMapping
        : DbMappingMetadataItem
    {
        private readonly BackingList<DbEntityTypeMappingFragment> typeMappingFragments =
            new BackingList<DbEntityTypeMappingFragment>();

        internal override DbMappingItemKind GetItemKind()
        {
            return DbMappingItemKind.EntityTypeMapping;
        }

        /// <summary>
        ///     Gets or sets an <see cref="EdmEntityType" /> value representing the entity type or hierarchy that is being mapped.
        /// </summary>
        public virtual EdmEntityType EntityType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this type mapping applies to <see cref="EntityType" /> and all its direct or indirect subtypes ( <code>true</code> ), or only to <see
        ///      cref="EntityType" /> ( <code>false</code> ).
        /// </summary>
        public virtual bool IsHierarchyMapping { get; set; }

        public virtual IList<DbEntityTypeMappingFragment> TypeMappingFragments
        {
            get { return typeMappingFragments.EnsureValue(); }
            set { typeMappingFragments.SetValue(value); }
        }
    }
}
