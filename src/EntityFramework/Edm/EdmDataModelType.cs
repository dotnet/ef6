namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     The base for all all Entity Data Model (EDM) types that represent a type from the EDM type system.
    /// </summary>
    internal abstract class EdmDataModelType
        : EdmNamespaceItem
    {
        /// <summary>
        ///     Gets a value indicating whether this type is abstract.
        /// </summary>
        public bool IsAbstract { get; internal set; }

        /// <summary>
        ///     Gets the optional base type of this type.
        /// </summary>
        public EdmDataModelType BaseType { get; internal set; }

#if IncludeUnusedEdmCode
    /// <summary>
    /// Indicates whether this type is an <see cref="EdmAssociationType"/>.
    /// </summary>
        public bool IsAssociationType { get { return (this.ItemKind == EdmItemKind.AssociationType); } }

        /// <summary>
        /// Indicates whether this type is an <see cref="EdmComplexType"/>.
        /// </summary>
        public bool IsComplexType { get { return (this.ItemKind == EdmItemKind.ComplexType); } }

        /// <summary>
        /// Indicates whether this type is an <see cref="EdmEntityType"/>.
        /// </summary>
        public bool IsEntityType { get { return (this.ItemKind == EdmItemKind.EntityType); } }

        /// <summary>
        /// Indicates whether this type is an <see cref="EdmEntityType"/>.
        /// </summary>
        public bool IsPrimitiveType { get { return (this.ItemKind == EdmItemKind.PrimitiveType); } }

        /// <summary>
        /// Indicates whether this type is an <see cref="EdmRefType"/>.
        /// </summary>
        public bool IsRefType { get { return (this.ItemKind == EdmItemKind.RefType); } }

        /// <summary>
        /// Indicates whether this type is an <see cref="EdmRowType"/>.
        /// </summary>
        public bool IsRowType { get { return (this.ItemKind == EdmItemKind.RowType); } }
#endif
    }
}
