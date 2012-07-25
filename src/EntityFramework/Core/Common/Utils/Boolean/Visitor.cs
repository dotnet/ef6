// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// Abstract visitor class. All Boolean expression nodes know how to
    /// 'accept' a visitor, and delegate to the appropriate visitor method.
    /// For instance, AndExpr invokes Visitor.VisitAnd.
    /// </summary>
    /// <typeparam name="T_Identifier">Type of leaf term identifiers in expression.</typeparam>
    /// <typeparam name="T_Return">Return type for visit methods.</typeparam>
    internal abstract class Visitor<T_Identifier, T_Return>
    {
        internal abstract T_Return VisitTrue(TrueExpr<T_Identifier> expression);
        internal abstract T_Return VisitFalse(FalseExpr<T_Identifier> expression);
        internal abstract T_Return VisitTerm(TermExpr<T_Identifier> expression);
        internal abstract T_Return VisitNot(NotExpr<T_Identifier> expression);
        internal abstract T_Return VisitAnd(AndExpr<T_Identifier> expression);
        internal abstract T_Return VisitOr(OrExpr<T_Identifier> expression);
    }
}
