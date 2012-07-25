// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Linq.Expressions;

    /// <summary>
    /// For collection results, we really want to know the expression to
    /// get the coordinator from its stateslot as well, so we have an 
    /// additional one...
    /// </summary>
    internal class CollectionTranslatorResult : TranslatorResult
    {
        internal readonly Expression ExpressionToGetCoordinator;

        internal CollectionTranslatorResult(Expression returnedExpression, Type requestedType, Expression expressionToGetCoordinator)
            : base(returnedExpression, requestedType)
        {
            ExpressionToGetCoordinator = expressionToGetCoordinator;
        }
    }
}
