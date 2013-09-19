// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// Represents a sentence in conjunctive normal form, e.g.:
    /// Clause1 . Clause2 . ...
    /// Where each DNF clause is of the form:
    /// Literal1 + Literal2 + ...
    /// Each literal is of the form:
    /// Term
    /// or
    /// !Term
    /// </summary>
    /// <typeparam name="T_Identifier"> Type of expression leaf term identifiers. </typeparam>
    internal sealed class CnfSentence<T_Identifier> : Sentence<T_Identifier, CnfClause<T_Identifier>>
    {
        // Initializes a new CNF sentence given its clauses.
        internal CnfSentence(Set<CnfClause<T_Identifier>> clauses)
            : base(clauses, ExprType.And)
        {
        }
    }
}
