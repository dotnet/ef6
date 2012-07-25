// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// A CNF clause is of the form:
    /// 
    ///     Literal1 + Literal2 . ...
    /// 
    /// Each literal is of the form:
    /// 
    ///     Term
    /// 
    /// or
    /// 
    ///     !Term
    /// </summary>
    /// <typeparam name="T_Identifier">Type of normal form literal.</typeparam>
    internal sealed class CnfClause<T_Identifier> : Clause<T_Identifier>,
                                                    IEquatable<CnfClause<T_Identifier>>
    {
        /// <summary>
        /// Initialize a CNF clause.
        /// </summary>
        /// <param name="literals">Literals in clause.</param>
        internal CnfClause(Set<Literal<T_Identifier>> literals)
            : base(literals, ExprType.Or)
        {
        }

        public bool Equals(CnfClause<T_Identifier> other)
        {
            return null != other &&
                   other.Literals.SetEquals(Literals);
        }
    }
}
