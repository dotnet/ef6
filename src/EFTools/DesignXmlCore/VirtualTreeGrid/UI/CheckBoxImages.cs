// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Diagnostics.CodeAnalysis;

    #region Standard CheckBox Image Indices

    /// <summary>
    ///     Image indices used for the VirtualTreeDisplayData.StateImageIndex property
    ///     when the VirtualTreeControl.StandardCheckBoxes property is set.
    /// </summary>
    internal enum StandardCheckBoxImage
    {
        /// <summary>
        ///     An unchecked box
        /// </summary>
        Unchecked = 0,

        /// <summary>
        ///     An unchecked box, drawn hot-tracked (usually used with mouse hover).
        /// </summary>
        UncheckedHot = 1,

        /// <summary>
        ///     An unchecked box, drawn disabled
        /// </summary>
        UncheckedDisabled = 2,

        /// <summary>
        ///     A checked box
        /// </summary>
        Checked = 3,

        /// <summary>
        ///     A checked box, drawn hot-tracked (usually used with mouse hover).
        /// </summary>
        CheckedHot = 4,

        /// <summary>
        ///     A checked box, drawn disabled
        /// </summary>
        CheckedDisabled = 5,

        /// <summary>
        ///     A mixed-state box
        /// </summary>
        Indeterminate = 6,

        /// <summary>
        ///     A mixed-state box, drawn hot-tracked (usually used with mouse hover).
        /// </summary>
        IndeterminateHot = 7,

        /// <summary>
        ///     A mixed-state box, drawn disabled
        /// </summary>
        IndeterminateDisabled = 8,

        /// <summary>
        ///     Provided for backwards compatibility, same as UncheckedDisabled
        /// </summary>
        Inactive = UncheckedDisabled,

        /// <summary>
        ///     An unchecked box, drawn flat
        /// </summary>
        UncheckedFlat = 9,

        /// <summary>
        ///     A checked box, drawn flat
        /// </summary>
        CheckedFlat = 10,

        /// <summary>
        ///     A grayed and checked box, drawn flat
        /// </summary>
        IndeterminateFlat = 11,

        /// <summary>
        ///     A grayed box with no check, drawn flat
        /// </summary>
        InactiveFlat = 12,

        // If values are added or removed here, LastStandardCheckBoxImageIndex below
        // must be updated as well.
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class VirtualTreeControl
    {
        /// <summary>
        ///     Indicates the last image index which corresponds to a standard check box.
        ///     Must be kept in sync with the enum above.
        /// </summary>
        private const int LastStandardCheckBoxImageIndex = 11;
    }

    #endregion // Standard CheckBox Image Indices
}
