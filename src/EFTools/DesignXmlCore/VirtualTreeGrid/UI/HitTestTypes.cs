// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;

    #region VirtualTreeHitTargets flags

    /// <summary>
    ///     Values used with VirtualTreeHitInfo and VirtualTreeExtendedHitInfo
    ///     to indicate where on a given item the hit occurred.
    /// </summary>
    [Flags]
    internal enum VirtualTreeHitTargets
    {
        /// <summary>
        ///     Indicates an empty structure
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        ///     The mouse is currently not on an item row or column
        /// </summary>
        NoWhere = 1,

        /// <summary>
        ///     The hit location is over the item's icon
        /// </summary>
        OnItemIcon = 2,

        /// <summary>
        ///     The hit location is on the items text region
        /// </summary>
        OnItemLabel = 4,

        /// <summary>
        ///     The hit location is over the indent region for the item
        /// </summary>
        OnItemIndent = 8,

        /// <summary>
        ///     The hit location is over the item. OnItem is a mask field that combines OnItemLabel, OnItemIcon, OnItemStateIcon
        /// </summary>
        OnItem = OnItemIcon | OnItemLabel | OnItemStateIcon,

        /// <summary>
        ///     The hit location is over the item's button
        /// </summary>
        OnItemButton = 0x10,

        /// <summary>
        ///     The hit location is to the right of the item label
        /// </summary>
        OnItemRight = 0x20,

        /// <summary>
        ///     The hit location is over the item's state icon (the state icon is generally used for checkboxes)
        /// </summary>
        OnItemStateIcon = 0x40,

        /// <summary>
        ///     The hit location is to the left of the item
        /// </summary>
        OnItemLeft = 0x80,

        /// <summary>
        ///     The hit location is over the item region. OnItemRegion is a mask field that combines OnItem, OnItemButton, OnItemRight, OnItemIndex, OnItemLeft.
        /// </summary>
        OnItemRegion = OnItem | OnItemButton | OnItemRight | OnItemIndent | OnItemLeft,

        /// <summary>
        ///     The hit location is above the client area
        /// </summary>
        Above = 0x100,

        /// <summary>
        ///     The hit location is below the client area
        /// </summary>
        Below = 0x200,

        /// <summary>
        ///     The hit location is to the right of the client area
        /// </summary>
        ToRight = 0x400,

        /// <summary>
        ///     The hit location is to the left of the client area
        /// </summary>
        ToLeft = 0x800,

        /// <summary>
        ///     The hit location is outside the client area. OutsideClientArea is mask field that combines Above, Below, ToRight, ToLeft.
        /// </summary>
        OutsideClientArea = Above | Below | ToRight | ToLeft,

        /// <summary>
        ///     The hit location is over a blank tree coordinate, but the VirtualTreeHitInfo has information
        ///     about the resolved blank expansion anchor. Use the RawRow and RawColumn properties to get the
        ///     unmodified hit information.
        /// </summary>
        OnBlankItem = 0x1000,

        /// <summary>
        ///     May be combined with OnItemStateIcon to indicate that the state icon is displayed
        ///     as hot-tracked.  Currently only supported for standard checkboxes.
        /// </summary>
        StateIconHotTracked = 0x2000
    }

    #endregion

    #region VirtualTreeHitInfo struct

    /// <summary>
    ///     Structure representing row and column information in the VirtualTreeControl
    /// </summary>
    internal struct VirtualTreeHitInfo
    {
        private int myRow;
        private readonly VirtualTreeHitTargets myHitTarget;
        private readonly int myDisplayColumn;
        private readonly int myNativeColumn;
        private int myRawRow;
        private readonly int myRawColumn;

        internal VirtualTreeHitInfo(int row, int column, VirtualTreeHitTargets target)
        {
            myRow = myRawRow = row;
            myHitTarget = target;
            myDisplayColumn = myNativeColumn = myRawColumn = column;
        }

        internal VirtualTreeHitInfo(int row, int displayColumn, int nativeColumn, int rawRow, int rawColumn, VirtualTreeHitTargets target)
        {
            myRow = row;
            myHitTarget = target;
            myDisplayColumn = displayColumn;
            myNativeColumn = nativeColumn;
            myRawRow = rawRow;
            myRawColumn = rawColumn;
        }

        /// <summary>
        ///     The row of the item. If the item is a blank, this is the resolved row, which may differ from the RawRow.
        /// </summary>
        public int Row
        {
            get { return myRow; }
        }

        internal void ClearRowData()
        {
            myRow = myRawRow = VirtualTreeConstant.NullIndex;
        }

        /// <summary>
        ///     Information about the area of the item we're on. This can also include the
        ///     VirtualTreeHitTargets.OnBlankItem if the hit is not directly over an item.
        /// </summary>
        public VirtualTreeHitTargets HitTarget
        {
            get { return myHitTarget; }
        }

        /// <summary>
        ///     The column as currently displayed in the tree. If the item is on a blank, this
        ///     is resolved to the anchor column of the blank. If there is a ColumnPermutation active,
        ///     then this differs from the NativeColumn.
        /// </summary>
        public int DisplayColumn
        {
            get { return myDisplayColumn; }
        }

        /// <summary>
        ///     The native column of the hit item. The native column
        ///     is used with the ITree interface and can differ from
        ///     the DisplayColumn if a ColumnPermutation has been
        ///     applied to the tree.
        /// </summary>
        public int NativeColumn
        {
            get { return myNativeColumn; }
        }

        /// <summary>
        ///     The RawRow is the row actually hovered on. In the case of blank expansions,
        ///     RawRow will correspond to the row actually hovered over, while the
        ///     Row will correspond to the resolved blank item anchor.
        /// </summary>
        public int RawRow
        {
            get { return myRawRow; }
        }

        /// <summary>
        ///     The RawColumn is the column actually hovered on. In the case of blank expansions,
        ///     RawColumn will correspond to the column actually hovered over, while
        ///     DisplayColumn will correspond to the resolved blank item anchor, and NativeColumn
        ///     the corresponding column to use with the current Tree.
        /// </summary>
        public int RawColumn
        {
            get { return myRawColumn; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     VirtualTreeHitInfo structures should not be compared
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode
        /// </summary>
        /// <returns>Returns base hash code</returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     VirtualTreeHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(VirtualTreeHitInfo operand1, VirtualTreeHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     VirtualTreeHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(VirtualTreeHitInfo operand1, VirtualTreeHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     VirtualTreeHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(VirtualTreeHitInfo operand1, VirtualTreeHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }

    #endregion

    #region ExtraHitInfo struct

    internal struct ExtraHitInfo
    {
        public bool IsTruncated;
        public Rectangle ClippedItemRectangle; // The label and glyphs, clipped for string truncation
        public Rectangle FullLabelRectangle; // The full label rectangle without glyphs or truncation
        public int LabelOffset; // The width of the glyph regions
        public Font LabelFont; // The font used to draw the item
        public StringFormat LabelFormat; // The format used to draw the string
    }

    #endregion

    #region VirtualTreeExtendedHitInfo struct

    /// <summary>
    ///     Structure representing row and column information in the VirtualTreeControl with additional
    ///     information about the location of different UI elements.
    /// </summary>
    internal struct VirtualTreeExtendedHitInfo
    {
        private VirtualTreeHitInfo myHitInfo;
        private ExtraHitInfo myExtraHitInfo;

        internal VirtualTreeExtendedHitInfo(ref VirtualTreeHitInfo hitInfo, ref ExtraHitInfo extraHitInfo)
        {
            myHitInfo = hitInfo;
            myExtraHitInfo = extraHitInfo;
        }

        /// <summary>
        ///     The row of the item. If the item is a blank, this is the resolved row, which may differ from the RawRow.
        /// </summary>
        public int Row
        {
            get { return myHitInfo.Row; }
        }

        /// <summary>
        ///     Information about the area of the item we're on. This can also include the
        ///     VirtualTreeHitTargets.OnBlankItem if the hit is not directly over an item.
        /// </summary>
        public VirtualTreeHitTargets HitTarget
        {
            get { return myHitInfo.HitTarget; }
        }

        /// <summary>
        ///     The column as currently displayed in the tree. If the item is on a blank, this
        ///     is resolved to the anchor column of the blank. If there is a ColumnPermutation active,
        ///     then this differs from the NativeColumn.
        /// </summary>
        public int DisplayColumn
        {
            get { return myHitInfo.DisplayColumn; }
        }

        /// <summary>
        ///     The native column of the hit item. The native column
        ///     is used with the ITree interface and can differ from
        ///     the DisplayColumn if a ColumnPermutation has been
        ///     applied to the tree.
        /// </summary>
        public int NativeColumn
        {
            get { return myHitInfo.NativeColumn; }
        }

        /// <summary>
        ///     The RawRow is the row actually hovered on. In the case of blank expansions,
        ///     RawRow will correspond to the row actually hovered over, while the
        ///     Row will correspond to the resolved blank item anchor.
        /// </summary>
        public int RawRow
        {
            get { return myHitInfo.RawRow; }
        }

        /// <summary>
        ///     The RawColumn is the column actually hovered on. In the case of blank expansions,
        ///     RawColumn will correspond to the column actually hovered over, while
        ///     DisplayColumn will correspond to the resolved blank item anchor, and NativeColumn
        ///     the corresponding column to use with the current Tree.
        /// </summary>
        public int RawColumn
        {
            get { return myHitInfo.RawColumn; }
        }

        /// <summary>
        ///     Returns true if the text portion of the item is obscured
        /// </summary>
        public bool IsTruncated
        {
            get { return myExtraHitInfo.IsTruncated; }
        }

        /// <summary>
        ///     The label and glyphs, clipped for string truncation. The
        ///     rectangle accounts for the current position of the horizontal
        ///     scrollbar, but it may not be clipped by the client boundaries
        ///     of the control.
        /// </summary>
        public Rectangle ClippedItemRectangle
        {
            get { return myExtraHitInfo.ClippedItemRectangle; }
        }

        /// <summary>
        ///     The full label rectangle without glyphs or truncation. The
        ///     rectangle accounts for the current position of the horizontal
        ///     scrollbar.
        /// </summary>
        public Rectangle FullLabelRectangle
        {
            get { return myExtraHitInfo.FullLabelRectangle; }
        }

        /// <summary>
        ///     The width of the glyph regions
        /// </summary>
        public int LabelOffset
        {
            get { return myExtraHitInfo.LabelOffset; }
        }

        /// <summary>
        ///     The font used to draw the item
        /// </summary>
        public Font LabelFont
        {
            get { return myExtraHitInfo.LabelFont; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     VirtualTreeExtendedHitInfo structures should not be compared
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode
        /// </summary>
        /// <returns>Returns base hashcode</returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     VirtualTreeExtendedHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(VirtualTreeExtendedHitInfo operand1, VirtualTreeExtendedHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     VirtualTreeExtendedHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(VirtualTreeExtendedHitInfo operand1, VirtualTreeExtendedHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     VirtualTreeExtendedHitInfo structures should not be compared
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(VirtualTreeExtendedHitInfo operand1, VirtualTreeExtendedHitInfo operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }

    #endregion
}
