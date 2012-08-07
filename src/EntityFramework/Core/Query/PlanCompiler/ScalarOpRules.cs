// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Transformation rules for ScalarOps
    /// </summary>
    internal static class ScalarOpRules
    {
        #region CaseOp Rules

        internal static readonly SimpleRule Rule_SimplifyCase = new SimpleRule(OpType.Case, ProcessSimplifyCase);
        internal static readonly SimpleRule Rule_FlattenCase = new SimpleRule(OpType.Case, ProcessFlattenCase);

        /// <summary>
        ///     We perform the following simple transformation for CaseOps. If every single
        ///     then/else expression in the CaseOp is equivalent, then we can simply replace
        ///     the Op with the first then/expression. Specifically,
        ///     case when w1 then t1 when w2 then t2 ... when wn then tn else e end
        ///     => t1
        ///     assuming that t1 is equivalent to t2 is equivalent to ... to e
        /// </summary>
        /// <param name="context"> Rule Processing context </param>
        /// <param name="caseOpNode"> The current subtree for the CaseOp </param>
        /// <param name="newNode"> the (possibly) modified subtree </param>
        /// <returns> true, if we performed any transformations </returns>
        private static bool ProcessSimplifyCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
        {
            var caseOp = (CaseOp)caseOpNode.Op;
            newNode = caseOpNode;

            //
            // Can I collapse the entire case-expression into a single expression - yes, 
            // if all the then/else clauses are the same expression
            //
            if (ProcessSimplifyCase_Collapse(caseOpNode, out newNode))
            {
                return true;
            }

            //
            // Can I remove any unnecessary when-then pairs ?
            //
            if (ProcessSimplifyCase_EliminateWhenClauses(context, caseOp, caseOpNode, out newNode))
            {
                return true;
            }

            // Nothing else I can think of
            return false;
        }

        /// <summary>
        ///     Try and collapse the case expression into a single expression. 
        ///     If every single then/else expression in the CaseOp is equivalent, then we can 
        ///     simply replace the CaseOp with the first then/expression. Specifically,
        ///     case when w1 then t1 when w2 then t2 ... when wn then tn else e end
        ///     => t1
        ///     if t1 is equivalent to t2 is equivalent to ... to e
        /// </summary>
        /// <param name="caseOpNode"> current subtree </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> true, if we performed a transformation </returns>
        private static bool ProcessSimplifyCase_Collapse(Node caseOpNode, out Node newNode)
        {
            newNode = caseOpNode;
            var firstThenNode = caseOpNode.Child1;
            var elseNode = caseOpNode.Children[caseOpNode.Children.Count - 1];
            if (!firstThenNode.IsEquivalent(elseNode))
            {
                return false;
            }
            for (var i = 3; i < caseOpNode.Children.Count - 1; i += 2)
            {
                if (!caseOpNode.Children[i].IsEquivalent(firstThenNode))
                {
                    return false;
                }
            }

            // All nodes are equivalent - simply return the first then node
            newNode = firstThenNode;
            return true;
        }

        /// <summary>
        ///     Try and remove spurious branches from the case expression. 
        ///     If any of the WHEN clauses is the 'FALSE' expression, simply remove that 
        ///     branch (when-then pair) from the case expression.
        ///     If any of the WHEN clauses is the 'TRUE' expression, then all branches to the 
        ///     right of it are irrelevant - eliminate them. Eliminate this branch as well, 
        ///     and make the THEN expression of this branch the ELSE expression for the entire
        ///     Case expression. If the WHEN expression represents the first branch, then 
        ///     replace the entire case expression by the corresponding THEN expression
        /// </summary>
        /// <param name="context"> rule processing context </param>
        /// <param name="caseOp"> current caseOp </param>
        /// <param name="caseOpNode"> Current subtree </param>
        /// <param name="newNode"> the new subtree </param>
        /// <returns> true, if there was a transformation </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessSimplifyCase_EliminateWhenClauses(
            RuleProcessingContext context, CaseOp caseOp, Node caseOpNode, out Node newNode)
        {
            List<Node> newNodeArgs = null;
            newNode = caseOpNode;

            for (var i = 0; i < caseOpNode.Children.Count;)
            {
                // Special handling for the else clause
                if (i == caseOpNode.Children.Count - 1)
                {
                    // If the else clause is a SoftCast then we do not attempt to simplify
                    // the case operation, since this may change the result type.
                    // This really belongs in more general SoftCastOp logic in the CTreeGenerator
                    // that converts SoftCasts that could affect the result type of the query into
                    // a real cast or a trivial case statement, to preserve the result type.
                    // This is tracked by SQL PT Work Item #300003327.
                    if (OpType.SoftCast
                        == caseOpNode.Children[i].Op.OpType)
                    {
                        return false;
                    }

                    if (newNodeArgs != null)
                    {
                        newNodeArgs.Add(caseOpNode.Children[i]);
                    }
                    break;
                }

                // If the current then clause is a SoftCast then we do not attempt to simplify
                // the case operation, since this may change the result type.
                // Again, this really belongs in the CTreeGenerator as per SQL PT Work Item #300003327.
                if (OpType.SoftCast
                    == caseOpNode.Children[i + 1].Op.OpType)
                {
                    return false;
                }

                // Check to see if the when clause is a ConstantPredicate
                if (caseOpNode.Children[i].Op.OpType
                    != OpType.ConstantPredicate)
                {
                    if (newNodeArgs != null)
                    {
                        newNodeArgs.Add(caseOpNode.Children[i]);
                        newNodeArgs.Add(caseOpNode.Children[i + 1]);
                    }
                    i += 2;
                    continue;
                }

                // Found a when-clause which is a constant predicate
                var constPred = (ConstantPredicateOp)caseOpNode.Children[i].Op;
                // Create the newArgs list, if we haven't done so already
                if (newNodeArgs == null)
                {
                    newNodeArgs = new List<Node>();
                    for (var j = 0; j < i; j++)
                    {
                        newNodeArgs.Add(caseOpNode.Children[j]);
                    }
                }

                // If the when-clause is the "true" predicate, then we simply ignore all
                // the succeeding arguments. We make the "then" clause of this when-clause
                // as the "else-clause" of the resulting caseOp
                if (constPred.IsTrue)
                {
                    newNodeArgs.Add(caseOpNode.Children[i + 1]);
                    break;
                }
                else
                {
                    // Otherwise, we simply skip the when-then pair
                    PlanCompiler.Assert(constPred.IsFalse, "constant predicate must be either true or false");
                    i += 2;
                    continue;
                }
            }

            // Did we see any changes? Simply return
            if (newNodeArgs == null)
            {
                return false;
            }

            // Otherwise, we did do some processing
            PlanCompiler.Assert(newNodeArgs.Count > 0, "new args list must not be empty");
            // Is there only one expression in the args list - simply return that expression
            if (newNodeArgs.Count == 1)
            {
                newNode = newNodeArgs[0];
            }
            else
            {
                newNode = context.Command.CreateNode(caseOp, newNodeArgs);
            }

            return true;
        }

        /// <summary>
        ///     If the else clause of the CaseOp is another CaseOp, when two can be collapsed into one. 
        ///     In particular, 
        /// 
        ///     CASE 
        ///     WHEN W1 THEN T1 
        ///     WHEN W2 THEN T2 ... 
        ///     ELSE (CASE 
        ///     WHEN WN1 THEN TN1, … 
        ///     ELSE E) 
        ///             
        ///     Is transformed into 
        /// 
        ///     CASE 
        ///     WHEN W1 THEN T1 
        ///     WHEN W2 THEN T2 ...
        ///     WHEN WN1  THEN TN1 ...
        ///     ELSE E
        /// </summary>
        /// <param name="caseOp"> the current caseOp </param>
        /// <param name="caseOpNode"> current subtree </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> true, if we performed a transformation </returns>
        private static bool ProcessFlattenCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
        {
            newNode = caseOpNode;
            var elseChild = caseOpNode.Children[caseOpNode.Children.Count - 1];
            if (elseChild.Op.OpType
                != OpType.Case)
            {
                return false;
            }

            // 
            // Flatten the case statements.
            // The else child is removed from the outer CaseOp op
            // and the else child's children are reparented to the outer CaseOp
            // Node info recomputation does not need to happen, the outer CaseOp
            // node still has the same descendants.
            //
            caseOpNode.Children.RemoveAt(caseOpNode.Children.Count - 1);
            caseOpNode.Children.AddRange(elseChild.Children);

            return true;
        }

        #endregion

        #region EqualsOverConstant Rules

        internal static readonly PatternMatchRule Rule_EqualsOverConstant =
            new PatternMatchRule(
                new Node(
                    ComparisonOp.PatternEq,
                    new Node(InternalConstantOp.Pattern),
                    new Node(InternalConstantOp.Pattern)),
                ProcessComparisonsOverConstant);

        /// <summary>
        ///     Convert an Equals(X, Y) to a "true" predicate if X=Y, or a "false" predicate if X!=Y
        ///     Convert a NotEquals(X,Y) in the reverse fashion
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="node"> current node </param>
        /// <param name="newNode"> possibly modified subtree </param>
        /// <returns> true, if transformation was successful </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessComparisonsOverConstant(RuleProcessingContext context, Node node, out Node newNode)
        {
            newNode = node;
            PlanCompiler.Assert(node.Op.OpType == OpType.EQ || node.Op.OpType == OpType.NE, "unexpected comparison op type?");

            bool? comparisonStatus = node.Child0.Op.IsEquivalent(node.Child1.Op);
            // Don't mess with nulls or with non-internal constants
            if (comparisonStatus == null)
            {
                return false;
            }
            var result = (node.Op.OpType == OpType.EQ) ? (bool)comparisonStatus : !((bool)comparisonStatus);
            var newOp = context.Command.CreateConstantPredicateOp(result);
            newNode = context.Command.CreateNode(newOp);
            return true;
        }

        #endregion

        #region LikeOp Rules

        private static bool? MatchesPattern(string str, string pattern)
        {
            // What we're trying to see is if the pattern is something that ends with a '%'
            // And if the "str" is something that matches everything before that

            // Make sure that the terminal character of the pattern is a '%' character. Also
            // ensure that this character does not occur anywhere else. And finally, ensure
            // that the pattern is atmost one character longer than the string itself
            var wildCardIndex = pattern.IndexOf('%');
            if ((wildCardIndex == -1) ||
                (wildCardIndex != pattern.Length - 1)
                ||
                (pattern.Length > str.Length + 1))
            {
                return null;
            }

            var match = true;

            var i = 0;
            for (i = 0; i < str.Length && i < pattern.Length - 1; i++)
            {
                if (pattern[i]
                    != str[i])
                {
                    match = false;
                    break;
                }
            }

            return match;
        }

        internal static readonly PatternMatchRule Rule_LikeOverConstants =
            new PatternMatchRule(
                new Node(
                    LikeOp.Pattern,
                    new Node(InternalConstantOp.Pattern),
                    new Node(InternalConstantOp.Pattern),
                    new Node(NullOp.Pattern)),
                ProcessLikeOverConstant);

        private static bool ProcessLikeOverConstant(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var patternOp = (InternalConstantOp)n.Child1.Op;
            var strOp = (InternalConstantOp)n.Child0.Op;

            var str = (string)strOp.Value;
            var pattern = (string)patternOp.Value;

            var match = MatchesPattern((string)strOp.Value, (string)patternOp.Value);
            if (match == null)
            {
                return false;
            }

            var constOp = context.Command.CreateConstantPredicateOp((bool)match);
            newNode = context.Command.CreateNode(constOp);
            return true;
        }

        #endregion

        #region LogicalOp (and,or,not) Rules

        internal static readonly PatternMatchRule Rule_AndOverConstantPred1 =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternAnd,
                    new Node(LeafOp.Pattern),
                    new Node(ConstantPredicateOp.Pattern)),
                ProcessAndOverConstantPredicate1);

        internal static readonly PatternMatchRule Rule_AndOverConstantPred2 =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternAnd,
                    new Node(ConstantPredicateOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessAndOverConstantPredicate2);

        internal static readonly PatternMatchRule Rule_OrOverConstantPred1 =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternOr,
                    new Node(LeafOp.Pattern),
                    new Node(ConstantPredicateOp.Pattern)),
                ProcessOrOverConstantPredicate1);

        internal static readonly PatternMatchRule Rule_OrOverConstantPred2 =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternOr,
                    new Node(ConstantPredicateOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessOrOverConstantPredicate2);

        internal static readonly PatternMatchRule Rule_NotOverConstantPred =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternNot,
                    new Node(ConstantPredicateOp.Pattern)),
                ProcessNotOverConstantPredicate);

        /// <summary>
        ///     Transform 
        ///     AND(x, true) => x;
        ///     AND(true, x) => x
        ///     AND(x, false) => false
        ///     AND(false, x) => false
        /// </summary>
        /// <param name="context"> Rule Processing context </param>
        /// <param name="node"> Current LogOp (And, Or, Not) node </param>
        /// <param name="constantPredicateNode"> constant predicate node </param>
        /// <param name="otherNode"> The other child of the LogOp (possibly null) </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> transformation status </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "constantPredicateOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessLogOpOverConstant(
            RuleProcessingContext context, Node node,
            Node constantPredicateNode, Node otherNode,
            out Node newNode)
        {
            PlanCompiler.Assert(constantPredicateNode != null, "null constantPredicateOp?");
            var pred = (ConstantPredicateOp)constantPredicateNode.Op;

            switch (node.Op.OpType)
            {
                case OpType.And:
                    newNode = pred.IsTrue ? otherNode : constantPredicateNode;
                    break;
                case OpType.Or:
                    newNode = pred.IsTrue ? constantPredicateNode : otherNode;
                    break;
                case OpType.Not:
                    PlanCompiler.Assert(otherNode == null, "Not Op with more than 1 child. Gasp!");
                    newNode = context.Command.CreateNode(context.Command.CreateConstantPredicateOp(!pred.Value));
                    break;
                default:
                    PlanCompiler.Assert(false, "Unexpected OpType - " + node.Op.OpType);
                    newNode = null;
                    break;
            }
            return true;
        }

        private static bool ProcessAndOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
        }

        private static bool ProcessAndOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
        }

        private static bool ProcessOrOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
        }

        private static bool ProcessOrOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
        }

        private static bool ProcessNotOverConstantPredicate(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, null, out newNode);
        }

        #endregion

        #region IsNull Rules

        internal static readonly PatternMatchRule Rule_IsNullOverConstant =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternIsNull,
                    new Node(InternalConstantOp.Pattern)),
                ProcessIsNullOverConstant);

        internal static readonly PatternMatchRule Rule_IsNullOverNullSentinel =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternIsNull,
                    new Node(NullSentinelOp.Pattern)),
                ProcessIsNullOverConstant);

        /// <summary>
        ///     Convert a 
        ///     IsNull(constant) 
        ///     to just the 
        ///     False predicate
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="isNullNode"> </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> </returns>
        private static bool ProcessIsNullOverConstant(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateFalseOp());
            return true;
        }

        internal static readonly PatternMatchRule Rule_IsNullOverNull =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternIsNull,
                    new Node(NullOp.Pattern)),
                ProcessIsNullOverNull);

        /// <summary>
        ///     Convert an IsNull(null) to just the 'true' predicate
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="isNullNode"> </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> </returns>
        private static bool ProcessIsNullOverNull(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateTrueOp());
            return true;
        }

        #endregion

        #region CastOp(NullOp) Rule

        internal static readonly PatternMatchRule Rule_NullCast = new PatternMatchRule(
            new Node(
                CastOp.Pattern,
                new Node(NullOp.Pattern)),
            ProcessNullCast);

        /// <summary>
        ///     eliminates nested null casts into a single cast of the outermost cast type.
        ///     basically the transformation applied is: cast(null[x] as T) => null[t]
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="castNullOp"> </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> </returns>
        private static bool ProcessNullCast(RuleProcessingContext context, Node castNullOp, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateNullOp(castNullOp.Op.Type));
            return true;
        }

        #endregion

        #region IsNull over VarRef

        internal static readonly PatternMatchRule Rule_IsNullOverVarRef =
            new PatternMatchRule(
                new Node(
                    ConditionalOp.PatternIsNull,
                    new Node(VarRefOp.Pattern)),
                ProcessIsNullOverVarRef);

        /// <summary>
        ///     Convert a 
        ///     IsNull(VarRef(v)) 
        ///     to just the 
        ///     False predicate
        ///    
        ///     if v is guaranteed to be non nullable.
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="isNullNode"> </param>
        /// <param name="newNode"> new subtree </param>
        /// <returns> </returns>
        private static bool ProcessIsNullOverVarRef(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            var command = context.Command;
            var trc = (TransformationRulesContext)context;

            var v = ((VarRefOp)isNullNode.Child0.Op).Var;

            if (trc.IsNonNullable(v))
            {
                newNode = command.CreateNode(context.Command.CreateFalseOp());
                return true;
            }
            else
            {
                newNode = isNullNode;
                return false;
            }
        }

        #endregion

        #region All ScalarOp Rules

        internal static readonly Rule[] Rules = new Rule[]
                                                    {
                                                        Rule_SimplifyCase,
                                                        Rule_FlattenCase,
                                                        Rule_LikeOverConstants,
                                                        Rule_EqualsOverConstant,
                                                        Rule_AndOverConstantPred1,
                                                        Rule_AndOverConstantPred2,
                                                        Rule_OrOverConstantPred1,
                                                        Rule_OrOverConstantPred2,
                                                        Rule_NotOverConstantPred,
                                                        Rule_IsNullOverConstant,
                                                        Rule_IsNullOverNullSentinel,
                                                        Rule_IsNullOverNull,
                                                        Rule_NullCast,
                                                        Rule_IsNullOverVarRef,
                                                    };

        #endregion
    }
}
