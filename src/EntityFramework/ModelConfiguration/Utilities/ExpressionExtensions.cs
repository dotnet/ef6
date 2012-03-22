namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        public static PropertyPath GetSimplePropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);
            Contract.Assert(propertyAccessExpression.Parameters.Count() == 1);

            var propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyPath == null)
            {
                throw Error.InvalidPropertyExpression(propertyAccessExpression);
            }

            return propertyPath;
        }

        public static PropertyPath GetComplexPropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);
            Contract.Assert(propertyAccessExpression.Parameters.Count() == 1);

            var propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchComplexPropertyAccess(propertyAccessExpression.Body);

            if (propertyPath == null)
            {
                throw Error.InvalidComplexPropertyExpression(propertyAccessExpression);
            }

            return propertyPath;
        }

        public static IEnumerable<PropertyPath> GetSimplePropertyAccessList(
            this LambdaExpression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);
            Contract.Assert(propertyAccessExpression.Parameters.Count() == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw Error.InvalidPropertiesExpression(propertyAccessExpression);
            }

            return propertyPaths;
        }

        public static IEnumerable<PropertyPath> GetComplexPropertyAccessList(
            this LambdaExpression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);
            Contract.Assert(propertyAccessExpression.Parameters.Count() == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchComplexPropertyAccess(p));

            if (propertyPaths == null)
            {
                throw Error.InvalidComplexPropertiesExpression(propertyAccessExpression);
            }

            return propertyPaths;
        }

        private static IEnumerable<PropertyPath> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyPath> propertyMatcher)
        {
            Contract.Requires(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body != null);

            var newExpression
                = lambdaExpression.Body.RemoveConvert() as NewExpression;

            if (newExpression != null)
            {
                var parameterExpression
                    = lambdaExpression.Parameters.Single();

                var propertyPaths
                    = newExpression.Arguments
                        .Select(a => propertyMatcher(a, parameterExpression))
                        .Where(p => p != null);

                if (propertyPaths.Count()
                    == newExpression.Arguments.Count())
                {
                    return newExpression.HasDefaultMembersOnly(propertyPaths) ? propertyPaths : null;
                }
            }

            var propertyPath = propertyMatcher(lambdaExpression.Body, lambdaExpression.Parameters.Single());

            return (propertyPath != null) ? propertyPath.AsEnumerable() : null;
        }

        private static bool HasDefaultMembersOnly(
            this NewExpression newExpression, IEnumerable<PropertyPath> propertyPaths)
        {
            Contract.Requires(newExpression != null);
            Contract.Requires(propertyPaths != null);

            return !newExpression.Members
                        .Where(
                            (t, i) =>
                            !string.Equals(t.Name, propertyPaths.ElementAt(i).Last().Name, StringComparison.Ordinal))
                        .Any();
        }

        private static PropertyPath MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);

            var propertyPath = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyPath.Count() == 1 ? propertyPath : null;
        }

        private static PropertyPath MatchComplexPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            Contract.Requires(propertyAccessExpression != null);

            var propertyPath = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyPath.Any() ? propertyPath : null;
        }

        private static PropertyPath MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            Contract.Requires(parameterExpression != null);
            Contract.Requires(propertyAccessExpression != null);

            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = propertyAccessExpression.RemoveConvert() as MemberExpression;

                if (memberExpression == null)
                {
                    return PropertyPath.Empty;
                }

                var propertyInfo = memberExpression.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return PropertyPath.Empty;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (memberExpression.Expression != parameterExpression);

            return new PropertyPath(propertyInfos);
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
