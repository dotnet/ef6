// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using Moq;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public partial class DbExpressionBuilderTests : IUseFixture<DbExpressionBuilderFixture>
    {
        private MetadataWorkspace workspace;
        private TypeUsage stringTypeUsage;
        private TypeUsage booleanTypeUsage;
        private TypeUsage integerTypeUsage;
        private TypeUsage geographyTypeUsage;
        private TypeUsage geometryTypeUsage;
        private TypeUsage enumTypeUsage;
        private TypeUsage enumTypeUsage2;
        private TypeUsage productTypeUsage;
        private TypeUsage discontinuedProductTypeUsage;
        private TypeUsage categoryTypeUsage;
        private EntitySet productsEntitySet;
        private EntitySet categoriesEntitySet;

        #region Arithmetic

        [Fact]
        public void Plus_same_types()
        {
            TestArithmeticExpression(2, 40, (l, r) => DbExpressionBuilder.Plus(l, r), DbExpressionKind.Plus, "Edm.Int32");
        }

        [Fact]
        public void Minus_directly_promotable_types()
        {
            TestArithmeticExpression((short)2, 40, (l, r) => DbExpressionBuilder.Minus(l, r), DbExpressionKind.Minus, "Edm.Int32");
        }

        [Fact]
        public void Multiply_indirectly_promotable_types()
        {
            TestArithmeticExpression((long)2, (float)40f, (l, r) => DbExpressionBuilder.Multiply(l, r), DbExpressionKind.Multiply, "Edm.Single");
        }

        [Fact]
        public void Divide_basic_test()
        {
            TestArithmeticExpression(2, 40, (l, r) => DbExpressionBuilder.Divide(l, r), DbExpressionKind.Divide, "Edm.Int32");
        }

        [Fact]
        public void Modulo_basic_test()
        {
            TestArithmeticExpression(2, 40, (l, r) => DbExpressionBuilder.Modulo(l, r), DbExpressionKind.Modulo, "Edm.Int32");
        }

        [Fact]
        public void UnaryMinus()
        {
            var arg = DbExpressionBuilder.Constant((Int64)120205);
            var expression = DbExpressionBuilder.UnaryMinus(arg);

            Assert.NotNull(expression);
            Assert.IsType<DbArithmeticExpression>(expression);
            Assert.Equal(DbExpressionKind.UnaryMinus, expression.ExpressionKind);
            Assert.Same(arg, expression.Arguments[0]);
            Assert.Equal("Edm.Int64", expression.ResultType.EdmType.FullName);
        }

        private void TestArithmeticExpression(
            object leftValue, 
            object rightValue, 
            Func<DbExpression, DbExpression, DbArithmeticExpression> expressionCreation, 
            DbExpressionKind expressionKind,
            string expectedResultType)
        {
            var left = DbExpressionBuilder.Constant(leftValue);
            var right = DbExpressionBuilder.Constant(rightValue);
            var expression = expressionCreation(left, right);

            Assert.NotNull(expression);
            Assert.Same(left, expression.Arguments[0]);
            Assert.Same(right, expression.Arguments[1]);
            Assert.Equal(expectedResultType, expression.ResultType.EdmType.FullName);

            var message = Assert.Throws<ArgumentException>(() => expressionCreation(
                DbExpressionBuilder.Constant((Int32)5),
                DbExpressionBuilder.Constant((string)"27"))).Message;

            Assert.True(message.Contains(Strings.Cqt_Arithmetic_NumericCommonType));
        }

        #endregion

        #region Comparison

        [Fact]
        public void Equal_same_types()
        {
            TestComparisonExpression(12, 12, (l, r) => DbExpressionBuilder.Equal(l, r), DbExpressionKind.Equals);
        }

        [Fact]
        public void Equal_same_enum_types()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var expression = DbExpressionBuilder.Equal(left, right);

            Assert.NotNull(expression);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Equals, expression.ExpressionKind);
        }

        [Fact]
        public void Equal_different_enum_types_throws()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(this.enumTypeUsage2, CardSuite.Hearts);

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Equal(left, right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Comparison_ComparableRequired));
        }

        [Fact]
        public void Equal_enum_and_number_throws()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(1);

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Equal(left, right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Comparison_ComparableRequired));

            var message2 = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Equal(right, left)).Message;
            Assert.True(message2.Contains(Strings.Cqt_Comparison_ComparableRequired));
        }

        [Fact]
        public void NotEqual_promotable_types()
        {
            TestComparisonExpression(12, (float)12f, (l, r) => DbExpressionBuilder.NotEqual(l, r), DbExpressionKind.NotEquals);
        }

        [Fact]
        public void LessThan_basic()
        {
            TestComparisonExpression(12, 12, (l, r) => DbExpressionBuilder.LessThan(l, r), DbExpressionKind.LessThan);
        }

        [Fact]
        public void LessThan_same_enum_types()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var expression = DbExpressionBuilder.LessThan(left, right);

            Assert.NotNull(expression);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.LessThan, expression.ExpressionKind);
        }

        [Fact]
        public void LessThan_different_enum_types_throws()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(this.enumTypeUsage2, CardSuite.Hearts);

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.LessThan(left, right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Comparison_ComparableRequired));
        }

        [Fact]
        public void LessThan_enum_and_number_throws()
        {
            var left = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Clubs);
            var right = DbExpressionBuilder.Constant(1);

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.LessThan(left, right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Comparison_ComparableRequired));

            var message2 = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.LessThan(right, left)).Message;
            Assert.True(message2.Contains(Strings.Cqt_Comparison_ComparableRequired));
        }

        [Fact]
        public void LessThanOrEqual_basic()
        {
            TestComparisonExpression(12, 12, (l, r) => DbExpressionBuilder.LessThanOrEqual(l, r), DbExpressionKind.LessThanOrEquals);
        }

        [Fact]
        public void GreaterThan_basic()
        {
            TestComparisonExpression(12, 12, (l, r) => DbExpressionBuilder.GreaterThan(l, r), DbExpressionKind.GreaterThan);
        }

        [Fact]
        public void GreaterThanOrEqual_basic()
        {
            TestComparisonExpression(12, 12, (l, r) => DbExpressionBuilder.GreaterThanOrEqual(l, r), DbExpressionKind.GreaterThanOrEquals);
        }

        [Fact]
        public void Comparing_non_comparable_types_throws()
        {
            var left = DbExpressionBuilder.Constant(12);
            var right = DbExpressionBuilder.Constant("Bar");

            var expressionCreationMethods = new List<Func<DbExpression, DbExpression, DbComparisonExpression>>
            {
                (l, r) => DbExpressionBuilder.Equal(l, r),
                (l, r) => DbExpressionBuilder.NotEqual(l, r),
                (l, r) => DbExpressionBuilder.GreaterThan(l, r),
                (l, r) => DbExpressionBuilder.GreaterThanOrEqual(l, r),
                (l, r) => DbExpressionBuilder.LessThan(l, r),
                (l, r) => DbExpressionBuilder.LessThanOrEqual(l, r),
            };

            foreach (var expressionCreationMethod in expressionCreationMethods)
            {
                var message = Assert.Throws<ArgumentException>(() => expressionCreationMethod(left, right)).Message;
                Assert.True(message.Contains(Strings.Cqt_Comparison_ComparableRequired));
            }
        }

        private void TestComparisonExpression(
            object leftValue, 
            object rightValue, 
            Func<DbExpression, DbExpression, DbComparisonExpression> expressionCreation, 
            DbExpressionKind expressionKind)
        {
            var left = DbExpressionBuilder.Constant(leftValue);
            var right = DbExpressionBuilder.Constant(rightValue);
            var expression = expressionCreation(left, right);

            Assert.NotNull(expression);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(expressionKind, expression.ExpressionKind);

            var invalidLeft = DbExpressionBuilder.Constant(12);
            var invalidRight = DbExpressionBuilder.Constant("Bar");

            Assert.Throws<ArgumentException>(() => expressionCreation(invalidLeft, invalidRight));
        }

        #endregion

        #region Logical
        
        [Fact]
        public void And_basic_test()
        {
            var left = DbExpressionBuilder.True;
            var right = DbExpressionBuilder.False;
            var expression = DbExpressionBuilder.And(left, right);

            Assert.NotNull(expression);
            Assert.IsType<DbAndExpression>(expression);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.And, expression.ExpressionKind);
        }

        [Fact]
        public void And_no_common_boolean_result_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() =>
                DbExpressionBuilder.And(DbExpressionBuilder.True, DbExpressionBuilder.Constant(1))).Message;

            Assert.True(message.Contains(Strings.Cqt_And_BooleanArgumentsRequired));
        }

        [Fact]
        public void Or_basic_test()
        {
            var left = DbExpressionBuilder.True;
            var right = DbExpressionBuilder.False;
            var expression = DbExpressionBuilder.Or(left, right);

            Assert.NotNull(expression);
            Assert.IsType<DbOrExpression>(expression);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Or, expression.ExpressionKind);
        }

        [Fact]
        public void Or_no_common_boolean_result_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() =>
                DbExpressionBuilder.Or(DbExpressionBuilder.True, DbExpressionBuilder.Constant(1))).Message;

            Assert.True(message.Contains(Strings.Cqt_Or_BooleanArgumentsRequired));
        }

        [Fact]
        public void Not_basic_test()
        {
            var argument = DbExpressionBuilder.True;
            var expression = DbExpressionBuilder.Not(argument);

            Assert.NotNull(expression);
            Assert.IsType<DbNotExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Not, expression.ExpressionKind);
        }

        [Fact]
        public void Not_non_boolean_result_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() =>
                DbExpressionBuilder.Not(DbExpressionBuilder.Constant(1))).Message;

            Assert.True(message.Contains(Strings.Cqt_Not_BooleanArgumentRequired));
        }

        #endregion

        #region IsNull

        [Fact]
        public void IsNull_basic()
        {
            var argument = DbExpressionBuilder.Constant("NullableValue");
            var expression = DbExpressionBuilder.IsNull(argument);

            VerifyIsNull(expression, argument);
        }

        [Fact]
        public void IsNull_null_expression()
        {
            var argument = DbExpressionBuilder.Null(this.stringTypeUsage);
            var expression = DbExpressionBuilder.IsNull(argument);

            VerifyIsNull(expression, argument);
        }

        [Fact]
        public void DbIsNull_member_based_enum_value()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Diamonds);
            var expression = DbExpressionBuilder.IsNull(argument);

            VerifyIsNull(expression, argument);
        }

        [Fact]
        public void DbIsNull_number_based_enum_value()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, (byte)1);
            var expression = DbExpressionBuilder.IsNull(argument);

            VerifyIsNull(expression, argument);
        }

        [Fact]
        public void IsNull_RowType()
        {
            var argument = new Row(((DbExpression)"columnValue").As("C1"));
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.IsNull(argument)).Message;

            Assert.True(message.Contains(Strings.Cqt_IsNull_InvalidType));
        }

        private void VerifyIsNull(DbIsNullExpression expression, DbExpression argument)
        {
            Assert.NotNull(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.IsNull, expression.ExpressionKind);
        }

        #endregion

        #region Like

        [Fact]
        public void Like_without_escape()
        {
            var argument = DbExpressionBuilder.Constant("TestString");
            var pattern = DbExpressionBuilder.Constant("%stStr%");
            var expression = DbExpressionBuilder.Like(argument, pattern);

            Assert.NotNull(expression);
            Assert.IsType<DbLikeExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Same(pattern, expression.Pattern);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Like, expression.ExpressionKind);
        }

        [Fact]
        public void Like_with_escape()
        {
            var argument = DbExpressionBuilder.Constant("TestString");
            var pattern = DbExpressionBuilder.Constant("%stStr%");
            var escape = DbExpressionBuilder.Constant("%");
            var expression = DbExpressionBuilder.Like(argument, pattern, escape);

            Assert.NotNull(expression);
            Assert.IsType<DbLikeExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Same(pattern, expression.Pattern);
            Assert.Same(escape, expression.Escape);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Like, expression.ExpressionKind);
        }

        #endregion

        #region Constant

        [Fact]
        public void Constant_true()
        {
            ValidateConstant(DbExpressionBuilder.True, true, "Edm.Boolean");
        }

        [Fact]
        public void Constant_false()
        {
            ValidateConstant(DbExpressionBuilder.False, false, "Edm.Boolean");
        }

        [Fact]
        public void Constant_boolean_value_true()
        {
            ValidateConstant(DbExpressionBuilder.Constant(true), true, "Edm.Boolean");
        }

        [Fact]
        public void Constant_boolean_value_false()
        {
            ValidateConstant(DbExpressionBuilder.Constant(false), false, "Edm.Boolean");
        }

        [Fact]
        public void Constant_string_value()
        {
            string value = "Test";
            ValidateConstant(DbExpressionBuilder.Constant(value), value, "Edm.String");
        }

        [Fact]
        public void Constant_int_value()
        {
            var value = 42;
            ValidateConstant(DbExpressionBuilder.Constant(value), value, "Edm.Int32");
        }

        [Fact]
        public void Constant_Guid_value()
        {
            var value = Guid.NewGuid();
            ValidateConstant(DbExpressionBuilder.Constant(value), value, "Edm.Guid");
        }

        [Fact]
        public void Constant_Geography_value()
        {
            var value = DbGeography.FromText("POINT(12 57)", 4957);
            ValidateConstant(DbExpressionBuilder.Constant(value), value, "Edm.Geography");
        }

        [Fact]
        public void Constant_null_argument_throws()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Constant(null));
        }

        [Fact]
        public void Constant_invalid_argument_throws()
        {
            Assert.True(
                Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(new List<string>())).Message.Contains(Strings.Cqt_Constant_InvalidType));
        }

        [Fact]
        public void Constant_string_value_string_type()
        {
            var value = "Test";
            ValidateConstant(DbExpressionBuilder.Constant(this.stringTypeUsage, value), value, "Edm.String");
        }

        [Fact]
        public void Constant_string_value_bool_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.booleanTypeUsage, "Test")).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_InvalidValueForType("Edm.Boolean")));
        }

        [Fact]
        public void Constant_string_value_entity_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.productTypeUsage, "Test")).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_InvalidConstantType("MyModel.Product")));
        }

        [Fact]
        public void Constant_invalid_value_valid_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.stringTypeUsage, new List<string>())).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_InvalidValueForType("Edm.String")));
        }

        [Fact]
        public void Constant_passing_enum_value_without_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(CardSuite.Spades)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_InvalidType));
        }

        [Fact]
        public void Constant_enum_with_numeric_value_and_enum_type()
        {
            byte enumConstValue = 3;
            var expression = DbExpressionBuilder.Constant(this.enumTypeUsage, enumConstValue);

            ValidateConstant(expression, enumConstValue, "MyModel.CardSuite");
        }

        [Fact]
        public void Constant_enum_with_clr_enum_value_and_enum_type()
        {
            var enumConstValue = CardSuite.Clubs;
            var expression = DbExpressionBuilder.Constant(this.enumTypeUsage, enumConstValue);

            ValidateConstant(expression, enumConstValue, "MyModel.CardSuite");
        }

        [Fact]
        public void Constant_value_of_different_type_than_underlying_enum_value_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, (long)3)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("Int64", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_of_different_type_than_edm_enum_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, FileOptions.None)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("FileOptions", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_missing_value_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.MissingMember.CardSuite.Clubs)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_additional_value_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.AdditionalMember.CardSuite.Diamonds)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_diffrent_underlying_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.DifferentUnderlyingTypes.CardSuite.Hearts)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_non_edm_underlying_type_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.NonEdmCompatibleUnderlyingType.CardSuite.Hearts)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_value_not_existant_in_edm_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.NonExistingMember.CardSuite.Trump)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_underlying_numeric_value_different_than_edm_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.DifferentMemberValue.CardSuite.Hearts)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_clr_enum_with_underlying_numeric_values_swapped_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(this.enumTypeUsage, Enums.SwapedMembersValues.CardSuite.Hearts)).Message;
            Assert.True(message.Contains(Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType("CardSuite", "CardSuite", "Byte")));
        }

        [Fact]
        public void Constant_with_enum_with_same_values()
        {
            string csdl = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""MyModel"">
  <EnumType Name=""CardSuite"" UnderlyingType=""Byte"">
    <Member Name=""Clubs"" Value=""1""/>
    <Member Name=""Diamonds"" Value=""1""/>
    <Member Name=""Hearts"" Value=""1""/>
    <Member Name=""Spades"" Value=""1""/>
  </EnumType>
</Schema>";

            var enumConstValue = Enums.MembersWithSameValues.CardSuite.Diamonds;
            var edmCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) });
            var enumType = TypeUsage.Create(edmCollection.GetType("CardSuite", "MyModel"));
            var expression = DbExpressionBuilder.Constant(enumType, enumConstValue);

            ValidateConstant(expression, enumConstValue, "MyModel.CardSuite");
        }

        private void ValidateConstant(DbConstantExpression expression, object expectedValue, string expectedResultTypeName)
        {
            Assert.NotNull(expression);
            Assert.Equal(DbExpressionKind.Constant, expression.ExpressionKind);
            Assert.Equal(expectedResultTypeName, expression.ResultType.EdmType.FullName);
            Assert.Equal(expectedValue, expression.Value);
        }

        #endregion

        #region FromType

        [Fact]
        public void FromBinary_basic_test()
        {
            TestFromType(new byte[] { 1, 2, 3 }, v => DbExpression.FromBinary(v), "Edm.Binary");
        }

        [Fact]
        public void FromBoolean_basic_test()
        {
            TestFromType(true, v => DbExpression.FromBoolean(v), "Edm.Boolean");
        }

        [Fact]
        public void FromByte_basic_test()
        {
            TestFromType((byte)1, v => DbExpression.FromByte(v), "Edm.Byte");
        }

        [Fact]
        public void FromDateTime_basic_test()
        {
            TestFromType(DateTime.Now, v => DbExpression.FromDateTime(v), "Edm.DateTime");
        }

        [Fact]
        public void FromDateTimeOffset_basic_test()
        {
            TestFromType(DateTimeOffset.Now, v => DbExpression.FromDateTimeOffset(v), "Edm.DateTimeOffset");
        }

        [Fact]
        public void FromDecimal_basic_test()
        {
            TestFromType(123.45m, v => DbExpression.FromDecimal(v), "Edm.Decimal");
        }

        [Fact]
        public void FromDouble_basic_test()
        {
            TestFromType(123.45d, v => DbExpression.FromDouble(v), "Edm.Double");
        }

        [Fact]
        public void FromGeometry_basic_test()
        {
            TestFromType(DbGeometry.FromText("POINT(8 47)", 0), v => DbExpression.FromGeometry(v), "Edm.Geometry");
        }

        [Fact]
        public void FromGeography_basic_test()
        {
            TestFromType(DbGeography.FromText("POINT(8 47)", 4957), v => DbExpression.FromGeography(v), "Edm.Geography");
        }

        [Fact]
        public void FromGuid_basic_test()
        {
            TestFromType(Guid.NewGuid(), v => DbExpression.FromGuid(v), "Edm.Guid");
        }

        [Fact]
        public void FromInt16_basic_test()
        {
            TestFromType((short)123, v => DbExpression.FromInt16(v), "Edm.Int16");
        }

        [Fact]
        public void FromInt32_basic_test()
        {
            TestFromType(123, v => DbExpression.FromInt32(v), "Edm.Int32");
        }

        [Fact]
        public void FromInt64_basic_test()
        {
            TestFromType((long)123, v => DbExpression.FromInt64(v), "Edm.Int64");
        }

        [Fact]
        public void FromSingle_basic_test()
        {
            TestFromType((float)123.45f, v => DbExpression.FromSingle(v), "Edm.Single");
        }

        [Fact]
        public void FromString_basic_test()
        {
            TestFromType("Foo", v => DbExpression.FromString(v), "Edm.String");
        }

        [Fact]
        public void FromBinary_null()
        {
            TestFromTypeNull(DbExpression.FromBinary((byte[])null), "Edm.Binary");
        }

        [Fact]
        public void FromByte_null()
        {
            TestFromTypeNull(DbExpression.FromByte((byte?)null), "Edm.Byte");
        }

        [Fact]
        public void FromDateTime_null()
        {
            TestFromTypeNull(DbExpression.FromDateTime((DateTime?)null), "Edm.DateTime");
        }

        [Fact]
        public void FromDateTimeOffset_null()
        {
            TestFromTypeNull(DbExpression.FromDateTimeOffset((DateTimeOffset?)null), "Edm.DateTimeOffset");
        }

        [Fact]
        public void FromDecimal_null()
        {
            TestFromTypeNull(DbExpression.FromDecimal((decimal?)null), "Edm.Decimal");
        }

        [Fact]
        public void FromDouble_null()
        {
            TestFromTypeNull(DbExpression.FromDouble((double?)null), "Edm.Double");
        }

        [Fact]
        public void FromGeometry_null()
        {
            TestFromTypeNull(DbExpression.FromGeometry((DbGeometry)null), "Edm.Geometry");
        }

        [Fact]
        public void FromGeography_null()
        {
            TestFromTypeNull(DbExpression.FromGeography((DbGeography)null), "Edm.Geography");
        }

        [Fact]
        public void FromGuid_null()
        {
            TestFromTypeNull(DbExpression.FromGuid((Guid?)null), "Edm.Guid");
        }

        [Fact]
        public void FromInt16_null()
        {
            TestFromTypeNull(DbExpression.FromInt16((short?)null), "Edm.Int16");
        }

        [Fact]
        public void FromInt32_null()
        {
            TestFromTypeNull(DbExpression.FromInt32((int?)null), "Edm.Int32");
        }

        [Fact]
        public void FromInt64_null()
        {
            TestFromTypeNull(DbExpression.FromInt64((long?)null), "Edm.Int64");
        }

        [Fact]
        public void FromSingle_null()
        {
            TestFromTypeNull(DbExpression.FromSingle((float?)null), "Edm.Single");
        }

        [Fact]
        public void FromString_null()
        {
            TestFromTypeNull(DbExpression.FromString((string)null), "Edm.String");
        }

        private void TestFromType<TValue>(TValue value, Func<TValue, DbExpression> expressionCreation, string expectedEdmTypeName)
        {
            var expression = expressionCreation(value);

            Assert.IsType<DbConstantExpression>(expression);
            Assert.Equal(DbExpressionKind.Constant, expression.ExpressionKind);
            Assert.Equal(expectedEdmTypeName, expression.ResultType.EdmType.FullName);
            Assert.Equal(value, ((DbConstantExpression)expression).Value);
        }

        private void TestFromTypeNull(DbExpression expression, string expectedEdmTypeName)
        {
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
            Assert.Equal(expectedEdmTypeName, expression.ResultType.EdmType.FullName);
        }

        #endregion

        #region Implicit cast operator

        [Fact]
        public void Implicit_cast_from_Binary()
        {
            var value = new byte[] { 1, 2, 3 };
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Binary");
        }

        [Fact]
        public void Implicit_cast_from_Boolean()
        {
            var value = true;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Boolean");
        }

        [Fact]
        public void Implicit_cast_from_Byte()
        {
            var value = (byte)1;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Byte");
        }

        [Fact]
        public void Implicit_cast_from_DateTime()
        {
            var value = DateTime.Now;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.DateTime");
        }

        [Fact]
        public void Implicit_cast_from_DateTimeOffset()
        {
            var value = DateTimeOffset.Now;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.DateTimeOffset");
        }

        [Fact]
        public void Implicit_cast_from_Decimal()
        {
            var value = 123.45m;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Decimal");
        }

        [Fact]
        public void Implicit_cast_from_Double()
        {
            var value = 123.45d;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Double");
        }

        [Fact]
        public void Implicit_cast_from_Geometry()
        {
            var value = DbGeometry.FromText("POINT(8 47)", 0);
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Geometry");
        }

        [Fact]
        public void Implicit_cast_from_Geography()
        {
            var value = DbGeography.FromText("POINT(8 47)", 4957);
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Geography");
        }

        [Fact]
        public void Implicit_cast_from_Guid()
        {
            var value = Guid.NewGuid();
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Guid");
        }

        [Fact]
        public void Implicit_cast_from_Int16()
        {
            var value = (short)123;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Int16");
        }

        [Fact]
        public void Implicit_cast_from_Int32()
        {
            var value = 123;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Int32");
        }

        [Fact]
        public void Implicit_cast_from_Int64()
        {
            var value = (long)123;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Int64");
        }

        [Fact]
        public void Implicit_cast_from_Single()
        {
            var value = (float)123.45f;
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.Single");
        }

        [Fact]
        public void Implicit_cast_from_String()
        {
            var value = "Foo";
            DbExpression expression = value;
            VerifyImplicitCast(value, expression, "Edm.String");
        }

        [Fact]
        public void Implicit_cast_from_null_Binary()
        {
            DbExpression expression = (byte[])null;
            VerifyNullImplicitCast(expression, "Edm.Binary");
        }

        [Fact]
        public void Implicit_cast_from_null_Byte()
        {
            DbExpression expression = (byte?)null;
            VerifyNullImplicitCast(expression, "Edm.Byte");
        }

        [Fact]
        public void Implicit_cast_from_null_DateTime()
        {
            DbExpression expression = (DateTime?)null;
            VerifyNullImplicitCast(expression, "Edm.DateTime");
        }

        [Fact]
        public void Implicit_cast_from_null_DateTimeOffset()
        {
            DbExpression expression = (DateTimeOffset?)null;
            VerifyNullImplicitCast(expression, "Edm.DateTimeOffset");
        }

        [Fact]
        public void Implicit_cast_from_null_Decimal()
        {
            DbExpression expression = (decimal?)null;
            VerifyNullImplicitCast(expression, "Edm.Decimal");
        }

        [Fact]
        public void Implicit_cast_from_null_Double()
        {
            DbExpression expression = (double?)null;
            VerifyNullImplicitCast(expression, "Edm.Double");
        }

        [Fact]
        public void Implicit_cast_from_null_Geometry()
        {
            DbExpression expression = (DbGeometry)null;
            VerifyNullImplicitCast(expression, "Edm.Geometry");
        }

        [Fact]
        public void Implicit_cast_from_null_Geography()
        {
            DbExpression expression = (DbGeography)null;
            VerifyNullImplicitCast(expression, "Edm.Geography");
        }

        [Fact]
        public void Implicit_cast_from_null_Guid()
        {
            DbExpression expression = (Guid?)null;
            VerifyNullImplicitCast(expression, "Edm.Guid");
        }

        [Fact]
        public void Implicit_cast_from_null_Int16()
        {
            DbExpression expression = (short?)null;
            VerifyNullImplicitCast(expression, "Edm.Int16");
        }

        [Fact]
        public void Implicit_cast_from_null_Int32()
        {
            DbExpression expression = (int?)null;
            VerifyNullImplicitCast(expression, "Edm.Int32");
        }

        [Fact]
        public void Implicit_cast_from_null_Int64()
        {
            DbExpression expression = (long?)null;
            VerifyNullImplicitCast(expression, "Edm.Int64");
        }

        [Fact]
        public void Implicit_cast_from_null_Single()
        {
            DbExpression expression = (float?)null;
            VerifyNullImplicitCast(expression, "Edm.Single");
        }

        [Fact]
        public void Implicit_cast_from_null_String()
        {
            DbExpression expression = (string)null;
            VerifyNullImplicitCast(expression, "Edm.String");
        }

        private void VerifyImplicitCast(object value, DbExpression expression, string expectedEdmTypeName)
        {
            Assert.IsType<DbConstantExpression>(expression);
            Assert.Equal(DbExpressionKind.Constant, expression.ExpressionKind);
            Assert.Equal(expectedEdmTypeName, expression.ResultType.EdmType.FullName);
            Assert.Equal(value, ((DbConstantExpression)expression).Value);
        }

        private void VerifyNullImplicitCast(DbExpression expression, string expectedEdmTypeName)
        {
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
            Assert.Equal(expectedEdmTypeName, expression.ResultType.EdmType.FullName);
        }

        #endregion

        #region Parameter
            
        [Fact]
        public void Basic_parameter_test()
        {
            string parameterName = "prm";
            var expression = DbExpressionBuilder.Parameter(this.stringTypeUsage, parameterName);

            Assert.NotNull(expression);
            Assert.IsType<DbParameterReferenceExpression>(expression);
            Assert.Equal(DbExpressionKind.ParameterReference, expression.ExpressionKind);
            Assert.Equal(parameterName, expression.ParameterName);
            Assert.Equal(this.stringTypeUsage, expression.ResultType);
        }

        [Fact]
        public void Parameter_with_empty_name_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Parameter(this.stringTypeUsage, string.Empty)).Message;
            Assert.True(message.Contains(Strings.Cqt_CommandTree_InvalidParameterName(string.Empty)));
        }

        [Fact]
        public void Parameter_with_white_spaces_for_name_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Parameter(this.stringTypeUsage, "prm 1")).Message;
            Assert.True(message.Contains(Strings.Cqt_CommandTree_InvalidParameterName("prm 1")));
        }

        [Fact]
        public void Parameter_with_invalid_chars_in_name_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Parameter(this.stringTypeUsage, "prm!@#")).Message;
            Assert.True(message.Contains(Strings.Cqt_CommandTree_InvalidParameterName("prm!@#")));
        }



        #endregion

        #region Null

        [Fact]
        public void Null_entity()
        {
            var expression = DbExpressionBuilder.Null(this.productTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal("MyModel.Product", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
        }

        [Fact]
        public void Null_int()
        {
            var expression = DbExpressionBuilder.Null(this.integerTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal("Edm.Int32", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
        }

        [Fact]
        public void Null_Geography()
        {
            var expression = DbExpressionBuilder.Null(this.geographyTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal("Edm.Geography", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
        }

        [Fact]
        public void Null_Geometry()
        {
            var expression = DbExpressionBuilder.Null(this.geometryTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbNullExpression>(expression);
            Assert.Equal("Edm.Geometry", expression.ResultType.EdmType.FullName);
            Assert.Equal(DbExpressionKind.Null, expression.ExpressionKind);
        }

        #endregion

        #region VariableReference

        [Fact]
        public void VariableReference_basic_test()
        {
            string varName = "Var1";
            var expression = DbExpressionBuilder.Variable(this.stringTypeUsage, varName);

            Assert.NotNull(expression);
            Assert.IsType<DbVariableReferenceExpression>(expression);
            Assert.Equal(varName, expression.VariableName);
            Assert.Equal("Edm.String", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void VariableReference_empty_name_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Variable(this.stringTypeUsage, "")).Message;
            Assert.True(message.Contains(Strings.ArgumentIsNullOrWhitespace("name")));
        }
        
        #endregion

        #region Type operators

        [Fact]
        public void Cast_basic_test()
        {
            var argument = DbExpressionBuilder.Constant((int)1);
            var expression = DbExpressionBuilder.CastTo(argument, this.stringTypeUsage);
            VerifyCast(expression, argument, "Edm.String");
        }

        [Fact]
        public void Cast_number_to_enum()
        {
            var argument = DbExpressionBuilder.Constant((Int64)Int64.MaxValue);
            var expression = DbExpressionBuilder.CastTo(argument, this.enumTypeUsage);
            VerifyCast(expression, argument, "MyModel.CardSuite");
        }

        [Fact]
        public void Cast_enum_to_number()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var expression = DbExpressionBuilder.CastTo(argument, this.stringTypeUsage);
            VerifyCast(expression, argument, "Edm.String");
        }

        [Fact]
        public void Cast_enum_to_itself()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var expression = DbExpressionBuilder.CastTo(argument, this.enumTypeUsage);
            VerifyCast(expression, argument, "MyModel.CardSuite");
        }

        [Fact]
        public void Cast_non_convertible()
        {
            var argument = DbExpressionBuilder.Constant((int)1);

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CastTo(argument, this.productTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_Cast_InvalidCast("Edm.Int32", "MyModel.Product")));
        }

        [Fact]
        public void Cast_enum_to_entity_throws()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CastTo(argument, this.productTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_Cast_InvalidCast("MyModel.CardSuite", "MyModel.Product")));
        }

        [Fact]
        public void Cast_enum_to_another_enum_throws()
        {
            var argument = DbExpressionBuilder.Constant(this.enumTypeUsage, CardSuite.Hearts);
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CastTo(argument, this.enumTypeUsage2)).Message;
            Assert.True(message.Contains(Strings.Cqt_Cast_InvalidCast("MyModel.CardSuite", "Foo.CardSuite")));
        }

        private void VerifyCast(DbCastExpression expression, DbExpression argument, string expectedResultType)
        {
            Assert.NotNull(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal(DbExpressionKind.Cast, expression.ExpressionKind);
            Assert.Equal(expectedResultType, expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void OfType_same_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.OfType(scan, this.productTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbOfTypeExpression>(expression);
            Assert.Same(scan, expression.Argument);
            Assert.Equal(DbExpressionKind.OfType, expression.ExpressionKind);
            Assert.Equal("MyModel.Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void OfType_derived_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.OfType(scan, this.discontinuedProductTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbOfTypeExpression>(expression);
            Assert.Same(scan, expression.Argument);
            Assert.Equal(DbExpressionKind.OfType, expression.ExpressionKind);
            Assert.Equal("MyModel.DiscontinuedProduct", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void OfType_non_polymorphic_type()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.OfType(DbExpressionBuilder.Scan(this.productsEntitySet), this.stringTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicTypeRequired("Edm.String")));
        }

        [Fact]
        public void OfType_non_polymorphic_argument()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.OfType(DbExpressionBuilder.NewCollection(1, 2, 3, 4, 5), this.productTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicArgRequired("DbOfTypeExpression")));
        }

        [Fact]
        public void OfType_unrelated_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.OfType(scan, this.categoryTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicArgRequired("DbOfTypeExpression")));
        }

        [Fact]
        public void OfTypeOnly_same_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.OfTypeOnly(scan, this.productTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbOfTypeExpression>(expression);
            Assert.Same(scan, expression.Argument);
            Assert.Equal(DbExpressionKind.OfTypeOnly, expression.ExpressionKind);
            Assert.Equal("MyModel.Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void OfTypeOnly_derived_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.OfTypeOnly(scan, this.discontinuedProductTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbOfTypeExpression>(expression);
            Assert.Same(scan, expression.Argument);
            Assert.Equal(DbExpressionKind.OfTypeOnly, expression.ExpressionKind);
            Assert.Equal("MyModel.DiscontinuedProduct", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void TreatAs_same_type()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var argument = scan.Element();
            var expression = DbExpressionBuilder.TreatAs(argument, this.productTypeUsage);

            Assert.NotNull(expression);
            Assert.IsType<DbTreatExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal(DbExpressionKind.Treat, expression.ExpressionKind);
            Assert.Equal("MyModel.Product", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void TreatAs_non_polymorphic()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.TreatAs(scan.Element(), this.stringTypeUsage)).Message;
            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicTypeRequired("Edm.String")));
        }

        [Fact]
        public void TreatAs_unrelated()
        {
            var scan = DbExpressionBuilder.Scan(this.productsEntitySet);
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.TreatAs(scan.Element(), this.categoryTypeUsage)).Message;

            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicArgRequired("DbTreatExpression")));
        }

        [Fact]
        public void IsOf_basic_test()
        {
            var argument = this.productsEntitySet.Scan().Element();
            var expression = DbExpressionBuilder.IsOf(argument, argument.ResultType);

            Assert.NotNull(expression);
            Assert.IsType<DbIsOfExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal(DbExpressionKind.IsOf, expression.ExpressionKind);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void IsOf_non_polymorphic_type()
        {
            var argument = this.productsEntitySet.Scan().Element();
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.IsOf(argument, this.stringTypeUsage)).Message;

            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicTypeRequired("Edm.String")));
        }

        [Fact]
        public void IsOf_unrelated_type()
        {
            var argument = this.productsEntitySet.Scan().Element();
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.IsOf(argument, this.categoryTypeUsage)).Message;

            Assert.True(message.Contains(Strings.Cqt_General_PolymorphicArgRequired("DbIsOfExpression")));
        }

        [Fact]
        public void IsOfOnly_basic_test()
        {
            var argument = this.productsEntitySet.Scan().Element();
            var expression = DbExpressionBuilder.IsOfOnly(argument, argument.ResultType);

            Assert.NotNull(expression);
            Assert.IsType<DbIsOfExpression>(expression);
            Assert.Same(argument, expression.Argument);
            Assert.Equal(DbExpressionKind.IsOfOnly, expression.ExpressionKind);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
        }

        #endregion

        #region Relational operators

        [Fact]
        public void CrossApply_basic_test()
        {
            var inputBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "Input");
            var applyBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.NewCollection(1, 2, 3, 5, 8, 13, 21), "Apply");
            var expression = DbExpressionBuilder.CrossApply(inputBinding, applyBinding);

            Assert.NotNull(expression);
            Assert.IsType<DbApplyExpression>(expression);
            Assert.Same(inputBinding, expression.Input);
            Assert.Same(applyBinding, expression.Apply);
            Assert.Equal(DbExpressionKind.CrossApply, expression.ExpressionKind);
        }

        [Fact]
        public void OuterApply_basic_test()
        {
            var inputBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "Input");
            var applyBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.NewCollection(1, 2, 3, 5, 8, 13, 21), "Apply");
            var expression = DbExpressionBuilder.OuterApply(inputBinding, applyBinding);

            Assert.NotNull(expression);
            Assert.IsType<DbApplyExpression>(expression);
            Assert.Same(inputBinding, expression.Input);
            Assert.Same(applyBinding, expression.Apply);
            Assert.Equal(DbExpressionKind.OuterApply, expression.ExpressionKind);
        }

        [Fact]
        public void CrossApply_duplicate_name_throws()
        {
            var inputBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "Input");
            var applyBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.NewCollection(1, 2, 3, 5, 8, 13, 21), "Input");

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CrossApply(inputBinding, applyBinding)).Message;
            Assert.True(message.Contains(Strings.Cqt_Apply_DuplicateVariableNames));
        }

        [Fact]
        public void Filter_basic_test()
        {
            var input = DbExpressionBuilder.NewCollection(21, 42, 84, 1, 7, 2, 9).BindAs("x");
            var predicate = input.Variable.GreaterThan(5);
            var expression = DbExpressionBuilder.Filter(input, predicate);

            Assert.NotNull(expression);
            Assert.IsType<DbFilterExpression>(expression);
            Assert.Same(input, expression.Input);
            Assert.Same(predicate, expression.Predicate);
        }

        [Fact]
        public void Filter_non_boolean_predicate_throws()
        {
            var input = this.productsEntitySet.Scan().BindAs("Input");
            var predicate = "Non-Boolean expression";

            var message = Assert.Throws<ArgumentException>(() => input.Filter(predicate)).Message;
            Assert.True(message.Contains(Strings.Cqt_ExpressionLink_TypeMismatch("String", "Boolean")));
        }

        [Fact]
        public void Project_constant_from_entity_set()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var projection = DbExpressionBuilder.Constant(42);
            var expression = DbExpressionBuilder.Project(input, projection);

            Assert.NotNull(expression);
            Assert.IsType<DbProjectExpression>(expression);
            Assert.Same(input, expression.Input);
            Assert.Same(projection, expression.Projection);
            Assert.Equal(DbExpressionKind.Project, expression.ExpressionKind);
            Assert.Equal("Edm.Int32", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void Project_row_from_entity_set()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var r1 = new KeyValuePair<string, DbExpression>("p1", input.Variable);
            var r2 = new KeyValuePair<string, DbExpression>("p2", input.Variable.Property("ProductID"));
            var r3 = new KeyValuePair<string, DbExpression>("p3", 42);
            var projection = DbExpressionBuilder.NewRow(new[] { r1, r2, r3 });
            var expression = input.Project(projection);

            Assert.NotNull(expression);
            Assert.IsType<DbProjectExpression>(expression);
            Assert.Same(input, expression.Input);
            Assert.Same(projection, expression.Projection);
            Assert.Equal(DbExpressionKind.Project, expression.ExpressionKind);
            Assert.IsType<RowType>(((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType);
        }

        [Fact]
        public void Sort_one_key_without_collation()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var id = input.Variable.Property("ProductID");
            var sortKey = new DbSortClause(id, true, string.Empty);
            var expression = input.Sort(new[] { sortKey });

            Assert.NotNull(expression);
            Assert.IsType<DbSortExpression>(expression);
            Assert.Same(input, expression.Input);
            Assert.Equal(1, expression.SortOrder.Count);
            Assert.Same(sortKey, expression.SortOrder[0]);
            Assert.Equal(DbExpressionKind.Sort, expression.ExpressionKind);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        #endregion

        #region Joins

        [Fact]
        public void CrossJoin_basic_test()
        {
            var input1 = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "products");
            var input2 = DbExpressionBuilder.NewCollection(new DbExpression[] { "Foo", "Bar", "Baz" }).BindAs("foos");
            var input3 = DbExpressionBuilder.NewCollection(new DbExpression[] { 2, 4, 6, 8 }).BindAs("ints");
            var expression = DbExpressionBuilder.CrossJoin(new[] { input1, input2, input3 });

            Assert.NotNull(expression);
            Assert.IsType<DbCrossJoinExpression>(expression);
            Assert.Equal(3, expression.Inputs.Count);
            Assert.Same(input1, expression.Inputs[0]);
            Assert.Same(input2, expression.Inputs[1]);
            Assert.Same(input3, expression.Inputs[2]);
            Assert.Equal(DbExpressionKind.CrossJoin, expression.ExpressionKind);
            Assert.IsType<RowType>(((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType);
        }

        [Fact]
        public void CrossJoin_duplicate_variable_name_throws()
        {
            var input1 = DbExpressionBuilder.NewCollection(new DbExpression[] { 2, 4, 6, 8 }).BindAs("foo");
            var input2 = DbExpressionBuilder.NewCollection(new DbExpression[] { "Foo", "Bar", "Baz" }).BindAs("foo");
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CrossJoin(new[] { input1, input2 })).Message;

            Assert.True(message.Contains(Strings.Cqt_CrossJoin_DuplicateVariableNames(0, 1, "foo")));
        }

        [Fact]
        public void CrossJoin_with_one_input_throws()
        {
            var input = DbExpressionBuilder.NewCollection(new DbExpression[] { 2, 4, 6, 8 }).BindAs("foo");
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.CrossJoin(new[] { input })).Message;

            Assert.True(message.Contains(Strings.Cqt_CrossJoin_AtLeastTwoInputs));
        }

        [Fact]
        public void LeftOuterJoin_basic_test()
        {
            var left = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "products");
            var right = DbExpressionBuilder.NewCollection(new DbExpression[] { "Foo", "Bar", "Baz" }).BindAs("foos");
            var joinCondition = DbExpressionBuilder.Constant(true);
            var expression = left.LeftOuterJoin(right, joinCondition);

            Assert.NotNull(expression);
            Assert.IsType<DbJoinExpression>(expression);
            Assert.Equal(DbExpressionKind.LeftOuterJoin, expression.ExpressionKind);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Same(joinCondition, expression.JoinCondition);
            Assert.IsType<RowType>(((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType);
        }

        [Fact]
        public void InnerJoin_basic_test()
        {
            var left = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(this.productsEntitySet), "products");
            var right = DbExpressionBuilder.NewCollection(new DbExpression[] { 1, 2, 3 }).BindAs("ints");
            var joinCondition = left.Variable.Property("ProductID").Equal(right.Variable);
            var expression = left.InnerJoin(right, joinCondition);

            Assert.NotNull(expression);
            Assert.IsType<DbJoinExpression>(expression);
            Assert.Equal(DbExpressionKind.InnerJoin, expression.ExpressionKind);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Same(joinCondition, expression.JoinCondition);
            Assert.IsType<RowType>(((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType);
        }

        [Fact]
        public void InnerJoin_duplicate_variable_name_throws()
        {
            var left = DbExpressionBuilder.NewCollection(new DbExpression[] { 2, 4, 6, 8 }).BindAs("foo");
            var right = DbExpressionBuilder.NewCollection(new DbExpression[] { "Foo", "Bar", "Baz" }).BindAs("foo");
            var message = Assert.Throws<ArgumentException>(() => left.InnerJoin(right,  true)).Message;

            Assert.True(message.Contains(Strings.Cqt_Join_DuplicateVariableNames));

        }

        #endregion

        #region Set Operators

        [Fact]
        public void UnionAll_two_collections_with_common_type()
        {
            var left = DbExpressionBuilder.NewCollection(1, 2, 3, 4, 5);
            var right = DbExpressionBuilder.NewCollection(6.1f, 7.2f, 8.3f, 9.4f, 10.5f);
            var expression = left.UnionAll(right);

            VerifyUnionAll(expression, left, right, "Edm.Single");
        }

        [Fact]
        public void UnionAll_matching_enum_types()
        {
            var left = DbExpressionBuilder.NewCollection(DbExpressionBuilder.Constant(enumTypeUsage, CardSuite.Hearts));
            var right = DbExpressionBuilder.NewCollection(DbExpressionBuilder.Constant(enumTypeUsage, CardSuite.Diamonds));
            var expression = left.UnionAll(right);

            VerifyUnionAll(expression, left, right, "MyModel.CardSuite");
        }

        [Fact]
        public void UnionAll_non_collection_input_throws()
        {
            var left = DbExpressionBuilder.NewCollection(1, 2, 3, 4, 5);
            var right = DbExpressionBuilder.Constant(6.1f);
            
            var message = Assert.Throws<ArgumentException>(() => left.UnionAll(right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Binary_CollectionsRequired("DbUnionAllExpression")));
        }

        [Fact]
        public void UnionAll_non_compatible_input_types_throws()
        {
            var left = DbExpressionBuilder.NewCollection(1);
            var right = DbExpressionBuilder.NewCollection("Right");

            var message = Assert.Throws<ArgumentException>(() => left.UnionAll(right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Binary_CollectionsRequired("DbUnionAllExpression")));
        }

        [Fact]
        public void UnionAll_enums_and_numbers_throws()
        {
            var left = DbExpressionBuilder.NewCollection(DbExpressionBuilder.Constant(enumTypeUsage, CardSuite.Hearts));
            var right = DbExpressionBuilder.NewCollection(1);

            var message = Assert.Throws<ArgumentException>(() => left.UnionAll(right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Binary_CollectionsRequired("DbUnionAllExpression")));
            
            var message2 = Assert.Throws<ArgumentException>(() => right.UnionAll(left)).Message;
            Assert.True(message2.Contains(Strings.Cqt_Binary_CollectionsRequired("DbUnionAllExpression")));
        }

        [Fact]
        public void UnionAll_non_matching_enum_types_throws()
        {
            var left = DbExpressionBuilder.NewCollection(DbExpressionBuilder.Constant(enumTypeUsage, CardSuite.Hearts));
            var right = DbExpressionBuilder.NewCollection(DbExpressionBuilder.Constant(enumTypeUsage2, CardSuite.Hearts));

            var message = Assert.Throws<ArgumentException>(() => left.UnionAll(right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Binary_CollectionsRequired("DbUnionAllExpression")));
        }

        private void VerifyUnionAll(DbUnionAllExpression expression, DbExpression left, DbExpression right, string expectedResultType)
        {
            Assert.NotNull(expression);
            Assert.Equal(DbExpressionKind.UnionAll, expression.ExpressionKind);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal(expectedResultType, ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.FullName);
        }

        [Fact]
        public void Intersect_basic_test()
        {
            var left = DbExpressionBuilder.Scan(this.productsEntitySet);
            var right = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.Intersect(left, right);

            Assert.NotNull(expression);
            Assert.IsType<DbIntersectExpression>(expression);
            Assert.Equal(DbExpressionKind.Intersect, expression.ExpressionKind);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Intersect_incomparable_input_types_throws()
        {
            var left = DbExpressionBuilder.Scan(this.productsEntitySet);
            var right = DbExpressionBuilder.NewCollection(1, 2, 3, 4, 5);

            var message = Assert.Throws<ArgumentException>(() => left.Intersect(right)).Message;
            Assert.True(message.Contains(Strings.Cqt_Binary_CollectionsRequired("DbIntersectExpression")));
        }

        [Fact]
        public void Except_basic_test()
        {
            var left = DbExpressionBuilder.Scan(this.productsEntitySet);
            var right = DbExpressionBuilder.Scan(this.productsEntitySet);
            var expression = DbExpressionBuilder.Except(left, right);

            Assert.NotNull(expression);
            Assert.IsType<DbExceptExpression>(expression);
            Assert.Equal(DbExpressionKind.Except, expression.ExpressionKind);
            Assert.Same(left, expression.Left);
            Assert.Same(right, expression.Right);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Distinct_on_collection_of_entities()
        {
            var argument = this.productsEntitySet.Scan();
            var expression = DbExpressionBuilder.Distinct(argument);

            Assert.NotNull(expression);
            Assert.IsType<DbDistinctExpression>(expression);
            Assert.Equal(DbExpressionKind.Distinct, expression.ExpressionKind);
            Assert.Same(argument, expression.Argument);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Distinct_on_collection_of_records()
        {

            var col1 = DbExpressionBuilder.NewCollection(1, 2, 3, 4, 5, 6).BindAs("ints");
            var col2 = DbExpressionBuilder.NewCollection("1", "2", "3", "4", "5", "6").BindAs("strings");
            var argument = DbExpressionBuilder.CrossJoin(new[] { col1, col2 });
            var expression = DbExpressionBuilder.Distinct(argument);

            Assert.NotNull(expression);
            Assert.IsType<DbDistinctExpression>(expression);
            Assert.Equal(DbExpressionKind.Distinct, expression.ExpressionKind);
            Assert.Same(argument, expression.Argument);
            Assert.IsType<RowType>(((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType);
        }

        [Fact]
        public void Distinct_on_collection_of_non_comparable_types_throws()
        {
            var argument = DbExpressionBuilder.NewCollection(DbExpressionBuilder.NewCollection(1, 2, 3, 4));
            var message = Assert.Throws<ArgumentException>(() => argument.Distinct()).Message;

            Assert.True(message.Contains(Strings.Cqt_Distinct_InvalidCollection));
        }

        [Fact]
        public void Element_basic_test()
        {
            var argument = this.productsEntitySet.Scan();
            var expression = DbExpressionBuilder.Element(argument);

            Assert.NotNull(expression);
            Assert.IsType<DbElementExpression>(expression);
            Assert.Equal(DbExpressionKind.Element, expression.ExpressionKind);
            Assert.Same(argument, expression.Argument);
            Assert.Equal("Product", expression.ResultType.EdmType.Name);
        }

        [Fact]
        public void Element_on_non_collection_throws()
        {
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Constant(1).Element()).Message;
            Assert.True(message.Contains(Strings.Cqt_Unary_CollectionRequired("DbElementExpression")));
        }

        [Fact]
        public void IsEmpty_basic_test()
        {
            var argument = this.productsEntitySet.Scan();
            var expression = DbExpressionBuilder.IsEmpty(argument);

            Assert.NotNull(expression);
            Assert.IsType<DbIsEmptyExpression>(expression);
            Assert.Equal(DbExpressionKind.IsEmpty, expression.ExpressionKind);
            Assert.Same(argument, expression.Argument);
            Assert.Equal("Edm.Boolean", expression.ResultType.EdmType.FullName);
        }

        #endregion

        #region Property

        [Fact]
        public void Property_from_edm_property()
        {
            var instance = this.productsEntitySet.Scan().Element();
            var edmProperty = (EdmProperty)((StructuralType)this.productTypeUsage.EdmType).Members["ProductID"];
            var expression = instance.Property(edmProperty);

            Assert.NotNull(expression);
            Assert.IsType<DbPropertyExpression>(expression);
            Assert.Equal(DbExpressionKind.Property, expression.ExpressionKind);
            Assert.Same(instance, expression.Instance);
            Assert.Same(edmProperty, expression.Property);
            Assert.Equal("Edm.Int32", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void Property_from_navigation()
        {
            var instance = this.categoriesEntitySet.Scan().Element();
            var navigationProperty = (NavigationProperty)((StructuralType)this.categoryTypeUsage.EdmType).Members["Products"];
            var expression = instance.Property(navigationProperty);

            Assert.NotNull(expression);
            Assert.IsType<DbPropertyExpression>(expression);
            Assert.Equal(DbExpressionKind.Property, expression.ExpressionKind);
            Assert.Same(instance, expression.Instance);
            Assert.Same(navigationProperty, expression.Property);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Property_from_entity_reference()
        {
            var instance = this.productsEntitySet.Scan().Element();
            var navigationProperty = (NavigationProperty)((StructuralType)this.productTypeUsage.EdmType).Members["Category"];
            var expression = instance.Property(navigationProperty);

            Assert.NotNull(expression);
            Assert.IsType<DbPropertyExpression>(expression);
            Assert.Equal(DbExpressionKind.Property, expression.ExpressionKind);
            Assert.Same(instance, expression.Instance);
            Assert.Same(navigationProperty, expression.Property);
            Assert.Equal("Category", expression.ResultType.EdmType.Name);
        }

        [Fact]
        public void Property_from_string()
        {
            var instance = this.productsEntitySet.Scan().Element();
            var expression = instance.Property("ProductID");

            Assert.NotNull(expression);
            Assert.IsType<DbPropertyExpression>(expression);
            Assert.Equal(DbExpressionKind.Property, expression.ExpressionKind);
            Assert.Same(instance, expression.Instance);
            Assert.Equal("ProductID", expression.Property.Name);
            Assert.Equal("Edm.Int32", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void Property_non_existant_property_throws()
        {
            var message = Assert.Throws<ArgumentOutOfRangeException>(() => this.productsEntitySet.Scan().Element().Property("Foo")).Message;
            Assert.True(message.Contains(Strings.Cqt_Factory_NoSuchProperty("Foo", "MyModel.Product")));
        }

        [Fact]
        public void Property_property_is_case_sensitive()
        {
            var message = Assert.Throws<ArgumentOutOfRangeException>(() => this.productsEntitySet.Scan().Element().Property("PRODUCTid")).Message;
            Assert.True(message.Contains(Strings.Cqt_Factory_NoSuchProperty("PRODUCTid", "MyModel.Product")));
        }

        #endregion

        #region Case

        [Fact]
        public void Case_basic_test()
        {
            var when = this.productsEntitySet.Scan().Element().Property("ProductID").Equal(1);
            var then = DbExpressionBuilder.Constant((int)10);
            var elseExpr = DbExpressionBuilder.Constant((byte)5);

            DbCaseExpression expression = DbExpressionBuilder.Case(
                new DbExpression[] { when },
                new DbExpression[] { then },
                elseExpr
            );

            Assert.NotNull(expression);
            Assert.IsType<DbCaseExpression>(expression);
            Assert.Equal(1, expression.When.Count);
            Assert.Equal(1, expression.Then.Count);
            Assert.Same(when, expression.When[0]);
            Assert.Same(then, expression.Then[0]);
            Assert.Same(elseExpr, expression.Else);
            Assert.Equal(DbExpressionKind.Case, expression.ExpressionKind);
            Assert.Equal("Edm.Int32", expression.ResultType.EdmType.FullName);
        }

        [Fact]
        public void Case_non_boolean_when_throws()
        {
            var when = "x > 5";
            var then = DbExpressionBuilder.Constant((int)10);
            var elseExpr = DbExpressionBuilder.Constant((int)5);

            var message = Assert.Throws<ArgumentException>(
                () => DbExpressionBuilder.Case(new DbExpression[] { when }, new DbExpression[] { then }, elseExpr)).Message;

            Assert.True(message.Contains(Strings.Cqt_ExpressionLink_TypeMismatch("String", "Boolean")));
        }

        [Fact]
        public void Case_no_common_type_between_then_arguments_throws()
        {
            var whens = new DbExpression[] { DbExpressionBuilder.True, DbExpressionBuilder.False };
            var thens = new DbExpression[] { 10, "Foo" };
            DbExpression elseExpr = 30;
            
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Case(whens, thens, elseExpr)).Message;
            Assert.True(message.Contains(Strings.Cqt_Case_InvalidResultType));
        }

        [Fact]
        public void Case_no_common_type_between_then_and_else_arguments_throws()
        {
            var whens = new DbExpression[] { DbExpressionBuilder.True, DbExpressionBuilder.False };
            var thens = new DbExpression[] { 10, 20 };
            DbExpression elseExpr = "Foo";
            
            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Case(whens, thens, elseExpr)).Message;
            Assert.True(message.Contains(Strings.Cqt_Case_InvalidResultType));
        }

        [Fact]
        public void Case_when_and_then_count_mismatch_throws()
        {
            var whens = new DbExpression[] { DbExpressionBuilder.True, DbExpressionBuilder.False };
            var thens = new DbExpression[] { 10 };
            DbExpression elseExpr = 20;

            var message = Assert.Throws<ArgumentException>(() => DbExpressionBuilder.Case(whens, thens, elseExpr)).Message;
            Assert.True(message.Contains(Strings.Cqt_Case_WhensMustEqualThens));
        }

        #endregion

        #region Paging

        [Fact]
        public void Limit_basic_test()
        {
            var argument = this.productsEntitySet.Scan();
            var limit = DbExpressionBuilder.Constant((long)5);
            var expression = DbExpressionBuilder.Limit(argument, limit);

            Assert.NotNull(expression);
            Assert.IsType<DbLimitExpression>(expression);
            Assert.Equal(DbExpressionKind.Limit, expression.ExpressionKind);
            Assert.Same(argument, expression.Argument);
            Assert.Same(limit, expression.Limit);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Limit_with_parameter_as_limit_value()
        {
            var argument = this.productsEntitySet.Scan();
            var limit = DbExpressionBuilder.Parameter(this.integerTypeUsage, "prm");
            var expression = DbExpressionBuilder.Limit(argument, limit);

            Assert.NotNull(expression);
            Assert.Same(limit, expression.Limit);
        }

        [Fact]
        public void Limit_invalid_limit_expression_throws()
        {
            var argument = this.productsEntitySet.Scan();
            var limit = DbExpressionBuilder.Plus(1, 1);
            
            var message = Assert.Throws<ArgumentException>(() => argument.Limit(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Limit_ConstantOrParameterRefRequired));
        }

        [Fact]
        public void Limit_non_integer_limit_value_throws()
        {
            var argument = this.productsEntitySet.Scan();
            var limit = DbExpressionBuilder.Constant(1f);

            var message = Assert.Throws<ArgumentException>(() => argument.Limit(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Limit_IntegerRequired));
        }

        [Fact]
        public void Limit_negative_limit_value_throws()
        {
            var argument = this.productsEntitySet.Scan();
            var limit = DbExpressionBuilder.Constant(-1);

            var message = Assert.Throws<ArgumentException>(() => argument.Limit(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Limit_NonNegativeLimitRequired));
        }

        [Fact]
        public void Skip_basic_test()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var sortOrder = new DbSortClause(input.Variable.Property("ProductID"), true, string.Empty);
            var argument = input.Sort(new[] { sortOrder });
            var count = DbExpressionBuilder.Constant(1);
            var expression = DbExpressionBuilder.Skip(argument, count);

            Assert.NotNull(expression);
            Assert.IsType<DbSkipExpression>(expression);
            Assert.Equal(DbExpressionKind.Skip, expression.ExpressionKind);
            Assert.Same(input, expression.Input);
            Assert.Same(count, expression.Count);
            Assert.Equal("Product", ((CollectionType)expression.ResultType.EdmType).TypeUsage.EdmType.Name);
        }

        [Fact]
        public void Skip_with_parameter_as_count_value()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var sortOrder = new DbSortClause(input.Variable.Property("ProductID"), true, string.Empty);
            var argument = input.Sort(new[] { sortOrder });
            var count = DbExpressionBuilder.Parameter(this.integerTypeUsage, "prm");
            var expression = DbExpressionBuilder.Skip(argument, count);

            Assert.Same(count, expression.Count);
        }

        [Fact]
        public void Skip_invalid_count_expression_throws()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var sortOrder = new DbSortClause(input.Variable.Property("ProductID"), true, string.Empty);
            var argument = input.Sort(new[] { sortOrder });
            var limit = DbExpressionBuilder.Plus(1, 1);

            var message = Assert.Throws<ArgumentException>(() => argument.Skip(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Skip_ConstantOrParameterRefRequired));
        }

        [Fact]
        public void Skip_non_integer_count_value_throws()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var sortOrder = new DbSortClause(input.Variable.Property("ProductID"), true, string.Empty);
            var argument = input.Sort(new[] { sortOrder });
            var limit = DbExpressionBuilder.Constant(1f);

            var message = Assert.Throws<ArgumentException>(() => argument.Skip(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Skip_IntegerRequired));
        }

        [Fact]
        public void Skip_negative_count_value_throws()
        {
            var input = this.productsEntitySet.Scan().BindAs("p");
            var sortOrder = new DbSortClause(input.Variable.Property("ProductID"), true, string.Empty);
            var argument = input.Sort(new[] { sortOrder });
            var limit = DbExpressionBuilder.Constant(-1);

            var message = Assert.Throws<ArgumentException>(() => argument.Skip(limit)).Message;
            Assert.True(message.Contains(Strings.Cqt_Skip_NonNegativeCountRequired));
        }

        #endregion

        #region Null checks

        [Fact]
        public void Null_check_Aggregate()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Aggregate(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Aggregate(new EdmFunction(), null));
        }

        [Fact]
        public void Null_check_AggregateDistinct()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.AggregateDistinct(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.AggregateDistinct(new EdmFunction(), null));
        }

        [Fact]
        public void Null_check_All()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.All(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.All(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.All(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.All(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_Any()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Any(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Any(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Any(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Any(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Any(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_Bind()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Bind(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.BindAs(null, "Foo"));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.BindAs(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True), null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBind((null)));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBindAs(null, "Foo", "Bar"));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBindAs(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True), null, "Bar"));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBindAs(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True), "Foo", null));
        }

        [Fact]
        public void Null_check_Case()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Case(null, new[] { DbExpressionBuilder.True }, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Case(new[] { DbExpressionBuilder.True }, null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Case(new[] { DbExpressionBuilder.True }, new[] { DbExpressionBuilder.True }, null));
        }

        [Fact]
        public void Null_check_CastTo()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CastTo(null, this.booleanTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CastTo(DbExpressionBuilder.True, null));
        }

        [Fact]
        public void Null_check_Constant()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Constant(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Constant(null, 1));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Constant(this.booleanTypeUsage, null));
        }

        [Fact]
        public void Null_check_CreateRef()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (DbExpression[])null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(null, new List<DbExpression>().AsEnumerable()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (IEnumerable<DbExpression>)null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(null, (EntityType)this.categoryTypeUsage.EdmType, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (EntityType)null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (EntityType)this.categoryTypeUsage.EdmType, (DbExpression[])null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(null, (EntityType)this.categoryTypeUsage.EdmType, new List<DbExpression>().AsEnumerable()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (EntityType)null, new List<DbExpression>().AsEnumerable()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CreateRef(this.categoriesEntitySet, (EntityType)this.categoryTypeUsage.EdmType, (IEnumerable<DbExpression>)null));
        }

        [Fact]
        public void Null_check_CrossApply()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CrossApply(null, e => new KeyValuePair<string, DbExpression>()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CrossApply(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CrossApply(null, DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CrossApply(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_Filter()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Filter(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Filter(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_FullOuterJoin()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(null, DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(DbExpressionBuilder.True, null, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(DbExpressionBuilder.True, DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(null, DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.FullOuterJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_GroupBy()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBy(null, Enumerable.Empty<KeyValuePair<string, DbExpression>>(), Enumerable.Empty<KeyValuePair<string, DbAggregate>>()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBy(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).GroupBind(), null, Enumerable.Empty<KeyValuePair<string, DbAggregate>>()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GroupBy(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).GroupBind(), Enumerable.Empty<KeyValuePair<string, DbExpression>>(), null));
        }

        [Fact]
        public void Null_check_Join()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(null, DbExpressionBuilder.True, o => DbExpressionBuilder.True, i => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, null, o => DbExpressionBuilder.True, i => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, DbExpressionBuilder.True, null, i => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, DbExpressionBuilder.True, i => DbExpressionBuilder.True, null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(null, DbExpressionBuilder.True, o => DbExpressionBuilder.True, i => DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, null, o => DbExpressionBuilder.True, i => DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, DbExpressionBuilder.True, null, i => DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, DbExpressionBuilder.True, i => DbExpressionBuilder.True, null, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Join(DbExpressionBuilder.True, DbExpressionBuilder.True, o => DbExpressionBuilder.True, i => DbExpressionBuilder.True, (Func<DbExpression, DbExpression, DbExpression>)null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(null, DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(DbExpressionBuilder.True, null, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(DbExpressionBuilder.True, DbExpressionBuilder.True, null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(null, DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LeftOuterJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(null, DbExpressionBuilder.True, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(DbExpressionBuilder.True, null, (l, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(DbExpressionBuilder.True, DbExpressionBuilder.True, null));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(null, DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.InnerJoin(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_IsOf()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsOf(null, this.integerTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsOf(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsOfOnly(null, this.integerTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsOfOnly(DbExpressionBuilder.True, null));
        }

        [Fact]
        public void Null_check_Lambda()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Lambda(null, new DbVariableReferenceExpression[] { }));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Lambda(DbExpressionBuilder.True, (DbVariableReferenceExpression[])null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Lambda(null, Enumerable.Empty<DbVariableReferenceExpression>()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Lambda(DbExpressionBuilder.True, (IEnumerable<DbVariableReferenceExpression>)null));
        }

        [Fact]
        public void Null_check_New()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.New(null, new DbExpression[] { }));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.New(null, Enumerable.Empty<DbExpression>()));
        }

        [Fact]
        public void Null_check_OfType()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OfType(null, this.integerTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OfType(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OfTypeOnly(null, this.integerTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OfTypeOnly(DbExpressionBuilder.True, null));
        }

        [Fact]
        public void Null_check_OrderBy()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderBy(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderBy(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True), null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderBy(null, e => DbExpressionBuilder.True, string.Empty));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderBy(DbExpressionBuilder.True, null, string.Empty));

            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderByDescending(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderByDescending(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderByDescending(null, e => DbExpressionBuilder.True, string.Empty));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OrderByDescending(DbExpressionBuilder.True, null, string.Empty));
        }

        [Fact]
        public void Null_check_OuterApply()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OuterApply(null, e => new KeyValuePair<string, DbExpression>()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OuterApply(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OuterApply(null, DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind()));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.OuterApply(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_Parameter()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Parameter(null, string.Empty));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Parameter(this.stringTypeUsage, null));
        }

        [Fact]
        public void Null_check_Project()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Project(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Project(DbExpressionBuilder.NewCollection(DbExpressionBuilder.True).Bind(), null));
        }

        [Fact]
        public void Null_check_Property()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(null, new EdmProperty("Foo")));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(DbExpressionBuilder.True, (EdmProperty)null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(null, new NavigationProperty("Foo", this.productTypeUsage)));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(DbExpressionBuilder.True, (NavigationProperty)null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(null, "Foo"));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Property(DbExpressionBuilder.True, (string)null));
        }

        [Fact]
        public void Null_check_RefFromKey()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.RefFromKey(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.RefFromKey(this.productsEntitySet, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.RefFromKey(null, DbExpressionBuilder.True, (EntityType)this.productTypeUsage.EdmType));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.RefFromKey(this.productsEntitySet, null, (EntityType)this.productTypeUsage.EdmType));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.RefFromKey(this.productsEntitySet, DbExpressionBuilder.True, null));
        }

        [Fact]
        public void Null_check_Select()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Select(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Select(DbExpressionBuilder.True, (Func<DbExpression, DbExpression>)null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.SelectMany(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.SelectMany(DbExpressionBuilder.True, (Func<DbExpression, DbExpression>)null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.SelectMany(null, e => DbExpressionBuilder.True, (e, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.SelectMany(DbExpressionBuilder.True, null, (e, r) => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.SelectMany(DbExpressionBuilder.True, e => DbExpressionBuilder.True, (Func<DbExpression, DbExpression, DbExpression>)null));
        }

        [Fact]
        public void Null_check_TreatAs()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.TreatAs(null, this.integerTypeUsage));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.TreatAs(DbExpressionBuilder.True, null));
        }

        [Fact]
        public void Null_check_Variable()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Variable(null, string.Empty));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Variable(this.stringTypeUsage, null));
        }

        [Fact]
        public void Null_check_Where()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Where(null, e => DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Where(DbExpressionBuilder.True, (Func<DbExpression, DbExpression>)null));
        }

        [Fact]
        public void Null_check_one_argument()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.CrossJoin(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Deref(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Distinct(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Element(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Exists(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GetEntityRef(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GetRefKey(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsEmpty(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.IsNull(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Negate(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.NewCollection(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.NewEmptyCollection(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.NewRow(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Not(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Null(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Scan(null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.UnaryMinus(null));
        }

        [Fact]
        public void Null_check_two_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.And(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.And(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Or(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Or(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Equal(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Equal(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.NotEqual(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.NotEqual(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LessThan(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LessThan(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LessThanOrEqual(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.LessThanOrEqual(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GreaterThan(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GreaterThan(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GreaterThanOrEqual(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.GreaterThanOrEqual(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Plus(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Plus(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Minus(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Minus(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Multiply(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Multiply(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Divide(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Divide(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Modulo(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Modulo(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Except(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Except(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Intersect(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Intersect(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Union(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Union(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.UnionAll(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.UnionAll(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Like(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Like(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Limit(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Limit(DbExpressionBuilder.True, null));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Take(null, DbExpressionBuilder.True));
            Assert.Throws<ArgumentNullException>(() => DbExpressionBuilder.Take(DbExpressionBuilder.True, null));
        }

        #endregion

        public void SetFixture(DbExpressionBuilderFixture data)
        {
            this.workspace = data.Workspace;
            this.stringTypeUsage = data.StringTypeUsage;
            this.booleanTypeUsage = data.BooleanTypeUsage;
            this.integerTypeUsage = data.IntegerTypeUsage;
            this.geographyTypeUsage = data.GeographyTypeUsage;
            this.geometryTypeUsage = data.GeometryTypeUsage;
            this.enumTypeUsage = data.EnumTypeUsage;
            this.enumTypeUsage2 = data.EnumTypeUsage2;
            this.productTypeUsage = data.ProductTypeUsage;
            this.discontinuedProductTypeUsage = data.DiscontinuedProductTypeUsage;
            this.categoryTypeUsage = data.CategoryTypeUsage;
            this.productsEntitySet = data.ProductsEntitySet;
            this.categoriesEntitySet = data.CategoriesEntitySet;
        }
    }

    public class DbExpressionBuilderFixture
    {
        private static string csdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""MyModel"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
  <EntityContainer Name=""MyContainer"">
    <EntitySet Name=""Products"" EntityType=""Self.Product"" />
    <EntitySet Name=""Categories"" EntityType=""Self.Category"" />
    <EntitySet Name=""Deck"" EntityType=""Self.Card"" />
    <AssociationSet Name=""CategoryProducts"" Association=""Self.CategoryProduct"">
      <End Role=""Category"" EntitySet=""Categories"" />
      <End Role=""Product"" EntitySet=""Products"" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name=""Product"">
    <Key>
      <PropertyRef Name=""ProductID"" />
    </Key>
    <Property Name=""ProductID"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""ProductName"" Type=""String"" MaxLength=""40"" />
    <NavigationProperty Name=""Category"" Relationship=""Self.CategoryProduct"" FromRole=""Product"" ToRole=""Category"" />
  </EntityType>
  <EntityType Name=""Category"">
    <Key>
      <PropertyRef Name=""CategoryID"" />
    </Key>
    <Property Name=""CategoryID"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""CategoryName"" Type=""String"" MaxLength=""15"" />
    <NavigationProperty Name=""Products"" Relationship=""Self.CategoryProduct"" FromRole=""Category"" ToRole=""Product"" />
  </EntityType>
  <EntityType Name=""DiscontinuedProduct"" BaseType=""Self.Product"" />
  <EntityType Name=""Card"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
    <Property Name=""Suite"" Type=""Self.CardSuite"" Nullable=""false""/>
    <Property Name=""Number"" Type=""Byte"" Nullable=""false""/>
  </EntityType>
  <EnumType Name=""CardSuite"" UnderlyingType=""Byte"">
    <Member Name=""Clubs"" Value=""0""/>
    <Member Name=""Diamonds"" Value=""1""/>
    <Member Name=""Hearts"" Value=""2""/>
    <Member Name=""Spades"" Value=""3""/>
  </EnumType>
  <Association Name=""CategoryProduct"">
    <End Role=""Category"" Type=""Self.Category"" Multiplicity=""1"" />
    <End Role=""Product"" Type=""Self.Product"" Multiplicity=""*"" />
  </Association>
</Schema>";

        public MetadataWorkspace Workspace { get; private set; }
        public TypeUsage StringTypeUsage { get; private set; }
        public TypeUsage BooleanTypeUsage { get; private set; }
        public TypeUsage IntegerTypeUsage { get; private set; }
        public TypeUsage GeographyTypeUsage { get; private set; }
        public TypeUsage GeometryTypeUsage { get; private set; }
        public TypeUsage EnumTypeUsage { get; private set; }
        public TypeUsage EnumTypeUsage2 { get; private set; }
        public TypeUsage ProductTypeUsage { get; private set; }
        public TypeUsage DiscontinuedProductTypeUsage { get; private set; }
        public TypeUsage CategoryTypeUsage { get; private set; }
        public EntitySet ProductsEntitySet { get; private set; }
        public EntitySet CategoriesEntitySet { get; private set; }

        public DbExpressionBuilderFixture()
        {
            this.Workspace = CreateMetadataWorkspace();
         
            var primitiveStringType = this.Workspace.GetMappedPrimitiveType(PrimitiveTypeKind.String, DataSpace.CSpace);
            this.StringTypeUsage = TypeUsage.CreateStringTypeUsage(primitiveStringType, true, false);

            var primitiveBoolType = this.Workspace.GetMappedPrimitiveType(PrimitiveTypeKind.Boolean, DataSpace.CSpace);
            this.BooleanTypeUsage = TypeUsage.Create(primitiveBoolType);

            var primitiveIntegerType = this.Workspace.GetMappedPrimitiveType(PrimitiveTypeKind.Int32, DataSpace.CSpace);
            this.IntegerTypeUsage = TypeUsage.Create(primitiveIntegerType);

            var primitiveGeographyType = this.Workspace.GetMappedPrimitiveType(PrimitiveTypeKind.Geography, DataSpace.CSpace);
            this.GeographyTypeUsage = TypeUsage.Create(primitiveGeographyType);

            var primitiveGeometryType = this.Workspace.GetMappedPrimitiveType(PrimitiveTypeKind.Geometry, DataSpace.CSpace);
            this.GeometryTypeUsage = TypeUsage.Create(primitiveGeometryType);

            var enumType = this.Workspace.GetType("CardSuite", "MyModel", DataSpace.CSpace);
            this.EnumTypeUsage = TypeUsage.Create(enumType);

            var productType = this.Workspace.GetType("Product", "MyModel", DataSpace.CSpace);
            this.ProductTypeUsage = TypeUsage.Create(productType);

            var discontinuedProductType = this.Workspace.GetType("DiscontinuedProduct", "MyModel", DataSpace.CSpace);
            this.DiscontinuedProductTypeUsage = TypeUsage.Create(discontinuedProductType);

            var categoryType = this.Workspace.GetType("Category", "MyModel", DataSpace.CSpace);
            this.CategoryTypeUsage = TypeUsage.Create(categoryType);

            var container = this.Workspace.GetEntityContainer("MyContainer", DataSpace.CSpace);
            this.ProductsEntitySet = container.GetEntitySetByName("Products", false);
            this.CategoriesEntitySet = container.GetEntitySetByName("Categories", false);

            string enumCsdl2 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""Foo"">
  <EnumType Name=""CardSuite"" UnderlyingType=""Byte"">
    <Member Name=""Clubs"" Value=""0""/>
    <Member Name=""Diamonds"" Value=""1""/>
    <Member Name=""Hearts"" Value=""2""/>
    <Member Name=""Spades"" Value=""3""/>
  </EnumType>
</Schema>";

            var edmCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumCsdl2)) });
            this.EnumTypeUsage2 = TypeUsage.Create(edmCollection.GetType("CardSuite", "Foo"));

        }

        private static MetadataWorkspace CreateMetadataWorkspace()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) });
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>
            {
                CallBase = true
            };

            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSpace, It.IsAny<bool>())).Returns(edmItemCollection);
            
            return metadataWorkspaceMock.Object;
        }
    }

    public enum CardSuite : byte { Clubs, Diamonds, Hearts, Spades };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.MembersWithSameValues
{
    public enum CardSuite : byte { Clubs = 1, Spades = 1, Diamonds = 1, Hearts = 1 };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.MissingMember
{
    public enum CardSuite : byte { Clubs, Diamonds, Hearts };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.AdditionalMember
{
    public enum CardSuite : byte { Clubs, Diamonds, Hearts, Spades, Trump };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.DifferentUnderlyingTypes
{
    public enum CardSuite : int { Clubs, Diamonds, Hearts, Spades };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.NonEdmCompatibleUnderlyingType
{
    public enum CardSuite : ulong { Clubs, Diamonds, Hearts, Spades };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.NonExistingMember
{
    public enum CardSuite : byte { Clubs, Diamonds, Hearts, Trump };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.DifferentMemberValue
{
    public enum CardSuite : byte { Clubs, Diamonds, Hearts, Spades = 15 };
}

namespace System.Data.Entity.Core.Common.CommandTrees.Enums.SwapedMembersValues
{
    public enum CardSuite : byte { Clubs, Spades, Diamonds, Hearts };
}
