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
