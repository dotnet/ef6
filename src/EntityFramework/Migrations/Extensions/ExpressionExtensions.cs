namespace System.Data.Entity.Migrations.Extensions
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        public static IEnumerable<PropertyInfo> GetPropertyAccessList(this LambdaExpression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);

            var propertyInfos
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => MatchPropertyAccess(e, p));

            if (propertyInfos == null)
            {
                throw Error.InvalidPropertiesExpression(propertyAccessExpression);
            }

            return propertyInfos;
        }

        private static IEnumerable<PropertyInfo> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Contract.Requires(lambdaExpression != null);
            Contract.Requires(propertyMatcher != null);

            var newExpression
                = lambdaExpression.Body.RemoveConvert() as NewExpression;

            if (newExpression != null)
            {
                var parameterExpression
                    = lambdaExpression.Parameters.Single();

                var propertyInfos
                    = newExpression.Arguments
                        .Select(a => propertyMatcher(a, parameterExpression))
                        .Where(p => p != null);

                if (propertyInfos.Count() == newExpression.Arguments.Count())
                {
                    return newExpression.HasDefaultMembersOnly(propertyInfos) ? propertyInfos : null;
                }
            }

            var propertyInfo = propertyMatcher(lambdaExpression.Body, lambdaExpression.Parameters.Single());

            return (propertyInfo != null) ? new[] { propertyInfo } : null;
        }

        private static bool HasDefaultMembersOnly(this NewExpression newExpression, IEnumerable<PropertyInfo> propertyInfos)
        {
            Contract.Requires(newExpression != null);
            Contract.Requires(propertyInfos != null);

            return !newExpression.Members
                        .Where((t, i) => !string.Equals(t.Name, propertyInfos.ElementAt(i).Name, StringComparison.Ordinal))
                        .Any();
        }

        private static PropertyInfo MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            Contract.Requires(parameterExpression != null);
            Contract.Requires(propertyAccessExpression != null);

            var memberExpression = propertyAccessExpression.RemoveConvert() as MemberExpression;

            if (memberExpression == null)
            {
                return null;
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                return null;
            }

            if (memberExpression.Expression != parameterExpression)
            {
                return null;
            }

            return propertyInfo;
        }

        public static Expression RemoveConvert(this Expression expression)
        {
            Contract.Requires(expression != null);

            while ((expression != null)
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }
}