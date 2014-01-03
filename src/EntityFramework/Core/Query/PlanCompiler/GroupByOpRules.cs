// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    // <summary>
    // Transformation Rules for GroupByOps
    // </summary>
    internal static class GroupByOpRules
    {
        #region GroupByOpWithSimpleVarRedefinitions

        internal static readonly SimpleRule Rule_GroupByOpWithSimpleVarRedefinitions = new SimpleRule(
            OpType.GroupBy, ProcessGroupByWithSimpleVarRedefinitions);

        // <summary>
        // If the GroupByOp defines some computedVars as part of its keys, but those computedVars are simply
        // redefinitions of other Vars, then eliminate the computedVars.
        // GroupBy(X, VarDefList(VarDef(cv1, VarRef(v1)), ...), VarDefList(...))
        // can be transformed into
        // GroupBy(X, VarDefList(...))
        // where cv1 has now been replaced by v1
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> current subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> transformation status </returns>
        private static bool ProcessGroupByWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var groupByOp = (GroupByOp)n.Op;
            // no local keys? nothing to do
            if (n.Child1.Children.Count == 0)
            {
                return false;
            }

            var trc = (TransformationRulesContext)context;
            var command = trc.Command;

            var nodeInfo = command.GetExtendedNodeInfo(n);

            //
            // Check to see if any of the computed Vars defined by this GroupByOp
            // are simple redefinitions of other VarRefOps. Consider only those 
            // VarRefOps that are not "external" references
            //
            var canEliminateSomeVars = false;
            foreach (var varDefNode in n.Child1.Children)
            {
                var definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType
                    == OpType.VarRef)
                {
                    var varRefOp = (VarRefOp)definingExprNode.Op;
                    if (!nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                    {
                        // this is a Var that we should remove 
                        canEliminateSomeVars = true;
                    }
                }
            }

            // Did we have any redefinitions
            if (!canEliminateSomeVars)
            {
                return false;
            }

            //
            // OK. We've now identified a set of vars that are simple redefinitions.
            // Try and replace the computed Vars with the Vars that they're redefining
            //

            // Lets now build up a new VarDefListNode
            var newVarDefNodes = new List<Node>();
            foreach (var varDefNode in n.Child1.Children)
            {
                var varDefOp = (VarDefOp)varDefNode.Op;
                var varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null
                    && !nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                {
                    groupByOp.Outputs.Clear(varDefOp.Var);
                    groupByOp.Outputs.Set(varRefOp.Var);
                    groupByOp.Keys.Clear(varDefOp.Var);
                    groupByOp.Keys.Set(varRefOp.Var);
                    trc.AddVarMapping(varDefOp.Var, varRefOp.Var);
                }
                else
                {
                    newVarDefNodes.Add(varDefNode);
                }
            }

            // Create a new vardeflist node, and set that as Child1 for the group by op
            var newVarDefListNode = command.CreateNode(command.CreateVarDefListOp(), newVarDefNodes);
            n.Child1 = newVarDefListNode;
            return true; // subtree modified
        }

        #endregion

        #region GroupByOpOnAllInputColumnsWithAggregateOperation

        internal static readonly SimpleRule Rule_GroupByOpOnAllInputColumnsWithAggregateOperation = new SimpleRule(
            OpType.GroupBy, ProcessGroupByOpOnAllInputColumnsWithAggregateOperation);

        // <summary>
        // Converts a GroupBy(X, Y, Z) => OuterApply(X', GroupBy(Filter(X, key(X') == key(X)), Y, Z))
        // if and only if X is a ScanTableOp, and Z is the upper node of an aggregate function and
        // the group by operation uses all the columns of X as the key.
        // This is a fix for codeplex workitem 1959. Since now we're supporting NewRecordOp nodes as
        // part of the GroupBy aggregate variable computations, we are also respecting the fact that
        // group by (e => e) means that we're grouping by all columns of entity e. This was not a
        // problem when the NewRecordOp node was not being processed since this caused the GroupBy
        // statement to be simplified to a form with no keys and no output columns. The generated SQL
        // is correct, but it is different from what it used to be and may be incompatible if the
        // entity contains fields with datatypes that do not support being grouped by, such as blobs
        // and images.
        // This rule simplifies the tree so that we remain compatible with the way we were generating
        // queries that contain group by (e => e).
        // What this does is enabling the tree to take a shape that further optimization can convert
        // into an expression that groups by the key of the table and calls the aggregate function
        // as expected.
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> Current ProjectOp node </param>
        // <param name="newNode"> modified subtree </param>
        // <returns> Transformation status </returns>
        private static bool ProcessGroupByOpOnAllInputColumnsWithAggregateOperation(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;

            if (n.Child0.Op.OpType != OpType.ScanTable)
            {
                return false;
            }

            if (n.Child2 == null
                || n.Child2.Child0 == null
                || n.Child2.Child0.Child0 == null
                || n.Child2.Child0.Child0.Op.OpType != OpType.Aggregate)
            {
                return false;
            }

            var groupByOp = (GroupByOp)n.Op;

            var sourceTable = ((ScanTableOp)n.Child0.Op).Table;
            var allInputColumns = sourceTable.Columns;

            // Exit if the group's keys do not contain all the columns defined by Child0
            foreach (var column in allInputColumns)
            {
                if (!groupByOp.Keys.IsSet(column))
                {
                    return false;
                }
            }

            // All the columns of Child0 are used, so remove them from the outputs and the keys
            foreach (var column in allInputColumns)
            {
                groupByOp.Outputs.Clear(column);
                groupByOp.Keys.Clear(column);
            }

            // Build the OuterApply and also set the filter around the GroupBy's scan table.
            var command = context.Command;

            var scanTableOp = command.CreateScanTableOp(sourceTable.TableMetadata);
            var scanTable = command.CreateNode(scanTableOp);
            var outerApplyNode = command.CreateNode(command.CreateOuterApplyOp(), scanTable, n);

            Var newVar;
            var varDefListNode = command.CreateVarDefListNode(command.CreateNode(command.CreateVarRefOp(groupByOp.Outputs.First)), out newVar);

            newNode = command.CreateNode(
                    command.CreateProjectOp(newVar),
                    outerApplyNode,
                    varDefListNode);

            Node equality = null;
            var leftKeys = scanTableOp.Table.Keys.GetEnumerator();
            var rightKeys = sourceTable.Keys.GetEnumerator();
            for (int i = 0; i < sourceTable.Keys.Count; ++i)
            {
                leftKeys.MoveNext();
                rightKeys.MoveNext();
                var comparison = command.CreateNode(
                                    command.CreateComparisonOp(OpType.EQ),
                                    command.CreateNode(command.CreateVarRefOp(leftKeys.Current)),
                                    command.CreateNode(command.CreateVarRefOp(rightKeys.Current)));
                if (equality != null)
                {
                    equality = command.CreateNode(
                                    command.CreateConditionalOp(OpType.And),
                                    equality, comparison);
                }
                else
                {
                    equality = comparison;
                }
            }

            var filter = command.CreateNode(command.CreateFilterOp(),
                         n.Child0,
                         equality);
            n.Child0 = filter;

            return true; // subtree modified
        }

        #endregion

        #region GroupByOverProject

        internal static readonly PatternMatchRule Rule_GroupByOverProject =
            new PatternMatchRule(
                new Node(
                    GroupByOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessGroupByOverProject);

        // <summary>
        // Converts a GroupBy(Project(X, c1,..ck), agg1, agg2, .. aggm) =>
        // GroupBy(X, agg1', agg2', .. aggm')
        // where agg1', agg2', .. aggm'  are the "mapped" versions
        // of agg1, agg2, .. aggm, such that the references to c1, ... ck are
        // replaced by their definitions.
        // We only do this if each c1, ..ck is refereneced (in aggregates) at most once or it is a constant.
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> Current ProjectOp node </param>
        // <param name="newNode"> modified subtree </param>
        // <returns> Transformation status </returns>
        private static bool ProcessGroupByOverProject(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var op = (GroupByOp)n.Op;
            var command = (context).Command;
            var projectNode = n.Child0;
            var projectNodeVarDefList = projectNode.Child1;

            var keys = n.Child1;
            var aggregates = n.Child2;

            // If there are any keys, we should not remove the inner project
            if (keys.Children.Count > 0)
            {
                return false;
            }

            //Get a list of all defining vars
            var projectDefinitions = command.GetExtendedNodeInfo(projectNode).LocalDefinitions;

            //If any of the defined vars is output, than we need the extra project anyway.
            if (op.Outputs.Overlaps(projectDefinitions))
            {
                return false;
            }

            var createdNewProjectDefinitions = false;

            //If there are any constants remove them from the list that needs to be tested,
            //These can safely be replaced
            for (var i = 0; i < projectNodeVarDefList.Children.Count; i++)
            {
                var varDefNode = projectNodeVarDefList.Children[i];
                if (varDefNode.Child0.Op.OpType == OpType.Constant
                    || varDefNode.Child0.Op.OpType == OpType.InternalConstant
                    || varDefNode.Child0.Op.OpType == OpType.NullSentinel)
                {
                    //We shouldn't modify the original project definitions, thus we copy it  
                    // the first time we encounter a constant
                    if (!createdNewProjectDefinitions)
                    {
                        projectDefinitions = command.CreateVarVec(projectDefinitions);
                        createdNewProjectDefinitions = true;
                    }
                    projectDefinitions.Clear(((VarDefOp)varDefNode.Op).Var);
                }
            }

            if (VarRefUsageFinder.AnyVarUsedMoreThanOnce(projectDefinitions, aggregates, command))
            {
                return false;
            }

            //If we got here it means that all vars were either constants, or used at most once
            // Create a dictionary to be used for remapping the keys and the aggregates
            var varToDefiningNode = new Dictionary<Var, Node>(projectNodeVarDefList.Children.Count);
            for (var j = 0; j < projectNodeVarDefList.Children.Count; j++)
            {
                var varDefNode = projectNodeVarDefList.Children[j];
                var var = ((VarDefOp)varDefNode.Op).Var;
                varToDefiningNode.Add(var, varDefNode.Child0);
            }

            newNode.Child2 = VarRefReplacer.Replace(varToDefiningNode, aggregates, command);

            newNode.Child0 = projectNode.Child0;
            return true;
        }

        // <summary>
        // Replaces each occurance of the given vars with their definitions.
        // </summary>
        internal class VarRefReplacer : BasicOpVisitorOfNode
        {
            private readonly Dictionary<Var, Node> m_varReplacementTable;
            private readonly Command m_command;

            private VarRefReplacer(Dictionary<Var, Node> varReplacementTable, Command command)
            {
                m_varReplacementTable = varReplacementTable;
                m_command = command;
            }

            // <summary>
            // "Public" entry point. In the subtree rooted at the given root,
            // replace each occurance of the given vars with their definitions,
            // where each key-value pair in the dictionary is a var-definition pair.
            // </summary>
            internal static Node Replace(Dictionary<Var, Node> varReplacementTable, Node root, Command command)
            {
                var replacer = new VarRefReplacer(varReplacementTable, command);
                return replacer.VisitNode(root);
            }

            public override Node Visit(VarRefOp op, Node n)
            {
                Node replacementNode;
                if (m_varReplacementTable.TryGetValue(op.Var, out replacementNode))
                {
                    return replacementNode;
                }
                else
                {
                    return n;
                }
            }

            // <summary>
            // Recomputes node info post regular processing.
            // </summary>
            protected override Node VisitDefault(Node n)
            {
                var result = base.VisitDefault(n);
                m_command.RecomputeNodeInfo(result);
                return result;
            }
        }

        // <summary>
        // Used to determine whether any of the given vars occurs more than once
        // in a given subtree.
        // </summary>
        internal class VarRefUsageFinder : BasicOpVisitor
        {
            private bool m_anyUsedMoreThenOnce;
            private readonly VarVec m_varVec;
            private readonly VarVec m_usedVars;

            private VarRefUsageFinder(VarVec varVec, Command command)
            {
                m_varVec = varVec;
                m_usedVars = command.CreateVarVec();
            }

            // <summary>
            // Public entry point. Returns true if at least one of the given vars occurs more than
            // once in the subree rooted at the given root.
            // </summary>
            internal static bool AnyVarUsedMoreThanOnce(VarVec varVec, Node root, Command command)
            {
                var usageFinder = new VarRefUsageFinder(varVec, command);
                usageFinder.VisitNode(root);
                return usageFinder.m_anyUsedMoreThenOnce;
            }

            public override void Visit(VarRefOp op, Node n)
            {
                var referencedVar = op.Var;
                if (m_varVec.IsSet(referencedVar))
                {
                    if (m_usedVars.IsSet(referencedVar))
                    {
                        m_anyUsedMoreThenOnce = true;
                    }
                    else
                    {
                        m_usedVars.Set(referencedVar);
                    }
                }
            }

            protected override void VisitChildren(Node n)
            {
                //small optimization: no need to continue if we have the answer
                if (m_anyUsedMoreThenOnce)
                {
                    return;
                }
                base.VisitChildren(n);
            }
        }

        #endregion

        #region GroupByOpWithNoAggregates

        internal static readonly PatternMatchRule Rule_GroupByOpWithNoAggregates =
            new PatternMatchRule(
                new Node(
                    GroupByOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern),
                    new Node(VarDefListOp.Pattern)),
                ProcessGroupByOpWithNoAggregates);

        // <summary>
        // If the GroupByOp has no aggregates:
        // (1) and if it includes all all the keys of the input, than it is unnecessary
        // GroupBy (X, keys) -> Project(X, keys) where keys includes all keys of X.
        // (2) else it can be turned into a Distinct:
        // GroupBy (X, keys) -> Distinct(X, keys)
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> current subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> transformation status </returns>
        private static bool ProcessGroupByOpWithNoAggregates(RuleProcessingContext context, Node n, out Node newNode)
        {
            var command = context.Command;
            var op = (GroupByOp)n.Op;

            var nodeInfo = command.GetExtendedNodeInfo(n.Child0);
            var newOp = command.CreateProjectOp(op.Keys);

            var varDefListOp = command.CreateVarDefListOp();
            var varDefListNode = command.CreateNode(varDefListOp);

            newNode = command.CreateNode(newOp, n.Child0, n.Child1);

            //If we know the keys of the input and the list of keys includes them all, 
            // this is the result, otherwise add distinct
            if (nodeInfo.Keys.NoKeys
                || !op.Keys.Subsumes(nodeInfo.Keys.KeyVars))
            {
                newNode = command.CreateNode(command.CreateDistinctOp(command.CreateVarVec(op.Keys)), newNode);
            }
            return true;
        }

        #endregion

        #region All GroupByOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_GroupByOpWithSimpleVarRedefinitions,
                Rule_GroupByOverProject,
                Rule_GroupByOpWithNoAggregates,
                Rule_GroupByOpOnAllInputColumnsWithAggregateOperation,
            };

        #endregion
    }
}
