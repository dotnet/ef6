// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class OrderByLiftingTests : FunctionalTestBase
    {
        [Fact]
        public void OrderBy_ThenBy_lifted_above_projection()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 });
                var baseline = context.Products.Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 }).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_type_filter()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).OfType<FeaturedProduct>();
                var baseline = context.Products.OfType<FeaturedProduct>().OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_lifted_above_projection()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 });
                var baseline = context.Products.Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 }).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var expectedSq =
@"SELECT 
[Skip1].[Discriminator] AS [Discriminator], 
[Skip1].[Id] AS [Id], 
[Skip1].[CategoryId] AS [CategoryId], 
[Skip1].[Name] AS [Name], 
[Skip1].[PromotionalCode] AS [PromotionalCode]
FROM ( SELECT [Filter1].[Id] AS [Id], [Filter1].[CategoryId] AS [CategoryId], [Filter1].[Name] AS [Name], [Filter1].[PromotionalCode] AS [PromotionalCode], [Filter1].[Discriminator] AS [Discriminator]
	FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[CategoryId] AS [CategoryId], [Extent1].[Name] AS [Name], [Extent1].[PromotionalCode] AS [PromotionalCode], [Extent1].[Discriminator] AS [Discriminator], row_number() OVER (ORDER BY [Extent1].[Name] DESC, [Extent1].[CategoryId] ASC) AS [row_number]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	)  AS [Filter1]
	WHERE [Filter1].[row_number] > 5
)  AS [Skip1]
WHERE (0 = ([Skip1].[Id] % 2)) AND ([Skip1].[Id] % 2 IS NOT NULL)
ORDER BY [Skip1].[Name] DESC, [Skip1].[CategoryId] ASC";

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Where(p => p.Id % 2 == 0);

                QueryTestHelpers.VerifyDbQuery(query, expectedSq);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var expectedSq =
@"SELECT 
[Skip1].[Discriminator] AS [Discriminator], 
[Skip1].[Id] AS [Id], 
[Skip1].[CategoryId] AS [CategoryId], 
[Skip1].[Name] AS [Name], 
[Skip1].[PromotionalCode] AS [PromotionalCode]
FROM ( SELECT [Filter1].[Id] AS [Id], [Filter1].[CategoryId] AS [CategoryId], [Filter1].[Name] AS [Name], [Filter1].[PromotionalCode] AS [PromotionalCode], [Filter1].[Discriminator] AS [Discriminator]
	FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[CategoryId] AS [CategoryId], [Extent1].[Name] AS [Name], [Extent1].[PromotionalCode] AS [PromotionalCode], [Extent1].[Discriminator] AS [Discriminator], row_number() OVER (ORDER BY [Extent1].[Name] DESC, [Extent1].[CategoryId] ASC) AS [row_number]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
	)  AS [Filter1]
	WHERE [Filter1].[row_number] > 5
)  AS [Skip1]
WHERE 0 = ([Skip1].[Id] % 2)
ORDER BY [Skip1].[Name] DESC, [Skip1].[CategoryId] ASC";

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Where(p => p.Id % 2 == 0);

                QueryTestHelpers.VerifyDbQuery(query, expectedSq);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_lifted_above_type_filter()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Project1].[C1] AS [C1], 
[Project1].[C2] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[C4] AS [C4], 
[Project1].[C5] AS [C5]
FROM ( SELECT 
	[Skip1].[CategoryId] AS [CategoryId], 
	[Skip1].[Name] AS [Name], 
	CASE WHEN ([Skip1].[Discriminator] = N'FeaturedProduct') THEN [Skip1].[Discriminator] END AS [C1], 
	CASE WHEN ([Skip1].[Discriminator] = N'FeaturedProduct') THEN [Skip1].[Id] END AS [C2], 
	CASE WHEN ([Skip1].[Discriminator] = N'FeaturedProduct') THEN [Skip1].[CategoryId] END AS [C3], 
	CASE WHEN ([Skip1].[Discriminator] = N'FeaturedProduct') THEN [Skip1].[Name] END AS [C4], 
	CASE WHEN ([Skip1].[Discriminator] = N'FeaturedProduct') THEN [Skip1].[PromotionalCode] END AS [C5]
	FROM ( SELECT [Filter1].[Id] AS [Id], [Filter1].[CategoryId] AS [CategoryId], [Filter1].[Name] AS [Name], [Filter1].[PromotionalCode] AS [PromotionalCode], [Filter1].[Discriminator] AS [Discriminator]
		FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[CategoryId] AS [CategoryId], [Extent1].[Name] AS [Name], [Extent1].[PromotionalCode] AS [PromotionalCode], [Extent1].[Discriminator] AS [Discriminator], row_number() OVER (ORDER BY [Extent1].[Name] DESC, [Extent1].[CategoryId] ASC) AS [row_number]
			FROM [dbo].[Products] AS [Extent1]
			WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
		)  AS [Filter1]
		WHERE [Filter1].[row_number] > 5
	)  AS [Skip1]
	WHERE [Skip1].[Discriminator] = N'FeaturedProduct'
)  AS [Project1]
ORDER BY [Project1].[Name] DESC, [Project1].[CategoryId] ASC";
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).OfType<FeaturedProduct>();

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_projection()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 });
                var baseline = context.Products.Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 }).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_type_filter()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Project1].[C1] AS [C1], 
[Project1].[C2] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[C4] AS [C4], 
[Project1].[C5] AS [C5]
FROM ( SELECT 
	[Limit1].[CategoryId] AS [CategoryId], 
	[Limit1].[Name] AS [Name], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Discriminator] END AS [C1], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Id] END AS [C2], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[CategoryId] END AS [C3], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Name] END AS [C4], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[PromotionalCode] END AS [C5]
	FROM ( SELECT TOP (10) [Extent1].[Id] AS [Id], [Extent1].[CategoryId] AS [CategoryId], [Extent1].[Name] AS [Name], [Extent1].[PromotionalCode] AS [PromotionalCode], [Extent1].[Discriminator] AS [Discriminator]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
		ORDER BY [Extent1].[Name] DESC, [Extent1].[CategoryId] ASC
	)  AS [Limit1]
	WHERE [Limit1].[Discriminator] = N'FeaturedProduct'
)  AS [Project1]
ORDER BY [Project1].[Name] DESC, [Project1].[CategoryId] ASC";
                
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Take(10).OfType<FeaturedProduct>();

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_projection()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 });
                var baseline = context.Products.Select(p => new { p.Name, p.CategoryId, Foo = p.Id * 5 }).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId);

                Assert.Equal(baseline.ToString(), query.ToString());
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_type_filter()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Project1].[C1] AS [C1], 
[Project1].[C2] AS [C2], 
[Project1].[C3] AS [C3], 
[Project1].[C4] AS [C4], 
[Project1].[C5] AS [C5]
FROM ( SELECT 
	[Limit1].[CategoryId] AS [CategoryId], 
	[Limit1].[Name] AS [Name], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Discriminator] END AS [C1], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Id] END AS [C2], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[CategoryId] END AS [C3], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[Name] END AS [C4], 
	CASE WHEN ([Limit1].[Discriminator] = N'FeaturedProduct') THEN [Limit1].[PromotionalCode] END AS [C5]
	FROM ( SELECT TOP (10) [Filter1].[Id] AS [Id], [Filter1].[CategoryId] AS [CategoryId], [Filter1].[Name] AS [Name], [Filter1].[PromotionalCode] AS [PromotionalCode], [Filter1].[Discriminator] AS [Discriminator]
		FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[CategoryId] AS [CategoryId], [Extent1].[Name] AS [Name], [Extent1].[PromotionalCode] AS [PromotionalCode], [Extent1].[Discriminator] AS [Discriminator], row_number() OVER (ORDER BY [Extent1].[Name] DESC, [Extent1].[CategoryId] ASC) AS [row_number]
			FROM [dbo].[Products] AS [Extent1]
			WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')
		)  AS [Filter1]
		WHERE [Filter1].[row_number] > 5
		ORDER BY [Filter1].[Name] DESC, [Filter1].[CategoryId] ASC
	)  AS [Limit1]
	WHERE [Limit1].[Discriminator] = N'FeaturedProduct'
)  AS [Project1]
ORDER BY [Project1].[Name] DESC, [Project1].[CategoryId] ASC";

                var query = context.Products.OrderByDescending(p => p.Name).ThenBy(p => p.CategoryId).Skip(5).Take(10).OfType<FeaturedProduct>();

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}
