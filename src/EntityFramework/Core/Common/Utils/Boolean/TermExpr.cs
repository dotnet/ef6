// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    // <summary>
    // A term is a leaf node in a Boolean expression. Its value (T/F) is undefined.
    // </summary>
    // <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal sealed class TermExpr<T_Identifier> : BoolExpr<T_Identifier>, IEquatable<TermExpr<T_Identifier>>
    {
        private readonly T_Identifier _identifier;
        private readonly IEqualityComparer<T_Identifier> _comparer;

        // <summary>
        // Construct a term.
        // </summary>
        // <param name="comparer"> Value comparer to use when comparing two term expressions. </param>
        // <param name="identifier"> Identifier/tag for this term. </param>
        internal TermExpr(IEqualityComparer<T_Identifier> comparer, T_Identifier identifier)
        {
            DebugCheck.NotNull((object)identifier);
            _identifier = identifier;
            if (null == comparer)
            {
                _comparer = EqualityComparer<T_Identifier>.Default;
            }
            else
            {
                _comparer = comparer;
            }
        }

        internal TermExpr(T_Identifier identifier)
            : this(null, identifier)
        {
        }

        // <summary>
        // Gets identifier for this term. This value is used to determine whether
        // two terms as equivalent.
        // </summary>
        internal T_Identifier Identifier
        {
            get { return _identifier; }
        }

        internal override ExprType ExprType
        {
            get { return ExprType.Term; }
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("use only typed equals");
            return Equals(obj as TermExpr<T_Identifier>);
        }

        public bool Equals(TermExpr<T_Identifier> other)
        {
            return _comparer.Equals(_identifier, other._identifier);
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return _comparer.Equals(_identifier, ((TermExpr<T_Identifier>)other)._identifier);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(_identifier);
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}", _identifier);
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitTerm(this);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            var literal = new Literal<T_Identifier>(this, true);
            // leverage normalization code if it exists
            var negatedLiteral = literal.MakeNegated();
            if (negatedLiteral.IsTermPositive)
            {
                return negatedLiteral.Term;
            }
            else
            {
                return new NotExpr<T_Identifier>(negatedLiteral.Term);
            }
        }
    }
}
