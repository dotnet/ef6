// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Simple implementation of the BasicOpVisitorOfT interface"/>
    /// </summary>
    /// <typeparam name="TResultType"> type parameter </typeparam>
    internal abstract class BasicOpVisitorOfT<TResultType>
    {
        #region visitor helpers

        /// <summary>
        ///     Simply iterates over all children, and manages any updates
        /// </summary>
        /// <param name="n"> The current node </param>
        protected virtual void VisitChildren(Node n)
        {
            for (var i = 0; i < n.Children.Count; i++)
            {
                VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        ///     Simply iterates over all children, and manages any updates, but in reverse order
        /// </summary>
        /// <param name="n"> The current node </param>
        protected virtual void VisitChildrenReverse(Node n)
        {
            for (var i = n.Children.Count - 1; i >= 0; i--)
            {
                VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        ///     Simple wrapper to invoke the appropriate action on a node
        /// </summary>
        /// <param name="n"> the node to process </param>
        /// <returns> </returns>
        internal TResultType VisitNode(Node n)
        {
            // Invoke the visitor
            return n.Op.Accept(this, n);
        }

        /// <summary>
        ///     A default processor for any node. Visits the children and returns itself unmodified.
        /// </summary>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially new node </returns>
        protected virtual TResultType VisitDefault(Node n)
        {
            VisitChildren(n);
            return default(TResultType);
        }

        #endregion

        /// <summary>
        ///     No processing yet for this node - raises an exception
        /// </summary>
        /// <param name="n"> </param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal virtual TResultType Unimplemented(Node n)
        {
            PlanCompiler.Assert(false, "Not implemented op type");
            return default(TResultType);
        }

        /// <summary>
        ///     Catch-all processor - raises an exception
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(Op op, Node n)
        {
            return Unimplemented(n);
        }

        #region AncillaryOp Visitors

        /// <summary>
        ///     A default processor for all AncillaryOps.
        ///     Allows new visitors to just override this to handle all AncillaryOps
        /// </summary>
        /// <param name="op"> the AncillaryOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        ///     VarDefOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(VarDefOp op, Node n)
        {
            return VisitAncillaryOpDefault(op, n);
        }

        /// <summary>
        ///     VarDefListOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(VarDefListOp op, Node n)
        {
            return VisitAncillaryOpDefault(op, n);
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        ///     A default processor for all PhysicalOps.
        ///     Allows new visitors to just override this to handle all PhysicalOps
        /// </summary>
        /// <param name="op"> the PhysicalOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        ///     PhysicalProjectOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(PhysicalProjectOp op, Node n)
        {
            return VisitPhysicalOpDefault(op, n);
        }

        #region NestOp Visitors

        /// <summary>
        ///     A default processor for all NestOps.
        ///     Allows new visitors to just override this to handle all NestOps
        /// </summary>
        /// <param name="op"> the NestOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitNestOp(NestBaseOp op, Node n)
        {
            return VisitPhysicalOpDefault(op, n);
        }

        /// <summary>
        ///     SingleStreamNestOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(SingleStreamNestOp op, Node n)
        {
            return VisitNestOp(op, n);
        }

        /// <summary>
        ///     MultiStreamNestOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(MultiStreamNestOp op, Node n)
        {
            return VisitNestOp(op, n);
        }

        #endregion

        #endregion

        #region RelOp Visitors

        /// <summary>
        ///     A default processor for all RelOps.
        ///     Allows new visitors to just override this to handle all RelOps
        /// </summary>
        /// <param name="op"> the RelOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitRelOpDefault(RelOp op, Node n)
        {
            return VisitDefault(n);
        }

        #region ApplyOp Visitors

        /// <summary>
        ///     Common handling for all ApplyOps
        /// </summary>
        /// <param name="op"> the ApplyOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitApplyOp(ApplyBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     CrossApply
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(CrossApplyOp op, Node n)
        {
            return VisitApplyOp(op, n);
        }

        /// <summary>
        ///     OuterApply
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(OuterApplyOp op, Node n)
        {
            return VisitApplyOp(op, n);
        }

        #endregion

        #region JoinOp Visitors

        /// <summary>
        ///     A default processor for all JoinOps.
        ///     Allows new visitors to just override this to handle all JoinOps.
        /// </summary>
        /// <param name="op"> the JoinOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitJoinOp(JoinBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     CrossJoin
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(CrossJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        ///     FullOuterJoin
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(FullOuterJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        ///     LeftOuterJoin
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(LeftOuterJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        ///     InnerJoin
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(InnerJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        #endregion

        #region SetOp Visitors

        /// <summary>
        ///     A default processor for all SetOps.
        ///     Allows new visitors to just override this to handle all SetOps.
        /// </summary>
        /// <param name="op"> the SetOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitSetOp(SetOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     Except
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ExceptOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        /// <summary>
        ///     Intersect
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(IntersectOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        /// <summary>
        ///     UnionAll
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(UnionAllOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        #endregion

        /// <summary>
        ///     Distinct
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(DistinctOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     FilterOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(FilterOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     GroupByBaseOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        protected virtual TResultType VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     GroupByOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(GroupByOp op, Node n)
        {
            return VisitGroupByOp(op, n);
        }

        /// <summary>
        ///     GroupByIntoOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(GroupByIntoOp op, Node n)
        {
            return VisitGroupByOp(op, n);
        }

        /// <summary>
        ///     ProjectOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ProjectOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        #region TableOps

        /// <summary>
        ///     Default handler for all TableOps
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        protected virtual TResultType VisitTableOp(ScanTableBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     ScanTableOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ScanTableOp op, Node n)
        {
            return VisitTableOp(op, n);
        }

        /// <summary>
        ///     ScanViewOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ScanViewOp op, Node n)
        {
            return VisitTableOp(op, n);
        }

        #endregion

        /// <summary>
        ///     Visitor pattern method for SingleRowOp
        /// </summary>
        /// <param name="op"> The SingleRowOp being visited </param>
        /// <param name="n"> The Node that references the Op </param>
        /// <returns> </returns>
        public virtual TResultType Visit(SingleRowOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     Visitor pattern method for SingleRowTableOp
        /// </summary>
        /// <param name="op"> The SingleRowTableOp being visited </param>
        /// <param name="n"> The Node that references the Op </param>
        /// <returns> </returns>
        public virtual TResultType Visit(SingleRowTableOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     A default processor for all SortOps.
        ///     Allows new visitors to just override this to handle ConstrainedSortOp/SortOp.
        /// </summary>
        /// <param name="op"> the SetOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially modified subtree </returns>
        protected virtual TResultType VisitSortOp(SortBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        ///     SortOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(SortOp op, Node n)
        {
            return VisitSortOp(op, n);
        }

        /// <summary>
        ///     ConstrainedSortOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ConstrainedSortOp op, Node n)
        {
            return VisitSortOp(op, n);
        }

        /// <summary>
        ///     UnnestOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(UnnestOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        #endregion

        #region ScalarOp Visitors

        /// <summary>
        ///     A default processor for all ScalarOps.
        ///     Allows new visitors to just override this to handle all ScalarOps
        /// </summary>
        /// <param name="op"> the ScalarOp </param>
        /// <param name="n"> the node to process </param>
        /// <returns> a potentially new node </returns>
        protected virtual TResultType VisitScalarOpDefault(ScalarOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        ///     Default handler for all constant Ops
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        protected virtual TResultType VisitConstantOp(ConstantBaseOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     AggregateOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(AggregateOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     ArithmeticOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ArithmeticOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     CaseOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(CaseOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     CastOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(CastOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     SoftCastOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(SoftCastOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NestOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(CollectOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     ComparisonOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ComparisonOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     ConditionalOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ConditionalOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     ConstantOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        ///     ConstantPredicateOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ConstantPredicateOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        ///     ElementOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ElementOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     ExistsOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(ExistsOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     FunctionOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(FunctionOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     GetEntityRefOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(GetEntityRefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     GetRefKeyOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(GetRefKeyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     InternalConstantOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(InternalConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        ///     IsOfOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(IsOfOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     LikeOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(LikeOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NewEntityOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NewEntityOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NewInstanceOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NewInstanceOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     DiscriminatedNewInstanceOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(DiscriminatedNewEntityOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NewMultisetOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NewMultisetOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NewRecordOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NewRecordOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     NullOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NullOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        ///     NullSentinelOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(NullSentinelOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        ///     PropertyOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(PropertyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     RelPropertyOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(RelPropertyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     RefOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(RefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     TreatOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(TreatOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        ///     VarRefOp
        /// </summary>
        /// <param name="op"> </param>
        /// <param name="n"> </param>
        /// <returns> </returns>
        public virtual TResultType Visit(VarRefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        public virtual TResultType Visit(DerefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        public virtual TResultType Visit(NavigateOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        #endregion
    }
}
