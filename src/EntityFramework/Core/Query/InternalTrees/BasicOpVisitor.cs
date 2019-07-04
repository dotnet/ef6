// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Simple implementation of the BasicOpVisitor interface.
    // </summary>
    internal abstract class BasicOpVisitor
    {
        #region Visitor Helpers

        // <summary>
        // Visit the children of this Node
        // </summary>
        // <param name="n"> The Node that references the Op </param>
        protected virtual void VisitChildren(Node n)
        {
            foreach (var chi in n.Children)
            {
                VisitNode(chi);
            }
        }

        // <summary>
        // Visit the children of this Node. but in reverse order
        // </summary>
        // <param name="n"> The current node </param>
        protected virtual void VisitChildrenReverse(Node n)
        {
            for (var i = n.Children.Count - 1; i >= 0; i--)
            {
                VisitNode(n.Children[i]);
            }
        }

        // <summary>
        // Visit this node
        // </summary>
        internal virtual void VisitNode(Node n)
        {
            n.Op.Accept(this, n);
        }

        // <summary>
        // Default node visitor
        // </summary>
        protected virtual void VisitDefault(Node n)
        {
            VisitChildren(n);
        }

        // <summary>
        // Default handler for all constantOps
        // </summary>
        // <param name="op"> the constant op </param>
        // <param name="n"> the node </param>
        protected virtual void VisitConstantOp(ConstantBaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Default handler for all TableOps
        // </summary>
        protected virtual void VisitTableOp(ScanTableBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Default handler for all JoinOps
        // </summary>
        // <param name="op"> join op </param>
        protected virtual void VisitJoinOp(JoinBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Default handler for all ApplyOps
        // </summary>
        // <param name="op"> apply op </param>
        protected virtual void VisitApplyOp(ApplyBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Default handler for all SetOps
        // </summary>
        // <param name="op"> set op </param>
        protected virtual void VisitSetOp(SetOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Default handler for all SortOps
        // </summary>
        // <param name="op"> sort op </param>
        protected virtual void VisitSortOp(SortBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Default handler for all GroupBy ops
        // </summary>
        protected virtual void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        #endregion

        #region BasicOpVisitor Members

        // <summary>
        // Trap method for unrecognized Op types
        // </summary>
        // <param name="op"> The Op being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(Op op, Node n)
        {
            throw new NotSupportedException(Strings.Iqt_General_UnsupportedOp(op.GetType().FullName));
        }

        #region ScalarOps

        protected virtual void VisitScalarOpDefault(ScalarOp op, Node n)
        {
            VisitDefault(n);
        }

        // <summary>
        // Visitor pattern method for ConstantOp
        // </summary>
        // <param name="op"> The ConstantOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ConstantOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        // <summary>
        // Visitor pattern method for NullOp
        // </summary>
        // <param name="op"> The NullOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NullOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        // <summary>
        // Visitor pattern method for NullSentinelOp
        // </summary>
        // <param name="op"> The NullSentinelOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NullSentinelOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        // <summary>
        // Visitor pattern method for InternalConstantOp
        // </summary>
        // <param name="op"> The InternalConstantOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(InternalConstantOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        // <summary>
        // Visitor pattern method for ConstantPredicateOp
        // </summary>
        // <param name="op"> The ConstantPredicateOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ConstantPredicateOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        // <summary>
        // Visitor pattern method for FunctionOp
        // </summary>
        // <param name="op"> The FunctionOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(FunctionOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for PropertyOp
        // </summary>
        // <param name="op"> The PropertyOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(PropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for RelPropertyOp
        // </summary>
        // <param name="op"> The RelPropertyOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(RelPropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for CaseOp
        // </summary>
        // <param name="op"> The CaseOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(CaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ComparisonOp
        // </summary>
        // <param name="op"> The ComparisonOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ComparisonOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for LikeOp
        // </summary>
        // <param name="op"> The LikeOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(LikeOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for AggregateOp
        // </summary>
        // <param name="op"> The AggregateOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(AggregateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for NewInstanceOp
        // </summary>
        // <param name="op"> The NewInstanceOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NewInstanceOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for NewEntityOp
        // </summary>
        // <param name="op"> The NewEntityOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NewEntityOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for DiscriminatedNewInstanceOp
        // </summary>
        // <param name="op"> The DiscriminatedNewInstanceOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(DiscriminatedNewEntityOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for NewMultisetOp
        // </summary>
        // <param name="op"> The NewMultisetOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NewMultisetOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for NewRecordOp
        // </summary>
        // <param name="op"> The NewRecordOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(NewRecordOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for RefOp
        // </summary>
        // <param name="op"> The RefOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(RefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for VarRefOp
        // </summary>
        // <param name="op"> The VarRefOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(VarRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ConditionalOp
        // </summary>
        // <param name="op"> The ConditionalOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ConditionalOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ArithmeticOp
        // </summary>
        // <param name="op"> The ArithmeticOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ArithmeticOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for TreatOp
        // </summary>
        // <param name="op"> The TreatOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(TreatOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for CastOp
        // </summary>
        // <param name="op"> The CastOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(CastOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for SoftCastOp
        // </summary>
        // <param name="op"> The SoftCastOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(SoftCastOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for IsOp
        // </summary>
        // <param name="op"> The IsOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(IsOfOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ExistsOp
        // </summary>
        // <param name="op"> The ExistsOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ExistsOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ElementOp
        // </summary>
        // <param name="op"> The ElementOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ElementOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for GetEntityRefOp
        // </summary>
        // <param name="op"> The GetEntityRefOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(GetEntityRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for GetRefKeyOp
        // </summary>
        // <param name="op"> The GetRefKeyOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(GetRefKeyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for NestOp
        // </summary>
        // <param name="op"> The NestOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(CollectOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        public virtual void Visit(DerefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        public virtual void Visit(NavigateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        #endregion

        #region AncillaryOps

        protected virtual void VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            VisitDefault(n);
        }

        // <summary>
        // Visitor pattern method for VarDefOp
        // </summary>
        // <param name="op"> The VarDefOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(VarDefOp op, Node n)
        {
            VisitAncillaryOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for VarDefListOp
        // </summary>
        // <param name="op"> The VarDefListOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(VarDefListOp op, Node n)
        {
            VisitAncillaryOpDefault(op, n);
        }

        #endregion

        #region RelOps

        protected virtual void VisitRelOpDefault(RelOp op, Node n)
        {
            VisitDefault(n);
        }

        // <summary>
        // Visitor pattern method for ScanTableOp
        // </summary>
        // <param name="op"> The ScanTableOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ScanTableOp op, Node n)
        {
            VisitTableOp(op, n);
        }

        // <summary>
        // Visitor pattern method for ScanViewOp
        // </summary>
        // <param name="op"> The ScanViewOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ScanViewOp op, Node n)
        {
            VisitTableOp(op, n);
        }

        // <summary>
        // Visitor pattern method for UnnestOp
        // </summary>
        // <param name="op"> The UnnestOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(UnnestOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for ProjectOp
        // </summary>
        // <param name="op"> The ProjectOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ProjectOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for FilterOp
        // </summary>
        // <param name="op"> The FilterOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(FilterOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for SortOp
        // </summary>
        // <param name="op"> The SortOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(SortOp op, Node n)
        {
            VisitSortOp(op, n);
        }

        // <summary>
        // Visitor pattern method for ConstrainedSortOp
        // </summary>
        // <param name="op"> The ConstrainedSortOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ConstrainedSortOp op, Node n)
        {
            VisitSortOp(op, n);
        }

        // <summary>
        // Visitor pattern method for GroupByOp
        // </summary>
        // <param name="op"> The GroupByOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(GroupByOp op, Node n)
        {
            VisitGroupByOp(op, n);
        }

        // <summary>
        // Visitor pattern method for GroupByIntoOp
        // </summary>
        // <param name="op"> The GroupByIntoOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
        }

        // <summary>
        // Visitor pattern method for CrossJoinOp
        // </summary>
        // <param name="op"> The CrossJoinOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(CrossJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        // <summary>
        // Visitor pattern method for InnerJoinOp
        // </summary>
        // <param name="op"> The InnerJoinOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(InnerJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        // <summary>
        // Visitor pattern method for LeftOuterJoinOp
        // </summary>
        // <param name="op"> The LeftOuterJoinOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(LeftOuterJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        // <summary>
        // Visitor pattern method for FullOuterJoinOp
        // </summary>
        // <param name="op"> The FullOuterJoinOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(FullOuterJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        // <summary>
        // Visitor pattern method for CrossApplyOp
        // </summary>
        // <param name="op"> The CrossApplyOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(CrossApplyOp op, Node n)
        {
            VisitApplyOp(op, n);
        }

        // <summary>
        // Visitor pattern method for OuterApplyOp
        // </summary>
        // <param name="op"> The OuterApplyOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(OuterApplyOp op, Node n)
        {
            VisitApplyOp(op, n);
        }

        // <summary>
        // Visitor pattern method for UnionAllOp
        // </summary>
        // <param name="op"> The UnionAllOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(UnionAllOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        // <summary>
        // Visitor pattern method for IntersectOp
        // </summary>
        // <param name="op"> The IntersectOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(IntersectOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        // <summary>
        // Visitor pattern method for ExceptOp
        // </summary>
        // <param name="op"> The ExceptOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(ExceptOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        // <summary>
        // Visitor pattern method for DistinctOp
        // </summary>
        // <param name="op"> The DistinctOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(DistinctOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for SingleRowOp
        // </summary>
        // <param name="op"> The SingleRowOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(SingleRowOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for SingleRowTableOp
        // </summary>
        // <param name="op"> The SingleRowTableOp being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(SingleRowTableOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        #endregion

        #region PhysicalOps

        protected virtual void VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            VisitDefault(n);
        }

        // <summary>
        // Visitor pattern method for PhysicalProjectOp
        // </summary>
        // <param name="op"> The op being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(PhysicalProjectOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
        }

        #region NestOps

        // <summary>
        // Common handling for all nestOps
        // </summary>
        // <param name="op"> nest op </param>
        protected virtual void VisitNestOp(NestBaseOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
        }

        // <summary>
        // Visitor pattern method for SingleStreamNestOp
        // </summary>
        // <param name="op"> The op being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(SingleStreamNestOp op, Node n)
        {
            VisitNestOp(op, n);
        }

        // <summary>
        // Visitor pattern method for MultistreamNestOp
        // </summary>
        // <param name="op"> The op being visited </param>
        // <param name="n"> The Node that references the Op </param>
        public virtual void Visit(MultiStreamNestOp op, Node n)
        {
            VisitNestOp(op, n);
        }

        #endregion

        #endregion

        #endregion
    }
}
