// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Linq;
    using Xunit;

    public class FilterOpRulesTests : FunctionalTestBase
    {
        public class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class BlogEntry
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        public class BlogContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<BlogEntry> BlogEntries { get; set; }

            static BlogContext()
            {
                Database.SetInitializer<BlogContext>(null);
            }
        }

        [Fact]
        public void Rule_FilterOverLeftOuterJoin_does_not_promote_to_InnerJoin_if_filter_predicate_is_expanded_in_null_semantics_phase()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    LEFT OUTER JOIN  (SELECT TOP (1) [c].[Name] AS [Name]
        FROM [dbo].[BlogEntries] AS [c] ) AS [Limit1] ON 1 = 1
    WHERE ([Extent1].[Name] = [Limit1].[Name]) OR (([Extent1].[Name] IS NULL) AND ([Limit1].[Name] IS NULL))";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = false;

                var query = from b in context.Blogs
                            where b.Name == context.BlogEntries.FirstOrDefault().Name
                            select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Rule_FilterOverLeftOuterJoin_promotes_to_InnerJoin_if_filter_predicate_is_not_expanded_in_null_semantics_phase()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    INNER JOIN  (SELECT TOP (1) [c].[Id] AS [Id]
        FROM [dbo].[BlogEntries] AS [c] ) AS [Limit1] ON [Extent1].[Id] = [Limit1].[Id]";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = false;

                var query = from b in context.Blogs
                            where b.Id == context.BlogEntries.FirstOrDefault().Id
                            select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Rule_FilterOverLeftOuterJoin_promotes_to_InnerJoin_if_using_database_null_semantics()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    INNER JOIN  (SELECT TOP (1) [c].[Name] AS [Name]
        FROM [dbo].[BlogEntries] AS [c] ) AS [Limit1] ON [Extent1].[Name] = [Limit1].[Name]";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = from b in context.Blogs
                            where b.Name == context.BlogEntries.FirstOrDefault().Name
                            select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Rule_FilterOverOuterApply_does_not_promote_to_CrossApply_if_filter_predicate_is_expanded_in_null_semantics_phase()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    OUTER APPLY  (SELECT TOP (1) [Extent2].[Name] AS [Name]
        FROM [dbo].[BlogEntries] AS [Extent2]
        WHERE ([Extent2].[Name] = [Extent1].[Name]) OR (([Extent2].[Name] IS NULL) AND ([Extent1].[Name] IS NULL)) ) AS [Limit1]
    WHERE ([Extent1].[Name] = [Limit1].[Name]) OR (([Extent1].[Name] IS NULL) AND ([Limit1].[Name] IS NULL))";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = false;

                var query = from b in context.Blogs
                    from e in context.BlogEntries.Where(e => e.Name == b.Name).Take(1).DefaultIfEmpty()
                    where b.Name == e.Name
                    select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Rule_FilterOverOuterApply_promotes_to_CrossApply_if_filter_predicate_is_not_expanded_in_null_semantics_phase()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    CROSS APPLY  (SELECT TOP (1) [Extent2].[Id] AS [Id]
        FROM [dbo].[BlogEntries] AS [Extent2]
        WHERE ([Extent2].[Name] = [Extent1].[Name]) OR (([Extent2].[Name] IS NULL) AND ([Extent1].[Name] IS NULL)) ) AS [Limit1]
    WHERE [Extent1].[Id] = [Limit1].[Id]";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = false;

                var query = from b in context.Blogs
                            from e in context.BlogEntries.Where(e => e.Name == b.Name).Take(1).DefaultIfEmpty()
                            where b.Id == e.Id
                            select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Rule_FilterOverOuterApply_promotes_to_CrossApply_if_using_database_null_semantics()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Blogs] AS [Extent1]
    CROSS APPLY  (SELECT TOP (1) [Extent2].[Name] AS [Name]
        FROM [dbo].[BlogEntries] AS [Extent2]
        WHERE [Extent2].[Name] = [Extent1].[Name] ) AS [Limit1]
    WHERE [Extent1].[Name] = [Limit1].[Name]";

            using (var context = new BlogContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = from b in context.Blogs
                            from e in context.BlogEntries.Where(e => e.Name == b.Name).Take(1).DefaultIfEmpty()
                            where b.Name == e.Name
                            select b;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}
