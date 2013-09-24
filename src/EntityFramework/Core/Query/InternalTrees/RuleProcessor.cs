// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // The RuleProcessor helps apply a set of rules to a query tree
    // </summary>
    internal class RuleProcessor
    {
        #region private state

        // <summary>
        // A lookup table for rules.
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        private readonly Dictionary<SubTreeId, SubTreeId> m_processedNodeMap;

        #endregion

        #region constructors

        // <summary>
        // Initializes a new RuleProcessor
        // </summary>
        internal RuleProcessor()
        {
            // Build up the accelerator tables
            m_processedNodeMap = new Dictionary<SubTreeId, SubTreeId>();
        }

        #endregion

        #region private methods

        private static bool ApplyRulesToNode(
            RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<Rule>> rules, Node currentNode, out Node newNode)
        {
            newNode = currentNode;

            // Apply any pre-rule delegates
            context.PreProcess(currentNode);

            foreach (var r in rules[(int)currentNode.Op.OpType])
            {
                if (!r.Match(currentNode))
                {
                    continue;
                }

                // Did the rule modify the subtree?
                if (r.Apply(context, currentNode, out newNode))
                {
                    // The node has changed; don't try to apply any more rules
                    context.PostProcess(newNode, r);
                    return true;
                }
                else
                {
                    Debug.Assert(newNode == currentNode, "Liar! This rule should have returned 'true'");
                }
            }

            context.PostProcess(currentNode, null);
            return false;
        }

        // <summary>
        // Apply rules to the current subtree in a bottom-up fashion.
        // </summary>
        // <param name="context"> Current rule processing context </param>
        // <param name="rules"> The look-up table with the rules to be applied </param>
        // <param name="subTreeRoot"> Current subtree </param>
        // <param name="parent"> Parent node </param>
        // <param name="childIndexInParent"> Index of this child within the parent </param>
        // <returns> the result of the transformation </returns>
        private Node ApplyRulesToSubtree(
            RuleProcessingContext context,
            ReadOnlyCollection<ReadOnlyCollection<Rule>> rules,
            Node subTreeRoot, Node parent, int childIndexInParent)
        {
            var loopCount = 0;
            var localProcessedMap = new Dictionary<SubTreeId, SubTreeId>();
            SubTreeId subTreeId;

            while (true)
            {
                // Am I looping forever
                Debug.Assert(loopCount < 12, "endless loops?");
                loopCount++;

                //
                // We may need to update state regardless of whether this subTree has 
                // changed after it has been processed last. For example, it may be 
                // affected by transformation in its siblings due to external references.
                //
                context.PreProcessSubTree(subTreeRoot);
                subTreeId = new SubTreeId(context, subTreeRoot, parent, childIndexInParent);

                // Have I seen this subtree already? Just return, if so
                if (m_processedNodeMap.ContainsKey(subTreeId))
                {
                    break;
                }

                // Avoid endless loops here - avoid cycles of 2 or more
                if (localProcessedMap.ContainsKey(subTreeId))
                {
                    // mark this subtree as processed
                    m_processedNodeMap[subTreeId] = subTreeId;
                    break;
                }
                // Keep track of this one
                localProcessedMap[subTreeId] = subTreeId;

                // Walk my children
                for (var i = 0; i < subTreeRoot.Children.Count; i++)
                {
                    var childNode = subTreeRoot.Children[i];
                    if (ShouldApplyRules(childNode, subTreeRoot))
                    {
                        subTreeRoot.Children[i] = ApplyRulesToSubtree(context, rules, childNode, subTreeRoot, i);
                    }
                }

                // Apply rules to myself. If no transformations were performed, 
                // then mark this subtree as processed, and break out
                Node newSubTreeRoot;
                if (!ApplyRulesToNode(context, rules, subTreeRoot, out newSubTreeRoot))
                {
                    Debug.Assert(subTreeRoot == newSubTreeRoot);
                    // mark this subtree as processed
                    m_processedNodeMap[subTreeId] = subTreeId;
                    break;
                }
                context.PostProcessSubTree(subTreeRoot);
                subTreeRoot = newSubTreeRoot;
            }

            context.PostProcessSubTree(subTreeRoot);
            return subTreeRoot;
        }

        private static bool ShouldApplyRules(Node node, Node parent)
        {
            // For performance reasons skip the OpType.Constant child nodes of an OpType.In parent node.
            return parent.Op.OpType != OpType.In || node.Op.OpType != OpType.Constant;
        }

        #endregion

        #region public methods

        // <summary>
        // Apply a set of rules to the subtree
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="subTreeRoot"> current subtree </param>
        // <returns> transformed subtree </returns>
        internal Node ApplyRulesToSubtree(
            RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<Rule>> rules, Node subTreeRoot)
        {
            return ApplyRulesToSubtree(context, rules, subTreeRoot, null, 0);
        }

        #endregion
    }
}
