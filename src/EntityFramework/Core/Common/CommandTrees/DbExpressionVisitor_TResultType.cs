// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Resources;

    /// <summary>Defines the basic functionality that should be implemented by visitors that return a result value of a specific type. </summary>
    /// <typeparam name="TResultType">The type of the result produced by the visitor.</typeparam>
    public abstract class DbExpressionVisitor<TResultType>
    {
        /// <summary>When overridden in a derived class, handles any expression of an unrecognized type.</summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAndExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAndExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbAndExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbApplyExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbApplyExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbApplyExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbArithmeticExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbArithmeticExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbArithmeticExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbCaseExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCastExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCastExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbCastExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbComparisonExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbComparisonExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbComparisonExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbConstantExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCrossJoinExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCrossJoinExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbCrossJoinExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDerefExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDerefExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbDerefExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDistinctExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDistinctExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbDistinctExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbElementExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbElementExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbElementExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExceptExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExceptExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbExceptExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFilterExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFilterExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbFilterExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbFunctionExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbEntityRefExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbEntityRefExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbEntityRefExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefKeyExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefKeyExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbRefKeyExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupByExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupByExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbGroupByExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIntersectExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIntersectExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbIntersectExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsEmptyExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsEmptyExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbIsEmptyExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsNullExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsNullExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbIsNullExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsOfExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsOfExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbIsOfExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbJoinExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbJoinExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbJoinExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern method for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambdaExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambdaExpression" /> that is being visited.
        /// </param>
        public virtual TResultType Visit(DbLambdaExpression expression)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbLikeExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbLimitExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNewInstanceExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNewInstanceExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbNewInstanceExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNotExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNotExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbNotExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNullExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNullExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbNullExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOfTypeExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOfTypeExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbOfTypeExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOrExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOrExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbOrExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbParameterReferenceExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbProjectExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbProjectExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbProjectExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbPropertyExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQuantifierExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQuantifierExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbQuantifierExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbRefExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbRelationshipNavigationExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbScanExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbScanExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbScanExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbSortExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbSkipExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbTreatExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbTreatExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbTreatExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbUnionAllExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbUnionAllExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbUnionAllExpression expression);

        /// <summary>
        ///     When overridden in a derived class, implements the visitor pattern for
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" />
        ///     .
        /// </summary>
        /// <returns>A result value of a specific type.</returns>
        /// <param name="expression">
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" /> that is being visited.
        /// </param>
        public abstract TResultType Visit(DbVariableReferenceExpression expression);

        /// <summary>
        ///     Typed visitor pattern method for DbInExpression.
        /// </summary>
        /// <param name="expression"> The DbInExpression that is being visited. </param>
        /// <returns> An instance of TResultType. </returns>
        public virtual TResultType Visit(DbInExpression expression)
        {
            throw new NotImplementedException(Strings.VisitDbInExpressionNotImplemented);
        }
    }
}
