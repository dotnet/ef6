namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// SetOp Transformation Rules
    /// </summary>
    internal static class SetOpRules
    {
        #region SetOpOverFilters

        internal static readonly SimpleRule Rule_UnionAllOverEmptySet =
            new SimpleRule(OpType.UnionAll, ProcessSetOpOverEmptySet);

        internal static readonly SimpleRule Rule_IntersectOverEmptySet =
            new SimpleRule(OpType.Intersect, ProcessSetOpOverEmptySet);

        internal static readonly SimpleRule Rule_ExceptOverEmptySet =
            new SimpleRule(OpType.Except, ProcessSetOpOverEmptySet);

        /// <summary>
        /// Process a SetOp when one of the inputs is an emptyset. 
        /// 
        /// An emptyset is represented by a Filter(X, ConstantPredicate)
        ///    where the ConstantPredicate has a value of "false"
        /// 
        /// The general rules are
        ///    UnionAll(X, EmptySet) => X
        ///    UnionAll(EmptySet, X) => X
        ///    Intersect(EmptySet, X) => EmptySet
        ///    Intersect(X, EmptySet) => EmptySet
        ///    Except(EmptySet, X) => EmptySet
        ///    Except(X, EmptySet) => X
        /// 
        /// These rules then translate into 
        ///    UnionAll: return the non-empty input
        ///    Intersect: return the empty input
        ///    Except: return the "left" input 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="setOpNode">the current setop tree</param>
        /// <param name="filterNodeIndex">Index of the filter node in the setop</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessSetOpOverEmptySet(RuleProcessingContext context, Node setOpNode, out Node newNode)
        {
            var leftChildIsEmptySet = context.Command.GetExtendedNodeInfo(setOpNode.Child0).MaxRows == RowCount.Zero;
            var rightChildIsEmptySet = context.Command.GetExtendedNodeInfo(setOpNode.Child1).MaxRows == RowCount.Zero;

            if (!leftChildIsEmptySet
                && !rightChildIsEmptySet)
            {
                newNode = setOpNode;
                return false;
            }

            int indexToReturn;
            var setOp = (SetOp)setOpNode.Op;
            if (!rightChildIsEmptySet && setOp.OpType == OpType.UnionAll
                ||
                !leftChildIsEmptySet && setOp.OpType == OpType.Intersect)
            {
                indexToReturn = 1;
            }
            else
            {
                indexToReturn = 0;
            }

            newNode = setOpNode.Children[indexToReturn];

            var trc = (TransformationRulesContext)context;
            foreach (var kv in setOp.VarMap[indexToReturn])
            {
                trc.AddVarMapping(kv.Key, kv.Value);
            }
            return true;
        }

        #endregion

        #region All SetOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_UnionAllOverEmptySet,
                Rule_IntersectOverEmptySet,
                Rule_ExceptOverEmptySet,
            };

        #endregion
    }
}
