// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // A visitor that collects all group aggregates and the corresponding function aggregates
    // that are defined over them, referred to as 'candidate aggregates'. The candidate aggregates are aggregates
    // that have an argument that has the corresponding group aggregate as the only external reference
    // </summary>
    internal class GroupAggregateRefComputingVisitor : BasicOpVisitor
    {
        #region private state

        private readonly Command _command;
        private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager = new GroupAggregateVarInfoManager();
        private readonly Dictionary<Node, Node> _childToParent = new Dictionary<Node, Node>();

        #endregion

        #region 'Public'

        // <summary>
        // Produces a list of all GroupAggregateVarInfos, each of which represents a single group aggregate
        // and it candidate function aggregates. It also produces a delegate that given a child node returns the parent node
        // </summary>
        internal static IEnumerable<GroupAggregateVarInfo> Process(Command itree, out TryGetValue tryGetParent)
        {
            var groupRefComputingVisitor = new GroupAggregateRefComputingVisitor(itree);
            groupRefComputingVisitor.VisitNode(itree.Root);
            tryGetParent = groupRefComputingVisitor._childToParent.TryGetValue;

            return groupRefComputingVisitor._groupAggregateVarInfoManager.GroupAggregateVarInfos;
        }

        #endregion

        #region Private Constructor

        // <summary>
        // Private constructor
        // </summary>
        private GroupAggregateRefComputingVisitor(Command itree)
        {
            _command = itree;
        }

        #endregion

        #region Visitor Methods

        #region AncillaryOps

        // <summary>
        // Determines whether the var or a property of the var (if the var is defined as a NewRecord)
        // is defined exclusively over a single group aggregate. If so, it registers it as such with the
        // group aggregate var info manager.
        // </summary>
        public override void Visit(VarDefOp op, Node n)
        {
            VisitDefault(n);

            var definingNode = n.Child0;
            var definingNodeOp = definingNode.Op;

            GroupAggregateVarInfo referencedVarInfo;
            Node templateNode;
            bool isUnnested;
            if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(
                definingNode, true, _command, _groupAggregateVarInfoManager, out referencedVarInfo, out templateNode, out isUnnested))
            {
                _groupAggregateVarInfoManager.Add(op.Var, referencedVarInfo, templateNode, isUnnested);
            }
            else if (definingNodeOp.OpType
                     == OpType.NewRecord)
            {
                var newRecordOp = (NewRecordOp)definingNodeOp;
                for (var i = 0; i < definingNode.Children.Count; i++)
                {
                    var argumentNode = definingNode.Children[i];
                    if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(
                        argumentNode, true, _command, _groupAggregateVarInfoManager, out referencedVarInfo, out templateNode, out isUnnested))
                    {
                        _groupAggregateVarInfoManager.Add(op.Var, referencedVarInfo, templateNode, isUnnested, newRecordOp.Properties[i]);
                    }
                }
            }
        }

        #endregion

        #region RelOp Visitors

        // <summary>
        // Registers the group aggregate var with the group aggregate var info manager
        // </summary>
        public override void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
            foreach (var child in n.Child3.Children)
            {
                var groupAggregateVar = ((VarDefOp)child.Op).Var;
                GroupAggregateVarRefInfo groupAggregateVarRefInfo;
                // If the group by is over a group, it may be already tracked as referencing a group var
                // An optimization would be to separately track this groupAggregateVar too, for the cases when the aggregate can 
                // not be pushed to the group by node over which this one is defined but can be propagated to this group by node.
                if (!_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(groupAggregateVar, out groupAggregateVarRefInfo))
                {
                    _groupAggregateVarInfoManager.Add(
                        groupAggregateVar, new GroupAggregateVarInfo(n, groupAggregateVar),
                        _command.CreateNode(_command.CreateVarRefOp(groupAggregateVar)), false);
                }
            }
        }

        // <summary>
        // If the unnestOp's var is defined as a reference of a group aggregate var,
        // then the columns it produces should be registered too, but as 'unnested' references
        // </summary>
        // <param name="op"> the unnestOp </param>
        // <param name="n"> current subtree </param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        public override void Visit(UnnestOp op, Node n)
        {
            VisitDefault(n);
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(op.Var, out groupAggregateVarRefInfo))
            {
                PlanCompiler.Assert(op.Table.Columns.Count == 1, "Expected one column before NTE");
                _groupAggregateVarInfoManager.Add(
                    op.Table.Columns[0], groupAggregateVarRefInfo.GroupAggregateVarInfo, groupAggregateVarRefInfo.Computation, true);
            }
        }

        #endregion

        #region ScalarOps Visitors

        // <summary>
        // If the op is a collection aggregate function it checks whether its arguement can be translated over
        // a single group aggregate var. If so, it is tracked as a candidate to be pushed into that
        // group by into node.
        // </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        public override void Visit(FunctionOp op, Node n)
        {
            VisitDefault(n);
            if (!PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
            {
                return;
            }
            PlanCompiler.Assert(n.Children.Count == 1, "Aggregate Function must have one argument");

            var argumentNode = n.Child0;

            GroupAggregateVarInfo referencedGroupAggregateVarInfo;
            Node templateNode;
            bool isUnnested;
            if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(
                n.Child0, false, _command, _groupAggregateVarInfoManager, out referencedGroupAggregateVarInfo, out templateNode,
                out isUnnested)
                &&
                (isUnnested || AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, referencedGroupAggregateVarInfo.GroupAggregateVar)))
            {
                referencedGroupAggregateVarInfo.CandidateAggregateNodes.Add(new KeyValuePair<Node, Node>(n, templateNode));
            }
        }

        #endregion

        // <summary>
        // Default visitor for nodes.
        // It tracks the child-parent relationship.
        // </summary>
        protected override void VisitDefault(Node n)
        {
            VisitChildren(n);
            foreach (var child in n.Children)
            {
                //No need to track terminal nodes, plus some of these may be reused.
                if (child.Op.Arity != 0)
                {
                    _childToParent.Add(child, n);
                }
            }
        }

        #endregion
    }
}
