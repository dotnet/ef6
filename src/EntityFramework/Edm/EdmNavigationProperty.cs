// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an Entity Data Model (EDM) navigation property.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    [DebuggerDisplay("{Name}")]
    internal class EdmNavigationProperty
        : EdmStructuralMember
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.NavigationProperty;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Enumerable.Empty<EdmMetadataItem>();
        }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationType" /> that specifies the association over which navigation takes place.
        /// </summary>
        public virtual EdmAssociationType Association { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref = "EdmAssociationEnd" /> that specifies which association end is the 'destination' end of the navigation and produces the navigation property result.
        /// </summary>
        public virtual EdmAssociationEnd ResultEnd { get; set; }
    }
}
