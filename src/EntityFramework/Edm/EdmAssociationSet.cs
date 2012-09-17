// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an association set in an Entity Data Model (EDM) <see
    ///      cref="EdmEntityContainer" /> ).
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EdmAssociationSet
        : EdmEntityContainerItem
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.AssociationSet;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the <see cref="EdmAssociationType" /> that specifies the association type for the set.
        /// </summary>
        public virtual EdmAssociationType ElementType { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="EdmEntitySet" /> that specifies the entity set corresponding to the <see
        ///      cref="EdmAssociationType.SourceEnd" /> association end for this association set.
        /// </summary>
        public virtual EdmEntitySet SourceSet { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="EdmEntitySet" /> that specifies the entity set corresponding to the <see
        ///      cref="EdmAssociationType.TargetEnd" /> association end for this association set.
        /// </summary>
        public virtual EdmEntitySet TargetSet { get; set; }
    }
}
