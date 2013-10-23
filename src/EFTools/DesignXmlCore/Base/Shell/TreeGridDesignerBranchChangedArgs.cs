// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    /// <summary>
    ///     Information about required branch modifications
    ///     It can be on of three modifications:
    ///     1. Add item to the branch (InsertItem == true)
    ///     2. Remove item form the branch (DeleteItem == true)
    ///     3. Item was changed (InsertItem == DeleteItem == false)
    ///     Row and Column indicates where the change should occur
    /// </summary>
    internal class TreeGridDesignerBranchChangedArgs
    {
        private int _row = -1;
        private int _column = -1;
        private bool _insertingItem;
        private bool _deletingItem;

        internal static TreeGridDesignerBranchChangedArgs InsertItemArgs
        {
            get
            {
                var args = new TreeGridDesignerBranchChangedArgs();
                args._insertingItem = true;
                return args;
            }
        }

        internal static TreeGridDesignerBranchChangedArgs DeleteItemArgs
        {
            get
            {
                var args = new TreeGridDesignerBranchChangedArgs();
                args._deletingItem = true;
                return args;
            }
        }

        internal int Row
        {
            get { return _row; }
            set { _row = value; }
        }

        internal int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        internal bool InsertingItem
        {
            get { return _insertingItem; }
        }

        internal bool DeletingItem
        {
            get { return _deletingItem; }
        }
    }
}
