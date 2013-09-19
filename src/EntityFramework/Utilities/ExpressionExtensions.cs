// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        public static PropertyPath GetSimplePropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

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
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

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

        public static IEnumerable<PropertyPath> GetSimplePropertyAccessList(this LambdaExpression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw Error.InvalidPropertiesExpression(propertyAccessExpression);
            }

            return propertyPaths;
        }

        public static IEnumerable<PropertyPath> GetComplexPropertyAccessList(this LambdaExpression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

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
            DebugCheck.NotNull(lambdaExpression);
            DebugCheck.NotNull(propertyMatcher);
            Debug.Assert(lambdaExpression.Body != null);

            var newExpression
                = RemoveConvert(lambdaExpression.Body) as NewExpression;

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

            return (propertyPath != null) ? new[] { propertyPath } : null;
        }

        private static bool HasDefaultMembersOnly(
            this NewExpression newExpression, IEnumerable<PropertyPath> propertyPaths)
        {
            DebugCheck.NotNull(newExpression);
            DebugCheck.NotNull(propertyPaths);

            return !newExpression.Members
                                 .Where(
                                     (t, i) =>
                                     !string.Equals(t.Name, propertyPaths.ElementAt(i).Last().Name, StringComparison.Ordinal))
                                 .Any();
        }

        private static PropertyPath MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);

            var propertyPath = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyPath != null && propertyPath.Count == 1 ? propertyPath : null;
        }

        private static PropertyPath MatchComplexPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);

            var propertyPath = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyPath;
        }

        private static PropertyPath MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            DebugCheck.NotNull(parameterExpression);
            DebugCheck.NotNull(propertyAccessExpression);

            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

                if (memberExpression == null)
                {
                    return null;
                }

                var propertyInfo = memberExpression.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (memberExpression.Expression != parameterExpression);

            return new PropertyPath(propertyInfos);
        }

        public static Expression RemoveConvert(this Expression expression)
        {
            DebugCheck.NotNull(expression);

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
