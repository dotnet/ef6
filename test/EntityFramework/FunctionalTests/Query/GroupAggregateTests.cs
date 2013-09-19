// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class GroupAggregateTests : FunctionalTestBase
    {
        private static readonly MetadataWorkspace workspace = QueryTestHelpers.CreateMetadataWorkspace(
            ProductModel.Csdl, ProductModel.Ssdl, ProductModel.Msl);

        private DbGroupExpressionBinding CreateBasicGroupBinding()
        {
            var entitySet = workspace.GetEntityContainer("ProductContainer", DataSpace.CSpace).GetEntitySetByName("Products", false);
            var scan = entitySet.Scan();
            return scan.GroupBindAs("input", "group");
        }

        [Fact]
        public void Basic_GroupBy_with_group_key_and_group_aggregate()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable.Property("ReorderLevel"))
                           };
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };
            var query = groupBinding.GroupBy(keys, aggregates);

            var expectedSql =
                @"SELECT 
[Project2].[C1] AS [C1], 
[Project2].[ReorderLevel] AS [ReorderLevel], 
[Project2].[C2] AS [C2], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[Distinct1].[ReorderLevel] AS [ReorderLevel], 
	1 AS [C1], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C2]
	FROM   (SELECT DISTINCT 
		[Extent1].[ReorderLevel] AS [ReorderLevel]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1) ) AS [Distinct1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([Distinct1].[ReorderLevel] = [Extent2].[ReorderLevel]) OR (([Distinct1].[ReorderLevel] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project2]
ORDER BY [Project2].[ReorderLevel] ASC, [Project2].[C2] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_with_function_aggregate_and_group_aggregate_being_first()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>();

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate),
                                     new KeyValuePair<string, DbAggregate>(
                                         "max", maxFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project2].[C2] AS [C1], 
[Project2].[C1] AS [C2], 
[Project2].[C3] AS [C3], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel] AS [ReorderLevel]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	1 AS [C2], 
	[Project1].[ProductID] AS [ProductID], 
	[Project1].[ProductName] AS [ProductName], 
	[Project1].[ReorderLevel] AS [ReorderLevel], 
	[Project1].[Discontinued] AS [Discontinued], 
	[Project1].[C1] AS [C3]
	FROM   (SELECT 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1) ) AS [GroupBy1]
	LEFT OUTER JOIN  (SELECT 
		[Extent2].[ProductID] AS [ProductID], 
		[Extent2].[ProductName] AS [ProductName], 
		[Extent2].[ReorderLevel] AS [ReorderLevel], 
		[Extent2].[Discontinued] AS [Discontinued], 
		1 AS [C1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE [Extent2].[Discontinued] IN (0,1) ) AS [Project1] ON 1 = 1
)  AS [Project2]
ORDER BY [Project2].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_with_function_aggregate_and_group_aggregate_being_second()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>();

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>(
                                         "max", maxFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate),
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project2].[C2] AS [C1], 
[Project2].[C1] AS [C2], 
[Project2].[C3] AS [C3], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel] AS [ReorderLevel]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	1 AS [C2], 
	[Project1].[ProductID] AS [ProductID], 
	[Project1].[ProductName] AS [ProductName], 
	[Project1].[ReorderLevel] AS [ReorderLevel], 
	[Project1].[Discontinued] AS [Discontinued], 
	[Project1].[C1] AS [C3]
	FROM   (SELECT 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1) ) AS [GroupBy1]
	LEFT OUTER JOIN  (SELECT 
		[Extent2].[ProductID] AS [ProductID], 
		[Extent2].[ProductName] AS [ProductName], 
		[Extent2].[ReorderLevel] AS [ReorderLevel], 
		[Extent2].[Discontinued] AS [Discontinued], 
		1 AS [C1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE [Extent2].[Discontinued] IN (0,1) ) AS [Project1] ON 1 = 1
)  AS [Project2]
ORDER BY [Project2].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_with_two_function_aggregates_and_group_aggregate_being_in_the_middle()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>();

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var minFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Min" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>(
                                         "max", maxFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate),
                                     new KeyValuePair<string, DbAggregate>(
                                         "min", minFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project2].[C3] AS [C1], 
[Project2].[C1] AS [C2], 
[Project2].[C2] AS [C3], 
[Project2].[C4] AS [C4], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel] AS [ReorderLevel]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[A2] AS [C2], 
	1 AS [C3], 
	[Project1].[ProductID] AS [ProductID], 
	[Project1].[ProductName] AS [ProductName], 
	[Project1].[ReorderLevel] AS [ReorderLevel], 
	[Project1].[Discontinued] AS [Discontinued], 
	[Project1].[C1] AS [C4]
	FROM   (SELECT 
		MAX([Extent1].[ProductID]) AS [A1], 
		MIN([Extent1].[ProductID]) AS [A2]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1) ) AS [GroupBy1]
	LEFT OUTER JOIN  (SELECT 
		[Extent2].[ProductID] AS [ProductID], 
		[Extent2].[ProductName] AS [ProductName], 
		[Extent2].[ReorderLevel] AS [ReorderLevel], 
		[Extent2].[Discontinued] AS [Discontinued], 
		1 AS [C1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE [Extent2].[Discontinued] IN (0,1) ) AS [Project1] ON 1 = 1
)  AS [Project2]
ORDER BY [Project2].[C4] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_with_just_group_aggregate()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project2].[C1] AS [C1], 
[Project2].[C2] AS [C2], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel] AS [ReorderLevel]
FROM ( SELECT 
    1 AS [C1], 
    [Project1].[ProductID] AS [ProductID], 
    [Project1].[ProductName] AS [ProductName], 
    [Project1].[ReorderLevel] AS [ReorderLevel], 
    [Project1].[Discontinued] AS [Discontinued], 
    [Project1].[C1] AS [C2]
    FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
    LEFT OUTER JOIN  (SELECT 
        [Extent1].[ProductID] AS [ProductID], 
        [Extent1].[ProductName] AS [ProductName], 
        [Extent1].[ReorderLevel] AS [ReorderLevel], 
        [Extent1].[Discontinued] AS [Discontinued], 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent1]
        WHERE [Extent1].[Discontinued] IN (0,1) ) AS [Project1] ON 1 = 1
)  AS [Project2]
ORDER BY [Project2].[C2] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        private DbGroupByExpression CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable.Property("ReorderLevel"))
                           };

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>(
                                         "max", maxFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };

            return groupBinding.GroupBy(keys, aggregates);
        }

        [Fact]
        public void GropuBy_with_group_key_function_aggregate_and_group_aggregate()
        {
            var query = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var expectedSql =
                @"
SELECT 
[Project1].[C2] AS [C1], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[Discontinued] AS [Discontinued], 
[Project1].[ProductID] AS [ProductID], 
[Project1].[ProductName] AS [ProductName], 
[Project1].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	1 AS [C2], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C3]
	FROM   (SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel] ) AS [GroupBy1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project1]
ORDER BY [Project1].[ReorderLevel] ASC, [Project1].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Project_over_group_by_with_group_key_function_aggregate_and_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>("max", groupByBinding.Variable.Property("max")),
                                      new KeyValuePair<string, DbExpression>(
                                          "groupPartition", groupByBinding.Variable.Property("groupPartition")),
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
[Project1].[C2] AS [C1], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[Discontinued] AS [Discontinued], 
[Project1].[ProductID] AS [ProductID], 
[Project1].[ProductName] AS [ProductName], 
[Project1].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	1 AS [C2], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C3]
	FROM   (SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel] ) AS [GroupBy1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project1]
ORDER BY [Project1].[ReorderLevel] ASC, [Project1].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Project_over_group_by_skip_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>("max", groupByBinding.Variable.Property("max")),
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ReorderLevel], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[ReorderLevel] AS [K1], 
	MAX([Extent1].[ProductID]) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ReorderLevel]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Project_over_group_by_only_project_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>(
                                          "groupPartition", groupByBinding.Variable.Property("groupPartition")),
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
[Project2].[ReorderLevel] AS [ReorderLevel], 
[Project2].[C1] AS [C1], 
[Project2].[C2] AS [C2], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[Distinct1].[ReorderLevel] AS [ReorderLevel], 
	1 AS [C1], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C2]
	FROM   (SELECT DISTINCT 
		[Extent1].[ReorderLevel] AS [ReorderLevel]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1) ) AS [Distinct1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([Distinct1].[ReorderLevel] = [Extent2].[ReorderLevel]) OR (([Distinct1].[ReorderLevel] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project2]
ORDER BY [Project2].[ReorderLevel] ASC, [Project2].[C2] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_over_GroupBy()
        {
            var innerGroupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var outerGroupBinding = innerGroupBy.GroupBindAs("input2", "group2");
            var outerKeys = new List<KeyValuePair<string, DbExpression>>
                                {
                                    new KeyValuePair<string, DbExpression>("groupKey2", outerGroupBinding.Variable.Property("max"))
                                };

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var outerAggregates = new List<KeyValuePair<string, DbAggregate>>
                                      {
                                          new KeyValuePair<string, DbAggregate>("groupPartition2", outerGroupBinding.GroupAggregate),
                                          new KeyValuePair<string, DbAggregate>(
                                              "max2", maxFunction.Aggregate(outerGroupBinding.GroupVariable.Property("groupKey"))),
                                      };

            var query = outerGroupBinding.GroupBy(outerKeys, outerAggregates);

            var expectedSql =
                @"SELECT 
[Project2].[C3] AS [C1], 
[Project2].[C1] AS [C2], 
[Project2].[C2] AS [C3], 
[Project2].[C6] AS [C4], 
[Project2].[ReorderLevel] AS [ReorderLevel], 
[Project2].[C4] AS [C5], 
[Project2].[C5] AS [C6], 
[Project2].[Discontinued] AS [Discontinued], 
[Project2].[ProductID] AS [ProductID], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[GroupBy2].[K1] AS [C1], 
	[GroupBy2].[A1] AS [C2], 
	1 AS [C3], 
	[Project1].[ReorderLevel] AS [ReorderLevel], 
	[Project1].[C1] AS [C4], 
	[Project1].[ProductID] AS [ProductID], 
	[Project1].[ProductName] AS [ProductName], 
	[Project1].[ReorderLevel1] AS [ReorderLevel1], 
	[Project1].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Project1].[C2] IS NULL) THEN CAST(NULL AS int) WHEN ([Project1].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C5], 
	[Project1].[C2] AS [C6]
	FROM   (SELECT 
		[GroupBy1].[A1] AS [K1], 
		MAX([GroupBy1].[K1]) AS [A1]
		FROM ( SELECT 
			[Extent1].[ReorderLevel] AS [K1], 
			MAX([Extent1].[ProductID]) AS [A1]
			FROM [dbo].[Products] AS [Extent1]
			WHERE [Extent1].[Discontinued] IN (0,1)
			GROUP BY [Extent1].[ReorderLevel]
		)  AS [GroupBy1]
		GROUP BY [GroupBy1].[A1] ) AS [GroupBy2]
	LEFT OUTER JOIN  (SELECT 
		[GroupBy3].[K1] AS [ReorderLevel], 
		[GroupBy3].[A1] AS [C1], 
		[Extent3].[ProductID] AS [ProductID], 
		[Extent3].[ProductName] AS [ProductName], 
		[Extent3].[ReorderLevel] AS [ReorderLevel1], 
		[Extent3].[Discontinued] AS [Discontinued], 
		1 AS [C2]
		FROM   (SELECT 
			[Extent2].[ReorderLevel] AS [K1], 
			MAX([Extent2].[ProductID]) AS [A1]
			FROM [dbo].[Products] AS [Extent2]
			WHERE [Extent2].[Discontinued] IN (0,1)
			GROUP BY [Extent2].[ReorderLevel] ) AS [GroupBy3]
		LEFT OUTER JOIN [dbo].[Products] AS [Extent3] ON ([Extent3].[Discontinued] IN (0,1)) AND (([GroupBy3].[K1] = [Extent3].[ReorderLevel]) OR (([GroupBy3].[K1] IS NULL) AND ([Extent3].[ReorderLevel] IS NULL))) ) AS [Project1] ON ([GroupBy2].[K1] = [Project1].[C1]) OR (([GroupBy2].[K1] IS NULL) AND ([Project1].[C1] IS NULL))
)  AS [Project2]
ORDER BY [Project2].[C1] ASC, [Project2].[C6] ASC, [Project2].[ReorderLevel] ASC, [Project2].[C5] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_over_union()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable.Property("ReorderLevel"))
                           };

            var maxFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Max" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate),
                                     new KeyValuePair<string, DbAggregate>(
                                         "max", maxFunction.Aggregate(groupBinding.GroupVariable.Property("ProductID"))),
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project1].[C2] AS [C1], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[Discontinued] AS [Discontinued], 
[Project1].[ProductID] AS [ProductID], 
[Project1].[ProductName] AS [ProductName], 
[Project1].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	1 AS [C2], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C3]
	FROM   (SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel] ) AS [GroupBy1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project1]
ORDER BY [Project1].[ReorderLevel] ASC, [Project1].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_with_nulls()
        {
            DbExpression one = DbExpressionBuilder.Constant(1);
            DbExpression intNull = one.ResultType.Null();

            var input = DbExpressionBuilder.NewCollection(new[] { 1, 2, 3, intNull, intNull, 2, 1, intNull });

            var groupBinding = input.GroupBindAs("input", "group");
            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable)
                           };
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };
            var query = groupBinding.GroupBy(keys, aggregates);

            var expectedSql =
                @"SELECT 
[Project30].[C2] AS [C1], 
[Project30].[C1] AS [C2], 
[Project30].[C4] AS [C3], 
[Project30].[C3] AS [C4]
FROM ( SELECT 
	[Distinct1].[C1] AS [C1], 
	1 AS [C2], 
	CASE WHEN ([UnionAll14].[C1] IS NULL) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 0) THEN cast(1 as tinyint) WHEN ([UnionAll14].[C1] = 1) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 2) THEN cast(3 as tinyint) WHEN ([UnionAll14].[C1] = 3) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 4) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 5) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 6) THEN cast(1 as tinyint) END AS [C3], 
	CASE WHEN ([UnionAll14].[C1] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C4]
	FROM   (SELECT DISTINCT 
		CASE WHEN ([UnionAll7].[C1] = 0) THEN cast(1 as tinyint) WHEN ([UnionAll7].[C1] = 1) THEN cast(2 as tinyint) WHEN ([UnionAll7].[C1] = 2) THEN cast(3 as tinyint) WHEN ([UnionAll7].[C1] = 3) THEN CAST(NULL AS int) WHEN ([UnionAll7].[C1] = 4) THEN CAST(NULL AS int) WHEN ([UnionAll7].[C1] = 5) THEN cast(2 as tinyint) WHEN ([UnionAll7].[C1] = 6) THEN cast(1 as tinyint) END AS [C1]
		FROM  (SELECT 
			[UnionAll6].[C1] AS [C1]
			FROM  (SELECT 
				[UnionAll5].[C1] AS [C1]
				FROM  (SELECT 
					[UnionAll4].[C1] AS [C1]
					FROM  (SELECT 
						[UnionAll3].[C1] AS [C1]
						FROM  (SELECT 
							[UnionAll2].[C1] AS [C1]
							FROM  (SELECT 
								[UnionAll1].[C1] AS [C1]
								FROM  (SELECT 
									0 AS [C1]
									FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
								UNION ALL
									SELECT 
									1 AS [C1]
									FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]) AS [UnionAll1]
							UNION ALL
								SELECT 
								2 AS [C1]
								FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]) AS [UnionAll2]
						UNION ALL
							SELECT 
							3 AS [C1]
							FROM  ( SELECT 1 AS X ) AS [SingleRowTable4]) AS [UnionAll3]
					UNION ALL
						SELECT 
						4 AS [C1]
						FROM  ( SELECT 1 AS X ) AS [SingleRowTable5]) AS [UnionAll4]
				UNION ALL
					SELECT 
					5 AS [C1]
					FROM  ( SELECT 1 AS X ) AS [SingleRowTable6]) AS [UnionAll5]
			UNION ALL
				SELECT 
				6 AS [C1]
				FROM  ( SELECT 1 AS X ) AS [SingleRowTable7]) AS [UnionAll6]
		UNION ALL
			SELECT 
			7 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable8]) AS [UnionAll7] ) AS [Distinct1]
	LEFT OUTER JOIN  (SELECT 
		[UnionAll13].[C1] AS [C1]
		FROM  (SELECT 
			[UnionAll12].[C1] AS [C1]
			FROM  (SELECT 
				[UnionAll11].[C1] AS [C1]
				FROM  (SELECT 
					[UnionAll10].[C1] AS [C1]
					FROM  (SELECT 
						[UnionAll9].[C1] AS [C1]
						FROM  (SELECT 
							[UnionAll8].[C1] AS [C1]
							FROM  (SELECT 
								0 AS [C1]
								FROM  ( SELECT 1 AS X ) AS [SingleRowTable9]
							UNION ALL
								SELECT 
								1 AS [C1]
								FROM  ( SELECT 1 AS X ) AS [SingleRowTable10]) AS [UnionAll8]
						UNION ALL
							SELECT 
							2 AS [C1]
							FROM  ( SELECT 1 AS X ) AS [SingleRowTable11]) AS [UnionAll9]
					UNION ALL
						SELECT 
						3 AS [C1]
						FROM  ( SELECT 1 AS X ) AS [SingleRowTable12]) AS [UnionAll10]
				UNION ALL
					SELECT 
					4 AS [C1]
					FROM  ( SELECT 1 AS X ) AS [SingleRowTable13]) AS [UnionAll11]
			UNION ALL
				SELECT 
				5 AS [C1]
				FROM  ( SELECT 1 AS X ) AS [SingleRowTable14]) AS [UnionAll12]
		UNION ALL
			SELECT 
			6 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable15]) AS [UnionAll13]
	UNION ALL
		SELECT 
		7 AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable16]) AS [UnionAll14] ON ([Distinct1].[C1] = (CASE WHEN ([UnionAll14].[C1] = 0) THEN cast(1 as tinyint) WHEN ([UnionAll14].[C1] = 1) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 2) THEN cast(3 as tinyint) WHEN ([UnionAll14].[C1] = 3) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 4) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 5) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 6) THEN cast(1 as tinyint) END)) OR (([Distinct1].[C1] IS NULL) AND (CASE WHEN ([UnionAll14].[C1] = 0) THEN cast(1 as tinyint) WHEN ([UnionAll14].[C1] = 1) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 2) THEN cast(3 as tinyint) WHEN ([UnionAll14].[C1] = 3) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 4) THEN CAST(NULL AS int) WHEN ([UnionAll14].[C1] = 5) THEN cast(2 as tinyint) WHEN ([UnionAll14].[C1] = 6) THEN cast(1 as tinyint) END IS NULL))
)  AS [Project30]
ORDER BY [Project30].[C1] ASC, [Project30].[C4] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_on_entity()
        {
            var groupBinding = CreateBasicGroupBinding();
            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable)
                           };
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project1].[ProductID] AS [ProductID], 
[Project1].[Discontinued] AS [Discontinued], 
[Project1].[ProductName] AS [ProductName], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C1], 
[Project1].[Discontinued1] AS [Discontinued1], 
[Project1].[ProductID1] AS [ProductID1], 
[Project1].[ProductName1] AS [ProductName1], 
[Project1].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[Extent1].[ProductID] AS [ProductID], 
	[Extent1].[ProductName] AS [ProductName], 
	[Extent1].[ReorderLevel] AS [ReorderLevel], 
	[Extent1].[Discontinued] AS [Discontinued], 
	[Extent2].[ProductID] AS [ProductID1], 
	[Extent2].[ProductName] AS [ProductName1], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued1], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
	FROM  [dbo].[Products] AS [Extent1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([Extent1].[ProductID] = [Extent2].[ProductID]) OR (([Extent1].[Discontinued] IS NULL) AND ([Extent2].[Discontinued] IS NULL)))
	WHERE [Extent1].[Discontinued] IN (0,1)
)  AS [Project1]
ORDER BY [Project1].[Discontinued] ASC, [Project1].[ProductID] ASC, [Project1].[ProductName] ASC, [Project1].[ReorderLevel] ASC, [Project1].[C1] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void GroupBy_row()
        {
            DbExpression one = DbExpressionBuilder.Constant(1);
            DbExpression intNull = one.ResultType.Null();

            var rows = new DbExpression[3]
                           {
                               DbExpressionBuilder.NewRow(
                                   new List<KeyValuePair<string, DbExpression>>
                                       {
                                           new KeyValuePair<string, DbExpression>("x", one),
                                           new KeyValuePair<string, DbExpression>("y", one),
                                       }),
                               DbExpressionBuilder.NewRow(
                                   new List<KeyValuePair<string, DbExpression>>
                                       {
                                           new KeyValuePair<string, DbExpression>("x", one),
                                           new KeyValuePair<string, DbExpression>("y", intNull),
                                       }),
                               DbExpressionBuilder.NewRow(
                                   new List<KeyValuePair<string, DbExpression>>
                                       {
                                           new KeyValuePair<string, DbExpression>("x", intNull),
                                           new KeyValuePair<string, DbExpression>("y", intNull),
                                       }),
                           };

            var input = DbExpressionBuilder.NewCollection(rows);
            var groupBinding = input.GroupBindAs("input", "group");

            var keys = new List<KeyValuePair<string, DbExpression>>
                           {
                               new KeyValuePair<string, DbExpression>("groupKey", groupBinding.Variable)
                           };
            var aggregates = new List<KeyValuePair<string, DbAggregate>>
                                 {
                                     new KeyValuePair<string, DbAggregate>("groupPartition", groupBinding.GroupAggregate)
                                 };

            var query = groupBinding.GroupBy(keys, aggregates);
            var expectedSql =
                @"SELECT 
[Project10].[C1] AS [C1], 
[Project10].[C2] AS [C2], 
[Project10].[C3] AS [C3], 
[Project10].[C7] AS [C4], 
[Project10].[C4] AS [C5], 
[Project10].[C5] AS [C6], 
[Project10].[C6] AS [C7]
FROM ( SELECT 
	[Distinct1].[C1] AS [C1], 
	[Distinct1].[C2] AS [C2], 
	[Distinct1].[C3] AS [C3], 
	[UnionAll4].[C1] AS [C4], 
	CASE WHEN ([UnionAll4].[C1] IS NULL) THEN CAST(NULL AS int) WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN 1 END AS [C5], 
	CASE WHEN ([UnionAll4].[C1] IS NULL) THEN CAST(NULL AS int) WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN CAST(NULL AS int) END AS [C6], 
	CASE WHEN ([UnionAll4].[C1] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C7]
	FROM   (SELECT DISTINCT 
		1 AS [C1], 
		CASE WHEN ([UnionAll2].[C1] = 0) THEN 1 WHEN ([UnionAll2].[C1] = 1) THEN 1 END AS [C2], 
		CASE WHEN ([UnionAll2].[C1] = 0) THEN 1 WHEN ([UnionAll2].[C1] = 1) THEN CAST(NULL AS int) END AS [C3]
		FROM  (SELECT 
			[UnionAll1].[C1] AS [C1]
			FROM  (SELECT 
				0 AS [C1]
				FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
			UNION ALL
				SELECT 
				1 AS [C1]
				FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]) AS [UnionAll1]
		UNION ALL
			SELECT 
			2 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]) AS [UnionAll2] ) AS [Distinct1]
	LEFT OUTER JOIN  (SELECT 
		[UnionAll3].[C1] AS [C1]
		FROM  (SELECT 
			0 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable4]
		UNION ALL
			SELECT 
			1 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable5]) AS [UnionAll3]
	UNION ALL
		SELECT 
		2 AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable6]) AS [UnionAll4] ON (([Distinct1].[C2] = (CASE WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN 1 END)) OR (([Distinct1].[C2] IS NULL) AND (CASE WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN 1 END IS NULL))) AND (([Distinct1].[C3] = (CASE WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN CAST(NULL AS int) END)) OR (([Distinct1].[C3] IS NULL) AND (CASE WHEN ([UnionAll4].[C1] = 0) THEN 1 WHEN ([UnionAll4].[C1] = 1) THEN CAST(NULL AS int) END IS NULL)))
)  AS [Project10]
ORDER BY [Project10].[C1] ASC, [Project10].[C2] ASC, [Project10].[C3] ASC, [Project10].[C7] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_Group_aggregate_with_group_key_and_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>(
                                          "groupPartition", groupByBinding.Variable.Property("groupPartition")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              groupByBinding.Variable.Property("groupPartition").Select(p => p.Property("ProductID"))))
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
[Project1].[C2] AS [C1], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[Discontinued] AS [Discontinued], 
[Project1].[ProductID] AS [ProductID], 
[Project1].[ProductName] AS [ProductName], 
[Project1].[ReorderLevel1] AS [ReorderLevel1]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	1 AS [C2], 
	[Extent2].[ProductID] AS [ProductID], 
	[Extent2].[ProductName] AS [ProductName], 
	[Extent2].[ReorderLevel] AS [ReorderLevel1], 
	[Extent2].[Discontinued] AS [Discontinued], 
	CASE WHEN ([Extent2].[ProductID] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C3]
	FROM   (SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		COUNT([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel] ) AS [GroupBy1]
	LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [Project1]
ORDER BY [Project1].[ReorderLevel] ASC, [Project1].[C3] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_Group_aggregate_with_group_key_and_no_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              groupByBinding.Variable.Property("groupPartition").Select(p => p.Property("ProductID"))))
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ReorderLevel], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[ReorderLevel] AS [K1], 
	COUNT([Extent1].[ProductID]) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ReorderLevel]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_filter_over_group_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var groupByBinding = groupBy.BindAs("groupBy");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              groupByBinding.Variable.Property("groupPartition").Where(
                                                  f => f.Property("ProductID").LessThan(3)).Select(p => p.Property("ProductID")))),
                                  };

            var query = groupByBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[Project2].[ReorderLevel] AS [ReorderLevel], 
[Project2].[C1] AS [C2]
FROM ( SELECT 
	[Distinct1].[ReorderLevel] AS [ReorderLevel], 
	(SELECT 
		COUNT([Extent2].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE ([Extent2].[Discontinued] IN (0,1)) AND (([Distinct1].[ReorderLevel] = [Extent2].[ReorderLevel]) OR (([Distinct1].[ReorderLevel] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL))) AND ([Extent2].[ProductID] < cast(3 as tinyint))) AS [C1]
	FROM ( SELECT DISTINCT 
		[Extent1].[ReorderLevel] AS [ReorderLevel]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
	)  AS [Distinct1]
)  AS [Project2]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_project_filtered_group_aggregate_also_include_group_key_and_function_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();
            var filteredGroupAggregateProjection = groupBy.Select(
                p => new
                         {
                             groupKey = p.Property("groupKey"),
                             max = p.Property("max"),
                             groupPartition = p.Property("groupPartition").Where(g => g.Property("ProductID").LessThan(3)),
                         });

            var filteredGroupAggregateProjectionBinding = filteredGroupAggregateProjection.BindAs("groupBy");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>(
                                          "groupKey", filteredGroupAggregateProjectionBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>(
                                          "max", filteredGroupAggregateProjectionBinding.Variable.Property("max")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              filteredGroupAggregateProjectionBinding.Variable.Property("groupPartition").Select(
                                                  p => p.Property("ProductID")))),
                                  };

            var query = filteredGroupAggregateProjectionBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[Project1].[ReorderLevel] AS [ReorderLevel], 
[Project1].[C1] AS [C2], 
[Project1].[C2] AS [C3]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	(SELECT 
		COUNT([Extent2].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL))) AND ([Extent2].[ProductID] < cast(3 as tinyint))) AS [C2]
	FROM ( SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel]
	)  AS [GroupBy1]
)  AS [Project1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_project_unioned_group_aggregate_also_include_group_key_and_function_aggregate()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var unionedGroupAggregateProjection = groupBy.Select(
                p => new
                         {
                             groupKey = p.Property("groupKey"),
                             max = p.Property("max"),
                             groupPartition = p.Property("groupPartition").UnionAll(p.Property("groupPartition")),
                         });

            var unionedGroupAggregateProjectionBinding = unionedGroupAggregateProjection.BindAs("groupBy");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>(
                                          "groupKey", unionedGroupAggregateProjectionBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>(
                                          "max", unionedGroupAggregateProjectionBinding.Variable.Property("max")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              unionedGroupAggregateProjectionBinding.Variable.Property("groupPartition").Select(
                                                  p => p.Property("ProductID")))),
                                  };

            var query = unionedGroupAggregateProjectionBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[Project3].[ReorderLevel] AS [ReorderLevel], 
[Project3].[C1] AS [C2], 
[Project3].[C2] AS [C3]
FROM ( SELECT 
	[GroupBy1].[A1] AS [C1], 
	[GroupBy1].[K1] AS [ReorderLevel], 
	(SELECT 
		COUNT([UnionAll1].[ProductID]) AS [A1]
		FROM  (SELECT 
			[Extent2].[ProductID] AS [ProductID]
			FROM [dbo].[Products] AS [Extent2]
			WHERE ([Extent2].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent2].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
		UNION ALL
			SELECT 
			[Extent3].[ProductID] AS [ProductID]
			FROM [dbo].[Products] AS [Extent3]
			WHERE ([Extent3].[Discontinued] IN (0,1)) AND (([GroupBy1].[K1] = [Extent3].[ReorderLevel]) OR (([GroupBy1].[K1] IS NULL) AND ([Extent3].[ReorderLevel] IS NULL)))) AS [UnionAll1]) AS [C2]
	FROM ( SELECT 
		[Extent1].[ReorderLevel] AS [K1], 
		MAX([Extent1].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
		GROUP BY [Extent1].[ReorderLevel]
	)  AS [GroupBy1]
)  AS [Project3]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_group_aggregate_propagated_through_Apply()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();

            var query = groupBy.OuterApply(c => new KeyValuePair<string, DbExpression>("applyColumn", groupBy)).
                Select(a => countFunction.Invoke(a.Property("c").Property("groupPartition").Select(p => p.Property("ProductID"))));

            var expectedSql =
                @"SELECT 
(SELECT 
	COUNT([Extent3].[ProductID]) AS [A1]
	FROM [dbo].[Products] AS [Extent3]
	WHERE ([Extent3].[Discontinued] IN (0,1)) AND (([Distinct1].[ReorderLevel] = [Extent3].[ReorderLevel]) OR (([Distinct1].[ReorderLevel] IS NULL) AND ([Extent3].[ReorderLevel] IS NULL)))) AS [C1]
FROM   (SELECT DISTINCT 
	[Extent1].[ReorderLevel] AS [ReorderLevel]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1) ) AS [Distinct1]
LEFT OUTER JOIN  (SELECT DISTINCT 
	[Extent2].[ReorderLevel] AS [ReorderLevel]
	FROM [dbo].[Products] AS [Extent2]
	WHERE [Extent2].[Discontinued] IN (0,1) ) AS [Distinct2] ON 1 = 1";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_group_aggregate_propagated_throgh_Element()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();

            var query = countFunction.Invoke(groupBy.Element().Property("groupPartition").Select(p => p.Property("ProductID")));

            var expectedSql =
                @"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	COUNT([Extent2].[ProductID]) AS [A1]
	FROM   (SELECT TOP (1) 
		[Distinct1].[ReorderLevel] AS [ReorderLevel]
		FROM ( SELECT DISTINCT 
			[Extent1].[ReorderLevel] AS [ReorderLevel]
			FROM [dbo].[Products] AS [Extent1]
			WHERE [Extent1].[Discontinued] IN (0,1)
		)  AS [Distinct1] ) AS [Element1]
	INNER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discontinued] IN (0,1)) AND (([Element1].[ReorderLevel] = [Extent2].[ReorderLevel]) OR (([Element1].[ReorderLevel] IS NULL) AND ([Extent2].[ReorderLevel] IS NULL)))
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_group_aggregate_propagated_through_Limit()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();
            var groupByLimit = groupBy.Limit(2);

            var groupByLimitBinding = groupByLimit.BindAs("groupByLimit");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByLimitBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>("max", groupByLimitBinding.Variable.Property("max")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              groupByLimitBinding.Variable.Property("groupPartition").Select(p => p.Property("ProductID")))),
                                  };

            var query = groupByLimitBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[Limit1].[K1] AS [ReorderLevel], 
[Limit1].[A1] AS [C2], 
[Limit1].[A2] AS [C3]
FROM ( SELECT TOP (2) 
	[Extent1].[ReorderLevel] AS [K1], 
	MAX([Extent1].[ProductID]) AS [A1], 
	COUNT([Extent1].[ProductID]) AS [A2]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ReorderLevel]
)  AS [Limit1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Count_over_group_aggregate_propagated_through_Filter()
        {
            var groupBy = CreateGroupByWithGroupKeyFunctionAggregateAndGroupAggregate();
            var groupByFilter = groupBy.Where(c => c.Property("groupKey").LessThan(3));

            var groupByFilterBinding = groupByFilter.BindAs("groupByLimit");

            var countFunction =
                workspace.GetItems<EdmFunction>(DataSpace.CSpace).Where(
                    a => a.Name == "Count" && a.Parameters[0].TypeUsage.EdmType.Name.Contains("Int32")).First();
            var projections = new List<KeyValuePair<string, DbExpression>>
                                  {
                                      new KeyValuePair<string, DbExpression>("groupKey", groupByFilterBinding.Variable.Property("groupKey")),
                                      new KeyValuePair<string, DbExpression>("max", groupByFilterBinding.Variable.Property("max")),
                                      new KeyValuePair<string, DbExpression>(
                                          "count",
                                          countFunction.Invoke(
                                              groupByFilterBinding.Variable.Property("groupPartition").Select(p => p.Property("ProductID")))),
                                  };

            var query = groupByFilterBinding.Project(DbExpressionBuilder.NewRow(projections));

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ReorderLevel], 
[GroupBy1].[A1] AS [C2], 
[GroupBy1].[A2] AS [C3]
FROM ( SELECT 
	[Extent1].[ReorderLevel] AS [K1], 
	MAX([Extent1].[ProductID]) AS [A1], 
	COUNT([Extent1].[ProductID]) AS [A2]
	FROM [dbo].[Products] AS [Extent1]
	WHERE ([Extent1].[Discontinued] IN (0,1)) AND ([Extent1].[ReorderLevel] < cast(3 as tinyint))
	GROUP BY [Extent1].[ReorderLevel]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Using_model_defined_aggregate_function()
        {
            var query =
                @"select gkey, ProductModel.F_CountProducts(GroupPartition(P))
FROM ProductContainer.Products as P
Group By P.ProductName as gkey";

            var expectedSql =
                @"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ProductName], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[ProductName] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ProductName]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }
    }
}
