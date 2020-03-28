// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class ExpressionExtensionsTests
    {
        [Fact]
        public void GetSimplePropertyAccess_should_return_property_info_when_valid_property_access_expression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Hour;

            var propertyPath = expression.GetSimplePropertyAccess();

            Assert.NotNull(propertyPath);
            Assert.Equal("Hour", propertyPath.Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_return_property_info_when_valid_dotted_property_access_expression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Date.TimeOfDay.Days;

            var propertyPath = expression.GetComplexPropertyAccess();

            Assert.NotNull(propertyPath);
            Assert.Equal("Date", propertyPath.First().Name);
            Assert.Equal("TimeOfDay", propertyPath.ElementAt(1).Name);
            Assert.Equal("Days", propertyPath.Last().Name);
        }

        [Fact]
        public void GetSimplePropertyAccess_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => 123;

            Assert.Equal(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccess()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => "".Length;

            Assert.Equal(
                Strings.InvalidComplexPropertyExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccess()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccess_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Equal(
                Strings.InvalidPropertyExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccess()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Date.Hour;

            Assert.Equal(
                Strings.InvalidComplexPropertyExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccess()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccess_should_remove_convert()
        {
            Expression<Func<DateTime, long>> expression = d => d.Hour;

            var propertyPath = expression.GetSimplePropertyAccess();

            Assert.NotNull(propertyPath);
            Assert.Equal("Hour", propertyPath.Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_remove_convert()
        {
            Expression<Func<DateTime, int>> expression = d => d.Date.TimeOfDay.Days;

            var propertyPath = expression.GetComplexPropertyAccess();

            Assert.NotNull(propertyPath);
            Assert.Equal("Date", propertyPath.First().Name);
        }

        [Fact]
        public void GetSimplePropertyAccessList_should_return_property_path_collection()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         d.Date,
                                                                         d.Day
                                                                     };

            var propertyInfos = expression.GetSimplePropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count());
            Assert.Equal("Date", propertyInfos.First().Single().Name);
            Assert.Equal("Day", propertyInfos.Last().Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_return_property_path_collection()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         d.Date.Month,
                                                                         d.Day
                                                                     };

            var propertyInfos = expression.GetComplexPropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count());
            Assert.Equal("Month", propertyInfos.First().Last().Name);
            Assert.Equal("Day", propertyInfos.Last().Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccessList_throws_when_member_name_specified_for_complex_member()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         month = d.Date.Month,
                                                                         d.Day
                                                                     };

            Assert.Equal(
                Strings.InvalidComplexPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_throws_when_member_name_specified_for_simple_member()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         d.Date.Month,
                                                                         _d = d.Day
                                                                     };

            Assert.Equal(
                Strings.InvalidComplexPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccessList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         P = d.AddTicks(23)
                                                                     };

            Assert.Equal(
                Strings.InvalidPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         P = d.Date.AddTicks(23)
                                                                     };

            Assert.Equal(
                Strings.InvalidComplexPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccessList_should_throw_when_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         d.Date,
                                                                         closure.Day
                                                                     };

            Assert.Equal(
                Strings.InvalidPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_throw_when_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new
                                                                     {
                                                                         d.Date,
                                                                         closure.Date.Day
                                                                     };

            Assert.Equal(
                Strings.InvalidComplexPropertiesExpression(expression),
                Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void IsNullConstant_returns_true_for_null_ConstantExpression()
        {
            Assert.True(Expression.Constant(null).IsNullConstant());
        }

        [Fact]
        public void IsNullConstant_returns_true_for_cast_null_ConstantExpression()
        {
            Assert.True(Expression.Convert(Expression.Constant(null), typeof(string)).IsNullConstant());
            Assert.True(Expression.ConvertChecked(Expression.Constant(null), typeof(string)).IsNullConstant());
        }

        [Fact]
        public void IsNullConstant_returns_false_for_non_null_ConstantExpression()
        {
            Assert.False(Expression.Constant(5).IsNullConstant());
        }

        [Fact]
        public void IsNullConstant_returns_false_for_cast_non_null_ConstantExpression()
        {
            Assert.False(Expression.Convert(Expression.Constant(5), typeof(object)).IsNullConstant());
            Assert.False(Expression.ConvertChecked(Expression.Constant(5), typeof(object)).IsNullConstant());
        }

        [Fact]
        public void IsNullConstant_returns_false_for_non_ConstantExpression()
        {
            Assert.False(Expression.IsTrue(Expression.Constant(true)).IsNullConstant());
        }

        [Fact]
        public void IsNullConstant_returns_false_for_cast_non_ConstantExpression()
        {
            Assert.False(Expression.IsTrue(Expression.Convert(Expression.Constant(true), typeof(bool))).IsNullConstant());
            Assert.False(Expression.IsTrue(Expression.ConvertChecked(Expression.Constant(false), typeof(bool))).IsNullConstant());
        }

        [Fact]
        public void IsStringAddExpression_returns_true_for_string_Add_expression()
        {
            Assert.True(
                Expression.Add(
                    Expression.Convert(Expression.Constant(3), typeof(object)), 
                    Expression.Convert(Expression.Constant("b"), typeof(object)),
                    typeof(string).GetMethod("Concat", new[] {typeof(object), typeof(object)}))
                    .IsStringAddExpression());
        }

        [Fact]
        public void IsStringAddExpression_returns_false_if_expression_is_not_binary()
        {
            Assert.False(Expression.Constant(42).IsStringAddExpression());
        }

        [Fact]
        public void IsStringAddExpression_returns_false_regular_Add()
        {
            Assert.False(Expression.Add(Expression.Constant(42), Expression.Constant(-42)).IsStringAddExpression());
        }

        [Fact]
        public void IsStringAddExpression_returns_false_if_expression_is_binary_but_not_Add()
        {
            Assert.False(Expression.Subtract(Expression.Constant(42), Expression.Constant(-42)).IsStringAddExpression());
        }

        [Fact]
        public void IsStringAddExpression_returns_false_for_Add_expression_if_method_is_not_Concat()
        {
            Assert.False(
                Expression.Add(
                    Expression.Constant("abc"),
                    Expression.Constant("xyz"),
                    typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }))
                    .IsStringAddExpression());
        }
    }
}
