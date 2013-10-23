// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyCommandTrees = System.Data.Common.CommandTrees;
using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions;
    using Xunit;

    public class LegacyDbExpressionConverterTests
    {
        private readonly LegacyDbExpressionConverter _legacyDbExpressionConverter;
        private readonly StoreItemCollection _storeItemCollection;
        private readonly LegacyMetadata.StoreItemCollection _legacyStoreItemCollection;

        public LegacyDbExpressionConverterTests()
        {
            const string ssdl =
                "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>"
                +
                "  <EntityContainer Name='AdventureWorksModelStoreContainer'>" +
                "    <EntitySet Name='EntitiesSet' EntityType='AdventureWorksModel.Store.Entities' Schema='dbo' />" +
                "    <EntitySet Name='OtherEntitiesSet' EntityType='AdventureWorksModel.Store.OtherEntities' Schema='dbo' />" +
                "  </EntityContainer>" +
                "  <EntityType Name='Entities'>" +
                "    <Key>" +
                "      <PropertyRef Name='Id' />" +
                "    </Key>" +
                "    <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />" +
                "    <Property Name='Name' Type='nvarchar(max)' Nullable='false' />" +
                "  </EntityType>" +
                "  <EntityType Name='OtherEntities'>" +
                "    <Key>" +
                "      <PropertyRef Name='Id' />" +
                "    </Key>" +
                "    <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />" +
                "    <Property Name='Name' Type='nvarchar(max)' Nullable='false' />" +
                "  </EntityType>" +
                "</Schema>";

            _storeItemCollection = Utils.CreateStoreItemCollection(ssdl);
            _legacyStoreItemCollection = _storeItemCollection.ToLegacyStoreItemCollection();
            _legacyDbExpressionConverter = new LegacyDbExpressionConverter(_legacyStoreItemCollection);
        }

        [Fact]
        public void Visit_DbConstantExpression_creates_equivalent_legacy_DbConstantExpression()
        {
            var typeUsage =
                TypeUsage.CreateStringTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    isUnicode: false,
                    isFixedLength: true,
                    maxLength: 1000);

            var constantExpression = typeUsage.Constant("test");

            var legacyConstantExpression =
                _legacyDbExpressionConverter.Visit(constantExpression) as LegacyCommandTrees.DbConstantExpression;

            Assert.NotNull(legacyConstantExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Constant, legacyConstantExpression.ExpressionKind);
            Assert.Equal(constantExpression.Value, legacyConstantExpression.Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyConstantExpression.ResultType, constantExpression.ResultType);
        }

        [Fact]
        public void
            Visit_DbVariableReferenceExpression_creates_equivalent_legacy_DbVariableReferenceExpression_for_CSpace_type()
        {
            var variableReference =
                TypeUsage
                    .CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))
                    .Variable("variable");

            var legacyVariableReference =
                _legacyDbExpressionConverter.Visit(variableReference) as
                LegacyCommandTrees.DbVariableReferenceExpression;

            Assert.NotNull(legacyVariableReference);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.VariableReference, legacyVariableReference.ExpressionKind);
            Assert.Equal(variableReference.VariableName, legacyVariableReference.VariableName);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyVariableReference.ResultType, variableReference.ResultType);
        }

        [Fact]
        public void
            Visit_DbVariableReferenceExpression_creates_equivalent_legacy_DbVariableReferenceExpression_for_SSpace_type()
        {
            var variableReference =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("variable");

            var legacyVariableReference =
                _legacyDbExpressionConverter.Visit(variableReference) as
                LegacyCommandTrees.DbVariableReferenceExpression;

            Assert.NotNull(legacyVariableReference);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.VariableReference, legacyVariableReference.ExpressionKind);
            Assert.Equal(variableReference.VariableName, legacyVariableReference.VariableName);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyVariableReference.ResultType, variableReference.ResultType);
        }

        [Fact]
        public void Visit_DbScanExpression_creates_equivalent_legacy_DbScanExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var legacyScanExpression =
                _legacyDbExpressionConverter.Visit(scanExpression) as LegacyCommandTrees.DbScanExpression;

            Assert.NotNull(legacyScanExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyScanExpression.ExpressionKind);
            Assert.Equal(scanExpression.Target.Name, legacyScanExpression.Target.Name);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyScanExpression.ResultType, scanExpression.ResultType);
        }

        [Fact]
        public void Visit_DbPropertyExpression_creates_equivalent_legacy_DbPropertyExpression()
        {
            var propertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var legacyPropertyExpression =
                _legacyDbExpressionConverter.Visit(propertyExpression) as LegacyCommandTrees.DbPropertyExpression;

            Assert.NotNull(legacyPropertyExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Property, legacyPropertyExpression.ExpressionKind);
            Assert.Equal(
                LegacyCommandTrees.DbExpressionKind.VariableReference,
                legacyPropertyExpression.Instance.ExpressionKind);
            Assert.Equal(
                "Table",
                ((LegacyCommandTrees.DbVariableReferenceExpression)legacyPropertyExpression.Instance)
                    .VariableName);
            Assert.Equal("Id", legacyPropertyExpression.Property.Name);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyPropertyExpression.Property.TypeUsage, propertyExpression.Property.TypeUsage);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyPropertyExpression.ResultType, propertyExpression.ResultType);
        }

        [Fact]
        public void Visit_DbNewInstanceExpression_collection_creates_equivalent_legacy_DbNewInstanceExpression()
        {
            var propertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var newInstanceExpressionCollection =
                DbExpressionBuilder.NewCollection(propertyExpression, DbExpressionBuilder.Constant(42));

            var legacyNewInstanceExpressionCollection =
                _legacyDbExpressionConverter.Visit(newInstanceExpressionCollection) as
                LegacyCommandTrees.DbNewInstanceExpression;

            Assert.NotNull(legacyNewInstanceExpressionCollection);
            Assert.Equal(
                LegacyCommandTrees.DbExpressionKind.NewInstance,
                legacyNewInstanceExpressionCollection.ExpressionKind);
            Assert.Equal(2, legacyNewInstanceExpressionCollection.Arguments.Count);
            Assert.IsType<LegacyCommandTrees.DbPropertyExpression>(legacyNewInstanceExpressionCollection.Arguments[0]);
            Assert.IsType<LegacyCommandTrees.DbConstantExpression>(legacyNewInstanceExpressionCollection.Arguments[1]);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyNewInstanceExpressionCollection.ResultType, newInstanceExpressionCollection.ResultType);
        }

        [Fact]
        public void Visit_DbNewInstanceExpression_rowtype_creates_equivalent_legacy_DbNewInstanceExpression()
        {
            var propertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var newInstanceExpressionRowType =
                DbExpressionBuilder.NewRow(
                    new[]
                        {
                            new KeyValuePair<string, DbExpression>("Id", propertyExpression),
                            new KeyValuePair<string, DbExpression>("Const", DbExpressionBuilder.Constant(42))
                        });

            var legacyNewInstanceExpressionRowType =
                _legacyDbExpressionConverter.Visit(newInstanceExpressionRowType) as
                LegacyCommandTrees.DbNewInstanceExpression;

            Assert.NotNull(legacyNewInstanceExpressionRowType);
            Assert.Equal(
                LegacyCommandTrees.DbExpressionKind.NewInstance,
                legacyNewInstanceExpressionRowType.ExpressionKind);
            Assert.Equal(2, legacyNewInstanceExpressionRowType.Arguments.Count);
            Assert.IsType<LegacyCommandTrees.DbPropertyExpression>(legacyNewInstanceExpressionRowType.Arguments[0]);
            Assert.IsType<LegacyCommandTrees.DbConstantExpression>(legacyNewInstanceExpressionRowType.Arguments[1]);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyNewInstanceExpressionRowType.ResultType, newInstanceExpressionRowType.ResultType);
        }

        [Fact]
        public void Visit_DbProjectExpression_creates_equivalent_legacy_DbProjectExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var propertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var newInstanceExpression =
                DbExpressionBuilder.NewRow(
                    new[]
                        {
                            new KeyValuePair<string, DbExpression>("Id", propertyExpression),
                            new KeyValuePair<string, DbExpression>("Const", DbExpressionBuilder.Constant(42))
                        });

            var projectExpression =
                scanExpression
                    .BindAs("Table")
                    .Project(newInstanceExpression);

            var legacyProjectExpression =
                _legacyDbExpressionConverter.Visit(projectExpression) as LegacyCommandTrees.DbProjectExpression;

            Assert.NotNull(legacyProjectExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Project, legacyProjectExpression.ExpressionKind);
            Assert.Equal(
                LegacyCommandTrees.DbExpressionKind.NewInstance,
                legacyProjectExpression.Projection.ExpressionKind);
            Assert.Equal(projectExpression.Input.VariableName, legacyProjectExpression.Input.VariableName);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyProjectExpression.Input.VariableType, projectExpression.Input.VariableType);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyProjectExpression.ResultType, projectExpression.ResultType);
        }

        [Fact]
        public void Visit_DbSortExpression_creates_equivalent_legacy_DbSortExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var idProperty =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var nameProperty =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Name");

            var sortExpression =
                scanExpression
                    .BindAs("Table")
                    .Sort(
                        new[]
                            {
                                idProperty.ToSortClause(),
                                nameProperty.ToSortClause("testCollationAscending"),
                                nameProperty.ToSortClauseDescending(),
                                nameProperty.ToSortClauseDescending("testCollationDescending")
                            });

            var legacySortExpression =
                _legacyDbExpressionConverter.Visit(sortExpression) as LegacyCommandTrees.DbSortExpression;

            Assert.NotNull(legacySortExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Sort, legacySortExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacySortExpression.Input.Expression.ExpressionKind);
            Assert.Equal("Table", legacySortExpression.Input.VariableName);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacySortExpression.Input.VariableType, sortExpression.Input.VariableType);

            Assert.Equal(4, legacySortExpression.SortOrder.Count);
            Assert.True(
                legacySortExpression.SortOrder.All(
                    e => e.Expression.ExpressionKind == LegacyCommandTrees.DbExpressionKind.Property));
            Assert.True(legacySortExpression.SortOrder[0].Ascending);
            Assert.Empty(legacySortExpression.SortOrder[0].Collation);
            Assert.True(legacySortExpression.SortOrder[1].Ascending);
            Assert.Equal("testCollationAscending", legacySortExpression.SortOrder[1].Collation);
            Assert.False(legacySortExpression.SortOrder[2].Ascending);
            Assert.Empty(legacySortExpression.SortOrder[2].Collation);
            Assert.False(legacySortExpression.SortOrder[3].Ascending);
            Assert.Equal("testCollationDescending", legacySortExpression.SortOrder[3].Collation);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacySortExpression.ResultType, sortExpression.ResultType);
        }

        [Fact]
        public void Visit_DbComparisonExpression_can_handle_all_comparison_operations()
        {
            var a = DbExpressionBuilder.Constant(42);
            var b = DbExpressionBuilder.Constant(911);

            ConvertAndVerifyComparisonExpression(a.Equal(b));
            ConvertAndVerifyComparisonExpression(a.NotEqual(b));
            ConvertAndVerifyComparisonExpression(a.LessThan(b));
            ConvertAndVerifyComparisonExpression(a.LessThanOrEqual(b));
            ConvertAndVerifyComparisonExpression(a.GreaterThan(b));
            ConvertAndVerifyComparisonExpression(a.GreaterThanOrEqual(b));
        }

        private void ConvertAndVerifyComparisonExpression(DbComparisonExpression comparisonExpression)
        {
            var legacyComparisonExpression =
                _legacyDbExpressionConverter.Visit(comparisonExpression) as LegacyCommandTrees.DbComparisonExpression;

            Assert.NotNull(legacyComparisonExpression);
            Assert.Equal((int)comparisonExpression.ExpressionKind, (int)legacyComparisonExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Constant, legacyComparisonExpression.Left.ExpressionKind);
            Assert.Equal(
                ((DbConstantExpression)comparisonExpression.Left).Value,
                ((LegacyCommandTrees.DbConstantExpression)legacyComparisonExpression.Left).Value);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Constant, legacyComparisonExpression.Right.ExpressionKind);
            Assert.Equal(
                ((DbConstantExpression)comparisonExpression.Right).Value,
                ((LegacyCommandTrees.DbConstantExpression)legacyComparisonExpression.Right).Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyComparisonExpression.ResultType, comparisonExpression.ResultType);
        }

        [Fact]
        public void Visit_DbArithmeticExpression_creates_equivalent_legacy_DbArithmeticExpressions_for_operators()
        {
            var a = DbExpressionBuilder.Constant(42);
            var b = DbExpressionBuilder.Constant(911);
            ConvertAndVerifyArithmeticExpression(a.Plus(b));
            ConvertAndVerifyArithmeticExpression(a.Minus(b));
            ConvertAndVerifyArithmeticExpression(a.Multiply(b));
            ConvertAndVerifyArithmeticExpression(a.Divide(b));
            ConvertAndVerifyArithmeticExpression(a.Modulo(b));
            ConvertAndVerifyArithmeticExpression(a.UnaryMinus());
            ConvertAndVerifyArithmeticExpression(a.Negate());
        }

        private void ConvertAndVerifyArithmeticExpression(DbArithmeticExpression arithmeticExpression)
        {
            var legacyArithmeticExpression =
                _legacyDbExpressionConverter.Visit(arithmeticExpression) as LegacyCommandTrees.DbArithmeticExpression;

            Assert.NotNull(arithmeticExpression);

            Assert.Equal((int)arithmeticExpression.ExpressionKind, (int)legacyArithmeticExpression.ExpressionKind);
            Assert.Equal(arithmeticExpression.Arguments.Count, legacyArithmeticExpression.Arguments.Count);
            Assert.True(
                arithmeticExpression.Arguments.Zip(
                    legacyArithmeticExpression.Arguments,
                    (e1, e2) => ((DbConstantExpression)e1).Value == ((LegacyCommandTrees.DbConstantExpression)e2).Value)
                    .All(r => r));

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyArithmeticExpression.ResultType, arithmeticExpression.ResultType);
        }

        [Fact]
        public void Visit_DbCaseExpression_creates_equivalent_legacy_DbCaseExpression()
        {
            var whens =
                new[]
                    {
                        DbExpressionBuilder.Constant(42).NotEqual(DbExpressionBuilder.Constant(42)),
                        DbExpressionBuilder.Constant(911).Equal(DbExpressionBuilder.Constant(911)),
                    };

            var thens =
                new[]
                    {
                        DbExpressionBuilder.False,
                        DbExpressionBuilder.True
                    };

            var caseExpression =
                DbExpressionBuilder.Case(whens, thens, DbExpressionBuilder.False);

            var legacyCaseExpression =
                _legacyDbExpressionConverter.Visit(caseExpression) as LegacyCommandTrees.DbCaseExpression;

            Assert.NotNull(legacyCaseExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Case, legacyCaseExpression.ExpressionKind);
            Assert.Equal(2, legacyCaseExpression.When.Count);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.NotEquals, legacyCaseExpression.When[0].ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Equals, legacyCaseExpression.When[1].ExpressionKind);

            Assert.Equal(2, legacyCaseExpression.Then.Count);
            Assert.False((bool)((LegacyCommandTrees.DbConstantExpression)legacyCaseExpression.Then[0]).Value);
            Assert.True((bool)((LegacyCommandTrees.DbConstantExpression)legacyCaseExpression.Then[1]).Value);
            Assert.False((bool)((LegacyCommandTrees.DbConstantExpression)legacyCaseExpression.Else).Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyCaseExpression.ResultType, caseExpression.ResultType);
        }

        [Fact]
        public void Visit_DbFitlerExpression_creates_equivalent_legacy_DbFilterExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var filterExpression =
                scanExpression
                    .BindAs("Table")
                    .Filter(
                        DbExpressionBuilder.Constant(911).Equal(DbExpressionBuilder.Constant(911)));

            var legacyFilterExpression =
                _legacyDbExpressionConverter.Visit(filterExpression) as LegacyCommandTrees.DbFilterExpression;

            Assert.NotNull(legacyFilterExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Filter, legacyFilterExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyFilterExpression.Input.Expression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Equals, legacyFilterExpression.Predicate.ExpressionKind);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyFilterExpression.ResultType, filterExpression.ResultType);
        }

        [Fact]
        public void Visit_DbUnionAllExpression_creates_equivalent_legacy_DbUnionAllExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var unionAllExpression = scanExpression.UnionAll(scanExpression);

            var legacyUnionAllExpression =
                _legacyDbExpressionConverter.Visit(unionAllExpression) as LegacyCommandTrees.DbUnionAllExpression;

            Assert.NotNull(legacyUnionAllExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.UnionAll, legacyUnionAllExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyUnionAllExpression.Left.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyUnionAllExpression.Right.ExpressionKind);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyUnionAllExpression.ResultType, unionAllExpression.ResultType);
        }

        [Fact]
        public void Visit_DbAndExpression_creates_equivalent_legacy_DbAndExpression()
        {
            var andExpression =
                DbExpressionBuilder.False
                    .And(DbExpressionBuilder.True);

            var legacyAndExpression =
                _legacyDbExpressionConverter.Visit(andExpression) as LegacyCommandTrees.DbAndExpression;

            Assert.NotNull(legacyAndExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.And, legacyAndExpression.ExpressionKind);
            Assert.False((bool)((LegacyCommandTrees.DbConstantExpression)legacyAndExpression.Left).Value);
            Assert.True((bool)((LegacyCommandTrees.DbConstantExpression)legacyAndExpression.Right).Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyAndExpression.ResultType, andExpression.ResultType);
        }

        [Fact]
        public void Visit_DbOrExpression_creates_equivalent_legacy_DbOrExpression()
        {
            var orExpression =
                DbExpressionBuilder.False
                    .Or(DbExpressionBuilder.True);

            var legacyOrExpression =
                _legacyDbExpressionConverter.Visit(orExpression) as LegacyCommandTrees.DbOrExpression;

            Assert.NotNull(legacyOrExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Or, legacyOrExpression.ExpressionKind);
            Assert.False((bool)((LegacyCommandTrees.DbConstantExpression)legacyOrExpression.Left).Value);
            Assert.True((bool)((LegacyCommandTrees.DbConstantExpression)legacyOrExpression.Right).Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyOrExpression.ResultType, orExpression.ResultType);
        }

        [Fact]
        public void Visit_DbJoinExpression_creates_equivalent_InnerJoin_DbJoinExpression()
        {
            ConvertAndVerifyJoinExpressions(
                (left, right, joinCondition) => left.InnerJoin(right, joinCondition));
        }

        [Fact]
        public void Visit_DbJoinExpression_creates_equivalent_LeftOuterJoin_DbJoinExpression()
        {
            ConvertAndVerifyJoinExpressions(
                (left, right, joinCondition) => left.LeftOuterJoin(right, joinCondition));
        }

        [Fact]
        public void Visit_DbJoinExpression_creates_equivalent_FullOuterJoin_DbJoinExpression()
        {
            ConvertAndVerifyJoinExpressions(
                (left, right, joinCondition) => left.FullOuterJoin(right, joinCondition));
        }

        private void ConvertAndVerifyJoinExpressions(
            Func<DbExpressionBinding, DbExpressionBinding, DbExpression, DbJoinExpression> createJoinExpression)
        {
            var leftScanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var leftPropertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("leftTable")
                    .Property("Id");

            var rightScanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "OtherEntitiesSet")
                    .Scan();

            var rightPropertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "OtherEntities"))
                    .Variable("rightTable")
                    .Property("Id");

            var joinExpression =
                createJoinExpression(
                    leftScanExpression.BindAs("leftTable"),
                    rightScanExpression.BindAs("rightTable"),
                    leftPropertyExpression.Equal(rightPropertyExpression));

            var legacyJoinExpression =
                _legacyDbExpressionConverter.Visit(joinExpression) as LegacyCommandTrees.DbJoinExpression;

            Assert.NotNull(legacyJoinExpression);
            Assert.Equal((int)joinExpression.ExpressionKind, (int)legacyJoinExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyJoinExpression.Left.Expression.ExpressionKind);
            Assert.Equal("EntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyJoinExpression.Left.Expression).Target.Name);
            Assert.Equal("leftTable", legacyJoinExpression.Left.VariableName);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyJoinExpression.Right.Expression.ExpressionKind);
            Assert.Equal("OtherEntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyJoinExpression.Right.Expression).Target.Name);
            Assert.Equal("rightTable", legacyJoinExpression.Right.VariableName);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Equals, legacyJoinExpression.JoinCondition.ExpressionKind);
            var comparisonExpression = (LegacyCommandTrees.DbComparisonExpression)legacyJoinExpression.JoinCondition;
            Assert.Equal("Entities", ((LegacyCommandTrees.DbPropertyExpression)comparisonExpression.Left).Property.DeclaringType.Name);
            Assert.Equal("OtherEntities", ((LegacyCommandTrees.DbPropertyExpression)comparisonExpression.Right).Property.DeclaringType.Name);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyJoinExpression.ResultType, joinExpression.ResultType);
        }

        [Fact]
        public void Visit_DbCrossJoinExpression_creates_equivalent_DbCrossJoinExpression()
        {
            var bindings =
                new[]
                    {
                        _storeItemCollection
                            .GetEntityContainer("AdventureWorksModelStoreContainer")
                            .EntitySets.Single(e => e.Name == "EntitiesSet")
                            .Scan()
                            .BindAs("table1"),
                        _storeItemCollection
                            .GetEntityContainer("AdventureWorksModelStoreContainer")
                            .EntitySets.Single(e => e.Name == "OtherEntitiesSet")
                            .Scan()
                            .BindAs("table2")
                    };

            var crossJoinExpression = DbExpressionBuilder.CrossJoin(bindings);

            var legacyCrossJoinExpression =
                _legacyDbExpressionConverter.Visit(crossJoinExpression) as LegacyCommandTrees.DbCrossJoinExpression;

            Assert.NotNull(legacyCrossJoinExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.CrossJoin, legacyCrossJoinExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyCrossJoinExpression.Inputs[0].Expression.ExpressionKind);
            Assert.Equal("EntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyCrossJoinExpression.Inputs[0].Expression).Target.Name);
            Assert.Equal("table1", legacyCrossJoinExpression.Inputs[0].VariableName);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyCrossJoinExpression.Inputs[1].Expression.ExpressionKind);
            Assert.Equal(
                "OtherEntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyCrossJoinExpression.Inputs[1].Expression).Target.Name);
            Assert.Equal("table2", legacyCrossJoinExpression.Inputs[1].VariableName);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyCrossJoinExpression.ResultType, crossJoinExpression.ResultType);
        }

        [Fact]
        public void Visit_DbIsNullExpression_creates_equivalent_legacy_DbIsNullExpression()
        {
            var isNullExpression =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)).Null().IsNull();

            var legacyIsNullExpression =
                _legacyDbExpressionConverter.Visit(isNullExpression) as LegacyCommandTrees.DbIsNullExpression;

            Assert.NotNull(legacyIsNullExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.IsNull, legacyIsNullExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Null, legacyIsNullExpression.Argument.ExpressionKind);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyIsNullExpression.ResultType, isNullExpression.ResultType);
        }

        [Fact]
        public void Visit_DbNullExpression_creates_equivalent_legacy_DbNullExpression()
        {
            var nullExpression =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)).Null();

            var legacyNullExpression =
                _legacyDbExpressionConverter.Visit(nullExpression) as LegacyCommandTrees.DbNullExpression;

            Assert.NotNull(legacyNullExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Null, legacyNullExpression.ExpressionKind);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyNullExpression.ResultType, nullExpression.ResultType);
        }

        [Fact]
        public void Visit_DbNotExpression_creates_equivalent_legacy_DbNotExpression()
        {
            var notExpression = DbExpressionBuilder.True.Not();

            var legacyNotExpression =
                _legacyDbExpressionConverter.Visit(notExpression) as LegacyCommandTrees.DbNotExpression;

            Assert.NotNull(legacyNotExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Not, legacyNotExpression.ExpressionKind);
            Assert.True((bool)((LegacyCommandTrees.DbConstantExpression)legacyNotExpression.Argument).Value);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyNotExpression.ResultType, notExpression.ResultType);
        }

        [Fact]
        public void Visit_DbLikeExpression_creates_equivalent_legacy_DbLikeExpression()
        {
            var propertyExpression =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Name");

            var stringTypeUsage =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var likeExpression = propertyExpression.Like(stringTypeUsage.Constant("foo"), stringTypeUsage.Constant("bar"));

            var legacyLikeExpression =
                _legacyDbExpressionConverter.Visit(likeExpression) as LegacyCommandTrees.DbLikeExpression;

            Assert.NotNull(legacyLikeExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Like, legacyLikeExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Property, legacyLikeExpression.Argument.ExpressionKind);
            Assert.Equal("foo", (string)((LegacyCommandTrees.DbConstantExpression)legacyLikeExpression.Pattern).Value);
            Assert.Equal("bar", (string)((LegacyCommandTrees.DbConstantExpression)legacyLikeExpression.Escape).Value);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyLikeExpression.ResultType, likeExpression.ResultType);
        }

        [Fact]
        public void Visit_DbParameterRefExpression_creates_equivalent_legacy_DbParameterRefExpression()
        {
            var paramRefExpression =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    .Parameter("foo");

            var legacyParamRefExpression =
                _legacyDbExpressionConverter.Visit(paramRefExpression) as LegacyCommandTrees.DbParameterReferenceExpression;

            Assert.NotNull(legacyParamRefExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.ParameterReference, legacyParamRefExpression.ExpressionKind);
            Assert.Equal("foo", legacyParamRefExpression.ParameterName);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyParamRefExpression.ResultType, paramRefExpression.ResultType);
        }

        [Fact]
        public void Visit_DbCastExpression_creates_equivalent_legacy_DbCastExpression()
        {
            var castExpression =
                DbExpressionBuilder.Constant(42).CastTo(
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal)));

            var legacyCastExpression =
                _legacyDbExpressionConverter.Visit(castExpression) as LegacyCommandTrees.DbCastExpression;

            Assert.NotNull(legacyCastExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Cast, legacyCastExpression.ExpressionKind);
            Assert.Equal(42, (int)((LegacyCommandTrees.DbConstantExpression)legacyCastExpression.Argument).Value);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyCastExpression.ResultType, castExpression.ResultType);
        }

        [Fact]
        public void Visit_DbDistinctExpression_creates_equivalent_legacy_DbDistinctExpression()
        {
            var distinctExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan()
                    .Distinct();

            var legacyDistinctExpression =
                _legacyDbExpressionConverter.Visit(distinctExpression) as LegacyCommandTrees.DbDistinctExpression;

            Assert.NotNull(legacyDistinctExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Distinct, legacyDistinctExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyDistinctExpression.Argument.ExpressionKind);
            Assert.Equal("EntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyDistinctExpression.Argument).Target.Name);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyDistinctExpression.ResultType, distinctExpression.ResultType);
        }

        [Fact]
        public void Visit_DbSkipExpression_creates_equivalent_legacy_DbSkipExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var idProperty =
                TypeUsage
                    .CreateDefaultTypeUsage(
                        _storeItemCollection.GetItems<EntityType>().Single(e => e.Name == "Entities"))
                    .Variable("Table")
                    .Property("Id");

            var skipExpression =
                scanExpression
                    .BindAs("Table")
                    .Skip(new[] { idProperty.ToSortClause() }, DbExpressionBuilder.Constant(42));

            var legacySkipExpression =
                _legacyDbExpressionConverter.Visit(skipExpression) as LegacyCommandTrees.DbSkipExpression;

            Assert.NotNull(legacySkipExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Skip, legacySkipExpression.ExpressionKind);
            Assert.Equal("Table", legacySkipExpression.Input.VariableName);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacySkipExpression.Input.Expression.ExpressionKind);
            Assert.Equal(
                "Id",
                ((LegacyCommandTrees.DbPropertyExpression)legacySkipExpression.SortOrder.Single().Expression).Property.Name);
            Assert.Equal(42, ((LegacyCommandTrees.DbConstantExpression)legacySkipExpression.Count).Value);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacySkipExpression.ResultType, skipExpression.ResultType);
        }

        [Fact]
        public void Visit_DbLimitExpression_creates_equivalent_legacy_DbLimitExpression()
        {
            var scanExpression =
                _storeItemCollection
                    .GetEntityContainer("AdventureWorksModelStoreContainer")
                    .EntitySets.Single(e => e.Name == "EntitiesSet")
                    .Scan();

            var limitExpression =
                scanExpression.Limit(DbExpressionBuilder.Constant(42));

            var legacyLimitExpression =
                _legacyDbExpressionConverter.Visit(limitExpression) as LegacyCommandTrees.DbLimitExpression;

            Assert.NotNull(legacyLimitExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Limit, legacyLimitExpression.ExpressionKind);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Scan, legacyLimitExpression.Argument.ExpressionKind);
            Assert.Equal("EntitiesSet", ((LegacyCommandTrees.DbScanExpression)legacyLimitExpression.Argument).Target.Name);
            Assert.Equal(42, ((LegacyCommandTrees.DbConstantExpression)legacyLimitExpression.Limit).Value);
            Assert.Equal(limitExpression.WithTies, legacyLimitExpression.WithTies);
            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyLimitExpression.ResultType, limitExpression.ResultType);
        }

        [Fact]
        public void Visit_DbExceptExpression_creates_equivalent_legacy_DbExceptExpression()
        {
            var left = DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(42) });
            var right = DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(24) });

            var exceptExpression = left.Except(right);

            var legacyExceptExpression =
                _legacyDbExpressionConverter.Visit(exceptExpression) as LegacyCommandTrees.DbExceptExpression;

            Assert.NotNull(legacyExceptExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Except, legacyExceptExpression.ExpressionKind);

            Assert.Equal(
                42,
                ((LegacyCommandTrees.DbConstantExpression)
                 ((LegacyCommandTrees.DbNewInstanceExpression)legacyExceptExpression.Left).Arguments.Single())
                    .Value);

            Assert.Equal(
                24,
                ((LegacyCommandTrees.DbConstantExpression)
                 ((LegacyCommandTrees.DbNewInstanceExpression)legacyExceptExpression.Right).Arguments.Single())
                    .Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyExceptExpression.ResultType, exceptExpression.ResultType);
        }

        [Fact]
        public void Visit_DbIntersectExpression_creates_equivalent_legacy_DbIntersectExpression()
        {
            var left = DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(42) });
            var right = DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(24) });

            var intersectExpression = left.Intersect(right);

            var legacyIntersectExpression =
                _legacyDbExpressionConverter.Visit(intersectExpression) as LegacyCommandTrees.DbIntersectExpression;

            Assert.NotNull(legacyIntersectExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Intersect, legacyIntersectExpression.ExpressionKind);

            Assert.Equal(
                42,
                ((LegacyCommandTrees.DbConstantExpression)
                 ((LegacyCommandTrees.DbNewInstanceExpression)legacyIntersectExpression.Left).Arguments.Single())
                    .Value);

            Assert.Equal(
                24,
                ((LegacyCommandTrees.DbConstantExpression)
                 ((LegacyCommandTrees.DbNewInstanceExpression)legacyIntersectExpression.Right).Arguments.Single())
                    .Value);

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyIntersectExpression.ResultType, intersectExpression.ResultType);
        }

        [Fact]
        public void Visit_DbIsEmptyExpression_creates_equivalent_legacy_DbIsEmptyExpression()
        {
            var isEmptyExpression =
                DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(42) })
                    .IsEmpty();

            var legacyIsEmptyExpression
                = _legacyDbExpressionConverter.Visit(isEmptyExpression) as LegacyCommandTrees.DbIsEmptyExpression;

            Assert.NotNull(legacyIsEmptyExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.IsEmpty, legacyIsEmptyExpression.ExpressionKind);
            Assert.Equal(
                42,
                (((LegacyCommandTrees.DbConstantExpression)
                  ((LegacyCommandTrees.DbNewInstanceExpression)legacyIsEmptyExpression.Argument).Arguments.Single())
                    .Value));

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyIsEmptyExpression.ResultType, isEmptyExpression.ResultType);
        }

        [Fact]
        public void Visit_DbElementExpression_creates_equivalent_legacy_DbElementExpression()
        {
            var elementExpression =
                DbExpressionBuilder.NewCollection(new DbExpression[] { DbExpressionBuilder.Constant(42) })
                    .Element();

            var legacyElementExpression
                = _legacyDbExpressionConverter.Visit(elementExpression) as LegacyCommandTrees.DbElementExpression;

            Assert.NotNull(legacyElementExpression);
            Assert.Equal(LegacyCommandTrees.DbExpressionKind.Element, legacyElementExpression.ExpressionKind);
            Assert.Equal(
                42,
                (((LegacyCommandTrees.DbConstantExpression)
                  ((LegacyCommandTrees.DbNewInstanceExpression)legacyElementExpression.Argument).Arguments.Single())
                    .Value));

            TypeUsageVerificationHelper
                .VerifyTypeUsagesEquivalent(legacyElementExpression.ResultType, elementExpression.ResultType);
        }
    }
}
