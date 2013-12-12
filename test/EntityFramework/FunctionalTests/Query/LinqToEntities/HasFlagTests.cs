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
        }



        [Fact]
        public void HasFlag_with_flag_enum_constant_is_translated_to_expected_sql()
        {
            const string expectedSql =
                @"SELECT[Extent1].[BlogId]AS[BlogId],[Extent1].[Name]AS[Name],[Extent1].[BlogType]AS[BlogType],[Extent1].[NullableBlogType]AS[NullableBlogType]FROM[dbo].[Blogs]AS[Extent1]WHERE((CAST([Extent1].[BlogType]ASint))&(CAST(1ASint)))=CAST(1ASint)";
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var context = new BloggingContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = from blog in context.Blogs
                            where blog.BlogType.HasFlag(BlogType.Favorite)
                            select blog;

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
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
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

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
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

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
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
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
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
                var blog = new Blog();
                string resultQuery;

                // these are probably mostly the same and you will get just a constant
                var blogType = BlogType.Favorite;
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(blogType)).ToString();
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(blog.BlogType)).ToString();

                //EF does not allow .First() to be translated, it hints to use FirstOrDefault
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault().BlogType)).ToString();

                //// invalid cases
                //db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(null)).ToString();
                //nullableBlogType = null;
                //db.Blogs.Where(b => b.BlogType.HasFlag(nullableBlogType)).ToString();
                //db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(OtherEnum.A)).ToString();

                //// ???
                //resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.First(x => x.NullableBlogType == null).NullableBlogType)).ToString();
            }
        }

        [Fact]
        public void HasFlag_can_translate_nullable_constant_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
                var blog = new Blog();
                string resultQuery;

                // nullable enum type
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag((BlogType?)1)).ToString();
                BlogType? nullableBlogType = BlogType.Favorite;
                resultQuery = db.Blogs.Where(b => b.BlogType.HasFlag(nullableBlogType)).ToString();
            }
        }

        [Fact]
        public void HasFlag_can_translate_nullable_reference_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
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
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
                var blog = new Blog();

                BlogType? nullableBlogType = null;
                Assert.Throws<NotSupportedException>(() => db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(null)));
                Assert.Throws<NotSupportedException>(() => db.Blogs.Where(b => b.BlogType.HasFlag(nullableBlogType)).ToString());
            }
        }
    }
}