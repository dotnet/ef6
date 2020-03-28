// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{    
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using Xunit;

    public class DbExpressionVisiorTests
    {
        [Fact]
        public static void VisitDbInExpression_throws_not_implemented_exception()
        {
            var constant = DbExpressionBuilder.Constant(3);
            var list = new List<DbConstantExpression>() {
                DbExpressionBuilder.Constant(0),
                DbExpressionBuilder.Constant(1),
                DbExpressionBuilder.Constant(2),
            };
            var expression = constant.In(list) as DbInExpression;

            var visitor1 = new DummyDbExpressionVisitor();
            Assert.Throws<NotImplementedException>(() => visitor1.Visit(expression));

            var visitor2 = new DummyDbExpressionVisitor<string>();
            Assert.Throws<NotImplementedException>(() => visitor2.Visit(expression));
        }

        internal class DummyDbExpressionVisitor : DbExpressionVisitor
        {
            public override void Visit(DbExpression expression) { }
            public override void Visit(DbAndExpression expression) { }
            public override void Visit(DbApplyExpression expression) { }
            public override void Visit(DbArithmeticExpression expression) { }
            public override void Visit(DbCaseExpression expression) { }
            public override void Visit(DbCastExpression expression) { }
            public override void Visit(DbComparisonExpression expression) { }
            public override void Visit(DbConstantExpression expression) { }
            public override void Visit(DbCrossJoinExpression expression) { }
            public override void Visit(DbDerefExpression expression) { }
            public override void Visit(DbDistinctExpression expression) { }
            public override void Visit(DbElementExpression expression) { }
            public override void Visit(DbExceptExpression expression) { }
            public override void Visit(DbFilterExpression expression) { }
            public override void Visit(DbFunctionExpression expression) { }
            public override void Visit(DbEntityRefExpression expression) { }
            public override void Visit(DbRefKeyExpression expression) { }
            public override void Visit(DbGroupByExpression expression) { }
            public override void Visit(DbIntersectExpression expression) { }
            public override void Visit(DbIsEmptyExpression expression) { }
            public override void Visit(DbIsNullExpression expression) { }
            public override void Visit(DbIsOfExpression expression) { }
            public override void Visit(DbJoinExpression expression) { }
            public override void Visit(DbLikeExpression expression) { }
            public override void Visit(DbLimitExpression expression) { }
            public override void Visit(DbNewInstanceExpression expression) { }
            public override void Visit(DbNotExpression expression) { }
            public override void Visit(DbNullExpression expression) { }
            public override void Visit(DbOfTypeExpression expression) { }
            public override void Visit(DbOrExpression expression) { }
            public override void Visit(DbParameterReferenceExpression expression) { }
            public override void Visit(DbProjectExpression expression) { }
            public override void Visit(DbPropertyExpression expression) { }
            public override void Visit(DbQuantifierExpression expression) { }
            public override void Visit(DbRefExpression expression) { }
            public override void Visit(DbRelationshipNavigationExpression expression) { }
            public override void Visit(DbScanExpression expression) { }
            public override void Visit(DbSkipExpression expression) { }
            public override void Visit(DbSortExpression expression) { }
            public override void Visit(DbTreatExpression expression) { }
            public override void Visit(DbUnionAllExpression expression) { }
            public override void Visit(DbVariableReferenceExpression expression) { }
        }

        internal class DummyDbExpressionVisitor<T> : DbExpressionVisitor<T>
        {
            public override T Visit(DbExpression expression) { return default(T); }
            public override T Visit(DbAndExpression expression) { return default(T); }
            public override T Visit(DbApplyExpression expression) { return default(T); }
            public override T Visit(DbArithmeticExpression expression) { return default(T); }
            public override T Visit(DbCaseExpression expression) { return default(T); }
            public override T Visit(DbCastExpression expression) { return default(T); }
            public override T Visit(DbComparisonExpression expression) { return default(T); }
            public override T Visit(DbConstantExpression expression) { return default(T); }
            public override T Visit(DbCrossJoinExpression expression) { return default(T); }
            public override T Visit(DbDerefExpression expression) { return default(T); }
            public override T Visit(DbDistinctExpression expression) { return default(T); }
            public override T Visit(DbElementExpression expression) { return default(T); }
            public override T Visit(DbExceptExpression expression) { return default(T); }
            public override T Visit(DbFilterExpression expression) { return default(T); }
            public override T Visit(DbFunctionExpression expression) { return default(T); }
            public override T Visit(DbEntityRefExpression expression) { return default(T); }
            public override T Visit(DbRefKeyExpression expression) { return default(T); }
            public override T Visit(DbGroupByExpression expression) { return default(T); }
            public override T Visit(DbIntersectExpression expression) { return default(T); }
            public override T Visit(DbIsEmptyExpression expression) { return default(T); }
            public override T Visit(DbIsNullExpression expression) { return default(T); }
            public override T Visit(DbIsOfExpression expression) { return default(T); }
            public override T Visit(DbJoinExpression expression) { return default(T); }
            public override T Visit(DbLikeExpression expression) { return default(T); }
            public override T Visit(DbLimitExpression expression) { return default(T); }
            public override T Visit(DbNewInstanceExpression expression) { return default(T); }
            public override T Visit(DbNotExpression expression) { return default(T); }
            public override T Visit(DbNullExpression expression) { return default(T); }
            public override T Visit(DbOfTypeExpression expression) { return default(T); }
            public override T Visit(DbOrExpression expression) { return default(T); }
            public override T Visit(DbParameterReferenceExpression expression) { return default(T); }
            public override T Visit(DbProjectExpression expression) { return default(T); }
            public override T Visit(DbPropertyExpression expression) { return default(T); }
            public override T Visit(DbQuantifierExpression expression) { return default(T); }
            public override T Visit(DbRefExpression expression) { return default(T); }
            public override T Visit(DbRelationshipNavigationExpression expression) { return default(T); }
            public override T Visit(DbScanExpression expression) { return default(T); }
            public override T Visit(DbSkipExpression expression) { return default(T); }
            public override T Visit(DbSortExpression expression) { return default(T); }
            public override T Visit(DbTreatExpression expression) { return default(T); }
            public override T Visit(DbUnionAllExpression expression) { return default(T); }
            public override T Visit(DbVariableReferenceExpression expression) { return default(T); }
        }
    }
}
