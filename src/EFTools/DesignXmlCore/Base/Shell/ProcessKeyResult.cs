// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Structure filled out by a branch when a key is pressed.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    internal struct ProcessKeyResult
    {
        private const int
            EXPAND_BRANCH = 2,
            LOCAL = 4,
            EDIT_MODE = 8,
            DELETE = 16;

        private NavigationDirection _treeNav;
        private KeyAction _action;
        private Type _columnType;
        private Type _branchType;
        private int _flags; // bitwise combination of constants above

        /// <summary>
        ///     Construct a ProcessKeyResult structure with the given action
        /// </summary>
        /// <param name="action"></param>
        internal ProcessKeyResult(KeyAction action)
        {
            _columnType = null;
            _branchType = null;
            _flags = LOCAL;
            _treeNav = NavigationDirection.Right;
            _action = action;
        }

        /// <summary>
        ///     Specifies the resulting action that should be taken in response to
        ///     a key press.
        /// </summary>
        internal KeyAction Action
        {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        ///     Next column to set focus to.  if this is null and KeyAction is Handled,
        ///     the control will determine the next editable
        ///     column and set focus to it.
        /// </summary>
        internal Type ColumnType
        {
            get { return _columnType; }
            set { _columnType = value; }
        }

        /// <summary>
        ///     Type of branch to set focus to.  If this is non-null,
        ///     it will be processed first, then ColumnType
        /// </summary>
        internal Type BranchType
        {
            get { return _branchType; }
            set { _branchType = value; }
        }

        /// <summary>
        ///     True iff the search should proceed forward
        /// </summary>
        internal NavigationDirection Direction
        {
            get { return _treeNav; }
            set { _treeNav = value; }
        }

        /// <summary>
        ///     True iff current branch should be expanded
        ///     before searching.
        /// </summary>
        internal bool ExpandBranch
        {
            get { return GetFlag(EXPAND_BRANCH); }
            set { SetFlag(EXPAND_BRANCH, value); }
        }

        /// <summary>
        ///     True iff the search should stop at the
        ///     current branch.
        /// </summary>
        internal bool Local
        {
            get { return GetFlag(LOCAL); }
            set { SetFlag(LOCAL, value); }
        }

        /// <summary>
        ///     True iff the cell should be placed in edit mode
        /// </summary>
        internal bool StartLabelEdit
        {
            get { return GetFlag(EDIT_MODE); }
            set { SetFlag(EDIT_MODE, value); }
        }

        internal bool Delete
        {
            get { return GetFlag(DELETE); }
            set { SetFlag(DELETE, value); }
        }

        private bool GetFlag(int flagValue)
        {
            return (_flags & flagValue) != 0;
        }

        private void SetFlag(int flagValue, bool value)
        {
            if (value)
            {
                _flags |= flagValue;
            }
            else
            {
                _flags &= ~flagValue;
            }
        }
    }
}
