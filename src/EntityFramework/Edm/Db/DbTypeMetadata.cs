// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     Represents a specific use of a type in a Database Metadata item.
    /// </summary>
    internal abstract class DbTypeMetadata : DbMetadataItem
    {
        private DbPrimitiveTypeFacets facets;

        public virtual string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets an optional <see cref="DbPrimitiveTypeFacets" /> instance that applies additional constraints to a referenced primitive type.
        /// </summary>
        /// <remarks>
        ///     Accessing this property forces the creation of a DbPrimitiveTypeFacets value if no value has previously been set. Use <see
        ///      cref="HasFacets" /> to determine whether or not this property currently has a value.
        /// </remarks>
        public virtual DbPrimitiveTypeFacets Facets
        {
            get
            {
                if (facets == null)
                {
                    facets = new DbPrimitiveTypeFacets();
                }
                return facets;
            }

            set { facets = value; }
        }

        public virtual bool HasFacets
        {
            get { return facets != null && facets.HasValue; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the represented type is a collection type.
        /// </summary>
        public virtual bool IsCollection { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the referenced type should be considered nullable.
        /// </summary>
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the type has been configured as a row type by the addition of one or more RowColumns.
        /// </summary>
        public virtual bool IsRow
        {
            get { return TypeName == null && !HasFacets; }
        }
    }
}
