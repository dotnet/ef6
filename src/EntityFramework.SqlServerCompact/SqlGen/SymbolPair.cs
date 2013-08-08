// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics;

    /// <summary>
    /// The SymbolPair exists to solve the record flattening problem.
    /// <see cref="SqlGenerator.Visit(DbPropertyExpression)" />
    /// Consider a property expression D(v, "j3.j2.j1.a.x")
    /// where v is a VarRef, j1, j2, j3 are joins, a is an extent and x is a columns.
    /// This has to be translated eventually into {j'}.{x'}
    /// The source field represents the outermost SqlStatement representing a join
    /// expression (say j2) - this is always a Join symbol.
    /// The column field keeps moving from one join symbol to the next, until it
    /// stops at a non-join symbol.
    /// This is returned by <see cref="SqlGenerator.Visit(DbPropertyExpression)" />,
    /// but never makes it into a SqlBuilder.
    /// </summary>
    internal class SymbolPair : ISqlFragment
    {
        public Symbol Source;
        public Symbol Column;

        public SymbolPair(Symbol source, Symbol column)
        {
            Source = source;
            Column = column;
        }

        #region ISqlFragment Members

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            // Symbol pair should never be part of a SqlBuilder.
            Debug.Assert(false);
        }

        #endregion
    }
}
