// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees
{
    using Xunit;

    public class DbExpressionRebinderTests
    {
        [Fact]
        public void Vistor_methods_validate_expression_is_non_null()
        {
            TestVisitNull(v => v.Visit((DbExpression)null));
            TestVisitNull(v => v.Visit((DbAndExpression)null));
            TestVisitNull(v => v.Visit((DbApplyExpression)null));
            TestVisitNull(v => v.Visit((DbArithmeticExpression)null));
            TestVisitNull(v => v.Visit((DbCaseExpression)null));
            TestVisitNull(v => v.Visit((DbCastExpression)null));
            TestVisitNull(v => v.Visit((DbComparisonExpression)null));
            TestVisitNull(v => v.Visit((DbConstantExpression)null));
            TestVisitNull(v => v.Visit((DbCrossJoinExpression)null));
            TestVisitNull(v => v.Visit((DbDerefExpression)null));
            TestVisitNull(v => v.Visit((DbDistinctExpression)null));
            TestVisitNull(v => v.Visit((DbElementExpression)null));
            TestVisitNull(v => v.Visit((DbExceptExpression)null));
            TestVisitNull(v => v.Visit((DbFilterExpression)null));
            TestVisitNull(v => v.Visit((DbFunctionExpression)null));
            TestVisitNull(v => v.Visit((DbEntityRefExpression)null));
            TestVisitNull(v => v.Visit((DbRefKeyExpression)null));
            TestVisitNull(v => v.Visit((DbGroupByExpression)null));
            TestVisitNull(v => v.Visit((DbIntersectExpression)null));
            TestVisitNull(v => v.Visit((DbIsEmptyExpression)null));
            TestVisitNull(v => v.Visit((DbIsNullExpression)null));
            TestVisitNull(v => v.Visit((DbIsOfExpression)null));
            TestVisitNull(v => v.Visit((DbJoinExpression)null));
            TestVisitNull(v => v.Visit((DbLikeExpression)null));
            TestVisitNull(v => v.Visit((DbLimitExpression)null));
            TestVisitNull(v => v.Visit((DbNewInstanceExpression)null));
            TestVisitNull(v => v.Visit((DbNotExpression)null));
            TestVisitNull(v => v.Visit((DbNullExpression)null));
            TestVisitNull(v => v.Visit((DbOfTypeExpression)null));
            TestVisitNull(v => v.Visit((DbOrExpression)null));
            TestVisitNull(v => v.Visit((DbParameterReferenceExpression)null));
            TestVisitNull(v => v.Visit((DbProjectExpression)null));
            TestVisitNull(v => v.Visit((DbPropertyExpression)null));
            TestVisitNull(v => v.Visit((DbQuantifierExpression)null));
            TestVisitNull(v => v.Visit((DbRefExpression)null));
            TestVisitNull(v => v.Visit((DbRelationshipNavigationExpression)null));
            TestVisitNull(v => v.Visit((DbScanExpression)null));
            TestVisitNull(v => v.Visit((DbSkipExpression)null));
            TestVisitNull(v => v.Visit((DbSortExpression)null));
            TestVisitNull(v => v.Visit((DbTreatExpression)null));
            TestVisitNull(v => v.Visit((DbUnionAllExpression)null));
            TestVisitNull(v => v.Visit((DbVariableReferenceExpression)null));
        }

        public class ConcreteExpressionVisitor : DbExpressionRebinder
        {
        }

        private static void TestVisitNull(Action<DbExpressionRebinder> test)
        {
            Assert.Equal("expression", Assert.Throws<ArgumentNullException>(() => test(new ConcreteExpressionVisitor())).ParamName);
        }
    }
}