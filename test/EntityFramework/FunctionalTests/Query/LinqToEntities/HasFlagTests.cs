// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
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
            static BloggingContext()
            {
                Database.SetInitializer(new BloggingInitializer());
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class BloggingInitializer : DropCreateDatabaseAlways<BloggingContext>
        {
            protected override void Seed(BloggingContext db)
            {
                db.Configuration.UseDatabaseNullSemantics = false;

                for (var i = -1; i < 20; i++)
                {
                    db.Blogs.Add(
                        new Blog
                        {
                            BlogType = (BlogType)i,
                            Name = "Blog with null prop" + i,
                            NullableBlogType = null
                        });

                    db.Blogs.Add(
                        new Blog
                        {
                            BlogType = (BlogType)i,
                            Name = "Blog w/o null prop" + i,
                            NullableBlogType = BlogType.Important,
                        });
                }
            } 
        }

        private static void AssertConsistency(BloggingContext db, Expression<Func<Blog, bool>> predicate)
        {
            var matching = db.Blogs.Where(predicate).ToArray();
            var compiledPredicate = predicate.Compile();

            foreach (var blog in matching)
            {
                //if anything fails here, the generated SQL is incorrect
                Assert.True(compiledPredicate(blog));
            }

            // Take all blogs from the db, except the ones found by the query
            // We do this in mem (AsEnum) to ensure we don't get an incorrect set because of the feature we are testing
            var nonMatching = db.Blogs.AsEnumerable().Except(matching).ToArray();
            Assert.NotEmpty(nonMatching);
                        
            foreach (var blog in nonMatching)
            {
                //if anything fails here, the generated SQL is incorrect
                Assert.False(compiledPredicate(blog));
            }
        }

        [Fact]
        public void HasFlag_with_incorrect_type_throws_NotSupportedException()
        {
            using (var db = new BloggingContext())
            {
                var query = db.Blogs.Where(b => b.BlogType.HasFlag(OtherEnum.A));
                Assert.Throws<NotSupportedException>(() => query.ToString());              
            }
        }

        [Fact]
        public void HasFlag_with_constants_values()
        {
            using (var db = new BloggingContext())
            {
                AssertConsistency(db, b => b.BlogType.HasFlag(BlogType.Favorite));
                AssertConsistency(db, b => b.BlogType.HasFlag(BlogType.Favorite | BlogType.Online));
                AssertConsistency(db, b => b.BlogType.HasFlag((BlogType)(-1)));
            }
        }

        [Fact]
        public void HasFlag_with_reference_values()
        {
            using (var db = new BloggingContext())
            {
                ((IObjectContextAdapter)db).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;
                
                var blogType = BlogType.Favorite;
                AssertConsistency(db, b => b.BlogType.HasFlag(blogType));
                var blog = new Blog();
                blog.BlogType = BlogType.Online | BlogType.Important;
                AssertConsistency(db, b => b.BlogType.HasFlag(blog.BlogType));
                //EF does not allow .First() to be translated, it hints to use FirstOrDefault
                AssertConsistency(db, b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.BlogType == BlogType.Favorite).BlogType));
            }
        }

        [Fact]
        public void HasFlag_with_nullable_constant_values()
        {
            using (var db = new BloggingContext())
            {
                AssertConsistency(db, b => b.BlogType.HasFlag((BlogType?)1));
                BlogType? nullableBlogType = BlogType.Favorite;
                AssertConsistency(db, b => b.BlogType.HasFlag(nullableBlogType));
            }
        }

        [Fact]
        public void HasFlag_with_nullable_reference_values()
        {
            using (var db = new BloggingContext())
            {
                //Expect EF to return data when querying HasFlag using nullable reference that has a value
                AssertConsistency(db, b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.NullableBlogType != null).NullableBlogType));

                //This throws in normal code / Linq to Objects
                //Expect EF to return no data when querying HasFlag using nullable reference that is null
                var query = db.Blogs.Where(b => b.BlogType.HasFlag(db.Blogs.FirstOrDefault(b2 => b2.NullableBlogType == null).NullableBlogType));
                var matching = query.ToArray();

                // "Querying HasFlag with null expression should not match any data"
                Assert.Empty(matching);
            }
        }

        [Fact]
        public void HasFlag_throws_when_translating_null_constant()
        {
            using (var db = new BloggingContext())
            {             
                Assert.Equal(
                    "flag", 
                    Assert.Throws<ArgumentNullException>(
                        () => db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(null))).ParamName);

                BlogType? nullableBlogType = null;
                Assert.Equal(
                    "flag",
                    Assert.Throws<ArgumentNullException>(
                        () => db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(nullableBlogType))).ParamName);

                var blog = new Blog();
                Assert.Equal(
                    "flag",
                    Assert.Throws<ArgumentNullException>(
                        () => db.Blogs.FirstOrDefault(b => b.BlogType.HasFlag(blog.NullableBlogType))).ParamName);
            }
        }

        [Fact]
        public void HasFlag_projects_boolean_values_in_select()
        {
            using (var db = new BloggingContext())
            {
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