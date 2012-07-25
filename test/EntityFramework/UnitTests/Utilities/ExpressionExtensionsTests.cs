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
        public void GetPropertyAccessList_should_return_property_info_when_valid_property_access_expression()
        {
            Expression<Func<DateTime, int>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccessList().Single().Single();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccessList_should_return_property_path_in_correct_order()
        {
            Expression<Func<DateTime, int>> expression = d => d.Date.Day;

            var propertyPath = expression.GetPropertyAccessList().Single();

            Assert.NotNull(propertyPath);
            Assert.Equal(2, propertyPath.Count());
            Assert.Equal("Date", propertyPath.First().Name);
            Assert.Equal("Day", propertyPath.Last().Name);
        }

        [Fact]
        public void GetPropertyAccessList_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => 123;

            Assert.Equal(new InvalidOperationException(Strings.InvalidPropertiesExpression(expression)).Message,
                Assert.Throws<InvalidOperationException>(() => expression.GetPropertyAccessList()).Message);
        }

        [Fact]
        public void GetPropertyAccessList_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Equal(new InvalidOperationException(Strings.InvalidPropertiesExpression(expression)).Message,
                Assert.Throws<InvalidOperationException>(() => expression.GetPropertyAccessList()).Message);
        }

        [Fact]
        public void GetPropertyAccessList_should_remove_convert()
        {
            Expression<Func<DateTime, long>> expression = d => d.Hour;

            var propertyInfo = expression.GetPropertyAccessList().Single().Single();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Hour", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyAccessListList_should_return_property_path_collection()
        {
            Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

            var propertyInfos = expression.GetPropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count());
            Assert.Equal("Date", propertyInfos.First().Single().Name);
            Assert.Equal("Day", propertyInfos.Last().Single().Name);
        }

        [Fact]
        public void GetPropertyAccessListList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new { P = d.AddTicks(23) };

            Assert.Equal(new InvalidOperationException(Strings.InvalidPropertiesExpression(expression)).Message,
                Assert.Throws<InvalidOperationException>(() => expression.GetPropertyAccessList()).Message);
        }

        [Fact]
        public void GetPropertyAccessListList_should_throw_when_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new { d.Date, closure.Day };

            Assert.Equal(new InvalidOperationException(Strings.InvalidPropertiesExpression(expression)).Message,
                Assert.Throws<InvalidOperationException>(() => expression.GetPropertyAccessList()).Message);
        }

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

            Assert.Equal(Strings.InvalidPropertyExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccess()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_throw_when_not_property_access()
        {
            Expression<Func<DateTime, int>> expression = d => "".Length;

            Assert.Equal(Strings.InvalidComplexPropertyExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccess()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccess_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Hour;

            Assert.Equal(Strings.InvalidPropertyExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccess()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccess_should_throw_when_not_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;
            Expression<Func<DateTime, int>> expression = d => closure.Date.Hour;

            Assert.Equal(Strings.InvalidComplexPropertyExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccess()).Message);
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
            Expression<Func<DateTime, object>> expression = d => new { d.Date, d.Day };

            var propertyInfos = expression.GetSimplePropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count());
            Assert.Equal("Date", propertyInfos.First().Single().Name);
            Assert.Equal("Day", propertyInfos.Last().Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_return_property_path_collection()
        {
            Expression<Func<DateTime, object>> expression = d => new { d.Date.Month, d.Day };

            var propertyInfos = expression.GetComplexPropertyAccessList();

            Assert.NotNull(propertyInfos);
            Assert.Equal(2, propertyInfos.Count());
            Assert.Equal("Month", propertyInfos.First().Last().Name);
            Assert.Equal("Day", propertyInfos.Last().Single().Name);
        }

        [Fact]
        public void GetComplexPropertyAccessList_throws_when_member_name_specified_for_complex_member()
        {
            Expression<Func<DateTime, object>> expression = d => new { month = d.Date.Month, d.Day };

            Assert.Equal(Strings.InvalidComplexPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_throws_when_member_name_specified_for_simple_member()
        {
            Expression<Func<DateTime, object>> expression = d => new { d.Date.Month, _d = d.Day };

            Assert.Equal(Strings.InvalidComplexPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccessList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new { P = d.AddTicks(23) };

            Assert.Equal(Strings.InvalidPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_throw_when_invalid_expression()
        {
            Expression<Func<DateTime, object>> expression = d => new { P = d.Date.AddTicks(23) };

            Assert.Equal(Strings.InvalidComplexPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }

        [Fact]
        public void GetSimplePropertyAccessList_should_throw_when_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new { d.Date, closure.Day };

            Assert.Equal(Strings.InvalidPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetSimplePropertyAccessList()).Message);
        }

        [Fact]
        public void GetComplexPropertyAccessList_should_throw_when_property_access_on_the_provided_argument()
        {
            var closure = DateTime.Now;

            Expression<Func<DateTime, object>> expression = d => new { d.Date, closure.Date.Day };

            Assert.Equal(Strings.InvalidComplexPropertiesExpression(expression), Assert.Throws<InvalidOperationException>(() => expression.GetComplexPropertyAccessList()).Message);
        }
    }
}