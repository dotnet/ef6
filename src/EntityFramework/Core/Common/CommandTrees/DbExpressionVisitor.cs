// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Resources;

    /// <summary>Defines the basic functionality that should be implemented by visitors that do not return a result value.</summary>
    public abstract class DbExpressionVisitor
    {
        /// <summary>When overridden in a derived class, handles any expression of an unrecognized type.</summary>
        /// <param name="expression">The expression to be handled.</param>
        public abstract void Visit(DbExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAndExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAndExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbAndExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbApplyExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbApplyExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbApplyExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbArithmeticExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbArithmeticExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbArithmeticExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCaseExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbCaseExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCastExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCastExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbCastExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbComparisonExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbComparisonExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbComparisonExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbConstantExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbConstantExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCrossJoinExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCrossJoinExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbCrossJoinExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDerefExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDerefExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbDerefExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDistinctExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbDistinctExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbDistinctExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbElementExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbElementExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbElementExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExceptExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExceptExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbExceptExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFilterExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFilterExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbFilterExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbFunctionExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbFunctionExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbEntityRefExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbEntityRefExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbEntityRefExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefKeyExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefKeyExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbRefKeyExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupByExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupByExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbGroupByExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIntersectExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIntersectExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbIntersectExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsEmptyExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsEmptyExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbIsEmptyExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsNullExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsNullExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbIsNullExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsOfExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbIsOfExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbIsOfExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbJoinExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbJoinExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbJoinExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambdaExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambdaExpression" /> that is visited.
        /// </param>
        public virtual void Visit(DbLambdaExpression expression)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLikeExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbLikeExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLimitExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbLimitExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNewInstanceExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNewInstanceExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbNewInstanceExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNotExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNotExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbNotExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNullExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbNullExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbNullExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOfTypeExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOfTypeExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbOfTypeExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOrExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbOrExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbOrExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbParameterReferenceExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbParameterReferenceExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbProjectExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbProjectExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbProjectExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbPropertyExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQuantifierExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQuantifierExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbQuantifierExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRefExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbRefExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbRelationshipNavigationExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbRelationshipNavigationExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbScanExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbScanExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbScanExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSkipExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbSkipExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbSortExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbTreatExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbTreatExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbTreatExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbUnionAllExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbUnionAllExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbUnionAllExpression expression);

        /// <summary>
        /// When overridden in a derived class, implements the visitor pattern for
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" />
        /// .
        /// </summary>
        /// <param name="expression">
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" /> that is visited.
        /// </param>
        public abstract void Visit(DbVariableReferenceExpression expression);

        /// <summary>
        /// Visitor pattern method for DbInExpression.
        /// </summary>
        /// <param name="expression"> The DbInExpression that is being visited. </param>
        public virtual void Visit(DbInExpression expression)
        {
            throw new NotImplementedException(Strings.VisitDbInExpressionNotImplemented);
        }
    }
}
