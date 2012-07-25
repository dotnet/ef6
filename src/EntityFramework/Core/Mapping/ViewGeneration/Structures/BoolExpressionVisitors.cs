// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using BoolDomainConstraint = System.Data.Entity.Core.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>;
    using DomainAndExpr = System.Data.Entity.Core.Common.Utils.Boolean.AndExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>
        ;
    using DomainBoolExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainFalseExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.FalseExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainNotExpr = System.Data.Entity.Core.Common.Utils.Boolean.NotExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>
        ;
    using DomainOrExpr = System.Data.Entity.Core.Common.Utils.Boolean.OrExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.TermExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTreeExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.TreeExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTrueExpr =
        System.Data.Entity.Core.Common.Utils.Boolean.TrueExpr<Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    // This class represents an arbitrary boolean expression
    internal partial class BoolExpression : InternalBase
    {
        #region FixRangeVisitor

        // A visitor that "fixes" the OneOfConsts according to the value of
        // the Range in the DomainConstraint
        private class FixRangeVisitor : BasicVisitor<BoolDomainConstraint>
        {
            #region Constructor/Fields/Invocation

            private FixRangeVisitor(MemberDomainMap memberDomainMap)
            {
                m_memberDomainMap = memberDomainMap;
            }

            private readonly MemberDomainMap m_memberDomainMap;

            // effects: Given expression and the domains of various members,
            // ensures that the range in OneOfConsts is in line with the
            // DomainConstraints in expression
            internal static DomainBoolExpr FixRange(DomainBoolExpr expression, MemberDomainMap memberDomainMap)
            {
                var visitor = new FixRangeVisitor(memberDomainMap);
                var result = expression.Accept(visitor);
                return result;
            }

            #endregion

            #region Visitors

            // The real work happens here in the literal's FixRange
            internal override DomainBoolExpr VisitTerm(DomainTermExpr expression)
            {
                var literal = GetBoolLiteral(expression);
                var result = literal.FixRange(expression.Identifier.Range, m_memberDomainMap);
                return result;
            }

            #endregion
        }

        #endregion

        #region IsFinalVisitor

        // A Visitor that determines if the OneOfConsts in this are complete or not
        private class IsFinalVisitor : Visitor<BoolDomainConstraint, bool>
        {
            internal static bool IsFinal(DomainBoolExpr expression)
            {
                var visitor = new IsFinalVisitor();
                return expression.Accept(visitor);
            }

            #region Visitors

            internal override bool VisitTrue(DomainTrueExpr expression)
            {
                return true;
            }

            internal override bool VisitFalse(DomainFalseExpr expression)
            {
                return true;
            }

            // Check if the oneOfConst is complete or not
            internal override bool VisitTerm(DomainTermExpr expression)
            {
                var literal = GetBoolLiteral(expression);
                var restriction = literal as MemberRestriction;
                var result = restriction == null || restriction.IsComplete;
                return result;
            }

            internal override bool VisitNot(DomainNotExpr expression)
            {
                return expression.Child.Accept(this);
            }

            internal override bool VisitAnd(DomainAndExpr expression)
            {
                return VisitAndOr(expression);
            }

            internal override bool VisitOr(DomainOrExpr expression)
            {
                return VisitAndOr(expression);
            }

            private bool VisitAndOr(DomainTreeExpr expression)
            {
                // If any child is not final, tree is not final -- we cannot
                // have a mix of final and non-final trees!
                var isFirst = true;
                var result = true;
                foreach (var child in expression.Children)
                {
                    if (child as DomainFalseExpr != null
                        || child as DomainTrueExpr != null)
                    {
                        // Ignore true or false since they carry no information
                        continue;
                    }
                    var isChildFinal = child.Accept(this);
                    if (isFirst)
                    {
                        result = isChildFinal;
                    }
                    Debug.Assert(result == isChildFinal, "All children must be final or non-final");
                    isFirst = false;
                }
                return result;
            }

            #endregion
        }

        #endregion

        #region RemapBoolVisitor

        // A visitor that remaps the JoinTreeNodes in a bool tree
        private class RemapBoolVisitor : BasicVisitor<BoolDomainConstraint>
        {
            #region Constructor/Fields/Invocation

            // effects: Creates a visitor with the JoinTreeNode remapping
            // information in remap
            private RemapBoolVisitor(MemberDomainMap memberDomainMap, Dictionary<MemberPath, MemberPath> remap)
            {
                m_remap = remap;
                m_memberDomainMap = memberDomainMap;
            }

            private readonly Dictionary<MemberPath, MemberPath> m_remap;
            private readonly MemberDomainMap m_memberDomainMap;

            internal static DomainBoolExpr RemapExtentTreeNodes(
                DomainBoolExpr expression, MemberDomainMap memberDomainMap,
                Dictionary<MemberPath, MemberPath> remap)
            {
                var visitor = new RemapBoolVisitor(memberDomainMap, remap);
                var result = expression.Accept(visitor);
                return result;
            }

            #endregion

            #region Visitors

            // The real work happens here in the literal's RemapBool
            internal override DomainBoolExpr VisitTerm(DomainTermExpr expression)
            {
                var literal = GetBoolLiteral(expression);
                var newLiteral = literal.RemapBool(m_remap);
                return newLiteral.GetDomainBoolExpression(m_memberDomainMap);
            }

            #endregion
        }

        #endregion

        #region RequiredSlotsVisitor

        // A visitor that determines the slots required in the whole tree (for
        // CQL Generation)
        private class RequiredSlotsVisitor : BasicVisitor<BoolDomainConstraint>
        {
            #region Constructor/Fields/Invocation

            private RequiredSlotsVisitor(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
            {
                m_projectedSlotMap = projectedSlotMap;
                m_requiredSlots = requiredSlots;
            }

            private readonly MemberProjectionIndex m_projectedSlotMap;
            private readonly bool[] m_requiredSlots;

            internal static void GetRequiredSlots(
                DomainBoolExpr expression, MemberProjectionIndex projectedSlotMap,
                bool[] requiredSlots)
            {
                var visitor = new RequiredSlotsVisitor(projectedSlotMap, requiredSlots);
                expression.Accept(visitor);
            }

            #endregion

            #region Visitors

            // The real work happends here - the slots are obtained from the literal
            internal override DomainBoolExpr VisitTerm(DomainTermExpr expression)
            {
                var literal = GetBoolLiteral(expression);
                literal.GetRequiredSlots(m_projectedSlotMap, m_requiredSlots);
                return expression;
            }

            #endregion
        }

        #endregion

        // A Visitor that determines the CQL format of this expression

        #region AsCqlVisitor

        #region AsEsqlVisitor

        private sealed class AsEsqlVisitor : AsCqlVisitor<StringBuilder>
        {
            internal static StringBuilder AsEsql(DomainBoolExpr expression, StringBuilder builder, string blockAlias)
            {
                var visitor = new AsEsqlVisitor(builder, blockAlias);
                return expression.Accept(visitor);
            }

            #region Constructor/Fields

            private AsEsqlVisitor(StringBuilder builder, string blockAlias)
            {
                m_builder = builder;
                m_blockAlias = blockAlias;
            }

            private readonly StringBuilder m_builder;
            private readonly string m_blockAlias;

            #endregion

            #region Visitors

            internal override StringBuilder VisitTrue(DomainTrueExpr expression)
            {
                m_builder.Append("True");
                return m_builder;
            }

            internal override StringBuilder VisitFalse(DomainFalseExpr expression)
            {
                m_builder.Append("False");
                return m_builder;
            }

            protected override StringBuilder BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull)
            {
                return literal.AsEsql(m_builder, m_blockAlias, skipIsNotNull);
            }

            protected override StringBuilder NotExprAsCql(DomainNotExpr expression)
            {
                m_builder.Append("NOT(");
                expression.Child.Accept(this); // we do not need the returned StringBuilder -- it is the same as m_builder
                m_builder.Append(")");
                return m_builder;
            }

            internal override StringBuilder VisitAnd(DomainAndExpr expression)
            {
                return VisitAndOr(expression, ExprType.And);
            }

            internal override StringBuilder VisitOr(DomainOrExpr expression)
            {
                return VisitAndOr(expression, ExprType.Or);
            }

            private StringBuilder VisitAndOr(DomainTreeExpr expression, ExprType kind)
            {
                Debug.Assert(kind == ExprType.Or || kind == ExprType.And);

                m_builder.Append('(');
                var isFirstChild = true;
                foreach (var child in expression.Children)
                {
                    if (false == isFirstChild)
                    {
                        // Add the operator
                        if (kind == ExprType.And)
                        {
                            m_builder.Append(" AND ");
                        }
                        else
                        {
                            m_builder.Append(" OR ");
                        }
                    }
                    isFirstChild = false;
                    // Recursively get the CQL for the child
                    child.Accept(this);
                }
                m_builder.Append(')');
                return m_builder;
            }

            #endregion
        }

        #endregion

        #region AsCqtVisitor

        private sealed class AsCqtVisitor : AsCqlVisitor<DbExpression>
        {
            internal static DbExpression AsCqt(DomainBoolExpr expression, DbExpression row)
            {
                var visitor = new AsCqtVisitor(row);
                return expression.Accept(visitor);
            }

            #region Constructor/Fields

            private AsCqtVisitor(DbExpression row)
            {
                m_row = row;
            }

            private readonly DbExpression m_row;

            #endregion

            #region Visitors

            internal override DbExpression VisitTrue(DomainTrueExpr expression)
            {
                return DbExpressionBuilder.True;
            }

            internal override DbExpression VisitFalse(DomainFalseExpr expression)
            {
                return DbExpressionBuilder.False;
            }

            protected override DbExpression BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull)
            {
                return literal.AsCqt(m_row, skipIsNotNull);
            }

            protected override DbExpression NotExprAsCql(DomainNotExpr expression)
            {
                var cqt = expression.Child.Accept(this);
                return cqt.Not();
            }

            internal override DbExpression VisitAnd(DomainAndExpr expression)
            {
                var cqt = VisitAndOr(expression, DbExpressionBuilder.And);
                Debug.Assert(cqt != null, "AND must have at least one child");
                return cqt;
            }

            internal override DbExpression VisitOr(DomainOrExpr expression)
            {
                var cqt = VisitAndOr(expression, DbExpressionBuilder.Or);
                Debug.Assert(cqt != null, "OR must have at least one child");
                return cqt;
            }

            private DbExpression VisitAndOr(DomainTreeExpr expression, Func<DbExpression, DbExpression, DbExpression> op)
            {
                DbExpression cqt = null;
                foreach (var child in expression.Children)
                {
                    if (cqt == null)
                    {
                        cqt = child.Accept(this);
                    }
                    else
                    {
                        cqt = op(cqt, child.Accept(this));
                    }
                }
                return cqt;
            }

            #endregion
        }

        #endregion

        #region AsCqlVisitor

        private abstract class AsCqlVisitor<T_Return> : Visitor<BoolDomainConstraint, T_Return>
        {
            #region Constructor

            protected AsCqlVisitor()
            {
                // All boolean expressions can evaluate to true or not true
                // (i.e., false or unknown) whether it is in CASE statements
                // or WHERE clauses
                m_skipIsNotNull = true;
            }

            // We could maintain a stack of bools ratehr than a single
            // boolean for the visitor to allow IS NOT NULLs to be not
            // generated for some scenarios
            private bool m_skipIsNotNull;

            #endregion

            #region Visitors

            internal override T_Return VisitTerm(DomainTermExpr expression)
            {
                // If m_skipIsNotNull is true at this point, it means that no ancestor of this
                // node is OR or NOT
                var literal = GetBoolLiteral(expression);
                return BooleanLiteralAsCql(literal, m_skipIsNotNull);
            }

            protected abstract T_Return BooleanLiteralAsCql(BoolLiteral literal, bool skipIsNotNull);

            internal override T_Return VisitNot(DomainNotExpr expression)
            {
                m_skipIsNotNull = false; // Cannot skip in NOTs
                return NotExprAsCql(expression);
            }

            protected abstract T_Return NotExprAsCql(DomainNotExpr expression);

            #endregion
        }

        #endregion

        #endregion

        // A Visitor that produces User understandable string of the given configuration represented by the BooleanExpression

        #region AsUserStringVisitor

        private class AsUserStringVisitor : Visitor<BoolDomainConstraint, StringBuilder>
        {
            #region Constructor/Fields/Invocation

            private AsUserStringVisitor(StringBuilder builder, string blockAlias)
            {
                m_builder = builder;
                m_blockAlias = blockAlias;
                // All boolean expressions can evaluate to true or not true
                // (i.e., false or unknown) whether it is in CASE statements
                // or WHERE clauses
                m_skipIsNotNull = true;
            }

            private readonly StringBuilder m_builder;
            private readonly string m_blockAlias;
            // We could maintain a stack of bools ratehr than a single
            // boolean for the visitor to allow IS NOT NULLs to be not
            // generated for some scenarios
            private bool m_skipIsNotNull;

            internal static StringBuilder AsUserString(DomainBoolExpr expression, StringBuilder builder, string blockAlias)
            {
                var visitor = new AsUserStringVisitor(builder, blockAlias);
                return expression.Accept(visitor);
            }

            #endregion

            #region Visitors

            internal override StringBuilder VisitTrue(DomainTrueExpr expression)
            {
                m_builder.Append("True");
                return m_builder;
            }

            internal override StringBuilder VisitFalse(DomainFalseExpr expression)
            {
                m_builder.Append("False");
                return m_builder;
            }

            internal override StringBuilder VisitTerm(DomainTermExpr expression)
            {
                // If m_skipIsNotNull is true at this point, it means that no ancestor of this
                // node is OR or NOT

                var literal = GetBoolLiteral(expression);

                if (literal is ScalarRestriction
                    || literal is TypeRestriction)
                {
                    return literal.AsUserString(m_builder, Strings.ViewGen_EntityInstanceToken, m_skipIsNotNull);
                }

                return literal.AsUserString(m_builder, m_blockAlias, m_skipIsNotNull);
            }

            internal override StringBuilder VisitNot(DomainNotExpr expression)
            {
                m_skipIsNotNull = false; // Cannot skip in NOTs

                var termExpr = expression.Child as DomainTermExpr;
                if (termExpr != null)
                {
                    var literal = GetBoolLiteral(termExpr);
                    return literal.AsNegatedUserString(m_builder, m_blockAlias, m_skipIsNotNull);
                }
                else
                {
                    m_builder.Append("NOT(");
                    // We do not need the returned StringBuilder -- it is the same as m_builder
                    expression.Child.Accept(this);
                    m_builder.Append(")");
                }
                return m_builder;
            }

            internal override StringBuilder VisitAnd(DomainAndExpr expression)
            {
                return VisitAndOr(expression, ExprType.And);
            }

            internal override StringBuilder VisitOr(DomainOrExpr expression)
            {
                return VisitAndOr(expression, ExprType.Or);
            }

            private StringBuilder VisitAndOr(DomainTreeExpr expression, ExprType kind)
            {
                Debug.Assert(kind == ExprType.Or || kind == ExprType.And);

                m_builder.Append('(');
                var isFirstChild = true;
                foreach (var child in expression.Children)
                {
                    if (false == isFirstChild)
                    {
                        // Add the operator
                        if (kind == ExprType.And)
                        {
                            m_builder.Append(" AND ");
                        }
                        else
                        {
                            m_builder.Append(" OR ");
                        }
                    }
                    isFirstChild = false;
                    // Recursively get the CQL for the child
                    child.Accept(this);
                }
                m_builder.Append(')');
                return m_builder;
            }

            #endregion
        }

        #endregion

        // Given an expression that has no NOTs or ORs (if allowAllOperators
        // is false in GetTerms), generates the terms  in it

        #region TermVisitor

        private class TermVisitor : Visitor<BoolDomainConstraint, IEnumerable<DomainTermExpr>>
        {
            #region Constructor/Fields/Invocation

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "allowAllOperators", Scope = "member",
                Target = "System.Data.Entity.Core.Mapping.ViewGeneration.Structures.BoolExpression+TermVisitor.#.ctor(System.Boolean)")]
            private TermVisitor(bool allowAllOperators)
            {
#if DEBUG
                m_allowAllOperators = allowAllOperators;
#endif
            }

            // effectS: Returns all the terms in expression. If
            // allowAllOperators is true, ensures that there are no NOTs or ORs
            internal static IEnumerable<DomainTermExpr> GetTerms(DomainBoolExpr expression, bool allowAllOperators)
            {
                var visitor = new TermVisitor(allowAllOperators);
                return expression.Accept(visitor);
            }

            #endregion

            #region Fields

#if DEBUG
            private readonly bool m_allowAllOperators;
#endif

            #endregion

            #region Visitors

            internal override IEnumerable<DomainTermExpr> VisitTrue(DomainTrueExpr expression)
            {
                yield break; // No Atoms here -- we are not looking for constants
            }

            internal override IEnumerable<DomainTermExpr> VisitFalse(DomainFalseExpr expression)
            {
                yield break; // No Atoms here -- we are not looking for constants
            }

            internal override IEnumerable<DomainTermExpr> VisitTerm(DomainTermExpr expression)
            {
                yield return expression;
            }

            internal override IEnumerable<DomainTermExpr> VisitNot(DomainNotExpr expression)
            {
#if DEBUG
                Debug.Assert(m_allowAllOperators, "Term should not be called when Nots are present in the expression");
#endif
                return VisitTreeNode(expression);
            }

            private IEnumerable<DomainTermExpr> VisitTreeNode(DomainTreeExpr expression)
            {
                foreach (var child in expression.Children)
                {
                    foreach (var result in child.Accept(this))
                    {
                        yield return result;
                    }
                }
            }

            internal override IEnumerable<DomainTermExpr> VisitAnd(DomainAndExpr expression)
            {
                return VisitTreeNode(expression);
            }

            internal override IEnumerable<DomainTermExpr> VisitOr(DomainOrExpr expression)
            {
#if DEBUG
                Debug.Assert(m_allowAllOperators, "TermVisitor should not be called when Ors are present in the expression");
#endif
                return VisitTreeNode(expression);
            }

            #endregion
        }

        #endregion

        #region CompactStringVisitor

        // Generates a human readable version of the expression and places it in
        // the StringBuilder
        private class CompactStringVisitor : Visitor<BoolDomainConstraint, StringBuilder>
        {
            #region Constructor/Fields/Invocation

            private CompactStringVisitor(StringBuilder builder)
            {
                m_builder = builder;
            }

            private StringBuilder m_builder;

            internal static StringBuilder ToBuilder(DomainBoolExpr expression, StringBuilder builder)
            {
                var visitor = new CompactStringVisitor(builder);
                return expression.Accept(visitor);
            }

            #endregion

            #region Visitors

            internal override StringBuilder VisitTrue(DomainTrueExpr expression)
            {
                m_builder.Append("True");
                return m_builder;
            }

            internal override StringBuilder VisitFalse(DomainFalseExpr expression)
            {
                m_builder.Append("False");
                return m_builder;
            }

            internal override StringBuilder VisitTerm(DomainTermExpr expression)
            {
                var literal = GetBoolLiteral(expression);
                literal.ToCompactString(m_builder);
                return m_builder;
            }

            internal override StringBuilder VisitNot(DomainNotExpr expression)
            {
                m_builder.Append("NOT(");
                expression.Child.Accept(this);
                m_builder.Append(")");
                return m_builder;
            }

            internal override StringBuilder VisitAnd(DomainAndExpr expression)
            {
                return VisitAndOr(expression, "AND");
            }

            internal override StringBuilder VisitOr(DomainOrExpr expression)
            {
                return VisitAndOr(expression, "OR");
            }

            private StringBuilder VisitAndOr(DomainTreeExpr expression, string opAsString)
            {
                var childrenStrings = new List<string>();
                var builder = m_builder;
                // Save the old string builder and pass a new one to each child
                foreach (var child in expression.Children)
                {
                    m_builder = new StringBuilder();
                    child.Accept(this);
                    childrenStrings.Add(m_builder.ToString());
                }
                // Now store the children in a sorted manner
                m_builder = builder;
                m_builder.Append('(');
                StringUtil.ToSeparatedStringSorted(m_builder, childrenStrings, " " + opAsString + " ");
                m_builder.Append(')');
                return m_builder;
            }

            #endregion
        }

        #endregion
    }
}
