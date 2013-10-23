// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyCommandTrees = System.Data.Common.CommandTrees;
using LegacyExpressionBuilder = System.Data.Common.CommandTrees.ExpressionBuilder.DbExpressionBuilder;
using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions;

    internal class LegacyDbExpressionConverter : DbExpressionVisitor<LegacyCommandTrees.DbExpression>
    {
        private readonly LegacyMetadata.StoreItemCollection _storeItemCollection;

        public LegacyDbExpressionConverter(LegacyMetadata.StoreItemCollection storeItemCollection)
        {
            Debug.Assert(storeItemCollection != null, "storeItemCollection != null");

            _storeItemCollection = storeItemCollection;
        }

        public override LegacyCommandTrees.DbExpression Visit(DbExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbAndExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Left.Accept(this).And(expression.Right.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbApplyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbArithmeticExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.Plus:
                    return expression.Arguments[0].Accept(this).Plus(expression.Arguments[1].Accept(this));
                case DbExpressionKind.Minus:
                    return expression.Arguments[0].Accept(this).Minus(expression.Arguments[1].Accept(this));
                case DbExpressionKind.Multiply:
                    return expression.Arguments[0].Accept(this).Multiply(expression.Arguments[1].Accept(this));
                case DbExpressionKind.Divide:
                    return expression.Arguments[0].Accept(this).Divide(expression.Arguments[1].Accept(this));
                case DbExpressionKind.Modulo:
                    return expression.Arguments[0].Accept(this).Modulo(expression.Arguments[1].Accept(this));
                case DbExpressionKind.UnaryMinus:
                    return expression.Arguments[0].Accept(this).UnaryMinus();
            }

            Debug.Fail("Unknown arithmetic operator.");

            throw new NotSupportedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbCaseExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                LegacyExpressionBuilder.Case(
                    expression.When.Select(e => e.Accept(this)),
                    expression.Then.Select(e => e.Accept(this)),
                    expression.Else.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbCastExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Argument.Accept(this).CastTo(expression.ResultType.ToLegacyEdmTypeUsage());
        }

        public override LegacyCommandTrees.DbExpression Visit(DbComparisonExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            var a = expression.Left.Accept(this);
            var b = expression.Right.Accept(this);

            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    return a.Equal(b);

                case DbExpressionKind.NotEquals:
                    return a.NotEqual(b);

                case DbExpressionKind.GreaterThan:
                    return a.GreaterThan(b);

                case DbExpressionKind.GreaterThanOrEquals:
                    return a.GreaterThanOrEqual(b);

                case DbExpressionKind.LessThan:
                    return a.LessThan(b);

                case DbExpressionKind.LessThanOrEquals:
                    return a.LessThanOrEqual(b);
            }

            Debug.Fail("Unknown comparison operator.");

            throw new NotSupportedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbConstantExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.ResultType.ToLegacyEdmTypeUsage().Constant(expression.Value);
        }

        public override LegacyCommandTrees.DbExpression Visit(DbCrossJoinExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                LegacyExpressionBuilder
                    .CrossJoin(
                        expression.Inputs.Select(
                            binding => binding.Expression.Accept(this).BindAs(binding.VariableName)));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbDerefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbDistinctExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Argument.Accept(this).Distinct();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbElementExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Argument.Accept(this).Element();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbExceptExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Left.Accept(this).Except(expression.Right.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbFilterExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Input.Expression.Accept(this)
                .BindAs(expression.Input.VariableName)
                .Filter(expression.Predicate.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbFunctionExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbEntityRefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbRefKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbGroupByExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbIntersectExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Left.Accept(this).Intersect(expression.Right.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbIsEmptyExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Argument.Accept(this).IsEmpty();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbIsNullExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Argument.Accept(this).IsNull();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbIsOfExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbJoinExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.InnerJoin:
                    return
                        expression.Left.Expression.Accept(this)
                            .BindAs(expression.Left.VariableName)
                            .InnerJoin(
                                expression.Right.Expression.Accept(this).BindAs(expression.Right.VariableName),
                                expression.JoinCondition.Accept(this));

                case DbExpressionKind.LeftOuterJoin:
                    return
                        expression.Left.Expression.Accept(this)
                            .BindAs(expression.Left.VariableName)
                            .LeftOuterJoin(
                                expression.Right.Expression.Accept(this).BindAs(expression.Right.VariableName),
                                expression.JoinCondition.Accept(this));

                case DbExpressionKind.FullOuterJoin:
                    return
                        expression.Left.Expression.Accept(this)
                            .BindAs(expression.Left.VariableName)
                            .FullOuterJoin(
                                expression.Right.Expression.Accept(this).BindAs(expression.Right.VariableName),
                                expression.JoinCondition.Accept(this));
            }

            Debug.Fail("Unknown comparison operator.");

            throw new NotSupportedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbLikeExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Argument.Accept(this)
                    .Like(expression.Pattern.Accept(this), expression.Escape.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbLimitExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Argument.Accept(this)
                    .Limit(expression.Limit.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbNewInstanceExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");
            Debug.Assert(
                expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType ||
                expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType,
                "Only collection and row types are supported");

            return expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType
                       ? CreateNewCollectionInstance(expression)
                       : CreateNewRowTypeInstance(expression);
        }

        private LegacyCommandTrees.DbNewInstanceExpression CreateNewCollectionInstance(DbNewInstanceExpression inputExpression)
        {
            Debug.Assert(inputExpression != null, "inputExpression != null");
            Debug.Assert(
                inputExpression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType,
                "Collection type expected");

            return LegacyExpressionBuilder.NewCollection(inputExpression.Arguments.Select(a => a.Accept(this)));
        }

        private LegacyCommandTrees.DbNewInstanceExpression CreateNewRowTypeInstance(DbNewInstanceExpression inputExpression)
        {
            Debug.Assert(inputExpression != null, "inputExpression != null");
            Debug.Assert(
                inputExpression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType,
                "Row type expected");

            var arguments = new KeyValuePair<string, LegacyCommandTrees.DbExpression>[inputExpression.Arguments.Count];

            for (var argIdx = 0; argIdx < arguments.Length; argIdx++)
            {
                arguments[argIdx] =
                    new KeyValuePair<string, LegacyCommandTrees.DbExpression>(
                        ((RowType)inputExpression.ResultType.EdmType).Properties[argIdx].Name,
                        inputExpression.Arguments[argIdx].Accept(this));
            }

            return LegacyExpressionBuilder.NewRow(arguments);
        }

        public override LegacyCommandTrees.DbExpression Visit(DbNotExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Argument.Accept(this).Not();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbNullExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return GetLegacyTypeUsage(expression.ResultType).Null();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbOfTypeExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbOrExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Left.Accept(this).Or(expression.Right.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbParameterReferenceExpression expression)
        {
            return GetLegacyTypeUsage(expression.ResultType).Parameter(expression.ParameterName);
        }

        public override LegacyCommandTrees.DbExpression Visit(DbProjectExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Input.Expression.Accept(this)
                .BindAs(expression.Input.VariableName)
                .Project(expression.Projection.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbPropertyExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return expression.Instance.Accept(this).Property(expression.Property.Name);
        }

        public override LegacyCommandTrees.DbExpression Visit(DbQuantifierExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbRefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbRelationshipNavigationExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbScanExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                _storeItemCollection
                    .GetEntityContainer(expression.Target.EntityContainer.Name)
                    .GetEntitySetByName(expression.Target.Name, false)
                    .Scan();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbSortExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Input.Expression.Accept(this)
                    .BindAs(expression.Input.VariableName)
                    .Sort(expression.SortOrder.Select(ConvertToLegacySortClause));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbSkipExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Input.Expression.Accept(this)
                    .BindAs(expression.Input.VariableName)
                    .Skip(
                        expression.SortOrder.Select(ConvertToLegacySortClause),
                        expression.Count.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbTreatExpression expression)
        {
            throw new NotImplementedException();
        }

        public override LegacyCommandTrees.DbExpression Visit(DbUnionAllExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return
                expression.Left.Accept(this).UnionAll(expression.Right.Accept(this));
        }

        public override LegacyCommandTrees.DbExpression Visit(DbVariableReferenceExpression expression)
        {
            Debug.Assert(expression != null, "expression != null");

            return GetLegacyTypeUsage(expression.ResultType).Variable(expression.VariableName);
        }

        public override LegacyCommandTrees.DbExpression Visit(DbInExpression expression)
        {
            // DbInExpression was added in EF6
            throw new NotImplementedException();
        }

        private LegacyMetadata.TypeUsage GetLegacyTypeUsage(TypeUsage typeUsage)
        {
            Debug.Assert(typeUsage != null, "typeUsage != null");

            return
                typeUsage.EdmType.GetDataSpace() == DataSpace.CSpace
                    ? typeUsage.ToLegacyEdmTypeUsage()
                    : typeUsage.ToLegacyStoreTypeUsage(
                        _storeItemCollection.GetItems<LegacyMetadata.EdmType>().ToArray());
        }

        private LegacyCommandTrees.DbSortClause ConvertToLegacySortClause(DbSortClause sortClause)
        {
            return
                sortClause.Ascending
                    ? string.IsNullOrEmpty(sortClause.Collation)
                          ? sortClause.Expression.Accept(this).ToSortClause()
                          : sortClause.Expression.Accept(this).ToSortClause(sortClause.Collation)
                    : string.IsNullOrEmpty(sortClause.Collation)
                          ? sortClause.Expression.Accept(this).ToSortClauseDescending()
                          : sortClause.Expression.Accept(this).ToSortClauseDescending(sortClause.Collation);
        }
    }
}
