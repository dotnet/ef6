namespace System.Data.Entity.SqlServer
{
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class Expressions
    {
        internal static Expression Null<TNullType>()
        {
            return Expression.Constant(null, typeof(TNullType));
        }

        internal static Expression Null(Type nullType)
        {
            return Expression.Constant(null, nullType);
        }

        internal static Expression<Func<TArg, TResult>> Lambda<TArg, TResult>(
            string argumentName, Func<ParameterExpression, Expression> createLambdaBodyGivenParameter)
        {
            var argParam = Expression.Parameter(typeof(TArg), argumentName);
            var lambdaBody = createLambdaBodyGivenParameter(argParam);
            return Expression.Lambda<Func<TArg, TResult>>(lambdaBody, argParam);
        }

        internal static Expression Call(this Expression exp, string methodName)
        {
            return Expression.Call(exp, methodName, Type.EmptyTypes);
        }

        internal static Expression ConvertTo(this Expression exp, Type convertToType)
        {
            return Expression.Convert(exp, convertToType);
        }

        internal static Expression ConvertTo<TConvertToType>(this Expression exp)
        {
            return Expression.Convert(exp, typeof(TConvertToType));
        }

        internal sealed class ConditionalExpressionBuilder
        {
            private readonly Expression condition;
            private readonly Expression ifTrueThen;

            internal ConditionalExpressionBuilder(Expression conditionExpression, Expression ifTrueExpression)
            {
                condition = conditionExpression;
                ifTrueThen = ifTrueExpression;
            }

            internal Expression Else(Expression resultIfFalse)
            {
                return Expression.Condition(condition, ifTrueThen, resultIfFalse);
            }
        }

        internal static ConditionalExpressionBuilder IfTrueThen(this Expression conditionExp, Expression resultIfTrue)
        {
            return new ConditionalExpressionBuilder(conditionExp, resultIfTrue);
        }

        internal static Expression Property<TPropertyType>(this Expression exp, string propertyName)
        {
            var prop = exp.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(
                prop != null,
                "Type '" + exp.Type.FullName + "' does not declare a public instance property with the name '" + propertyName + "'");

            return Expression.Property(exp, prop);
        }
    }
}
