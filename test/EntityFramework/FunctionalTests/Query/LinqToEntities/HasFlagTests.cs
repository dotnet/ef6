// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
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
            protected override void Seed(BloggingContext db)
            {
                base.Seed(db);

                for (int i = -1; i < 20; i++)
                {
                    var blog = new Blog{
                        BlogType = (BlogType)i,
                        Name = "Blog with null prop" + i,
                        NullableBlogType = null,
                    };
                    db.Blogs.Add(blog);

                    blog = new Blog
                    {
                        BlogType = (BlogType)i,
                        Name = "Blog w/o null prop" + i,
                        NullableBlogType = BlogType.Important,
                    };
                    db.Blogs.Add(blog);
                }
            } 
        }

        private void AssertConsistentWithLinqToObjects(BloggingContext db, Expression<Func<Blog, bool>> predicate)
        {
            var query = db.Blogs.Where(predicate);
            var compiledPredicate = predicate.Compile();
            var matching = query.ToArray();
            foreach (var blog in matching)
            {
                Assert.True(compiledPredicate(blog));
            }

            // Take all blogs from the db, except the ones found by the query
            // We do this in mem (AsEnum) to ensure we don't get an incorrect set because of the feature we are testing
            var nonMatching = db.Blogs.AsEnumerable().Except(query).ToArray();
            Assert.False(nonMatching.Count() == 0, "No non matching entries was found, query is too greedy");
            
            foreach (var blog in nonMatching)
            {
                Assert.False(compiledPredicate(blog));
            }
        }


        [Fact]
        public void HasFlag_with_incorrect_type_throws_NotSupportedException()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = db.Blogs.Where(b => b.BlogType.HasFlag(OtherEnum.A));
                var actualMessage = Assert.Throws<NotSupportedException>(() => query.ToArray()).Message;
                var expectedMessage = Assert.Throws<ArgumentException>(() => BlogType.Favorite.HasFlag(OtherEnum.A)).Message;
                Assert.Equal(expectedMessage, actualMessage);
            }
        }

        [Fact]
        public void HasFlag_with_constants_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(BlogType.Favorite));
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(BlogType.Favorite | BlogType.Online));
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag((BlogType)(-1)));
            }
        }

        [Fact]
        public void HasFlag_with_reference_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
                
                var blogType = BlogType.Favorite;
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(blogType));
                var blog = new Blog();
                blog.BlogType = BlogType.Online | BlogType.Important;
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(blog.BlogType));
                //EF does not allow .First() to be translated, it hints to use FirstOrDefault
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.BlogType == BlogType.Favorite).BlogType));
            }
        }



        [Fact]
        public void HasFlag_with_nullable_constant_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag((BlogType?)1));
                BlogType? nullableBlogType = BlogType.Favorite;
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(nullableBlogType));
            }
        }

        [Fact]
        public void HasFlag_with_nullable_reference_values()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                //Expect EF to return data when querying HasFlag using nullable reference that has a value
                AssertConsistentWithLinqToObjects(db, b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.NullableBlogType != null).NullableBlogType));

                //This throws in normal code / Linq to Objects
                //Expect EF to return no data when querying HasFlag using nullable reference that is null
                //Is this the expected behavior? (there isn't much else that can be done..)
                var query = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.NullableBlogType == null).NullableBlogType));
                var matching = query.ToArray();
                Assert.True(matching.Count() == 0, "Querying HasFlag with null expression should not match any data");
            }
        }

        [Fact]
        public void HasFlag_throws_when_translating_null_constant()
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

        [Fact]
        public void HasFlag_projects_boolean_values_in_select()
        {
            Database.SetInitializer<BloggingContext>(new BloggingInitializer());
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var blog = db.Blogs
                    .Where(b => 
                        b.BlogType.HasFlag(BlogType.Important) && 
                        !b.BlogType.HasFlag(BlogType.Online))
                    .Select(b => new
                    {
                        hasFlagTrue = b.BlogType.HasFlag(BlogType.Important),
                        hasFlagFalse = b.BlogType.HasFlag(BlogType.Online),
                    })
                    .First();

                Assert.True(blog.hasFlagTrue);
                Assert.False(blog.hasFlagFalse);
            }
        }
    }
}