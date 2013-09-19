// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// A DNF clause is of the form:
    /// Literal1 . Literal2 . ...
    /// Each literal is of the form:
    /// Term
    /// or
    /// !Term
    /// </summary>
    /// <typeparam name="T_Identifier"> Type of normal form literal. </typeparam>
    internal sealed class DnfClause<T_Identifier> : Clause<T_Identifier>,
                                                    IEquatable<DnfClause<T_Identifier>>
    {
        /// <summary>
        /// Initialize a DNF clause.
        /// </summary>
        /// <param name="literals"> Literals in clause. </param>
        internal DnfClause(Set<Literal<T_Identifier>> literals)
            : base(literals, ExprType.And)
        {
        }

        public bool Equals(DnfClause<T_Identifier> other)
        {
            return null != other &&
                   other.Literals.SetEquals(Literals);
        }
    }
}
