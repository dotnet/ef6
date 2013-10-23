// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Data for modifying the relative level of items below a given branch
    /// </summary>
    internal struct ShiftBranchLevelsData
    {
        private readonly IBranch myBranch;
        private readonly int myRemoveLevels;
        private readonly int myInsertLevels;
        private readonly int myDepth;
        private readonly IBranch myReplacementBranch;
        private readonly ILevelShiftAdjuster myBranchTester;
        private readonly int myStartIndex;
        private readonly int myCount;
        private readonly int myNewCount;

        /// <summary>
        ///     For all expansions of 'branch', first remove 'removeLevels' then insert 'insertLevels' branches from
        ///     the immediate children of the given branch.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="removeLevels">The number of levels to remove</param>
        /// <param name="insertLevels">The number of levels to insert</param>
        public ShiftBranchLevelsData(IBranch branch, int removeLevels, int insertLevels)
            :
                this(branch, removeLevels, insertLevels, 0, null, null, -1, -1, -1)
        {
        }

        /// <summary>
        ///     For all expansions of 'branch', first remove 'removeLevels' then insert 'insertLevels' branches from
        ///     all branches 'depth' levels below the current branch. If depth>0, then a new VisibleItemCount
        ///     is retrieved from the branch and all sub branches are relocated.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="removeLevels">The number of levels to remove</param>
        /// <param name="insertLevels">The number of levels to insert</param>
        /// <param name="depth">The number of levels below the starting branch to skip before removing and inserting levels</param>
        public ShiftBranchLevelsData(IBranch branch, int removeLevels, int insertLevels, int depth)
            :
                this(branch, removeLevels, insertLevels, depth, null, null, -1, -1, -1)
        {
        }

        /// <summary>
        ///     For all expansions of 'branch', first remove 'removeLevels' then insert 'insertLevels' branches from
        ///     all branches 'depth' levels below the current branch. Replace the tree's reference to the current
        ///     branch object with a reference to 'replacementBranch' (if set). If depth>0, then a new VisibleItemCount
        ///     is retrieved from the branch and all sub branches are relocated.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="removeLevels">The number of levels to remove</param>
        /// <param name="insertLevels">The number of levels to insert</param>
        /// <param name="depth">The number of levels below the starting branch to skip before removing and inserting levels</param>
        /// <param name="replacementBranch">Replace the starting branch with this one</param>
        public ShiftBranchLevelsData(IBranch branch, int removeLevels, int insertLevels, int depth, IBranch replacementBranch)
            :
                this(branch, removeLevels, insertLevels, depth, replacementBranch, null, -1, -1, -1)
        {
        }

        /// <summary>
        ///     For all expansions of 'branch', first remove 'removeLevels' then insert 'insertLevels' branches from
        ///     all branches 'depth' levels below the current branch. Replace the tree's reference to the current
        ///     branch object with a reference to 'replacementBranch' (if set). If depth>0, then a new VisibleItemCount
        ///     is retrieved from the branch and all sub branches are relocated. If 'branchTester' is not null, then
        ///     it is used to adjust the operation as described in the ILevelShiftAdjuster interface.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="removeLevels">The number of levels to remove</param>
        /// <param name="insertLevels">The number of levels to insert</param>
        /// <param name="depth">The number of levels below the starting branch to skip before removing and inserting levels</param>
        /// <param name="replacementBranch">Replace the starting branch with this one</param>
        /// <param name="branchTester">A callback interface for advanced level shifting</param>
        public ShiftBranchLevelsData(
            IBranch branch, int removeLevels, int insertLevels, int depth, IBranch replacementBranch, ILevelShiftAdjuster branchTester)
            :
                this(branch, removeLevels, insertLevels, depth, replacementBranch, branchTester, -1, -1, -1)
        {
        }

        /// <summary>
        ///     For all expansions of 'branch', first remove 'removeLevels' then insert 'insertLevels' branches from
        ///     all branches 'depth' levels below the current branch. Replace the tree's reference to the current
        ///     branch object with a reference to 'replacementBranch' (if set). The operation affects 'count' items
        ///     at position 'startIndex', which are replaced with 'newCount' new items. If depth>0, then a new VisibleItemCount
        ///     is retrieved from the branch and all sub branches are relocated. StartIndex/Count/NewCount affect only the passed in branch.
        ///     If Start/Count/NewCount are used with a replacement branch, then the replacement branch must be
        ///     consistent with the original. If 'branchTester' is not null, then it is used to adjust the operation
        ///     as described in the ILevelShiftAdjuster interface.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="removeLevels">The number of levels to remove</param>
        /// <param name="insertLevels">The number of levels to insert</param>
        /// <param name="depth">The number of levels below the starting branch to skip before removing and inserting levels</param>
        /// <param name="replacementBranch">Replace the starting branch with this one</param>
        /// <param name="branchTester">A callback interface for advanced level shifting</param>
        /// <param name="startIndex">The starting index of the items being adjusted in the starting branch</param>
        /// <param name="count">The original count for the number of items being adjusted in the starting branch</param>
        /// <param name="newCount">The replacement count for the number of items being adjusted in the starting branch</param>
        public ShiftBranchLevelsData(
            IBranch branch, int removeLevels, int insertLevels, int depth, IBranch replacementBranch, ILevelShiftAdjuster branchTester,
            int startIndex, int count, int newCount)
        {
            myBranch = branch;
            myRemoveLevels = removeLevels;
            myInsertLevels = insertLevels;
            myDepth = depth;
            myReplacementBranch = replacementBranch;
            myBranchTester = branchTester;
            myStartIndex = startIndex;
            myCount = count;
            myNewCount = newCount;
        }

        /// <summary>
        ///     The branch being adjusted
        /// </summary>
        public IBranch Branch
        {
            get { return myBranch; }
        }

        /// <summary>
        ///     The number of levels to remove
        /// </summary>
        public int RemoveLevels
        {
            get { return myRemoveLevels; }
        }

        /// <summary>
        ///     The number of levels to insert
        /// </summary>
        public int InsertLevels
        {
            get { return myInsertLevels; }
        }

        /// <summary>
        ///     The number of levels below the starting branch to skip before removing and inserting levels
        /// </summary>
        public int Depth
        {
            get { return myDepth; }
        }

        /// <summary>
        ///     A branch to replace the starting branch
        /// </summary>
        /// <value></value>
        public IBranch ReplacementBranch
        {
            get { return myReplacementBranch; }
        }

        /// <summary>
        ///     A callback interface for advanced level shifting.
        /// </summary>
        /// <value>ILevelShiftAdjuster</value>
        public ILevelShiftAdjuster BranchTester
        {
            get { return myBranchTester; }
        }

        /// <summary>
        ///     The starting index of the items to be adjusted in the starting branch, or -1 if the entire branch is being adjusted. Set along with Count and NewCount.
        /// </summary>
        public int StartIndex
        {
            get { return myStartIndex; }
        }

        /// <summary>
        ///     The starting count for the number of items being adjusted in the starting branch, or -1 if the entire branch is being adjusted. Set along with StartIndex and NewCount.
        /// </summary>
        public int Count
        {
            get { return myCount; }
        }

        /// <summary>
        ///     The replacement count for the number of items being adjusted in the starting branch, or -1 if the entire branch is being adjusted. Set along with StartIndex and Count.
        /// </summary>
        public int NewCount
        {
            get { return myNewCount; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare two ShiftBranchLevelsData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(ShiftBranchLevelsData operand1, ShiftBranchLevelsData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two ShiftBranchLevelsData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare two ShiftBranchLevelsData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(ShiftBranchLevelsData operand1, ShiftBranchLevelsData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare two ShiftBranchLevelsData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(ShiftBranchLevelsData operand1, ShiftBranchLevelsData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }
}
