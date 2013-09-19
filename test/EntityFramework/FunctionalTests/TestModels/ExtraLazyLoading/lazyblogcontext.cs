// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace LazyUnicorns
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class LazyBlogContext : DbContext
    {
        public LazyBlogContext()
        {
            Database.SetInitializer(new LazyBlogsContextInitializer());
            Configuration.LazyLoadingEnabled = false;

            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized +=
                (s, e) => new QueryableCollectionInitializer().InitializeCollections(this, e.Entity);
        }

        public DbSet<LazyPost> Posts { get; set; }
        public DbSet<LazyComment> Comments { get; set; }
    }

    public class LazyBlogsContextInitializer : DropCreateDatabaseIfModelChanges<LazyBlogContext>
    {
        protected override void Seed(LazyBlogContext context)
        {
            context.Posts.Add(
                new LazyPost
                    {
                        Id = 1,
                        Title = "Lazy Unicorns",
                        Comments = new List<LazyComment>
                                       {
                                           new LazyComment
                                               {
                                                   Content = "Are enums supported?"
                                               },
                                           new LazyComment
                                               {
                                                   Content = "My unicorns are so lazy they fell asleep."
                                               },
                                           new LazyComment
                                               {
                                                   Content = "Is a unicorn without a horn just a horse?"
                                               },
                                       }
                    });

            context.Posts.Add(
                new LazyPost
                    {
                        Id = 2,
                        Title = "Sleepy Horses",
                        Comments = new List<LazyComment>
                                       {
                                           new LazyComment
                                               {
                                                   Content = "Are enums supported?"
                                               },
                                       }
                    });
        }
    }

    public class LazyComment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public LazyPost Post { get; set; }
    }

    public class LazyPost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public ICollection<LazyComment> Comments { get; set; }
    }
}
