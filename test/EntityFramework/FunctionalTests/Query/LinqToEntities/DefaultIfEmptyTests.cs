// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;
    
    public class DefaultIfEmptyTests : FunctionalTestBase
    {
        [Fact]
        public void SelectMany_with_DefaultIfEmpty_translates_into_left_outer_join()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Extent2].[Discriminator] AS [Discriminator], 
[Extent2].[Id] AS [Id], 
[Extent2].[CategoryId] AS [CategoryId], 
[Extent2].[Name] AS [Name], 
[Extent2].[PromotionalCode] AS [PromotionalCode]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId])";

                var query = context.Categories.SelectMany(c => c.Products.DefaultIfEmpty());

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_null_default()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
[Extent1].[Id] AS [Id], 
[Extent1].[DetailedDescription] AS [DetailedDescription], 
[Extent2].[Discriminator] AS [Discriminator], 
[Extent2].[Id] AS [Id1], 
[Extent2].[CategoryId] AS [CategoryId], 
[Extent2].[Name] AS [Name], 
[Extent2].[PromotionalCode] AS [PromotionalCode]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND (1 = [Extent2].[Id])";

                var query = context.Categories.SelectMany(c => c.Products.Where(p => p.Id == 1).DefaultIfEmpty(), (c, p) => new { c, p });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_non_null_default()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
[Extent1].[Id] AS [Id], 
[Extent1].[DetailedDescription] AS [DetailedDescription], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN 0 ELSE [Extent2].[Id] END AS [C2]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND (1 = [Extent2].[Id])";

                var query = context.Categories.SelectMany(c => c.Products.Where(p => p.Id == 1).Select(p => p.Id).DefaultIfEmpty(), (c, p) => new { c, p });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_explicit_default()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
[Extent1].[Id] AS [Id], 
[Extent1].[DetailedDescription] AS [DetailedDescription], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Foo' ELSE [Extent2].[Name] END AS [C2]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND (1 = [Extent2].[Id])";

                var query = context.Categories.SelectMany(c => c.Products.Where(p => p.Id == 1).Select(p => p.Name).DefaultIfEmpty("Foo"), (c, p) => new { c, p });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_anonymous_type_default()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
[Extent1].[Id] AS [Id], 
[Extent1].[DetailedDescription] AS [DetailedDescription], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN -1 ELSE [Extent2].[Id] END AS [C2], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Unknown' ELSE [Extent2].[Name] END AS [C3], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Extent2].[CategoryId] END AS [C4]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND ([Extent2].[Id] < 5)";

                var query = context.Categories.SelectMany(c => c.Products.Where(p => p.Id < 5).Select(p => new { p.Id, p.Name, p.CategoryId }).DefaultIfEmpty(new { Id = -1, Name = "Unknown", CategoryId = (string)null }), (c, p) => new { c, p });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void SelectMany_of_two_entity_sets_withDefaultIfEmpty_translated_to_left_outer_join()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Extent2].[Discriminator] AS [Discriminator], 
[Extent2].[Id] AS [Id], 
[Extent2].[CategoryId] AS [CategoryId], 
[Extent2].[Name] AS [Name], 
[Extent2].[PromotionalCode] AS [PromotionalCode]
FROM  [dbo].[Categories] AS [Extent1]
LEFT OUTER JOIN [dbo].[Products] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId])";

                var query = context.Categories.GroupJoin(context.Products, c => c.Id, p => p.CategoryId, (c, p) => new { c, p }).SelectMany(r => r.p.DefaultIfEmpty());

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}
