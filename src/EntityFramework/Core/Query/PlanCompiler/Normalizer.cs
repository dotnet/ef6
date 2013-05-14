// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The normalizer performs transformations of the tree to bring it to a 'normalized' format
    /// </summary>
    internal class Normalizer : SubqueryTrackingVisitor
    {
        #region constructors

        private Normalizer(PlanCompiler planCompilerState)
            : base(planCompilerState)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     The driver routine.
        /// </summary>
        /// <param name="planCompilerState"> plan compiler state </param>
        internal static void Process(PlanCompiler planCompilerState)
        {
            var normalizer = new Normalizer(planCompilerState);
            normalizer.Process();
        }

        #endregion

        #region private methods

        #region driver

        private void Process()
        {
            m_command.Root = VisitNode(m_command.Root);
        }

        #endregion

        #region visitor methods

        #region ScalarOps

        /// <summary>
        ///     Translate Exists(X) into Exists(select 1 from X)
        /// </summary>
        public override Node Visit(ExistsOp op, Node n)
        {
            VisitChildren(n);

            // Build up a dummy project node over the input
            n.Child0 = BuildDummyProjectForExists(n.Child0);

            return n;
        }

        /// <summary>
        ///     Build Project(select 1 from child).
        /// </summary>
        private Node BuildDummyProjectForExists(Node child)
        {
            Var newVar;
            var projectNode = m_command.BuildProject(
                child,
                m_command.CreateNode(m_command.CreateInternalConstantOp(m_command.IntegerType, 1)),
                out newVar);
            return projectNode;
        }

        /// <summary>
        ///     Build up an unnest above a scalar op node
        ///     X => unnest(X)
        /// </summary>
        /// <param name="collectionNode"> the scalarop collection node </param>
        /// <returns> the unnest node </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private Node BuildUnnest(Node collectionNode)
        {
            PlanCompiler.Assert(collectionNode.Op.IsScalarOp, "non-scalar usage of Un-nest?");
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(collectionNode.Op.Type), "non-collection usage for Un-nest?");

            Var newVar;
            var varDefNode = m_command.CreateVarDefNode(collectionNode, out newVar);
            var unnestOp = m_command.CreateUnnestOp(newVar);
            var unnestNode = m_command.CreateNode(unnestOp, varDefNode);

            return unnestNode;
        }

        /// <summary>
        ///     Converts the reference to a TVF as following: Collect(PhysicalProject(Unnest(Func)))
        /// </summary>
        /// <param name="op"> current function op </param>
        /// <param name="n"> current function subtree </param>
        /// <returns> the new expression that corresponds to the TVF </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private Node VisitCollectionFunction(FunctionOp op, Node n)
        {
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(op.Type), "non-TVF function?");

            var unnestNode = BuildUnnest(n);
            var unnestOp = unnestNode.Op as UnnestOp;
            var projectOp = m_command.CreatePhysicalProjectOp(unnestOp.Table.Columns[0]);
            var projectNode = m_command.CreateNode(projectOp, unnestNode);
            var collectOp = m_command.CreateCollectOp(n.Op.Type);
            var collectNode = m_command.CreateNode(collectOp, projectNode);

            return collectNode;
        }

        /// <summary>
        ///     Converts a collection aggregate function count(X), where X is a collection into
        ///     two parts. Part A is a groupby subquery that looks like
        ///     GroupBy(Unnest(X), empty, count(y))
        ///     where "empty" describes the fact that the groupby has no keys, and y is an
        ///     element var of the Unnest
        ///     Part 2 is a VarRef that refers to the aggregate var for count(y) described above.
        ///     Logically, we would replace the entire functionOp by element(GroupBy...). However,
        ///     since we also want to translate element() into single-row-subqueries, we do this
        ///     here as well.
        ///     The function itself is replaced by the VarRef, and the GroupBy is added to the list
        ///     of scalar subqueries for the current relOp node on the stack
        /// </summary>
        /// <param name="op"> the functionOp for the collection agg </param>
        /// <param name="n"> current subtree </param>
        /// <returns> the VarRef node that should replace the function </returns>
        private Node VisitCollectionAggregateFunction(FunctionOp op, Node n)
        {
            TypeUsage softCastType = null;
            var argNode = n.Child0;
            if (OpType.SoftCast
                == argNode.Op.OpType)
            {
                softCastType = TypeHelpers.GetEdmType<CollectionType>(argNode.Op.Type).TypeUsage;
                argNode = argNode.Child0;

                while (OpType.SoftCast
                       == argNode.Op.OpType)
                {
                    argNode = argNode.Child0;
                }
            }

            var unnestNode = BuildUnnest(argNode);
            var unnestOp = unnestNode.Op as UnnestOp;
            var unnestOutputVar = unnestOp.Table.Columns[0];

            var aggregateOp = m_command.CreateAggregateOp(op.Function, false);
            var unnestVarRefOp = m_command.CreateVarRefOp(unnestOutputVar);
            var unnestVarRefNode = m_command.CreateNode(unnestVarRefOp);
            if (softCastType != null)
            {
                unnestVarRefNode = m_command.CreateNode(m_command.CreateSoftCastOp(softCastType), unnestVarRefNode);
            }
            var aggExprNode = m_command.CreateNode(aggregateOp, unnestVarRefNode);

            var keyVars = m_command.CreateVarVec(); // empty keys
            var keyVarDefListNode = m_command.CreateNode(m_command.CreateVarDefListOp());

            var gbyOutputVars = m_command.CreateVarVec();
            Var aggVar;
            var aggVarDefListNode = m_command.CreateVarDefListNode(aggExprNode, out aggVar);
            gbyOutputVars.Set(aggVar);
            var gbyOp = m_command.CreateGroupByOp(keyVars, gbyOutputVars);
            var gbySubqueryNode = m_command.CreateNode(gbyOp, unnestNode, keyVarDefListNode, aggVarDefListNode);

            // "Move" this subquery to my parent relop
            var ret = AddSubqueryToParentRelOp(aggVar, gbySubqueryNode);

            return ret;
        }

        /// <summary>
        ///     Pre-processing for a function. Does the default scalar op processing.
        ///     If the function returns a collection (TVF), the method converts this expression into
        ///     Collect(PhysicalProject(Unnest(Func))).
        ///     If the function is a collection aggregate, converts it into the corresponding group aggregate.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "functionOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        public override Node Visit(FunctionOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Node newNode = null;

            // Is this a TVF?
            if (TypeSemantics.IsCollectionType(op.Type))
            {
                newNode = VisitCollectionFunction(op, n);
            }
            // Is this a collection-aggregate function?
            else if (PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
            {
                newNode = VisitCollectionAggregateFunction(op, n);
            }
            else
            {
                newNode = n;
            }

            PlanCompiler.Assert(newNode != null, "failure to construct a functionOp?");
            return newNode;
        }

        #endregion

        #region RelOps

        /// <summary>
        ///     Processing for all JoinOps
        /// </summary>
        /// <param name="op"> JoinOp </param>
        /// <param name="n"> Current subtree </param>
        protected override Node VisitJoinOp(JoinBaseOp op, Node n)
        {
            if (base.ProcessJoinOp(n))
            {
                // update the join condition
                // #479372: Build up a dummy project node over the input, as we always wrap the child of exists
                n.Child2.Child0 = BuildDummyProjectForExists(n.Child2.Child0);
            }
            return n;
        }

        #endregion

        #endregion

        #endregion
    }
}
