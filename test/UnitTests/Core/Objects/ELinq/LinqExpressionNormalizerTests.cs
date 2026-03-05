// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using Xunit;
#if NET5_0_OR_GREATER
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#endif

    public class LinqExpressionNormalizerTests
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(LinqExpressionNormalizer.RelationalOperatorPlaceholderMethod);
        }

#if NET5_0_OR_GREATER

        [Fact]
        public void MemoryExtensions_Contains_with_ReadOnlySpan_is_rewritten_to_Enumerable_Contains()
        {
            // Test: MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T)
            var array = new[] { "Title1", "Title2", "Title3" };
            var testValue = "Title1";

            var arrayExpr = Expression.Constant(array);
            var testValueExpr = Expression.Constant(testValue);

            // Get ReadOnlySpan<T>.op_Implicit method
            var spanType = typeof(ReadOnlySpan<>).MakeGenericType(typeof(string));
            var implicitMethod = spanType.GetMethod("op_Implicit", new[] { typeof(string[]) });
            var spanExpr = Expression.Call(implicitMethod, arrayExpr);

            // Get MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T)
            // Note: There are both Span<T> and ReadOnlySpan<T> versions, we want ReadOnlySpan
            var memExtContainsGeneric = typeof(MemoryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(MemoryExtensions.Contains)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                .Single();
            var memExtContains = memExtContainsGeneric.MakeGenericMethod(typeof(string));

            var methodCall = Expression.Call(memExtContains, spanExpr, testValueExpr);

            // Normalize the expression
            var normalizer = new LinqExpressionNormalizer();
            var normalized = normalizer.Visit(methodCall);

            // Verify it was rewritten to Enumerable.Contains
            var normalizedCall = Assert.IsAssignableFrom<MethodCallExpression>(normalized);
            Assert.Equal(nameof(Enumerable.Contains), normalizedCall.Method.Name);
            Assert.Equal(typeof(Enumerable), normalizedCall.Method.DeclaringType);
            Assert.Equal(2, normalizedCall.Arguments.Count);

            // The first argument should be the unwrapped array
            Assert.IsAssignableFrom<ConstantExpression>(normalizedCall.Arguments[0]);
            var unwrappedArray = ((ConstantExpression)normalizedCall.Arguments[0]).Value;
            Assert.Same(array, unwrappedArray);
        }

        [Fact]
        public void MemoryExtensions_Contains_with_Span_is_rewritten_to_Enumerable_Contains()
        {
            // Test: MemoryExtensions.Contains<T>(Span<T>, T)
            var array = new[] { "Title1", "Title2", "Title3" };
            var testValue = "Title1";

            var arrayExpr = Expression.Constant(array);
            var testValueExpr = Expression.Constant(testValue);

            // Get Span<T>.op_Implicit method
            var spanType = typeof(Span<>).MakeGenericType(typeof(string));
            var implicitMethod = spanType.GetMethod("op_Implicit", new[] { typeof(string[]) });
            var spanExpr = Expression.Call(implicitMethod, arrayExpr);

            // Get MemoryExtensions.Contains<T>(Span<T>, T)
            var memExtContainsGeneric = typeof(MemoryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(MemoryExtensions.Contains)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Span<>))
                .Single();
            var memExtContains = memExtContainsGeneric.MakeGenericMethod(typeof(string));

            var methodCall = Expression.Call(memExtContains, spanExpr, testValueExpr);

            // Normalize the expression
            var normalizer = new LinqExpressionNormalizer();
            var normalized = normalizer.Visit(methodCall);

            // Verify it was rewritten to Enumerable.Contains
            var normalizedCall = Assert.IsAssignableFrom<MethodCallExpression>(normalized);
            Assert.Equal(nameof(Enumerable.Contains), normalizedCall.Method.Name);
            Assert.Equal(typeof(Enumerable), normalizedCall.Method.DeclaringType);
            Assert.Equal(2, normalizedCall.Arguments.Count);

            // The first argument should be the unwrapped array
            Assert.IsAssignableFrom<ConstantExpression>(normalizedCall.Arguments[0]);
            var unwrappedArray = ((ConstantExpression)normalizedCall.Arguments[0]).Value;
            Assert.Same(array, unwrappedArray);
        }

#if NET10_0_OR_GREATER
        [Fact]
        public void MemoryExtensions_Contains_with_null_comparer_is_rewritten()
        {
            // Test: MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T, IEqualityComparer<T>) with null comparer
            // Note: This 3-parameter overload was added in .NET 10
            // The fix rewrites this when the comparer parameter is null
            var array = new[] { "Title1", "Title2", "Title3" };
            var testValue = "Title1";

            var arrayExpr = Expression.Constant(array);
            var testValueExpr = Expression.Constant(testValue);
            var nullComparerExpr = Expression.Constant(null, typeof(System.Collections.Generic.IEqualityComparer<string>));

            // Get ReadOnlySpan<T>.op_Implicit method
            var spanType = typeof(ReadOnlySpan<>).MakeGenericType(typeof(string));
            var implicitMethod = spanType.GetMethod("op_Implicit", new[] { typeof(string[]) });
            var spanExpr = Expression.Call(implicitMethod, arrayExpr);

            // Get MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T, IEqualityComparer<T>)
            var memExtContainsGeneric = typeof(MemoryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(MemoryExtensions.Contains)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 3
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>)
                    && m.GetParameters()[2].ParameterType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEqualityComparer<>))
                .Single();
            var memExtContains = memExtContainsGeneric.MakeGenericMethod(typeof(string));

            var methodCall = Expression.Call(memExtContains, spanExpr, testValueExpr, nullComparerExpr);

            // Normalize the expression
            var normalizer = new LinqExpressionNormalizer();
            var normalized = normalizer.Visit(methodCall);

            // Verify it was rewritten to Enumerable.Contains
            var normalizedCall = Assert.IsAssignableFrom<MethodCallExpression>(normalized);
            Assert.Equal(nameof(Enumerable.Contains), normalizedCall.Method.Name);
            Assert.Equal(typeof(Enumerable), normalizedCall.Method.DeclaringType);
            Assert.Equal(2, normalizedCall.Arguments.Count);
        }
#endif

        [Fact]
        public void MemoryExtensions_SequenceEqual_is_rewritten_to_Enumerable_SequenceEqual()
        {
            // Test that SequenceEqual is also rewritten correctly

            var array1 = new[] { 1, 2, 3 };
            var array2 = new[] { 1, 2, 3 };

            var array1Expr = Expression.Constant(array1);
            var array2Expr = Expression.Constant(array2);

            // Get the Span<T> op_Implicit method
            var spanType = typeof(ReadOnlySpan<>).MakeGenericType(typeof(int));
            var implicitMethod = spanType.GetMethod("op_Implicit", new[] { typeof(int[]) });
            var span1Expr = Expression.Call(implicitMethod, array1Expr);
            var span2Expr = Expression.Call(implicitMethod, array2Expr);

            // Get MemoryExtensions.SequenceEqual<T>(ReadOnlySpan<T>, ReadOnlySpan<T>) - the generic method with 2 parameters
            // Note: There are both Span<T> and ReadOnlySpan<T> versions, we want ReadOnlySpan
            var memExtSequenceEqualGeneric = typeof(MemoryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(MemoryExtensions.SequenceEqual)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                .Single();
            var memExtSequenceEqual = memExtSequenceEqualGeneric.MakeGenericMethod(typeof(int));

            var methodCall = Expression.Call(memExtSequenceEqual, span1Expr, span2Expr);

            // Normalize the expression
            var normalizer = new LinqExpressionNormalizer();
            var normalized = normalizer.Visit(methodCall);

            // Verify it was rewritten to Enumerable.SequenceEqual
            var normalizedCall = Assert.IsAssignableFrom<MethodCallExpression>(normalized);
            Assert.Equal(nameof(Enumerable.SequenceEqual), normalizedCall.Method.Name);
            Assert.Equal(typeof(Enumerable), normalizedCall.Method.DeclaringType);
            Assert.Equal(2, normalizedCall.Arguments.Count);
        }

#endif
    }
}
