// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Collections.Generic;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    public abstract class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public static void DoStuff(BlogContext context)
        {
            Blog blog = null;
            Post post = null;
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    blog = context.Blogs.Single();
                    Assert.Equal("Half a Unicorn", blog.Title);

                    post = blog.Posts.Single();
                    Assert.Equal("Wrap it up...", post.Title);
                });

            blog.Posts.Add(
                new Post
                {
                    Title = "Throw it away...",
                    Spacetime = new DateTime(1915, 11, 25)
                });

            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (context.Database.BeginTransaction())
                    {
                        Assert.Equal(1, context.SaveChanges());
                        Assert.Equal(
                            new[] { "Throw it away...", "Wrap it up..." },
                            context.Posts.AsNoTracking().Select(p => p.Title).OrderBy(t => t));
                    }
                });

#if !NET40

            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (context.Database.BeginTransaction())
                    {
                        post.Title = "I'm a logger and I'm okay...";

                        var saveTask = context.SaveChangesAsync();
                        saveTask.Wait();
                        Assert.Equal(1, saveTask.Result);

                        var queryTask = context.Posts
                            .AsNoTracking()
                            .Select(p => p.Title).OrderBy(t => t)
                            .ToListAsync();
                        queryTask.Wait();

                        Assert.Equal(new[] { "I'm a logger and I'm okay..." }, queryTask.Result);
                    }
                });
#endif
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public virtual ICollection<Post> Posts { get; set; }

            public DateTime TimeDilation { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public int BlogId { get; set; }
            public virtual Blog Blog { get; set; }
            
            public DateTime Spacetime { get; set; }
        }

        public class BlogInitializer : DropCreateDatabaseAlways<BlogContext>
        {
            protected override void Seed(BlogContext context)
            {
                context.Posts.Add(
                    new Post
                        {
                            Title = "Wrap it up...",
                            Spacetime = new DateTime(1915, 11, 25),
                            Blog = new Blog
                                {
                                    Title = "Half a Unicorn",
                                    TimeDilation = new DateTime(1905, 6, 30)
                                }
                        });
            }
        }
    }
}
