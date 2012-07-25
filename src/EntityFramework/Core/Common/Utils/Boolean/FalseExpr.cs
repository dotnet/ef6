// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// Boolean expression that evaluates to false.
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal sealed class FalseExpr<T_Identifier> : BoolExpr<T_Identifier>
    {
        private static readonly FalseExpr<T_Identifier> _value = new FalseExpr<T_Identifier>();

        // private constructor so that we control existence of False instance
        private FalseExpr()
        {
        }

        /// <summary>
        /// Gets the one instance of FalseExpr
        /// </summary>
        internal static FalseExpr<T_Identifier> Value
        {
            get { return _value; }
        }

        internal override ExprType ExprType
        {
            get { return ExprType.False; }
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitFalse(this);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            return TrueExpr<T_Identifier>.Value;
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return ReferenceEquals(this, other);
        }
    }
}
