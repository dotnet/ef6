// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Text;
    using Xunit;

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

    public class EnumerableContainsTests
    {
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
        public static void EnumerableContains_with_unicode_string_and_store_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_unicode_string_and_csharp_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_non_unicode_string_and_store_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_non_unicode_string_and_csharp_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_enum_and_store_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_enum_and_csharp_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_complex_type_throws_NotSupportedException()
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
        public static void EnumerableContains_with_parameter_and_store_null_is_translated_to_expected_sql()
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

                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void EnumerableContains_with_parameter_and_csharp_null_is_translated_to_expected_sql()
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


                VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public static void DbExpressionBuilder_In_returns_false_constant_expression_for_empty_enumerable()
        {
            var expression = DbExpressionBuilder.Constant(0);
            var list = new List<DbConstantExpression>();
            var result = expression.In(list);

            Assert.Equal(DbExpressionBuilder.False, result);
        }

        [Fact]
        public static void DbExpressionBuilder_In_returns_correct_in_expression_for_non_empty_enumerable()
        {
            var expression = DbExpressionBuilder.Constant(3);
            var list = new List<DbConstantExpression>()
                           {
                               DbExpressionBuilder.Constant(0),
                               DbExpressionBuilder.Constant(1),
                               DbExpressionBuilder.Constant(2),
                           };
            var result = expression.In(list) as DbInExpression;

            Assert.NotEqual(null, result);
            Assert.Equal(expression, result.Item);
            Assert.Equal(list.Count, result.List.Count);

            for (var i = 0; i < list.Count; i++)
            {
                Assert.Equal(list[i], result.List[i]);
            }
        }

        [Fact]
        public static void DbExpressionBuilder_In_throws_argument_exception_for_input_expressions_with_different_result_types()
        {
            var list1 = new List<DbConstantExpression>()
                           {
                               DbExpressionBuilder.True,
                               DbExpressionBuilder.Constant(0)
                           };

            var list2 = new List<DbConstantExpression>()
                           {
                               DbExpressionBuilder.True,
                               DbExpressionBuilder.False
                           };

            Assert.Throws(typeof(ArgumentException), () => DbExpressionBuilder.False.In(list1));
            Assert.Throws(typeof(ArgumentException), () => DbExpressionBuilder.Constant(0).In(list2));
        }

        private static void VerifyQuery(object query, string expectedSql)
        {
            Assert.Equal(StripFormatting(expectedSql), StripFormatting(query.ToString()));
        }

        private static string StripFormatting(string str)
        {
            var sb = new StringBuilder(str.Length);

            foreach (var chr in str)
            {
                switch (chr)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        sb.Append(chr);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
