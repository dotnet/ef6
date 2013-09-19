// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents to the various Join nodes in the view: IJ, LOJ, FOJ.
    /// </summary>
    internal sealed class JoinCqlBlock : CqlBlock
    {
        /// <summary>
        /// Creates a join block (type given by <paramref name="opType" />) with SELECT (<paramref name="slotInfos" />), FROM (
        /// <paramref
        ///     name="children" />
        /// ),
        /// ON (<paramref name="onClauses" /> - one for each child except 0th), WHERE (true), AS (
        /// <paramref
        ///     name="blockAliasNum" />
        /// ).
        /// </summary>
        internal JoinCqlBlock(
            CellTreeOpType opType,
            SlotInfo[] slotInfos,
            List<CqlBlock> children,
            List<OnClause> onClauses,
            CqlIdentifiers identifiers,
            int blockAliasNum)
            : base(slotInfos, children, BoolExpression.True, identifiers, blockAliasNum)
        {
            m_opType = opType;
            m_onClauses = onClauses;
        }

        private readonly CellTreeOpType m_opType;
        private readonly List<OnClause> m_onClauses;

        internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
        {
            // The SELECT part.
            StringUtil.IndentNewLine(builder, indentLevel);
            builder.Append("SELECT ");
            GenerateProjectionEsql(
                builder,
                null,
                /* There is no single input, so the blockAlias is null. ProjectedSlot objects will have to carry their own input block info:
                       * see QualifiedSlot and QualifiedCellIdBoolean for more info. */
                false,
                indentLevel,
                isTopLevel);
            StringUtil.IndentNewLine(builder, indentLevel);

            // The FROM part by joining all the children using ON Clauses.
            builder.Append("FROM ");
            var i = 0;
            foreach (var child in Children)
            {
                if (i > 0)
                {
                    StringUtil.IndentNewLine(builder, indentLevel + 1);
                    builder.Append(OpCellTreeNode.OpToEsql(m_opType));
                }
                builder.Append(" (");
                child.AsEsql(builder, false, indentLevel + 1);
                builder.Append(") AS ")
                       .Append(child.CqlAlias);

                // The ON part.
                if (i > 0)
                {
                    StringUtil.IndentNewLine(builder, indentLevel + 1);
                    builder.Append("ON ");
                    m_onClauses[i - 1].AsEsql(builder);
                }
                i++;
            }
            return builder;
        }

        internal override DbExpression AsCqt(bool isTopLevel)
        {
            // The FROM part:
            //  - build a tree of binary joins out of the inputs (this.Children).
            //  - update each child block with its relative position in the join tree, 
            //    so that QualifiedSlot and QualifiedCellIdBoolean objects could find their 
            //    designated block areas inside the cumulative join row passed into their AsCqt(row) method.
            var leftmostBlock = Children[0];
            var left = leftmostBlock.AsCqt(false);
            var joinTreeCtxParentQualifiers = new List<string>();
            for (var i = 1; i < Children.Count; ++i)
            {
                // Join the current left expression (a tree) to the current right block.
                var rightBlock = Children[i];
                var right = rightBlock.AsCqt(false);
                Func<DbExpression, DbExpression, DbExpression> joinConditionFunc = m_onClauses[i - 1].AsCqt;
                DbJoinExpression join;
                switch (m_opType)
                {
                    case CellTreeOpType.FOJ:
                        join = left.FullOuterJoin(right, joinConditionFunc);
                        break;
                    case CellTreeOpType.IJ:
                        join = left.InnerJoin(right, joinConditionFunc);
                        break;
                    case CellTreeOpType.LOJ:
                        join = left.LeftOuterJoin(right, joinConditionFunc);
                        break;
                    default:
                        Debug.Fail("Unknown operator");
                        return null;
                }

                if (i == 1)
                {
                    // Assign the joinTreeContext to the leftmost block.
                    leftmostBlock.SetJoinTreeContext(joinTreeCtxParentQualifiers, join.Left.VariableName);
                }
                else
                {
                    // Update the joinTreeCtxParentQualifiers.
                    // Note that all blocks that already participate in the left expression tree share the same copy of the joinTreeContext.
                    joinTreeCtxParentQualifiers.Add(join.Left.VariableName);
                }

                // Assign the joinTreeContext to the right block.
                rightBlock.SetJoinTreeContext(joinTreeCtxParentQualifiers, join.Right.VariableName);

                left = join;
            }

            // The SELECT part.
            return left.Select(row => GenerateProjectionCqt(row, false));
        }

        /// <summary>
        /// Represents a complete ON clause "slot1 == slot2 AND "slot3 == slot4" ... for two <see cref="JoinCqlBlock" />s.
        /// </summary>
        internal sealed class OnClause : InternalBase
        {
            internal OnClause()
            {
                m_singleClauses = new List<SingleClause>();
            }

            private readonly List<SingleClause> m_singleClauses;

            /// <summary>
            /// Adds an <see cref="SingleClause" /> element for a join of the form <paramref name="leftSlot" /> =
            /// <paramref
            ///     name="rightSlot" />
            /// .
            /// </summary>
            internal void Add(
                QualifiedSlot leftSlot, MemberPath leftSlotOutputMember, QualifiedSlot rightSlot, MemberPath rightSlotOutputMember)
            {
                var singleClause = new SingleClause(leftSlot, leftSlotOutputMember, rightSlot, rightSlotOutputMember);
                m_singleClauses.Add(singleClause);
            }

            /// <summary>
            /// Generates eSQL string of the form "LeftSlot1 = RightSlot1 AND LeftSlot2 = RightSlot2 AND ...
            /// </summary>
            internal StringBuilder AsEsql(StringBuilder builder)
            {
                var isFirst = true;
                foreach (var singleClause in m_singleClauses)
                {
                    if (false == isFirst)
                    {
                        builder.Append(" AND ");
                    }
                    singleClause.AsEsql(builder);
                    isFirst = false;
                }
                return builder;
            }

            /// <summary>
            /// Generates CQT of the form "LeftSlot1 = RightSlot1 AND LeftSlot2 = RightSlot2 AND ...
            /// </summary>
            internal DbExpression AsCqt(DbExpression leftRow, DbExpression rightRow)
            {
                var cqt = m_singleClauses[0].AsCqt(leftRow, rightRow);
                for (var i = 1; i < m_singleClauses.Count; ++i)
                {
                    cqt = cqt.And(m_singleClauses[i].AsCqt(leftRow, rightRow));
                }
                return cqt;
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append("ON ");
                StringUtil.ToSeparatedString(builder, m_singleClauses, " AND ");
            }

            /// <summary>
            /// Represents an expression between slots of the form: LeftSlot = RightSlot
            /// </summary>
            private sealed class SingleClause : InternalBase
            {
                internal SingleClause(
                    QualifiedSlot leftSlot, MemberPath leftSlotOutputMember, QualifiedSlot rightSlot, MemberPath rightSlotOutputMember)
                {
                    m_leftSlot = leftSlot;
                    m_leftSlotOutputMember = leftSlotOutputMember;
                    m_rightSlot = rightSlot;
                    m_rightSlotOutputMember = rightSlotOutputMember;
                }

                private readonly QualifiedSlot m_leftSlot;
                private readonly MemberPath m_leftSlotOutputMember;
                private readonly QualifiedSlot m_rightSlot;
                private readonly MemberPath m_rightSlotOutputMember;

                /// <summary>
                /// Generates eSQL string of the form "leftSlot = rightSlot".
                /// </summary>
                internal StringBuilder AsEsql(StringBuilder builder)
                {
                    builder.Append(m_leftSlot.GetQualifiedCqlName(m_leftSlotOutputMember))
                           .Append(" = ")
                           .Append(m_rightSlot.GetQualifiedCqlName(m_rightSlotOutputMember));
                    return builder;
                }

                /// <summary>
                /// Generates CQT of the form "leftSlot = rightSlot".
                /// </summary>
                internal DbExpression AsCqt(DbExpression leftRow, DbExpression rightRow)
                {
                    return m_leftSlot.AsCqt(leftRow, m_leftSlotOutputMember).Equal(m_rightSlot.AsCqt(rightRow, m_rightSlotOutputMember));
                }

                internal override void ToCompactString(StringBuilder builder)
                {
                    m_leftSlot.ToCompactString(builder);
                    builder.Append(" = ");
                    m_rightSlot.ToCompactString(builder);
                }
            }
        }
    }
}
