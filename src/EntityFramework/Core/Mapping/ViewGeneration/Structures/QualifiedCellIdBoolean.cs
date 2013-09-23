// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
    using System.Diagnostics;
    using System.Text;

    // <summary>
    // A class that denotes "block_alias.booleanVar", e.g., "T1._from2".
    // It is a subclass of <see cref="CellIdBoolean" /> with an added block alias.
    // </summary>
    internal sealed class QualifiedCellIdBoolean : CellIdBoolean
    {
        // <summary>
        // Creates a boolean of the form "<paramref name="block" />.<paramref name="originalCellNum" />".
        // </summary>
        internal QualifiedCellIdBoolean(CqlBlock block, CqlIdentifiers identifiers, int originalCellNum)
            : base(identifiers, originalCellNum)
        {
            m_block = block;
        }

        private readonly CqlBlock m_block;

        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // QualifiedCellIdBoolean is only used during JOIN processing where there is no single input, hence blockAlias is expected to be null.
            Debug.Assert(blockAlias == null, "QualifiedCellIdBoolean: blockAlias mismatch");
            return base.AsEsql(builder, m_block.CqlAlias, skipIsNotNull);
        }

        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            return base.AsCqt(m_block.GetInput(row), skipIsNotNull);
        }
    }
}
