// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    // <summary>
    // Abstract implementation of node visitor that allows the specification of visit methods
    // for different node types (VisitPre virtual methods) and evaluation of nodes with respect
    // to the typed (TReturn) return values of their children.
    // </summary>
    // <remarks>
    // This is not a general purpose class. It is tailored to the needs of the update pipeline.
    // All virtual methods throw NotSupportedException (must be explicitly overridden by each visitor).
    // </remarks>
    // <typeparam name="TReturn"> Return type for the visitor </typeparam>
    internal abstract class UpdateExpressionVisitor<TReturn> : DbExpressionVisitor<TReturn>
    {
        // <summary>
        // Gets the name of this visitor for debugging and tracing purposes.
        // </summary>
        protected abstract string VisitorName { get; }

        // <summary>
        // Utility method to generate an exception when unsupported node types are encountered.
        // </summary>
        // <param name="node"> Unsupported node </param>
        // <returns> Not supported exception </returns>
        protected NotSupportedException ConstructNotSupportedException(DbExpression node)
        {
            var nodeKind = null == node
                               ? null
                               : node.ExpressionKind.ToString();

            return new NotSupportedException(Strings.Update_UnsupportedExpressionKind(nodeKind, VisitorName));
        }

        public override TReturn Visit(DbExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (null != expression)
            {
                return expression.Accept(this);
            }
            else
            {
                throw ConstructNotSupportedException(expression);
            }
        }

        public override TReturn Visit(DbAndExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbApplyExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbArithmeticExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCaseExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCastExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbComparisonExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbConstantExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCrossJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbDerefExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbDistinctExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbElementExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbExceptExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbFilterExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbFunctionExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLambdaExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbEntityRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRefKeyExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbGroupByExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIntersectExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsEmptyExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsOfExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLikeExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLimitExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbNewInstanceExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbNotExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbOfTypeExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbOrExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbInExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbParameterReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbProjectExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbPropertyExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbQuantifierExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRelationshipNavigationExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbSkipExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbSortExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbTreatExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbUnionAllExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbVariableReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbScanExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw ConstructNotSupportedException(expression);
        }
    }
}
