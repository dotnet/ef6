// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Transformation rules for ApplyOps - CrossApply, OuterApply
    // </summary>
    internal static class ApplyOpRules
    {
        #region ApplyOverFilter

        internal static readonly PatternMatchRule Rule_CrossApplyOverFilter =
            new PatternMatchRule(
                new Node(
                    CrossApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessApplyOverFilter);

        internal static readonly PatternMatchRule Rule_OuterApplyOverFilter =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessApplyOverFilter);

        // <summary>
        // Convert CrossApply(X, Filter(Y, p)) => InnerJoin(X, Y, p)
        // OuterApply(X, Filter(Y, p)) => LeftOuterJoin(X, Y, p)
        // if "Y" has no external references to X
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="applyNode"> Current ApplyOp </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> Transformation status </returns>
        private static bool ProcessApplyOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var filterNode = applyNode.Child1;
            var command = context.Command;

            var filterInputNodeInfo = command.GetNodeInfo(filterNode.Child0);
            var applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            //
            // check to see if the inputNode to the FilterOp has any external references 
            // to the left child of the ApplyOp. If it does, we simply return, we 
            // can't do much more here
            //
            if (filterInputNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                return false;
            }

            //
            // We've now gotten to the stage where the only external references (if any)
            // are from the filter predicate. 
            // We can now simply convert the apply into an inner/leftouter join with the 
            // filter predicate acting as the join condition
            //
            JoinBaseOp joinOp = null;
            if (applyNode.Op.OpType
                == OpType.CrossApply)
            {
                joinOp = command.CreateInnerJoinOp();
            }
            else
            {
                joinOp = command.CreateLeftOuterJoinOp();
            }

            newNode = command.CreateNode(joinOp, applyNode.Child0, filterNode.Child0, filterNode.Child1);
            return true;
        }

        internal static readonly PatternMatchRule Rule_OuterApplyOverProjectInternalConstantOverFilter =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(
                            FilterOp.Pattern,
                            new Node(LeafOp.Pattern),
                            new Node(LeafOp.Pattern)),
                        new Node(
                            VarDefListOp.Pattern,
                            new Node(
                                VarDefOp.Pattern,
                                new Node(InternalConstantOp.Pattern))))),
                ProcessOuterApplyOverDummyProjectOverFilter);

        internal static readonly PatternMatchRule Rule_OuterApplyOverProjectNullSentinelOverFilter =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(
                            FilterOp.Pattern,
                            new Node(LeafOp.Pattern),
                            new Node(LeafOp.Pattern)),
                        new Node(
                            VarDefListOp.Pattern,
                            new Node(
                                VarDefOp.Pattern,
                                new Node(NullSentinelOp.Pattern))))),
                ProcessOuterApplyOverDummyProjectOverFilter);

        // <summary>
        // Convert OuterApply(X, Project(Filter(Y, p), constant)) =>
        // LeftOuterJoin(X, Project(Y, constant), p)
        // if "Y" has no external references to X
        // In an ideal world, we would be able to push the Project below the Filter,
        // and then have the normal ApplyOverFilter rule handle this - but that causes us
        // problems because we always try to pull up ProjectOp's as high as possible. Hence,
        // the special case for this rule
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="applyNode"> Current ApplyOp </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> Transformation status </returns>
        private static bool ProcessOuterApplyOverDummyProjectOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var projectNode = applyNode.Child1;
            var projectOp = (ProjectOp)projectNode.Op;
            var filterNode = projectNode.Child0;
            var filterInputNode = filterNode.Child0;
            var command = context.Command;

            var filterInputNodeInfo = command.GetExtendedNodeInfo(filterInputNode);
            var applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            //
            // Check if the outputs of the ProjectOp or the inputNode to the FilterOp 
            // have any external references to the left child of the ApplyOp. 
            // If they do, we simply return, we can't do much more here
            //
            if (projectOp.Outputs.Overlaps(applyLeftChildNodeInfo.Definitions)
                || filterInputNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                return false;
            }

            //
            // We've now gotten to the stage where the only external references (if any)
            // are from the filter predicate. 
            // First, push the Project node down below the filter - but make sure that
            // all the Vars needed by the Filter are projected out 
            //
            var capWithProject = false;
            Node joinNodeRightInput = null;

            //
            // Check to see whether there is a sentinel var available - if there is, then
            // we can simply move the ProjectOp above the join we're going to construct 
            // and of course, build a NullIf expression for the constant.
            // Otherwise, the ProjectOp will need to be the child of the joinOp that we're
            // building - and we'll need to make sure that the ProjectOp projects out
            // any vars that are required for the Filter in the first place
            //
            var trc = (TransformationRulesContext)context;
            Var sentinelVar;
            bool sentinelIsInt32;

            if (TransformationRulesContext.TryGetInt32Var(filterInputNodeInfo.NonNullableDefinitions, out sentinelVar))
            {
                sentinelIsInt32 = true;
            }
            else
            {
                sentinelVar = filterInputNodeInfo.NonNullableDefinitions.First;
                sentinelIsInt32 = false;
            }

            if (sentinelVar != null)
            {
                capWithProject = true;
                var varDefNode = projectNode.Child1.Child0;
                if (varDefNode.Child0.Op.OpType == OpType.NullSentinel
                    && sentinelIsInt32
                    && trc.CanChangeNullSentinelValue)
                {
                    varDefNode.Child0 = context.Command.CreateNode(context.Command.CreateVarRefOp(sentinelVar));
                }
                else
                {
                    varDefNode.Child0 = trc.BuildNullIfExpression(sentinelVar, varDefNode.Child0);
                }
                command.RecomputeNodeInfo(varDefNode);
                command.RecomputeNodeInfo(projectNode.Child1);
                joinNodeRightInput = filterInputNode;
            }
            else
            {
                // We need to keep the projectNode - unfortunately
                joinNodeRightInput = projectNode;
                //
                // Make sure that every Var that is needed for the filter predicate
                // is captured in the projectOp outputs list
                //
                var filterPredicateNodeInfo = command.GetNodeInfo(filterNode.Child1);
                foreach (var v in filterPredicateNodeInfo.ExternalReferences)
                {
                    if (filterInputNodeInfo.Definitions.IsSet(v))
                    {
                        projectOp.Outputs.Set(v);
                    }
                }
                projectNode.Child0 = filterInputNode;
            }

            context.Command.RecomputeNodeInfo(projectNode);

            //
            // We can now simply convert the apply into an inner/leftouter join with the 
            // filter predicate acting as the join condition
            //
            var joinNode = command.CreateNode(command.CreateLeftOuterJoinOp(), applyNode.Child0, joinNodeRightInput, filterNode.Child1);
            if (capWithProject)
            {
                var joinNodeInfo = command.GetExtendedNodeInfo(joinNode);
                projectNode.Child0 = joinNode;
                projectOp.Outputs.Or(joinNodeInfo.Definitions);
                newNode = projectNode;
            }
            else
            {
                newNode = joinNode;
            }
            return true;
        }

        #endregion

        #region ApplyOverProject

        internal static readonly PatternMatchRule Rule_CrossApplyOverProject =
            new PatternMatchRule(
                new Node(
                    CrossApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessCrossApplyOverProject);

        // <summary>
        // Converts a CrossApply(X, Project(Y, ...)) => Project(CrossApply(X, Y), ...)
        // where the projectVars are simply pulled up
        // </summary>
        // <param name="context"> RuleProcessing context </param>
        // <param name="applyNode"> The ApplyOp subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> Transfomation status </returns>
        private static bool ProcessCrossApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var projectNode = applyNode.Child1;
            var projectOp = (ProjectOp)projectNode.Op;
            var command = context.Command;

            // We can simply pull up the project over the apply; provided we make sure 
            // that all the definitions of the apply are represented in the projectOp
            var applyNodeInfo = command.GetExtendedNodeInfo(applyNode);
            var vec = command.CreateVarVec(projectOp.Outputs);
            vec.Or(applyNodeInfo.Definitions);
            projectOp.Outputs.InitFrom(vec);

            // pull up the project over the apply node
            applyNode.Child1 = projectNode.Child0;
            context.Command.RecomputeNodeInfo(applyNode);
            projectNode.Child0 = applyNode;

            newNode = projectNode;
            return true;
        }

        internal static readonly PatternMatchRule Rule_OuterApplyOverProject =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessOuterApplyOverProject);

        // <summary>
        // Converts a
        // OuterApply(X, Project(Y, ...))
        // =>
        // Project(OuterApply(X, Project(Y, ...)), ...) or
        // Project(OuterApply(X, Y), ...)
        // The second (simpler) form is used if a "sentinel" var can be located (ie)
        // some Var of Y that is guaranteed to be non-null. Otherwise, we create a
        // dummy ProjectNode as the right child of the Apply - which
        // simply projects out all the vars of the Y, and adds on a constant (say "1"). This
        // constant is now treated as the sentinel var
        // Then the existing ProjectOp is pulled up above the the outer-apply, but all the locally defined
        // Vars have their defining expressions now expressed as
        // case when sentinelVar is null then null else oldDefiningExpr end
        // where oldDefiningExpr represents the original defining expression
        // This allows us to get nulls for the appropriate columns when necessary.
        // Special cases.
        // * If the oldDefiningExpr is itself an internal constant equivalent to the null sentinel ("1"),
        // we simply project a ref to the null sentinel, no need for cast
        // * If the ProjectOp contained exactly one locally defined Var, and it was a constant, then
        // we simply return - we will be looping endlessly otherwise
        // * If the ProjectOp contained no local definitions, then we don't need to create the
        // dummy projectOp - we can simply pull up the Project
        // * If any of the defining expressions of the local definitions was simply a VarRefOp
        // referencing a Var that was defined by Y, then there is no need to add the case
        // expression for that.
        // </summary>
        // <param name="context"> RuleProcessing context </param>
        // <param name="applyNode"> The ApplyOp subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> Transfomation status </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "VarDefOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessOuterApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var projectNode = applyNode.Child1;
            var varDefListNode = projectNode.Child1;

            var trc = (TransformationRulesContext)context;
            var inputNodeInfo = context.Command.GetExtendedNodeInfo(projectNode.Child0);
            var sentinelVar = inputNodeInfo.NonNullableDefinitions.First;

            //
            // special case handling first - we'll end up in an infinite loop otherwise.
            // If the ProjectOp is the dummy ProjectOp that we would be building (ie)
            // it defines only 1 var - and the defining expression is simply a constant
            // 
            if (sentinelVar == null
                &&
                varDefListNode.Children.Count == 1
                &&
                (varDefListNode.Child0.Child0.Op.OpType == OpType.InternalConstant
                 || varDefListNode.Child0.Child0.Op.OpType == OpType.NullSentinel))
            {
                return false;
            }

            var command = context.Command;
            Node dummyProjectNode = null;
            InternalConstantOp nullSentinelDefinitionOp = null;

            // get node information for the project's child
            var projectInputNodeInfo = command.GetExtendedNodeInfo(projectNode.Child0);

            //
            // Build up a dummy project node. 
            // Walk through each local definition of the current project Node, and convert
            // all expressions into case expressions whose value depends on the var
            // produced by the dummy project node
            //

            // Dev10 #480443: If any of the definitions changes we need to recompute the node info.
            var anyVarDefChagned = false;
            foreach (var varDefNode in varDefListNode.Children)
            {
                PlanCompiler.Assert(varDefNode.Op.OpType == OpType.VarDef, "Expected VarDefOp. Found " + varDefNode.Op.OpType + " instead");
                var varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp == null
                    || !projectInputNodeInfo.Definitions.IsSet(varRefOp.Var))
                {
                    // do we need to build a dummy project node
                    if (sentinelVar == null)
                    {
                        nullSentinelDefinitionOp = command.CreateInternalConstantOp(command.IntegerType, 1);
                        var dummyConstantExpr = command.CreateNode(nullSentinelDefinitionOp);
                        var dummyProjectVarDefListNode = command.CreateVarDefListNode(dummyConstantExpr, out sentinelVar);
                        var dummyProjectOp = command.CreateProjectOp(sentinelVar);
                        dummyProjectOp.Outputs.Or(projectInputNodeInfo.Definitions);
                        dummyProjectNode = command.CreateNode(dummyProjectOp, projectNode.Child0, dummyProjectVarDefListNode);
                    }

                    Node currentDefinition;

                    // If the null sentinel was just created, and the local definition of the current project Node 
                    // is an internal constant equivalent to the null sentinel, it can be rewritten as a reference
                    // to the null sentinel.
                    if (nullSentinelDefinitionOp != null
                        && (nullSentinelDefinitionOp.IsEquivalent(varDefNode.Child0.Op) ||
                        //The null sentinel has the same value of 1, thus it is safe.        
                            varDefNode.Child0.Op.OpType == OpType.NullSentinel))
                    {
                        currentDefinition = command.CreateNode(command.CreateVarRefOp(sentinelVar));
                    }
                    else
                    {
                        currentDefinition = trc.BuildNullIfExpression(sentinelVar, varDefNode.Child0);
                    }
                    varDefNode.Child0 = currentDefinition;
                    command.RecomputeNodeInfo(varDefNode);
                    anyVarDefChagned = true;
                }
            }

            // Recompute node info if needed
            if (anyVarDefChagned)
            {
                command.RecomputeNodeInfo(varDefListNode);
            }

            //
            // If we've created a dummy project node, make that the new child of the applyOp
            //
            applyNode.Child1 = dummyProjectNode != null ? dummyProjectNode : projectNode.Child0;
            command.RecomputeNodeInfo(applyNode);

            //
            // Pull up the project node above the apply node now. Also, make sure that every Var of 
            // the applyNode's definitions actually shows up in the new Project
            //
            projectNode.Child0 = applyNode;
            var applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);
            var projectOp = (ProjectOp)projectNode.Op;
            projectOp.Outputs.Or(applyLeftChildNodeInfo.Definitions);

            newNode = projectNode;
            return true;
        }

        #endregion

        #region ApplyOverAnything

        internal static readonly PatternMatchRule Rule_CrossApplyOverAnything =
            new PatternMatchRule(
                new Node(
                    CrossApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessApplyOverAnything);

        internal static readonly PatternMatchRule Rule_OuterApplyOverAnything =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessApplyOverAnything);

        // <summary>
        // Converts a CrossApply(X,Y) => CrossJoin(X,Y)
        // OuterApply(X,Y) => LeftOuterJoin(X, Y, true)
        // only if Y has no external references to X
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="applyNode"> The ApplyOp subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> the transformation status </returns>
        private static bool ProcessApplyOverAnything(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var applyLeftChild = applyNode.Child0;
            var applyRightChild = applyNode.Child1;
            var applyOp = (ApplyBaseOp)applyNode.Op;
            var command = context.Command;

            var applyRightChildNodeInfo = command.GetExtendedNodeInfo(applyRightChild);
            var applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyLeftChild);

            //
            // If we're currently dealing with an OuterApply, and the right child is guaranteed
            // to produce at least one row, then we can convert the outer-apply into a cross apply
            //
            var convertedToCrossApply = false;
            if (applyOp.OpType == OpType.OuterApply
                &&
                applyRightChildNodeInfo.MinRows >= RowCount.One)
            {
                applyOp = command.CreateCrossApplyOp();
                convertedToCrossApply = true;
            }

            //
            // Does the right child reference any of the definitions of the left child? If it
            // does, then simply return from this function
            //
            if (applyRightChildNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                if (convertedToCrossApply)
                {
                    newNode = command.CreateNode(applyOp, applyLeftChild, applyRightChild);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //
            // So, we now know that the right child does not reference any definitions
            // from the left. 
            // So, we simply convert the apply into an appropriate join Op
            //
            if (applyOp.OpType
                == OpType.CrossApply)
            {
                //
                // Convert "x CrossApply y" into "x CrossJoin y"
                //
                newNode = command.CreateNode(
                    command.CreateCrossJoinOp(),
                    applyLeftChild, applyRightChild);
            }
            else // outer apply
            {
                //
                // Convert "x OA y" into "x LOJ y on (true)"
                //
                var joinOp = command.CreateLeftOuterJoinOp();
                var trueOp = command.CreateTrueOp();
                var trueNode = command.CreateNode(trueOp);
                newNode = command.CreateNode(joinOp, applyLeftChild, applyRightChild, trueNode);
            }
            return true;
        }

        #endregion

        #region ApplyIntoScalarSubquery

        internal static readonly PatternMatchRule Rule_CrossApplyIntoScalarSubquery =
            new PatternMatchRule(
                new Node(
                    CrossApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessApplyIntoScalarSubquery);

        internal static readonly PatternMatchRule Rule_OuterApplyIntoScalarSubquery =
            new PatternMatchRule(
                new Node(
                    OuterApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessApplyIntoScalarSubquery);

        // <summary>
        // Converts a Apply(X,Y) => Project(X, Y1), where Y1 is a scalar subquery version of Y
        // The transformation is valid only if all of the following conditions hold:
        // 1. Y produces only one output
        // 2. Y produces at most one row
        // 3. Y produces at least one row, or the Apply operator in question is an OuterApply
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="applyNode"> The ApplyOp subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> the transformation status </returns>
        private static bool ProcessApplyIntoScalarSubquery(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            var command = context.Command;
            var applyRightChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child1);
            var applyKind = applyNode.Op.OpType;

            if (!CanRewriteApply(applyNode.Child1, applyRightChildNodeInfo, applyKind))
            {
                newNode = applyNode;
                return false;
            }

            // Create the project node over the original input with element over the apply as new projected var
            var applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            var oldVar = applyRightChildNodeInfo.Definitions.First;

            // Project all the outputs from the left child
            var projectOpOutputs = command.CreateVarVec(applyLeftChildNodeInfo.Definitions);

            //
            // Remap the var defining tree to get it into a consistent state
            // and then remove all references to oldVar from it to avoid them being wrongly remapped to newVar 
            // in subsequent remappings.
            //
            var trc = (TransformationRulesContext)context;
            trc.RemapSubtree(applyNode.Child1);
            VarDefinitionRemapper.RemapSubtree(applyNode.Child1, command, oldVar);

            var elementNode = command.CreateNode(command.CreateElementOp(oldVar.Type), applyNode.Child1);

            Var newVar;
            var varDefListNode = command.CreateVarDefListNode(elementNode, out newVar);
            projectOpOutputs.Set(newVar);

            newNode = command.CreateNode(
                command.CreateProjectOp(projectOpOutputs),
                applyNode.Child0,
                varDefListNode);

            // Add the var mapping from oldVar to newVar
            trc.AddVarMapping(oldVar, newVar);
            return true;
        }

        // <summary>
        // Determines whether an applyNode can be rewritten into a projection with a scalar subquery.
        // It can be done if all of the following conditions hold:
        // 1. The right child or the apply has only one output
        // 2. The right child of the apply produces at most one row
        // 3. The right child of the apply produces at least one row, or the Apply operator in question is an OuterApply
        // </summary>
        private static bool CanRewriteApply(Node rightChild, ExtendedNodeInfo applyRightChildNodeInfo, OpType applyKind)
        {
            //Check whether it produces only one definition
            if (applyRightChildNodeInfo.Definitions.Count != 1)
            {
                return false;
            }

            //Check whether it produces at most one row
            if (applyRightChildNodeInfo.MaxRows
                != RowCount.One)
            {
                return false;
            }

            //For cross apply it must also return exactly one row
            if (applyKind == OpType.CrossApply
                && (applyRightChildNodeInfo.MinRows != RowCount.One))
            {
                return false;
            }

            //Dev10 #488632: Make sure the right child not only declares to produce only one definition,
            // but has exactly one output. For example, ScanTableOp really outputs all the columns from the table, 
            // but in its ExtendedNodeInfo.Definitions only these that are referenced are shown.
            // This is to allow for projection pruning of the unreferenced columns. 
            if (OutputCountVisitor.CountOutputs(rightChild) != 1)
            {
                return false;
            }

            return true;
        }

        // <summary>
        // A visitor that calculates the number of output columns for a subree
        // with a given root
        // </summary>
        internal class OutputCountVisitor : BasicOpVisitorOfT<int>
        {
            #region Constructors

            #endregion

            #region Public Methods

            // <summary>
            // Calculates the number of output columns for the subree
            // rooted at the given node
            // </summary>
            internal static int CountOutputs(Node node)
            {
                var visitor = new OutputCountVisitor();
                return visitor.VisitNode(node);
            }

            #endregion

            #region Visitor Methods

            #region Helpers

            // <summary>
            // Visitor for children. Simply visit all children,
            // and sum the number of their outputs.
            // </summary>
            // <param name="n"> Current node </param>
            internal new int VisitChildren(Node n)
            {
                var result = 0;
                foreach (var child in n.Children)
                {
                    result += VisitNode(child);
                }
                return result;
            }

            // <summary>
            // A default processor for any node.
            // Returns the sum of the children outputs
            // </summary>
            protected override int VisitDefault(Node n)
            {
                return VisitChildren(n);
            }

            #endregion

            #region RelOp Visitors

            #region SetOp Visitors

            // <summary>
            // The number of outputs is same as for any of the inputs
            // </summary>
            protected override int VisitSetOp(SetOp op, Node n)
            {
                return op.Outputs.Count;
            }

            #endregion

            // <summary>
            // Distinct
            // </summary>
            public override int Visit(DistinctOp op, Node n)
            {
                return op.Keys.Count;
            }

            // <summary>
            // FilterOp
            // </summary>
            public override int Visit(FilterOp op, Node n)
            {
                return VisitNode(n.Child0);
            }

            // <summary>
            // GroupByOp
            // </summary>
            public override int Visit(GroupByOp op, Node n)
            {
                return op.Outputs.Count;
            }

            // <summary>
            // ProjectOp
            // </summary>
            public override int Visit(ProjectOp op, Node n)
            {
                return op.Outputs.Count;
            }

            #region TableOps

            // <summary>
            // ScanTableOp
            // </summary>
            public override int Visit(ScanTableOp op, Node n)
            {
                return op.Table.Columns.Count;
            }

            // <summary>
            // SingleRowTableOp
            // </summary>
            public override int Visit(SingleRowTableOp op, Node n)
            {
                return 0;
            }

            // <summary>
            // Same as the input
            // </summary>
            protected override int VisitSortOp(SortBaseOp op, Node n)
            {
                return VisitNode(n.Child0);
            }

            #endregion

            #endregion

            #endregion
        }

        // <summary>
        // A utility class that remaps a given var at its definition and also remaps all its references.
        // The given var is remapped to an arbitrary new var.
        // If the var is defined by a ScanTable, all the vars defined by that table and all their references
        // are remapped as well.
        // </summary>
        internal class VarDefinitionRemapper : VarRemapper
        {
            private readonly Var m_oldVar;

            private VarDefinitionRemapper(Var oldVar, Command command)
                : base(command)
            {
                m_oldVar = oldVar;
            }

            // <summary>
            // Public entry point.
            // Remaps the subree rooted at the given tree
            // </summary>
            internal static void RemapSubtree(Node root, Command command, Var oldVar)
            {
                var remapper = new VarDefinitionRemapper(oldVar, command);
                remapper.RemapSubtree(root);
            }

            // <summary>
            // Update vars in this subtree. Recompute the nodeinfo along the way
            // Unlike the base implementation, we want to visit the childrent, even if no vars are in the
            // remapping dictionary.
            // </summary>
            internal override void RemapSubtree(Node subTree)
            {
                foreach (var chi in subTree.Children)
                {
                    RemapSubtree(chi);
                }

                VisitNode(subTree);
                m_command.RecomputeNodeInfo(subTree);
            }

            // <summary>
            // If the node defines the node that needs to be remapped,
            // it remaps it to a new var.
            // </summary>
            public override void Visit(VarDefOp op, Node n)
            {
                if (op.Var == m_oldVar)
                {
                    Var newVar = m_command.CreateComputedVar(n.Child0.Op.Type);
                    n.Op = m_command.CreateVarDefOp(newVar);
                    AddMapping(m_oldVar, newVar);
                }
            }

            // <summary>
            // If the columnVars defined by the table contain the var that needs to be remapped
            // all the column vars produces by the table are remaped to new vars.
            // </summary>
            public override void Visit(ScanTableOp op, Node n)
            {
                if (op.Table.Columns.Contains(m_oldVar))
                {
                    var newScanTableOp = m_command.CreateScanTableOp(op.Table.TableMetadata);
                    var varDefListOp = m_command.CreateVarDefListOp();
                    for (var i = 0; i < op.Table.Columns.Count; i++)
                    {
                        AddMapping(op.Table.Columns[i], newScanTableOp.Table.Columns[i]);
                    }
                    n.Op = newScanTableOp;
                }
            }

            // <summary>
            // The var that needs to be remapped may be produced by a set op,
            // in which case the varmaps need to be updated too.
            // </summary>
            protected override void VisitSetOp(SetOp op, Node n)
            {
                base.VisitSetOp(op, n);

                if (op.Outputs.IsSet(m_oldVar))
                {
                    Var newVar = m_command.CreateSetOpVar(m_oldVar.Type);
                    op.Outputs.Clear(m_oldVar);
                    op.Outputs.Set(newVar);
                    RemapVarMapKey(op.VarMap[0], newVar);
                    RemapVarMapKey(op.VarMap[1], newVar);
                    AddMapping(m_oldVar, newVar);
                }
            }

            // <summary>
            // Replaces the entry in the varMap in which m_oldVar is a key
            // with an entry in which newVAr is the key and the value remains the same.
            // </summary>
            private void RemapVarMapKey(VarMap varMap, Var newVar)
            {
                var value = varMap[m_oldVar];
                varMap.Remove(m_oldVar);
                varMap.Add(newVar, value);
            }
        }

        #endregion

        #region CrossApply over LeftOuterJoin of SingleRowTable with anything and with constant predicate

        internal static readonly PatternMatchRule Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable =
            new PatternMatchRule(
                new Node(
                    CrossApplyOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        LeftOuterJoinOp.Pattern,
                        new Node(SingleRowTableOp.Pattern),
                        new Node(LeafOp.Pattern),
                        new Node(ConstantPredicateOp.Pattern))),
                ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable);

        // <summary>
        // Convert a CrossApply(X, LeftOuterJoin(SingleRowTable, Y, on true))
        // into just OuterApply(X, Y)
        // </summary>
        // <param name="context"> rule processing context </param>
        // <param name="applyNode"> the apply node </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> transformation status </returns>
        private static bool ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable(
            RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            var joinNode = applyNode.Child1;

            //Check the value of the predicate
            var joinPredicate = (ConstantPredicateOp)joinNode.Child2.Op;
            if (joinPredicate.IsFalse)
            {
                return false;
            }

            applyNode.Op = context.Command.CreateOuterApplyOp();
            applyNode.Child1 = joinNode.Child1;
            return true;
        }

        #endregion

        #region All ApplyOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_CrossApplyOverAnything,
                Rule_CrossApplyOverFilter,
                Rule_CrossApplyOverProject,
                Rule_OuterApplyOverAnything,
                Rule_OuterApplyOverProjectInternalConstantOverFilter,
                Rule_OuterApplyOverProjectNullSentinelOverFilter,
                Rule_OuterApplyOverProject,
                Rule_OuterApplyOverFilter,
                Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable,
                Rule_CrossApplyIntoScalarSubquery,
                Rule_OuterApplyIntoScalarSubquery,
            };

        #endregion
    }
}
