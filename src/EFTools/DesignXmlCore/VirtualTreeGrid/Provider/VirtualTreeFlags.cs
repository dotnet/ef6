// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;

    #region BranchFeatures

    /// <summary>
    ///     Features support by a branch.
    /// </summary>
    [Flags]
    internal enum BranchFeatures
    {
        /// <summary>
        ///     Branch is used to display a non-expandable static branch
        /// </summary>
        None = 0,

        /// <summary>
        ///     IBranch.IsExpandable and IBranch.GetObject(ObjectStyle.ExpandedBranch) will
        ///     be called for each item.
        /// </summary>
        Expansions = 0x0001,

        /// <summary>
        ///     Support LocateObject(ObjectStyle.ExpandedBranch). If this is not set, then
        ///     a Realign modification will close all child branches.
        /// </summary>
        BranchRelocation = 0x0002,

        /// <summary>
        ///     Support insertion and deletion. If these are not set, then at any attempt
        ///     to insert or delete items in the branch will fail
        /// </summary>
        InsertsAndDeletes = 0x0004,

        /// <summary>
        ///     UpdateCounter should be called during a global realign to determine if a branch
        ///     needs to be realigned.
        /// </summary>
        DelayedUpdates = 0x0008,

        /// <summary>
        ///     Realign may be called for this list. If this is not set, then the branch is static
        ///     and realignment of child elements is not attempted during a global realign.
        /// </summary>
        Realigns = 0x0010,

        /// <summary>
        ///     Support the IBranch.ToggleState method
        /// </summary>
        StateChanges = 0x0020,

        /// <summary>
        ///     Support selection tracking. GetObject(ObjectStyle.TrackableObject) can be called.
        /// </summary>
        PositionTracking = 0x040,

        /// <summary>
        ///     The indexed position of an item in the branch does not change
        /// </summary>
        DefaultPositionTracking = 0x0080,

        /// <summary>
        ///     Toss this branch and all children when the expansion is closed
        /// </summary>
        OnCollapseCloseAndDiscard = 0x0100,

        /// <summary>
        ///     Discard children when the branch is closed
        /// </summary>
        OnCollapseCloseChildren = 0x0200,

        /// <summary>
        ///     Don't do any discarding, just unexpand the node
        /// </summary>
        OnCollapseDoNothing = 0x0,

        /// <summary>
        ///     The number of columns in a multi column branch depends on the row
        /// </summary>
        JaggedColumns = 0x0400,

        /// <summary>
        ///     Label edits can be activated by explicit command
        /// </summary>
        ExplicitLabelEdits = 0x0800,

        /// <summary>
        ///     Label edits can be activated automatically after a delay when an item is selected with the mouse
        /// </summary>
        DelayedLabelEdits = 0x1000,

        /// <summary>
        ///     Label edits can be activated immediately when an item is selected with the mouse
        /// </summary>
        ImmediateMouseLabelEdits = 0x2000,

        /// <summary>
        ///     Label edits can be activated immediately when an item is selected, regardless of
        ///     the selection style (mouse, keyboard, or selection change via code). This flag
        ///     implies ImmediateMouseLabelEdits.
        /// </summary>
        ImmediateSelectionLabelEdits = 0x4000,
        // Update VirtualTree.BranchFeaturesToActivationStyleMask, VirtualTree.BranchFeaturesToActivationStyleShift when
        // these values change. The order should be kept the same.

        /// <summary>
        ///     Call IMultiColumnBranch.ColumnStyles when the branch is initially created to
        ///     look for columns with the SubItemCellStyles.Complex column style.
        /// </summary>
        ComplexColumns = 0x8000,

        /// <summary>
        ///     This flag means that the branch will never fire the DisplayDataChanged event.
        /// </summary>
        DisplayDataFixed = 0x10000, //(see bug 58253)

        // Update VirtualTreeConstant.FirstUserBranchFeature if values are added/removed here
    };

    #endregion

    #region TreeNavigation

    /// <summary>
    ///     Flags used to navigate in a tree or tree grid structures. Navigation is
    ///     used to locate adjacent or related items. It is not used to perform any
    ///     expansion operations on the items. For example, a right keystroke on a
    ///     collapsed node will generally expand the node, but navigating right will
    ///     either go to the next column or the first child expansion.
    /// </summary>
    internal enum TreeNavigation
    {
        /// <summary>
        ///     No data.
        /// </summary>
        None,

        /// <summary>
        ///     Go to the parent node. Available for any node that
        ///     is not in a root list (either the primary root list, or the
        ///     root list in a complex or expandable cell).
        /// </summary>
        Parent,

        /// <summary>
        ///     Go to the parent node. If the item is a the root of a subitem
        ///     list then this goes to the parent column.
        /// </summary>
        ComplexParent,

        /// <summary>
        ///     Move to the first child of an expanded node.
        /// </summary>
        FirstChild,

        /// <summary>
        ///     Move the last child of an expanded node.
        /// </summary>
        LastChild,

        /// <summary>
        ///     Move to the next sibling. A sibling is a node with the same parent, level, and column.
        /// </summary>
        NextSibling,

        /// <summary>
        ///     Move to the previous sibling. A sibling is a node with the same parent, level, and column.
        /// </summary>
        PreviousSibling,

        /// <summary>
        ///     Move up one item in the same column. Jump over blank cells as needed. If the current cell is blank, then move to the anchor for the blank range.
        /// </summary>
        Up,

        /// <summary>
        ///     Move down one item in the same column. Jump over blank cells as needed. If the current cell is blank, then move to the anchor for next blank range.
        /// </summary>
        Down,

        /// <summary>
        ///     Move to the parent node, or the previous column if no parent is available.
        /// </summary>
        Left,

        /// <summary>
        ///     Move to the closest cell in the previous column.
        /// </summary>
        LeftColumn,

        /// <summary>
        ///     Move to the next column, or the first child if no column is available.
        /// </summary>
        Right,

        /// <summary>
        ///     Move to the closest cell in the next column.
        /// </summary>
        RightColumn,
    };

    #endregion

    #region ToolTipType

    /// <summary>
    ///     The location a tooltip is displayed
    /// </summary>
    internal enum ToolTipType
    {
        /// <summary>
        ///     The tooltip for a hover in the text area. Ignore if the standard text should be used
        /// </summary>
        Default = 0x0000,

        /// <summary>
        ///     The tip text to show for an icon hover
        /// </summary>
        Icon = 0x0001,

        /// <summary>
        ///     The tip text to show for a state icon hover
        /// </summary>
        StateIcon = 0x0002
    }

    #endregion

    #region VirtualTreeDisplayMasks

    /// <summary>
    ///     Describes which fields in the VirtualTreeDisplayData should be set.
    ///     Note that values &lt;=0x40 correspond to TVIF_* flags
    /// </summary>
    [Flags]
    internal enum VirtualTreeDisplayMasks
    {
        /// <summary>
        ///     Set the Image fields
        /// </summary>
        Image = 0x0002,

        /// <summary>
        ///     Set the Image overlay fields
        /// </summary>
        ImageOverlays = 0x0004,

        /// <summary>
        ///     Set the StateImage fields
        /// </summary>
        StateImage = 0x0008,

        /// <summary>
        ///     Set the state fields
        /// </summary>
        State = 0x0010,

        /// <summary>
        ///     Set the SelectedImage field
        /// </summary>
        SelectedImage = 0x0020,

        /// <summary>
        ///     Set the ForceSelect information if applicable
        /// </summary>
        ForceSelect = 0x0080,

        /// <summary>
        ///     Set the BackColor and ForeColor fields are applicable
        /// </summary>
        Color = 0x0200,
    }

    #endregion

    #region VirtualTreeDisplayStates

    /// <summary>
    ///     Display state flags. Items &lt;=0xF000 correspond to TVIS_* flags in the system SDK
    /// </summary>
    [Flags]
    internal enum VirtualTreeDisplayStates
    {
        /// <summary>
        ///     The item is selected. This is set only in the VirtualTreeDisplayDataMasks.Mask property
        ///     sent while an item is being drawn. Setting it in the VirtualTreeDisplayData.State property
        ///     has no effect. This state should not be used to determine if an icon or state icon is
        ///     visible or to set the Bold state because it is not sent during IBranch.GetDisplayData
        ///     calls that occur while determining item width, which could lead to inconsistencies between
        ///     the display and hit testing (ToolTip interaction, VirtualTreeControl.HitInfo, etc).
        /// </summary>
        Selected = 0x000002,

        /// <summary>
        ///     The item is cut (NYI)
        /// </summary>
        Cut = 0x000004,

        /// <summary>
        ///     The item is drawn with a drop highlight (NYI)
        /// </summary>
        DropHighlighted = 0x000008,

        /// <summary>
        ///     Display the item bold
        /// </summary>
        Bold = 0x000010,

        /// <summary>
        ///     The force selection fields are set
        /// </summary>
        ForceSelect = 0x000020,

        /// <summary>
        ///     Display the item gray. Can be set indirectly with the
        /// </summary>
        GrayText = 0x000040,

        /// <summary>
        ///     The item is expanded. This is set only in the VirtualTreeDisplayDataMasks.Mask property
        ///     sent while an item is being drawn. Setting it in the VirtualTreeDisplayData.State property
        ///     has no effect. This state should not be used to determine if an icon or state icon is
        ///     visible or to set the Bold state because it is not sent during IBranch.GetDisplayData
        ///     calls that occur while determining item width, which could lead to inconsistencies between
        ///     the display and hit testing (ToolTip interaction, VirtualTreeControl.HitInfo, etc).
        /// </summary>
        Expanded = 0x000080,

        /// <summary>
        ///     Text aligned opposite (far) from the glyph.  This will cause text to be aligned to the right
        ///     if the RightToLeft property on the TreeControl is false, or to the left if RightToLeft is true.
        /// </summary>
        TextAlignFar = 0x000100,
        // If the ReverseTree property is implemented, we would want the following flag, to allow control
        // over text align behavior when this property was set.
        //
        // TextAlignIgnoreReverseTree = 0x000200
        //TextTypeMask    = 0xF00000,
    }

    #endregion

    #region VirtualTreeDisplayDataChanges

    /// <summary>
    ///     Settings for the DisplayDataChanged notification. These
    ///     values are sent with the event to indicate which part of the
    ///     UI needs to be updated.
    /// </summary>
    [Flags]
    internal enum VirtualTreeDisplayDataChanges
    {
        /// <summary>
        ///     The text needs to be redrawn
        /// </summary>
        Text = 1,

        /// <summary>
        ///     The image needs to be redrawn
        /// </summary>
        Image = 2,

        /// <summary>
        ///     The state image needs to be redrawn
        /// </summary>
        StateImage = 4,

        /// <summary>
        ///     The button for the item needs to be redrawn
        /// </summary>
        ItemButton = 8,

        /// <summary>
        ///     All visible elements need to be redrawn
        /// </summary>
        VisibleElements = Text | Image | StateImage | ItemButton,

        /// <summary>
        ///     If an item is selected in this set, then the
        ///     selection needs to be refreshed. Include this
        ///     flag if visual elements outside the control may
        ///     need to be kept in sync with this item.
        /// </summary>
        DependentUIElements = 0x10,

        /// <summary>
        ///     Value of a cell has changed.  Setting this flag will
        ///     cause the control to fire a value or name change WinEvents,
        ///     as appropriate.
        /// </summary>
        AccessibleValue = 0x20,

        /// <summary>
        ///     Update all elements for this item. A combination
        ///     of VisibleElements and DependentUIElements
        /// </summary>
        All = VisibleElements | DependentUIElements | AccessibleValue,
    }

    #endregion

    #region LabelEditResult

    /// <summary>
    ///     The result of a label edit action
    /// </summary>
    internal enum LabelEditResult
    {
        /// <summary>
        ///     The edit is acknowledged and new text retrieved will reflect this value
        /// </summary>
        AcceptEdit = 1,

        /// <summary>
        ///     The edit should be canceled
        /// </summary>
        CancelEdit = 2,

        /// <summary>
        ///     Block deactivation of the edit (NYI, defers to CancelEdit)
        /// </summary>
        BlockDeactivate = 3
    };

    #endregion

    #region StateRefreshChanges

    /// <summary>
    ///     Values returned by IBranch.ToggleState to indicate the scope of
    ///     relative items that need to be correctly display the state change.
    /// </summary>
    [Flags]
    internal enum StateRefreshChanges
    {
        /// <summary>
        ///     No refresh required
        /// </summary>
        None = 0x0000,

        /// <summary>
        ///     Refresh toggled item
        /// </summary>
        Current = 0x0001,

        /// <summary>
        ///     Refresh children of toggled item
        /// </summary>
        Children = 0x0002,

        /// <summary>
        ///     Refresh parents of toggled item
        /// </summary>
        Parents = 0x0004,

        /// <summary>
        ///     Refresh children of all parents
        /// </summary>
        ParentsChildren = 0x0008,

        /// <summary>
        ///     Refresh entire tree
        /// </summary>
        Entire = 0x0010,
    };

    #endregion

    #region BranchLocationAction

    /// <summary>
    ///     Enumeration for values return by IBranch.LocateObject called with the
    ///     ObjectStyle.ExpandedBranch style.
    /// </summary>
    internal enum BranchLocationAction
    {
        /// <summary>
        ///     Discard the branch
        /// </summary>
        DiscardBranch = 0,

        /// <summary>
        ///     Keep the branch. The return values indicates the new index
        /// </summary>
        KeepBranch = 1,

        /// <summary>
        ///     Used during an insertLevels > 0 ITree.ShiftBranchLevels call to attach an existing expansion
        ///     at a level other than the insertLevels depth. Same as KeepBranch otherwise.
        /// </summary>
        KeepBranchAtThisLevel = 2,

        /// <summary>
        ///     Discard the current object, and retrieve a new branch using the returned index.
        /// </summary>
        RetrieveNewBranch = 3,
    }

    #endregion

    #region TrackingObjectAction

    /// <summary>
    ///     Enumeration for values return by IBranch.LocateObject called with the
    ///     ObjectStyle.TrackingObject style.
    /// </summary>
    internal enum TrackingObjectAction
    {
        /// <summary>
        ///     The object could not be tracked.
        /// </summary>
        NotTracked = 0,

        /// <summary>
        ///     The object occurs at this level in the tree.
        /// </summary>
        ThisLevel = 1,

        /// <summary>
        ///     The object occurs at deeper level in the tree
        /// </summary>
        NextLevel = 2,

        /// <summary>
        ///     The object could not be tracked at this level. Return the coordinate for
        ///     the most recent IBranch.LocateObject that returned NextLevel.
        /// </summary>
        NotTrackedReturnParent = 3,
    }

    #endregion

    #region BranchModificationAction

    /// <summary>
    ///     An enumeration of actions that can be taken indirectly by a branch
    ///     against all trees (and other listeners) containing that branch
    /// </summary>
    internal enum BranchModificationAction
    {
        /// <summary>
        ///     Call the ITree.DisplayDataChanged method
        /// </summary>
        DisplayDataChanged,

        /// <summary>
        ///     Call the ITree.Realign method
        /// </summary>
        Realign,

        /// <summary>
        ///     Call the ITree.InsertItems method
        /// </summary>
        InsertItems,

        /// <summary>
        ///     Call the ITree.DeleteItems method
        /// </summary>
        DeleteItems,

        /// <summary>
        ///     Call the ITree.MoveItems method
        /// </summary>
        MoveItem,

        /// <summary>
        ///     Call the ITree.ShiftBranchLevels method
        /// </summary>
        ShiftBranchLevels,

        /// <summary>
        ///     Set the ITree.Redraw property
        /// </summary>
        Redraw,

        /// <summary>
        ///     Set the ITree.DelayRedraw property
        /// </summary>
        DelayRedraw,

        /// <summary>
        ///     Set the ITree.ListShuffle property
        /// </summary>
        ListShuffle,

        /// <summary>
        ///     Set the ITree.DelayListShuffle property
        /// </summary>
        DelayListShuffle,

        /// <summary>
        ///     Call the IMultiColumnTree.UpdateCellStyle method
        /// </summary>
        UpdateCellStyle,

        /// <summary>
        ///     Call the ITree.RemoveBranch method
        /// </summary>
        RemoveBranch,
    }

    #endregion

    #region DragEventType

    /// <summary>
    ///     An enum specifying the type of drag event being handled in IBranch.OnDragEvent.
    /// </summary>
    internal enum DragEventType
    {
        /// <summary>
        ///     An item has been dropped on the specified branch item
        /// </summary>
        Drop,

        /// <summary>
        ///     An drag operation has entered the specified branch item
        /// </summary>
        Enter,

        /// <summary>
        ///     An drag operation has entered the specified branch item.
        ///     The args parameter is null for this type of event.
        /// </summary>
        Leave,

        /// <summary>
        ///     An drag operation is over the specified branch item
        /// </summary>
        Over,
    }

    #endregion

    #region DragReason

    /// <summary>
    ///     An enum specifying why the IBranch.OnStartDrag method
    ///     is called. Both Drag/Drop and Copy/Paste operations are coordinated through the various
    ///     drag methods. This flag allows the branch implementer to proffer different
    ///     data objects for the different operations.
    /// </summary>
    internal enum DragReason
    {
        /// <summary>
        ///     The data will be used for a Drag/Drop operation
        /// </summary>
        DragDrop,

        /// <summary>
        ///     The data is being retrieved in response to a Copy command
        /// </summary>
        Copy,

        /// <summary>
        ///     The data is being retrieved in response to a Cut command
        /// </summary>
        Cut,

        /// <summary>
        ///     The status of the Copy command is being retrieved.
        /// </summary>
        CanCopy,

        /// <summary>
        ///     The status of the Cut command is being retrieved.
        /// </summary>
        CanCut
    }

    #endregion

    #region ObjectStyle

    /// <summary>
    ///     Determines the style of object to be returned by the IBranch.GetObject function.
    /// </summary>
    internal enum ObjectStyle
    {
        /// <summary>
        ///     Get the expanded branch at this location. The returned object
        ///     should be an IBranch or ITree implementation. Returning
        ///     a tree will clone all branches from the tree into the current
        ///     location. If the ITree is an alien implementation to the tree
        ///     making the request, then only currently expanded items can be
        ///     included in the current tree. Use the ExpansionOptions enum
        ///     to specify more options with GetObject, and the BranchLocationAction
        ///     settings with LocateObject.
        /// </summary>
        ExpandedBranch,

        /// <summary>
        ///     Get an object that uniquely identifies this item. Used to
        ///     maintain item selection when a list is rearranged. Use the
        ///     TrackingObjectAction enum values with LocateObject and this style.
        /// </summary>
        TrackingObject,

        /// <summary>
        ///     Get the root branch for a complex subitem. Root branches are
        ///     requested for each row in a column with a cell style of Complex
        ///     or Mixed. Use the ExpansionOptions enum to specify more options.
        /// </summary>
        SubItemRootBranch,

        /// <summary>
        ///     Get the expanded branch for a subitem cell. Use ExpansionOptions enum
        ///     to specify more options.
        /// </summary>
        SubItemExpansion,
        // If an item is added here, VirtualTreeConstant.FirstUserObjectStyle must be updated
    }

    #endregion

    #region SubItemCellStyles

    /// <summary>
    ///     Specifies the style of cells in subitem columns of
    ///     a multicolumn branch. The cell style is retrieved
    ///     when the branch is first loaded and cannot be changed.
    /// </summary>
    [Flags]
    internal enum SubItemCellStyles
    {
        /// <summary>
        ///     Subitem cells are single-valued and non-expandable.
        /// </summary>
        Simple = 1,

        /// <summary>
        ///     Subitem cells can be the parent node of an expansion, but
        ///     cannot have siblings. The data for the parent node is provided
        ///     by the IBranch implementation for the row. The IsExpandable method
        ///     is used to determine if a given cell is expandable.
        /// </summary>
        Expandable = 2,

        /// <summary>
        ///     A subitem can be comprised of multiple top level cells defined
        ///     by a separate branch. The IBranch functions and settings for a given row
        ///     do not affect these nodes. The subitem branches are requested when
        ///     the tree is first loaded. The cell reverts to simple state if a subitem
        ///     root branch is not available. UNDONE: Tree methods to toggle between
        ///     simple/expandable state and complex state.
        /// </summary>
        Complex = 4,

        /// <summary>
        ///     Cells can be either complex or expandable. A subitem root list will
        ///     be requested for each row in the branch with an ObjectStyle.SubItemRootBranch
        ///     call to IBranch.GetObject. To keep the subitem collapsed,
        ///     simply return null from this request. To preexpand an expandable subitem,
        ///     set the ExpansionOptions.UseAsSubItemExpansion flag.
        /// </summary>
        Mixed = Expandable | Complex,
    }

    #endregion

    #region ExpansionOptions

    /// <summary>
    ///     Options used in an IBranch.GetObject call for ObjectStyle.ExpandedBranch,
    ///     ObjectStyle.SubitemRootBranch, and ObjectStyle.SubitemBranch objects.
    /// </summary>
    [Flags]
    internal enum ExpansionOptions
    {
        /// <summary>
        ///     A user request for recursive expansion (* on the number keypad)
        ///     will expand one level instead of recursing. This must be set
        ///     for circular branch structures to be safe.
        /// </summary>
        BlockRecursion = 1,

        /// <summary>
        ///     If a tree object is returned in response to an ExpandedBranch call,
        ///     then incorporate all of the data from the tree. If this flag is not
        ///     set, then the returned tree can continue to be used.
        /// </summary>
        ConsumeTree = 2,

        /// <summary>
        ///     Set to turn the branch returned by an ObjectStyle.SubItemRootBranch
        ///     request into a ObjectStyle.SubItemExpansion request. This flag enables
        ///     branches returned during the initial load of a SubItemCellStyles.Mixed
        ///     column to result in subitem expansion instead of a root branch.
        /// </summary>
        UseAsSubItemExpansion = 4,
    }

    #endregion

    #region VirtualTreeLabelEditActivationStyles

    /// <summary>
    ///     Activation options for label editing. Combinations are used
    ///     to enable different support levels, and the current activation
    ///     style is sent to IBranch.BeginLabelEdit.
    /// </summary>
    [Flags]
    internal enum VirtualTreeLabelEditActivationStyles
    {
        /// <summary>
        ///     Label editing is not supported
        /// </summary>
        None = 0,

        /// <summary>
        ///     Label editing occurs in response to explicit user commands
        /// </summary>
        Explicit = 1,

        /// <summary>
        ///     Label editing occurs automatically in response to a timer firing after a mouse activation
        /// </summary>
        Delayed = 2,

        /// <summary>
        ///     Label editing occurs immediately when the item is activated with the mouse.
        /// </summary>
        ImmediateMouse = 4,

        /// <summary>
        ///     Label editing occurs immediately when the item is selected. Implies support for ImmediateMouse.
        ///     Specify both BranchFeatures.ImmediateSelectionLabelEdits and BranchFeatures.ImmediateMouseLabelEdits to get
        ///     an ImmediateMouse activation style for mouse-triggered selection, or just ImmediateSelection if the distinction
        ///     is irrelevant to your IBranch.BeginLabelEdit implementation.
        /// </summary>
        ImmediateSelection = 8,
        // Note that changes here need to be reflected in VirtualTreeControl.VTCStyleFlags and BranchFeatures
    }

    #endregion

    #region VirtualTreeConstant class

    /// <summary>
    ///     Constants used in various VirtualTree classes
    /// </summary>
    internal sealed class VirtualTreeConstant
    {
        private VirtualTreeConstant()
        {
        }

        /// <summary>
        ///     Constant used to represent any invalid index
        /// </summary>
        public const int NullIndex = -1;

        /// <summary>
        ///     The IBranch.GetObject and IBranch.LocateObject methods both
        ///     take an ObjectStyle parameter. There are a number of predefined
        ///     object styles that are recognized by the VirtualTreeGrid implementation.
        ///     However, the GetObject mechanism provides a natural entry point for
        ///     working with object styles not required by the core tree objects.
        ///     To enable the ObjectStyle enum to expand in future versions without
        ///     breaking code compiled against the original object styles, the user should
        ///     create readonly static ObjectStyle values that are greater than or equal to
        ///     VirtualTreeConstant.FirstUserObjectStyle ((ObjectStyle)(VirtualTreeConstant.FirstUserObjectStyle + 0),
        ///     (ObjectStyle)(VirtualTreeConstant.FirstUserObjectStyle + 1), etc).
        /// </summary>
        public static int FirstUserObjectStyle
        {
            get { return (int)ObjectStyle.SubItemExpansion + 1; }
        }

        /// <summary>
        ///     The first bit that can be used to store user flags in the BranchFeatures of
        ///     a given branch. User flags should be defined as static readonly BranchFeature
        ///     values shifted from this value.
        ///     public static readonly BranchFeatures CustomFeature1 = (BranchFeatures)(VirtualTreeConstant.FirstUserBranchFeature &lt;&lt; 0);
        ///     public static readonly BranchFeatures CustomFeature2 = (BranchFeatures)(VirtualTreeConstant.FirstUserBranchFeature &lt;&lt; 1);
        /// </summary>
        public static int FirstUserBranchFeature
        {
            get { return (int)(BranchFeatures.DisplayDataFixed) << 1; }
        }
    }

    #endregion
}
