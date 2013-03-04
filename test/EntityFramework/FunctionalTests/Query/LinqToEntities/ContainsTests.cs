// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class EnumerableContainsTests : FunctionalTestBase
    {
        public enum Genre
        {
            Action,
            Humor,
            Fantasy,
        }

        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public Genre? Genre { get; set; }
            public AuthorName Author { get; set; }
        }

        public class AuthorName
        {
            public string First { get; set; }
            public string Last { get; set; }
        }

        public class UnicodeContext : DbContext
        {
            public DbSet<Book> Books { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Book>().Property(p => p.Title).IsUnicode(true);
            }
        }

        public class NonUnicodeContext : DbContext
        {
            public DbSet<Book> Books { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Book>().Property(p => p.Title).IsUnicode(false);
            }
        }

        [Fact]
        public void EnumerableContains_with_unicode_string_and_store_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE ([Extent1].[Title] IN (N'Title1', N'Title2')) 
    OR ([Extent1].[Title] IS NULL)";

            var array = new[] { "Title1", "Title2", null };

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = from book in context.Books
                            where array.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_unicode_string_and_csharp_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE (([Extent1].[Title] IN (N'Title1', N'Title2')) AND ([Extent1].[Title] IS NOT NULL)) 
    OR ([Extent1].[Title] IS NULL)";

            var array = new[] { "Title1", "Title2", null };

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = from book in context.Books
                            where array.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_non_unicode_string_and_store_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE ([Extent1].[Title] IN ('Title1', 'Title2')) 
    OR ([Extent1].[Title] IS NULL)";

            var array = new[] { "Title1", "Title2", null };

            using (var context = new NonUnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = from book in context.Books
                            where array.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_non_unicode_string_and_csharp_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE (([Extent1].[Title] IN ('Title1', 'Title2')) AND ([Extent1].[Title] IS NOT NULL)) 
    OR ([Extent1].[Title] IS NULL)";

            var array = new[] { "Title1", "Title2", null };

            using (var context = new NonUnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = from book in context.Books
                            where array.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_enum_and_store_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE ([Extent1].[Genre] IN (1,2)) 
    OR ([Extent1].[Genre] IS NULL)";

            var array = new[] { Genre.Humor, Genre.Fantasy, default(Genre?) };

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = from book in context.Books
                            where array.Contains(book.Genre)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_enum_and_csharp_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE (([Extent1].[Genre] IN (1,2)) AND ([Extent1].[Genre] IS NOT NULL))
    OR ([Extent1].[Genre] IS NULL)";

            var array = new[] { Genre.Humor, Genre.Fantasy, default(Genre?) };

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = from book in context.Books
                            where array.Contains(book.Genre)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_complex_type_throws_NotSupportedException()
        {
            var array = new[] { new AuthorName(), new AuthorName() };

            using (var context = new UnicodeContext())
            {
                var query = from book in context.Books
                            where array.Contains(book.Author)
                            select book.Id;

                Assert.Throws(typeof(NotSupportedException), () => query.ToString());
            }
        }

        [Fact]
        public void EnumerableContains_with_parameter_and_store_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE ([Extent1].[Title] IN (N'Title1', N'Title2')) 
    OR ([Extent1].[Title] = @p__linq__0) 
    OR ([Extent1].[Title] IS NULL)

/*
String p__linq__0 = ""Title3""
*/";

            var parameter = "Title3";

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = from book in context.Books
                            where new[] { "Title1", "Title2", parameter, null }.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void EnumerableContains_with_parameter_and_csharp_null_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Books] AS [Extent1]
WHERE (([Extent1].[Title] IN (N'Title1', N'Title2')) AND ([Extent1].[Title] IS NOT NULL))
    OR (([Extent1].[Title] = @p__linq__0) AND (NOT ([Extent1].[Title] IS NULL OR @p__linq__0 IS NULL))) 
    OR (([Extent1].[Title] IS NULL) AND (@p__linq__0 IS NULL)) 
    OR ([Extent1].[Title] IS NULL)

/*
String p__linq__0 = ""Title3""
*/";

            var parameter = "Title3";

            using (var context = new UnicodeContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = from book in context.Books
                            where new[] { "Title1", "Title2", parameter, null }.Contains(book.Title)
                            select book.Id;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Contains_throws_for_constant_null_array()
        {
            using (var context = new UnicodeContext())
            {
                string[] names = null;
                var query = context.Books.Where(b => names.Contains(b.Title));

                Assert.Throws<NotSupportedException>(() => query.ToString());
            }
        }

        [Fact]
        public void Contains_throws_for_constant_null_list()
        {
            using (var context = new UnicodeContext())
            {
                List<string> names = null;
                var query = context.Books.Where(b => names.Contains(b.Title));

                Assert.Throws<NotSupportedException>(() => query.ToString());
            }
        }

        [Fact]
        public void Contains_on_non_static_collection_of_enums()
        {
            const string expectedSql =
                @"SELECT 
CASE WHEN ( EXISTS (SELECT 
	1 AS [C1]
	FROM [dbo].[Books] AS [Extent2]
	WHERE 0 = [Extent2].[Genre]
)) THEN cast(1 as bit) WHEN ( NOT EXISTS (SELECT 
	1 AS [C1]
	FROM [dbo].[Books] AS [Extent3]
	WHERE 0 = [Extent3].[Genre]
)) THEN cast(0 as bit) END AS [C1]
FROM [dbo].[Books] AS [Extent1]";

            using (var context = new UnicodeContext())
            {
                var query = context.Books.Select(q => context.Books.Select(b => b.Genre).Contains(Genre.Action));

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}
