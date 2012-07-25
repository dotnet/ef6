// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using WrapperBoolExpr = System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<LeftCellWrapper>;
    using WrapperTreeExpr = System.Data.Entity.Core.Common.Utils.Boolean.TreeExpr<LeftCellWrapper>;
    using WrapperAndExpr = System.Data.Entity.Core.Common.Utils.Boolean.AndExpr<LeftCellWrapper>;
    using WrapperOrExpr = System.Data.Entity.Core.Common.Utils.Boolean.OrExpr<LeftCellWrapper>;
    using WrapperNotExpr = System.Data.Entity.Core.Common.Utils.Boolean.NotExpr<LeftCellWrapper>;
    using WrapperTermExpr = System.Data.Entity.Core.Common.Utils.Boolean.TermExpr<LeftCellWrapper>;
    using WrapperTrueExpr = System.Data.Entity.Core.Common.Utils.Boolean.TrueExpr<LeftCellWrapper>;
    using WrapperFalseExpr = System.Data.Entity.Core.Common.Utils.Boolean.FalseExpr<LeftCellWrapper>;

    internal partial class CellTreeNode
    {
        #region Abstract Visitors

        // Abstract visitor implementation for Cell trees
        // TOutput is the return type of the visitor and TInput is a single
        // parameter that can be passed in
        internal abstract class CellTreeVisitor<TInput, TOutput>
        {
            internal abstract TOutput VisitLeaf(LeafCellTreeNode node, TInput param);
            internal abstract TOutput VisitUnion(OpCellTreeNode node, TInput param);
            internal abstract TOutput VisitInnerJoin(OpCellTreeNode node, TInput param);
            internal abstract TOutput VisitLeftOuterJoin(OpCellTreeNode node, TInput param);
            internal abstract TOutput VisitFullOuterJoin(OpCellTreeNode node, TInput param);
            internal abstract TOutput VisitLeftAntiSemiJoin(OpCellTreeNode node, TInput param);
        }

        // Another abstract visitor that does not distinguish between different
        // operation nodes
        internal abstract class SimpleCellTreeVisitor<TInput, TOutput>
        {
            internal abstract TOutput VisitLeaf(LeafCellTreeNode node, TInput param);
            internal abstract TOutput VisitOpNode(OpCellTreeNode node, TInput param);
        }

        #endregion

        #region Default CellTree Visitor

        // Default visitor implementation for CellTreeVisitor
        // TInput is the type of the parameter that can be passed in to each visit 
        // Returns a CellTreeVisitor as output
        private class DefaultCellTreeVisitor<TInput> : CellTreeVisitor<TInput, CellTreeNode>
        {
            internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, TInput param)
            {
                return node;
            }

            internal override CellTreeNode VisitUnion(OpCellTreeNode node, TInput param)
            {
                return AcceptChildren(node, param);
            }

            internal override CellTreeNode VisitInnerJoin(OpCellTreeNode node, TInput param)
            {
                return AcceptChildren(node, param);
            }

            internal override CellTreeNode VisitLeftOuterJoin(OpCellTreeNode node, TInput param)
            {
                return AcceptChildren(node, param);
            }

            internal override CellTreeNode VisitFullOuterJoin(OpCellTreeNode node, TInput param)
            {
                return AcceptChildren(node, param);
            }

            internal override CellTreeNode VisitLeftAntiSemiJoin(OpCellTreeNode node, TInput param)
            {
                return AcceptChildren(node, param);
            }

            private OpCellTreeNode AcceptChildren(OpCellTreeNode node, TInput param)
            {
                var newChildren = new List<CellTreeNode>();
                foreach (var child in node.Children)
                {
                    newChildren.Add(child.Accept(this, param));
                }
                return new OpCellTreeNode(node.ViewgenContext, node.OpType, newChildren);
            }
        }

        #endregion

        #region Flattening Visitor

        // Flattens the tree, i.e., pushes up nodes that just have just one child
        private class FlatteningVisitor : SimpleCellTreeVisitor<bool, CellTreeNode>
        {
            #region Constructor/Fields/Invocation

            protected FlatteningVisitor()
            {
            }

            // effects: Flattens node and returns a new tree that is flattened
            internal static CellTreeNode Flatten(CellTreeNode node)
            {
                var visitor = new FlatteningVisitor();
                return node.Accept(visitor, true);
            }

            #endregion

            #region Visitors

            internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, bool dummy)
            {
                return node;
            }

            // effects: Visits an internal Op node and processes it
            internal override CellTreeNode VisitOpNode(OpCellTreeNode node, bool dummy)
            {
                // Flatten the children first
                var flattenedChildren = new List<CellTreeNode>();
                foreach (var child in node.Children)
                {
                    var flattenedChild = child.Accept(this, dummy);
                    flattenedChildren.Add(flattenedChild);
                }

                Debug.Assert(flattenedChildren.Count >= 1, "node must have more than 1 child and be an OpCellTreeNode");
                // If only one child, return that
                if (flattenedChildren.Count == 1)
                {
                    return flattenedChildren[0];
                }

                Debug.Assert(flattenedChildren.Count > 1, "Opnode has 0 children?");
                Debug.Assert(node.OpType != CellTreeOpType.Leaf, "Wrong op type for operation node");

                var result = new OpCellTreeNode(node.ViewgenContext, node.OpType, flattenedChildren);
                return result;
            }

            #endregion
        }

        #endregion

        #region AssociativeOpFlatteningVisitor

        // Flattens associative ops and single children nodes. Like the
        // FlatteningVisitor, it gets rid of the single children
        // nodes. Furthermore, it also collapses nodes of associative operations,
        // i.e., A IJ (B IJ C) is changed to A IJ B IJ C
        private class AssociativeOpFlatteningVisitor : SimpleCellTreeVisitor<bool, CellTreeNode>
        {
            #region Constructor/Fields/Invocation

            private AssociativeOpFlatteningVisitor()
            {
            }

            internal static CellTreeNode Flatten(CellTreeNode node)
            {
                // First do simple flattening and then associative op flattening
                var newNode = FlatteningVisitor.Flatten(node);
                var visitor = new AssociativeOpFlatteningVisitor();
                return newNode.Accept(visitor, true);
            }

            #endregion

            #region Visitors

            internal override CellTreeNode VisitLeaf(LeafCellTreeNode node, bool dummy)
            {
                return node;
            }

            internal override CellTreeNode VisitOpNode(OpCellTreeNode node, bool dummy)
            {
                var flattenedChildren = new List<CellTreeNode>();
                // Flatten the children first
                foreach (var child in node.Children)
                {
                    var flattenedChild = child.Accept(this, dummy);
                    flattenedChildren.Add(flattenedChild);
                }

                Debug.Assert(flattenedChildren.Count > 1, "node must have more than 1 child and be an OpCellTreeNode");

                // If this op is associative and a child's OP is the same as this
                // op, add those to be this nodes children
                var finalChildren = flattenedChildren;
                if (IsAssociativeOp(node.OpType))
                {
                    finalChildren = new List<CellTreeNode>();
                    foreach (var child in flattenedChildren)
                    {
                        if (child.OpType
                            == node.OpType)
                        {
                            finalChildren.AddRange(child.Children);
                        }
                        else
                        {
                            finalChildren.Add(child);
                        }
                    }
                }

                var result = new OpCellTreeNode(node.ViewgenContext, node.OpType, finalChildren);
                return result;
            }

            #endregion
        }

        #endregion

        #region LeafVisitor

        // This visitor returns all the leaf tree nodes in this
        private class LeafVisitor : SimpleCellTreeVisitor<bool, IEnumerable<LeafCellTreeNode>>
        {
            private LeafVisitor()
            {
            }

            internal static IEnumerable<LeafCellTreeNode> GetLeaves(CellTreeNode node)
            {
                var visitor = new LeafVisitor();
                return node.Accept(visitor, true);
            }

            internal override IEnumerable<LeafCellTreeNode> VisitLeaf(LeafCellTreeNode node, bool dummy)
            {
                yield return node;
            }

            internal override IEnumerable<LeafCellTreeNode> VisitOpNode(OpCellTreeNode node, bool dummy)
            {
                foreach (var child in node.Children)
                {
                    var children = child.Accept(this, dummy);
                    foreach (var leafNode in children)
                    {
                        yield return leafNode;
                    }
                }
            }
        }

        #endregion
    }
}
