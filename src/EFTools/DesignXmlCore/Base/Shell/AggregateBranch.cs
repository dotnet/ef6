// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     Class that aggregates a set of branches and displays them as a single
    ///     branch in the tree
    /// </summary>
    internal sealed class AggregateBranch : IBranch, IMultiColumnBranch, ITreeGridDesignerBranch
    {
        private readonly ArrayList _branchList;
        private readonly IBranch _primaryBranch;

        #region Construction

        /// <summary>
        ///     Creates an AggregateBranch
        /// </summary>
        /// <param name="branchList">List of branches to aggregate.  All contained objects must implement IBranch</param>
        /// <param name="primaryBranchIndex">
        ///     Index of the primary branch in the list.  The primary branch
        ///     is used to determine branch-wide properties such as branch flags.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        internal AggregateBranch(IList branchList, int primaryBranchIndex)
        {
            if (branchList == null)
            {
                throw new ArgumentNullException("branchList");
            }

            _primaryBranch = (IBranch)branchList[primaryBranchIndex];

            _branchList = new ArrayList(branchList.Count);
            foreach (var obj in branchList)
            {
                var branch = obj as IBranch;
                if (branch != null)
                {
                    _branchList.Add(branch);
                    branch.OnBranchModification += OnInnerBranchModification;
                }
                else
                {
                    throw new ArgumentException("branchList");
                }
            }
        }

        #endregion

        #region IBranch

        BranchFeatures IBranch.Features
        {
            get { return _primaryBranch.Features; }
        }

        /// <summary>
        ///     IBranch interface implementation.
        /// </summary>
        public int /* IBranch */ VisibleItemCount
        {
            get
            {
                var visibleItemCount = 0;

                foreach (IBranch branch in _branchList)
                {
                    visibleItemCount += branch.VisibleItemCount;
                }

                return visibleItemCount;
            }
        }

        object IBranch.GetObject(int row, int column, ObjectStyle style, ref int options)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetObject(row, column, style, ref options);
        }

        LocateObjectData IBranch.LocateObject(object obj, ObjectStyle style, int locateOptions)
        {
            var locateData = new LocateObjectData();
            locateData.Row = -1;
            foreach (IBranch branch in _branchList)
            {
                locateData = branch.LocateObject(obj, style, locateOptions);
                if (locateData.Row != -1)
                {
                    return locateData;
                }
            }

            return locateData;
        }

        string IBranch.GetText(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetText(row, column);
        }

        string IBranch.GetTipText(int row, int column, ToolTipType tipType)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetTipText(row, column, tipType);
        }

        bool IBranch.IsExpandable(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.IsExpandable(row, column);
        }

        VirtualTreeDisplayData IBranch.GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetDisplayData(row, column, requiredData);
        }

        VirtualTreeAccessibilityData IBranch.GetAccessibilityData(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetAccessibilityData(row, column);
        }

        string IBranch.GetAccessibleName(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetAccessibleName(row, column);
        }

        string IBranch.GetAccessibleValue(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.GetAccessibleValue(row, column);
        }

        VirtualTreeLabelEditData IBranch.BeginLabelEdit(int row, int column, VirtualTreeLabelEditActivationStyles activationStyle)
        {
            var branch = FindBranchForRow(ref row);
            return branch.BeginLabelEdit(row, column, activationStyle);
        }

        LabelEditResult IBranch.CommitLabelEdit(int row, int column, string newText)
        {
            var branch = FindBranchForRow(ref row);
            return branch.CommitLabelEdit(row, column, newText);
        }

        int IBranch.UpdateCounter
        {
            get { return _primaryBranch.UpdateCounter; }
        }

        StateRefreshChanges IBranch.ToggleState(int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            return branch.ToggleState(row, column);
        }

        StateRefreshChanges IBranch.SynchronizeState(int row, int column, IBranch matchBranch, int matchRow, int matchColumn)
        {
            var branch = FindBranchForRow(ref row);
            return branch.SynchronizeState(row, column, matchBranch, matchRow, matchColumn);
        }

        void IBranch.OnDragEvent(object sender, int row, int column, DragEventType eventType, DragEventArgs args)
        {
            var branch = FindBranchForRow(ref row);
            branch.OnDragEvent(sender, row, column, eventType, args);
        }

        VirtualTreeStartDragData IBranch.OnStartDrag(object sender, int row, int column, DragReason reason)
        {
            var branch = FindBranchForRow(ref row);
            return branch.OnStartDrag(sender, row, column, reason);
        }

        void IBranch.OnGiveFeedback(GiveFeedbackEventArgs args, int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            branch.OnGiveFeedback(args, row, column);
        }

        void IBranch.OnQueryContinueDrag(QueryContinueDragEventArgs args, int row, int column)
        {
            var branch = FindBranchForRow(ref row);
            branch.OnQueryContinueDrag(args, row, column);
        }

        /// <summary>
        ///     Fired when one of the branches we are aggregating is modified
        /// </summary>
        public event BranchModificationEventHandler OnBranchModification;

        #endregion

        #region IMultiColumnBranch

        /// <summary>
        ///     IMultiColumnBranch interface implementation.
        /// </summary>
        public int /* IMultiColumnBranch */ ColumnCount
        {
            get
            {
                if (_primaryBranch is IMultiColumnBranch)
                {
                    return ((IMultiColumnBranch)_primaryBranch).ColumnCount;
                }

                return 1;
            }
        }

        SubItemCellStyles IMultiColumnBranch.ColumnStyles(int column)
        {
            if (_primaryBranch is IMultiColumnBranch)
            {
                return ((IMultiColumnBranch)_primaryBranch).ColumnStyles(column);
            }

            return SubItemCellStyles.Simple;
        }

        /// <summary>
        ///     Returns the number of columns for a specific row. This
        ///     is only called if the IBranch.TreeFlags include TreeFlags.JaggedColumns.
        ///     The number return must be in the range {1,..., ColumnCount}
        /// </summary>
        /// <param name="row">The row to get a column count for</param>
        /// <returns>The number of columns on this row</returns>
        public int GetJaggedColumnCount(int row)
        {
            var branch = FindBranchForRow(ref row) as IMultiColumnBranch;
            if (branch != null)
            {
                return branch.GetJaggedColumnCount(row);
            }

            return 1;
        }

        #endregion

        #region ITreeGridDesignerBranch

        ProcessKeyResult ITreeGridDesignerBranch.ProcessKeyPress(int row, int column, char keyPressed, Keys modifiers)
        {
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                return branch.ProcessKeyPress(row, column, keyPressed, modifiers);
            }

            return new ProcessKeyResult();
        }

        ProcessKeyResult ITreeGridDesignerBranch.ProcessKeyDown(int row, int column, KeyEventArgs e)
        {
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                return branch.ProcessKeyDown(row, column, e);
            }

            return new ProcessKeyResult();
        }

        TreeGridDesignerValueSupportedStates ITreeGridDesignerBranch.GetValueSupported(int row, int column)
        {
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                return branch.GetValueSupported(row, column);
            }

            return TreeGridDesignerValueSupportedStates.Unsupported;
        }

        /// <summary>
        ///     Gets/sets read-only state of this branch.  Text in read-only branches appears grayed-out
        ///     and cannot be edited.  Creator nodes also do not appear.
        /// </summary>
        bool ITreeGridDesignerBranch.ReadOnly { get; set; }

        CommandID ITreeGridDesignerBranch.GetDefaultAction(int index)
        {
            var branch = FindBranchForRow(ref index) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                return branch.GetDefaultAction(index);
            }

            return null;
        }

        void ITreeGridDesignerBranch.InsertCreatorNode(int index, int creatorNodeIndex)
        {
            var branch = FindBranchForRow(ref index) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                branch.InsertCreatorNode(index, creatorNodeIndex);
            }
        }

        void ITreeGridDesignerBranch.EndInsert(int row)
        {
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                branch.EndInsert(row);
            }
        }

        object ITreeGridDesignerBranch.GetBranchComponent()
        {
            // for aggregates branches, return the component used to initialize the first branch
            if (_branchList.Count > 0)
            {
                var branch = _branchList[0] as ITreeGridDesignerBranch;
                if (branch != null)
                {
                    return branch.GetBranchComponent();
                }
            }

            return null;
        }

        void ITreeGridDesignerBranch.Delete(int row, int column)
        {
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                branch.Delete(row, column);
            }
        }

        #endregion

        private IBranch FindBranchForRow(ref int row)
        {
            var count = 0;
            var originalRow = row;

            foreach (IBranch branch in _branchList)
            {
                var visibleItems = branch.VisibleItemCount;
                count += visibleItems;

                if (originalRow < count)
                {
                    return branch;
                }

                row -= visibleItems;
            }

            throw new ArgumentOutOfRangeException("row");
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private int FindIndexForBranch(IBranch innerBranch, int index)
        {
            var outerIndex = index;
            foreach (IBranch branch in _branchList)
            {
                if (branch == innerBranch)
                {
                    return outerIndex;
                }

                outerIndex += branch.VisibleItemCount;
            }

            throw new ArgumentException("innerBranch");
        }

        private void OnInnerBranchModification(object sender, BranchModificationEventArgs args)
        {
            if (OnBranchModification != null)
            {
                args.Index = FindIndexForBranch(args.Branch, args.Index);
                args.Branch = this;
                OnBranchModification(this, args);
            }
        }

        public void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args)
        {
            var row = args.Row;
            var branch = FindBranchForRow(ref row) as ITreeGridDesignerBranch;
            if (branch != null)
            {
                branch.OnColumnValueChanged(args);
            }
        }
    }
}
