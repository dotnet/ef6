// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;

    /// <summary>
    ///     The Sql8ConformanceChecker walks a DbExpression tree and determines whether
    ///     it should be rewritten in order to be translated to SQL appropriate for SQL Server 2000.
    ///     The tree should be rewritten if it contains any of the following expressions:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="DbExceptExpression" />
    ///         </item>
    ///         <item>
    ///             <see cref="DbIntersectExpression" />
    ///         </item>
    ///         <item>
    ///             <see cref="DbSkipExpression" />
    ///         </item>
    ///     </list>
    ///     Also, it throws if it determines that the tree can not
    ///     be translated into SQL appropriate for SQL Server 2000.
    ///     This happens if:
    ///     <list type="bullet">
    ///         <item>
    ///             The tree contains
    ///             <see cref="DbApplyExpression" />
    ///         </item>
    ///         <item>
    ///             The tree contains
    ///             <see cref="DbLimitExpression" />
    ///             with property Limit of type
    ///             <see cref="DbParameterReferenceExpression" />
    ///         </item>
    ///         <item>
    ///             The tree contains
    ///             <see cref="DbSkipExpression" />
    ///             with property Count of type
    ///             <see cref="DbParameterReferenceExpression" />
    ///         </item>
    ///     </list>
    ///     The visitor only checks for expressions for which the support differs between SQL Server 2000 and SQL Server 2005,
    ///     but does not check/throw for expressions that are not supported for both providers.
    ///     Implementation note: In the cases when the visitor encounters an expression that requires rewrite,
    ///     it still needs to walk its structure in case something below it is not supported and needs to throw.
    /// </summary>
    internal class Sql8ConformanceChecker : DbExpressionVisitor<bool>
    {
        #region 'Public' API

        /// <summary>
        ///     The entry point
        /// </summary>
        /// <param name="expr"> </param>
        /// <returns> True if the tree needs to be rewriten, false otherwise </returns>
        internal static bool NeedsRewrite(DbExpression expr)
        {
            var checker = new Sql8ConformanceChecker();
            return expr.Accept(checker);
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Default Constructor
        /// </summary>
        private Sql8ConformanceChecker()
        {
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        ///     Default handling for DbUnaryExpression-derived classes. Simply visits its argument
        /// </summary>
        /// <param name="expr"> The DbUnaryExpression to visit </param>
        /// <returns> </returns>
        private bool VisitUnaryExpression(DbUnaryExpression expr)
        {
            return VisitExpression(expr.Argument);
        }

        /// <summary>
        ///     Default handling for DbBinaryExpression-derived classes. Visits both arguments.
        /// </summary>
        /// <param name="expr"> The DbBinaryExpression to visit </param>
        /// <returns> </returns>
        private bool VisitBinaryExpression(DbBinaryExpression expr)
        {
            var leftNeedsRewrite = VisitExpression(expr.Left);
            var rightNeedsRewrite = VisitExpression(expr.Right);
            return leftNeedsRewrite || rightNeedsRewrite;
        }

        /// <summary>
        ///     Used for <see cref="VisitList" />
        /// </summary>
        /// <typeparam name="TElementType"> </typeparam>
        /// <param name="element"> </param>
        /// <returns> </returns>
        private delegate bool ListElementHandler<TElementType>(TElementType element);

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="aggregate"> </param>
        /// <returns> </returns>
        private bool VisitAggregate(DbAggregate aggregate)
        {
            return VisitExpressionList(aggregate.Arguments);
        }

        /// <summary>
        ///     DbExpressionBinding handler
        /// </summary>
        /// <param name="expressionBinding"> </param>
        /// <returns> </returns>
        private bool VisitExpressionBinding(DbExpressionBinding expressionBinding)
        {
            return VisitExpression(expressionBinding.Expression);
        }

        /// <summary>
        ///     Used as handler for expressions
        /// </summary>
        /// <param name="expression"> </param>
        /// <returns> </returns>
        private bool VisitExpression(DbExpression expression)
        {
            if (expression == null)
            {
                return false;
            }
            return expression.Accept(this);
        }

        /// <summary>
        ///     Used as handler for SortClauses
        /// </summary>
        /// <param name="sortClause"> </param>
        /// <returns> </returns>
        private bool VisitSortClause(DbSortClause sortClause)
        {
            return VisitExpression(sortClause.Expression);
        }

        /// <summary>
        ///     Helper method for iterating a list
        /// </summary>
        /// <typeparam name="TElementType"> </typeparam>
        /// <param name="handler"> </param>
        /// <param name="list"> </param>
        /// <returns> </returns>
        private static bool VisitList<TElementType>(ListElementHandler<TElementType> handler, IList<TElementType> list)
        {
            var result = false;

            foreach (var element in list)
            {
                var localResult = handler(element);
                result = result || localResult;
            }
            return result;
        }

        /// <summary>
        ///     Handing for list of <see cref="DbExpressionBinding" />s.
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        private bool VisitAggregateList(IList<DbAggregate> list)
        {
            return VisitList(VisitAggregate, list);
        }

        /// <summary>
        ///     Handing for list of <see cref="DbExpressionBinding" />s.
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        private bool VisitExpressionBindingList(IList<DbExpressionBinding> list)
        {
            return VisitList(VisitExpressionBinding, list);
        }

        /// <summary>
        ///     Handing for list of <see cref="DbExpression" />s.
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        private bool VisitExpressionList(IList<DbExpression> list)
        {
            return VisitList(VisitExpression, list);
        }

        /// <summary>
        ///     Handling for list of <see cref="DbSortClause" />s.
        /// </summary>
        /// <param name="list"> </param>
        /// <returns> </returns>
        private bool VisitSortClauseList(IList<DbSortClause> list)
        {
            return VisitList(VisitSortClause, list);
        }

        #endregion

        #region DbExpressionVisitor Members

        /// <summary>
        ///     Called when an <see cref="DbExpression" /> of an otherwise unrecognized type is encountered.
        /// </summary>
        /// <param name="expression"> The expression </param>
        /// <returns> </returns>
        /// <exception cref="NotSupportedException">
        ///     Always thrown if this method is called, since it indicates that
        ///     <paramref name="expression" />
        ///     is of an unsupported type
        /// </exception>
        public override bool Visit(DbExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(expression.GetType().FullName));
        }

        /// <summary>
        ///     <see cref="VisitBinaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbAndExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbAndExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinaryExpression(expression);
        }

        /// <summary>
        ///     Not supported on SQL Server 2000.
        /// </summary>
        /// <param name="expression"> The DbApplyExpression that is being visited. </param>
        /// <exception cref="NotSupportedException">Always</exception>
        public override bool Visit(DbApplyExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw new NotSupportedException(Strings.SqlGen_ApplyNotSupportedOnSql8);
        }

        /// <summary>
        ///     Default handling for DbArithmeticExpression. Visits all arguments.
        /// </summary>
        /// <param name="expression"> The DbArithmeticExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbArithmeticExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpressionList(expression.Arguments);
        }

        /// <summary>
        ///     Walks the strucutre
        /// </summary>
        /// <param name="expression"> The DbCaseExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbCaseExpression expression)
        {
            Check.NotNull(expression, "expression");

            var whenNeedsRewrite = VisitExpressionList(expression.When);
            var thenNeedsRewrite = VisitExpressionList(expression.Then);
            var elseNeedsRewrite = VisitExpression(expression.Else);
            return whenNeedsRewrite || thenNeedsRewrite || elseNeedsRewrite;
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbCastExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbCastExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitBinaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbComparisonExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbComparisonExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinaryExpression(expression);
        }

        /// <summary>
        ///     Returns false
        /// </summary>
        /// <param name="expression"> The DbConstantExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbConstantExpression expression)
        {
            Check.NotNull(expression, "expression");

            return false;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbCrossJoinExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbCrossJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpressionBindingList(expression.Inputs);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DeRefExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbDerefExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbDistinctExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbDistinctExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbElementExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbElementExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbEntityRefExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbEntityRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     Returns true, the tree needs to be rewritten.
        /// </summary>
        /// <param name="expression"> The DbExceptExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbExceptExpression expression)
        {
            Check.NotNull(expression, "expression");

            //Walk the structure in case a non-supported construct is encountered 
            VisitExpression(expression.Left);
            VisitExpression(expression.Right);
            return true;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbFilterExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbFilterExpression expression)
        {
            Check.NotNull(expression, "expression");

            var inputNeedsRewrite = VisitExpressionBinding(expression.Input);
            var predicateNeedsRewrite = VisitExpression(expression.Predicate);
            return inputNeedsRewrite || predicateNeedsRewrite;
        }

        /// <summary>
        ///     Visits the arguments
        /// </summary>
        /// <param name="expression"> The DbFunctionExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbFunctionExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpressionList(expression.Arguments);
        }

        /// <summary>
        ///     Visits the arguments and lambda body
        /// </summary>
        /// <param name="expression"> The DbLambdaExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbLambdaExpression expression)
        {
            Check.NotNull(expression, "expression");

            var argumentsNeedRewrite = VisitExpressionList(expression.Arguments);
            var bodyNeedsRewrite = VisitExpression(expression.Lambda.Body);

            return argumentsNeedRewrite || bodyNeedsRewrite;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbGroupByExpression expression)
        {
            Check.NotNull(expression, "expression");

            var inputNeedsRewrite = VisitExpression(expression.Input.Expression);
            var keysNeedRewrite = VisitExpressionList(expression.Keys);
            var aggregatesNeedRewrite = VisitAggregateList(expression.Aggregates);

            return inputNeedsRewrite || keysNeedRewrite || aggregatesNeedRewrite;
        }

        /// <summary>
        ///     Returns true.
        /// </summary>
        /// <param name="expression"> The DbIntersectExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbIntersectExpression expression)
        {
            Check.NotNull(expression, "expression");

            //Walk the structure in case a non-supported construct is encountered 
            VisitExpression(expression.Left);
            VisitExpression(expression.Right);
            return true;
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbIsEmptyExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbIsEmptyExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbIsNullExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbIsNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbIsOfExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbIsOfExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbJoinExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            var leftNeedsRewrite = VisitExpressionBinding(expression.Left);
            var rightNeedsRewrite = VisitExpressionBinding(expression.Right);
            var conditionNeedsRewrite = VisitExpression(expression.JoinCondition);
            return leftNeedsRewrite || rightNeedsRewrite || conditionNeedsRewrite;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbLikeExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbLikeExpression expression)
        {
            Check.NotNull(expression, "expression");

            var argumentNeedsRewrite = VisitExpression(expression.Argument);
            var patternNeedsRewrite = VisitExpression(expression.Pattern);
            var excapeNeedsRewrite = VisitExpression(expression.Escape);
            return argumentNeedsRewrite || patternNeedsRewrite || excapeNeedsRewrite;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> </param>
        /// <returns> </returns>
        /// <exception cref="NotSupportedException">expression.Limit is DbParameterReferenceExpression</exception>
        public override bool Visit(DbLimitExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Limit is DbParameterReferenceExpression)
            {
                throw new NotSupportedException(Strings.SqlGen_ParameterForLimitNotSupportedOnSql8);
            }

            return VisitExpression(expression.Argument);
        }

        /// <summary>
        ///     Walks the arguments
        /// </summary>
        /// <param name="expression"> The DbNewInstanceExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbNewInstanceExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpressionList(expression.Arguments);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbNotExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbNotExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     Returns false
        /// </summary>
        /// <param name="expression"> The DbNullExpression that is being visited. </param>
        /// <returns> false </returns>
        public override bool Visit(DbNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            return false;
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbOfTypeExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbOfTypeExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitBinaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbOrExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbOrExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinaryExpression(expression);
        }

        /// <summary>
        ///     Returns false
        /// </summary>
        /// <param name="expression"> The DbParameterReferenceExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbParameterReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            return false;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbProjectExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbProjectExpression expression)
        {
            Check.NotNull(expression, "expression");

            var inputNeedsRewrite = VisitExpressionBinding(expression.Input);
            var projectionNeedsRewrite = VisitExpression(expression.Projection);
            return inputNeedsRewrite || projectionNeedsRewrite;
        }

        /// <summary>
        ///     Returns false
        /// </summary>
        /// <param name="expression"> The DbPropertyExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbPropertyExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpression(expression.Instance);
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbQuantifierExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbQuantifierExpression expression)
        {
            Check.NotNull(expression, "expression");

            var inputNeedsRewrite = VisitExpressionBinding(expression.Input);
            var predicateNeedsRewrite = VisitExpression(expression.Predicate);
            return inputNeedsRewrite || predicateNeedsRewrite;
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbRefExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbRefKeyExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbRefKeyExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbRelationshipNavigationExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbRelationshipNavigationExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitExpression(expression.NavigationSource);
        }

        /// <summary>
        ///     Returns false;
        /// </summary>
        /// <param name="expression"> The DbScanExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbScanExpression expression)
        {
            Check.NotNull(expression, "expression");

            return false;
        }

        /// <summary>
        ///     Resturns true
        /// </summary>
        /// <param name="expression"> </param>
        /// <returns> </returns>
        /// <exception cref="NotSupportedException">expression.Count is DbParameterReferenceExpression</exception>
        public override bool Visit(DbSkipExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Count is DbParameterReferenceExpression)
            {
                throw new NotSupportedException(Strings.SqlGen_ParameterForSkipNotSupportedOnSql8);
            }

            //Walk the structure in case a non-supported construct is encountered 
            VisitExpressionBinding(expression.Input);
            VisitSortClauseList(expression.SortOrder);
            VisitExpression(expression.Count);

            return true;
        }

        /// <summary>
        ///     Walks the structure
        /// </summary>
        /// <param name="expression"> The DbSortExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbSortExpression expression)
        {
            Check.NotNull(expression, "expression");

            var inputNeedsRewrite = VisitExpressionBinding(expression.Input);
            var sortClauseNeedsRewrite = VisitSortClauseList(expression.SortOrder);
            return inputNeedsRewrite || sortClauseNeedsRewrite;
        }

        /// <summary>
        ///     <see cref="VisitUnaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbTreatExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbTreatExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnaryExpression(expression);
        }

        /// <summary>
        ///     <see cref="VisitBinaryExpression" />
        /// </summary>
        /// <param name="expression"> The DbUnionAllExpression that is being visited. </param>
        /// <returns> </returns>
        public override bool Visit(DbUnionAllExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinaryExpression(expression);
        }

        /// <summary>
        ///     Returns false
        /// </summary>
        /// <param name="expression"> The DbVariableReferenceExpression that is being visited. </param>
        /// <returns> false </returns>
        public override bool Visit(DbVariableReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            return false;
        }

        #endregion
    }
}
