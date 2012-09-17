// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     Allows the construction and modification of additional constraints that can be applied to a specific use of a primitive type in an Entity Data Model (EDM) item. See <see
    ///      cref="EdmTypeReference" /> .
    /// </summary>
    public class EdmPrimitiveTypeFacets : EdmDataModelItem
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.PrimitiveFacets;
        }

        /// <summary>
        ///     Returns <code>true</code> if any facet value property currently has a non-null value; otherwise returns <code>false</code> .
        /// </summary>
        public virtual bool HasValue
        {
            get
            {
                return (MaxLength.HasValue || IsMaxLength.HasValue || IsFixedLength.HasValue ||
                        IsUnicode.HasValue || Precision.HasValue || Scale.HasValue);
            }
        }

        /// <summary>
        ///     Gets or sets an optional value indicating the current constraint on the type's maximum length.
        /// </summary>
        public virtual int? MaxLength { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the referenced type should be considered to have its intrinsic maximum length, rather than a specific value.
        /// </summary>
        public virtual bool? IsMaxLength { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the referenced type should be considered to have a fixed or variable length.
        /// </summary>
        public virtual bool? IsFixedLength { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the referenced type should be considered to be Unicode or non-Unicode.
        /// </summary>
        public virtual bool? IsUnicode { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating the current constraint on the type's precision.
        /// </summary>
        public virtual byte? Precision { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating the current constraint on the type's scale.
        /// </summary>
        public virtual byte? Scale { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating that the current spatial type's SRID is unconstrained.
        /// </summary>
        public virtual bool? IsVariableSrid { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating the current spatial type's SRID.
        /// </summary>
        public virtual int? Srid { get; set; }

        /// <summary>
        ///     Gets or sets an optional value indicating whether the spatial type is to be type checked strictly.
        /// </summary>
        public virtual bool? IsStrict { get; set; }
    }
}
