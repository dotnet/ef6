// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Linq;
    using Xunit;

    public class GroupByOptimizationTests
    {
        [Fact]
        public void GroupBy_is_optimized_when_projecting_group_key()
        {
            var expectedSql =
@"SELECT 
[Distinct1].[CategoryId] AS [CategoryId]
FROM ( SELECT DISTINCT 
	[Extent1].[CategoryId] AS [CategoryId]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
)  AS [Distinct1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => g.Key);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_group_count()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[CategoryId] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	GROUP BY [Extent1].[CategoryId]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => g.Count());

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_expression_containing_group_key()
        {
            var expectedSql =
@"SELECT 
[Extent1].[Id] * 2 AS [C1]
FROM [dbo].[Products] AS [Extent1]
WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.Id).Select(g => g.Key * 2);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_aggregate_on_the_group()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[CategoryId] AS [K1], 
	MAX([Extent1].[Id]) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	GROUP BY [Extent1].[CategoryId]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => g.Max(p => p.Id));

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[CategoryId] AS [K1], 
	MAX([Extent1].[Id]) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	GROUP BY [Extent1].[CategoryId]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => g.Max(p => p.Id));

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [CategoryId], 
[GroupBy1].[A1] AS [C2], 
[GroupBy1].[A2] AS [C3]
FROM ( SELECT 
	[Filter1].[K1] AS [K1], 
	MAX([Filter1].[A1]) AS [A1], 
	MIN([Filter1].[A2]) AS [A2]
	FROM ( SELECT 
		[Extent1].[CategoryId] AS [K1], 
		[Extent1].[Id] AS [A1], 
		[Extent1].[Id] + 2 AS [A2]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	)  AS [Filter1]
	GROUP BY [K1]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => new { key1 = g.Key, key2 = g.Key, max = g.Max(p => p.Id), min = g.Min(s => s.Id + 2) });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key()
        {
            var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
N'not null' AS [C1], 
CASE WHEN (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1))) THEN cast(1 as bit) WHEN ( NOT (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1)))) THEN cast(0 as bit) END AS [C2]
FROM [dbo].[Products] AS [Extent1]
WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')

/*
Boolean p__linq__0 = True
Boolean p__linq__1 = False
Boolean p__linq__2 = False
Boolean p__linq__3 = True
*/
";

            bool a = true;
            bool b = false;
            bool c = true;

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.Id).Select(g => new { keyIsNull = ((int?)g.Key == null) ? "is null" : "not null", logicExpression = (a && b || b && c) });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_filerting_and_projecting_anonymous_type_with_group_key_and_function_aggregate()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [Name], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[Name] AS [K1], 
	AVG( CAST( [Extent1].[Id] AS float)) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] > 5)
	GROUP BY [Extent1].[Name]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.Where(p => p.Id > 5).GroupBy(p => p.Name).Select(g => new { ProductName = g.Key, AveragePrice = g.Average(p => p.Id) });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_function_aggregate_with_expression()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Filter1].[K1] AS [K1], 
	MAX([Filter1].[A1]) AS [A1]
	FROM ( SELECT 
		[Extent1].[CategoryId] AS [K1], 
		[Extent1].[Id] * 2 AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	)  AS [Filter1]
	GROUP BY [K1]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => g.Max(p => p.Id * 2));

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[A1] - [GroupBy1].[A2] AS [C2]
FROM ( SELECT 
	[Extent1].[CategoryId] AS [K1], 
	MAX([Extent1].[Id]) AS [A1], 
	MIN([Extent1].[Id]) AS [A2]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	GROUP BY [Extent1].[CategoryId]
)  AS [GroupBy1]";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.GroupBy(p => p.CategoryId).Select(g => new { maxMinusMin = g.Max(p => p.Id) - g.Min(s => s.Id) });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row()
        {
            var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Products] AS [Extent1]
WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] < 4)";

            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.Where(p => p.Id < 4).GroupBy(g => new { g.Id }).Select(g => g.Key.Id);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}
