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

    // <summary>
    // Represents Union nodes in the <see cref="CqlBlock" /> tree.
    // </summary>
    internal sealed class UnionCqlBlock : CqlBlock
    {
        // <summary>
        // Creates a union block with SELECT (<paramref name="slotInfos" />), FROM (<paramref name="children" />), WHERE (true), AS (
        // <paramref
        //     name="blockAliasNum" />
        // ).
        // </summary>
        internal UnionCqlBlock(SlotInfo[] slotInfos, List<CqlBlock> children, CqlIdentifiers identifiers, int blockAliasNum)
            : base(slotInfos, children, BoolExpression.True, identifiers, blockAliasNum)
        {
        }

        internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
        {
            Debug.Assert(Children.Count > 0, "UnionCqlBlock: Children collection must not be empty");

            // Simply get the Cql versions of the children and add the union operator between them.
            var isFirst = true;
            foreach (var child in Children)
            {
                if (false == isFirst)
                {
                    StringUtil.IndentNewLine(builder, indentLevel + 1);
                    builder.Append(OpCellTreeNode.OpToEsql(CellTreeOpType.Union));
                }
                isFirst = false;

                builder.Append(" (");
                child.AsEsql(builder, isTopLevel, indentLevel + 1);
                builder.Append(')');
            }
            return builder;
        }

        internal override DbExpression AsCqt(bool isTopLevel)
        {
            Debug.Assert(Children.Count > 0, "UnionCqlBlock: Children collection must not be empty");
            var cqt = Children[0].AsCqt(isTopLevel);
            for (var i = 1; i < Children.Count; ++i)
            {
                cqt = cqt.UnionAll(Children[i].AsCqt(isTopLevel));
            }
            return cqt;
        }
    }
}
