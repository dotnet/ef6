// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of a specific use of a type in an Entity Data Model (EDM) item. See <see cref = "EdmProperty.PropertyType" /> for examples.
    /// </summary>
    internal class EdmTypeReference : EdmMetadataItem
    {
        private EdmPrimitiveTypeFacets facets;

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.TypeReference;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets a value indicating the collection rank of the type reference. A collection rank greater than zero indicates that the type reference represents a collection of its referenced <see cref = "EdmType" /> .
        /// </summary>
        public virtual int CollectionRank { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating the <see cref = "EdmDataModelType" /> referenced by this type reference.
        /// </summary>
        public virtual EdmDataModelType EdmType { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the referenced type should be considered nullable.
        /// </summary>
        public virtual bool? IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets an optional <see cref = "EdmPrimitiveTypeFacets" /> instance that applies additional constraints to a referenced primitive type.
        /// </summary>
        /// <remarks>
        ///     Accessing this property forces the creation of an EdmPrimitiveTypeFacets value if no value has previously been set. Use <see cref = "HasFacets" /> to determine whether or not this property currently has a value.
        /// </remarks>
        public virtual EdmPrimitiveTypeFacets PrimitiveTypeFacets
        {
            get
            {
                if (facets == null)
                {
                    facets = new EdmPrimitiveTypeFacets();
                }
                return facets;
            }

            set { facets = value; }
        }

        #region Type Inspection Properties

        /// <summary>
        ///     Gets a value indicating whether the <see cref = "PrimitiveTypeFacets" /> property of this type reference has been assigned an <see cref = "EdmPrimitiveTypeFacets" /> value with at least one facet value specified.
        /// </summary>
        public bool HasFacets
        {
            get { return facets != null && facets.HasValue; }
        }

        /// <summary>
        ///     Indicates whether this type reference represents a collection of its referenced <see cref = "EdmType" /> (when <see cref = "CollectionRank" /> is greater than zero) or not.
        /// </summary>
        public bool IsCollectionType
        {
            get { return IsValid() && CollectionRank > 0; }
        }

        /// <summary>
        ///     Indicates whether the <see cref = "EdmType" /> property of this type reference currently refers to an <see cref = "EdmComplexType" /> , is not a collection type, and does not have primitive facet values specified.
        /// </summary>
        public bool IsComplexType
        {
            get { return IsValidNonPrimitive(EdmItemKind.ComplexType); }
        }

        /// <summary>
        ///     Gets the <see cref = "EdmComplexType" /> currently referred to by this type reference, or <code>null</code> if the type reference is a collection type or does not refer to a complex type.
        /// </summary>
        public EdmComplexType ComplexType
        {
            get { return GetEdmTypeAs<EdmComplexType>(IsValidNonPrimitive(EdmItemKind.ComplexType)); }
        }

        /// <summary>
        ///     Indicates whether the <see cref = "EdmType" /> property of this type reference currently refers to an <see cref = "EdmPrimitiveType" /> and is not a collection type.
        /// </summary>
        public bool IsPrimitiveType
        {
            get { return IsValidPrimitive(); }
        }

        /// <summary>
        ///     Gets the <see cref = "EdmPrimitiveType" /> currently referred to by this type reference, or <code>null</code> if the type reference is a collection type or does not refer to a primitive type.
        /// </summary>
        public EdmPrimitiveType PrimitiveType
        {
            get { return GetEdmTypeAs<EdmPrimitiveType>(IsValidPrimitive()); }
        }

        public bool IsEnumType
        {
            get { return IsValidNonPrimitive(EdmItemKind.EnumType); }
        }

        public EdmEnumType EnumType
        {
            get { return GetEdmTypeAs<EdmEnumType>(IsValidNonPrimitive(EdmItemKind.EnumType)); }
        }

        public bool IsUnderlyingPrimitiveType
        {
            get { return IsPrimitiveType || IsEnumType; }
        }

        public EdmPrimitiveType UnderlyingPrimitiveType
        {
            get { return IsEnumType ? EnumType.UnderlyingType : PrimitiveType; }
        }

        private bool IsValid()
        {
            return (EdmType != null &&
                    CollectionRank >= 0 &&
                    (EdmType.ItemKind == EdmItemKind.PrimitiveType || !HasFacets));
        }

        internal bool IsValidPrimitive()
        {
            return (IsValid() && EdmType.ItemKind == EdmItemKind.PrimitiveType && CollectionRank == 0);
        }

        internal bool IsValidNonPrimitive(EdmItemKind kind)
        {
            Contract.Assert(
                kind != EdmItemKind.PrimitiveType,
                "Calling IsNonPrimitiveTypeKind with EdmItemKind.Primitive as TypeKind?");

            return (IsValid() && EdmType.ItemKind == kind && CollectionRank == 0);
        }

        internal TEdmType GetEdmTypeAs<TEdmType>(bool condition)
            where TEdmType : EdmDataModelType
        {
            return (condition ? EdmType as TEdmType : null);
        }

        #endregion
    }
}
