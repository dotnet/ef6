// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// A Rule - more specifically, a transformation rule - describes an action that is to
    /// be taken when a specific kind of subtree is found in the tree
    /// </summary>
    internal abstract class Rule
    {
        /// <summary>
        /// The "callback" function for each rule.
        /// Every callback function must return true if the subtree has
        /// been modified (or a new subtree has been returned); and must return false
        /// otherwise. If the root of the subtree has not changed, but some internal details
        /// of the subtree have changed, it is the responsibility of the rule to update any
        /// local bookkeeping information.
        /// </summary>
        /// <param name="context"> The rule processing context </param>
        /// <param name="subTree"> the subtree to operate on </param>
        /// <param name="newSubTree"> possibly transformed subtree </param>
        /// <returns> transformation status - true, if there was some change; false otherwise </returns>
        internal delegate bool ProcessNodeDelegate(RuleProcessingContext context, Node subTree, out Node newSubTree);

        #region private state

        private readonly ProcessNodeDelegate m_nodeDelegate;
        private readonly OpType m_opType;

        #endregion

        #region Constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="opType"> The OpType we're interested in processing </param>
        /// <param name="nodeProcessDelegate"> The callback to invoke </param>
        protected Rule(OpType opType, ProcessNodeDelegate nodeProcessDelegate)
        {
            DebugCheck.NotNull(nodeProcessDelegate);
            Debug.Assert(opType != OpType.NotValid, "bad OpType");
            Debug.Assert(opType != OpType.Leaf, "bad OpType - Leaf");

            m_opType = opType;
            m_nodeDelegate = nodeProcessDelegate;
        }

        #endregion

        #region protected methods

        #endregion

        #region public methods

        /// <summary>
        /// Does the rule match the current node?
        /// </summary>
        /// <param name="node"> the node in question </param>
        /// <returns> true, if a match was found </returns>
        internal abstract bool Match(Node node);

        /// <summary>
        /// We need to invoke the specified callback on the subtree in question - but only
        /// if the match succeeds
        /// </summary>
        /// <param name="ruleProcessingContext"> Current rule processing context </param>
        /// <param name="node"> The node (subtree) to process </param>
        /// <param name="newNode"> the (possibly) modified subtree </param>
        /// <returns> true, if the subtree was modified </returns>
        internal bool Apply(RuleProcessingContext ruleProcessingContext, Node node, out Node newNode)
        {
            // invoke the real callback
            return m_nodeDelegate(ruleProcessingContext, node, out newNode);
        }

        /// <summary>
        /// The OpType we're interested in transforming
        /// </summary>
        internal OpType RuleOpType
        {
            get { return m_opType; }
        }

#if DEBUG
    /// <summary>
    /// The method name for the rule
    /// </summary>
        internal string MethodName
        {
            get { return m_nodeDelegate.Method.Name; }
        }
#endif

        #endregion
    }
}
