
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Diagnostics;

    internal sealed class EdmTypeReferenceElementType
    {
        private readonly EdmTypeReference typeRef;

        internal EdmTypeReferenceElementType(EdmTypeReference owner)
        {
            System.Diagnostics.Contracts.Contract.Assert(owner != null, "Collection element type created without owner?");
            this.typeRef = owner;
        }

        /// <summary>
        /// Gets the <see cref="EdmDataModelType"/> referenced by this collection element type, or <c>null</c> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type.
        /// </summary>
        public EdmDataModelType EdmType { get { return (this.typeRef.IsCollectionType ? this.typeRef.EdmType : null); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmAssociationType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsAssociationType { get { return this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.AssociationType); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmAssociationType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to an association type.
        /// </summary>
        public EdmAssociationType AssociationType { get { return this.typeRef.GetEdmTypeAs<EdmAssociationType>(this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.AssociationType)); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmComplexType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsComplexType { get { return this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.ComplexType); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmComplexType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to a complex type.
        /// </summary>
        public EdmComplexType ComplexType { get { return this.typeRef.GetEdmTypeAs<EdmComplexType>(this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.ComplexType)); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmEntityType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsEntityType { get { return this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.EntityType); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmEntityType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to an entity type.
        /// </summary>
        public EdmEntityType EntityType { get { return this.typeRef.GetEdmTypeAs<EdmEntityType>(this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.EntityType)); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmPrimitiveType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsPrimitiveType { get { return this.typeRef.IsValidPrimitiveCollection(); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmPrimitiveType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to a primitive type.
        /// </summary>
        public EdmPrimitiveType PrimitiveType { get { return this.typeRef.GetEdmTypeAs<EdmPrimitiveType>(this.typeRef.IsValidPrimitiveCollection()); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmRefType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsRefType { get { return this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.RefType); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmRefType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to a Ref type.
        /// </summary>
        public EdmRefType RefType { get { return this.typeRef.GetEdmTypeAs<EdmRefType>(this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.RefType)); } }

        /// <summary>
        /// Returns <c>true</c> if this element type's parent <see cref="EdmTypeReference"/> represents a collection type and refers to an <see cref="EdmRowType"/>; otherwise returns <c>false</c>.
        /// </summary>
        public bool IsRowType { get { return this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.RowType); } }

        /// <summary>
        /// Gets this collection element type's <see cref="EdmType"/> as an <see cref="EdmRowType"/>, or <code>null</code> if this element type's parent <see cref="EdmTypeReference"/> does not represent a collection type or does not refer to a row type.
        /// </summary>
        public EdmRowType RowType { get { return this.typeRef.GetEdmTypeAs<EdmRowType>(this.typeRef.IsValidNonPrimitiveCollection(EdmItemKind.RowType)); } }
    }
}

#endif
