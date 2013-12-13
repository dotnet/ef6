// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;


    public class HasFlagTests : FunctionalTestBase
    {
        [Flags]
        public enum BlogType
        {
            None = 0x0,
            Favorite = 0x1,
            Online = 0x2,
            Important = 0x4,
        }

        public enum OtherEnum
        {
            None = 0x0,
            A = 0x1,
            B = 0x2,
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Name { get; set; }
            public BlogType BlogType { get; set; }
            public BlogType? NullableBlogType { get; set; }
        }

        public class BloggingContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
        }

        public class BloggingInitializer : DropCreateDatabaseAlways<BloggingContext>
        {
            protected override void Seed(BloggingContext context)
            {
                base.Seed(context);
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                for (int i = -1; i < 20; i++)
                {
                    var blog = new Blog{
                        BlogType = (BlogType)i,
                        Name = "Blog " + i,
                        NullableBlogType = null,
                    };
                    context.Blogs.Add(blog);
                }
            } 
        }



        [Fact]
        public void HasFlag_with_flag_enum_constant_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT[Extent1].[BlogId]AS[BlogId],[Extent1].[Name]AS[Name],[Extent1].[BlogType]AS[BlogType],[Extent1].[NullableBlogType]AS[NullableBlogType]FROM[dbo].[Blogs]AS[Extent1]WHERE((CAST([Extent1].[BlogType]ASint))&(CAST(1ASint)))=CAST(1ASint)";
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var context = new BloggingContext())
            {
                var query = from blog in context.Blogs
                            where blog.BlogType.HasFlag(BlogType.Favorite)
                            select blog;

                AssertCorrectData(context, query, blog => blog.BlogType.HasFlag(BlogType.Favorite));
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        private static void AssertCorrectData(BloggingContext context, IQueryable<Blog> query,Func<Blog,bool> predicate)
        {
            var blogs = query.ToArray();
            foreach (var blog in blogs)
            {
                Assert.True(predicate(blog));
            }
            var rest = context.Blogs.Except(query).ToArray();
            foreach (var blog in rest)
            {
                Assert.False(predicate(blog));
            }
        }

        [Fact]
        public void HasFlag_with_flag_enum_property_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT[Extent1].[BlogId]AS[BlogId],[Extent1].[Name]AS[Name],[Extent1].[BlogType]AS[BlogType],[Extent1].[NullableBlogType]AS[NullableBlogType]FROM[dbo].[Blogs]AS[Extent1]WHERE((CAST([Extent1].[BlogType]ASint))&(CAST([Extent1].[BlogType]ASint)))=CAST([Extent1].[BlogType]ASint)";
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var context = new BloggingContext())
            {
                var query = from blog in context.Blogs
                            where blog.BlogType.HasFlag(blog.BlogType)
                            select blog;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void HasFlag_with_flag_enum_of_incorrect_type_throws_NotSupportedException()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var context = new BloggingContext())
            {

                var query = from blog in context.Blogs
                            where blog.BlogType.HasFlag(OtherEnum.A)
                            select blog;

                var actualMessage = Assert.Throws<NotSupportedException>(() => query.ToArray()).Message;
                var expectedMessage = Assert.Throws<ArgumentException>(() => BlogType.Favorite.HasFlag(OtherEnum.A)).Message;
                Assert.Equal(expectedMessage, actualMessage);
            }
        }

        [Fact]
        public void HasFlag_can_translate_constant_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                var blog = new Blog();
                string resultQuery;

                // these are probably mostly the same and you will get just a constant
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(BlogType.Favorite)).ToString();
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(BlogType.Favorite | BlogType.Online)).ToString();
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag((BlogType)(-1))).ToString();
            }
        }

        [Fact]
        public void HasFlag_can_translate_reference_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                var blog = new Blog();
                string resultQuery;

                // these are probably mostly the same and you will get just a constant
                var blogType = BlogType.Favorite;
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(blogType)).ToString();
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(blog.BlogType)).ToString();

                //EF does not allow .First() to be translated, it hints to use FirstOrDefault
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault().BlogType)).ToString();

                // ???
                //resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.First(x => x.NullableBlogType == null).NullableBlogType)).ToString();
            }
        }



        [Fact]
        public void HasFlag_can_translate_nullable_constant_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                var blog = new Blog();
                string actual;

                string expected = @"SELECT 
    [Extent1].[BlogType] AS [BlogType], 
    cast(1 as bit) AS [C1]
    FROM [dbo].[Blogs] AS [Extent1]
    WHERE (((( CAST( [Extent1].[BlogType] AS int)) & ( CAST( 1 AS int))) =  CAST( 1 AS int)) AND ( NOT ((( CAST( [Extent1].[BlogType] AS int)) & ( CAST( 1 AS int)) IS NULL) OR ( CAST( 1 AS int) IS NULL)))) OR ((( CAST( [Extent1].[BlogType] AS int)) & ( CAST( 1 AS int)) IS NULL) AND ( CAST( 1 AS int) IS NULL))";


                // nullable enum type
                actual = db.Blogs.Where(b => b.BlogType.HasFlag((BlogType?)1)).Select(b => new { }).ToString();
                Assert.Equal(expected, actual);

                BlogType? nullableBlogType = BlogType.Favorite;
                actual = db.Blogs.Where(b => b.BlogType.HasFlag(nullableBlogType)).Select(b => new { }).ToString();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void HasFlag_can_translate_nullable_reference_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                var blog = new Blog();
                string resultQuery;

                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault().NullableBlogType)).ToString();
            }
        }

        [Fact]
        public void HasFlag_throws_when_translating_null_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                var blog = new Blog();

                BlogType? nullableBlogType = null;
                Assert.Throws<NotSupportedException>(() => db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(null)));
                Assert.Throws<NotSupportedException>(() => db.Blogs.Where(b => b.BlogType.HasFlag(nullableBlogType)).ToString());
            }
        }
    }
}