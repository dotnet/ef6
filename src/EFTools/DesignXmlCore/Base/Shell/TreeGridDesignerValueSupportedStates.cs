// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;

    /// <remarks>
    ///     Describes the view/edit state of a particular value (cell) in the grid.
    /// </remarks>
    [Flags]
    internal enum TreeGridDesignerValueSupportedStates
    {
        /// <summary>
        ///     Default state, value displayed normally (not read-only), but no in-place editing or keyboard navigation supported.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Value should be displayed as read-only.  Controls the VirtualTreeDisplayData.GrayText flag in the base
        ///     implementation of GetDisplayData.
        /// </summary>
        DisplayReadOnly = 1,

        /// <summary>
        ///     Value should be displayed as read-only, also implies no support for in-place editing or keyboard navigation.
        /// </summary>
        Unsupported = DisplayReadOnly,

        /// <summary>
        ///     Value can be in-place edited.  If this flag is set on a ColumnDescriptor, the ColumnDescriptor.GetInPlaceEdit
        ///     will be called to retrieve an in-place edit control.
        /// </summary>
        SupportsInPlaceEdit = 2,

        /// <summary>
        ///     Value supports keyboard navigation (e.g., should be included as a tab stop).
        /// </summary>
        SupportsKeyboardNavigation = 4,

        /// <summary>
        ///     Supports editing and keyboard navigation.
        /// </summary>
        Supported = SupportsInPlaceEdit | SupportsKeyboardNavigation
    }
}
