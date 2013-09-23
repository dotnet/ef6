// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;

    // <summary>
    // Manages state used to translate BoolExpr to decision diagram vertices and back again.
    // Specializations exist for generic and DomainConstraint expressions.
    // </summary>
    internal abstract class ConversionContext<T_Identifier>
    {
        // <summary>
        // Gets the solver instance associated with this conversion context. Used to reterieve
        // canonical Decision Diagram vertices for this context.
        // </summary>
        internal readonly Solver Solver = new Solver();

        // <summary>
        // Given a term in BoolExpr, returns the corresponding decision diagram vertex.
        // </summary>
        internal abstract Vertex TranslateTermToVertex(TermExpr<T_Identifier> term);

        // <summary>
        // Describes a vertex as a series of literal->vertex successors such that the literal
        // logically implies the given vertex successor.
        // </summary>
        internal abstract IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex);
    }
}
