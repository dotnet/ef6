namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    ///     Allows the construction and modification of a primitive- or complex-valued property of an Entity Data Model (EDM) entity or complex type.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    internal class EdmProperty : EdmStructuralMember
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.Property;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Yield(PropertyType);
        }

        /// <summary>
        ///     Gets or sets an <see cref = "EdmCollectionKind" /> value that indicates which collection semantics - if any - apply to the property.
        /// </summary>
        public virtual EdmCollectionKind CollectionKind { get; set; }

        /// <summary>
        ///     Gets or sets a <see cref = "EdmConcurrencyMode" /> value that indicates whether the property is used for concurrency validation.
        /// </summary>
        public virtual EdmConcurrencyMode ConcurrencyMode { get; set; }

        /// <summary>
        ///     Gets or sets on optional value that indicates an initial default value for the property.
        /// </summary>
        public virtual object DefaultValue { get; set; }

        /// <summary>
        ///     Gets or sets an <see cref = "EdmTypeReference" /> that specifies the result type of the property.
        /// </summary>
        public virtual EdmTypeReference PropertyType { get; set; }
    }
}