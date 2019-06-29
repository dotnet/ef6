// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Windows.Forms;

    /// <summary>
    ///     Interface that must be implemented to provide items for the tree. The IBranch
    ///     implementation owns all details of the items within its branch (test, glyphs, drag-drop and label edit
    ///     behavior, expansions, selection tracking, etc). However, a branch has no knowledge of where
    ///     it resides in a tree, or if it is currently visible. A single branch can exist in multiple
    ///     trees, or in multiple locations in the same tree.
    /// </summary>
    internal interface IBranch
    {
        /// <summary>
        ///     Return the features supported by this branch.
        /// </summary>
        BranchFeatures Features { get; }

        /// <summary>
        ///     Return the count of visible items in this branch. The branch can contain
        ///     items beyond the visible count as long as expanded items each have a unique
        ///     index. The tree does not care how many total items a branch has.
        /// </summary>
        int VisibleItemCount { get; }

        /// <summary>
        ///     Retrieve an object associated with this branch. See ObjectStyle
        ///     for descriptions of the different object styles that the tree will request.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="style">Style of object to retrieve</param>
        /// <param name="options">Placeholder for setting/returns options. Contents depend on the style.</param>
        /// <returns>An object or null, with the type of the object determined by the style parameter.</returns>
        object GetObject(int row, int column, ObjectStyle style, ref int options);

        /// <summary>
        ///     Locate an object returned previously by the GetObject method. This enables select
        ///     and expansion states to be tracked across modifications to the tree.
        /// </summary>
        /// <param name="obj">The object to locate</param>
        /// <param name="style">The style of object to retrieve (same value originally passed to GetObject)</param>
        /// <param name="locateOptions">Placholde for setting options. Contents depend on the style.</param>
        /// <returns>The new object location and options. For the predefined styles, a row of -1 (VirtualTreeConstant.NullIndex) indicates that the object cannot be located.</returns>
        LocateObjectData LocateObject(object obj, ObjectStyle style, int locateOptions);

        //Get a pointer to the main text for the list item. Caller will NOT free, implementor
        //can reuse buffer for each call to GetText except for TTO_SORTTEXT. See VSTREETEXTOPTIONS for tto details
        /// <summary>
        ///     Get the string to display for the given row and column.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>
        ///     A string. As this string is often frequently requested, especially by layered implementations that
        ///     use it to sort the displayed text, it is recommended that the string be cached by the IBranch implementation. Dynamically
        ///     regenerating the string for each request is not recommended
        /// </returns>
        string GetText(int row, int column);

        /// <summary>
        ///     Get the accessibility name to display for the given row and column.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>The accessibility name for the given row</returns>
        string GetAccessibleName(int row, int column);

        /// <summary>
        ///     Get the accessibility value to display for the given row and column.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>The accessibility value for the given row</returns>
        string GetAccessibleValue(int row, int column);

        /// <summary>
        ///     Get the string for the tiptext of the item, or null to use the standard text.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="tipType">The hover location associated with the tiptext</param>
        /// <returns>A string, or null to use the standard text</returns>
        string GetTipText(int row, int column, ToolTipType tipType);

        /// <summary>
        ///     Test whether an item is expandable. This is called whenever an unexpanded (does not included collapsed)
        ///     item is drawn, so should be a reasonably fast routine. This is called only if Features.Expansions is set.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>True to display the item as expandable</returns>
        bool IsExpandable(int row, int column);

        /// <summary>
        ///     Determine how an item should be displayed.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="requiredData">The required display information</param>
        /// <returns>Display data for the item</returns>
        VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData);

        /// <summary>
        ///     Retrieve accessibility data for the item at the given row, column
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>Populated VirtualTreeAccessibilityData structue, or VirtualTreeAccessibilityData.Empty</returns>
        VirtualTreeAccessibilityData GetAccessibilityData(int row, int column);

        //Begin a label edit. Return true to continue, false to abort.
        //CustomInPlaceEdit can either contain a type, which will be instantiated,
        //or an instance of an object, which will be used as-is.  Both must implement
        //the VirtualTreeControl.IVirtualTreeInPlaceControl interface.
        /// <summary>
        ///     Begin a label edit
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="activationStyle">
        ///     The activation style. This method will be called only if the
        ///     style must be supported by the Features for this branch as well for as by the VirtualTreeControl.LabelEditSupport
        ///     property before this function is called.
        /// </param>
        /// <returns>
        ///     Settings for the label edit. Return VirtualTreeLabelEditData.Invalid to block label activation,
        ///     and VirtualTreeLabelEditData.DeferActivation to turn an immediate activation request into a delayed activation request.
        /// </returns>
        VirtualTreeLabelEditData BeginLabelEdit(int row, int column, VirtualTreeLabelEditActivationStyles activationStyle);

        /// <summary>
        ///     Commit the results of the last label edit.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="newText">The text resulting from the label edit</param>
        /// <returns>Accept, cancel, or block deactivation of the edit operation</returns>
        LabelEditResult CommitLabelEdit(int row, int column, string newText);

        /// <summary>
        ///     Return the latest update value. True/False isn't sufficient here since
        ///     multiple trees may be using the branch. Return an update counter greater than
        ///     the last one cached by a given tree will force calls to VisibleItemCount and
        ///     LocateObject as needed. Features.DelayedUpdates must be set for this function
        ///     to be called.
        /// </summary>
        int UpdateCounter { get; }

        /// <summary>
        ///     Toggle the state of the given item. If there are more than two states,
        ///     then 'toggle' means move to the next state. Features.StateChange must be
        ///     set for ToggleState to be called.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>StateRefreshChanges value other than None to indicate a change</returns>
        StateRefreshChanges ToggleState(int row, int column);

        /// <summary>
        ///     Synchronize the state of the given item to match the state of the passed
        ///     in branch. Synchronize state is called in multiselect scenarios if
        ///     more than one checkbox item is selected when the state is toggled.
        ///     The implementation of this function must recognize the underlying type
        ///     of the implementing branch to retrieve its state. Features.StateChange must
        ///     be set for SynchronizeState to be called.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="matchBranch">Branch to synchronize with</param>
        /// <param name="matchRow">Row in branch to synchronize with</param>
        /// <param name="matchColumn">Column in branch to synchronize with</param>
        /// <returns>StateRefreshChanges value other than None to indicate a change</returns>
        StateRefreshChanges SynchronizeState(int row, int column, IBranch matchBranch, int matchRow, int matchColumn);

        /// <summary>
        ///     Handle an item is being dragged or dropped over this item.
        /// </summary>
        /// <param name="sender">The control responding to the drag</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="eventType">The type of drag event that is occuring</param>
        /// <param name="args">The arguments for this event (null for a Leave event)</param>
        void OnDragEvent(object sender, int row, int column, DragEventType eventType, DragEventArgs args);

        /// <summary>
        ///     Begin dragging data owned by this branch
        /// </summary>
        /// <param name="sender">The control initiating the drag</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="reason">The user action that triggered the drag request</param>
        /// <returns>Return valid drag data, VirtualTreeStartDragData.Empty if there is nothing to drag</returns>
        VirtualTreeStartDragData OnStartDrag(object sender, int row, int column, DragReason reason);

        /// <summary>
        ///     Provide feedback for the last drag object returned by OnStartDrag at this position.
        ///     The row and column parameters are provided to support providing feedback during
        ///     multiselect drag/drop operations, where one branch can provide multiple drag objects.
        /// </summary>
        /// <param name="args">Standard GiveFeedbackEventArgs arguments</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        void OnGiveFeedback(GiveFeedbackEventArgs args, int row, int column);

        /// <summary>
        ///     Called for query continue events on last object return by OnStartDrag at this position.
        ///     The row and column parameters are provided to support providing feedback during
        ///     multiselect drag/drop operations, where one branch can provide multiple drag objects.
        ///     The Drop action can also be used to indicate the end of the drag/drop negotiation sequence.
        /// </summary>
        /// <param name="args">Standard QueryContinueDragEvent arguments</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        void OnQueryContinueDrag(QueryContinueDragEventArgs args, int row, int column);

        /// <summary>
        ///     An event used to notify all users of this branch that is being modified. Notifications are sent using
        ///     BranchModificationEventArgs instances returned by static methods on the BranchModificationEventArgs class.
        ///     A branch can exist in multiple tree implementations, so a many-to-one relationship should never be assumed
        ///     between a tree and a branch. The BranchModification mechanism enables a branch to notify all trees using
        ///     it that changes have been made without holding a direct reference to the containing trees.
        /// </summary>
        event BranchModificationEventHandler OnBranchModification;
    }
}
