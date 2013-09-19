// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class ExpressionConverterTests
    {
        [Fact]
        public void DescribeClrType_returns_full_name_for_normal_types()
        {
            Assert.Equal("System.Data.Entity.Core.Objects.ELinq.ExpressionConverterTests", ExpressionConverter.DescribeClrType(GetType()));

            Assert.Equal(
                "System.Data.Entity.Core.Objects.ELinq.ExpressionConverterTests+ANest", 
                ExpressionConverter.DescribeClrType(typeof(ANest)));
        }

        private class ANest
        {
        }

        [Fact]
        public void DefaultTranslator_suggests_different_VB_method_signature_when_unsupported_method_is_use()
        {
            var unsupportedMid = typeof(Microsoft.VisualBasic.Strings).GetDeclaredMethod("Mid", new[] { typeof(string), typeof(int) });
            var transalter = new ExpressionConverter.MethodCallTranslator.DefaultTranslator();
            var callExpression = Expression.Call(unsupportedMid, Expression.Constant("A"), Expression.Constant(1));

            Assert.Equal(
                Strings.ELinq_UnsupportedMethodSuggestedAlternative(
                    "System.String Mid(System.String, Int32)", "System.String Mid(System.String, Int32, Int32)"),
                Assert.Throws<NotSupportedException>(() => transalter.Translate(null, callExpression)).Message);
        }

        [Fact]
        public void DefaultTranslator_generates_normal_error_message_with_non_special_methods()
        {
            var supportedMid = typeof(Microsoft.VisualBasic.Strings).GetDeclaredMethod("Mid", new[] { typeof(string), typeof(int), typeof(int) });
            var transalter = new ExpressionConverter.MethodCallTranslator.DefaultTranslator();
            var callExpression = Expression.Call(supportedMid, Expression.Constant("A"), Expression.Constant(1), Expression.Constant(1));

            Assert.Equal(
                Strings.ELinq_UnsupportedMethod("System.String Mid(System.String, Int32, Int32)"),
                Assert.Throws<NotSupportedException>(() => transalter.Translate(null, callExpression)).Message);
        }

        [Fact]
        public void CanonicalFunctionDefaultTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.CanonicalFunctionDefaultTranslator().Methods;

            Assert.Equal(23, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void AsUnicodeFunctionTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.AsUnicodeFunctionTranslator().Methods;

            Assert.Equal(2, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void AsNonUnicodeFunctionTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.AsNonUnicodeFunctionTranslator().Methods;

            Assert.Equal(2, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void MathTruncateTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.MathTruncateTranslator().Methods;

            Assert.Equal(2, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void MathPowerTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.MathPowerTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void GuidNewGuidTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.GuidNewGuidTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void StringContainsTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.StringContainsTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void IndexOfTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.IndexOfTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void StartsWithTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.StartsWithTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void EndsWithTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.EndsWithTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void SubstringTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.SubstringTranslator().Methods;

            Assert.Equal(2, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void RemoveTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.RemoveTranslator().Methods;

            Assert.Equal(2, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void InsertTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.InsertTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void IsNullOrEmptyTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.IsNullOrEmptyTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void StringConcatTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.StringConcatTranslator().Methods;

            Assert.Equal(3, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void TrimTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.TrimTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void TrimStartTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.TrimStartTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void TrimEndTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.TrimEndTranslator().Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void VBCanonicalFunctionDefaultTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.VBCanonicalFunctionDefaultTranslator(
                typeof(Microsoft.VisualBasic.Strings).Assembly).Methods;

            Assert.Equal(11, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void VBCanonicalFunctionRenameTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.VBCanonicalFunctionRenameTranslator(
                typeof(Microsoft.VisualBasic.Strings).Assembly).Methods;

            Assert.Equal(4, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void VBDatePartTranslator_finds_all_expected_methods()
        {
            var methods = new ExpressionConverter.MethodCallTranslator.VBDatePartTranslator(
                typeof(Microsoft.VisualBasic.Strings).Assembly).Methods;

            Assert.Equal(1, methods.Count());
            Assert.True(methods.All(m => m != null));
        }

        [Fact]
        public void DefaultCanonicalFunctionPropertyTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.DefaultCanonicalFunctionPropertyTranslator().Properties;

            Assert.Equal(15, properties.Count());
            Assert.True(properties.All(p => p != null));
        }

        [Fact]
        public void RenameCanonicalFunctionPropertyTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.RenameCanonicalFunctionPropertyTranslator().Properties;

            Assert.Equal(7, properties.Count());
            Assert.True(properties.All(p => p != null));
        }

        [Fact]
        public void VBDateAndTimeNowTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.VBDateAndTimeNowTranslator(
                typeof(Microsoft.VisualBasic.DateAndTime).Assembly).Properties;

            Assert.Equal(1, properties.Count());
            Assert.True(properties.All(p => p != null));
        }

        [Fact]
        public void EntityCollectionCountTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.EntityCollectionCountTranslator().Properties;

            Assert.Equal(1, properties.Count());
            Assert.True(properties.All(p => p != null));
        }

        [Fact]
        public void NullableHasValueTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.NullableHasValueTranslator().Properties;

            Assert.Equal(1, properties.Count());
            Assert.True(properties.All(p => p != null));
        }

        [Fact]
        public void NullableValueTranslator_finds_all_expected_properties()
        {
            var properties = new ExpressionConverter.MemberAccessTranslator.NullableValueTranslator().Properties;

            Assert.Equal(1, properties.Count());
            Assert.True(properties.All(p => p != null));
        }
    }
}
