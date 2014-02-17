// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Linq;
    using Xunit;

    public class PredicateScalarizationTests : FunctionalTestBase
    {
        [Fact]
        public void Null_comparison_is_converted_to_CASE_with_single_ISNULL_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Products.Select(p => p.Name == null);

                const string expectedSql =
@"SELECT 
    CASE WHEN ([Extent1].[Name] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Products] AS [Extent1]
    WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Negated_null_comparison_is_converted_to_CASE_with_single_ISNULL_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Products.Select(p => p.Name != null);

                const string expectedSql =
@"SELECT 
    CASE WHEN ([Extent1].[Name] IS NOT NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Products] AS [Extent1]
    WHERE [Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Any_without_predicate_is_converted_to_CASE_with_single_EXISTS_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => c.Products.Any());

                const string expectedSql =
@"SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId])
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Any_with_predicate_is_converted_to_CASE_with_single_EXISTS_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => c.Products.Any(p => p.Id > 10));

                const string expectedSql = 
@"SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND ([Extent2].[Id] > 10)
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Negated_Any_is_converted_to_CASE_with_single_EXISTS_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => !c.Products.Any(p => p.Id > 10));

                const string expectedSql =
@"SELECT 
    CASE WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND ([Extent2].[Id] > 10)
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void All_is_converted_to_CASE_with_single_EXISTS_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => c.Products.All(p => p.Id > 10));

                const string expectedSql =
@"SELECT 
    CASE WHEN ( NOT EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND (( NOT ([Extent2].[Id] > 10)) OR (CASE WHEN ([Extent2].[Id] > 10) THEN cast(1 as bit) WHEN ( NOT ([Extent2].[Id] > 10)) THEN cast(0 as bit) END IS NULL))
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Negated_All_is_converted_to_CASE_with_single_EXISTS_statement()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => !c.Products.All(p => p.Id > 10));

                const string expectedSql =
@"SELECT 
    CASE WHEN ( EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND (( NOT ([Extent2].[Id] > 10)) OR (CASE WHEN ([Extent2].[Id] > 10) THEN cast(1 as bit) WHEN ( NOT ([Extent2].[Id] > 10)) THEN cast(0 as bit) END IS NULL))
    )) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Or_with_two_valued_logic_operands_is_converted_to_CASE_with_two_branches()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => c.Products.Any(p => p.Id > 10) || c.DetailedDescription == null);

                const string expectedSql =
@"SELECT 
    CASE WHEN (( EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND ([Extent2].[Id] > 10)
    )) OR ([Extent1].[DetailedDescription] IS NULL)) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void And_with_two_valued_logic_operands_is_converted_to_CASE_with_two_branches()
        {
            using (var context = new SimpleModelContext())
            {
                var actualSql = context.Categories.Select(c => c.Products.Any(p => p.Id > 10) && c.DetailedDescription == null);

                const string expectedSql =
@"SELECT 
    CASE WHEN (( EXISTS (SELECT 
        1 AS [C1]
        FROM [dbo].[Products] AS [Extent2]
        WHERE ([Extent2].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent1].[Id] = [Extent2].[CategoryId]) AND ([Extent2].[Id] > 10)
    )) AND ([Extent1].[DetailedDescription] IS NULL)) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C1]
    FROM [dbo].[Categories] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }
    }
}
