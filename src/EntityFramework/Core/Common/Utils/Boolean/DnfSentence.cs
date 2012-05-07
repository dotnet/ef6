namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// Represents a sentence in disjunctive normal form, e.g.:
    /// 
    ///     Clause1 + Clause2 . ...
    /// 
    /// Where each DNF clause is of the form:
    /// 
    ///     Literal1 . Literal2 . ...
    /// 
    /// Each literal is of the form:
    /// 
    ///     Term
    /// 
    /// or
    /// 
    ///     !Term    
    /// </summary>
    /// <typeparam name="T_Identifier">Type of expression leaf term identifiers.</typeparam>
    internal sealed class DnfSentence<T_Identifier> : Sentence<T_Identifier, DnfClause<T_Identifier>>
    {
        // Initializes a new DNF sentence given its clauses.
        internal DnfSentence(Set<DnfClause<T_Identifier>> clauses)
            : base(clauses, ExprType.Or)
        {
        }
    }
}