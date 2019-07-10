// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{

    #region ILevelShiftAdjuster definition

    /// <summary>
    ///     Advanced interface for making complex changes during a level shift operation.
    ///     Justification for ILevelShiftAdjuster interface:
    ///     The ShiftBranchLevels method works very well if the structure
    ///     of all branches below the given object have the same branching
    ///     structure. However, this is an idealized situation. The intent
    ///     of ShiftBranchLevels is to make it easy to add and remove multiple
    ///     levels of header groups over the same set of data. For the 'Basic Sample'
    ///     data shown below, the 'Real' nodes are the actual data, and the rest
    ///     of the nodes are headers. If the goal is to remove the Type 2 headers for
    ///     this tree, then a ShiftBranchLevels(Depth = 1, Remove = 2, Insert = 1)
    ///     will collapse the type 2 headers and create once set of Type3 headers while
    ///     maintaining the sub expansion states of the Type3 Header nodes.
    ///     *****Basic Sample*****
    ///     FirstType1Header
    ///     FirstType2Header
    ///     SecondType2Header
    ///     FirstType3Header
    ///     RealNodeX
    ///     SecondType1Header
    ///     FirstType2Header
    ///     FirstType3Header
    ///     RealNodeZ
    ///     SecondType2Header
    ///     FirstType3Header
    ///     SecondType3Header
    ///     RealNodeW
    ///     The advanced sample shown below is a less idealized state. There are
    ///     two concepts added here:
    ///     1) A SubSet node is inserted between branching levels. In this branch,
    ///     removing Type2 headers requires operating on a depth 2 levels from the
    ///     top, not the requested one. Level shifting should provide an easy mechanism
    ///     to skip the subset node altogether while it walks down to the desired depth,
    ///     or to adjust the depth (plus or minus) that insert and remove operations
    ///     should take place at. For example, the SubSetNode would increase the Depth
    ///     for this part of the tree by one level.
    ///     2) The Type2NeutralRealNodeY represents real data that cannot be categorized
    ///     under any of the headers at this level. The 'Remove 2 levels and insert 1'
    ///     at this point would blow away any expansion on this node. However, when the
    ///     Type3 headers are put back in place, this item will fit naturally at either
    ///     the Type3 header level, or underneath one of the Type3 headers from the replacement
    ///     list. In other words, the branch coming off of Type2NeutralRealNodeY should be treated
    ///     the same as the branches at the depth specified at shift branch levels. These branches
    ///     are collected then reattached at the appropriate level using IBranch.LocateExpandedList.
    ///     There needs to be a way to place the RealNodeY branch into this 'reattach' list.
    ///     *****Advanced Sample*****
    ///     FirstType1Header
    ///     SubSetNode
    ///     FirstType2Header
    ///     SecondType2Header
    ///     FirstType3Header
    ///     RealNodeX
    ///     SecondType1Header
    ///     Type2NeutralRealNodeY
    ///     RealNodeYChild1
    ///     FirstType2Header
    ///     FirstType3Header
    ///     RealNodeZ
    ///     SecondType2Header
    ///     FirstType3Header
    ///     SecondType3Header
    ///     RealNodeW
    /// </summary>
    internal interface ILevelShiftAdjuster
    {
        /// <summary>
        ///     Allow a branch to be skipped during a level shift operation, or
        ///     reattached at a custom depth.
        /// </summary>
        /// <param name="branch">The branch being shifted</param>
        /// <returns>ValidateAdjustDepthResult structure. Continue should be true to keep processing this branch.</returns>
        ValidateAdjustDepthResult ValidateAdjustDepth(IBranch branch);

        /// <summary>
        ///     TestReattachBranch will be called for all branches in the kill zones, meaning all
        ///     branches between depth and (depth + removeLevels - 1) levels. Returning
        ///     a result other than Discard will treat the branch as a child that needs to be reattached.
        ///     LocateObject should return KeepBranchAtThisLevel to place this
        ///     branch at a location other than the original location.
        /// </summary>
        /// <param name="branch">The branch being adjusted</param>
        /// <returns>See TestReattachBranchResult values</returns>
        TestReattachBranchResult TestReattachBranch(IBranch branch);

        /// <summary>
        ///     TestGetNewBranch will be called for branches at all depths above the kill
        ///     zone. Returning true will force a new branch object to replace the given one.
        ///     Returning false will have no effect.
        /// </summary>
        /// <param name="branch">The branch to test</param>
        /// <returns>true to retrieve a new branch</returns>
        bool TestGetNewBranch(IBranch branch);
    }

    #endregion

    #region TestReattachBranchResult Enum

    /// <remarks>
    ///     Return codes for the ILevelShiftAdjuster.TestReattachBranch method.
    ///     The results allow any branch in the kill zone to be discarded (the default
    ///     if ILevelShiftAdjuster is not specified), or added to the reattach list for
    ///     processing when the branch structure is put back together. The other options
    ///     specify what needs to be done if the branch is kept.
    /// </remarks>
    internal enum TestReattachBranchResult
    {
        /// <summary>
        ///     The branch is discarded and its children (below the kill zone) are
        ///     reattached (to new branches, if necessary).
        /// </summary>
        Discard,

        /// <summary>
        ///     The branch is kept with its child branches and tracking objects
        ///     are not included in the reattach phase.
        /// </summary>
        ReattachIntact,

        /// <summary>
        ///     The child branches and tracking objects are detached from the parent,
        ///     but the parent branch is not discarded, and maintains its expansion state.
        /// </summary>
        ReattachChildren,

        /// <summary>
        ///     Includes behavior of ReattachChildren. The child branches and
        ///     tracking objects are detached and the item count of the branch is required.
        ///     This is very similar to a realign on the branch,
        ///     except that the existing child branches can potentially end up
        ///     reattached to other branches, with is not possible with an ITree.Realign call.
        /// </summary>
        Realign,
    }

    #endregion // TestReattachBranchResult Enum

    #region ValidateAdjustDepthResult struct

    /// <summary>
    ///     The result of an ILevelShiftAdjuster.ValidateAdjustDepth call
    /// </summary>
    internal struct ValidateAdjustDepthResult
    {
        private readonly bool myContinue;
        private readonly int myDepthAdjustment;

        /// <summary>
        ///     Continue processing this branch without any depth adjustment
        /// </summary>
        public static readonly ValidateAdjustDepthResult ContinueProcessing = new ValidateAdjustDepthResult(true);

        /// <summary>
        ///     Stop processing this branch
        /// </summary>
        public static readonly ValidateAdjustDepthResult StopProcessing = new ValidateAdjustDepthResult(false);

        /// <summary>
        ///     Continue processing the branch but adjust the depth
        /// </summary>
        /// <param name="depthAdjustment">The number of levels to shift the branch</param>
        /// <returns>A new result structure</returns>
        public static ValidateAdjustDepthResult AdjustDepth(int depthAdjustment)
        {
            return new ValidateAdjustDepthResult(depthAdjustment);
        }

        private ValidateAdjustDepthResult(bool continueProcessing)
        {
            myContinue = continueProcessing;
            myDepthAdjustment = 0;
        }

        private ValidateAdjustDepthResult(int depthAdjustment)
        {
            myContinue = true;
            myDepthAdjustment = depthAdjustment;
        }

        /// <summary>
        ///     Continue processing the branch if true
        /// </summary>
        public bool Continue
        {
            get { return myContinue; }
        }

        /// <summary>
        ///     Adjust the branch depth by this value
        /// </summary>
        public int DepthAdjustment
        {
            get { return myDepthAdjustment; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is ValidateAdjustDepthResult)
            {
                return Compare(this, (ValidateAdjustDepthResult)obj);
            }
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
        /// <returns></returns>
        public static bool operator ==(ValidateAdjustDepthResult operand1, ValidateAdjustDepthResult operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two ValidateAdjustDepthResult structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(ValidateAdjustDepthResult operand1, ValidateAdjustDepthResult operand2)
        {
            return operand1.myContinue == operand2.myContinue && operand1.myDepthAdjustment == operand2.myDepthAdjustment;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(ValidateAdjustDepthResult operand1, ValidateAdjustDepthResult operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }

    #endregion
}
