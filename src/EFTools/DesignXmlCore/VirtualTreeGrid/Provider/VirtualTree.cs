// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Standard implementation of the ITree interface
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class VirtualTree : ITree
    {
        // UNDONE_MC: Use of COLUMN_ZERO indicates a multicolumn undone, rip when no longer in use
        internal const int COLUMN_ZERO = 0;

        private const int BranchFeaturesToActivationStyleMask =
            (int)
            (BranchFeatures.ExplicitLabelEdits | BranchFeatures.DelayedLabelEdits | BranchFeatures.ImmediateMouseLabelEdits
             | BranchFeatures.ImmediateSelectionLabelEdits);

        private const int BranchFeaturesToActivationStyleShift = 11;

        /// <summary>
        ///     Translate values from a BranchFeatures value into the corresponding VirtualTreeLabelEditActivationStyles value
        /// </summary>
        /// <param name="features">Features value</param>
        /// <returns>Label Edit Activation Style</returns>
        public static VirtualTreeLabelEditActivationStyles ActivationStylesFromBranchFeatures(BranchFeatures features)
        {
            return
                (VirtualTreeLabelEditActivationStyles)
                (((int)features & BranchFeaturesToActivationStyleMask) >> BranchFeaturesToActivationStyleShift);
        }

        /// <summary>
        ///     Translate values from a VirtualTreeLabelEditActivationStyles value into the corresponding BranchFeatures value
        /// </summary>
        /// <param name="activationStyles">Activation styles value</param>
        /// <returns>Branch Features Value</returns>
        public static BranchFeatures BranchFeaturesFromActivationStyles(VirtualTreeLabelEditActivationStyles activationStyles)
        {
            return (BranchFeatures)(((int)activationStyles << BranchFeaturesToActivationStyleShift) & BranchFeaturesToActivationStyleMask);
        }

        #region NODEPOSITIONTRACKER

        private abstract class NODEPOSITIONTRACKER
        {
            public static bool Add(
                TREENODE parentNode, PositionTracker[] positions, int positionIndex, int relativeRow, int relativeColumn,
                ref NODEPOSITIONTRACKER lastTracker)
            {
                Debug.Assert((lastTracker == null) || (lastTracker.myNext == null));
                Debug.Assert(!parentNode.NoTracking);
                var retVal = false;
                NODEPOSITIONTRACKER newTracker = null;
                if (parentNode.DefaultTracking)
                {
                    newTracker = new NODEPOSITIONTRACKER_Default(parentNode, positions, positionIndex, relativeRow, relativeColumn);
                }
                else
                {
                    object trackerObject;
                    var dummyOptions = 0;
                    trackerObject = parentNode.Branch.GetObject(relativeRow, relativeColumn, ObjectStyle.TrackingObject, ref dummyOptions);
                    if (trackerObject != null)
                    {
                        newTracker = new NODEPOSITIONTRACKER_Dynamic(parentNode, positions, positionIndex, trackerObject);
                    }
                }
                if (newTracker != null)
                {
                    if (lastTracker != null)
                    {
                        lastTracker.myNext = newTracker;
                    }
                    lastTracker = newTracker;
                    retVal = true;
                }
                return retVal;
            }

            public static void DetachAll(ref NODEPOSITIONTRACKER ntHead)
            {
                var ntCur = ntHead;
                ntHead = null;
                while (ntCur != null)
                {
                    if (ntCur.myParentNode != null)
                    {
                        // Note that this will detach multiple nodes. The sibling
                        // chains don't matter as long as they aren't attached to any TREENODE.
                        ntCur.myParentNode.FirstPositionTracker = null;
                    }
                    ntCur = ntCur.myNext;
                }
            }

            public void OnParentNodeDeleted()
            {
                // Called on the first item of the sibling chain on a parent object
                Debug.Assert(myParentNode != null && myParentNode.FirstPositionTracker == this);
                myParentNode.FirstPositionTracker = null;
                var ntCur = this;
                while (ntCur != null)
                {
                    ntCur.myParentNode = null;
                    ntCur.myPositions[ntCur.myPositionIndex].EndRow = VirtualTreeConstant.NullIndex;
                    ntCur = ntCur.myNextSibling;
                }
            }

            public static void UpdateEndPositions(NODEPOSITIONTRACKER ntHead)
            {
                var ntCur = ntHead;
                while (ntCur != null)
                {
                    ntCur.UpdateEndPosition(ref ntCur.myPositions[ntCur.myPositionIndex]);
                    ntCur = ntCur.myNext;
                }
            }

            public static void UpdateParentNode(NODEPOSITIONTRACKER ntHead, TREENODE newParentNode)
            {
                var ntCur = ntHead;
                while (ntCur != null)
                {
                    ntCur.myParentNode = newParentNode;
                    ntCur = ntCur.myNextSibling;
                }
            }

            private void UpdateEndPosition(ref PositionTracker tracker)
            {
                if (tracker.StartRow != VirtualTreeConstant.NullIndex)
                {
                    int relativeRow;
                    int relativeColumn;
                    GetRelativePosition(out relativeRow, out relativeColumn);
                    if (relativeRow == VirtualTreeConstant.NullIndex)
                    {
                        // Can't track the node
                        tracker.EndRow = VirtualTreeConstant.NullIndex;
                    }
                    else
                    {
                        // Have end position, now determine where this node sits in the tree
                        // UNDONE: Should be able to optimize this by walking the sibling lists
                        // off of individual nodes.
                        int singleColumnSubItemAdjust;
                        var coord = FindAbsoluteIndex(myParentNode, relativeRow, out singleColumnSubItemAdjust);
                        if (coord.IsValid)
                        {
                            tracker.EndRow = coord.Row;
                            tracker.Column = coord.Column;
                        }
                        else
                        {
                            tracker.EndRow = -1;
                        }
                    }
                }
            }

            protected NODEPOSITIONTRACKER(TREENODE parentNode, PositionTracker[] positions, int positionIndex)
            {
                myParentNode = parentNode;
                myPositions = positions;
                myPositionIndex = positionIndex;
                myNextSibling = myParentNode.FirstPositionTracker;
                myParentNode.FirstPositionTracker = this;
            }

            protected TREENODE myParentNode; // Node that this tracker is attached to
            private readonly PositionTracker[] myPositions; // Reference to Positions array
            private readonly int myPositionIndex; // Position in Positions array
            private NODEPOSITIONTRACKER myNext; // Next tracker
            protected NODEPOSITIONTRACKER myNextSibling; // Next tracker on this ParentNode
            public abstract void GetRelativePosition(out int relativeRow, out int relativeColumn);
        }

        private class NODEPOSITIONTRACKER_Default : NODEPOSITIONTRACKER
        {
            private readonly int myRelativeRow;
            private readonly int myRelativeColumn;

            public NODEPOSITIONTRACKER_Default(
                TREENODE parentNode, PositionTracker[] positions, int positionIndex, int relativeRow, int relativeColumn)
                :
                    base(parentNode, positions, positionIndex)
            {
                myRelativeRow = relativeRow;
                myRelativeColumn = relativeColumn;
            }

            public override void GetRelativePosition(out int relativeRow, out int relativeColumn)
            {
                if (myParentNode == null)
                {
                    relativeRow = VirtualTreeConstant.NullIndex;
                    relativeColumn = 0;
                }
                else
                {
                    relativeRow = myRelativeRow;
                    relativeColumn = myRelativeColumn;
                }
            }
        }

        private class NODEPOSITIONTRACKER_Dynamic : NODEPOSITIONTRACKER
        {
            private readonly object myTrackerObject;

            public NODEPOSITIONTRACKER_Dynamic(TREENODE parentNode, PositionTracker[] positions, int positionIndex, object trackerObject)
                :
                    base(parentNode, positions, positionIndex)
            {
                myTrackerObject = trackerObject;
            }

            public override void GetRelativePosition(out int relativeRow, out int relativeColumn)
            {
                relativeRow = VirtualTreeConstant.NullIndex;
                relativeColumn = 0;
                if (myParentNode != null)
                {
                    TrackingObjectAction action;
                    var locateData = myParentNode.Branch.LocateObject(myTrackerObject, ObjectStyle.TrackingObject, 0);
                    relativeRow = locateData.Row;
                    relativeColumn = locateData.Column;
                    action = (TrackingObjectAction)locateData.Options;
                    if (action == TrackingObjectAction.NotTracked
                        || action == TrackingObjectAction.NotTrackedReturnParent)
                    {
                        relativeRow = VirtualTreeConstant.NullIndex;
                    }
                }
            }

            public static void DetachTrackers(ref NODEPOSITIONTRACKER ntFirst, ref NODEPOSITIONTRACKER_Dynamic ntDetached)
            {
                Debug.Assert(ntFirst != null); // Precondition, check before calling
                var ntHead = (NODEPOSITIONTRACKER_Dynamic)ntFirst;
                var ntNext = ntHead;
                NODEPOSITIONTRACKER_Dynamic ntCur = null;
                while (ntNext != null)
                {
                    ntCur = ntNext;
                    ntCur.myParentNode = null;
                    ntNext = (NODEPOSITIONTRACKER_Dynamic)ntCur.myNextSibling;
                }
                // Link all of the detached objects together
                ntCur.myNextSibling = ntDetached;
                ntDetached = ntHead;
                ntFirst = null;
            }

            public void QueryReattachObjects(
                VirtualTree tree, TREENODE tnStartParent, int maxLevels, out int changeCount, out int subItemChangeCount)
            {
                // Return the ChangeCount wrt/tnParent
                NODEPOSITIONTRACKER_Dynamic nptPrev = null;
                var nptNext = this;
                NODEPOSITIONTRACKER_Dynamic nptCur;
                //TREENODE tnLastChildPrimary = null;
                TREENODE tnLastChildSecondary;
                int level;
                int attachIndex;
                int expansionCount;
                TrackingObjectAction action;
                TREENODE tnCurParent;
                TREENODE tnNextParent;
                changeCount = 0;
                subItemChangeCount = 0;
                while ((nptCur = nptNext) != null)
                {
                    // Grab the next up front
                    nptNext = (NODEPOSITIONTRACKER_Dynamic)nptCur.myNextSibling;

                    // Walk the parents to see where this goes
                    for (level = 0, tnCurParent = tnStartParent; level <= maxLevels; ++level)
                    {
                        if (tnCurParent.NoTracking)
                        {
                            // Move along, reattach failed
                            nptPrev = nptCur;
                            break;
                        }
                        else
                        {
                            var locateData = tnCurParent.Branch.LocateObject(nptCur.myTrackerObject, ObjectStyle.TrackingObject, 0);
                            attachIndex = locateData.Row;
                            action = (TrackingObjectAction)locateData.Options;
                            // Protect against inconsistent data from clients
                            if (attachIndex == -1)
                            {
                                action = TrackingObjectAction.NotTracked;
                            }
                            switch (action)
                            {
                                case TrackingObjectAction.ThisLevel:
                                    // Detach from the list of nodes to insert
                                    if (nptPrev != null)
                                    {
                                        nptPrev.myNextSibling = nptNext;
                                    }
                                    nptCur.myNextSibling = tnCurParent.FirstPositionTracker;
                                    tnCurParent.FirstPositionTracker = nptCur;
                                    nptCur.myParentNode = tnCurParent;

                                    // Easy to break out, just pretend we're at the last level and move on.
                                    level = maxLevels;
                                    break;
                                case TrackingObjectAction.NextLevel:
                                    tnLastChildSecondary = null;
                                    tnNextParent = FindIndexedNode(attachIndex, tnCurParent.FirstChild, ref tnLastChildSecondary);
                                    if (tnNextParent != null)
                                    {
                                        if (!tnNextParent.Expanded)
                                        {
                                            tnNextParent.Expanded = true;
                                            changeCount += tnNextParent.FullCount;
                                            subItemChangeCount += tnNextParent.ExpandedSubItemGain;
                                                // UNDONE_MC: Is ExpandedSubItemGain correct?
                                            tree.ChangeFullCountRecursive(
                                                tnCurParent, tnNextParent.FullCount, tnNextParent.ExpandedSubItemGain, tnStartParent);
                                        }
                                    }
                                    else
                                    {
                                        int subItemIncr;
                                        // UNDONE_MC: Need a column
                                        tnNextParent = tree.ExpandTreeNode(
                                            tnCurParent, null, attachIndex, COLUMN_ZERO, false, out expansionCount, out subItemIncr);
                                        changeCount += tnNextParent.FullCount;
                                        subItemChangeCount += subItemIncr;
                                        tree.ChangeFullCountRecursive(tnCurParent, tnNextParent.FullCount, subItemIncr, tnStartParent);
                                        InsertIndexedNode(tnCurParent, tnNextParent, ref tnLastChildSecondary);
                                    }
                                    tnCurParent = tnNextParent;
                                    break;
                                case TrackingObjectAction.NotTracked:
                                case TrackingObjectAction.NotTrackedReturnParent:
                                    // Break out
                                    nptPrev = nptCur;
                                    level = maxLevels;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        #endregion //NODEPOSITIONTRACKER

        // Position information relative to a given TREENODE
        private struct ITEMPOSITION
        {
            public TREENODE ParentNode; //Last parent returned by TrackIndex
            public int ParentAbsolute; //Absolute position of LastParentNode
            public int Index; //Index index, relative to parent
            public int Level; //Level of ParentNode
            public int SubItemOffset; //Offset below given level

#if DEBUG
            public override string ToString()
            {
                return string.Format(
                    CultureInfo.CurrentCulture, "Index={0}, Level={1}, ParentAbsolute={2}, ParentNode={3}", Index, Level, ParentAbsolute,
                    ParentNode);
            }
#endif
            // DEBUG

            // The following helpers assume that ITEMPOSITION is fully resolved
            // with the TREENODE.TrackCell method.

            // Is the item blank?
            public bool IsBlank(int column)
            {
                return SubItemOffset > 0 || column >= ParentNode.GetColumnCount(Index);
            }

            public bool IsExpandable(int column)
            {
                if (SubItemOffset > 0
                    || column >= ParentNode.GetColumnCount(Index))
                {
                    // Blank item
                    return false;
                }
                else if (column == 0)
                {
                    if (ParentNode.NoChildExpansion)
                    {
                        return false;
                    }
                }
                else if (ParentNode.SubItemStyle(column) == SubItemCellStyles.Simple)
                {
                    return false;
                }
                return ParentNode.Branch.IsExpandable(Index, column);
            }

            public bool IsExpanded(int column)
            {
                if (SubItemOffset == 0
                    && column < ParentNode.GetColumnCount(Index)) // Test blank
                {
                    // Blank otherwise for a fully resolved position.

                    // Don't pre-check IsExpandable flags here. The assumption
                    // is that the user is smart enough to have done that already.
                    // This will still return the correct data, it is just a little slower.
                    var tnChild = ParentNode.GetChildNode(Index);
                    if (tnChild != null)
                    {
                        if (column == 0)
                        {
                            return tnChild.Expanded;
                        }
                        else
                        {
                            var sn = tnChild.SubItemAtColumn(column);
                            if (sn != null)
                            {
                                return sn.RootNode.Expanded;
                            }
                        }
                    }
                }
                return false;
            }
        }

        // ITEMPOSITION
        private struct POSITIONCACHE
        {
            public int LastAbsolute;
            public ITEMPOSITION Position;

            public void Clear()
            {
                LastAbsolute = VirtualTreeConstant.NullIndex;
            }
        }

        // A node to hold a tree for expanded sub items
        private class SUBITEMNODE
        {
            public int Column;
            public TREENODE RootNode;
            public SUBITEMNODE NextSibling;

            /// <summary>
            ///     Find the column number of the given node in the
            ///     subitem chain starting with this item
            /// </summary>
            /// <param name="tn">The root node of this item or one of its trailing siblings</param>
            /// <returns>Column number</returns>
            public int ColumnOfRootNode(TREENODE tn)
            {
                var sn = this;
                while (sn.RootNode != tn)
                {
                    sn = sn.NextSibling;
                }
                Debug.Assert(sn != null);
                return sn.Column;
            }
        }

        /// <summary>
        ///     A helper struct to use to enable trailing item counts
        ///     for subitem changes to be calculated. All of the SubItemColumnAdjust data
        ///     data is calculated during ChangeFullCountRecursive, except that
        ///     the position offsets (which enable an accurate TrailingItems value)
        ///     need to be calculated during the same TrackCell that is used to determine
        ///     the number of affected sub items.
        /// </summary>
        private struct AffectedSubItems
        {
            // Total number of affected subitems
            private int myCount;
            // A single offset, enables delayed creation of myOffsets,
            // which are not needed for the normal case.
            private int myOffset;
            // A list of trailing items
            private List<int> myOffsets;
            private readonly bool myEnabled;

            public AffectedSubItems(bool enabled)
            {
                myCount = myOffset = 0;
                myOffsets = null;
                myEnabled = enabled;
            }

            // Turn off until used (probably by caching) so FxCop doesn't gripe
            //public void Reset()
            //{
            //	myCount = 0;
            //	if (myOffsets != null)
            //	{
            //		myOffsets.Clear();
            //	}
            //}
            public int Count
            {
                get { return myCount; }
            }

            public void AddOffset(int offset)
            {
                if (!myEnabled)
                {
                    return;
                }
                if (myCount == 0)
                {
                    ++myCount;
                    myOffset = offset;
                    return;
                }
                else if (myCount == 1)
                {
                    if (myOffsets != null)
                    {
                        myOffsets.Clear();
                    }
                    else
                    {
                        myOffsets = new List<int>();
                    }
                    myOffsets.Add(myOffset);
                }
                ++myCount;
                myOffsets.Add(offset);
            }

            /// <summary>
            ///     Get back an offset added with AddOffset. The affected column comes in
            ///     reverse, so the highest offset is the first one added
            /// </summary>
            /// <param name="affectedColumn">The affected column being requested.</param>
            /// <returns></returns>
            public int GetOffset(int affectedColumn)
            {
                Debug.Assert(affectedColumn < myCount);
                if (myCount == 1)
                {
                    return myOffset;
                }
                else
                {
                    return myOffsets[affectedColumn];
                }
            }
        }

        private enum BranchCollapseAction
        {
            // Be careful on order, needs to match values in TREENODE.TreeNodeFlags
            Nothing = 0x0000, //Don't do any discarding, just unexpand the node
            CloseAndDiscard = 0x0001, //Toss this and all children
            CloseChildren = 0x0002, //Discard children (the children will also get an OnClose)
        }

        private class TREENODE
        {
            [Flags]
            internal enum TreeNodeFlags
            {
                None = 0x0,
                Expanded = 0x0001, //Is the node currently expanded?
                UpdateDelayed = 0x0002, //UpdatedRequired returned true, but node is closed
                NoChildExpansion = 0x0004, //GetExpandedList not supported
                Dynamic = 0x0008, //Support jumping from list to node (Placed in binary tree)
                NoRelocate = 0x0010, //Can't relocate expanded nodes, explicitly collapse
                CallUpdate = 0x0020, //Call UpdateRequired on a refresh
                AllowRecursion = 0x0040, //Cached return from allowRecursion in GetObject(ExpandedBranch) call
                CheckState = 0x0080, //Allow call to ToggleState on a list
                NoTracking = 0x0100, //Indices cannot be tracked
                DefaultTracking = 0x0200, //Use index directly for position tracking instead of GetTrackableObject
                MultiColumn = 0x0400, //The node is a multi-column branch
                // Be careful with next two values, order needs to match BranchCollapseAction
                // Adjust ONCOLLAPSEBITSHIFT constant if these change.
                OnCollapseCloseAndDiscard = 0x0800, //Collapse and discard all children on branch collapse
                OnCollapseCloseChildren = 0x1000, //Collapse children and ask them if they want to discard
                SubItemRoot = 0x2000, // This is the root node of an expanded SubItem cell
                ComplexSubItem = 0x4000, // This branch represents the topmost cells of a subitem, not an expanded single cell
                AllowMultiColumn = 0x8000,
                // This branch supports multi column expansions. Set for root branches where column = columns - 1.
                JaggedColumns = 0x10000, // For a multi column branch, the number of columns depends on the row
                InSubItemColumn = 0x20000, // Any branch in a subitem column that is not itself a multicolumn root will have this set
                HasImmediateSubItemGain = 0x40000,
                // A perf enhancement so we can avoid making a virtual function call to locate multi-line items
                HasFullSubItemGain = 0x80000, // A perf enhancement so we can avoid making a virtual function call
                ComplexColumns = 0x100000, // A node supports complex children. Used during initial expansion only
            }

            private const int ONCOLLAPSEBITSHIFT = 11;
#if DEBUG
            private static int NextNodeNumber;

            /// <summary>
            ///     Debug only, used to identify the TREENODE
            /// </summary>
            public readonly int NodeNumber;

            public TREENODE()
            {
                NodeNumber = NextNodeNumber++;
            }

            public override string ToString()
            {
                return string.Format(
                    CultureInfo.CurrentCulture, "NodeNumber={0}, Index={1}, ImmedCount={2}, Branch={3}", NodeNumber, Index,
                    ImmedCount, Branch);
            }
#endif
            // DEBUG
            /// <summary>
            ///     The branch associated with this node
            /// </summary>
            public IBranch Branch;

            /// <summary>
            ///     The parent node
            /// </summary>
            public TREENODE Parent;

            /// <summary>
            ///     The first child of this node
            /// </summary>
            public TREENODE FirstChild;

            /// <summary>
            ///     The next sibling of this node, used to walk the list of children from the parent node
            /// </summary>
            public TREENODE NextSibling;

            /// <summary>
            ///     The head of the position tracker list associated with this node
            /// </summary>
            public NODEPOSITIONTRACKER FirstPositionTracker;

            /// <summary>
            ///     The index of this node in the parent branch
            /// </summary>
            public int Index;

            /// <summary>
            ///     The number of children in my immediate branch.
            /// </summary>
            public int ImmedCount;

            /// <summary>
            ///     Recursive count of all expanded descendants, not including subitems
            /// </summary>
            public int FullCount;

            /// <summary>
            ///     The total number of items, including subitems and expanded subitems.
            /// </summary>
            public int TotalCount
            {
                get { return FullCount + (HasFullSubItemGain ? StoreFullSubItemGain : 0); }
            }

            /* Code is unused, but this is a good concept that may be used in the future
            /// <summary>
            /// The total number of items in my expansion, not including immediate subitems.
            /// Equivalent to FullCount + FullSubItemGain - ImmedSubItemGain
            /// </summary>
            public int		TotalExpansionCount
            {
                get
                {
                    // ImmedSubItemGain <= FullSubItemGain, so a FullSubItemGain of 0
                    // implies an ImmedSubItemGain of 0.
                    return FullCount + (HasFullSubItemGain ? (StoreFullSubItemGain - ImmedSubItemGain) : 0);
                }
            }
            */

            /// <summary>
            ///     The total number of subitems in my expansion
            /// </summary>
            public int ExpandedSubItemGain
            {
                get { return HasFullSubItemGain ? (StoreFullSubItemGain - ImmedSubItemGain) : 0; }
            }

            // The next set of properties are overridden in by different implementations
            // of TREENODE to add functionality.

            /// <summary>
            ///     The largest subitem expansion size associated with this node
            /// </summary>
            public int ImmedSubItemGain
            {
                get { return HasImmediateSubItemGain ? StoreImmedSubItemGain : 0; }
                set { StoreImmedSubItemGain = value; }
            }

            protected virtual int StoreImmedSubItemGain
            {
                get { return 0; }
                set { }
            }

            /// <summary>
            ///     The total count of subitem gains for this node and all its
            ///     children. Tracking this separately from FullCount enables
            ///     us to proffer a multi-column tree that can be simultaneously
            ///     displayed as a single-column tree.
            /// </summary>
            public int FullSubItemGain
            {
                get { return HasFullSubItemGain ? StoreFullSubItemGain : 0; }
                set { StoreFullSubItemGain = value; }
            }

            protected virtual int StoreFullSubItemGain
            {
                get { return 0; }
                set { }
            }

            /// <summary>
            ///     The next node that uses this branch. Nodes created for trackable branches
            ///     are stored in an unordered list keyed off the branch for easy notification.
            /// </summary>
            /// <value></value>
            public virtual TREENODE NextNode
            {
                get { return null; }
                set { }
            }

            /// <summary>
            ///     The previous node that uses this branch. Nodes created for trackable branches
            ///     are stored in an unordered list keyed off the branch for easy notification.
            /// </summary>
            /// <value></value>
            public virtual TREENODE PrevNode
            {
                get { return null; }
                set { }
            }

            // UpdateCounter enables use of a simple counter on a branch
            // to trigger a delayed tree update. Note that I might kill
            // this mechanism now that I have a full BranchModification
            // eventing mechanism in place.
            public virtual int UpdateCounter
            {
                get { return 0; }
                set { }
            }

            // The first subitem node off of this tree node.
            // The SUBITEMNODE indicates which column the
            // subitem expansion is on and refers to the
            // TREENODE for that item.
            public virtual SUBITEMNODE FirstSubItem
            {
                get { return null; }
                set { }
            }

            // Adjust the subitem count based on the current state of the subitem nodes
            public virtual int AdjustSubItemGain()
            {
                return 0;
            }

            public virtual SUBITEMNODE SubItemAtColumn(int column)
            {
                return null;
            }

            public virtual SUBITEMNODE SubItemAtColumn(int column, out SUBITEMNODE snPrev)
            {
                snPrev = null;
                return null;
            }

            public virtual int GetColumnCount(int row)
            {
                return 1;
            }

            public virtual SubItemCellStyles SubItemStyle(int column)
            {
                return SubItemCellStyles.Simple;
            }

            private TreeNodeFlags myFlags;

            private bool GetFlag(TreeNodeFlags bit)
            {
                return (myFlags & bit) == bit;
            }

            private bool GetAnyFlag(TreeNodeFlags bits)
            {
                return (myFlags & bits) != 0;
            }

            private void SetFlag(TreeNodeFlags bit, bool value)
            {
                if (value)
                {
                    myFlags |= bit;
                }
                else
                {
                    myFlags &= ~bit;
                }
            }

            public bool Expanded
            {
                get { return GetFlag(TreeNodeFlags.Expanded); }
                set { SetFlag(TreeNodeFlags.Expanded, value); }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public bool UpdateDelayed
            {
                get { return GetFlag(TreeNodeFlags.UpdateDelayed); }
                set { SetFlag(TreeNodeFlags.UpdateDelayed, value); }
            }

            public bool NoChildExpansion
            {
                get { return GetFlag(TreeNodeFlags.NoChildExpansion); }
                set { SetFlag(TreeNodeFlags.NoChildExpansion, value); }
            }

            public bool Dynamic
            {
                get { return GetFlag(TreeNodeFlags.Dynamic); }
                set { SetFlag(TreeNodeFlags.Dynamic, value); }
            }

            public bool NoRelocate
            {
                get { return GetFlag(TreeNodeFlags.NoRelocate); }
                set { SetFlag(TreeNodeFlags.NoRelocate, value); }
            }

            public bool CallUpdate
            {
                get { return GetFlag(TreeNodeFlags.CallUpdate); }
                set { SetFlag(TreeNodeFlags.CallUpdate, value); }
            }

            public bool AllowRecursion
            {
                get { return GetFlag(TreeNodeFlags.AllowRecursion); }
                set { SetFlag(TreeNodeFlags.AllowRecursion, value); }
            }

            public bool CheckState
            {
                get { return GetFlag(TreeNodeFlags.CheckState); }
                set { SetFlag(TreeNodeFlags.CheckState, value); }
            }

            public bool NoTracking
            {
                get { return GetFlag(TreeNodeFlags.NoTracking); }
                set { SetFlag(TreeNodeFlags.NoTracking, value); }
            }

            public bool DefaultTracking
            {
                get { return GetFlag(TreeNodeFlags.DefaultTracking); }
                set { SetFlag(TreeNodeFlags.DefaultTracking, value); }
            }

            public bool MultiColumn
            {
                get { return GetFlag(TreeNodeFlags.MultiColumn); }
                set { SetFlag(TreeNodeFlags.MultiColumn, value); }
            }

            // Setting 'SubItemRoot' also sets 'InSubItemColumn'
            public bool SubItemRoot
            {
                get { return GetFlag(TreeNodeFlags.SubItemRoot); }
                set { SetFlag(TreeNodeFlags.SubItemRoot | TreeNodeFlags.InSubItemColumn, value); }
            }

            public bool ComplexSubItem
            {
                get { return GetFlag(TreeNodeFlags.ComplexSubItem); }
                set { SetFlag(TreeNodeFlags.ComplexSubItem, value); }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public bool AllowMultiColumnChildren
            {
                get { return GetFlag(TreeNodeFlags.AllowMultiColumn); }
                set { SetFlag(TreeNodeFlags.AllowMultiColumn, value); }
            }

            public bool JaggedColumns
            {
                get { return GetFlag(TreeNodeFlags.JaggedColumns); }
                set { SetFlag(TreeNodeFlags.JaggedColumns, value); }
            }

            public bool ComplexColumns
            {
                get { return GetFlag(TreeNodeFlags.ComplexColumns); }
                set { SetFlag(TreeNodeFlags.ComplexColumns, value); }
            }

            public bool InSubItemColumn
            {
                get { return GetFlag(TreeNodeFlags.InSubItemColumn); }
                set { SetFlag(TreeNodeFlags.InSubItemColumn, value); }
            }

            public bool HasImmediateSubItemGain
            {
                get { return GetFlag(TreeNodeFlags.HasImmediateSubItemGain); }
                set { SetFlag(TreeNodeFlags.HasImmediateSubItemGain, value); }
            }

            public bool HasFullSubItemGain
            {
                get { return GetFlag(TreeNodeFlags.HasFullSubItemGain); }
                set { SetFlag(TreeNodeFlags.HasFullSubItemGain, value); }
            }

            public bool MultiLine
            {
                get { return GetAnyFlag(TreeNodeFlags.Expanded | TreeNodeFlags.HasImmediateSubItemGain); }
            }

            public BranchCollapseAction CloseAction
            {
                get
                {
                    return
                        (BranchCollapseAction)
                        ((int)(myFlags & (TreeNodeFlags.OnCollapseCloseAndDiscard | TreeNodeFlags.OnCollapseCloseChildren))
                         >> ONCOLLAPSEBITSHIFT);
                }
            }

            public static bool RequireDynamic(BranchFeatures tf)
            {
                // if BranchFeatures.DisplayDataFixed flag is not set, we always require dynamic (aka Tracking) nodes. (see bug 58253)
                // if DisplayDataFixed flag is set, then we only require dynamic nodes if the Realigns or InsertsAndDeletes features are
                // set on the branch.
                return ((tf & (BranchFeatures.Realigns | BranchFeatures.InsertsAndDeletes)) != 0)
                       || ((tf & BranchFeatures.DisplayDataFixed) == 0);
            }

            public static bool RequireUpdatable(BranchFeatures tf)
            {
                return (tf & BranchFeatures.DelayedUpdates) != 0;
            }

            public void SetFlags(BranchFeatures tf)
            {
                // Don't clear myFlags because this messes up a second call to this routine, such
                // as when a new branch is attached to the same not during a level shift. Not all
                // flags are calculated from the tree flags.
                NoChildExpansion = (tf & BranchFeatures.Expansions) == 0;
                NoRelocate = (tf & BranchFeatures.BranchRelocation) == 0;
                CallUpdate = RequireUpdatable(tf);
                Dynamic = RequireDynamic(tf);
                CheckState = (tf & BranchFeatures.StateChanges) != 0;
                DefaultTracking = (tf & BranchFeatures.DefaultPositionTracking) != 0;
                NoTracking = DefaultTracking ? false : ((tf & BranchFeatures.PositionTracking) == 0);
                JaggedColumns = (tf & BranchFeatures.JaggedColumns) != 0;
                ComplexColumns = (tf & BranchFeatures.ComplexColumns) != 0;
                if (0 != (tf & BranchFeatures.OnCollapseCloseAndDiscard))
                {
                    SetFlag(TreeNodeFlags.OnCollapseCloseAndDiscard, true);
                }
                else if (0 != (tf & BranchFeatures.OnCollapseCloseChildren))
                {
                    SetFlag(TreeNodeFlags.OnCollapseCloseChildren, true);
                }
            }

            public void TransferPositionTrackerTo(TREENODE tn)
            {
                if (FirstPositionTracker == null)
                {
                    return;
                }
                Debug.Assert(tn.FirstPositionTracker == null);
                tn.FirstPositionTracker = FirstPositionTracker;
                FirstPositionTracker = null;
                NODEPOSITIONTRACKER.UpdateParentNode(tn.FirstPositionTracker, tn);
            }

            // TrackRow functions are called for  a root node (either the tree root
            // or a subitem root) to get the position information for the given offset. The 
            // IgnoreOffsets functions can be used to provide position information that
            // enables a multi-column tree to be represented simultaneously as a single-column
            // tree.
            public ITEMPOSITION TrackRowIgnoreOffsets(int absRow)
            {
                if (absRow > FullCount)
                {
                    throw new ArgumentOutOfRangeException("absRow");
                }
                TREENODE tn1;
                TREENODE tn2;
                var curIndex = 0;
                var pos = new ITEMPOSITION();
                pos.ParentNode = tn2 = this;
                pos.ParentAbsolute = VirtualTreeConstant.NullIndex;
                tn2 = NextExpanded(tn2.FirstChild);
                if (tn2 != null)
                {
                    for (;;)
                    {
                        curIndex += tn2.Index;
                        if (curIndex >= absRow)
                        {
                            pos.Index = tn2.Index + absRow - curIndex;
                            break;
                        }
                        else if (curIndex + tn2.FullCount >= absRow)
                        {
                            absRow -= ++curIndex;
                            pos.ParentAbsolute += curIndex;
                            curIndex = 0;
                            tn1 = tn2;
                            tn2 = NextExpanded(tn1.FirstChild);
                            pos.ParentNode = tn1;
                            ++pos.Level;
                            if (tn2 == null)
                            {
                                pos.Index = absRow;
                                break;
                            }
                        }
                        else
                        {
                            curIndex += tn2.FullCount - tn2.Index;
                            tn1 = NextExpanded(tn2.NextSibling);
                            if (tn1 != null)
                            {
                                tn2 = tn1;
                            }
                            else
                            {
                                pos.Index = absRow - curIndex;
                                break;
                            }
                        }
                    } //for(;;)
                }
                else
                {
                    pos.Index = absRow;
                }
                return pos;
            }

            // This does not need a return value. cache.Position can always be
            // considered the return value. This does not necessarily apply to
            // TrackRows, where the position cache can apply to one level only.
            public void TrackRowIgnoreOffsets(int absRow, ref POSITIONCACHE cache)
            {
                cache.Position = TrackRowIgnoreOffsets(absRow);
                if (absRow != cache.LastAbsolute)
                {
                    if (cache.LastAbsolute != VirtualTreeConstant.NullIndex)
                    {
                        var lastParentAbsolute = cache.Position.ParentAbsolute;
                        var lastLevel = cache.Position.Level;
                        var lastParent = cache.Position.ParentNode;

                        if ((lastParentAbsolute == VirtualTreeConstant.NullIndex)
                            ||
                            (lastParentAbsolute < absRow &&
                             absRow < lastParentAbsolute + lastParent.FullCount))
                        {
                            cache.Position = lastParent.TrackRowIgnoreOffsets(absRow - lastParentAbsolute - 1);
                            // UNDONE_CACHE: Verify next two lines
                            cache.Position.Level += lastLevel;
                            cache.Position.ParentAbsolute += lastParentAbsolute + 1;
                        }
                        else
                        {
                            cache.Position = TrackRowIgnoreOffsets(absRow);
                        }
                    }
                    else
                    {
                        cache.Position = TrackRowIgnoreOffsets(absRow);
                    }
                    cache.LastAbsolute = absRow;
                }
            }

            public int TranslateSingleColumnRow(int singleColumnRow)
            {
                if (singleColumnRow >= FullCount)
                {
                    throw new ArgumentOutOfRangeException("singleColumnRow");
                }
                var tn = this;
                var startSingleColumnRow = singleColumnRow;
                var totalSubItemGain = 0;
                var curIndex = 0;
                tn = NextMultiLineSibling(tn.FirstChild);
                if (tn != null)
                {
                    for (;;)
                    {
                        curIndex += tn.Index;
                        if (curIndex >= singleColumnRow)
                        {
                            break;
                        }
                        else if (!tn.Expanded)
                        {
                            // The node was returned from NextMultiLineSibling because of the
                            // subitem gain, not the expansion. Move to the next node.
                            totalSubItemGain += tn.ImmedSubItemGain;
                            curIndex -= tn.Index;
                            tn = NextMultiLineSibling(tn.NextSibling);
                            if (tn == null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (curIndex + tn.FullCount >= singleColumnRow)
                            {
                                totalSubItemGain += tn.ImmedSubItemGain;
                                singleColumnRow -= ++curIndex;
                                curIndex = 0;
                                tn = NextMultiLineSibling(tn.FirstChild);
                                if (tn == null)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                totalSubItemGain += tn.FullSubItemGain;
                                curIndex += tn.FullCount - tn.Index;
                                tn = NextMultiLineSibling(tn.NextSibling);
                                if (tn == null)
                                {
                                    break;
                                }
                            }
                        }
                    } //for(;;)
                }
                return startSingleColumnRow + totalSubItemGain;
            }

            public ITEMPOSITION TrackRow(int absRow)
            {
                var singleColumnSubItemAdjust = 0;
                return TrackRow(absRow, ref singleColumnSubItemAdjust);
            }

            public virtual ITEMPOSITION TrackRow(int absRow, ref int singleColumnSubItemAdjust)
            {
                // Also change TrackCell if this call changes
                return TrackRowIgnoreOffsets(absRow);
            }

            public virtual ITEMPOSITION TrackRow(int absRow, ref POSITIONCACHE cache)
            {
                TrackRowIgnoreOffsets(absRow, ref cache);
                return cache.Position;
            }

            /// <summary>
            ///     Track the position information for the given row and cell relative to
            ///     this TREENODE. Called recursively starting with the root node of a tree
            ///     to get information for any item in a multi-column tree.
            /// </summary>
            /// <param name="absRow">The offset of the row to locate, relative to this node</param>
            /// <param name="column">The column offset relative to this node</param>
            /// <param name="parentRowOffset">A cumulative offset of the number of rows above this node needed to reach the given item</param>
            /// <param name="affectedSubItemColumns">The number of subitem columns that are affected by toggling the expansion at this position</param>
            /// <param name="lastSubItem">Whether this item is the trailing item in the subitem cell</param>
            /// <param name="singleColumnSubItemAdjust">The number of blanks above the given item. absRow - singleColumnSubItemAdjust provides the single-column index</param>
            /// <returns>The position object for the given cell</returns>
            public virtual ITEMPOSITION TrackCell(
                int absRow, ref int column, ref int parentRowOffset, ref AffectedSubItems affectedSubItemColumns, ref bool lastSubItem,
                ref int singleColumnSubItemAdjust)
            {
                return TrackRowIgnoreOffsets(absRow);
            }

            protected static TREENODE NextExpanded(TREENODE sibling)
            {
                TREENODE tn;
                for (tn = sibling; tn != null; tn = tn.NextSibling)
                {
                    if (tn.Expanded)
                    {
                        break;
                    }
                }
                return tn;
            }

            protected static TREENODE NextMultiLineSibling(TREENODE sibling)
            {
                TREENODE tn;
                for (tn = sibling; tn != null; tn = tn.NextSibling)
                {
                    if (tn.MultiLine)
                    {
                        break;
                    }
                }
                return tn;
            }

            public TREENODE GetChildNode(int index)
            {
                TREENODE tn;
                for (tn = FirstChild; tn != null; tn = tn.NextSibling)
                {
                    if (index == tn.Index)
                    {
                        return tn;
                    }
                }
                return null;
            }

            public TREENODE GetChildNode(int index, out TREENODE tnPrev)
            {
                TREENODE tn;
                tnPrev = null;
                for (tn = FirstChild; tn != null; tn = tn.NextSibling)
                {
                    if (index == tn.Index)
                    {
                        return tn;
                    }
                    tnPrev = tn;
                }
                return null;
            }

            public int GetChildOffset(int relativeIndex)
            {
                var singleColumnSubItemAdjust = 0;
                return GetChildOffset(relativeIndex, out singleColumnSubItemAdjust);
            }

            public int GetChildOffset(int relativeIndex, out int singleColumnSubItemAdjust)
            {
                // Account for immediate child items, item 0 has an offset of 1 from its parent,
                // plus the subitem gain of the parent.
                var subItemAdjust = ImmedSubItemGain;
                var index = relativeIndex + 1;
                //Account for expanded items
                var tnTmp = FirstChild;
                while ((tnTmp != null)
                       && (tnTmp.Index < relativeIndex))
                {
                    if (tnTmp.Expanded)
                    {
                        index += tnTmp.FullCount;
                        subItemAdjust += tnTmp.FullSubItemGain;
                    }
                    else
                    {
                        subItemAdjust += tnTmp.ImmedSubItemGain;
                    }
                    tnTmp = tnTmp.NextSibling;
                }
                singleColumnSubItemAdjust = subItemAdjust;
                return index + subItemAdjust;
            }

            public int GetSingleColumnChildOffset(int relativeIndex)
            {
                var index = relativeIndex + 1;
                var tnTmp = FirstChild;
                while ((tnTmp != null)
                       && (tnTmp.Index < relativeIndex))
                {
                    if (tnTmp.Expanded)
                    {
                        index += tnTmp.FullCount;
                    }
                    tnTmp = tnTmp.NextSibling;
                }
                return index;
            }
        }

        // Adds support for FullSubItemGain. This is used in place
        // of TREENODE derivatives for single-column branches in a
        // multicolumn tree
        private class TREENODE_Single : TREENODE
        {
            private int myFullSubItemGain;

            protected override int StoreFullSubItemGain
            {
                get { return myFullSubItemGain; }
                set
                {
                    myFullSubItemGain = value;
                    HasFullSubItemGain = value != 0;
                }
            }

            public override sealed ITEMPOSITION TrackRow(int absRow, ref POSITIONCACHE cache)
            {
                // UNDONE_CACHE: Never got this right in the prototype, work out later.
                // May want to switch to an array of caches (one per column)
                return TrackRow(absRow);
            }

            public override sealed ITEMPOSITION TrackRow(int absRow, ref int singleColumnSubItemAdjust)
            {
                if (absRow >= TotalCount)
                {
                    throw new ArgumentOutOfRangeException("absRow");
                }
                TREENODE tn1;
                TREENODE tn2;
                int curImmedSubItemGain; // ImmedSubItemGain is virtual, avoid calling multiple times.
                int curTotalCount;
                int curFullSubItemGain;
                var curIndex = 0;
                var pos = new ITEMPOSITION();
                pos.ParentNode = tn2 = this;
                pos.ParentAbsolute = VirtualTreeConstant.NullIndex;
                var parentAbsoluteAdjust = 0;
                tn2 = NextMultiLineSibling(tn2.FirstChild);
                if (tn2 != null)
                {
                    for (;;)
                    {
                        curIndex += tn2.Index;
                        if (curIndex >= absRow)
                        {
                            pos.Index = tn2.Index + absRow - curIndex;
                            break;
                        }
                        else if ((curImmedSubItemGain = tn2.ImmedSubItemGain) + curIndex >= absRow)
                        {
                            pos.Index = tn2.Index;
                            pos.SubItemOffset = absRow - curIndex;
                            break;
                        }
                        else if (!tn2.Expanded)
                        {
                            // The node was returned from NextMultiLineSibling because of the
                            // subitem gain, not the expansion. Move to the next node.
                            curIndex += curImmedSubItemGain - tn2.Index;
                            singleColumnSubItemAdjust += curImmedSubItemGain;
                            tn2 = NextMultiLineSibling(tn2.NextSibling);
                            if (tn2 == null)
                            {
                                // Same as else clause below
                                pos.Index = absRow - curIndex;
                                pos.SubItemOffset = 0;
                                break;
                            }
                        }
                        else
                        {
                            curFullSubItemGain = tn2.FullSubItemGain;
                            curTotalCount = curFullSubItemGain + tn2.FullCount;
                            if (curIndex + curTotalCount >= absRow)
                            {
                                curIndex += curImmedSubItemGain;
                                absRow -= ++curIndex;
                                singleColumnSubItemAdjust += curImmedSubItemGain;
                                pos.ParentAbsolute += curIndex;
                                // We want the subitemgain for recursion cases, but
                                // not if we end up returning this parent absolute.
                                // Save this value so we can take it off later.
                                parentAbsoluteAdjust = curImmedSubItemGain;
                                curIndex = 0;
                                tn1 = tn2;
                                tn2 = NextMultiLineSibling(tn1.FirstChild);
                                pos.ParentNode = tn1;
                                ++pos.Level;
                                if (tn2 == null)
                                {
                                    pos.Index = absRow;
                                    break;
                                }
                            }
                            else
                            {
                                curIndex += curTotalCount - tn2.Index; // UNDONE_SUBITEM Does this need a SubItemGain adjust?
                                singleColumnSubItemAdjust += curFullSubItemGain;
                                tn1 = NextMultiLineSibling(tn2.NextSibling);
                                if (tn1 != null)
                                {
                                    tn2 = tn1;
                                }
                                else
                                {
                                    pos.Index = absRow - curIndex;
                                    pos.SubItemOffset = 0; // UNDONE_SUBITEM
                                    break;
                                }
                            }
                        }
                    } //for(;;)
                    pos.ParentAbsolute -= parentAbsoluteAdjust;
                }
                else
                {
                    pos.Index = absRow;
                }
                return pos;
            }

            public override sealed ITEMPOSITION TrackCell(
                int absRow, ref int column, ref int parentRowOffset, ref AffectedSubItems affectedSubItemColumns, ref bool lastSubItem,
                ref int singleColumnSubItemAdjust)
            {
                var pos = TrackRow(absRow, ref singleColumnSubItemAdjust);
                if (column > 0)
                {
                    // UNDONE_MC: Verify this
                    affectedSubItemColumns.AddOffset(pos.SubItemOffset);
                    lastSubItem = false;
                    TREENODE tnChild;
                    SUBITEMNODE sn;
                    var parentColumns = pos.ParentNode.GetColumnCount(pos.Index);
                    if (column < parentColumns)
                    {
                        pos.Level = 0;
                        if (0 != (pos.ParentNode.SubItemStyle(column) & SubItemCellStyles.Mixed))
                        {
                            tnChild = pos.ParentNode.GetChildNode(pos.Index);
                            sn = (tnChild == null) ? null : tnChild.SubItemAtColumn(column);
                            // This cell is in the range of columns for the current position
                            if (sn != null
                                && sn.RootNode.Expanded)
                            {
                                lastSubItem = tnChild.ImmedSubItemGain == pos.SubItemOffset;
                                var isComplex = sn.RootNode.ComplexSubItem;
                                if (pos.SubItemOffset > 0 || isComplex)
                                {
                                    if (pos.SubItemOffset < (sn.RootNode.TotalCount + (isComplex ? 0 : 1)))
                                    {
                                        // Need a new position in a different tree, absRow is the SubItemOffset
                                        column = 0;
                                        parentRowOffset += absRow - pos.SubItemOffset;
                                        // TrackRow is sufficient with column 0, don't recurse
                                        if (isComplex)
                                        {
                                            pos = sn.RootNode.TrackRow(pos.SubItemOffset);
                                        }
                                        else
                                        {
                                            pos = sn.RootNode.TrackRow(pos.SubItemOffset - 1);
                                            ++parentRowOffset;
                                            ++pos.Level;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // The column is past the range of columns provided by the branch, but
                        // it may be a subitem of a complex cell in the last column of this tree
                        if (0 != (pos.ParentNode.SubItemStyle(parentColumns - 1) & SubItemCellStyles.Complex))
                        {
                            tnChild = pos.ParentNode.GetChildNode(pos.Index);
                            sn = (tnChild == null) ? null : tnChild.SubItemAtColumn(parentColumns - 1);
                            if (sn != null
                                && sn.RootNode.ComplexSubItem
                                && sn.RootNode.MultiColumn)
                            {
                                column -= parentColumns;
                                parentRowOffset += absRow - pos.SubItemOffset;
                                // There is nothing to be done with singleColumnSubItemAdjust, it is undefined
                                // if the column is not 0
                                pos = sn.RootNode.TrackCell(
                                    pos.SubItemOffset, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem,
                                    ref singleColumnSubItemAdjust);
                            }
                        }
                    }
                }
                return pos;
            }
        }

        // Adds support for subitem expansion.
        private class TREENODE_Complex : TREENODE_Single
        {
            private int myImmedSubItemGain;
            private SUBITEMNODE myFirstSubItem;

            protected override sealed int StoreImmedSubItemGain
            {
                get { return myImmedSubItemGain; }
                set
                {
                    myImmedSubItemGain = value;
                    HasImmediateSubItemGain = value != 0;
                }
            }

            public override sealed SUBITEMNODE FirstSubItem
            {
                get { return myFirstSubItem; }
                set { myFirstSubItem = value; }
            }

            public override sealed int AdjustSubItemGain()
            {
                var sn = myFirstSubItem;
                TREENODE tn;
                var maxGain = 0;
                int curGain;
                while (sn != null)
                {
                    tn = sn.RootNode;
                    Debug.Assert(tn.ImmedSubItemGain == 0); // A root node is not visible and should have no immediate subitem rows
                    if (tn.Expanded)
                    {
                        curGain = tn.TotalCount;
                        if (tn.ComplexSubItem)
                        {
                            --curGain;
                        }
                        if (curGain > maxGain)
                        {
                            maxGain = curGain;
                        }
                    }
                    sn = sn.NextSibling;
                }
                curGain = maxGain - myImmedSubItemGain;
                // Use property setter here so we can set the HasImmediateSubItemGain flag
                ImmedSubItemGain = maxGain;
                return curGain;
            }

            public override sealed SUBITEMNODE SubItemAtColumn(int column)
            {
                var sn = FirstSubItem;
                while (sn != null)
                {
                    if (sn.Column >= column)
                    {
                        return (sn.Column == column) ? sn : null;
                    }
                    sn = sn.NextSibling;
                }
                return null;
            }

            public override sealed SUBITEMNODE SubItemAtColumn(int column, out SUBITEMNODE snPrev)
            {
                var sn = FirstSubItem;
                snPrev = null;
                while (sn != null)
                {
                    if (sn.Column >= column)
                    {
                        return (sn.Column == column) ? sn : null;
                    }
                    snPrev = sn;
                    sn = sn.NextSibling;
                }
                return null;
            }
        }

        // Adds support for multi column branches.
        private class TREENODE_Multi : TREENODE_Complex
        {
            public override sealed int GetColumnCount(int row)
            {
                var mcBranch = Branch as IMultiColumnBranch;
                return (mcBranch == null)
                           ? 1
                           : JaggedColumns ? mcBranch.GetJaggedColumnCount(row) : mcBranch.ColumnCount;
            }

            public override sealed SubItemCellStyles SubItemStyle(int column)
            {
                Debug.Assert((Branch as IMultiColumnBranch) != null); // Caught in ColumnCount
                return (Branch as IMultiColumnBranch).ColumnStyles(column);
            }
        }

        #region Tracked and Updatable versions of the different tree node styles

        // Trackable versions of the three main node styles
        private class TREENODE_Tracked : TREENODE
        {
            public override TREENODE NextNode { get; set; }

            public override TREENODE PrevNode { get; set; }
        }

        private class TREENODE_Single_Tracked : TREENODE_Single
        {
            public override TREENODE NextNode { get; set; }

            public override TREENODE PrevNode { get; set; }
        }

        private class TREENODE_Complex_Tracked : TREENODE_Complex
        {
            public override TREENODE NextNode { get; set; }

            public override TREENODE PrevNode { get; set; }
        }

        private class TREENODE_Multi_Tracked : TREENODE_Multi
        {
            public override TREENODE NextNode { get; set; }

            public override TREENODE PrevNode { get; set; }
        }

        // Updatable versions of the three node styles
        private class TREENODE_Updatable : TREENODE
        {
            public override int UpdateCounter { get; set; }
        }

        private class TREENODE_Single_Updatable : TREENODE_Single
        {
            public override int UpdateCounter { get; set; }
        }

        private class TREENODE_Complex_Updatable : TREENODE_Complex
        {
            public override int UpdateCounter { get; set; }
        }

        private class TREENODE_Multi_Updatable : TREENODE_Multi
        {
            public override int UpdateCounter { get; set; }
        }

        // Tracked and updatable versions of the three node styles
        private class TREENODE_Tracked_Updatable : TREENODE_Tracked
        {
            public override int UpdateCounter { get; set; }
        }

        private class TREENODE_Single_Tracked_Updatable : TREENODE_Single_Tracked
        {
            public override int UpdateCounter { get; set; }
        }

        private class TREENODE_Complex_Tracked_Updatable : TREENODE_Complex_Tracked
        {
            public override int UpdateCounter { get; set; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
        private class TREENODE_Multi_Tracked_Updatable : TREENODE_Multi_Tracked
        {
            public override int UpdateCounter { get; set; }
        }

        #endregion // Tracked and Updatable versions of the different tree node styles

        private class TrackedTreeNodeCollection : DictionaryBase
        {
            // Return the first tracked node for this branch
            public TREENODE this[IBranch branch]
            {
                get
                {
                    Debug.Assert(branch != null);
                    return (TREENODE)Dictionary[branch];
                }
            }

            public void Add(IBranch branch, TREENODE value)
            {
                // Add this item the an existing branch if any
                var existing = this[branch];
                if (existing == null)
                {
                    Dictionary.Add(branch, value);
                }
                else
                {
                    // Add to end of the list. The end is important because
                    // value might already have nodes attached to it. We
                    // need to walk one list or the other.
                    while (existing.NextNode != null)
                    {
                        existing = existing.NextNode;
                    }
                    existing.NextNode = value;
                    value.PrevNode = existing;
                }
            }

            public void Remove(IBranch branch, TREENODE value)
            {
                var existing = this[branch];
                if (existing == null)
                {
                    return;
                }
                if (existing == value)
                {
                    if (existing.NextNode == null)
                    {
                        Dictionary.Remove(branch);
                    }
                    else
                    {
                        existing.NextNode.PrevNode = null;
                        Dictionary[branch] = existing.NextNode;
                    }
                }
                else
                {
                    Debug.Assert(value.PrevNode != null);
                    value.PrevNode.NextNode = existing.NextNode;
                    if (value.NextNode != null)
                    {
                        value.NextNode.PrevNode = value.PrevNode;
                    }
                }
            }

            public void ReBranchTreeNode(IBranch newBranch, TREENODE value)
            {
                Remove(value.Branch, value);
                Add(newBranch, value);
                value.Branch = newBranch;
            }

            public void RemoveBranch(IBranch branch)
            {
                Dictionary.Remove(branch);
            }
        }

        private class ColumnItemEnumeratorImpl : ColumnItemEnumerator
        {
            private readonly VirtualTree myTree;

            public ColumnItemEnumeratorImpl(
                VirtualTree tree, int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow)
                :
                    base(column, columnPermutation, returnBlankAnchors, startRow, endRow, (tree as ITree).VisibleItemCount)
            {
                myTree = tree;
            }

            public ColumnItemEnumeratorImpl(
                VirtualTree tree, int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter,
                bool markExcludedFilterItems)
                :
                    base(column, columnPermutation, returnBlankAnchors, rowFilter, markExcludedFilterItems)
            {
                myTree = tree;
            }

            protected override sealed void GetNextSection()
            {
                Debug.Assert(CurrentBranch == null);
                var startRow = NextStartRow;
                IBranch branch;
                int firstRelRow;
                int lastRelRow;
                int relColumn;
                int sectionLevel;
                int treeColumn;
                int blanks;
                bool simpleCell;
                myTree.EnumOrderedListItems(
                    ref startRow, EnumerationColumn, ColumnPermutation, ReturnBlankAnchors, out branch, out treeColumn, out firstRelRow,
                    out lastRelRow, out relColumn, out sectionLevel, out blanks, out simpleCell);
                NextStartRow = startRow;
                CurrentTrailingBlanks = blanks;
                if (branch != null)
                {
                    // The remaining settings are only important if the branch is set.
                    CurrentBranch = branch;
                    FirstRelativeRow = firstRelRow;
                    LastRelativeRow = lastRelRow;
                    RelativeColumn = relColumn;
                    CurrentLevel = sectionLevel;
                    CurrentCellIsSimple = simpleCell;
                    CurrentTreeColumn = treeColumn;
                }
            }
        }

        private class ColumnItemEnumeratorSingleColumnImpl : ColumnItemEnumerator
        {
            private readonly VirtualTree myTree;

            public ColumnItemEnumeratorSingleColumnImpl(VirtualTree tree, int startRow, int endRow)
                :
                    base(0, null, false, startRow, endRow, (tree as ITree).VisibleItemCount)
            {
                myTree = tree;
            }

            public ColumnItemEnumeratorSingleColumnImpl(VirtualTree tree, int[] rowFilter, bool markExcludedFilterItems)
                :
                    base(0, null, false, rowFilter, markExcludedFilterItems)
            {
                myTree = tree;
            }

            protected override sealed void GetNextSection()
            {
                Debug.Assert(CurrentBranch == null);
                var startRow = NextStartRow;
                IBranch branch;
                int firstRelRow;
                int lastRelRow;
                int sectionLevel;
                myTree.EnumSingleColumnOrderedListItems(ref startRow, out branch, out firstRelRow, out lastRelRow, out sectionLevel);
                NextStartRow = startRow;
                if (branch != null)
                {
                    // The remaining settings are only important if the branch is set.
                    CurrentBranch = branch;
                    FirstRelativeRow = firstRelRow;
                    LastRelativeRow = lastRelRow;
                    CurrentLevel = sectionLevel;
                }
            }
        }

        [Flags]
        private enum TreeStateFlags
        {
            /// <summary>
            ///     Block redraw toggles while grabbing a new tree branch
            /// </summary>
            InExpansion = 1,

            /// <summary>
            ///     Redrawing is currently delayed and needs to be turned off
            ///     before other notifications are sent.
            /// </summary>
            TurnOffRedraw = 2,

            /// <summary>
            ///     Redrawing is currently delayed in the single column view of
            ///     the tree and needs to be turned off before other notifications are sent.
            /// </summary>
            TurnOffSingleColumnRedraw = 4,

            /// <summary>
            ///     Shuffling is currently delayed and needs to be turned off before other notifications are sent.
            /// </summary>
            TurnOffShuffle = 8,

            /// <summary>
            ///     Support multiple columns. A derived class that sets this via EnableMultiColumn must support IMultiColumnTree.
            /// </summary>
            MultiColumnSupport = 0x10,
            // The following items track single-column events so that we don't have
            // to go through multiple virtual function calls just to see if we need to get data
            // to fire the event.
            FireSingleColumnItemCountChanged = 0x20,
            FireSingleColumnItemMoved = 0x40,
            FireSingleColumnStateToggled = 0x80,
            FireSingleColumnOnDisplayDataChanged = 0x100,
            FireSingleColumnOnSetRedraw = 0x200,
        }

        private bool GetStateFlag(TreeStateFlags bit)
        {
            return (myFlags & bit) == bit;
        }

        private void SetStateFlag(TreeStateFlags bit, bool value)
        {
            if (value)
            {
                myFlags |= bit;
            }
            else
            {
                myFlags &= ~bit;
            }
        }

        // Main class variables
        private TREENODE myRootNode;
        private TreeStateFlags myFlags;
        private short myRedrawCount; //A reference counter for the redraw state
        private short myDelayRedrawCount; //A reference counter for delayed redraw state
        private short myDelayShuffleCount; //A reference counter for delayed list shuffle redraw state
        private short myShuffleCount; //A reference counter for the list shuffling state
        private TrackedTreeNodeCollection myNodeTracker;
        private PositionManagerEventArgs myPositionManager;
        private NODEPOSITIONTRACKER myPositionHead;

        // Event definitions

        /// <summary>
        ///     The number of items in the tree has changed
        /// </summary>
        public event ItemCountChangedEventHandler ItemCountChanged;

        /// <summary>
        ///     An item has moved in the tree
        /// </summary>
        public event ItemMovedEventHandler ItemMoved;

        /// <summary>
        ///     The state of a checkbox has been toggled
        /// </summary>
        public event ToggleStateEventHandler StateToggled;

        /// <summary>
        ///     The tree must be refreshed
        /// </summary>
        public event RefreshEventHandler OnRefresh;

        /// <summary>
        ///     Redraw is being turned on/off.
        /// </summary>
        public event SetRedrawEventHandler OnSetRedraw;

        /// <summary>
        ///     Determine whether an item is visible
        /// </summary>
        public event QueryItemVisibleEventHandler OnQueryItemVisible;

        /// <summary>
        ///     A list shuffle is beginning, cache position information
        /// </summary>
        public event ListShuffleEventHandler ListShuffleBeginning;

        /// <summary>
        ///     A list shuffle is ending, apply changes to position information
        /// </summary>
        public event ListShuffleEventHandler ListShuffleEnding;

        /// <summary>
        ///     Display information must be refreshed
        /// </summary>
        public event DisplayDataChangedEventHandler OnDisplayDataChanged;

        /// <summary>
        ///     A SynchronizeState call is beginning.
        /// </summary>
        public event SynchronizeStateEventHandler SynchronizationBeginning;

        /// <summary>
        ///     A SynchronizeState call is ending.
        /// </summary>
        public event SynchronizeStateEventHandler SynchronizationEnding;

        private POSITIONCACHE myPosCache;

        /// <summary>
        ///     Call from the constructor to turn on MultiColumn support.
        /// </summary>
        protected void EnableMultiColumn()
        {
            SetStateFlag(TreeStateFlags.MultiColumnSupport, true);
        }

        /// <summary>
        ///     Override on a class that also implements IMultiColumnTree to
        ///     provide a single column view on the multi-column tree.
        /// </summary>
        /// <value>An alternate view on a multi-column tree</value>
        protected virtual ITree SingleColumnTree
        {
            get { return null; }
        }

        /// <summary>
        ///     Create a new single column tree. Use this method when overriding the
        ///     SingleColumnTree property to create the returned object.This function is
        ///     only useful for derived classes that implement IMultiColumnTree.
        /// </summary>
        /// <returns>A new single column view on this tree.</returns>
        protected ITree CreateSingleColumnTree()
        {
            return new SingleColumnView(this);
        }

        private bool MultiColumnSupport
        {
            get { return 0 != (myFlags & TreeStateFlags.MultiColumnSupport); }
        }

        /// <summary>
        ///     Override on a class that also implements IMultiColumnTree to
        ///     provide multi column support.
        /// </summary>
        /// <value>The number of supported columns. 1 for a single column tree.</value>
        protected virtual int ColumnCount
        {
            get { return 1; }
        }

        private void ClearPositionCache()
        {
            myPosCache.Clear();
        }

        private ITEMPOSITION TrackSingleColumnRow(int absRow)
        {
            // UNDONE: Caching, the single-column tree should
            // maintain its own position cache and pass it in here.
            return myRootNode.TrackRowIgnoreOffsets(absRow);
        }

        /// <summary>
        ///     Translate a single column row into its multi column equivalent
        /// </summary>
        /// <param name="singleColumnRow">The single column row to translate</param>
        /// <returns>The multi column equivalent</returns>
        internal int TranslateSingleColumnRow(int singleColumnRow)
        {
            return myRootNode.TranslateSingleColumnRow(singleColumnRow);
        }

        /// <summary>
        ///     Translate a multi column row into its single column equivalent
        /// </summary>
        /// <param name="multiColumnRow">The multi column row to translate</param>
        /// <returns>The single column equivalent</returns>
        internal int TranslateMultiColumnRow(int multiColumnRow)
        {
            var singleColumnSubItemAdjust = 0;
            var pos = myRootNode.TrackRow(multiColumnRow, ref singleColumnSubItemAdjust);
            return multiColumnRow - singleColumnSubItemAdjust - pos.SubItemOffset;
        }

        /// <summary>
        ///     Find the first non-blank column for the given row.
        /// </summary>
        /// <param name="row">The row to test</param>
        /// <returns>A valid column number</returns>
        internal int FindFirstNonBlankColumn(int row)
        {
            var pos = myRootNode.TrackRow(row);
            var offset = pos.SubItemOffset;
            if (offset > 0)
            {
                // We're looking at a blank item. Walk the subitems
                // until we a find a column with a size sufficient
                // for this offset.
                var tn = pos.ParentNode.GetChildNode(pos.Index);
                var sn = tn.FirstSubItem;
                int maxOffset;
                while (sn != null)
                {
                    maxOffset = sn.RootNode.TotalCount;
                    if (sn.RootNode.ComplexSubItem)
                    {
                        --maxOffset;
                    }
                    if (offset <= maxOffset)
                    {
                        // UNDONE_MC: This won't work with nested multicolumn
                        // branches. Move the bulk of this code into TREENODE and
                        // let it recurse to handle this case cleanly.
                        return sn.Column;
                    }
                    sn = sn.NextSibling;
                }
            }
            return 0;
        }

        // Return the item info for the given item in the tree. If the
        // item is not blank, then SubItemOffset will not be set. Column
        // will be adjusted to a value relative to the multicolumn
        // branch being returned.
        private ITEMPOSITION TrackCell(int absRow, ref int column)
        {
            if (column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            // UNDONE_CACHE
            var parentRowOffset = 0;
            var affectedSubItemColumns = new AffectedSubItems(false);
            var lastSubItem = false;
            var singleColumnSubItemAdjust = 0;
            return myRootNode.TrackCell(
                absRow, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem, ref singleColumnSubItemAdjust);
        }

        // Return the item info for the given item in the tree. If the
        // item is not blank, then SubItemOffset will not be set. Column
        // will be adjusted to a value relative to the multicolumn
        // branch being returned.
        private ITEMPOSITION TrackCell(int absRow, ref int column, ref int singleColumnSubItemAdjust)
        {
            if (column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            // UNDONE_CACHE
            var parentRowOffset = 0;
            var affectedSubItemColumns = new AffectedSubItems(false);
            var lastSubItem = false;
            return myRootNode.TrackCell(
                absRow, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem, ref singleColumnSubItemAdjust);
        }

        private ITEMPOSITION TrackCell(int absRow, ref int column, ref bool lastSubItem)
        {
            if (column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            // UNDONE_CACHE
            var parentRowOffset = 0;
            var affectedSubItemColumns = new AffectedSubItems(false);
            var singleColumnSubItemAdjust = 0;
            return myRootNode.TrackCell(
                absRow, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem, ref singleColumnSubItemAdjust);
        }

        private ITEMPOSITION TrackCell(int absRow, ref int column, ref int parentRowOffset, ref int singleColumnSubItemAdjust)
        {
            if (column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            // UNDONE_CACHE
            var affectedSubItemColumns = new AffectedSubItems(false);
            var lastSubItem = false;
            return myRootNode.TrackCell(
                absRow, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem, ref singleColumnSubItemAdjust);
        }

        private ITEMPOSITION TrackCell(
            int absRow, ref int column, ref int parentRowOffset, ref AffectedSubItems affectedSubItemColumns,
            ref int singleColumnSubItemAdjust)
        {
            if (column >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            // UNDONE_CACHE
            var lastSubItem = false;
            return myRootNode.TrackCell(
                absRow, ref column, ref parentRowOffset, ref affectedSubItemColumns, ref lastSubItem, ref singleColumnSubItemAdjust);
        }

        private void OnBranchModification(object sender, BranchModificationEventArgs change)
        {
            //Decode the BranchModification structure to correspond to methods on the
            //tree. One branch can be in multiple trees. This event-based indirection
            //enables the branches to multicast instead of binding directly to a tree..
            ITree tree = this;
            switch (change.Action)
            {
                case BranchModificationAction.DisplayDataChanged:
                    {
                        var displayChange = change as BranchModificationEventArgs.BranchModificationDisplayData;
                        tree.DisplayDataChanged(
                            new DisplayDataChangedData(
                                displayChange.Changes, displayChange.Branch, displayChange.Index, displayChange.Column, displayChange.Count));
                        break;
                    }
                case BranchModificationAction.Realign:
                    tree.Realign(change.Branch);
                    break;
                case BranchModificationAction.InsertItems:
                    tree.InsertItems(change.Branch, change.Index, change.Count);
                    break;
                case BranchModificationAction.DeleteItems:
                    tree.DeleteItems(change.Branch, change.Index, change.Count);
                    break;
                case BranchModificationAction.MoveItem:
                    tree.MoveItem(change.Branch, change.Index, change.Count);
                    break;
                case BranchModificationAction.ShiftBranchLevels:
                    {
                        var levelChange = change as BranchModificationEventArgs.BranchModificationLevelShift;
                        tree.ShiftBranchLevels(
                            new ShiftBranchLevelsData(
                                change.Branch, levelChange.RemoveLevels, levelChange.InsertLevels, levelChange.Depth,
                                levelChange.ReplacementBranch, levelChange.BranchTester, change.Index, change.Count, levelChange.NewCount));
                        break;
                    }
                case BranchModificationAction.Redraw:
                    tree.Redraw = change.Flag;
                    break;
                case BranchModificationAction.DelayRedraw:
                    tree.DelayRedraw = change.Flag;
                    break;
                case BranchModificationAction.ListShuffle:
                    tree.ListShuffle = change.Flag;
                    break;
                case BranchModificationAction.DelayListShuffle:
                    tree.DelayListShuffle = change.Flag;
                    break;
                case BranchModificationAction.UpdateCellStyle:
                    if (MultiColumnSupport)
                    {
                        (this as IMultiColumnTree).UpdateCellStyle(change.Branch, change.Index, change.Count, change.Flag);
                    }
                    break;
                case BranchModificationAction.RemoveBranch:
                    tree.RemoveBranch(change.Branch);
                    break;
            }
        }

        // ITree implementation
        IBranch ITree.Root
        {
            get { return Root; }
            set { Root = value; }
        }

        /// <summary>
        ///     Get or set the root object for the tree.
        /// </summary>
        protected IBranch Root
        {
            get { return myRootNode == null ? null : myRootNode.Branch; }
            set
            {
                if (value != null)
                {
                    var tn = CreateTreeNode(null, value, this, true, false, false);
                    tn.FullCount = tn.ImmedCount = tn.Branch.VisibleItemCount;
                    if (tn.MultiColumn
                        && tn.ComplexColumns)
                    {
                        int fullSubItemGain;
                        ExpandInitialComplexSubItems(tn as TREENODE_Multi, out fullSubItemGain);
                        tn.FullSubItemGain = fullSubItemGain;
                    }
                    // The state of the tree is vulnerable until we get to this point.
                    // Don't update the root node until the complex subitems expand cleanly.
                    if (myRootNode != null)
                    {
                        FreeRecursive(ref myRootNode);
                        Debug.Assert(myRootNode == null);
                    }
                    tn.Index = VirtualTreeConstant.NullIndex;
                    ClearPositionCache();
                    myRootNode = tn;
                    tn.Expanded = true;
                    tn.UpdateDelayed = false;
                    tn.AllowRecursion = true;
                    if (tn.Dynamic)
                    {
                        AddTrackedNode(value, tn);
                    }
                }
                else if (myRootNode != null)
                {
                    FreeRecursive(ref myRootNode);
                    Debug.Assert(myRootNode == null);
                }
                Refresh();
            }
        }

        void ITree.Realign(IBranch branch)
        {
            Realign(branch);
        }

        /// <summary>
        ///     Query the branch to reorder child branches. This will call IBranch.VisibleItemCount
        ///     and IBranch.LocateObject as needed. If a list doesn't support relocation, then the
        ///     expanded list will be collapsed. If a branch is located after the visible item count,
        ///     then it's expansion state will be maintained so it can be realigned later back into
        ///     the visible range intact.
        /// </summary>
        /// <param name="branch">The branch to Realign, or null for all branches.</param>
        protected void Realign(IBranch branch)
        {
            var tn = LocateTrackedNode(branch);
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            int startFullCount;
            int startExpandedSubItemGain;
            while (tn != null)
            {
                startFullCount = tn.FullCount;
                startExpandedSubItemGain = tn.ExpandedSubItemGain;
                RealignTreeNode(tn);
                DoRealignNotification(tn, startFullCount, startExpandedSubItemGain);
                tn = tn.NextNode;
            }
            ClearPositionCache(); //Cached absolute information is toast.
        }

        void ITree.DisplayDataChanged(DisplayDataChangedData changeData)
        {
            DisplayDataChanged(changeData);
        }

        /// <summary>
        ///     The displayed data for the branch has changed, and
        ///     any views on this tree need to be notified.
        /// </summary>
        /// <param name="changeData">The DisplayDataChangedData structure</param>
        protected void DisplayDataChanged(DisplayDataChangedData changeData)
        {
            var branch = changeData.Branch;
            var column = changeData.Column;
            var changes = changeData.Changes;
            if (column >= ColumnCount)
            {
                return;
            }
            var fireNormalEvent = OnDisplayDataChanged != null;
            var fireSingleColumnEvent = column == 0 && GetStateFlag(TreeStateFlags.FireSingleColumnOnDisplayDataChanged);
            if (!(fireNormalEvent || fireSingleColumnEvent))
            {
                return;
            }
            ITree singleTree = null;
            SingleColumnView singleView = null;
            if (fireSingleColumnEvent)
            {
                singleTree = SingleColumnTree;
                singleView = singleTree as SingleColumnView;
            }
            var tn = LocateTrackedNode(branch);
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            var coordinate = new VirtualTreeCoordinate();
            int prevAbsIndex;
            int prevAbsIndexSingleColumn;
            int reportCount;
            int reportCountSingleColumn;
            int count;
            int startRow;
            int singleColumnSubItemAdjust;
            //UNDONE: Does firing the event after we get the next node adversely affect the
            //performance of the node tracking algorithm.
            while (tn != null)
            {
                prevAbsIndex = prevAbsIndexSingleColumn = -2; // Set to -2 so that first diff check is never 1
                reportCount = reportCountSingleColumn = 0;
                if (changeData.StartRow == -1)
                {
                    startRow = 0;
                    count = tn.ImmedCount;
                }
                else
                {
                    startRow = changeData.StartRow;
                    count = changeData.Count;
                }
                while (count > 0)
                {
                    coordinate = FindAbsoluteIndex(tn, startRow, out singleColumnSubItemAdjust);
                    if (coordinate.IsValid)
                    {
                        if (fireNormalEvent)
                        {
                            if (((coordinate.Row - prevAbsIndex) == 1)
                                || (reportCount == 0))
                            {
                                ++reportCount; //Wait to fire the event until we find something discontiguous
                            }
                            else
                            {
                                OnDisplayDataChanged(
                                    this,
                                    new DisplayDataChangedEventArgs(
                                        this, changes, prevAbsIndex - reportCount + 1, coordinate.Column + column, reportCount));
                                reportCount = 1;
                            }
                            prevAbsIndex = coordinate.Row;
                        }

                        if (fireSingleColumnEvent && coordinate.Column == 0)
                        {
                            if (((coordinate.Row - singleColumnSubItemAdjust - prevAbsIndexSingleColumn) == 1)
                                || (reportCountSingleColumn == 0))
                            {
                                ++reportCountSingleColumn; //Wait to fire the event until we find something discontiguous
                            }
                            else
                            {
                                singleView.myOnDisplayDataChanged(
                                    singleTree,
                                    new DisplayDataChangedEventArgs(
                                        singleTree, changes, prevAbsIndexSingleColumn - reportCountSingleColumn + 1, 0,
                                        reportCountSingleColumn));
                                reportCountSingleColumn = 1;
                            }
                            prevAbsIndexSingleColumn = coordinate.Row - singleColumnSubItemAdjust;
                        }

                        ++startRow;
                        --count;
                    }
                    else
                    {
                        break;
                    }
                }
                if (reportCount != 0)
                {
                    OnDisplayDataChanged(
                        this,
                        new DisplayDataChangedEventArgs(
                            this, changes, prevAbsIndex - reportCount + 1, coordinate.Column + column, reportCount));
                }
                if (reportCountSingleColumn != 0)
                {
                    singleView.myOnDisplayDataChanged(
                        singleTree,
                        new DisplayDataChangedEventArgs(
                            singleTree, changes, prevAbsIndexSingleColumn - reportCountSingleColumn + 1, 0, reportCountSingleColumn));
                }
                tn = tn.NextNode;
            }
        }

        private void DoRealignNotification(TREENODE tn, int startFullCount, int startExpandedSubItemGain)
        {
            var fireNormalEvent = ItemCountChanged != null;
            var fireSingleColumnEvent = GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged);
            if (fireNormalEvent || fireSingleColumnEvent)
            {
                int singleColumnAbsIndex;
                ClearPositionCache(); //Cached absolute information is toast.
                var tnNext = tn; //Ignore next
                var coord = EnumAbsoluteIndices(VirtualTreeConstant.NullIndex, ref tnNext, out singleColumnAbsIndex);
                if (coord.IsValid
                    || tn == myRootNode)
                {
                    var startFullExpansionCount = startFullCount + startExpandedSubItemGain;
                    var fullCount = tn.FullCount;
                    var expandedSubItemGain = tn.ExpandedSubItemGain;
                    var fullExpansionCount = fullCount + expandedSubItemGain;
                    var resetExpansion = false;
                    ITree singleTree = null;
                    SingleColumnView singleView = null;
                    if ((fullExpansionCount != 0)
                        && (myRedrawCount == 0))
                    {
                        //Vegas#47930  If Redraw is on, then firing notifications may
                        //call back into this object, so we have to keep the object completely
                        //in sync with the events we're firing.  The OnDeleteItems call indicates
                        //that the entire branch is collapsed, so fake that state, then unhide it
                        //before the OnInsertItems call.

                        //Temporarily empty out node so DeleteItems notifications are accurate
                        Debug.Assert(tn.Expanded, "EnumAbsoluteIndices gave bad data");
                        ChangeFullCountRecursive(tn, -fullCount, -expandedSubItemGain);
                        Debug.Assert(tn.TotalCount == tn.ImmedSubItemGain && tn.Expanded, "Node should be expanded, but empty");
                        tn.Expanded = false;
                        resetExpansion = true;
                    }
                    if (fireNormalEvent && startFullExpansionCount != 0)
                    {
                        DelayTurnOffRedraw();
                        // UNDONE_MC: UNDONE_NOW:
                        //ItemsDeleted(this, absIndex + 1, startFullExpansionCount);
                        ItemCountChanged(
                            this,
                            new ItemCountChangedEventArgs(
                                this, coord.Row, coord.Column, -startFullExpansionCount, coord.Row, 0, null, false));
                    }
                    if (fireSingleColumnEvent
                        && singleColumnAbsIndex != VirtualTreeConstant.NullIndex
                        && startFullCount != 0)
                    {
                        DelayTurnOffSingleColumnRedraw();
                        singleTree = SingleColumnTree;
                        singleView = singleTree as SingleColumnView;
                        singleView.myItemCountChanged(
                            singleTree,
                            new ItemCountChangedEventArgs(
                                singleTree, singleColumnAbsIndex, 0, -startFullCount, singleColumnAbsIndex, 0, null, false));
                    }
                    if (resetExpansion)
                    {
                        tn.Expanded = true;
                        ChangeFullCountRecursive(tn, fullCount, expandedSubItemGain);
                        ClearPositionCache(); //Cached absolute information is toast.
                    }
                    if (fireNormalEvent && fullExpansionCount != 0)
                    {
                        DelayTurnOffRedraw();
                        // UNDONE_MC: UNDONE_NOW:
                        //ItemsInserted(this, absIndex, fullExpansionCount);
                        ItemCountChanged(
                            this,
                            new ItemCountChangedEventArgs(this, coord.Row, coord.Column, fullExpansionCount, coord.Row, 0, null, false));
                    }
                    if (fireSingleColumnEvent
                        && singleColumnAbsIndex != VirtualTreeConstant.NullIndex
                        && fullCount != 0)
                    {
                        DelayTurnOffSingleColumnRedraw();
                        if (singleTree == null)
                        {
                            singleTree = SingleColumnTree;
                            singleView = singleTree as SingleColumnView;
                        }
                        singleView.myItemCountChanged(
                            singleTree,
                            new ItemCountChangedEventArgs(
                                singleTree, singleColumnAbsIndex, 0, fullCount, singleColumnAbsIndex, 0, null, false));
                    }
                }
            }
        }

        void ITree.InsertItems(IBranch branch, int after, int count)
        {
            InsertItems(branch, after, count);
        }

        /// <summary>
        ///     Use to insert items without forcing a realign. InsertItems should be used
        ///     to adjust an existing branch: do not add a node with no children then immediately
        ///     call InsertItems repeatedly to add children. The branches VisibleItemCount is
        ///     assumed to be adjusted for the new items before a call to InsertItems. If the
        ///     items are inserted at the end of the visible portion of the branch, then
        ///     VisibleItemCount will be used to determine if the new items are hidden or visible.
        /// </summary>
        /// <param name="branch">The branch where items have been inserted.</param>
        /// <param name="after">The index to insert after. Use -1 to insert at the beginning</param>
        /// <param name="count">The number of items that have been inserted.</param>
        protected void InsertItems(IBranch branch, int after, int count)
        {
            var tn = LocateTrackedNode(branch);
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            if (after < 0)
            {
                after = -1;
            }
            if (after == -1
                ||
                after < tn.ImmedCount)
            {
                TREENODE tnChild;
                int singleColumnAbsIndex;
                while (tn != null)
                {
                    tnChild = tn.FirstChild;
                    while (tnChild != null)
                    {
                        if (after < tnChild.Index)
                        {
                            break;
                        }
                        tnChild = tnChild.NextSibling;
                    }
                    while (tnChild != null)
                    {
                        tnChild.Index += count;
                        tnChild = tnChild.NextSibling;
                    }
                    tn.ImmedCount += count;
                    ChangeFullCountRecursive(
                        tn, count, 0 /*UNDONE_MC, also need to expand initial complex sub items for newly inserted items*/);
                    if ((count != 0)
                        && ((ItemCountChanged != null)
                            || (GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged) && 0 == COLUMN_ZERO)))
                    {
                        var coord = EnumAbsoluteIndices(after + 1, ref tn, out singleColumnAbsIndex);
                        if (coord.IsValid)
                        {
                            if (ItemCountChanged != null)
                            {
                                DelayTurnOffRedraw();
                                // UNDONE_MC: Fold the OnToggleExpansion, OnInsertItems, OnDeleteItems
                                // into one event so we don't have to duplicate as much stuff for
                                // multi column support. If we're adding the item to the first row
                                // of a complex branch, then this is not sufficient information.
                                // It only works with the control right now because the expansion
                                // routines are not optimized for drawing.
                                // UNDONE_NOW:
                                //ItemsInserted(this, --absIndex, count);
                                ItemCountChanged(
                                    this,
                                    new ItemCountChangedEventArgs(this, coord.Row - 1, coord.Column, count, coord.Row - 1, 0, null, false));
                            }
                            if (singleColumnAbsIndex != VirtualTreeConstant.NullIndex
                                && GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged))
                            {
                                DelayTurnOffSingleColumnRedraw();
                                --singleColumnAbsIndex;
                                var singleTree = SingleColumnTree;
                                (singleTree as SingleColumnView).myItemCountChanged(
                                    singleTree,
                                    new ItemCountChangedEventArgs(
                                        singleTree, singleColumnAbsIndex, 0, count, singleColumnAbsIndex, 0, null, false));
                            }
                        }
                    }
                    else
                    {
                        tn = tn.NextNode;
                    }
                }
                ClearPositionCache(); //Cached absolute information is toast.
            }
        }

        void ITree.DeleteItems(IBranch branch, int start, int count)
        {
            DeleteItems(branch, start, count);
        }

        /// <summary>
        ///     Delete specific items without calling Realign
        /// </summary>
        /// <param name="branch">The branch where items have been removed</param>
        /// <param name="start">The first item to deleted</param>
        /// <param name="count">The number of items to delete</param>
        protected void DeleteItems(IBranch branch, int start, int count)
        {
            var tn = LocateTrackedNode(branch);
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            if (start >= 0
                && start < tn.ImmedCount)
            {
                TREENODE tnChild;
                TREENODE tnPrevChild;
                var endKillRange = start + count;
                int killCount;
                var enableSingleColumnEvent = GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged);
                var enableNormalEvent = ItemCountChanged != null || OnDisplayDataChanged != null;
                var notify = (count != 0) && (enableNormalEvent || enableSingleColumnEvent);
                while (tn != null)
                {
                    var absIndex = -1;
                    var singleColumnAbsIndex = -1;
                    var notifyColumn = 0;
                    if (notify)
                    {
                        // We need to get the notification index before deleting anything
                        var tnNext = tn;
                        var coord = EnumAbsoluteIndices(start, ref tnNext, out singleColumnAbsIndex);
                        if (coord.IsValid)
                        {
                            absIndex = coord.Row;
                            notifyColumn = coord.Column;
                        }
                    }
                    killCount = count;
                    tnChild = tn.FirstChild;
                    tnPrevChild = null;
                    while (tnChild != null)
                    {
                        if (start <= tnChild.Index)
                        {
                            break;
                        }
                        tnPrevChild = tnChild;
                        tnChild = tnChild.NextSibling;
                    }
                    while (tnChild != null)
                    {
                        if (tnChild.Index < endKillRange)
                        {
                            if (tnChild.Expanded)
                            {
                                killCount += tnChild.FullCount;
                            }
                            if (tnPrevChild == null)
                            {
                                tn.FirstChild = tnChild.NextSibling;
                            }
                            else
                            {
                                tnPrevChild.NextSibling = tnChild.NextSibling;
                            }
                            FreeRecursive(ref tnChild);
                            if (tnPrevChild == null)
                            {
                                tnChild = tn.FirstChild;
                            }
                            else
                            {
                                tnChild = tnPrevChild.NextSibling;
                            }
                        }
                        else
                        {
                            tnChild.Index -= count;
                            tnChild = tnChild.NextSibling;
                        }
                    }
                    tn.ImmedCount -= count;
                    ChangeFullCountRecursive(tn, -killCount, 0 /*UNDONE_MC*/);
                    if (notify)
                    {
                        if (enableNormalEvent && absIndex != -1)
                        {
                            if (tn.ComplexSubItem
                                && tn.TotalCount == 0)
                            {
                                if (OnDisplayDataChanged != null)
                                {
                                    OnDisplayDataChanged(
                                        this,
                                        new DisplayDataChangedEventArgs(this, VirtualTreeDisplayDataChanges.All, absIndex, -1, killCount));
                                }
                            }
                            else if (ItemCountChanged != null)
                            {
                                // UNDONE_MC: See comments in InsertItems. Fold the notifications
                                // for the three size changes into one, and get enough info
                                // back to do multi column. For example, the ComplexSubItem check
                                // in this if statement is equivalent to the output rowIncr parameter
                                // from the more complex ChangeFullCountRecursive call.
                                // UNDONE_NOW:
                                DelayTurnOffRedraw();
                                //ItemsDeleted(this, absIndex, killCount);
                                ItemCountChanged(
                                    this,
                                    new ItemCountChangedEventArgs(
                                        this, absIndex - 1, notifyColumn, -killCount, absIndex - 1, 0, null, false));
                            }
                        }
                        if (enableSingleColumnEvent && singleColumnAbsIndex != -1)
                        {
                            DelayTurnOffSingleColumnRedraw();
                            var singleTree = SingleColumnTree;
                            (singleTree as SingleColumnView).myItemCountChanged(
                                singleTree,
                                new ItemCountChangedEventArgs(
                                    singleTree, singleColumnAbsIndex - 1, 0, -killCount, singleColumnAbsIndex - 1, 0, null, false));
                        }
                    }
                    tn = tn.NextNode;
                }
                ClearPositionCache(); //Cached absolute information is toast.
            }
        }

        void ITree.MoveItem(IBranch branch, int fromRow, int toRow)
        {
            MoveItem(branch, fromRow, toRow);
        }

        /// <summary>
        ///     Change the position of a single item in a branch
        /// </summary>
        /// <param name="branch">The branch where the item moved</param>
        /// <param name="fromRow">The row the item used to be on</param>
        /// <param name="toRow">The row the item is on now</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected void MoveItem(IBranch branch, int fromRow, int toRow)
        {
            if (fromRow == toRow)
            {
                return;
            }
            var tn = LocateTrackedNode(branch);
            ITree singleTree = null;
            SingleColumnView singleView = null;
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            while (tn != null)
            {
                int changeAfter;
                int changeUntil;
                int indexIncr;
                var fireNormalEvent = ItemMoved != null;
                var fireSingleColumnEvent = GetStateFlag(TreeStateFlags.FireSingleColumnItemMoved);
                var fireEvent = fireNormalEvent || fireSingleColumnEvent;
                var absRowAfter = -1;
                var absRowUntil = -1;
                var notifyColumn = 0;
                var singleColumnSubItemAdjust = 0;
                var rowUntilSubItemGain = 0;
                var rowAfterSubItemGain = 0;
                if (fireEvent)
                {
                    var coordinate = FindAbsoluteIndex(tn, 0, out singleColumnSubItemAdjust);
                    if (!coordinate.IsValid
                        || !tn.Expanded)
                    {
                        fireEvent = fireNormalEvent = fireSingleColumnEvent = false;
                    }
                    else
                    {
                        notifyColumn = coordinate.Column;
                        absRowAfter = coordinate.Row;
                        fireSingleColumnEvent &= notifyColumn == 0;
                        fireEvent = fireNormalEvent || fireSingleColumnEvent;
                        if (fireSingleColumnEvent && singleTree == null)
                        {
                            singleTree = SingleColumnTree;
                            singleView = singleTree as SingleColumnView;
                        }
                    }
                }
                if (fromRow < toRow)
                {
                    changeAfter = fromRow;
                    changeUntil = toRow;
                    indexIncr = -1;
                }
                else
                {
                    changeAfter = toRow;
                    changeUntil = fromRow;
                    indexIncr = 1;
                }
                TREENODE tnLeading = null; // The item with an exact match at the end of the range
                TREENODE tnPrevLeading = null; // The item before the first item [between) the from and to nodes
                TREENODE tnPrevTrailing = null;
                var tnChild = tn.FirstChild;
                while (tnChild != null
                       && tnChild.Index < changeAfter)
                {
                    tnPrevLeading = tnChild;
                    if (fireEvent)
                    {
                        if (tnChild.Expanded)
                        {
                            rowAfterSubItemGain += tnChild.FullSubItemGain;
                            absRowAfter += tnChild.FullCount;
                        }
                        else
                        {
                            rowAfterSubItemGain += tnChild.ImmedSubItemGain;
                        }
                    }
                    tnChild = tnChild.NextSibling;
                }
                if (fireEvent)
                {
                    absRowUntil = absRowAfter + changeUntil;
                    rowUntilSubItemGain = rowAfterSubItemGain;
                    absRowAfter += changeAfter;
                }

                if (tnChild != null
                    && tnChild.Index == changeAfter
                    && changeAfter == fromRow)
                {
                    // Pull tnLeading out of the linked list. We'll reattach it later.
                    tnLeading = tnChild;
                    tnChild = tnChild.NextSibling;
                    if (tnPrevLeading == null)
                    {
                        tn.FirstChild = tnLeading.NextSibling;
                    }
                    else
                    {
                        tnPrevLeading.NextSibling = tnLeading.NextSibling;
                    }
                }
                tnPrevTrailing = tnPrevLeading;
                while (tnChild != null
                       && tnChild.Index < changeUntil)
                {
                    if (fireEvent)
                    {
                        if (tnChild.Expanded)
                        {
                            rowUntilSubItemGain += tnChild.FullSubItemGain;
                            absRowUntil += tnChild.FullCount;
                        }
                        else
                        {
                            rowUntilSubItemGain += tnChild.ImmedSubItemGain;
                        }
                    }
                    tnChild.Index += indexIncr;
                    tnPrevTrailing = tnChild;
                    tnChild = tnChild.NextSibling;
                }

                // We now have all the data to put the list back together. tnLeading
                // holds the leading item, and tnChild will hold the trailing one.
                if (changeAfter == fromRow)
                {
                    // Moving the item down. We've already pulled it from the list
                    if (tnLeading != null)
                    {
                        tnLeading.Index = toRow;
                        if (tnChild != null
                            && tnChild.Index == changeUntil)
                        {
                            // Item is displaced by one, decrement it's index
                            --tnChild.Index;
                            tnLeading.NextSibling = tnChild.NextSibling;
                            tnChild.NextSibling = tnLeading;
                            if (fireEvent)
                            {
                                if (tnChild.Expanded)
                                {
                                    rowUntilSubItemGain += tnChild.FullSubItemGain;
                                    absRowUntil += tnChild.FullCount;
                                }
                                else
                                {
                                    rowUntilSubItemGain += tnChild.ImmedSubItemGain;
                                }
                            }
                        }
                        else if (tnPrevTrailing == null)
                        {
                            tnLeading.NextSibling = tn.FirstChild;
                            tn.FirstChild = tnLeading;
                        }
                        else
                        {
                            tnLeading.NextSibling = tnPrevTrailing.NextSibling;
                            tnPrevTrailing.NextSibling = tnLeading;
                        }
                        // Fire a move down with potentially multiple items

                        if (fireNormalEvent)
                        {
                            ItemMoved(
                                this,
                                new ItemMovedEventArgs(
                                    this, notifyColumn, absRowAfter + rowAfterSubItemGain, absRowUntil + rowUntilSubItemGain,
                                    1 + (tnLeading.Expanded ? tnLeading.TotalCount : tnLeading.ImmedSubItemGain), tn.MultiColumn));
                        }
                        if (fireSingleColumnEvent)
                        {
                            singleView.myItemMoved(
                                singleTree,
                                new ItemMovedEventArgs(
                                    singleTree, 0, absRowAfter - singleColumnSubItemAdjust, absRowUntil - singleColumnSubItemAdjust,
                                    1 + (tnLeading.Expanded ? tnLeading.FullCount : 0), false));
                        }
                    }
                    else
                    {
                        if (tnChild != null
                            && tnChild.Index == changeUntil)
                        {
                            if (fireEvent)
                            {
                                if (tnChild.Expanded)
                                {
                                    rowUntilSubItemGain += tnChild.FullSubItemGain;
                                    absRowUntil += tnChild.FullCount;
                                }
                                else
                                {
                                    rowUntilSubItemGain += tnChild.ImmedSubItemGain;
                                }
                            }
                            --tnChild.Index;
                        }

                        // Fire a move down event with a single item
                        if (fireNormalEvent)
                        {
                            ItemMoved(
                                this,
                                new ItemMovedEventArgs(
                                    this, notifyColumn, absRowAfter + rowAfterSubItemGain, absRowUntil + rowUntilSubItemGain, 1,
                                    tn.MultiColumn));
                        }
                        if (fireSingleColumnEvent)
                        {
                            singleView.myItemMoved(
                                singleTree,
                                new ItemMovedEventArgs(
                                    singleTree, 0, absRowAfter - singleColumnSubItemAdjust, absRowUntil - singleColumnSubItemAdjust, 1,
                                    false));
                        }
                    }
                }
                else if (tnChild != null
                         && tnChild.Index == changeUntil)
                {
                    // Move the item up
                    if (tnPrevTrailing == null)
                    {
                        tn.FirstChild = tnChild.NextSibling;
                    }
                    else
                    {
                        tnPrevTrailing.NextSibling = tnChild.NextSibling;
                    }
                    tnChild.Index = toRow;
                    if (tnPrevLeading == null)
                    {
                        tnChild.NextSibling = tn.FirstChild;
                        tn.FirstChild = tnChild;
                    }
                    else
                    {
                        tnChild.NextSibling = tnPrevLeading.NextSibling;
                        tnPrevLeading.NextSibling = tnChild;
                    }

                    // Fire a move up with potentially multiple items
                    if (fireNormalEvent)
                    {
                        ItemMoved(
                            this,
                            new ItemMovedEventArgs(
                                this, notifyColumn, absRowUntil + rowUntilSubItemGain, absRowAfter + rowAfterSubItemGain,
                                1 + (tnChild.Expanded ? tnChild.TotalCount : tnChild.ImmedSubItemGain), tn.MultiColumn));
                    }
                    if (fireSingleColumnEvent)
                    {
                        singleView.myItemMoved(
                            singleTree,
                            new ItemMovedEventArgs(
                                singleTree, 0, absRowUntil - singleColumnSubItemAdjust, absRowAfter - singleColumnSubItemAdjust,
                                1 + (tnChild.Expanded ? tnChild.FullCount : 0), false));
                    }
                }
                else if (fireEvent)
                {
                    // Fire a move item up event with a single item
                    if (fireNormalEvent)
                    {
                        ItemMoved(
                            this,
                            new ItemMovedEventArgs(
                                this, notifyColumn, absRowUntil + rowUntilSubItemGain, absRowAfter + rowAfterSubItemGain, 1, tn.MultiColumn));
                    }
                    if (fireSingleColumnEvent)
                    {
                        singleView.myItemMoved(
                            singleTree,
                            new ItemMovedEventArgs(
                                singleTree, 0, absRowUntil - singleColumnSubItemAdjust, absRowAfter - singleColumnSubItemAdjust, 1, false));
                    }
                }
                // UNDONE: Also need to do modifications to position trackers.
                tn = tn.NextNode;
            }
        }

        ToggleExpansionData ITree.ToggleExpansion(int row, int column)
        {
            return ToggleExpansion(row, column);
        }

        /// <summary>
        ///     Expand or collapse the item at the given position
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>Data showing the incremental change in the number of items in the tree</returns>
        protected ToggleExpansionData ToggleExpansion(int row, int column)
        {
            bool allowRecursion;
            int itemExpansionCount;
            int subItemExpansionCount;
            ToggleExpansion(row, column, out itemExpansionCount, out subItemExpansionCount, out allowRecursion);
            return new ToggleExpansionData(itemExpansionCount + subItemExpansionCount, allowRecursion);
        }

        private void ToggleExpansion(
            int row, int column, out int itemExpansionCount, out int subItemExpansionCount, out bool allowRecursion)
        {
            allowRecursion = false;
            int rowChange;
            int blanksAboveChange;
            int singleColumnSubItemAdjust;
            SubItemColumnAdjustment[] subItemChanges;

            ToggleExpansion(
                row, column, out allowRecursion, out singleColumnSubItemAdjust, out itemExpansionCount, out subItemExpansionCount,
                out rowChange, out blanksAboveChange, out subItemChanges);
            if (ItemCountChanged != null)
            {
                DelayTurnOffRedraw();

                //Note: Notify even when change is 0 for cases where new lists are being tracked
                ItemCountChanged(
                    this, new ItemCountChangedEventArgs(this, row, column, rowChange, row - 1, blanksAboveChange, subItemChanges, true));
            }
            if (column == 0
                && GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged))
            {
                DelayTurnOffSingleColumnRedraw();

                var singleTree = SingleColumnTree;
                (singleTree as SingleColumnView).myItemCountChanged(
                    singleTree,
                    new ItemCountChangedEventArgs(
                        singleTree, row - singleColumnSubItemAdjust, 0, itemExpansionCount, row - singleColumnSubItemAdjust - 1, 0, null,
                        true));
            }
        }

        bool ITree.IsExpanded(int absRow, int column)
        {
            return IsExpanded(absRow, column);
        }

        /// <summary>
        ///     Is the item at this absolute index expanded?
        /// </summary>
        /// <param name="absRow">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>true if the item is currently expanded</returns>
        protected bool IsExpanded(int absRow, int column)
        {
            var pos = TrackCell(absRow, ref column);
            return pos.IsExpanded(column);
        }

        bool ITree.IsExpandable(int absRow, int column)
        {
            return IsExpandable(absRow, column);
        }

        /// <summary>
        ///     Is the item at this absolute index expandable?
        /// </summary>
        /// <param name="absRow">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>true if the item can be expanded</returns>
        protected bool IsExpandable(int absRow, int column)
        {
            var pos = TrackCell(absRow, ref column);
            return pos.IsExpandable(column);
        }

        BlankExpansionData ITree.GetBlankExpansion(int row, int column, ColumnPermutation columnPermutation)
        {
            return GetBlankExpansion(row, column, columnPermutation);
        }

        private int ColumnWidthOfRow(int row)
        {
            var currentNode = myRootNode;
            var currentRow = row;
            var columns = 1;
            int lastColumn;
            TREENODE parentNode;
            TREENODE childNode;
            SUBITEMNODE sn;
            while (true)
            {
                var pos = currentNode.TrackRow(currentRow);
                parentNode = pos.ParentNode;
                lastColumn = parentNode.GetColumnCount(pos.Index) - 1;
                columns += lastColumn;
                childNode = parentNode.GetChildNode(pos.Index);
                if ((null != (childNode = parentNode.GetChildNode(pos.Index)))
                    &&
                    (null != (sn = childNode.SubItemAtColumn(lastColumn)))
                    &&
                    sn.RootNode.MultiColumn)
                    // Note that only the last column is allowed to be multicolumn, so this will eliminate jaggeds with no extra checking
                {
                    currentRow = pos.SubItemOffset;
                    currentNode = sn.RootNode;
                    continue;
                }
                break;
            }
            return columns;
        }

        /// <summary>
        ///     For the given row and column, retrieve the range of cells that form the
        ///     blank region for the given items. The anchor cell, which is the only
        ///     cell in the range that is not a blank item, will always be the top left
        ///     cell. In most cases, the returned range will simply be the input cell.
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the column permutation, not
        ///     the unpermuted position.
        /// </param>
        /// <returns>The expansion for this row and column</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected BlankExpansionData GetBlankExpansion(int row, int column, ColumnPermutation columnPermutation)
        {
            if (!MultiColumnSupport)
            {
                // There is no way to get blank cells in a single-column tree
                return new BlankExpansionData(row, column);
            }
            else if (columnPermutation != null)
            {
                var permutedColumns = columnPermutation.GetColumnExpansion(column, ColumnWidthOfRow(row) - 1);
                // The consumer should not let the user do this but we need to handle it gracefully.
                // Make sure we have a row count by pulling the native row count from row 0 if the
                // row is fully blank, and from the native permutation column if it is not.
                var nativeData = GetBlankExpansion(
                    row,
                    (permutedColumns.AnchorColumn == VirtualTreeConstant.NullIndex)
                        ? 0
                        : columnPermutation.GetNativeColumn(permutedColumns.AnchorColumn), null);
                // Get the row data from the native expansion and combine with the columns from
                // the permuted expansion.
                return new BlankExpansionData(
                    nativeData.TopRow, permutedColumns.LeftColumn, nativeData.BottomRow, permutedColumns.RightColumn,
                    permutedColumns.AnchorColumn);
            }
            else
            {
                int topRow;
                int leftColumn;
                int bottomRow;
                int rightColumn;
                topRow = bottomRow = row;
                leftColumn = rightColumn = column;
                var localColumn = column;
                var lastSubItem = false;
                var pos = TrackCell(row, ref localColumn, ref lastSubItem);

                // Check the column range
                var lastNonBlankColumn = pos.ParentNode.GetColumnCount(pos.Index) - 1;
                var columnDiff = lastNonBlankColumn - localColumn;
                var localColumnAdjusted = false;
                var rowPadding = 0;
                if (columnDiff <= 0)
                {
                    // Unfortunately, there is no easy check to see if we should apply this
                    // column expansion or not. It should only be supplied if the node is a
                    // child of either a complex sub item or the root node, not in other case.
                    var tn = pos.ParentNode;
                    var nextIndex = pos.Index;
                    var onLastItem = tn.InSubItemColumn;
                    while (tn != null)
                    {
                        if (onLastItem)
                        {
                            // While we're walking the loop, see if we're on the last cell
                            if (nextIndex < (tn.FullCount - 1))
                            {
                                onLastItem = false;
                            }
                            else
                            {
                                nextIndex = tn.Index;
                            }
                        }
                        if (tn.SubItemRoot)
                        {
                            if (tn.MultiColumn)
                            {
                                // UNDONE_MC: Not sure about this one
                                tn = null;
                            }
                            else
                            {
                                // We're looking at an expandable subitem. If this is in
                                // the last column, then we need to expand to its right and treat
                                // the parent node as the real owner. We're also in a perfect position
                                // to see if we need extra row padding at the end.
                                var subitemOwner = tn.Parent;
                                if (onLastItem)
                                {
                                    rowPadding = subitemOwner.ImmedSubItemGain - tn.FullCount;
                                    if (tn.ComplexSubItem)
                                    {
                                        ++rowPadding;
                                    }
                                }
                                lastNonBlankColumn = subitemOwner.Parent.GetColumnCount(subitemOwner.Index) - 1;
                                columnDiff = lastNonBlankColumn - subitemOwner.FirstSubItem.ColumnOfRootNode(tn);
                                if (columnDiff <= 0)
                                {
                                    tn = null;
                                }
                            }
                            break;
                        }
                        tn = tn.Parent;
                    }
                    if (tn == null)
                    {
                        if (!pos.ParentNode.InSubItemColumn)
                        {
                            localColumn += columnDiff;
                            localColumnAdjusted = true;
                        }
                        leftColumn += columnDiff;
                        rightColumn = ColumnCount - 1;
                    }
                }

                // Check the row range. Note that this does not depend on the column,
                // any column in the given column range provides the same answer.
                var tnChild = pos.ParentNode.GetChildNode(pos.Index);
                if (localColumn > 0
                    && tnChild != null)
                {
                    var sn = tnChild.SubItemAtColumn(localColumn);
                    var tnSubItemRoot = (sn == null) ? null : sn.RootNode;
                    Debug.Assert(tnChild == null || !tnChild.ComplexSubItem); // A complex subitem should track to column 0
                    if (tnSubItemRoot != null
                        && tnSubItemRoot.Expanded
                        && tnSubItemRoot.FullCount > 0)
                    {
                        if (localColumnAdjusted)
                        {
                            // We are in a region with subitems, but we hit to the
                            // right of the last column, which happens to be expanded.
                            var lastRowInCell = tnSubItemRoot.FullCount;
                            if (tnSubItemRoot.ComplexSubItem)
                            {
                                --lastRowInCell;
                            }

                            // If the item is on or after
                            if (pos.SubItemOffset >= lastRowInCell)
                            {
                                topRow -= pos.SubItemOffset;
                                bottomRow = topRow + tnChild.ImmedSubItemGain;
                                topRow += lastRowInCell;
                            }
                            tnChild = null;
                        }
                        else
                        {
                            if (pos.SubItemOffset > 0)
                            {
                                topRow -= pos.SubItemOffset;
                                bottomRow = topRow + tnChild.ImmedSubItemGain;
                                // UNDONE_MC: This doesn't handle a multicolumn complex column
                                topRow += tnSubItemRoot.FullCount;
                                if (tnSubItemRoot.ComplexSubItem)
                                {
                                    --topRow;
                                }
                                tnChild = null;
                            }
                        }
                    }
                }
                if (tnChild != null)
                {
                    if (pos.SubItemOffset > 0)
                    {
                        topRow -= pos.SubItemOffset;
                        bottomRow = topRow + tnChild.ImmedSubItemGain;
                    }
                    else
                    {
                        bottomRow += tnChild.ImmedSubItemGain;
                    }
                }
                if (rowPadding != 0)
                {
                    bottomRow += rowPadding;
                }

                return new BlankExpansionData(topRow, leftColumn, bottomRow, rightColumn, leftColumn);
            }
        }

        VirtualTreeItemInfo ITree.GetItemInfo(int absRow, int column, bool setFlags)
        {
            return GetItemInfo(absRow, column, setFlags);
        }

        /// <summary>
        ///     Retrieve information about the item at the target position
        /// </summary>
        /// <param name="absRow">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="setFlags">Calculate information beyond Branch, Row, Column, Level, and Blank</param>
        /// <returns>An VirtualTreeItemInfo describing the item at the given location</returns>
        protected VirtualTreeItemInfo GetItemInfo(int absRow, int column, bool setFlags)
        {
            var lastSubItem = false;
            var pos = TrackCell(absRow, ref column, ref lastSubItem);
            return GetItemInfo(ref pos, column, setFlags, false, lastSubItem);
        }

        private VirtualTreeItemInfo GetItemInfo(ref ITEMPOSITION pos, int column, bool setFlags, bool ignoreMultiColumn, bool lastSubItem)
        {
            var info = new VirtualTreeItemInfo(pos.ParentNode.Branch, pos.Index, column, pos.Level);
            var blankItem = ignoreMultiColumn ? false : pos.IsBlank(column);
            info.Blank = blankItem;

            if (setFlags)
            {
                if (!ignoreMultiColumn && MultiColumnSupport)
                {
                    TREENODE tnChild;
                    if (blankItem)
                    {
                        if (column >= pos.ParentNode.GetColumnCount(pos.Index))
                        {
                            // If the item is after all supported columns then
                            // just let it draw blank by leaving all flags clear.
                            info.ClearLevel();
                        }
                        else
                        {
                            Debug.Assert(pos.SubItemOffset > 0);
                            tnChild = pos.ParentNode.GetChildNode(pos.Index);
                            Debug.Assert(tnChild != null); // Can't get a subitem offset without a node
                            if (column == 0)
                            {
                                // A blank item in the first column
                                info.TrailingItem = pos.SubItemOffset == tnChild.ImmedSubItemGain;
                                info.LastBranchItem = (pos.Index + 1) == pos.ParentNode.ImmedCount;
                                info.FirstBranchItem = pos.Index == 0;
                                info.Expanded = tnChild.Expanded && (tnChild.ImmedCount > 0);
                                    // Required information to draw lines down to expanded item
                            }
                            else
                            {
                                // A blank item in subitem column
                                info.ClearLevel();
                                info.TrailingItem = pos.SubItemOffset == tnChild.ImmedSubItemGain;
                            }
                        }
                    } // if (blankItem)
                    else if (!pos.ParentNode.MultiColumn)
                    {
                        info.Expanded = pos.IsExpanded(column);
                        info.Expandable = info.Expanded || pos.IsExpandable(column);
                        info.FirstBranchItem = pos.Index == 0;
                        info.LastBranchItem = pos.ParentNode.Branch.VisibleItemCount == (pos.Index + 1);
                        if (pos.ParentNode.InSubItemColumn)
                        {
                            info.LeadingItem = info.FirstBranchItem && pos.ParentNode.ComplexSubItem;
                            info.TrailingItem = lastSubItem;
                            info.SimpleCell = info.Level == 0 && pos.ParentNode.NoChildExpansion;
                        }
                        else
                        {
                            info.TrailingItem = info.LeadingItem = true;
                        }
                    }
                    else if (column == 0)
                    {
                        info.LeadingItem = true;
                        info.FirstBranchItem = pos.Index == 0;
                        info.LastBranchItem = (pos.Index + 1) == pos.ParentNode.ImmedCount;
                        tnChild = pos.ParentNode.GetChildNode(pos.Index);
                        if (tnChild == null)
                        {
                            info.TrailingItem = true;
                            info.Expandable = pos.IsExpandable(0); //column == 0
                        }
                        else
                        {
                            info.TrailingItem = tnChild.ImmedSubItemGain == 0;
                            info.Expanded = pos.IsExpanded(0); // column == 0
                            info.Expandable = info.Expanded || pos.IsExpandable(0);
                        }
                        info.SimpleCell = info.Level == 0 && pos.ParentNode.NoChildExpansion;
                    } // else if (column == 0)
                    else
                    {
                        info.LeadingItem = info.FirstBranchItem = true;
                        info.LastBranchItem = true;
                        tnChild = pos.ParentNode.GetChildNode(pos.Index);
                        if (tnChild != null)
                        {
                            info.TrailingItem = tnChild.ImmedSubItemGain == 0;
                        }
                        else
                        {
                            info.TrailingItem = true;
                        }
                        info.Expanded = pos.IsExpanded(column);
                        info.Expandable = info.Expanded || pos.IsExpandable(column);
                        info.SimpleCell = info.Level == 0 && !info.Expandable;
                    } // else
                } // if (MultiColumnSupport)
                else
                {
                    Debug.Assert(column == 0);
                    if (!blankItem)
                    {
                        info.Expanded = pos.IsExpanded(column);
                        info.Expandable = info.Expanded ? true : pos.IsExpandable(column);
                    }
                    info.FirstBranchItem = pos.Index == 0;
                    info.LastBranchItem = pos.ParentNode.Branch.VisibleItemCount == (pos.Index + 1);
                    info.LeadingItem = info.TrailingItem = true;
                } // else
            }
            return info;
        }

        int ITree.VisibleItemCount
        {
            get { return VisibleItemCount; }
        }

        /// <summary>
        ///     The total number of items currently displayed by the tree
        /// </summary>
        /// <value></value>
        protected int VisibleItemCount
        {
            get { return (myRootNode == null) ? 0 : myRootNode.TotalCount; }
        }

        void ITree.Refresh()
        {
            Refresh();
        }

        /// <summary>
        ///     Refresh the tree
        /// </summary>
        protected void Refresh()
        {
            UpdateTreeNodeRecursive(myRootNode);
            if (OnRefresh != null)
            {
                DelayTurnOffRedraw();
                OnRefresh(this, EventArgs.Empty);
            }
        }

        int ITree.GetSubItemCount(int absRow, int column)
        {
            return GetSubItemCount(absRow, column);
        }

        /// <summary>
        ///     Get the number of sub items for this item. The next non-blank item
        ///     in a given column is at row + GetSubItemCount(absRow, column) + 1
        /// </summary>
        /// <param name="absRow">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>The number of sub items immediately below this node.</returns>
        protected int GetSubItemCount(int absRow, int column)
        {
            if (!MultiColumnSupport)
            {
                return 0;
            }
            else
            {
                TREENODE tn;
                if (absRow < 0)
                {
                    if (column > 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "column", VirtualTreeStrings.GetString(VirtualTreeStrings.GetSubItemCountExceptionDesc));
                    }
                    tn = myRootNode;
                }
                else
                {
                    var pos = TrackCell(absRow, ref column);
                    if (pos.IsBlank(column))
                    {
                        throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.BlankSubItemException), "absRow");
                    }
                    tn = (column == 0) ? pos.ParentNode.GetChildNode(pos.Index) : null;
                }
                return (tn == null) ? 0 : tn.ImmedSubItemGain;
            }
        }

        int ITree.GetDescendantItemCount(int absRow, int column, bool includeSubItems, bool complexColumnRoot)
        {
            return GetDescendantItemCount(absRow, column, includeSubItems, complexColumnRoot);
        }

        /// <summary>
        ///     Get the number of descendants of a given node
        /// </summary>
        /// <param name="absRow">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="includeSubItems">Whether to include any subitems in the item count.</param>
        /// <param name="complexColumnRoot">row and column are the first node in a complex subitem column. Return the count of all items in the root list, not the expanded count for the first item.</param>
        /// <returns>0 if the node is not expanded and there are no subitems</returns>
        protected int GetDescendantItemCount(int absRow, int column, bool includeSubItems, bool complexColumnRoot)
        {
            TREENODE tn;
            if (absRow < 0)
            {
                if (column > 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "column", VirtualTreeStrings.GetString(VirtualTreeStrings.ColumnOutOfRangeException));
                }
                // Return all items
                tn = myRootNode;
            }
            else if (complexColumnRoot)
            {
                var pos = TrackCell(absRow, ref column);
                tn = pos.ParentNode;
                if (!tn.ComplexSubItem)
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ComplexColumnRootException));
                }
                Debug.Assert(column == 0); // column resolved by TrackCell for a complex item
            }
            else
            {
                var pos = TrackCell(absRow, ref column);
                tn = pos.IsBlank(column) ? null : pos.ParentNode.GetChildNode(pos.Index);
                if (column > 0
                    && tn != null)
                {
                    var sn = tn.SubItemAtColumn(column);
                    tn = (sn == null) ? null : sn.RootNode;
                    Debug.Assert(tn == null || !tn.ComplexSubItem); // A complex subitem should track to column 0
                }
            }
            var retVal = 0;
            if (tn != null)
            {
                if (includeSubItems)
                {
                    retVal = tn.Expanded ? tn.TotalCount : tn.ImmedSubItemGain;
                }
                else if (tn.Expanded)
                {
                    retVal = tn.FullCount;
                }
            }
            return retVal;
        }

        int ITree.GetParentIndex(int row, int column)
        {
            return GetParentIndex(row, column);
        }

        /// <summary>
        ///     Get the parent index of the given row and column
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>Returns the parent index, or -1 if the parent is the root list</returns>
        protected int GetParentIndex(int row, int column)
        {
            var parentOffset = 0;
            var singleColumnSubItemAdjust = 0;
            var pos = TrackCell(row, ref column, ref parentOffset, ref singleColumnSubItemAdjust);
            if (pos.ParentAbsolute == VirtualTreeConstant.NullIndex)
            {
                if (!pos.ParentNode.SubItemRoot
                    || pos.ParentNode.ComplexSubItem)
                {
                    return VirtualTreeConstant.NullIndex;
                }
            }
            return pos.ParentAbsolute + parentOffset;
        }

        ExpandedBranchData ITree.GetExpandedBranch(int row, int column)
        {
            return GetExpandedBranch(row, column);
        }

        /// <summary>
        ///     Returns the expanded list at the given index if one already exists. This method
        ///     will throw an exception if IsExpanded for this cell is false. Use ToggleExpansion
        ///     to create a new expansion.
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate</param>
        /// <returns>The branch and level of the expansion at the given location</returns>
        protected ExpandedBranchData GetExpandedBranch(int row, int column)
        {
            if (myRootNode != null)
            {
                if (row < 0)
                {
                    if (column > 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "column", VirtualTreeStrings.GetString(VirtualTreeStrings.ColumnOutOfRangeException));
                    }
                    return new ExpandedBranchData(myRootNode.Branch, 0);
                }
                else
                {
                    var pos = TrackCell(row, ref column);
                    if (!pos.IsBlank(column))
                    {
                        var tnCur = pos.ParentNode.GetChildNode(pos.Index);
                        if (tnCur != null)
                        {
                            if (column > 0)
                            {
                                if (pos.SubItemOffset == 0
                                    &&
                                    column < pos.ParentNode.GetColumnCount(pos.Index))
                                {
                                    var sn = tnCur.SubItemAtColumn(column);
                                    if (sn != null)
                                    {
                                        return new ExpandedBranchData(sn.RootNode.Branch, 0);
                                    }
                                }
                            }
                            else if (tnCur.Expanded)
                            {
                                return new ExpandedBranchData(tnCur.Branch, pos.Level);
                            }
                        }
                    }
                }
            }
            throw new ArgumentOutOfRangeException("row"); //UNDONE: EXCEPTION, different exception for not parent object??
        }

        //Toggles the state of the given item (may be more than two states)
        StateRefreshChanges ITree.ToggleState(int row, int column)
        {
            return ToggleState(row, column);
        }

        /// <summary>
        ///     Toggles the state of the given item (may be more than two states)
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <returns>The related nodes that need to be refreshed</returns>
        protected StateRefreshChanges ToggleState(int row, int column)
        {
            return DoToggleState(row, column, null, -1, 0);
        }

        /// <summary>
        ///     Helper function. Handles ToggleState and SynchronizeState.
        /// </summary>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="matchBranch">Branch to synchronize with (null forces an initial call to ToggleState instead of SynchronizeState)</param>
        /// <param name="matchRow">Row in branch to synchronize with</param>
        /// <param name="matchColumn">Column in branch to synchronize with</param>
        /// <returns>StateRefreshChanges value other than None to fire the change event</returns>
        private StateRefreshChanges DoToggleState(int row, int column, IBranch matchBranch, int matchRow, int matchColumn)
        {
            var stateRefreshOptions = StateRefreshChanges.None;
            var pos = TrackCell(row, ref column);
            if (pos.ParentNode.CheckState)
            {
                // UNDONE: The branch being toggle can be participating in more than one location in
                // in the tree. All of the branches need to be updated via events back to the parent,
                // not just the one at the current location.
                if (matchBranch == null)
                {
                    stateRefreshOptions = pos.ParentNode.Branch.ToggleState(pos.Index, column);
                }
                else
                {
                    stateRefreshOptions = pos.ParentNode.Branch.SynchronizeState(pos.Index, column, matchBranch, matchRow, matchColumn);
                }
                if (stateRefreshOptions != StateRefreshChanges.None)
                {
                    NotifyStateChange(row, column, stateRefreshOptions);
                }
            }
            return stateRefreshOptions;
        }

        internal void NotifyStateChange(int row, int column, StateRefreshChanges stateRefreshOptions)
        {
            if (StateToggled != null)
            {
                DelayTurnOffRedraw();
                StateToggled(this, new ToggleStateEventArgs(row, column, stateRefreshOptions));
            }
            if (column == 0
                && GetStateFlag(TreeStateFlags.FireSingleColumnStateToggled))
            {
                var singleColumnSubItemAdjust = 0;
                TrackCell(row, ref column, ref singleColumnSubItemAdjust);
                DelayTurnOffSingleColumnRedraw();
                var singleTree = SingleColumnTree;
                (singleTree as SingleColumnView).myStateToggled(
                    singleTree, new ToggleStateEventArgs(row - singleColumnSubItemAdjust, 0, stateRefreshOptions));
            }
        }

        void ITree.SynchronizeState(ColumnItemEnumerator itemsToSynchronize, IBranch matchBranch, int matchRow, int matchColumn)
        {
            SynchronizeState(itemsToSynchronize, matchBranch, matchRow, matchColumn, false /* translateSingleColumnView */);
        }

        /// <summary>
        ///     Synchronize the state of the given items to the state of another branch.
        /// </summary>
        /// <param name="itemsToSynchronize">items whose state should be synchronized.</param>
        /// <param name="matchBranch">Branch to synchronize with</param>
        /// <param name="matchRow">Row in branch to synchronize with</param>
        /// <param name="matchColumn">Column in branch to synchronize with</param>
        /// <param name="translateSingleColumnView">True if this synchronize is coming from a single column view attached to the tree.</param>
        protected internal void SynchronizeState(
            ColumnItemEnumerator itemsToSynchronize, IBranch matchBranch, int matchRow, int matchColumn, bool translateSingleColumnView)
        {
            // UNDONE : check translateSingleColumnView before firing events because sync batching is not currently supported 
            // for the single column view.
            SynchronizeStateEventArgs e = null;
            if (!translateSingleColumnView && SynchronizationBeginning != null
                || SynchronizationEnding != null)
            {
                e = new SynchronizeStateEventArgs(this, itemsToSynchronize, matchBranch, matchRow, matchColumn);
            }

            if (!translateSingleColumnView
                && SynchronizationBeginning != null)
            {
                SynchronizationBeginning(this, e);
            }

            if (e == null
                || !e.Handled)
            {
                itemsToSynchronize.Reset();
                if (!translateSingleColumnView)
                {
                    while (itemsToSynchronize.MoveNext())
                    {
                        if (
                            !(itemsToSynchronize.Branch == matchBranch && itemsToSynchronize.RowInBranch == matchRow
                              && itemsToSynchronize.ColumnInBranch == matchColumn))
                        {
                            DoToggleState(itemsToSynchronize.RowInTree, itemsToSynchronize.ColumnInTree, matchBranch, matchRow, matchColumn);
                        }
                    }
                }
                else
                {
                    while (itemsToSynchronize.MoveNext())
                    {
                        if (
                            !(itemsToSynchronize.Branch == matchBranch && itemsToSynchronize.RowInBranch == matchRow
                              && itemsToSynchronize.ColumnInBranch == matchColumn))
                        {
                            DoToggleState(TranslateSingleColumnRow(itemsToSynchronize.RowInTree), 0, matchBranch, matchRow, matchColumn);
                        }
                    }
                }
            }

            if (!translateSingleColumnView
                && SynchronizationEnding != null)
            {
                SynchronizationEnding(this, e);
            }
        }

        /// <summary>
        ///     Get the absolute index of an item in a TREENODE
        /// </summary>
        /// <param name="tn">A TREENODE in this tree</param>
        /// <param name="index">The relative index of the item within the TREENODE</param>
        /// <param name="singleColumnSubItemAdjust">The number of blanks before the item. Undefined if Column > 0.</param>
        /// <returns>A valid coordinate, or VirtualTreeCoordinate.Invalid if the item does not currently appear.</returns>
        private static VirtualTreeCoordinate FindAbsoluteIndex(TREENODE tn, int index, out int singleColumnSubItemAdjust)
        {
            Debug.Assert(tn != null);
            TREENODE tnTmp;
            var isExpanded = false;
            var column = 0;
            int tnImmedSubItemGain; // ImmedSubItemGain is virtual, limit number of calls
            int tmpSubItemGain;
            singleColumnSubItemAdjust = 0;
            //Walk up parent chain to find offset
            tnImmedSubItemGain = tn.ImmedSubItemGain;
            singleColumnSubItemAdjust = tnImmedSubItemGain;
            var absIndex = tn.Index + index + tnImmedSubItemGain + 1;
            while ((tn != null)
                   && (isExpanded = tn.MultiLine))
            {
                if (tn.Expanded)
                {
                    //First, take into account all expanded nodes at this level
                    //which occur before our start position
                    tnTmp = tn.FirstChild;
                    while ((tnTmp != null)
                           && tnTmp.Index < index)
                    {
                        if (tnTmp.Expanded)
                        {
                            tmpSubItemGain = tnTmp.FullSubItemGain;
                            absIndex += tnTmp.FullCount;
                        }
                        else
                        {
                            tmpSubItemGain = tnTmp.ImmedSubItemGain;
                        }
                        absIndex += tmpSubItemGain;
                        singleColumnSubItemAdjust += tmpSubItemGain;
                        tnTmp = tnTmp.NextSibling;
                    }
                }
                index = tn.Index;
                tnTmp = tn;
                if ((tn = tn.Parent) != null)
                {
                    tnImmedSubItemGain = tn.ImmedSubItemGain;
                    if (tnTmp.SubItemRoot)
                    {
                        // Track how many columns we need to shift for this column
                        column += tn.FirstSubItem.ColumnOfRootNode(tnTmp);

                        // The final index does not include the new parent's subitem gain,
                        // and there is no automatic plus one like with a normal expanded branch 
                        absIndex += tn.Index;
                        if (!tnTmp.ComplexSubItem)
                        {
                            // Expanded cell
                            ++absIndex;
                        }
                    }
                    else
                    {
                        singleColumnSubItemAdjust += tnImmedSubItemGain;
                        absIndex += tn.Index + tnImmedSubItemGain + 1;
                    }
                }
            }
            return isExpanded ? new VirtualTreeCoordinate(absIndex, column) : VirtualTreeCoordinate.Invalid;
        }

        private static VirtualTreeCoordinate EnumAbsoluteIndices(int index, ref TREENODE nextNode, out int singleColumnAbsIndex)
        {
            singleColumnAbsIndex = VirtualTreeConstant.NullIndex;
            if (nextNode != null)
            {
                int singleColumnSubItemAdjust;
                var coord = FindAbsoluteIndex(nextNode, index, out singleColumnSubItemAdjust);
                if (coord.Column == 0)
                {
                    singleColumnAbsIndex = coord.Row - singleColumnSubItemAdjust;
                }
                nextNode = nextNode.NextNode;
                return coord;
            }
            return VirtualTreeCoordinate.Invalid;
        }

        int ITree.GetOffsetFromParent(int parentRow, int column, int relativeIndex, bool complexColumnRoot)
        {
            return GetOffsetFromParent(parentRow, column, relativeIndex, complexColumnRoot);
        }

        /// <summary>
        ///     Determine how far a given index is from its parent node. Due to expansions
        ///     and subitems, this value can be much greater than the index in the branch itself.
        /// </summary>
        /// <param name="parentRow">The row of the parent object</param>
        /// <param name="column">The column coordinate</param>
        /// <param name="relativeIndex">The index in the child list to get the offset for</param>
        /// <param name="complexColumnRoot">The row and column are the first node of a complex subitem column. Return the offset from this node to other items in the root branch.</param>
        /// <returns>The offset from the parent to the given index in an expanded child list</returns>
        protected int GetOffsetFromParent(int parentRow, int column, int relativeIndex, bool complexColumnRoot)
        {
            TREENODE tn = null;
            if (parentRow == VirtualTreeConstant.NullIndex)
            {
                if (column > 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "column", VirtualTreeStrings.GetString(VirtualTreeStrings.ColumnOutOfRangeException));
                }
                tn = myRootNode;
            }
            else if (complexColumnRoot)
            {
                if (relativeIndex == 0)
                {
                    return 1;
                }
                var pos = TrackCell(parentRow, ref column);
                tn = pos.ParentNode;
                if (!tn.ComplexSubItem)
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ComplexColumnRootException));
                }
                Debug.Assert(column == 0); // column resolved by TrackCell for a complex item
            }
            else
            {
                var pos = TrackCell(parentRow, ref column);
                tn = pos.ParentNode.GetChildNode(pos.Index);
                if (column > 0
                    && tn != null)
                {
                    var sn = tn.SubItemAtColumn(column);
                    tn = (sn != null) ? sn.RootNode : null;
                }
            }
            if ((tn != null)
                && tn.Expanded)
            {
                return tn.GetChildOffset(relativeIndex);
            }
            else
            {
                throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ParentRowException));
            }
        }

        VirtualTreeCoordinate ITree.GetNavigationTarget(
            TreeNavigation direction, int sourceRow, int sourceColumn, ColumnPermutation columnPermutation)
        {
            return GetNavigationTarget(direction, sourceRow, sourceColumn, columnPermutation);
        }

        /// <summary>
        ///     Get the navigation target coordinate for a given cell.
        /// </summary>
        /// <param name="direction">The direction to navigate</param>
        /// <param name="sourceRow">The starting row coordinate</param>
        /// <param name="sourceColumn">The starting column coordinate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then sourceColumn and the returned column are relative to the
        ///     column permutation, not the unpermuted position.
        /// </param>
        /// <returns>The target coordinates, or VirtualTreeCoordinate.Invalid</returns>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "sourceRow+1",
            Justification = "[pedrosi] overflow not possible with checks made on sourceRow")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected VirtualTreeCoordinate GetNavigationTarget(
            TreeNavigation direction, int sourceRow, int sourceColumn, ColumnPermutation columnPermutation)
        {
            var targetRow = sourceRow;
            var targetColumn = sourceColumn;
            if (myRootNode == null)
            {
                return VirtualTreeCoordinate.Invalid;
            }
            var startColumn = sourceColumn;

            var retVal = false;
            var expansion = GetBlankExpansion(sourceRow, sourceColumn, columnPermutation);
            sourceRow = expansion.TopRow;
            var nativeSourceColumn = sourceColumn;
            if (expansion.AnchorColumn != VirtualTreeConstant.NullIndex)
            {
                sourceColumn = expansion.AnchorColumn;
            }
            if (columnPermutation != null)
            {
                nativeSourceColumn = columnPermutation.GetNativeColumn(sourceColumn);
            }

            var localColumn = nativeSourceColumn;
            var lastSubItem = false;
            var pos = TrackCell(sourceRow, ref localColumn, ref lastSubItem);
            TREENODE tnChild;
            var attemptNextColumn = false;
            int selectColumn;
            TREENODE tnParent = null;
            var rowOffset = 0;
            switch (direction)
            {
                case TreeNavigation.Parent:
                case TreeNavigation.ComplexParent:
                    if (localColumn == 0
                        &&
                        (pos.ParentAbsolute != -1 ||
                         (pos.ParentNode.SubItemRoot && !pos.ParentNode.ComplexSubItem)))
                    {
                        if (nativeSourceColumn > 0)
                        {
                            // This is more work than column zero. The parent
                            // absolute is relative to the root node in the branch.
                            targetRow = sourceRow - pos.ParentNode.GetChildOffset(pos.Index);
                        }
                        else
                        {
                            targetRow = pos.ParentAbsolute;
                        }
                        targetColumn = sourceColumn;
                        retVal = true;
                    }
                    else if (direction == TreeNavigation.ComplexParent)
                    {
                        if (pos.ParentNode.ComplexSubItem)
                        {
                            targetColumn = nativeSourceColumn - pos.ParentNode.Parent.FirstSubItem.ColumnOfRootNode(pos.ParentNode);
                            if ((columnPermutation == null)
                                || (-1 != (targetColumn = columnPermutation.GetPermutedColumn(targetColumn))))
                            {
                                targetRow = sourceRow - pos.ParentNode.GetChildOffset(pos.Index) + 1;
                                retVal = true;
                            }
                        }
                        else if (localColumn > 0)
                        {
                            targetColumn = nativeSourceColumn - localColumn;
                            if ((columnPermutation == null)
                                || (-1 != (targetColumn = columnPermutation.GetPermutedColumn(targetColumn))))
                            {
                                targetRow = sourceRow;
                                retVal = true;
                            }
                        }
                    }
                    break;
                case TreeNavigation.FirstChild:
                case TreeNavigation.LastChild:
                    // FirstChild and LastChild share code for testing if the node is expanded
                    tnChild = pos.ParentNode.GetChildNode(pos.Index);
                    if (localColumn > 0
                        && tnChild != null)
                    {
                        var sn = tnChild.SubItemAtColumn(localColumn);
                        tnChild = (sn == null) ? null : sn.RootNode;
                    }
                    if (tnChild != null
                        && tnChild.Expanded
                        && tnChild.ImmedCount > 0)
                    {
                        // Right jumps to FirstChild, test LastChild, not FirstChild, so
                        // else handles both elements.
                        if (direction == TreeNavigation.LastChild)
                        {
                            targetRow = sourceRow + tnChild.GetChildOffset(tnChild.ImmedCount - 1);
                        }
                        else
                        {
                            targetRow = sourceRow + tnChild.ImmedSubItemGain + 1;
                        }
                        targetColumn = sourceColumn;
                        retVal = true;
                    }
                    else if (attemptNextColumn)
                    {
                        goto case TreeNavigation.RightColumn;
                    }
                    break;
                case TreeNavigation.RightColumn:
                    {
                        // Move right to the next column. Note that this is not a natural
                        // thing to do from the tree perspective because the closest element
                        // to the right is likely to be only a distant relative of the current
                        // node, but the user is unlikely to appreciate this distinction and
                        // just wants to move right, so we let them.
                        if (nativeSourceColumn == 0
                            || localColumn > 0)
                        {
                            if (columnPermutation == null)
                            {
                                if (pos.ParentNode.GetColumnCount(pos.Index) > (localColumn + 1))
                                {
                                    targetRow = sourceRow;
                                    targetColumn = sourceColumn + 1;
                                    retVal = true;
                                }
                            }
                            else if (expansion.RightColumn < (columnPermutation.VisibleColumnCount - 1))
                            {
                                targetColumn = GetBlankExpansion(sourceRow, expansion.RightColumn + 1, columnPermutation).AnchorColumn;
                                targetRow = sourceRow;
                                retVal = true;
                            }
                        }
                        else
                        {
                            // Walk back up this tree until we get to a subitem root node, tracking
                            // offsets as we go.
                            tnParent = pos.ParentNode;
                            rowOffset = tnParent.GetChildOffset(pos.Index);
                            while (!tnParent.SubItemRoot)
                            {
                                tnChild = tnParent;
                                tnParent = tnChild.Parent;
                                rowOffset += tnParent.GetChildOffset(tnChild.Index);
                            }
                            if (tnParent.ComplexSubItem)
                            {
                                --rowOffset;
                            }
                            tnChild = tnParent;
                            tnParent = tnChild.Parent;

                            // Find the SUBITEMNODE for the next column and the next column
                            // index. The sub item node may be null if the next column is simple.
                            SUBITEMNODE snNext = null;
                            selectColumn = -1;
                            if (columnPermutation == null)
                            {
                                if (localColumn < (tnParent.Parent.GetColumnCount(tnParent.Index) - 1))
                                {
                                    var sn = tnParent.FirstSubItem;
                                    while (sn.RootNode != tnChild)
                                    {
                                        sn = sn.NextSibling;
                                    }
                                    var snTestNext = sn.NextSibling;
                                    if (snTestNext != null
                                        && (snTestNext.Column - sn.Column) == 1)
                                    {
                                        snNext = snTestNext;
                                    }
                                    selectColumn = sourceColumn + 1;
                                }
                            }
                            else
                            {
                                if (expansion.RightColumn < (columnPermutation.VisibleColumnCount - 1))
                                {
                                    selectColumn = GetBlankExpansion(sourceRow, expansion.RightColumn + 1, columnPermutation).AnchorColumn;
                                    // Note that column zero has no subitem node, so this covers this case
                                    // UNDONE_MC: Need local column, not global
                                    snNext = tnParent.SubItemAtColumn(columnPermutation.GetNativeColumn(selectColumn));
                                }
                            }

                            if (selectColumn != -1)
                            {
                                if (snNext != null)
                                {
                                    var nextGain = snNext.RootNode.TotalCount;
                                    if (snNext.RootNode.ComplexSubItem)
                                    {
                                        --nextGain;
                                    }
                                    else if (!snNext.RootNode.Expanded)
                                    {
                                        nextGain = 0;
                                    }
                                    if (nextGain < rowOffset)
                                    {
                                        targetRow = sourceRow - rowOffset + nextGain;
                                    }
                                    else
                                    {
                                        targetRow = sourceRow;
                                    }
                                }
                                else
                                {
                                    targetRow = sourceRow - rowOffset;
                                }
                                targetColumn = selectColumn;
                                retVal = true;
                            }
                        }
                        break;
                    }
                case TreeNavigation.NextSibling:
                    // Test localColumn. Expandable and simple cells don't have siblings,
                    // and all other nodes will give back a localColumn of 0.
                    if (localColumn == 0
                        && pos.Index < (pos.ParentNode.ImmedCount - 1))
                    {
                        targetRow = sourceRow + 1;
                        tnChild = pos.ParentNode.GetChildNode(pos.Index);
                        if (tnChild != null)
                        {
                            targetRow += tnChild.TotalCount;
                        }
                        targetColumn = sourceColumn;
                        retVal = true;
                    }
                    break;
                case TreeNavigation.PreviousSibling:
                    if (localColumn == 0
                        && pos.Index > 0)
                    {
                        if (pos.ParentNode.ComplexSubItem)
                        {
                            // The parent absolute is -1 and can't be used to give back a real count, 
                            // so use a more expensive algorithm in this case.
                            targetRow = sourceRow - pos.ParentNode.GetChildOffset(pos.Index) + pos.ParentNode.GetChildOffset(pos.Index - 1);
                        }
                        else
                        {
                            targetRow = pos.ParentAbsolute + pos.ParentNode.GetChildOffset(pos.Index - 1);
                        }
                        targetColumn = sourceColumn;
                        retVal = true;
                        break;
                    }
                    break;
                case TreeNavigation.Up:
                    while (targetRow > 0)
                    {
                        expansion = GetBlankExpansion(targetRow - 1, startColumn, columnPermutation);
                        targetRow = expansion.TopRow;
                        if (startColumn == expansion.AnchorColumn)
                        {
                            retVal = true;
                            break;
                        }
                    }
                    break;
                case TreeNavigation.Down:
                    while (expansion.BottomRow < (myRootNode.TotalCount - 1))
                    {
                        // Just return the next non-blank column
                        var testRow = expansion.BottomRow + 1;
                        expansion = GetBlankExpansion(testRow, startColumn, columnPermutation);
                        targetRow = expansion.TopRow;
                        if (startColumn == expansion.AnchorColumn
                            && targetRow >= testRow)
                        {
                            retVal = true;
                            break;
                        }
                    }
                    break;
                case TreeNavigation.LeftColumn:
                    if (localColumn > 0)
                    {
                        if (columnPermutation == null)
                        {
                            if (sourceColumn > 0)
                            {
                                targetRow = sourceRow;
                                targetColumn = sourceColumn - 1;
                                retVal = true;
                            }
                        }
                        else if (expansion.LeftColumn > 0)
                        {
                            targetRow = sourceRow;
                            targetColumn = GetBlankExpansion(targetRow, expansion.LeftColumn - 1, columnPermutation).AnchorColumn;
                            retVal = true;
                        }
                    }
                    else
                    {
                        // tnParent may have been set already in TreeNavigation.Left, only set it if needed
                        // Note that the nativeSourceColumn == 0 case can happen with a columnPermutation only
                        if (tnParent == null
                            && nativeSourceColumn != 0)
                        {
                            tnParent = pos.ParentNode;
                            rowOffset = tnParent.GetChildOffset(pos.Index);
                            while (!tnParent.SubItemRoot)
                            {
                                tnChild = tnParent;
                                tnParent = tnChild.Parent;
                                rowOffset += tnParent.GetChildOffset(tnChild.Index);
                            }
                            if (tnParent.ComplexSubItem)
                            {
                                --rowOffset;
                            }
                        }
                        selectColumn = -1;
                        SUBITEMNODE snPrev = null;
                        if (columnPermutation == null)
                        {
                            if (tnParent != null
                                && tnParent.Parent != null)
                            {
                                var sn = tnParent.Parent.FirstSubItem;
                                SUBITEMNODE snTestPrev = null;
                                while (sn.RootNode != tnParent)
                                {
                                    snTestPrev = sn;
                                    sn = sn.NextSibling;
                                }
                                if (snTestPrev != null
                                    && (sn.Column - snTestPrev.Column) == 1)
                                {
                                    snPrev = snTestPrev;
                                }
                            }
                            selectColumn = sourceColumn - 1;
                        }
                        else if (expansion.LeftColumn > 0)
                        {
                            selectColumn = GetBlankExpansion(sourceRow, expansion.LeftColumn - 1, columnPermutation).AnchorColumn;
                            if (tnParent != null)
                            {
                                // Note that column zero has no subitem node, so this covers this case
                                // UNDONE_MC: Need local column, not global
                                snPrev = tnParent.Parent.SubItemAtColumn(columnPermutation.GetNativeColumn(selectColumn));
                            }
                        }
                        if (selectColumn != -1)
                        {
                            targetColumn = selectColumn;
                            retVal = true;
                            if (snPrev == null)
                            {
                                // The previous column is the parent node or a simple cell. Move
                                // to that cell.
                                targetRow = sourceRow - rowOffset;
                            }
                            else
                            {
                                // The most closely related node to the left is the 0 node in the
                                // previous column. However, this node is not spatially closest
                                // to the current cell. The subtle the distinction that the closest
                                // cell is only a distant relative of the current one is generally lost
                                // on the user, so we just let them move left to the current value.
                                var prevGain = snPrev.RootNode.TotalCount;
                                if (snPrev.RootNode.ComplexSubItem)
                                {
                                    --prevGain;
                                }
                                else if (!snPrev.RootNode.Expanded)
                                {
                                    prevGain = 0;
                                }
                                if (prevGain < rowOffset)
                                {
                                    targetRow = sourceRow - rowOffset + prevGain;
                                }
                                else
                                {
                                    targetRow = sourceRow;
                                }
                            }
                        }
                    }
                    break;
                case TreeNavigation.Left:
                    if (sourceColumn > 0)
                    {
                        if (localColumn > 0
                            ||
                            (pos.Index == 0 && pos.ParentNode.ComplexSubItem))
                        {
                            // Move left one column
                            if (columnPermutation == null)
                            {
                                targetRow = sourceRow;
                                targetColumn = sourceColumn - 1;
                                retVal = true;
                            }
                            else if (expansion.LeftColumn > 0)
                            {
                                targetRow = sourceRow;
                                targetColumn = GetBlankExpansion(sourceRow, expansion.LeftColumn - 1, columnPermutation).AnchorColumn;
                                retVal = true;
                            }
                            break;
                        }
                        else if (pos.ParentNode.ComplexSubItem
                                 && pos.Index > 0)
                        {
                            tnParent = pos.ParentNode;
                            rowOffset = tnParent.GetChildOffset(pos.Index) - 1;
                            goto case TreeNavigation.LeftColumn;
                        }
                        else if (nativeSourceColumn == 0
                                 && pos.ParentNode.Index == -1)
                        {
                            goto case TreeNavigation.LeftColumn;
                        }
                    }
                    goto case TreeNavigation.Parent;
                case TreeNavigation.Right:
                    if (MultiColumnSupport)
                    {
                        if ((sourceColumn == 0)
                            ||
                            (pos.ParentNode.ComplexSubItem && pos.Index == 0)
                            ||
                            (localColumn > 0))
                        {
                            if (columnPermutation == null)
                            {
                                if (localColumn < (pos.ParentNode.GetColumnCount(pos.Index) - 1))
                                {
                                    targetRow = sourceRow;
                                    targetColumn = sourceColumn + 1;
                                    retVal = true;
                                    break;
                                }
                            }
                            else if (expansion.RightColumn < (columnPermutation.VisibleColumnCount - 1))
                            {
                                targetRow = sourceRow;
                                targetColumn = GetBlankExpansion(sourceRow, expansion.RightColumn + 1, columnPermutation).AnchorColumn;
                                retVal = true;
                                break;
                            }
                        }
                        else if (columnPermutation == null)
                        {
                            if (sourceColumn < (ColumnCount - 1))
                            {
                                // If firstchild doesn't go anywhere, then attempt
                                // to move to the next branch.
                                attemptNextColumn = true;
                            }
                        }
                        else if (expansion.RightColumn < (columnPermutation.VisibleColumnCount - 1))
                        {
                            attemptNextColumn = true;
                        }
                    }
                    goto case TreeNavigation.FirstChild;
            }
            return retVal ? new VirtualTreeCoordinate(targetRow, targetColumn) : VirtualTreeCoordinate.Invalid;
        }

        ColumnItemEnumerator ITree.EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter, bool markExcludedFilterItems)
        {
            return EnumerateColumnItems(column, columnPermutation, returnBlankAnchors, rowFilter, markExcludedFilterItems);
        }

        /// <summary>
        ///     Walk the items in a single tree column. EnumerateColumnItems is much more efficient
        ///     than calling GetItemInfo for each row in the column. The behavior of the returned
        ///     iterator is undefined if the tree structure changes during iteration.
        /// </summary>
        /// <param name="column">The column to iterate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the permutation.
        /// </param>
        /// <param name="returnBlankAnchors">
        ///     If an item is on a horizontal blank expansion, then
        ///     return the anchor for that item. All blanks are skipped if this is false.
        /// </param>
        /// <param name="rowFilter">
        ///     An array of items to return. The array must be sorted in ascending order
        ///     and all values must be valid indices in the tree.
        /// </param>
        /// <param name="markExcludedFilterItems">If true, items in the filter that are not returned will be marked by bitwise inverting them (~initialIndex)</param>
        protected ColumnItemEnumerator EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter, bool markExcludedFilterItems)
        {
            return new ColumnItemEnumeratorImpl(this, column, columnPermutation, returnBlankAnchors, rowFilter, markExcludedFilterItems);
        }

        ColumnItemEnumerator ITree.EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow)
        {
            return EnumerateColumnItems(column, columnPermutation, returnBlankAnchors, startRow, endRow);
        }

        /// <summary>
        ///     Walk the items in a single tree column. EnumerateColumnItems is much more efficient
        ///     than calling GetItemInfo for each row in the column. The behavior of the returned
        ///     iterator is undefined if the tree structure changes during iteration.
        /// </summary>
        /// <param name="column">The column to iterate</param>
        /// <param name="columnPermutation">
        ///     The column permutation to apply. If this
        ///     is provided, then column is relative to the permutation.
        /// </param>
        /// <param name="returnBlankAnchors">
        ///     If an item is on a horizontal blank expansion, then
        ///     return the anchor for that item. All blanks are skipped if this is false.
        /// </param>
        /// <param name="startRow">The position of the first item to return</param>
        /// <param name="endRow">
        ///     The position of the last item to return. If endRow is less than startRow, the
        ///     the iterator will cycle back to the top of the tree. To search to the end of the list, pass an endRow
        ///     of -1 (VirtualTreeConstant.NullIndex)
        /// </param>
        protected ColumnItemEnumerator EnumerateColumnItems(
            int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow)
        {
            return new ColumnItemEnumeratorImpl(this, column, columnPermutation, returnBlankAnchors, startRow, endRow);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EnumOrderedListItems(
            ref int nextStartRow,
            int column,
            ColumnPermutation permutation,
            bool returnBlankAnchors,
            out IBranch branch,
            out int treeColumn,
            out int firstRelativeRow,
            out int lastRelativeRow,
            out int relativeColumn,
            out int level,
            out int trailingBlanks,
            out bool simpleCell)
        {
            trailingBlanks = 0;
            relativeColumn = column;
            treeColumn = column;
            var pos = TrackCell(nextStartRow, ref relativeColumn);
            TREENODE tn;
            var columnCount = 0;
            var tnParent = pos.ParentNode;
            level = pos.Level;
            var processItem = true;
            var verifyColumnCount = false;
            if (pos.IsBlank(relativeColumn))
            {
                // We're currently in a blank cell. Determine the next non-blank cell in this column
                // unless we've been asked to return the blank expansions.
                columnCount = tnParent.GetColumnCount(pos.Index);
                if (relativeColumn >= columnCount)
                {
                    tn = null;
                    if (returnBlankAnchors)
                    {
                        // For returning blank anchors, we need to see if we're within
                        // the range of rows supported by the last column in this row that
                        // has items.
                        if (permutation != null)
                        {
                            var expansion = permutation.GetColumnExpansion(
                                permutation.GetPermutedColumn(relativeColumn), columnCount - 1
                                /*UNDONE_MC: Column count local, not global, relativeColumn local, not global*/);
                            if (expansion.AnchorColumn == VirtualTreeConstant.NullIndex)
                            {
                                processItem = false;
                            }
                            else
                            {
                                // UNDONE_MC: Global and relative not the same
                                relativeColumn = treeColumn = permutation.GetNativeColumn(expansion.AnchorColumn);
                            }
                        }
                        else
                        {
                            // UNDONE_MC: Global and relative not the same
                            relativeColumn = treeColumn = columnCount - 1;
                        }
                        if (processItem)
                        {
                            pos = TrackCell(nextStartRow, ref relativeColumn);
                            if (pos.IsBlank(relativeColumn))
                            {
                                processItem = false;
                            }
                            else
                            {
                                tnParent = pos.ParentNode;
                                level = pos.Level;
                                verifyColumnCount = tnParent.JaggedColumns;
                            }
                        }
                    }
                    else
                    {
                        processItem = false;
                    }
                    if (!processItem)
                    {
                        // This isn't perfect, but a call to GetBlankExpansion is unlikely to give better information,
                        // so we just move one at a time. Moving one row at a time is relatively harmless here and shouldn't
                        // cause any sort of noticeable performance hit.
                        level = -1;
                        if ((tnParent.ExpandedSubItemGain != 0)
                            && (null != (tn = tnParent.GetChildNode(pos.Index))))
                        {
                            // If there is a reason to check, then go looking for the full subitem gain
                            // of a child at this node.
                            trailingBlanks = tn.ImmedSubItemGain - pos.SubItemOffset + 1; // Include this item in the blank count
                            nextStartRow += trailingBlanks;
                        }
                        else
                        {
                            trailingBlanks = 1; // Ask the next item
                            ++nextStartRow;
                        }
                    }
                }
                else
                {
                    processItem = false;
                    tn = tnParent.GetChildNode(pos.Index);
                    if (relativeColumn != 0)
                    {
                        // Be consistent with GetItemInfo
                        level = -1;
                    }
                    trailingBlanks = tn.ImmedSubItemGain - pos.SubItemOffset + 1; // Include this item in the blank count
                    nextStartRow += trailingBlanks;
                }
                if (nextStartRow >= myRootNode.TotalCount)
                {
                    nextStartRow = VirtualTreeConstant.NullIndex;
                }
            }
            if (processItem)
            {
                lastRelativeRow = tnParent.ImmedCount - 1; // Default value if nothing else found
                branch = tnParent.Branch;
                firstRelativeRow = pos.Index;
                int subItemGain;
                var totalSubItemGain = 0;
                trailingBlanks = 0;
                var lastVerifiedColumnCount = pos.Index;
                for (tn = tnParent.FirstChild; tn != null; tn = tn.NextSibling)
                {
                    if (pos.Index <= tn.Index)
                    {
                        for (; tn != null; tn = tn.NextSibling)
                        {
                            if (verifyColumnCount)
                            {
                                // In this case, we only want to use columns with
                                // the current column count. This prevents requests with
                                // returnBlankAnchors true from attaching to an anchor in
                                // the same column.
                                var testIndex = tn.Index;
                                for (var i = lastVerifiedColumnCount + 1; i <= testIndex; ++i)
                                {
                                    if (tnParent.GetColumnCount(i) != columnCount)
                                    {
                                        lastRelativeRow = lastVerifiedColumnCount = i - 1;
                                        testIndex = -1;
                                        break;
                                    }
                                }
                                if (testIndex == -1)
                                {
                                    tn = null;
                                    break;
                                }
                                lastVerifiedColumnCount = testIndex;
                            }
                            subItemGain = tn.ImmedSubItemGain;
                            if ((tn.Expanded && tn.ImmedCount > 0)
                                || (subItemGain > 0))
                            {
                                lastRelativeRow = tn.Index;
                                trailingBlanks = subItemGain;
                                break;
                            }
                            totalSubItemGain += subItemGain;
                        }
                        break;
                    }
                }

                if (verifyColumnCount && lastVerifiedColumnCount < lastRelativeRow)
                {
                    for (var i = lastVerifiedColumnCount + 1; i <= lastRelativeRow; ++i)
                    {
                        if (tnParent.GetColumnCount(i) != columnCount)
                        {
                            lastRelativeRow = i - 1;
                            break;
                        }
                    }
                }

                nextStartRow += lastRelativeRow - firstRelativeRow + totalSubItemGain + 1;
                if (tn != null)
                {
                    nextStartRow += tn.ImmedSubItemGain; // Undo later if not correct
                    if (relativeColumn != 0)
                    {
                        // Don't pretest SubItemStyle, can't tell if we got a complex or expanded node
                        var sn = tn.SubItemAtColumn(relativeColumn);
                        if (sn != null)
                        {
                            var subItemRoot = sn.RootNode;
                            if (!subItemRoot.ComplexSubItem
                                && subItemRoot.Expanded
                                && subItemRoot.TotalCount > 0)
                            {
                                // We went too far, undo
                                nextStartRow -= tn.ImmedSubItemGain;
                            }
                        }
                    }
                }
                // See if we're looking at a simple cell. This extra
                // information is needed by ComputeWidthOfRange on the
                // control side to get an accurate width, so we need to
                // be consistent with GetItemInfo here.
                if (level == 0 && MultiColumnSupport)
                {
                    if (!tnParent.MultiColumn)
                    {
                        simpleCell = tnParent.InSubItemColumn && tnParent.NoChildExpansion;
                    }
                    else if (column == 0)
                    {
                        simpleCell = tnParent.NoChildExpansion;
                    }
                    else
                    {
                        simpleCell = !(pos.IsExpanded(column) || pos.IsExpandable(column));
                    }
                }
                else
                {
                    simpleCell = false;
                }

                Debug.Assert(nextStartRow <= myRootNode.TotalCount, "Off end of list");
                if (nextStartRow == myRootNode.TotalCount) // == instead of >= OK with assert
                {
                    nextStartRow = VirtualTreeConstant.NullIndex;
                }
            }
            else
            {
                branch = null;
                simpleCell = false;
                firstRelativeRow = lastRelativeRow = VirtualTreeConstant.NullIndex;
            }
        }

        private void EnumSingleColumnOrderedListItems(
            ref int nextStartRow,
            out IBranch branch,
            out int firstRelativeRow,
            out int lastRelativeRow,
            out int level)
        {
            // UNDONE_CACHE: Get a position cache in here
            var pos = TrackSingleColumnRow(nextStartRow);
            TREENODE tn;
            var tnParent = pos.ParentNode;
            level = pos.Level;

            lastRelativeRow = tnParent.ImmedCount - 1; // Default value if nothing else found
            branch = tnParent.Branch;
            firstRelativeRow = pos.Index;
            for (tn = tnParent.FirstChild; tn != null; tn = tn.NextSibling)
            {
                if (pos.Index <= tn.Index)
                {
                    for (; tn != null; tn = tn.NextSibling)
                    {
                        if (tn.Expanded
                            && tn.ImmedCount > 0)
                        {
                            lastRelativeRow = tn.Index;
                            break;
                        }
                    }
                    break;
                }
            }
            nextStartRow += lastRelativeRow - firstRelativeRow + 1;
            Debug.Assert(nextStartRow <= myRootNode.FullCount, "Off end of list");
            if (nextStartRow == myRootNode.FullCount) // == instead of >= OK with assert
            {
                nextStartRow = VirtualTreeConstant.NullIndex;
            }
        }

        //Addrefed redraw toggle FALSE turns off, TRUE turns on (depending on refcount)
        //OnSetRedraw fires only when toggling to/from 0 refcount
        bool ITree.Redraw
        {
            get { return Redraw; }
            set { Redraw = value; }
        }

        /// <summary>
        ///     Views on the tree should not attempt to redraw any items when Redraw is off.
        ///     Calls to Redraw can be nested, and must be balanced.
        /// </summary>
        /// <value>false to turn off redraw, true to restore it.</value>
        protected bool Redraw
        {
            get { return myRedrawCount == 0; }
            set { SetRedraw(value, true, true); }
        }

        private void SetRedraw(bool value, bool fireNormalEvent, bool fireSingleColumnEvent)
        {
            if (GetStateFlag(TreeStateFlags.InExpansion))
            {
                return;
            }

            if (value)
            {
                if (--myRedrawCount != 0)
                {
                    return;
                }
            }
            else
            {
                var notified = DelayTurnOffRedraw();
                ++myRedrawCount;
                if (notified || (myRedrawCount != 1))
                {
                    fireNormalEvent = false;
                    ;
                }
                notified = DelayTurnOffSingleColumnRedraw();
                if (notified || (myRedrawCount != 1))
                {
                    fireSingleColumnEvent = false;
                }
            }

            if (fireNormalEvent)
            {
                Debug.Assert(!GetStateFlag(TreeStateFlags.TurnOffRedraw), "Don't call Redraw = true if TurnOffRedraw is set");
                Debug.Assert(myRedrawCount != -1, "Redraw calls unbalanced");
                if (OnSetRedraw != null)
                {
                    OnSetRedraw(this, new SetRedrawEventArgs(value));
                }
            }

            if (fireSingleColumnEvent)
            {
                Debug.Assert(!GetStateFlag(TreeStateFlags.TurnOffSingleColumnRedraw), "Don't call Redraw = true if TurnOffRedraw is set");
                Debug.Assert(myRedrawCount != -1, "Redraw calls unbalanced");
                if (GetStateFlag(TreeStateFlags.FireSingleColumnOnSetRedraw))
                {
                    var singleTree = SingleColumnTree;
                    (singleTree as SingleColumnView).myOnSetRedraw(singleTree, new SetRedrawEventArgs(value));
                }
            }
        }

        bool ITree.DelayRedraw
        {
            get { return DelayRedraw; }
            set { DelayRedraw = value; }
        }

        /// <summary>
        ///     Used to batch calls to Redraw without triggering unnecessary redraw operations.
        ///     Set this property to true if an operation may cause one or more redraw calls, then
        ///     to false on completion. The cost is negligible if Redraw is never triggered, whereas
        ///     an unneeded Redraw true/false can be very expensive. Calls to DelayRedraw can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <value>true to delay, false to finish operation</value>
        protected bool DelayRedraw
        {
            get { return myDelayRedrawCount > 0; }
            set
            {
                if (GetStateFlag(TreeStateFlags.InExpansion))
                {
                    return;
                }
                if (value ? myDelayRedrawCount++ == 0 : --myDelayRedrawCount == 0)
                {
                    Debug.Assert(myDelayRedrawCount != -1, "DelayRedraw calls unbalanced");
                    if (value)
                    {
                        SetStateFlag(TreeStateFlags.TurnOffRedraw | TreeStateFlags.TurnOffSingleColumnRedraw, myRedrawCount == 0);
                        ++myRedrawCount;
                    }
                    else
                    {
                        var fireNormalRedraw = true;
                        var fireSingleColumnRedraw = true;
                        // If the delay redraw flags are on, then nothing
                        // actually fired to cause OnSetRedraw notifications.
                        if (GetStateFlag(TreeStateFlags.TurnOffRedraw))
                        {
                            fireNormalRedraw = false;
                            SetStateFlag(TreeStateFlags.TurnOffRedraw, false);
                        }
                        if (GetStateFlag(TreeStateFlags.TurnOffSingleColumnRedraw))
                        {
                            fireSingleColumnRedraw = false;
                            SetStateFlag(TreeStateFlags.TurnOffSingleColumnRedraw, false);
                        }
                        SetRedraw(true, fireNormalRedraw, fireSingleColumnRedraw);
                    }
                }
            }
        }

        bool ITree.DelayListShuffle
        {
            get { return DelayListShuffle; }
            set { DelayListShuffle = value; }
        }

        /// <summary>
        ///     Used to batch calls to ListShuffle without triggering unnecessary shuffle operations.
        ///     Set this property to true if an operation may cause one or more list shuffles, then
        ///     to false on completion. The cost is negligible if a shuffle is never triggered, whereas
        ///     an unneeded ListShuffle true/false can be very expensive. Calls to DelayListShuffle can
        ///     be nested, and must be balanced.
        /// </summary>
        /// <value>true to delay, false to finish operation</value>
        protected bool DelayListShuffle
        {
            get { return myDelayShuffleCount > 0; }
            set
            {
                if (value)
                {
                    if (myDelayShuffleCount == 0)
                    {
                        SetStateFlag(TreeStateFlags.TurnOffShuffle, false);
                    }
                    ++myDelayShuffleCount;
                }
                else
                {
                    --myDelayShuffleCount;
                    Debug.Assert(myShuffleCount != -1, "DelayListShuffle calls unbalanced");
                    if (myDelayShuffleCount == 0
                        && GetStateFlag(TreeStateFlags.TurnOffShuffle))
                    {
                        SetStateFlag(TreeStateFlags.TurnOffShuffle, false);
                        DoAfterListShuffle();
                    }
                }
            }
        }

        private void DoBeforeListShuffle()
        {
            DelayTurnOffRedraw();
            // Begin shuffling list by retrieving PositionTracker arrays from
            // all of our listeners (if any).
            Debug.Assert(myPositionManager == null); // Should be long gone at this point
            // Check after as well, Before is useless without an After listener
            if (ListShuffleBeginning != null
                && ListShuffleEnding != null)
            {
                var positionManager = new PositionManagerEventArgs(this);
                NODEPOSITIONTRACKER ntHead = null;
                NODEPOSITIONTRACKER ntLast = null;
                TREENODE tnParent;
                int relativeRow;
                int relativeColumn;
                try
                {
                    ListShuffleBeginning(this, positionManager);
                    foreach (PositionTracker[] trackerSet in positionManager)
                    {
                        var upper = trackerSet.GetUpperBound(0);
                        for (var i = trackerSet.GetLowerBound(0); i <= upper; ++i)
                        {
                            if (TrackPosition(ref trackerSet[i], out tnParent, out relativeRow, out relativeColumn))
                            {
                                if (ntHead != null)
                                {
                                    if (NODEPOSITIONTRACKER.Add(tnParent, trackerSet, i, relativeRow, relativeColumn, ref ntLast))
                                    {
                                        continue;
                                    }
                                }
                                else if (NODEPOSITIONTRACKER.Add(tnParent, trackerSet, i, relativeRow, relativeColumn, ref ntHead))
                                {
                                    ntLast = ntHead;
                                    continue;
                                }
                            }
                            ClearPositionTracker(ref trackerSet[i]);
                        }
                    }
                }
                catch
                {
                    // Errors from the branches are caught during add, so this is a
                    // failure on our side. Make sure that all position tracking information
                    // is fully detached from the TREENODE objects before continuing. 
                    if (ntHead != null)
                    {
                        NODEPOSITIONTRACKER.DetachAll(ref ntHead);
                    }
                    throw;
                }
                finally
                {
                    // We made it all the way, go ahead and record the results
                    // with this object. Note that we record our results even if
                    // the position head is null so that the user gets a ListShuffleEnding
                    // event if nothing was tracked successfully. This lets the listener
                    // reduce the set of operations they do during other insert and delete
                    // notifications while a position manager is active.
                    myPositionManager = positionManager;
                    myPositionHead = ntHead;
                }
            }
        }

        private void DoAfterListShuffle()
        {
            if (myPositionManager != null)
            {
                try
                {
                    if (ListShuffleEnding != null)
                    {
                        // Fill in the current positions for each position tracker
                        // by asking the associated lists where the object is now.
                        if (myPositionHead != null)
                        {
                            NODEPOSITIONTRACKER.UpdateEndPositions(myPositionHead);
                        }

                        // Now that all of the PositionTracker structures are up
                        // to date, notify the listeners to update their selections.
                        ListShuffleEnding(this, myPositionManager);
                    }
                }
                finally
                {
                    NODEPOSITIONTRACKER.DetachAll(ref myPositionHead);
                    myPositionManager = null;
                }
            }
        }

        bool ITree.ListShuffle
        {
            get { return ListShuffle; }
            set { ListShuffle = value; }
        }

        /// <summary>
        ///     A reference-counted property for beginning/ending a list shuffle. A list
        ///     shuffle is any major change in the structure of the tree for which you need
        ///     to preserve expansion and selection information. Calls to this property must
        ///     be balanced (set to true, do your work, then set to false)
        /// </summary>
        /// <value>true to begin a list shuffle operation, false to end it</value>
        protected bool ListShuffle
        {
            get { return myShuffleCount > 0; }
            set
            {
                if (value)
                {
                    ++myShuffleCount;
                    if (myShuffleCount == 1
                        && !GetStateFlag(TreeStateFlags.TurnOffShuffle))
                    {
                        DoBeforeListShuffle();
                        SetStateFlag(TreeStateFlags.TurnOffShuffle, true);
                    }
                }
                else
                {
                    --myShuffleCount;
                    Debug.Assert(myShuffleCount != -1, "ListShuffle calls unbalanced");
                    if (myShuffleCount == 0
                        && myDelayShuffleCount == 0)
                    {
                        SetStateFlag(TreeStateFlags.TurnOffShuffle, false);
                        DoAfterListShuffle();
                    }
                }
            }
        }

        private static void ClearPositionTracker(ref PositionTracker tracker)
        {
            tracker.EndRow = tracker.StartRow = VirtualTreeConstant.NullIndex;
        }

        private bool TrackPosition(ref PositionTracker tracker, out TREENODE tnParent, out int relativeRow, out int relativeColumn)
        {
            relativeRow = VirtualTreeConstant.NullIndex;
            relativeColumn = 0;
            tnParent = null;
            var startRow = tracker.StartRow;
            if (startRow != VirtualTreeConstant.NullIndex
                && startRow < (this as ITree).VisibleItemCount)
            {
                var column = tracker.Column;
                var pos = TrackCell(startRow, ref column);
                if (!pos.ParentNode.NoTracking)
                {
                    tnParent = pos.ParentNode;
                    relativeRow = pos.Index;
                    relativeColumn = column;
                }
            }
            return tnParent != null;
        }

        bool ITree.IsItemVisible(int absIndex)
        {
            return IsItemVisible(absIndex);
        }

        /// <summary>
        ///     Fire OnQueryItemVisible events to listeners to see if an item is visible in any view
        /// </summary>
        /// <param name="absIndex">The absolute index of the item to test</param>
        /// <returns>true if the item is visible. If an OnQueryItemVisible listener is not attached, then the item is assumed to be visible.</returns>
        protected bool IsItemVisible(int absIndex)
        {
            if (OnQueryItemVisible == null)
            {
                return true;
            }
            else
            {
                var args = new QueryItemVisibleEventArgs(absIndex);
                OnQueryItemVisible(this, args);
                return args.IsVisible;
            }
        }

        void ITree.RemoveBranch(IBranch branch)
        {
            RemoveBranch(branch);
        }

        /// <summary>
        ///     Remove all occurrences of branch from the tree. Note that removing all
        ///     items from a branch is not the same as removing the branch itself.
        /// </summary>
        /// <param name="branch">The branch to remove</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected void RemoveBranch(IBranch branch)
        {
            var tnNextNode = LocateTrackedNode(branch);
            TREENODE tn;
            Debug.Assert(tnNextNode != null); //Expect LocateTrackedNode to throw otherwise
            int killCount;
            int subItemKillCount;
            var enableSingleColumnEvent = GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged)
                                          | GetStateFlag(TreeStateFlags.FireSingleColumnOnDisplayDataChanged);
            var enableNormalEvent = ItemCountChanged != null || OnDisplayDataChanged != null;
            var notify = enableNormalEvent || enableSingleColumnEvent;
            while (null != (tn = tnNextNode))
            {
                Debug.Assert(!tn.ComplexSubItem); // Not handled, UNDONE_MC
                var absIndex = -1;
                var singleColumnAbsIndex = -1;
                var notifyColumn = 0;

                tnNextNode = tn.NextNode;
                if (notify)
                {
                    // We need to get the notification index before deleting anything
                    var tnNext = tn;
                    var coord = EnumAbsoluteIndices(VirtualTreeConstant.NullIndex, ref tnNext, out singleColumnAbsIndex);
                    if (coord.IsValid)
                    {
                        absIndex = coord.Row;
                        notifyColumn = coord.Column;
                    }
                }
                killCount = subItemKillCount = 0;
                if (tn.Expanded)
                {
                    killCount = tn.FullCount;
                    subItemKillCount = tn.ExpandedSubItemGain;
                    if ((killCount + subItemKillCount) > 0)
                    {
                        // UNDONE_MC: Need broader method call to catch all subitem cases
                        ChangeFullCountRecursive(tn, -killCount, -subItemKillCount);
                    }
                }
                var tnTemp = tn.Parent;
                if (tnTemp != null)
                {
                    if (tn.FirstSubItem == null)
                    {
                        if (tn == tnTemp.FirstChild)
                        {
                            tnTemp.FirstChild = tn.NextSibling;
                        }
                        else
                        {
                            tnTemp = tnTemp.FirstChild;
                            while (tnTemp.NextSibling != tn)
                            {
                                tnTemp = tnTemp.NextSibling;
                            }

                            tnTemp.NextSibling = tn.NextSibling;
                        }
                        FreeRecursive(ref tn);
                    }
                    else
                    {
                        var tnNext = tn.FirstChild;
                        while (null != (tnTemp = tnNext))
                        {
                            tnNext = tnTemp.NextSibling;
                            FreeRecursive(ref tnTemp);
                        }
                        tn.FirstChild = null;
                        tn.Expanded = false;
                    }
                }
                else
                {
                    Debug.Assert(myRootNode != null && myRootNode.Branch == branch);
                    (this as ITree).Root = null;
                    break;
                }
                if (notify)
                {
                    if (enableNormalEvent && absIndex != -1)
                    {
                        if ((killCount + subItemKillCount) == 0)
                        {
                            if (OnDisplayDataChanged != null)
                            {
                                OnDisplayDataChanged(
                                    this,
                                    new DisplayDataChangedEventArgs(
                                        this, VirtualTreeDisplayDataChanges.VisibleElements, absIndex, notifyColumn, 1));
                            }
                        }
                        else if (ItemCountChanged != null)
                        {
                            // UNDONE_MC: See comments in InsertItems. Fold the notifications
                            // for the three size changes into one, and get enough info
                            // back to do multi column. For example, the ComplexSubItem check
                            // in this if statement is equivalent to the output rowIncr parameter
                            // from the more complex ChangeFullCountRecursive call.
                            // UNDONE_NOW:
                            DelayTurnOffRedraw();
                            //ItemsDeleted(this, absIndex, killCount);
                            ItemCountChanged(
                                this,
                                new ItemCountChangedEventArgs(
                                    this, absIndex, notifyColumn, -killCount - subItemKillCount, absIndex, 0, null, true));
                        }
                    }
                    if (enableSingleColumnEvent && singleColumnAbsIndex != -1)
                    {
                        var singleTree = SingleColumnTree;
                        if (killCount == 0)
                        {
                            if (GetStateFlag(TreeStateFlags.FireSingleColumnOnDisplayDataChanged))
                            {
                                (singleTree as SingleColumnView).myOnDisplayDataChanged(
                                    singleTree,
                                    new DisplayDataChangedEventArgs(
                                        singleTree, VirtualTreeDisplayDataChanges.VisibleElements, singleColumnAbsIndex, 0, 1));
                            }
                        }
                        else if (GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged))
                        {
                            DelayTurnOffSingleColumnRedraw();
                            (singleTree as SingleColumnView).myItemCountChanged(
                                singleTree,
                                new ItemCountChangedEventArgs(
                                    singleTree, singleColumnAbsIndex, 0, -killCount, singleColumnAbsIndex, 0, null, true));
                        }
                    }
                }
            }
            ClearPositionCache(); //Cached absolute information is toast.
        }

        private void FreeRecursive(ref TREENODE pStart)
        {
            if (pStart != null)
            {
                var tn1 = pStart;
                TREENODE tn2;
                SUBITEMNODE sn;

                //Stop the recursion from walking above the node
                tn1.Parent = null;
                while (tn1 != null)
                {
                    tn2 = tn1.FirstChild;
                    if (tn2 != null)
                    {
                        tn1.FirstChild = tn2.NextSibling;
                        tn1 = tn2;
                    }
                    else if (tn1.FirstSubItem != null)
                    {
                        sn = tn1.FirstSubItem;
                        tn1.FirstSubItem = sn.NextSibling;
                        tn1 = sn.RootNode;
                    }
                    else
                    {
                        tn2 = tn1;
                        tn1 = tn2.Parent;
                        DestroyTreeNode(ref tn2);
                    }
                }
                pStart = null;
            }
        }

        private void FreeRecursive(ref TREENODE pStart, ref NODEPOSITIONTRACKER_Dynamic ntDetached)
        {
            if (pStart != null)
            {
                if (myPositionManager == null
                    || pStart.NoTracking
                    || pStart.DefaultTracking)
                {
                    FreeRecursive(ref pStart);
                    return;
                }
                var tn1 = pStart;
                TREENODE tn2;
                SUBITEMNODE sn;

                //Stop the recursion from walking above the node
                tn1.Parent = null;
                while (tn1 != null)
                {
                    tn2 = tn1.FirstChild;
                    if (tn2 != null)
                    {
                        tn1.FirstChild = tn2.NextSibling;
                        tn1 = tn2;
                    }
                    else if (tn1.FirstSubItem != null)
                    {
                        sn = tn1.FirstSubItem;
                        tn1.FirstSubItem = sn.NextSibling;
                        tn1 = sn.RootNode;
                    }
                    else
                    {
                        tn2 = tn1;
                        tn1 = tn2.Parent;
                        if (tn2.FirstPositionTracker != null)
                        {
                            NODEPOSITIONTRACKER_Dynamic.DetachTrackers(ref tn2.FirstPositionTracker, ref ntDetached);
                            Debug.Assert(tn2.FirstPositionTracker == null);
                        }
                        DestroyTreeNode(ref tn2);
                    }
                }
                pStart = null;
            }
        }

        private void DestroyTreeNode(ref TREENODE tn)
        {
            if (tn.Branch != null)
            {
                tn.Branch.OnBranchModification -= OnBranchModification;
                if (tn.Dynamic)
                {
                    RemoveTrackedNode(tn.Branch, tn);
                }
            }
            if (tn.FirstPositionTracker != null)
            {
                tn.FirstPositionTracker.OnParentNodeDeleted();
            }
            tn = null;
        }

        private TrackedTreeNodeCollection NodeTracker
        {
            get
            {
                if (myNodeTracker == null)
                {
                    myNodeTracker = new TrackedTreeNodeCollection();
                }
                return myNodeTracker;
            }
        }

        private void RemoveTrackedNode(IBranch branch, TREENODE tn)
        {
            Debug.Assert(myNodeTracker != null);
            myNodeTracker.Remove(branch, tn);
        }

        private void AddTrackedNode(IBranch branch, TREENODE tn)
        {
            NodeTracker.Add(branch, tn);
        }

        private TREENODE LocateTrackedNode(IBranch branch)
        {
            Debug.Assert(myNodeTracker != null);
            return myNodeTracker[branch];
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static TREENODE CreateTreeNode(
            TREENODE startNode, IBranch branch, VirtualTree tree, bool allowMultiColumn, bool inSubItemColumn, bool multiColumnParent)
        {
            Debug.Assert(startNode == null || (startNode.Branch == null && startNode.GetType() == typeof(TREENODE_Complex)));
            BranchFeatures tf;
            TREENODE retVal;
            tf = branch.Features;
            var isDynamic = TREENODE.RequireDynamic(tf);
            var supportMultiColumn = tree.MultiColumnSupport;
            allowMultiColumn &= supportMultiColumn;
            var isMultiColumn = false;
            if (allowMultiColumn)
            {
                var mcTest = branch as IMultiColumnBranch;
                // Checking the column count allows a single wrapper class to
                // support either single or multicolumn branches
                isMultiColumn = mcTest != null && mcTest.ColumnCount > 1;
            }

            if (isDynamic)
            {
                if (TREENODE.RequireUpdatable(tf))
                {
                    retVal = isMultiColumn
                                 ? new TREENODE_Multi_Tracked_Updatable()
                                 : (supportMultiColumn
                                        ? (multiColumnParent
                                               ? (new TREENODE_Complex_Tracked_Updatable()) as TREENODE
                                               : new TREENODE_Single_Tracked_Updatable())
                                        : new TREENODE_Tracked_Updatable());
                }
                else
                {
                    retVal = isMultiColumn
                                 ? new TREENODE_Multi_Tracked()
                                 : (supportMultiColumn
                                        ? (multiColumnParent
                                               ? (new TREENODE_Complex_Tracked()) as TREENODE
                                               : new TREENODE_Single_Tracked())
                                        : new TREENODE_Tracked());
                }
            }
            else if (TREENODE.RequireUpdatable(tf))
            {
                retVal = isMultiColumn
                             ? new TREENODE_Multi_Updatable()
                             : (supportMultiColumn
                                    ? (multiColumnParent
                                           ? (new TREENODE_Complex_Updatable()) as TREENODE
                                           : new TREENODE_Single_Updatable())
                                    : new TREENODE_Updatable());
            }
            else
            {
                retVal = isMultiColumn
                             ? new TREENODE_Multi()
                             : (supportMultiColumn
                                    ? (multiColumnParent
                                           ? ((startNode == null) ? new TREENODE_Complex() : startNode)
                                           : new TREENODE_Single())
                                    : new TREENODE());
            }
            retVal.Branch = branch;
            retVal.SetFlags(tf);
            retVal.MultiColumn = isMultiColumn;
            retVal.AllowMultiColumnChildren = allowMultiColumn;
            retVal.InSubItemColumn = inSubItemColumn;
            if (retVal.CallUpdate)
            {
                retVal.UpdateCounter = branch.UpdateCounter;
            }
            branch.OnBranchModification += tree.OnBranchModification;

            if (startNode != null
                && startNode != retVal)
            {
                var subItem = startNode.FirstSubItem;
                retVal.FirstSubItem = subItem;
                retVal.ImmedSubItemGain = startNode.ImmedSubItemGain;
                retVal.FullSubItemGain = startNode.FullSubItemGain;
                while (subItem != null)
                {
                    subItem.RootNode.Parent = retVal;
                    subItem = subItem.NextSibling;
                }
                TREENODE tnPrev = null;
                var tnTest = startNode.Parent.FirstChild;
                while (tnTest != startNode)
                {
                    tnPrev = tnTest;
                    tnTest = tnTest.NextSibling;
                }
                if (tnPrev == null)
                {
                    startNode.Parent.FirstChild = retVal;
                    retVal.NextSibling = startNode.NextSibling;
                }
                else
                {
                    tnPrev.NextSibling = retVal;
                    retVal.NextSibling = startNode.NextSibling;
                }
            }

            return retVal;
        }

        private TREENODE ExpandTreeNode(
            TREENODE parentNode, TREENODE startNode, int row, int column, bool insertNewChild, out int itemIncr, out int subItemIncr)
        {
            ExpansionOptions options;
            return ExpandTreeNode(
                parentNode,
                startNode,
                row,
                column,
                column == 0 ? ObjectStyle.ExpandedBranch : ObjectStyle.SubItemExpansion,
                insertNewChild,
                out options,
                out itemIncr,
                out subItemIncr);
        }

        private TREENODE ExpandTreeNode(
            TREENODE parentNode, TREENODE startNode, int row, int column, bool insertNewChild, out int itemIncr,
            out bool requireInitialSubItemExpansion)
        {
            ExpansionOptions options;
            return ExpandTreeNode(
                parentNode,
                startNode,
                row,
                column,
                column == 0 ? ObjectStyle.ExpandedBranch : ObjectStyle.SubItemExpansion,
                insertNewChild,
                out options,
                out itemIncr,
                out requireInitialSubItemExpansion);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private TREENODE ExpandTreeNode(
            TREENODE parentNode, TREENODE startNode, int row, int column, ObjectStyle branchStyle, bool insertNewChild,
            out ExpansionOptions expansionOptions, out int itemIncr, out int subItemIncr)
        {
            bool requireInitialSubItemExpansion;
            subItemIncr = 0;
            var retVal = ExpandTreeNode(
                parentNode,
                startNode,
                row,
                column,
                branchStyle,
                insertNewChild,
                out expansionOptions,
                out itemIncr,
                out requireInitialSubItemExpansion);
            if (retVal != null && requireInitialSubItemExpansion)
            {
                try
                {
                    // UNDONE_MC: The full subitem gain will be higher than the immediate
                    // gain if the expanded items are also complex
                    ExpandInitialComplexSubItems(retVal as TREENODE_Multi, out subItemIncr);
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message, ex.StackTrace);
                    // Swallow this exception. The tree is left in a very bad state
                    // if we get this far into an expansion and then throw.
                }
                finally
                {
                    // These values may be positive already set if startnode is set,
                    // so increment instead of blindly setting to subItemIncr.
                    retVal.FullSubItemGain += subItemIncr;
                }
            }
            return retVal;
        }

        private TREENODE ExpandTreeNode(
            TREENODE parentNode, TREENODE startNode, int row, int column, ObjectStyle branchStyle, bool insertNewChild,
            out ExpansionOptions expansionOptions, out int itemIncr, out bool requireInitialSubItemExpansion)
        {
            TREENODE tnCur;
            TREENODE tn1;
            TREENODE tn2;
            TREENODE newChildNode = null;
            // Don't test expandable while loading a complex column, the request can be ambiguous.
            // Just go ahead and fire off the request. A null return will leave the cell as a simple item.
            Debug.Assert(
                branchStyle == ObjectStyle.SubItemRootBranch || parentNode.Branch.IsExpandable(row, column),
                "GetExpandable should be called before this is attempted");
            IBranch newBranch;
            try
            {
                SetStateFlag(TreeStateFlags.InExpansion, true);
                var options = 0;
                newBranch = parentNode.Branch.GetObject(row, column, branchStyle, ref options) as IBranch;
                expansionOptions = (ExpansionOptions)options;
            }
            finally
            {
                SetStateFlag(TreeStateFlags.InExpansion, false);
            }

            itemIncr = 0;
            requireInitialSubItemExpansion = false;
            newChildNode = null;
            if (newBranch != null)
            {
                itemIncr = newBranch.VisibleItemCount; //ExpansionCount can be 0 on success, don't check
                // UNDONE_MC: Only allow multi-column children if the parent branch does,
                // or if this is the root node of a complex item in the last column of a tree.
                var allowMultiColumn = true;
                tnCur = CreateTreeNode(startNode, newBranch, this, allowMultiColumn, parentNode.InSubItemColumn, parentNode.MultiColumn);
                tnCur.Index = row;
                tnCur.ImmedCount = tnCur.FullCount = itemIncr;
                if (allowMultiColumn
                    && tnCur.MultiColumn
                    && tnCur.ComplexColumns)
                {
                    requireInitialSubItemExpansion = true;
                }
                tnCur.Expanded = true;
                tnCur.AllowRecursion = (expansionOptions & ExpansionOptions.BlockRecursion) == 0;
                tnCur.UpdateDelayed = false;
                tnCur.Parent = parentNode;
                if (tnCur.Dynamic)
                {
                    AddTrackedNode(newBranch, tnCur);
                }
                newChildNode = tnCur;
                if (insertNewChild && startNode == null)
                {
                    //Place node in proper position in child chain based on index
                    tn1 = parentNode.FirstChild;
                    tn2 = null;
                    if (tn1 != null)
                    {
                        while (row >= tn1.Index)
                        {
                            Debug.Assert(tn1.Index != row, "TrackIndex screwed up");
                            tn2 = tn1;
                            if (null == (tn1 = tn2.NextSibling))
                            {
                                break;
                            }
                        }
                        if (tn2 != null)
                        {
                            tn2.NextSibling = tnCur;
                        }
                        else
                        {
                            parentNode.FirstChild = tnCur;
                        }
                        tnCur.NextSibling = tn1; //tn1 May be null, not worth the check
                    }
                    else
                    {
                        parentNode.FirstChild = tnCur;
                    }
                }
            }
            return newChildNode;
        }

        /// <summary>
        ///     Attempt to expand complex sub items in a multi-column branch. Called
        ///     immediately after a new node is created.
        /// </summary>
        /// <param name="tn">The node for the parent branch</param>
        /// <param name="totalIncrease">The cumulative subitem gain</param>
        private void ExpandInitialComplexSubItems(TREENODE_Multi tn, out int totalIncrease)
        {
            // The algorithm will walk sequentially through the branch and
            // create a new tree node for each row. We walk all columns
            // in a given row to make it easier to build the TREENODE list
            // attached to the parent.
            Debug.Assert(tn.FirstChild == null);
            Debug.Assert(tn.ComplexColumns); // Check before calling
            var mcBranch = tn.Branch as IMultiColumnBranch;
            var columnCount = (mcBranch == null) ? 1 : mcBranch.ColumnCount;
            totalIncrease = 0;
            if (columnCount > 1)
            {
                // Get an array with the complex columns
                int iCol;
                var complexColumnCount = 0;
                for (iCol = 1; iCol < columnCount; ++iCol)
                {
                    if (0 != (tn.SubItemStyle(iCol) & SubItemCellStyles.Complex))
                    {
                        ++complexColumnCount;
                    }
                }
                if (complexColumnCount > 0)
                {
                    var complexColumns = new int[complexColumnCount];
                    var jaggedColumns = tn.JaggedColumns;
                    var maxRow = tn.FullCount;
                    var maxColumn = columnCount - 1;
                    int iRow;
                    int itemIncr;
                    int subItemIncr;
                    TREENODE tnSubItemAnchor;
                    TREENODE tnSubItem;
                    TREENODE tnPrevHint;
                    SUBITEMNODE snPrev;
                    SUBITEMNODE sn;
                    complexColumnCount = 0;
                    ExpansionOptions options;
                    for (iCol = 1; iCol < columnCount; ++iCol)
                    {
                        if (0 != (tn.SubItemStyle(iCol) & SubItemCellStyles.Complex))
                        {
                            complexColumns[complexColumnCount] = iCol;
                            ++complexColumnCount;
                        }
                    }
                    // Walk the columns that matter and link up the returned tree nodes
                    tnPrevHint = null;
                    Debug.Assert(tn.FirstChild == null); // This is initial expansion only
                    for (iRow = 0; iRow < maxRow; ++iRow)
                    {
                        snPrev = null;
                        tnSubItemAnchor = null;
                        if (jaggedColumns)
                        {
                            maxColumn = mcBranch.GetJaggedColumnCount(iRow) - 1;
                        }
                        for (iCol = 0; iCol < complexColumnCount; ++iCol)
                        {
                            if (jaggedColumns && complexColumns[iCol] > maxColumn)
                            {
                                break;
                            }
                            options = 0;
                            tnSubItem = ExpandTreeNode(
                                tn,
                                null,
                                iRow,
                                complexColumns[iCol],
                                ObjectStyle.SubItemRootBranch,
                                false,
                                out options,
                                out itemIncr,
                                out subItemIncr);
                            if (tnSubItem != null)
                            {
                                if (snPrev == null)
                                {
                                    // First node for this row, go ahead and create a dummy node
                                    // TREENODE_Complex is the lowest common denominator
                                    // that supports multi column expansion. This node may
                                    // be upgraded later if an expansion is actually made
                                    // on this item.
                                    tnSubItemAnchor = new TREENODE_Complex();
                                    tnSubItemAnchor.Index = iRow;
                                    // UNDONE_MC: Might want to insert last so there are no lasting side effects in case of failure
                                    InsertIndexedNode(tn, tnSubItemAnchor, ref tnPrevHint);
                                }
                                // UNDONE_MC: Should we store the column in the blank index field of the root
                                // instead of in the SUBITEMNODE? This would save calculating the column with
                                // SUBITEMNODE.ColumnOfRootNode when we need it.
                                tnSubItem.SubItemRoot = true;
                                if (0 == (options & ExpansionOptions.UseAsSubItemExpansion))
                                {
                                    tnSubItem.ComplexSubItem = true;
                                }
                                tnSubItem.Index = VirtualTreeConstant.NullIndex;
                                tnSubItem.Parent = tnSubItemAnchor;
                                sn = new SUBITEMNODE();
                                sn.Column = complexColumns[iCol];
                                sn.RootNode = tnSubItem;
                                if (snPrev == null)
                                {
                                    sn.NextSibling = tnSubItemAnchor.FirstSubItem;
                                    tnSubItemAnchor.FirstSubItem = sn;
                                }
                                else
                                {
                                    sn.NextSibling = snPrev.NextSibling;
                                    snPrev.NextSibling = sn;
                                }
                                snPrev = sn;
                            }
                            if (tnSubItemAnchor != null)
                            {
                                var adjustment = tnSubItemAnchor.AdjustSubItemGain();
                                tnSubItemAnchor.FullSubItemGain += adjustment;
                                totalIncrease += adjustment;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Used by IMultiColumnTree.UpdateCellStyle implementations in derived classes
        /// </summary>
        /// <param name="branch">Target branch</param>
        /// <param name="row">Target row</param>
        /// <param name="column">Target column</param>
        /// <param name="makeComplex">Action to take</param>
        protected void UpdateCellStyle(IBranch branch, int row, int column, bool makeComplex)
        {
            var tnNext = LocateTrackedNode(branch);
            Debug.Assert(tnNext != null); //Expect LocateTrackedNode to throw otherwise
            var keyOffSubItemRoot = !makeComplex && row == -1 && column == 0;
            if (!keyOffSubItemRoot)
            {
                // Do an exception-inducing cast here. The branch must be a multi-column
                var mcBranch = (IMultiColumnBranch)branch;
                // UNDONE_MC: This is a branch-relative column value, which may not be the absolute
                // column in the grid. Need a good way of either calculating or storing the absolute
                // column for a given branch. The unused Index property on a SubItemRoot node is a good
                // candidate slot for this data, which would minimize recursion required to calculate this.
                if (row < 0
                    || row > tnNext.ImmedCount)
                {
                    throw new ArgumentOutOfRangeException("row");
                }
                var columnStyle = mcBranch.ColumnStyles(column);
                if (0 == (columnStyle & SubItemCellStyles.Complex))
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ComplexColumnCellStyleException));
                }
            }
            // UNDONE_MC: This is not correct for the nested complex scenario. Calculate
            // an accurate absolute column for each branch usage.
            var absColumn = column;
            TREENODE tnChild;
            TREENODE tnPrevChild;
            SUBITEMNODE sn;
            SUBITEMNODE snPrev;
            ExpansionOptions options;
            int itemIncr;
            int subItemIncr;
            TREENODE tnNewSubItem;
            TREENODE tnParent;
            while (tnNext != null)
            {
                tnParent = tnNext;
                if (keyOffSubItemRoot)
                {
                    if (!tnNext.ComplexSubItem)
                    {
                        tnNext = tnParent.NextNode;
                        continue;
                    }
                    // Set up all of the variables to be just like the keying off
                    // the parent branch.
                    tnPrevChild = null;
                    tnChild = tnNext.Parent;
                    column = tnChild.FirstSubItem.ColumnOfRootNode(tnNext);
                    sn = tnChild.SubItemAtColumn(column, out snPrev);
                    tnNext = tnChild.Parent;
                    row = tnChild.Index;
                }
                else
                {
                    tnChild = tnParent.GetChildNode(row, out tnPrevChild);
                    if (tnChild != null)
                    {
                        sn = tnChild.SubItemAtColumn(column, out snPrev);
                    }
                    else
                    {
                        sn = snPrev = null;
                    }
                }
                // We either need a subitem node and a move to not being complex, or not subitem
                // node and a move to complex. This condition satisfies both so we can combine some processing.
                if ((sn == null) == makeComplex)
                {
                    if (makeComplex)
                    {
                        tnNewSubItem = ExpandTreeNode(
                            tnParent,
                            null,
                            row,
                            column,
                            ObjectStyle.SubItemRootBranch,
                            false,
                            out options,
                            out itemIncr,
                            out subItemIncr);
                        if (tnNewSubItem == null)
                        {
                            tnNext = tnParent.NextNode;
                            continue;
                        }
                        if (tnChild == null)
                        {
                            // See comments in ExpandInitialComplexSubItems
                            tnChild = new TREENODE_Complex();
                            tnChild.Index = row;
                            InsertIndexedNode(tnParent, tnChild, ref tnPrevChild);
                        }
                        tnNewSubItem.SubItemRoot = true;
                        if (0 == (options & ExpansionOptions.UseAsSubItemExpansion))
                        {
                            tnNewSubItem.ComplexSubItem = true;
                        }
                        tnNewSubItem.Index = VirtualTreeConstant.NullIndex;
                        tnNewSubItem.Parent = tnChild;
                        sn = new SUBITEMNODE();
                        sn.Column = column;
                        sn.RootNode = tnNewSubItem;
                        if (snPrev == null)
                        {
                            sn.NextSibling = tnChild.FirstSubItem;
                            tnChild.FirstSubItem = sn;
                        }
                        else
                        {
                            sn.NextSibling = snPrev.NextSibling;
                            snPrev.NextSibling = sn;
                        }
                    }
                    else
                    {
                        // Delete the expansion
                        if (snPrev == null)
                        {
                            tnChild.FirstSubItem = sn.NextSibling;
                        }
                        else
                        {
                            snPrev.NextSibling = sn.NextSibling;
                        }
                        FreeRecursive(ref sn.RootNode);
                    }
                    subItemIncr = tnChild.AdjustSubItemGain();
                    if (subItemIncr != 0)
                    {
                        if (ItemCountChanged != null)
                        {
                            int dummySingleColumnAbsRow; // This action does not affect the single column view
                            var absRow = EnumAbsoluteIndices(row, ref tnNext, out dummySingleColumnAbsRow).Row;
                            if (absRow != VirtualTreeConstant.NullIndex)
                            {
                                SubItemColumnAdjustment[] subItemChanges;
                                var parentRowOffset = 0;
                                var affectedSubItemColumns = new AffectedSubItems(true);
                                var singleColumnSubItemAdjust = 0;
                                int rowIncr;
                                // We need to track all of this for expanded nodes to get the correct information for the event.
                                var adjustColumn = absColumn;

                                // TrackCell is currently broken for all values after absRow, but it will work down
                                // to the row we are currently expanding. We need this call to get the affectedSubItemColumns.
                                TrackCell(
                                    absRow, ref adjustColumn, ref parentRowOffset, ref affectedSubItemColumns, ref singleColumnSubItemAdjust);
                                Debug.Assert(adjustColumn == 0);
                                    // This should pick up the new list now in this cell, so the column will always be zero
                                ChangeFullCountRecursive(
                                    tnChild, 0, subItemIncr, null, ref affectedSubItemColumns, out rowIncr, out subItemChanges);
                                DelayTurnOffRedraw();
                                ItemCountChanged(
                                    this,
                                    new ItemCountChangedEventArgs(
                                        this, absRow, absColumn, rowIncr, absRow - 1, tnChild.ImmedSubItemGain, subItemChanges, true));
                                tnNext = tnParent.NextNode;
                                continue;
                            }
                        }
                        ChangeFullCountRecursive(tnChild, 0, subItemIncr);
                    }
                    else
                    {
                        // UNDONE: Nothing changed from a global structure perspective,
                        // the cell contents may have. Defer to DisplayDataChanged at this point
                    }
                }
                tnNext = tnParent.NextNode;
            }
            ClearPositionCache(); //Cached absolute information is toast.
        }

        //Return size of expansion
        private void ToggleExpansion(
            int absRow, int column, out bool allowRecursion, out int singleColumnSubItemAdjust, out int itemExpansionCount,
            out int subItemExpansionCount, out int rowChange, out int blanksAboveChange, out SubItemColumnAdjustment[] subItemChanges)
        {
            TREENODE tnCur;
            TREENODE tnSubItemAnchor;
            TREENODE tnUnexpandedSubItemAnchor;
            TREENODE tn1;
            TREENODE tn2;
            SUBITEMNODE sn;
            SUBITEMNODE snPrev;
            itemExpansionCount = subItemExpansionCount = 0;
            rowChange = 0;
            singleColumnSubItemAdjust = 0;
            subItemChanges = null;
            allowRecursion = true; //Default to true, too many implementations are missing this, and most are actually recursive
            var localColumn = column;
            var parentRowOffset = 0;
            var affectedColumns = new AffectedSubItems(true);
            var pos = TrackCell(absRow, ref localColumn, ref parentRowOffset, ref affectedColumns, ref singleColumnSubItemAdjust);
            var tnRecurseOn = pos.ParentNode;
            var subItemExpansion = localColumn != 0;
            blanksAboveChange = 0;
            tnCur = pos.ParentNode.GetChildNode(pos.Index);
            tnSubItemAnchor = null;
            tnUnexpandedSubItemAnchor = null;
            snPrev = null;
            if (subItemExpansion)
            {
                Debug.Assert(pos.SubItemOffset == 0); // TrackIndex messed up
                tnSubItemAnchor = tnCur;
                if (tnCur != null)
                {
                    sn = tnCur.SubItemAtColumn(localColumn, out snPrev);
                    tnCur = (sn != null) ? sn.RootNode : null;
                }
            }
            else if (tnCur != null
                     && tnCur.Branch == null)
            {
                tnUnexpandedSubItemAnchor = tnCur;
                tnCur = null;
            }
            if (tnCur != null)
            {
                blanksAboveChange = tnCur.ImmedSubItemGain;
                if (tnCur.Expanded)
                {
                    itemExpansionCount = -tnCur.FullCount;
                    subItemExpansionCount = -tnCur.ExpandedSubItemGain;
                    switch (tnCur.CloseAction)
                    {
                        case BranchCollapseAction.CloseAndDiscard:
                            tn1 = pos.ParentNode.FirstChild;
                            Debug.Assert(tn1 != null, "");
                            tn2 = null;
                            while (tn1 != tnCur)
                            {
                                //ptnCur is a child of the parent node, should never miss it completely
                                Debug.Assert(tn1 != null, "");
                                tn2 = tn1;
                                tn1 = tn1.NextSibling;
                            }
                            if (tn2 != null)
                            {
                                tn2.NextSibling = tn1.NextSibling;
                            }
                            else
                            {
                                pos.ParentNode.FirstChild = tn1.NextSibling;
                            }
                            FreeRecursive(ref tnCur);
                            break;

                        case BranchCollapseAction.CloseChildren:
                            tn1 = tnCur.FirstChild;
                            while (tn1 != null)
                            {
                                tn2 = tn1.NextSibling;
                                FreeRecursive(ref tn1);
                                tn1 = tn2;
                            }
                            tn1 = tn2 = null;
                            tnCur.FirstChild = null;
                            tnCur.FullCount = tnCur.ImmedCount;
                            goto case BranchCollapseAction.Nothing;
                        case BranchCollapseAction.Nothing:
                            tnCur.Expanded = false;
                            break;
                        default:
                            Debug.Assert(false, "Bogus BranchCollapseAction");
                            break;
                    }
                }
                else
                {
                    tnCur.Expanded = true;
                    allowRecursion = tnCur.AllowRecursion;
                    itemExpansionCount = tnCur.FullCount;
                    subItemExpansionCount = tnCur.ExpandedSubItemGain;
                }
            }
            else
            {
                tnCur = ExpandTreeNode(
                    pos.ParentNode,
                    tnUnexpandedSubItemAnchor,
                    pos.Index,
                    localColumn,
                    !subItemExpansion,
                    out itemExpansionCount,
                    out subItemExpansionCount);

                // Make sure we don't crash if the branch returns null. If this happens,
                // the branch should stop returning true for IsExpandable and initiate
                // an DisplayDataChanged event to update the expansion bitmap.
                if (tnCur == null)
                {
                    allowRecursion = false;
                    return;
                }

                allowRecursion = tnCur.AllowRecursion;

                // We need to insert the node ourselves
                if (subItemExpansion)
                {
                    if (tnSubItemAnchor == null)
                    {
                        // TREENODE_Complex is the lowest common denominator
                        // that supports multi column expansion. This node may
                        // be upgraded later if an expansion is actually made
                        // on this item.
                        tnSubItemAnchor = new TREENODE_Complex();
                        tnSubItemAnchor.Index = pos.Index;
                        TREENODE tnDummy = null;
                        // UNDONE_MC: Might want to insert last so there are no lasting side effects in case of failure
                        InsertIndexedNode(pos.ParentNode, tnSubItemAnchor, ref tnDummy);
                    }
                    tnCur.SubItemRoot = true;
                    // UNDONE_MC: Should we store the column in the blank index field of the root
                    // instead of in the SUBITEMNODE? This would save calculating the column with
                    // SUBITEMNODE.ColumnOfRootNode when we need it.
                    tnCur.Index = VirtualTreeConstant.NullIndex;
                    tnCur.Parent = tnSubItemAnchor;
                    sn = new SUBITEMNODE();
                    sn.Column = localColumn;
                    sn.RootNode = tnCur;
                    if (snPrev == null)
                    {
                        sn.NextSibling = tnSubItemAnchor.FirstSubItem;
                        tnSubItemAnchor.FirstSubItem = sn;
                    }
                    else
                    {
                        sn.NextSibling = snPrev.NextSibling;
                        snPrev.NextSibling = sn;
                    }
                }
            }
            if ((itemExpansionCount + subItemExpansionCount) != 0)
            {
                if (subItemExpansion)
                {
                    // An item increment will be picked up by the first pass in
                    // ChangeFullCountRecursive and passed back up the chain. The
                    // value is calculated in the call to AdjustSubItemGain, which is
                    // why we do not call that function here. However, ChangeFullCountRecursive
                    // also adjusts the following two values, so we compensate here.
                    Debug.Assert(tnCur.FullCount == Math.Abs(itemExpansionCount));
                    Debug.Assert(tnCur.FullSubItemGain == Math.Abs(subItemExpansionCount));
                    tnCur.FullCount -= itemExpansionCount;
                    tnCur.FullSubItemGain -= subItemExpansionCount;
                    tnRecurseOn = tnCur;
                }
                ChangeFullCountRecursive(
                    tnRecurseOn,
                    itemExpansionCount,
                    subItemExpansionCount,
                    null,
                    ref affectedColumns,
                    out rowChange,
                    out subItemChanges);
            }
        }

        // An advanced version of this routine to deal with multi column scenarios
        private void ChangeFullCountRecursive(
            TREENODE tn, int itemIncr, int subItemIncr, TREENODE tnCeiling, ref AffectedSubItems affectedColumns, out int rowIncr,
            out SubItemColumnAdjustment[] subItemChanges)
        {
            Debug.Assert(tn != null, "");
            if (MultiColumnSupport)
            {
                var affectedColumnsCount = affectedColumns.Count;
                subItemChanges = (affectedColumnsCount == 0) ? null : new SubItemColumnAdjustment[affectedColumnsCount];
                rowIncr = 0;
                while (tn != tnCeiling)
                {
                    tn.FullCount += itemIncr;
                    tn.FullSubItemGain += subItemIncr;
                    if (tn.Expanded
                        || tn.SubItemRoot)
                    {
                        if (tn.SubItemRoot)
                        {
                            // A subitem root node always has a parent node in the owning branch
                            var subItemRoot = tn.Parent;
                            var totalIncr = itemIncr + subItemIncr;
                            Debug.Assert(subItemRoot != null);
                            itemIncr = 0;
                            // Recalculate, see if this has affected the parent total
                            subItemIncr = subItemRoot.AdjustSubItemGain();
                            if (affectedColumnsCount != 0)
                            {
                                --affectedColumnsCount;
                                // Figure out the trailing items, which is the number of rows wholly
                                // contained in the column that did not change.

                                // First, get the number of rows past the current change
                                var offset = affectedColumns.GetOffset(affectedColumnsCount);
                                var trailingItems = tn.TotalCount - offset - 1;
                                var itemsBelowAnchor = subItemRoot.ImmedSubItemGain - offset - subItemIncr;
                                if (!tn.ComplexSubItem)
                                {
                                    ++trailingItems;
                                }
                                if (totalIncr > 0)
                                {
                                    trailingItems -= totalIncr;
                                }

                                // Now, adjust by the number of rows that have moved into/out of being
                                // wholly contained by this column
                                if (subItemIncr > 0)
                                {
                                    trailingItems -= subItemIncr;
                                }
                                else
                                {
                                    trailingItems += subItemIncr;
                                }
                                if (trailingItems < 0)
                                {
                                    trailingItems = 0;
                                }

                                // UNDONE_MC: Need a global column for the first two parameters here
                                subItemChanges[affectedColumnsCount] = new SubItemColumnAdjustment(
                                    subItemRoot.FirstSubItem.ColumnOfRootNode(tn),
                                    subItemRoot.Parent.GetColumnCount(subItemRoot.Index) - 1,
                                    totalIncr,
                                    trailingItems,
                                    itemsBelowAnchor);
                            }
                            if (subItemIncr == 0)
                            {
                                break;
                            }
                            subItemRoot.FullSubItemGain += subItemIncr;
                            tn = subItemRoot;
                        }
                        tn = tn.Parent;
                        if (tn == null)
                        {
                            rowIncr = itemIncr + subItemIncr;
                            break;
                        }
                    }
                    else if (subItemIncr != 0)
                    {
                        tn = tn.Parent;
                        if (tn != null
                            &&
                            (tn.Expanded || tn.SubItemRoot))
                        {
                            itemIncr = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                var tnNext = tn;
                do
                {
                    tn = tnNext;
                    if (tn == tnCeiling)
                    {
                        break;
                    }
                    tn.FullCount += itemIncr;
                }
                while (tn.Expanded
                       && ((tnNext = tn.Parent) != null));
                subItemChanges = null;
                rowIncr = (tn != null && tn.Expanded) ? itemIncr : 0;
            }
        }

        private void ChangeFullCountRecursive(TREENODE tn, int itemIncr, int subItemIncr)
        {
            Debug.Assert(tn != null, "");
            if (MultiColumnSupport)
            {
                for (;;)
                {
                    tn.FullCount += itemIncr;
                    tn.FullSubItemGain += subItemIncr;
                    if (tn.Expanded
                        || tn.SubItemRoot)
                    {
                        if (tn.SubItemRoot)
                        {
                            // A subitem root node always has a parent node in the owning branch
                            Debug.Assert(tn.Parent != null);
                            tn = tn.Parent;
                            itemIncr = 0;
                            // Recalculate, see if this has affected the parent total
                            subItemIncr = tn.AdjustSubItemGain();
                            if (subItemIncr == 0)
                            {
                                break;
                            }
                            tn.FullSubItemGain += subItemIncr;
                        }
                        tn = tn.Parent;
                        if (tn == null)
                        {
                            break;
                        }
                    }
                    else if (subItemIncr != 0)
                    {
                        tn = tn.Parent;
                        if (tn != null
                            &&
                            (tn.Expanded || tn.SubItemRoot))
                        {
                            itemIncr = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                } //for(;;)
            }
            else
            {
                do
                {
                    tn.FullCount += itemIncr;
                }
                while (tn.Expanded
                       && ((tn = tn.Parent) != null));
            }
        }

        // Modify the count up to, but not including, the ceiling node
        private void ChangeFullCountRecursive(TREENODE tn, int itemIncr, int subItemIncr, TREENODE tnCeiling)
        {
            int subItemRemainder;
            ChangeFullCountRecursive(tn, itemIncr, subItemIncr, tnCeiling, out subItemRemainder);
        }

        private void ChangeFullCountRecursive(TREENODE tn, int itemIncr, int subItemIncr, TREENODE tnCeiling, out int subItemRemainder)
        {
            Debug.Assert(tn != null, "");
            if (MultiColumnSupport)
            {
                while (tn != tnCeiling)
                {
                    tn.FullCount += itemIncr;
                    tn.FullSubItemGain += subItemIncr;
                    if (tn.Expanded
                        || tn.SubItemRoot)
                    {
                        if (tn.SubItemRoot)
                        {
                            // A subitem root node always has a parent node in the owning branch
                            Debug.Assert(tn.Parent != null);
                            tn = tn.Parent;
                            itemIncr = 0;
                            // Recalculate, see if this has affected the parent total
                            subItemIncr = tn.AdjustSubItemGain();
                            if (subItemIncr == 0)
                            {
                                break;
                            }
                            tn.FullSubItemGain += subItemIncr;
                        }
                        tn = tn.Parent;
                        if (tn == null)
                        {
                            break;
                        }
                    }
                    else if (subItemIncr != 0)
                    {
                        tn = tn.Parent;
                        if (tn != null
                            &&
                            (tn.Expanded || tn.SubItemRoot))
                        {
                            itemIncr = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                do
                {
                    if (tn == tnCeiling)
                    {
                        break;
                    }
                    tn.FullCount += itemIncr;
                }
                while (tn.Expanded
                       && ((tn = tn.Parent) != null));
            }
            subItemRemainder = subItemIncr;
        }

        VirtualTreeCoordinate ITree.LocateObject(IBranch startingBranch, object target, int locateStyle, int locateOptions)
        {
            return LocateObject(startingBranch, target, locateStyle, locateOptions);
        }

        /// <summary>
        ///     Locate the given object in the tree, expanding branches as needed.
        /// </summary>
        /// <param name="startingBranch">The branch to search, or null for the root branch</param>
        /// <param name="target">The target object to locate</param>
        /// <param name="locateStyle">
        ///     The style to send to IBranch.LocateObject.
        ///     Generally ObjectStyle.TrackableObject, but values at or above ObjectStyle.FirstUserStyle may also be used.
        ///     Regardless of the style used, IBranch.LocateObject should return values from the TrackingObjectAction enum.
        /// </param>
        /// <param name="locateOptions">User-definable options passed to each call to IBranch.LocateObject</param>
        /// <returns>
        ///     A VirtualTreeCoordinate structure with the correct coordinates. The IsValid property of
        ///     the returned structure will be false if the object could not be located.
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected VirtualTreeCoordinate LocateObject(IBranch startingBranch, object target, int locateStyle, int locateOptions)
        {
            // Object location is done in two steps:
            // 1) Track the object below the starting node.
            // 2) Find the offset from the starting node to the child object
            // 3) If the downward search succeeded, then make sure that each of
            //    the branches that lead to the starting node are expanded.
            var targetRow = -1;
            var targetColumn = 0;
            var tn = (startingBranch == null) ? myRootNode : LocateTrackedNode(startingBranch);
            if (tn == null)
            {
                return VirtualTreeCoordinate.Invalid;
            }
            Debug.Assert(tn.NextNode == null); // Branch is in multiple places in the tree. The one we get is undefined. Benign assert.
            var tnStartNode = tn;
            TREENODE tnFirstUnexpanded = null;
            TREENODE tnAttachCache;
            TREENODE tnNext;
            TrackingObjectAction action;
            var firstUnexpandedRow = -1;
            var firstUnexpandedRowSubItemAdjust = 0;
            var firstUnexpandedColumnOffset = 0;
            var totalRowOffset = 0;
            var totalColumnOffset = 0;
            var totalRowOffsetSubItemAdjust = 0;
            int row;
            int rowOffset;
            int rowOffsetSubItemAdjust;
            int column;
            var retVal = false;
            int changeCount;
            int subItemChangeCount;
            int subItemRemainder;
            var totalChangeCount = 0;
            var totalSubItemChangeCount = 0;
            var inSubItemTerritory = false;
            var partialSuccess = false;
            while (tn != null)
            {
                var locateData = tn.Branch.LocateObject(target, (ObjectStyle)locateStyle, locateOptions);
                row = locateData.Row;
                column = locateData.Column;
                action = (TrackingObjectAction)locateData.Options;
                switch (action)
                {
                    case TrackingObjectAction.NotTracked:
                        // Nothing more we can do, just get out.
                        tn = null;
                        break;
                    case TrackingObjectAction.NotTrackedReturnParent:
                        if (partialSuccess)
                        {
                            retVal = true;
                        }
                        tn = null;
                        break;
                    case TrackingObjectAction.ThisLevel:
                    case TrackingObjectAction.NextLevel:
                        partialSuccess = true;
                        rowOffset = tn.GetChildOffset(row, out rowOffsetSubItemAdjust);
                        if (tn.ComplexSubItem)
                        {
                            --rowOffset;
                        }
                        totalRowOffset += rowOffset;
                        totalRowOffsetSubItemAdjust += rowOffsetSubItemAdjust;
                        totalColumnOffset += column;
                        if (!tn.Expanded)
                        {
                            changeCount = tn.FullCount;
                            subItemChangeCount = tn.ExpandedSubItemGain;

                            // Adjust for subsequent ChangeFullCountRecursive call. We
                            // want to make this call on the current node so that any
                            // subitem expansions are picked up and edit correctly, but
                            // we need to avoid the side effects on the current node.
                            tn.FullCount -= changeCount;
                            tn.FullSubItemGain -= subItemChangeCount;
                            if (tnFirstUnexpanded == null)
                            {
                                tnFirstUnexpanded = tn;
                            }
                            Debug.Assert(tn.Branch != null);
                            tn.Expanded = true;
                            ChangeFullCountRecursive(tn, changeCount, subItemChangeCount, tnFirstUnexpanded, out subItemRemainder);
                            totalSubItemChangeCount += subItemRemainder;
                            if (!inSubItemTerritory)
                            {
                                totalChangeCount += changeCount;
                            }
                        }
                        if (tnFirstUnexpanded == null)
                        {
                            firstUnexpandedColumnOffset += column;
                            firstUnexpandedRow += rowOffset;
                            firstUnexpandedRowSubItemAdjust += rowOffsetSubItemAdjust;
                        }
                        if (action == TrackingObjectAction.ThisLevel)
                        {
                            retVal = true;
                            tn = null;
                        }
                        else
                        {
                            // Keep going down the tree
                            tnAttachCache = null;
                            tnNext = FindIndexedNode(row, tn.FirstChild, ref tnAttachCache);

                            // If we have a node with a branch, then it will be expanded if needed
                            // in the next pass through the main loop. We only do expansion here if
                            // that is not the case, meaning that the branch has never been expanded.
                            if (column > 0)
                            {
                                // We're being directed down a subitem column. In this case,
                                // tnNext is a subitem anchor node.
                                if (column < tn.GetColumnCount(row))
                                {
                                    var columnStyle = tn.SubItemStyle(column);
                                    if (0 != (tn.SubItemStyle(column) & SubItemCellStyles.Mixed))
                                    {
                                        SUBITEMNODE snPrev = null;
                                        var sn = (tnNext != null) ? tnNext.SubItemAtColumn(column, out snPrev) : null;
                                        if (sn == null)
                                        {
                                            if (0 != (columnStyle & SubItemCellStyles.Expandable))
                                            {
                                                // We only handle null for expandable cells.
                                                // A complex cell would already have been expanded
                                                // when the branch initially loaded.
                                                ExpansionOptions expandOptions;
                                                var tnSubItem = ExpandTreeNode(
                                                    tn,
                                                    null,
                                                    row,
                                                    column,
                                                    ObjectStyle.SubItemExpansion,
                                                    false,
                                                    out expandOptions,
                                                    out changeCount,
                                                    out subItemChangeCount);
                                                if (tnSubItem != null)
                                                {
                                                    if (tnNext == null)
                                                    {
                                                        // Create a new anchor
                                                        tnNext = new TREENODE_Complex();
                                                        tnNext.Index = row;
                                                        InsertIndexedNode(tn, tnNext, ref tnAttachCache);
                                                    }
                                                    tnSubItem.SubItemRoot = true;
                                                    // UNDONE: Would be nice to store a global column in Index
                                                    tnSubItem.Index = VirtualTreeConstant.NullIndex;
                                                    tnSubItem.Parent = tnNext;
                                                    sn = new SUBITEMNODE();
                                                    sn.Column = column;
                                                    sn.RootNode = tnSubItem;
                                                    if (snPrev == null)
                                                    {
                                                        sn.NextSibling = tnNext.FirstSubItem;
                                                        tnNext.FirstSubItem = sn;
                                                    }
                                                    else
                                                    {
                                                        sn.NextSibling = snPrev.NextSibling;
                                                        snPrev.NextSibling = sn;
                                                    }

                                                    // Adjust for ChangeFullCountRecursive call
                                                    tnSubItem.FullCount -= changeCount;
                                                    tnSubItem.FullSubItemGain -= subItemChangeCount;
                                                    if (tnFirstUnexpanded == null)
                                                    {
                                                        tnFirstUnexpanded = tnSubItem;
                                                        totalChangeCount += changeCount;
                                                        totalSubItemChangeCount += subItemChangeCount;
                                                    }
                                                    else
                                                    {
                                                        inSubItemTerritory = true;
                                                        ChangeFullCountRecursive(
                                                            tnSubItem, changeCount, subItemChangeCount, tnFirstUnexpanded,
                                                            out subItemRemainder);
                                                        totalSubItemChangeCount += subItemRemainder;
                                                    }
                                                    tnNext = tnSubItem;
                                                }
                                                else
                                                {
                                                    // Break the outer loop
                                                    tnNext = null;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tnNext = sn.RootNode;
                                            inSubItemTerritory = tnFirstUnexpanded != null;
                                        }
                                    }
                                }
                            }
                            else if (tnNext == null
                                     || tnNext.Branch == null)
                            {
                                var insertNode = tnNext == null;
                                tnNext = ExpandTreeNode(tn, tnNext, row, 0, false, out changeCount, out subItemChangeCount);
                                if (insertNode)
                                {
                                    // If the branch was set coming in, then the node already existed
                                    // for a subitem expansion and is already in the list, so we
                                    // only insert if we created a node for this position.
                                    InsertIndexedNode(tn, tnNext, ref tnAttachCache);
                                }

                                // Adjust back to prepare for the ChangeFullCountRecursive loop that will happen
                                // eventually. If this is the first expanded, then the one below will do nothing.
                                if (tnFirstUnexpanded == null)
                                {
                                    tnFirstUnexpanded = tnNext;
                                    tnNext.FullSubItemGain -= subItemChangeCount;
                                    tnNext.FullCount -= changeCount;
                                    totalChangeCount += changeCount;
                                    totalSubItemChangeCount += subItemChangeCount;
                                }
                                else
                                {
                                    ChangeFullCountRecursive(tn, changeCount, subItemChangeCount, tnFirstUnexpanded, out subItemRemainder);
                                    totalSubItemChangeCount += subItemRemainder;
                                    if (!inSubItemTerritory)
                                    {
                                        totalChangeCount += changeCount;
                                    }
                                }
                            }
                            tn = tnNext;
                        }
                        break;
                }
            }

            // Now, walk back up the chain to ensure that all parent nodes are expanded
            // We've only tracked information from the first unexpanded node, not the
            // one we first found, so we have to walk back up from the point our information
            // is current at. Track the offset so we have a global position if we make it
            // to the top without hitting an unexpanded item.
            tn = (tnFirstUnexpanded == null) ? tnStartNode : tnFirstUnexpanded;
            var backedUpPastStartNode = false;
            while (tn != null)
            {
                if (!backedUpPastStartNode
                    && tn == tnStartNode)
                {
                    backedUpPastStartNode = true;
                }
                tnNext = tn.Parent;
                if (!tn.Expanded)
                {
                    if (!retVal)
                    {
                        // We can't find the object we're looking for, and any expansion we've made isn't
                        // visible. Notify up to this point and get out of the routine. There is no
                        // change to the visible state, so eventing is not needed.
                        if (tnFirstUnexpanded != null)
                        {
                            ChangeFullCountRecursive(tnFirstUnexpanded, totalChangeCount, totalSubItemChangeCount);
                        }
                        return VirtualTreeCoordinate.Invalid;
                    }

                    // Recurse up to (but not including) the current node. For this reason, we
                    // keep adding onto the old totals.
                    subItemRemainder = 0;
                    if (tnFirstUnexpanded != null)
                    {
                        ChangeFullCountRecursive(tnFirstUnexpanded, totalChangeCount, totalSubItemChangeCount, tnNext, out subItemRemainder);
                    }
                    tnFirstUnexpanded = tnNext;
                    firstUnexpandedColumnOffset = 0;
                    firstUnexpandedRow = 0; // Set below
                    firstUnexpandedRowSubItemAdjust = 0; // UNDONE_SINGLECOLUMN: Verify
                    totalChangeCount += tnNext.FullCount;
                    totalSubItemChangeCount = tnNext.FullSubItemGain + subItemRemainder;
                    tnNext.FullCount = 0;
                    tnNext.FullSubItemGain = 0;
                }
                tnNext = tn.Parent;
                if (tn.SubItemRoot)
                {
                    if (backedUpPastStartNode)
                    {
                        column = tnNext.FirstSubItem.ColumnOfRootNode(tn);
                        if (tnFirstUnexpanded != null)
                        {
                            firstUnexpandedColumnOffset += column;
                        }
                        else
                        {
                            totalSubItemChangeCount += totalChangeCount;
                            totalChangeCount = 0;
                        }
                        totalColumnOffset += column;
                        if (!tn.ComplexSubItem)
                        {
                            ++totalRowOffset;
                            ++firstUnexpandedRow;
                        }
                    }

                    // Skip the subitem anchor
                    tnNext = tnNext.Parent;
                }
                else if (backedUpPastStartNode && tnNext != null)
                {
                    rowOffset = tnNext.GetChildOffset(tn.Index, out rowOffsetSubItemAdjust);
                    firstUnexpandedRow += rowOffset;
                    firstUnexpandedRowSubItemAdjust += rowOffsetSubItemAdjust;
                    totalRowOffset += rowOffset;
                    totalRowOffsetSubItemAdjust += rowOffsetSubItemAdjust;
                }
                tn = tnNext;
            }

            // Recurse the parent even if retVal is false, there may have been some
            // changes that partially expanded the tree and need to be accounted for.
            if ((totalChangeCount + totalSubItemChangeCount) > 0)
            {
                Debug.Assert(tnFirstUnexpanded != null); // The count can't change if nothing expanded
                // There is a lot of extra work to do if we have to fire redraw events
                if (ItemCountChanged != null)
                {
                    Debug.Assert(tnFirstUnexpanded.Parent != null); // The root node is always expanded
                    SubItemColumnAdjustment[] subItemChanges;
                    var parentRowOffset = 0;
                    var affectedSubItemColumns = new AffectedSubItems(true);
                    int rowIncr;
                    var adjustColumn = firstUnexpandedColumnOffset;
                    var singleColumnSubItemAdjust = 0;
                    // TrackCell is currently broken for all values after absRow, but it will work down
                    // to the row we are currently expanding. We need this call to get the affectedSubItemColumns.
                    // UNDONE: We should be able to calculate affectedSubItemColumns during the course of this
                    // routine instead of rewalking the tree here.
                    TrackCell(
                        firstUnexpandedRow, ref adjustColumn, ref parentRowOffset, ref affectedSubItemColumns, ref singleColumnSubItemAdjust);
                    ChangeFullCountRecursive(
                        tnFirstUnexpanded, totalChangeCount, totalSubItemChangeCount, null, ref affectedSubItemColumns, out rowIncr,
                        out subItemChanges);
                    DelayTurnOffRedraw();
                    // UNDONE: Verify tnFirstUnexpanded.ImmedSubItemGain passed here
                    ItemCountChanged(
                        this,
                        new ItemCountChangedEventArgs(
                            this, firstUnexpandedRow, firstUnexpandedColumnOffset, rowIncr, firstUnexpandedRow - 1,
                            tnFirstUnexpanded.ImmedSubItemGain, subItemChanges, true));
                }
                else
                {
                    ChangeFullCountRecursive(tnFirstUnexpanded, totalChangeCount, totalSubItemChangeCount);
                }
                if (firstUnexpandedColumnOffset == 0
                    && GetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged))
                {
                    DelayTurnOffSingleColumnRedraw();
                    var singleTree = SingleColumnTree;
                    (singleTree as SingleColumnView).myItemCountChanged(
                        singleTree,
                        new ItemCountChangedEventArgs(
                            singleTree, firstUnexpandedRow - firstUnexpandedRowSubItemAdjust, 0, totalChangeCount,
                            firstUnexpandedRow - firstUnexpandedRowSubItemAdjust - 1, 0, null, true));
                }
            }

            if (retVal)
            {
                targetRow += totalRowOffset;
                targetColumn += totalColumnOffset;
            }
            return retVal ? new VirtualTreeCoordinate(targetRow, targetColumn) : VirtualTreeCoordinate.Invalid;
        }

        void ITree.ShiftBranchLevels(ShiftBranchLevelsData shiftData)
        {
            ShiftBranchLevels(shiftData);
        }

        /// <summary>
        ///     Add or remove entire levels from existing branch structures in the tree
        /// </summary>
        /// <param name="shiftData">The data for the shift operation</param>
        protected void ShiftBranchLevels(ShiftBranchLevelsData shiftData)
        {
            ShiftBranchLevels(
                shiftData.Branch, shiftData.RemoveLevels, shiftData.InsertLevels, shiftData.Depth, shiftData.ReplacementBranch,
                shiftData.BranchTester, shiftData.StartIndex, shiftData.Count, shiftData.NewCount);
        }

        private void ShiftBranchLevels(
            IBranch branch, int removeLevels, int insertLevels, int depth, IBranch replacementBranch, ILevelShiftAdjuster branchTester,
            int startIndex, int count, int newCount)
        {
            //UNDONE: Do some argument validation
            if (removeLevels < 0)
            {
                throw new ArgumentOutOfRangeException("removeLevels");
            }
            if (insertLevels < 0)
            {
                throw new ArgumentOutOfRangeException("insertLevels");
            }
            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException("depth");
            }
            if ((insertLevels + removeLevels) == 0)
            {
                throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ShiftBranchLevelsExceptionDesc));
            }

            var tnFirst = LocateTrackedNode(branch);
            var tn = tnFirst;
            Debug.Assert(tn != null); //Expect LocateTrackedNode to throw otherwise
            if (startIndex != -1)
            {
                // Use the original count if newCount not specified
                if (newCount == -1)
                {
                    newCount = count;
                }

                // Do a quick sanity check to make sure that the current branch is consistent
                // with the past in properties. This is called after the branch is modified
                // to reflect th new settings.
                var testBranch = (replacementBranch == null) ? branch : replacementBranch;
                if (testBranch.VisibleItemCount != (tn.ImmedCount + newCount - count))
                {
                    throw new ArgumentException(string.Empty, "newCount"); //UNDONE: EXCEPTION
                }
            }
            int startFullCount;
            int startExpandedSubItemGain;
            // Make sure we are officially shuffling a list before the replacement
            // branch is blasted into the structure.
            ListShuffle = true;
            var reattachTracker = false;
            try
            {
                // Take care of the replacementBranch up front
                if (replacementBranch != null)
                {
                    //UNDONE: Cache a delegate for this.OnBranchModification
                    branch.OnBranchModification -= OnBranchModification;
                    replacementBranch.OnBranchModification += OnBranchModification;
                    var newFlags = replacementBranch.Features;
                    while (tn != null)
                    {
                        tn.Branch = replacementBranch;
                        tn.SetFlags(newFlags);
                        tn.Dynamic = true; // Set regardless
                        tn = tn.NextNode;
                    }
                    Debug.Assert(tnFirst.Dynamic); // LocateTrackedNode wouldn't have worked without this
                    // Get the branch out of the tracked list altogether. Readding the list of
                    // nodes with the replacement branch is deferred until all the work is done because
                    // it is possible to replace with an existing branch, or expand the existing branch
                    // in a different location while reattaching tree nodes. As a simple example, the top
                    // level branch can be eliminated by calling ShiftBranchLevels(branch, 2, 1, replacement)
                    // where the replacement a child list of the parent (useful if parent has 1 item in it).
                    myNodeTracker.RemoveBranch(branch);
                    reattachTracker = true;
                    tn = tnFirst;
                }

                while (tn != null)
                {
                    startFullCount = tn.FullCount;
                    startExpandedSubItemGain = tn.ExpandedSubItemGain;
                    RealignTreeNodeLevelShift(tn, removeLevels, insertLevels, depth, branchTester, startIndex, count, newCount, true);
                    DoRealignNotification(tn, startFullCount, startExpandedSubItemGain);
                    tn = tn.NextNode;
                }
            }
            finally
            {
                if (reattachTracker)
                {
                    myNodeTracker.Add(replacementBranch, tnFirst);
                }
                ClearPositionCache(); //Cached absolute information is toast.
                ListShuffle = false;
            }
        }

        // removeLevels = number of levels to remove (removal done first)
        // insertLevels = number of levels to insert
        // depth = level offset from calling branch to operate at
        // start = beginning of range to consider (affects top level branch only, this and subsequent parameters ignored if -1)
        // count = number of items affected
        // newcount = use this to calculate new total and realign inside range only (-1 to keep same item count, applies to top level only)
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void RealignTreeNodeLevelShift(
            TREENODE tn, int removeLevels, int insertLevels, int depth, ILevelShiftAdjuster branchTester, int start, int count, int newCount,
            bool firstLevel)
        {
            int maxIndex; //The highest index we care about, plus one

            // See if a new branch should be retrieved. The new branch at the
            // top level is passed in directly (required to support modifying
            // the root node). This enables the caller to provide new objects
            // for existing items instead of modifying existing ones.
            if (!firstLevel
                && branchTester != null)
            {
                if (branchTester.TestGetNewBranch(tn.Branch))
                {
                    var options = 0;
                    IBranch newBranch;
                    var wasDynamic = tn.Dynamic;
                    if (tn.SubItemRoot)
                    {
                        var tnParent = tn.Parent;
                        newBranch = tnParent.Parent.Branch.GetObject(
                            tnParent.Index,
                            tnParent.FirstSubItem.ColumnOfRootNode(tn),
                            tn.ComplexSubItem ? ObjectStyle.SubItemRootBranch : ObjectStyle.SubItemExpansion,
                            ref options) as IBranch;
                        // UNDONE_MC: Should we respect an option switch from SubItemRootBranch to SubItemExpansion here, or respect
                        // a SubItemCellStyles.Mixed setting on the parent branch and reattempt a SubItemRootBranch even if we
                        // currently have an expansion? These are both advanced scenarios. The code here will change the branch,
                        // but will not switch it between an expansion and a root branch, or vice versa.
                    }
                    else
                    {
                        newBranch = tn.Parent.Branch.GetObject(
                            tn.Index,
                            0,
                            ObjectStyle.ExpandedBranch,
                            ref options) as IBranch;
                    }
                    tn.SetFlags(newBranch.Features);
                    tn.Dynamic = wasDynamic; // Don't allow this one to change

                    Debug.Assert(newBranch != null);
                    if (tn.Dynamic)
                    {
                        myNodeTracker.ReBranchTreeNode(newBranch, tn);
                    }
                    else
                    {
                        tn.Branch = newBranch;
                    }
                }
            }
            if (start == -1)
            {
                count = tn.ImmedCount;
                maxIndex = count;
                newCount = tn.Branch.VisibleItemCount;
            }
            else
            {
                maxIndex = start + count;
            }

            // Get the first child node and adjust it past the
            // requested starting position.
            var tnChild = tn.FirstChild;
            TREENODE tnLastUntouched = null;
            if (start != -1)
            {
                while (tnChild != null)
                {
                    if (tnChild.Index >= start)
                    {
                        break;
                    }
                    tnLastUntouched = tnChild;
                    tnChild = tnChild.NextSibling;
                }
            }

            if (depth > 0)
            {
                // Walk all of the child nodes that are in range
                var depthAdjustment = 0;
                var continueDownBranch = true;
                while (tnChild != null)
                {
                    if (tnChild.Index >= maxIndex)
                    {
                        break;
                    }

                    if (branchTester != null)
                    {
                        var result = branchTester.ValidateAdjustDepth(tnChild.Branch);
                        continueDownBranch = result.Continue;
                        depthAdjustment = result.DepthAdjustment;
                    }

                    if (continueDownBranch)
                    {
                        // Recurse on this function for all applicable nodes
                        RealignTreeNodeLevelShift(
                            tnChild, removeLevels, insertLevels, depth + depthAdjustment - 1, branchTester, -1, -1, -1, false);
                    }
                    tnChild = tnChild.NextSibling;
                }
            }
            else
            {
                //
                // The expansions from levelShift levels away are moving up to this
                // level. There are several steps involved.
                // 1) Get the current node counts of the targeted items in the list
                //    and remove the expansion counts from the full count;
                // 2) Get all of the lists that are levelShift levels away and put them
                //    in a single list. Adjust the Expanded states along the way to
                //    reflect the expanded state of the parent lists.
                // 3) Adjust the indices of trailing items by count - newCount.
                // 4) Use LocateExpandedList to attach nodes to the new position and adjust
                //    counts accordingly.
                //   

                var changeCount = newCount - count;
                var subItemChange = 0;
                tn.ImmedCount += changeCount;
                var maxIndexReattach = maxIndex + changeCount;
                NODEPOSITIONTRACKER_Dynamic detachedTrackers = null;
                if ((tn.FirstPositionTracker != null)
                    && !tn.DefaultTracking)
                {
                    NODEPOSITIONTRACKER_Dynamic.DetachTrackers(ref tn.FirstPositionTracker, ref detachedTrackers);
                }
                if (tnChild != null)
                {
                    TREENODE tnDummyHead = null;
                    TREENODE tnAttach = null;
                    TREENODE tnDummyHeadInKillZone = null;
                    TREENODE tnAttachInKillZone = null;
                    var tnNext = tnChild;
                    if (removeLevels > 1)
                    {
                        var alwaysTossExpansions = tn.NoRelocate && (insertLevels == 0);
                        --removeLevels;
                        while (tnNext != null)
                        {
                            if (tnNext.Index >= maxIndex)
                            {
                                // Adjust indices
                                var adjust = newCount - count;
                                if (adjust != 0)
                                {
                                    // Keep the first untouched node (after the touched nodes) for later use
                                    tnChild = tnNext;
                                    while (tnChild != null)
                                    {
                                        tnChild.Index += adjust;
                                        tnChild = tnChild.NextSibling;
                                    }
                                }
                                // Get out of outer loop
                                break;
                            }

                            // Get the next node before deleting
                            tnChild = tnNext;
                            tnNext = tnChild.NextSibling;

                            // Make sure we're actually processing the right level.
                            if (branchTester != null)
                            {
                                var adjustResult = branchTester.ValidateAdjustDepth(tnChild.Branch);
                                var depthAdjustment = adjustResult.DepthAdjustment;
                                var keepWalkingBranch = adjustResult.Continue;
                                // Ignore any negative depth adjustment returned by the branchTester.
                                // We can't do anything with a negative adjustment here because we're 
                                // already at 0. Also, it is very likely that this same call in the 
                                // depth > 0 block is what put us here in the first place, so we should
                                // not reapply these results.
                                if (!keepWalkingBranch
                                    || depthAdjustment > 0)
                                {
                                    if (tnAttach == null)
                                    {
                                        if (tnDummyHead == null)
                                        {
                                            tnDummyHead = new TREENODE();
                                        }
                                        // Create a dummy head to attach to
                                        tnAttach = tnDummyHead;
                                    }
                                    tnAttach.NextSibling = tnChild;
                                    tnChild.NextSibling = null;
                                    tnAttach = tnChild;

                                    if (tnChild.Expanded)
                                    {
                                        // Drop change count here and add back later if
                                        // the branch successfully relocates.
                                        changeCount -= tnChild.FullCount;
                                        subItemChange -= tnChild.ExpandedSubItemGain;
                                    }
                                    subItemChange -= tnChild.ImmedSubItemGain;

                                    if (keepWalkingBranch && depthAdjustment > 0)
                                    {
                                        RealignTreeNodeLevelShift(
                                            tnChild, removeLevels + 1, insertLevels, depthAdjustment - 1, branchTester, -1, -1, -1, false);
                                    }
                                    else if (!alwaysTossExpansions)
                                    {
                                        // See comments on CollectChildBranches below
                                        CollectChildBranches(
                                            ref tnChild, branchTester, ref tnAttach, ref tnDummyHead, ref tnAttachInKillZone,
                                            ref tnDummyHeadInKillZone, ref detachedTrackers, removeLevels, true);
                                    }
                                    continue;
                                }
                            }

                            //
                            // Adjust the change count for this node and retrieve the child nodes
                            // to reattach later. Note that much of the item count will be recouped
                            // later on when the nodes are reattached.
                            //
                            if (tnChild.Expanded)
                            {
                                changeCount -= tnChild.FullCount;
                                subItemChange -= tnChild.ExpandedSubItemGain;
                            }
                            subItemChange -= tnChild.ImmedSubItemGain;

                            if (!alwaysTossExpansions)
                            {
                                // Get the branches from the child nodes. Note that the parent item
                                // is always treated as a root in and of itself, so it is always
                                // considered to be expanded (parentExpanded = true) even if it is
                                // collapsed in its parent branch. Note than tnChild will come
                                // back null if the branchTester decides that this is a branch
                                // that needs to be reattached. We've already retrieved the next
                                CollectChildBranches(
                                    ref tnChild, branchTester, ref tnAttach, ref tnDummyHead, ref tnAttachInKillZone,
                                    ref tnDummyHeadInKillZone, ref detachedTrackers, removeLevels, true);
                            }

                            // Free the child nodes. Note that CollectBranches detaches the
                            // branches it cares about, so this doesn't affect the branches that
                            // we might end up keeping.
                            if (tnChild != null)
                            {
                                FreeRecursive(ref tnChild, ref detachedTrackers);
                            }
                        }
                    }
                    else
                    {
                        // UNDONE: The original condition on the if was > 0, but that took out one too many levels.
                        // We need to figure out if this is the correct code for removeLevels == 0, or if
                        // we ever call with this value.
                        Debug.Assert(removeLevels == 1);
                        // No levels were removed. Explicitly pull in-range items from this list into the
                        // dummy list. We can't use CollectChildBranches here without making it understand
                        // the range issues, which isn't worth it.
                        if (tnChild.Index < maxIndex)
                        {
                            // Separate the children we care about into a separate list
                            tnDummyHead = new TREENODE();
                            tnDummyHead.NextSibling = tnChild; // Record our starting point
                            while (tnNext != null)
                            {
                                if (tnNext.Index < maxIndex)
                                {
                                    if (tnNext.Expanded)
                                    {
                                        changeCount -= tnNext.FullCount;
                                        subItemChange -= tnNext.ExpandedSubItemGain;
                                    }
                                    subItemChange -= tnNext.ImmedSubItemGain;
                                }
                                else
                                {
                                    // Leave tnChild alone set so we can reattach.
                                    // Adjust the indices on trailing nodes here.
                                    var adjust = newCount - count;
                                    if (adjust != 0)
                                    {
                                        tnChild = tnNext;
                                        while (tnChild != null)
                                        {
                                            tnChild.Index += adjust;
                                            tnChild = tnChild.NextSibling;
                                        }
                                    }
                                    break;
                                }
                                tnNext = tnNext.NextSibling;
                            }
                        }
                    }

                    //
                    // Relink untouched nodes
                    //
                    if (tnLastUntouched == null)
                    {
                        tn.FirstChild = tnNext;
                    }
                    else
                    {
                        tnLastUntouched.NextSibling = tnNext;
                    }

                    // Reattach any detached branches
                    // First reattach the branches that survived the kill zone (via ILevelShiftAdjuster.TestReattachBranch),
                    // then do a second pass to reattach the items below the kill zone. This enables the higher level nodes
                    // to reattach before we start creating replacement nodes for them.
                    if (tnDummyHeadInKillZone == null)
                    {
                        tnDummyHeadInKillZone = tnDummyHead;
                    }
                    while (tnDummyHeadInKillZone != null)
                    {
                        try
                        {
                            // Work on reattaching the nodes
                            TREENODE tnCurParent;
                            TREENODE tnNextParent;
                            var tnLastChildPrimary = tnLastUntouched;
                            TREENODE tnLastChildSecondary;
                            var tnPrev = tnDummyHeadInKillZone;
                            BranchLocationAction action;
                            int attachIndex;
                            int level;
                            bool expandBranch;
                            // UNDONE: There is a really bad issue here with initial subitem expansion. Initial subitem expansion is
                            // normally done when the list is first expanded. However, when items are shuffled, the subitem expansion
                            // state suddenly belongs to a different parent node. We need to wait until after all detached branches
                            // are reattached to be able to actually run the 'initial' subitem expansion, which now should only attempt
                            // to expand indices which do not yet have a corresponding treenode. For now, it is up to the providers to
                            // successfully locate the subitem expansions, or these items will be tossed. In the long run, we need
                            // to keep track of all of these objects and deal with after the main loop has complete.
                            bool dummyRequireSubItemExpansion;
                            int expansionCount;
                            int subItemIncr;
                            tnNext = tnPrev.NextSibling;
                            // UNDONE: Is this right? Another opinion below. If this is not done
                            // correctly, then we end up calling ExpandTreeNode below in the KeepBranch
                            // cases when we shouldn't and incorrectly create a new node/branch when
                            // the existing one is fine. This leads to a crash when tnChild is orphaned
                            // and its FirstPositionTracker is never cleared
                            var maxInsertLevel = insertLevels - 1;
                            while ((tnChild = tnNext) != null)
                            {
                                // Get the next node up front in case we delete this one
                                tnNext = tnChild.NextSibling;

                                // Expand nodes to the correct level
                                for (level = 0, tnCurParent = tn; level <= maxInsertLevel; ++level)
                                {
                                    if (tnCurParent.NoRelocate)
                                    {
                                        // Just move up the list, nodes we don't reattach
                                        // are deleted at the end.
                                        tnPrev = tnChild;
                                        break;
                                    }
                                    else
                                    {
                                        LocateObjectData locateData;
                                        if (tnChild.Branch == null)
                                        {
                                            expandBranch = tnCurParent.Expanded;
                                            var tnLocate = tnChild.FirstSubItem.RootNode;
                                            locateData = tnCurParent.Branch.LocateObject(
                                                tnLocate.Branch,
                                                tnLocate.ComplexSubItem ? ObjectStyle.SubItemRootBranch : ObjectStyle.SubItemExpansion,
                                                0);
                                            Debug.Assert(
                                                (BranchLocationAction)locateData.Options == BranchLocationAction.DiscardBranch
                                                || tnChild.FirstSubItem.ColumnOfRootNode(tnLocate) == locateData.Column);
                                        }
                                        else
                                        {
                                            expandBranch = tnChild.Expanded;
                                            locateData = tnCurParent.Branch.LocateObject(
                                                tnChild.Branch,
                                                ObjectStyle.ExpandedBranch,
                                                0);
                                        }
                                        attachIndex = locateData.Row;
                                        action = (BranchLocationAction)locateData.Options;
                                        switch (action)
                                        {
                                            case BranchLocationAction.RetrieveNewBranch:
                                                // This only makes sense at the last insertion level
                                                if (level == maxInsertLevel)
                                                {
                                                    //Use the returned Index to retrieve a new list. Go ahead and
                                                    //leave the current node in the kill list and get a brand new one.
                                                    tnPrev = tnChild;
                                                    try
                                                    {
                                                        tnChild = ExpandTreeNode(
                                                            tnCurParent, null, attachIndex, 0, false, out expansionCount, out subItemIncr);
                                                        tnPrev.TransferPositionTrackerTo(tnChild);
                                                        changeCount += expansionCount;
                                                        subItemChange += subItemIncr;
                                                        if (level == 0)
                                                        {
                                                            InsertIndexedNode(tnCurParent, tnChild, ref tnLastChildPrimary);
                                                        }
                                                        else
                                                        {
                                                            tnLastChildSecondary = null;
                                                            InsertIndexedNode(tnCurParent, tnChild, ref tnLastChildSecondary);
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        // Ignore an error here, just let the expansion be lost.
                                                        level = maxInsertLevel + 1;
                                                    }
                                                }
                                                else
                                                {
                                                    goto case BranchLocationAction.DiscardBranch;
                                                }
                                                break;
                                            case BranchLocationAction.DiscardBranch:
                                                // The deletion of items in this branch have already been
                                                // accounted for. Just leave it in the dummy list and discard
                                                // it when we're done reattaching.
                                                tnPrev = tnChild;
                                                level = maxInsertLevel + 1;
                                                break;
                                            case BranchLocationAction.KeepBranchAtThisLevel:
                                            case BranchLocationAction.KeepBranch:
                                                // Detach from the list of nodes to insert
                                                tnPrev.NextSibling = tnNext;
                                                tnChild.NextSibling = null;
                                                if (action == BranchLocationAction.KeepBranchAtThisLevel)
                                                {
                                                    // Easy to do, just pretend we're at the last level and move on.
                                                    level = maxInsertLevel;
                                                }
                                                if (level == 0)
                                                {
                                                    // First level down, we do a little more work here than in the secondary branches

                                                    // Validate the attach index. We don't want to put a new
                                                    // branch on top of one outside our edit range.
                                                    if (start != -1
                                                        &&
                                                        (attachIndex < start || attachIndex >= maxIndexReattach))
                                                    {
                                                        // Undo detach
                                                        tnChild.NextSibling = tnPrev.NextSibling;
                                                        tnPrev.NextSibling = tnChild;
                                                        goto case BranchLocationAction.DiscardBranch;
                                                    }
                                                    else if (insertLevels == 0
                                                             || level == maxInsertLevel)
                                                    {
                                                        // Removing nodes only, just reattach
                                                        if (expandBranch)
                                                        {
                                                            changeCount += tnChild.FullCount;
                                                            subItemChange += tnChild.ExpandedSubItemGain;
                                                        }
                                                        subItemChange += tnChild.ImmedSubItemGain;
                                                        tnChild.Index = attachIndex;
                                                        InsertIndexedNode(tn, tnChild, ref tnLastChildPrimary);
                                                    }
                                                    else
                                                    {
                                                        tnNextParent = FindIndexedNode(
                                                            attachIndex,
                                                            (tnLastUntouched == null) ? tn.FirstChild : tnLastUntouched.NextSibling,
                                                            ref tnLastChildPrimary);
                                                        if (tnNextParent != null)
                                                        {
                                                            if (expandBranch && !tnNextParent.Expanded)
                                                            {
                                                                tnNextParent.Expanded = true;
                                                                changeCount += tnNextParent.FullCount;
                                                                subItemChange += tnNextParent.ExpandedSubItemGain;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            tnNextParent = ExpandTreeNode(
                                                                tnCurParent, null, attachIndex, 0, false, out expansionCount,
                                                                out dummyRequireSubItemExpansion);
                                                            tnNextParent.Expanded = expandBranch;
                                                            if (expandBranch)
                                                            {
                                                                changeCount += tnNextParent.FullCount;
                                                                subItemChange += tnNextParent.ExpandedSubItemGain;
                                                            }
                                                            subItemChange += tnNextParent.ImmedSubItemGain;
                                                            InsertIndexedNode(tnCurParent, tnNextParent, ref tnLastChildPrimary);
                                                        }
                                                        tnCurParent = tnNextParent;
                                                    }
                                                }
                                                else if (level == maxInsertLevel)
                                                {
                                                    // Reattach existing node to current parent
                                                    tnLastChildSecondary = null;
                                                    tnChild.Index = attachIndex;
                                                    InsertIndexedNode(tnCurParent, tnChild, ref tnLastChildSecondary);
                                                    if (expandBranch)
                                                    {
                                                        changeCount += tnChild.FullCount;
                                                        subItemChange += tnChild.FullSubItemGain;
                                                    }
                                                    if (tnCurParent != tn)
                                                    {
                                                        ChangeFullCountRecursive(
                                                            tnCurParent, tnChild.Expanded ? tnChild.FullCount : 0, tnChild.FullSubItemGain,
                                                            tn);
                                                    }
                                                }
                                                else
                                                {
                                                    tnLastChildSecondary = null;
                                                    tnNextParent = FindIndexedNode(
                                                        attachIndex, tnCurParent.FirstChild, ref tnLastChildSecondary);
                                                    if (tnNextParent != null)
                                                    {
                                                        if (expandBranch && !tnNextParent.Expanded)
                                                        {
                                                            tnNextParent.Expanded = true;
                                                            changeCount += tnNextParent.FullCount;
                                                            subItemChange += tnNextParent.FullSubItemGain;
                                                            ChangeFullCountRecursive(
                                                                tnCurParent, tnNextParent.FullCount, tnNextParent.FullSubItemGain, tn);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        tnNextParent = ExpandTreeNode(
                                                            tnCurParent, null, attachIndex, 0, false, out expansionCount,
                                                            out dummyRequireSubItemExpansion);
                                                        tnNextParent.Expanded = expandBranch;
                                                        if (expandBranch)
                                                        {
                                                            changeCount += tnNextParent.FullCount;
                                                            subItemChange += tnNextParent.FullSubItemGain;
                                                            ChangeFullCountRecursive(
                                                                tnCurParent, tnNextParent.FullCount, tnNextParent.FullSubItemGain, tn);
                                                        }
                                                        InsertIndexedNode(tnCurParent, tnNextParent, ref tnLastChildSecondary);
                                                    }
                                                    tnCurParent = tnNextParent;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            // Delete any items that are still attached to the dummy head.
                            // We need to explicitly free here so that DestroyTreeNode is
                            // called correctly, GC can't do this automatically for us.
                            tnNext = tnDummyHeadInKillZone.NextSibling;
                            while ((tnChild = tnNext) != null)
                            {
                                tnNext = tnChild.NextSibling;
                                FreeRecursive(ref tnChild, ref detachedTrackers);
                            }
                            // tnDummyHeadInKillZone will be destroyed via the ref parameter
                        }

                        // Trigger a second pass
                        tnDummyHeadInKillZone = tnDummyHead;
                        tnDummyHead = null;
                    } // tnDummyHeadInKillZone != null
                } // tnChild != null

                // Attempt to reattach any trackable objects back into the new tree structure
                if (detachedTrackers != null)
                {
                    int reattachChangeCount;
                    int reattachSubItemChangeCount;
                    detachedTrackers.QueryReattachObjects(
                        this, tn, (removeLevels == 0) ? insertLevels : insertLevels - 1, out reattachChangeCount,
                        out reattachSubItemChangeCount);
                    changeCount += reattachChangeCount;
                    subItemChange += reattachSubItemChangeCount;
                }
                if ((changeCount + subItemChange) != 0)
                {
                    ChangeFullCountRecursive(tn, changeCount, subItemChange);
                }
            }
        }

        // Helper function for RealignTreeNodeLevelShift. Collects sub branches into a single
        // list, detaches them from the parent nodes, and adjusts the Expanded setting on each.
        private void CollectChildBranches(
            ref TREENODE tnParent, ILevelShiftAdjuster branchTester, ref TREENODE tnAttach, ref TREENODE tnDummyHead,
            ref TREENODE tnAttachInKillZone, ref TREENODE tnDummyHeadInKillZone, ref NODEPOSITIONTRACKER_Dynamic detachedTrackers,
            int levelShift, bool parentExpanded)
        {
            // The ILevelShiftAdjuster mechanism allows us to keep this expansion for later attachment,
            // essentially making it a potential sibling of branches originally below this level.
            var tnProcessParent = tnParent;
            if (branchTester != null)
            {
                var testResult = branchTester.TestReattachBranch(tnParent.Branch);
                if (testResult != TestReattachBranchResult.Discard)
                {
                    tnParent.Expanded = tnParent.Expanded && parentExpanded;
                    if (tnAttachInKillZone == null)
                    {
                        if (tnDummyHeadInKillZone == null)
                        {
                            tnDummyHeadInKillZone = new TREENODE();
                        }
                        // Create a dummy head to attach to
                        tnAttachInKillZone = tnDummyHeadInKillZone;
                    }
                    tnAttachInKillZone.NextSibling = tnParent;
                    tnAttachInKillZone = tnParent;
                    tnParent.NextSibling = null;
                    tnParent = null;
                    switch (testResult)
                    {
                        case TestReattachBranchResult.ReattachIntact:
                            return;
                        case TestReattachBranchResult.Realign:
                            tnProcessParent.ImmedCount = tnProcessParent.Branch.VisibleItemCount;
                            if ((tnProcessParent.FirstPositionTracker != null)
                                && !tnProcessParent.DefaultTracking)
                            {
                                NODEPOSITIONTRACKER_Dynamic.DetachTrackers(ref tnProcessParent.FirstPositionTracker, ref detachedTrackers);
                            }
                            goto case TestReattachBranchResult.ReattachChildren;
                        case TestReattachBranchResult.ReattachChildren: // Allow processing to continue below
                            tnProcessParent.FullCount = tnProcessParent.ImmedCount;
                            tnProcessParent.ImmedSubItemGain = 0;
                            tnProcessParent.FullSubItemGain = 0;
                            break;
                    }
                }
            }

            var tnChild = tnProcessParent.FirstChild;
            if (tnChild == null)
            {
                // Not checked before calling CollectChildBranches so that TestReattachBranch has a chance
                return;
            }
            Debug.Assert((tnAttach == null) || (tnAttach.NextSibling == null)); // Bad attach point
            parentExpanded = parentExpanded && tnProcessParent.Expanded;
            if (levelShift <= 1)
            {
                // Walk the children to adjust the Expanded states and locate
                // the last child (for our next attach point).
                TREENODE tnPrev = null;
                while (tnChild != null)
                {
                    tnPrev = tnChild;
                    tnChild.Expanded = tnChild.Expanded && parentExpanded;
                    tnChild = tnChild.NextSibling;
                }

                // Detach the child list from the parent node and reattach it. Note
                // that this thrashes the FullCount of the parent node, but this is
                // harmless because we're about to blow the immediate parents away anyway.
                if (tnAttach == null)
                {
                    if (tnDummyHead == null)
                    {
                        tnDummyHead = new TREENODE();
                    }
                    // Create a dummy head to attach to
                    tnAttach = tnDummyHead;
                }
                tnAttach.NextSibling = tnProcessParent.FirstChild;
                tnProcessParent.FirstChild = null;
                tnAttach = tnPrev;
            }
            else
            {
                --levelShift;
                var tnNext = tnChild;
                while ((tnChild = tnNext) != null)
                {
                    // tnChild can come back null, get NextSibling first
                    tnNext = tnChild.NextSibling;
                    CollectChildBranches(
                        ref tnChild, branchTester, ref tnAttach, ref tnDummyHead, ref tnAttachInKillZone, ref tnDummyHeadInKillZone,
                        ref detachedTrackers, levelShift, parentExpanded);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RealignTreeNode(TREENODE tn)
        {
            TREENODE tnChild;
            TREENODE tnChildTmp;
            TREENODE tnLastChild;
            int ChangeCount;

            //Make sure the number of items is in sync 
            ChangeCount = 0;
            ChangeCount -= tn.ImmedCount;
            tn.ImmedCount = tn.Branch.VisibleItemCount;
            ChangeCount += tn.ImmedCount;

            //Make sure the expanded items are in sync
            if (tn.NoRelocate)
            {
                //Relocation isn't supported. Just
                //close down any open children
                tnChildTmp = tn.FirstChild;
                tn.FirstChild = null;
                while ((tnChild = tnChildTmp) != null)
                {
                    if (tnChild.Expanded)
                    {
                        ChangeCount -= tnChild.FullCount;
                    }
                    tnChildTmp = tnChild.NextSibling; //Get child before calling FreeRecursive
                    FreeRecursive(ref tnChild);
                }
            }
            else
            {
                int ExpansionCount;
                int ReloadIndex;
                BranchLocationAction action;
                //This is similar to above, but more complicated
                tnChildTmp = tn.FirstChild;
                tn.FirstChild = null;
                tnLastChild = null;
                while ((tnChild = tnChildTmp) != null)
                {
                    tnChildTmp = tnChild.NextSibling; //Get next child before calling FreeRecursive
                    var locateData = tn.Branch.LocateObject(tnChild.Branch, ObjectStyle.ExpandedBranch, 0);
                    // UNDONE_MC: locateData.Column not used
                    tnChild.Index = locateData.Row;
                    action = (BranchLocationAction)locateData.Options;
                    switch (action)
                    {
                        case BranchLocationAction.DiscardBranch:
                            //Get rid of list, just like in norelocate case
                            if (tnChild.Expanded)
                            {
                                ChangeCount -= tnChild.FullCount;
                            }
                            FreeRecursive(ref tnChild);
                            break;
                        case BranchLocationAction.KeepBranchAtThisLevel:
                        case BranchLocationAction.KeepBranch:
                            InsertIndexedNode(tn, tnChild, ref tnLastChild);
                            break;
                        case BranchLocationAction.RetrieveNewBranch:
                            //Use the returned Index to retrieve a new list
                            ReloadIndex = tnChild.Index;
                            ChangeCount -= tnChild.FullCount;
                            FreeRecursive(ref tnChild);
                            try
                            {
                                int subItemIncr;
                                tnChild = ExpandTreeNode(
                                    tn, null, ReloadIndex, COLUMN_ZERO /*UNDONE_MC*/, false,
                                    out ExpansionCount, out subItemIncr);
                            }
                            catch
                            {
                                // Ignore an error here, just let the expansion be lost.
                                break;
                            }
                            goto case BranchLocationAction.KeepBranch;
                    }
                }
            }

            if (ChangeCount != 0)
            {
                ChangeFullCountRecursive(tn, ChangeCount, 0 /*UNDONE_MC*/);
            }
        }

        // Helper function to see if a node already exists in a sibling list
        private static TREENODE FindIndexedNode(int findIndex, TREENODE tnFirstChild, ref TREENODE tnHintChild)
        {
            TREENODE retVal = null;
            if (tnFirstChild != null)
            {
                if (tnHintChild == null
                    || tnHintChild.Index > findIndex)
                {
                    tnHintChild = tnFirstChild;
                }
                var tnChild = tnHintChild;
                while (tnChild != null)
                {
                    if (tnChild.Index < findIndex)
                    {
                        tnChild = tnHintChild = tnChild.NextSibling;
                    }
                    else
                    {
                        if (tnChild.Index == findIndex)
                        {
                            retVal = tnHintChild = tnChild;
                        }
                        break;
                    }
                }
            }
            return retVal;
        }

        // Helper function to insert a node into the middle of an existing sibling list of
        // child nodes. The hint item is used to optimize adding items in order.
        private static void InsertIndexedNode(TREENODE tnParent, TREENODE tnChild, ref TREENODE tnHintChild)
        {
            TREENODE tnPrev;
            tnChild.Parent = tnParent;
            if (tnParent.FirstChild != null)
            {
                if (tnHintChild != null
                    &&
                    tnChild.Index > tnHintChild.Index)
                {
                    //Optimized for case where expanded lists are still in order
                    tnPrev = tnChild;
                    tnChild = tnHintChild;
                    tnHintChild = tnPrev;
                }
                else
                {
                    //Have to search from beginning and re-insert
                    tnHintChild = tnChild;
                    tnChild = tnParent.FirstChild;
                }

                tnPrev = null;
                while (tnHintChild.Index >= tnChild.Index)
                {
                    tnPrev = tnChild;
                    if (null == (tnChild = tnPrev.NextSibling))
                    {
                        break;
                    }
                }
                if (tnPrev != null)
                {
                    Debug.Assert(tnPrev.Index != tnHintChild.Index);
                    tnPrev.NextSibling = tnHintChild;
                }
                else
                {
                    tnParent.FirstChild = tnHintChild;
                }
                tnHintChild.NextSibling = tnChild;
            }
            else
            {
                tnParent.FirstChild = tnHintChild = tnChild;
                tnChild.NextSibling = null;
            }
        }

        private void UpdateTreeNodeRecursive(TREENODE tn)
        {
            int UpdateCounter;
            while (tn != null)
            {
                if (tn.CallUpdate)
                {
                    UpdateCounter = tn.Branch.UpdateCounter;
                    if (tn.UpdateCounter != UpdateCounter)
                    {
                        tn.UpdateCounter = UpdateCounter;
                        RealignTreeNode(tn);
                    }
                }
                if (tn.FirstChild != null)
                {
                    UpdateTreeNodeRecursive(tn.FirstChild);
                }
                tn = tn.NextSibling;
            }
        }

        private bool DelayTurnOffRedraw()
        {
            if (GetStateFlag(TreeStateFlags.TurnOffRedraw))
            {
                SetStateFlag(TreeStateFlags.TurnOffRedraw, false);
                if (OnSetRedraw != null)
                {
                    OnSetRedraw(this, new SetRedrawEventArgs(false));
                }
                return true;
            }
            return false;
        }

        private bool DelayTurnOffSingleColumnRedraw()
        {
            if (GetStateFlag(TreeStateFlags.TurnOffSingleColumnRedraw))
            {
                SetStateFlag(TreeStateFlags.TurnOffSingleColumnRedraw, false);
                if (GetStateFlag(TreeStateFlags.FireSingleColumnOnSetRedraw))
                {
                    var singleTree = SingleColumnTree;
                    (singleTree as SingleColumnView).myOnSetRedraw(singleTree, new SetRedrawEventArgs(false));
                }
                return true;
            }
            return false;
        }

        #region SingleColumnView, single column view of multi-column tree

        /// <summary>
        ///     Reimplementation of ITree interface to show first column of a multi-column
        ///     tree as a single column tree.
        /// </summary>
        private class SingleColumnView : ITree
        {
            // ITree methods in this class fall into three categories
            // 1) Pass through only. These are mostly branch modification style methods. Many of
            //    these methods will result in event notifications back to the single-column view.
            // 2) Informational methods to retrieve information about the single column tree
            // 3) Active methods, which fire events back to the single and multi-column trees in response
            //    to actions at a given index (as opposed to pass through methods, which take branches).
            //    These methods are implemented by translating he single-column row into a multi-column
            //    row and deferring to the main VirtualTree implementation, which will fire events
            //    to both the multi-column tree and the single-column view.
            private readonly VirtualTree myParent;

            // We use private events so we can track when they are detached and attached.
            // This lets us set a flag in the VirtualTree when the last significant single
            // column event is detached, so the main code can make a simple check before
            // gathering data to fire these events.
            internal QueryItemVisibleEventHandler myOnQueryItemVisible;
            internal ItemCountChangedEventHandler myItemCountChanged;
            internal ItemMovedEventHandler myItemMoved;
            internal ToggleStateEventHandler myStateToggled;
            internal DisplayDataChangedEventHandler myOnDisplayDataChanged;
            internal SetRedrawEventHandler myOnSetRedraw;

            /// <summary>
            ///     Create a single column view on the parent tree
            /// </summary>
            /// <param name="parent">The parent tree object</param>
            public SingleColumnView(VirtualTree parent)
            {
                myParent = parent;
            }

            #region Active ITree methods

            ToggleExpansionData ITree.ToggleExpansion(int row, int column)
            {
                bool allowRecursion;
                int itemExpansionCount;
                int subItemExpansionCount;
                myParent.ToggleExpansion(
                    myParent.TranslateSingleColumnRow(row), 0, out itemExpansionCount, out subItemExpansionCount, out allowRecursion);
                return new ToggleExpansionData(itemExpansionCount, allowRecursion);
            }

            StateRefreshChanges ITree.ToggleState(int row, int column)
            {
                return (myParent as ITree).ToggleState(myParent.TranslateSingleColumnRow(row), 0);
            }

            void ITree.SynchronizeState(ColumnItemEnumerator itemsToSynchronize, IBranch matchBranch, int matchRow, int matchColumn)
            {
                myParent.SynchronizeState(itemsToSynchronize, matchBranch, matchRow, matchColumn, true /* translateSingleColumnView */);
            }

            #endregion // Active ITree methods

            #region Informational ITree methods and events

            bool ITree.IsExpanded(int row, int column)
            {
                var pos = myParent.TrackSingleColumnRow(row);
                return pos.IsExpanded(0);
            }

            bool ITree.IsExpandable(int row, int column)
            {
                var pos = myParent.TrackSingleColumnRow(row);
                return pos.IsExpandable(0);
            }

            VirtualTreeItemInfo ITree.GetItemInfo(int row, int column, bool setFlags)
            {
                var pos = myParent.TrackSingleColumnRow(row);
                return myParent.GetItemInfo(ref pos, 0, setFlags, true, false);
            }

            int ITree.VisibleItemCount
            {
                get { return myParent.myRootNode == null ? 0 : myParent.myRootNode.FullCount; }
            }

            int ITree.GetDescendantItemCount(int row, int column, bool includeSubItems, bool complexColumnRoot)
            {
                var pos = myParent.TrackSingleColumnRow(row);
                var tn = pos.ParentNode.GetChildNode(pos.Index);
                if (tn != null
                    && tn.Expanded)
                {
                    return tn.FullCount;
                }
                return 0;
            }

            int ITree.GetSubItemCount(int row, int column)
            {
                // Single-column view never has sub items
                return 0;
            }

            BlankExpansionData ITree.GetBlankExpansion(int row, int column, ColumnPermutation columnPermutation)
            {
                // The blank expansion in a single-column tree is always the single item
                return new BlankExpansionData(row, 0);
            }

            int ITree.GetParentIndex(int row, int column)
            {
                var pos = myParent.TrackSingleColumnRow(row);
                return pos.ParentAbsolute;
            }

            ExpandedBranchData ITree.GetExpandedBranch(int row, int column)
            {
                if (myParent.myRootNode != null)
                {
                    var pos = myParent.TrackSingleColumnRow(row);
                    var tn = pos.ParentNode.GetChildNode(pos.Index);

                    if (tn != null
                        && tn.Expanded)
                    {
                        return new ExpandedBranchData(tn.Branch, pos.Level);
                    }
                }
                throw new ArgumentOutOfRangeException("row"); //UNDONE: EXCEPTION, different exception for not parent object??
            }

            int ITree.GetOffsetFromParent(int parentRow, int column, int relativeIndex, bool complexColumnRoot)
            {
                TREENODE tn;
                if (parentRow == VirtualTreeConstant.NullIndex)
                {
                    tn = myParent.myRootNode;
                }
                else
                {
                    var pos = myParent.TrackSingleColumnRow(parentRow);
                    tn = pos.ParentNode.GetChildNode(pos.Index);
                }
                if ((tn != null)
                    && tn.Expanded)
                {
                    return tn.GetSingleColumnChildOffset(relativeIndex);
                }
                else
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.ParentRowException));
                }
            }

            VirtualTreeCoordinate ITree.GetNavigationTarget(
                TreeNavigation direction, int sourceRow, int sourceColumn, ColumnPermutation columnPermutation)
            {
                Debug.Assert(sourceColumn == 0);
                sourceColumn = 0;
                var targetRow = sourceRow;
                if (myParent.myRootNode == null)
                {
                    return VirtualTreeCoordinate.Invalid;
                }

                var retVal = false;

                var pos = myParent.TrackSingleColumnRow(sourceRow);
                TREENODE tnChild;
                switch (direction)
                {
                    case TreeNavigation.Left:
                    case TreeNavigation.Parent:
                    case TreeNavigation.ComplexParent:
                        if (pos.ParentAbsolute != -1)
                        {
                            targetRow = pos.ParentAbsolute;
                            retVal = true;
                        }
                        break;
                    case TreeNavigation.Right:
                    case TreeNavigation.FirstChild:
                    case TreeNavigation.LastChild:
                        // FirstChild and LastChild share code for testing if the node is expanded
                        tnChild = pos.ParentNode.GetChildNode(pos.Index);
                        if (tnChild != null
                            && tnChild.Expanded
                            && tnChild.ImmedCount > 0)
                        {
                            // Right jumps to FirstChild, test LastChild, not FirstChild, so
                            // else handles both elements.
                            if (direction == TreeNavigation.LastChild)
                            {
                                targetRow = sourceRow + tnChild.GetSingleColumnChildOffset(tnChild.ImmedCount - 1);
                            }
                            else
                            {
                                targetRow = sourceRow + 1;
                            }
                            retVal = true;
                        }
                        break;
                    case TreeNavigation.NextSibling:
                        // Test localColumn. Expandable and simple cells don't have siblings,
                        // and all other nodes will give back a localColumn of 0.
                        if (pos.Index < (pos.ParentNode.ImmedCount - 1))
                        {
                            targetRow = sourceRow + 1;
                            tnChild = pos.ParentNode.GetChildNode(pos.Index);
                            if (tnChild != null)
                            {
                                targetRow += tnChild.FullCount;
                            }
                            retVal = true;
                        }
                        break;
                    case TreeNavigation.PreviousSibling:
                        if (pos.Index > 0)
                        {
                            targetRow = pos.ParentAbsolute + pos.ParentNode.GetSingleColumnChildOffset(pos.Index - 1);
                            retVal = true;
                        }
                        break;
                    case TreeNavigation.Up:
                        if (targetRow > 0)
                        {
                            --targetRow;
                            retVal = true;
                        }
                        break;
                    case TreeNavigation.Down:
                        if (targetRow < (myParent.myRootNode.FullCount - 1))
                        {
                            ++targetRow;
                            retVal = true;
                        }
                        break;
                        //case TreeNavigation.RightColumn:
                        //case TreeNavigation.LeftColumn:
                }
                return retVal ? new VirtualTreeCoordinate(targetRow, 0) : VirtualTreeCoordinate.Invalid;
            }

            ColumnItemEnumerator ITree.EnumerateColumnItems(
                int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int[] rowFilter, bool markExcludedFilterItems)
            {
                return new ColumnItemEnumeratorSingleColumnImpl(myParent, rowFilter, markExcludedFilterItems);
            }

            ColumnItemEnumerator ITree.EnumerateColumnItems(
                int column, ColumnPermutation columnPermutation, bool returnBlankAnchors, int startRow, int endRow)
            {
                return new ColumnItemEnumeratorSingleColumnImpl(myParent, startRow, endRow);
            }

            VirtualTreeCoordinate ITree.LocateObject(IBranch startingBranch, object target, int locateStyle, int locateOptions)
            {
                // UNDONE: Might want to add a flag to indicate that we should limit the search to a single column
                var multiColumnCoordinate = (myParent as ITree).LocateObject(startingBranch, target, locateStyle, locateOptions);
                if (multiColumnCoordinate.IsValid
                    && multiColumnCoordinate.Column == 0)
                {
                    return new VirtualTreeCoordinate(myParent.TranslateMultiColumnRow(multiColumnCoordinate.Row), 0);
                }
                return VirtualTreeCoordinate.Invalid;
            }

            void ITree.RemoveBranch(IBranch branch)
            {
                (myParent as ITree).RemoveBranch(branch);
            }

            bool ITree.IsItemVisible(int absIndex)
            {
                if (myOnQueryItemVisible == null)
                {
                    return true;
                }
                else
                {
                    var args = new QueryItemVisibleEventArgs(absIndex);

                    myOnQueryItemVisible(this, args);
                    return args.IsVisible;
                }
            }

            event QueryItemVisibleEventHandler ITree.OnQueryItemVisible
            {
                add
                {
                    if (value != null)
                    {
                        myOnQueryItemVisible += value;
                        // The parent doesn't use this event, no need to set
                        // a tree state as with the other events
                    }
                }
                remove { myOnQueryItemVisible -= value; }
            }

            event ItemCountChangedEventHandler ITree.ItemCountChanged
            {
                add
                {
                    if (value != null)
                    {
                        myItemCountChanged += value;
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged, true);
                    }
                }
                remove
                {
                    myItemCountChanged -= value;
                    if (myItemCountChanged == null)
                    {
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnItemCountChanged, false);
                    }
                }
            }

            event ItemMovedEventHandler ITree.ItemMoved
            {
                add
                {
                    if (value != null)
                    {
                        myItemMoved += value;
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnItemMoved, true);
                    }
                }
                remove
                {
                    myItemMoved -= value;
                    if (myItemMoved == null)
                    {
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnItemMoved, false);
                    }
                }
            }

            event ToggleStateEventHandler ITree.StateToggled
            {
                add
                {
                    if (value != null)
                    {
                        myStateToggled += value;
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnStateToggled, true);
                    }
                }
                remove
                {
                    myStateToggled -= value;
                    if (myStateToggled == null)
                    {
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnStateToggled, false);
                    }
                }
            }

            event DisplayDataChangedEventHandler ITree.OnDisplayDataChanged
            {
                add
                {
                    if (value != null)
                    {
                        myOnDisplayDataChanged += value;
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnOnDisplayDataChanged, true);
                    }
                }
                remove
                {
                    myOnDisplayDataChanged -= value;
                    if (myOnDisplayDataChanged == null)
                    {
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnOnDisplayDataChanged, false);
                    }
                }
            }

            event SetRedrawEventHandler ITree.OnSetRedraw
            {
                add
                {
                    if (value != null)
                    {
                        myOnSetRedraw += value;
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnOnSetRedraw, true);
                    }
                }
                remove
                {
                    myOnSetRedraw -= value;
                    if (myOnSetRedraw == null)
                    {
                        myParent.SetStateFlag(TreeStateFlags.FireSingleColumnOnSetRedraw, false);
                    }
                }
            }

            #endregion // Informational ITree methods and events

            #region Pass through methods and events

            IBranch ITree.Root
            {
                get { return (myParent as ITree).Root; }
                set { (myParent as ITree).Root = value; }
            }

            void ITree.Realign(IBranch branch)
            {
                (myParent as ITree).Realign(branch);
            }

            void ITree.InsertItems(IBranch branch, int after, int count)
            {
                (myParent as ITree).InsertItems(branch, after, count);
            }

            void ITree.DeleteItems(IBranch branch, int start, int count)
            {
                (myParent as ITree).DeleteItems(branch, start, count);
            }

            void ITree.MoveItem(IBranch branch, int fromRow, int toRow)
            {
                (myParent as ITree).MoveItem(branch, fromRow, toRow);
            }

            void ITree.Refresh()
            {
                (myParent as ITree).Refresh();
            }

            void ITree.ShiftBranchLevels(ShiftBranchLevelsData shiftData)
            {
                (myParent as ITree).ShiftBranchLevels(shiftData);
            }

            bool ITree.Redraw
            {
                get { return (myParent as ITree).Redraw; }
                set { (myParent as ITree).Redraw = value; }
            }

            bool ITree.DelayRedraw
            {
                get { return (myParent as ITree).DelayRedraw; }
                set { (myParent as ITree).DelayRedraw = value; }
            }

            bool ITree.ListShuffle
            {
                get { return (myParent as ITree).ListShuffle; }
                set { (myParent as ITree).ListShuffle = value; }
            }

            bool ITree.DelayListShuffle
            {
                get { return (myParent as ITree).DelayListShuffle; }
                set { (myParent as ITree).DelayListShuffle = value; }
            }

            void ITree.DisplayDataChanged(DisplayDataChangedData changeData)
            {
                (myParent as ITree).DisplayDataChanged(changeData);
            }

            event RefreshEventHandler ITree.OnRefresh
            {
                add { myParent.OnRefresh += value; }
                remove { myParent.OnRefresh -= value; }
            }

            event ListShuffleEventHandler ITree.ListShuffleBeginning
            {
                add { myParent.ListShuffleBeginning += value; }
                remove { myParent.ListShuffleBeginning -= value; }
            }

            event ListShuffleEventHandler ITree.ListShuffleEnding
            {
                add { myParent.ListShuffleEnding += value; }
                remove { myParent.ListShuffleEnding -= value; }
            }

            event SynchronizeStateEventHandler ITree.SynchronizationBeginning
            {
                add { Debug.Fail("Synchronization events not supported on the SingleColumnView."); }
                remove { Debug.Fail("Synchronization events not supported on the SingleColumnView."); }
            }

            event SynchronizeStateEventHandler ITree.SynchronizationEnding
            {
                add { Debug.Fail("Synchronization events not supported on the SingleColumnView."); }
                remove { Debug.Fail("Synchronization events not supported on the SingleColumnView."); }
            }

            #endregion // Pass through ITree methods and events
        }

        // class SingleColumnView

        #endregion
    }

    // class VirtualTree
}
