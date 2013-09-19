// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using LazyUnicorns;
    using Xunit;

    public class ExtraLazyLoadingTests : FunctionalTestBase
    {
        #region General extra-lazy collection tests

        [Fact]
        public void LazyCountCollection_Count_returns_count_without_loading_collection()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(0, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void LazyCountCollection_Count_returns_count_even_when_collection_is_loaded()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                context.Entry(post).Collection(p => p.Comments).Load();

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void LazyCountCollection_Count_returns_database_count_not_collection_count()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                context.Entry(post).Collection(p => p.Comments).Load();
                post.Comments.Add(new LazyComment());

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(4, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void Enumerating_the_LazyCountCollection_causes_it_to_be_lazy_loaded()
        {
            using (var context = new LazyBlogContext())
            {
                context.Posts.Find(1).Comments.ToList();

                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void Adding_to_the_LazyCountCollection_does_not_cause_it_to_be_lazy_loaded()
        {
            using (var context = new LazyBlogContext())
            {
                context.Posts.Find(1).Comments.Add(new LazyComment());

                Assert.Equal(1, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void LazyCountCollection_Count_returns_count_even_when_collection_is_eager_loaded()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts
                    .Where(p => p.Id == 1)
                    .Include(p => p.Comments)
                    .Single();

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        public class FakeEntityWithListCollection
        {
            public List<LazyPost> Posts { get; set; }
        }

        [Fact]
        public void Collections_not_declared_as_ICollection_are_ignored()
        {
            Assert.Null(
                new QueryableCollectionInitializer()
                    .TryGetElementType(
                        typeof(FakeEntityWithListCollection)
                            .GetProperty("Posts")));
        }

        public class FakeEntityWithReadonlyCollection
        {
            public ICollection<LazyPost> Posts
            {
                get { return null; }
            }
        }

        [Fact]
        public void Collections_without_setters_are_ignored()
        {
            Assert.Null(
                new QueryableCollectionInitializer()
                    .TryGetElementType(
                        typeof(FakeEntityWithReadonlyCollection)
                            .GetProperty("Posts")));
        }

        [Fact]
        public void QueryableCollection_can_be_used_for_First_without_loading_entire_collection()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);

                var firstComment = post.Comments
                    .AsQueryable()
                    .OrderBy(c => c.Id)
                    .FirstOrDefault();

                Assert.NotNull(firstComment);
                Assert.Equal(1, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void QueryableCollection_can_be_used_to_load_filtered_results()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);

                var unicornComments = post.Comments
                    .AsQueryable()
                    .Where(c => c.Content.Contains("unicorn"))
                    .ToList();

                Assert.Equal(2, unicornComments.Count());
                Assert.Equal(2, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void IHasIsLoaded_can_be_used_to_set_IsLoaded_after_a_filtered_query()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);

                post.Comments.AsQueryable()
                    .Where(c => c.Content.Contains("unicorn"))
                    .Load();
                post.Comments.SetLoaded(true);

                Assert.Equal(2, post.Comments.Count); // Doesn't trigger further loading
                Assert.Equal(2, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        #endregion

        #region Tests that cause the call to Load and so potentially cause nested DetectChanges call

        [Fact]
        public void LazyCountCollection_Count_returns_count_without_loading_collection_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;

                Assert.Equal(3, post.Comments.Count);

                // Result is 3 because DetectChanges caused Load
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void LazyCountCollection_Count_returns_count_even_when_collection_is_loaded_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;
                context.Entry(post).Collection(p => p.Comments).Load();

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void LazyCountCollection_Count_returns_database_count_not_collection_count_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;
                context.Entry(post).Collection(p => p.Comments).Load();
                post.Comments.Add(new LazyComment());

                // Result is 4 because DetectChanges called Load and so IsLoaded got set to true
                Assert.Equal(4, post.Comments.Count);
                Assert.Equal(4, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void Enumerating_the_LazyCountCollection_causes_it_to_be_lazy_loaded_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;
                post.Comments.ToList();

                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void Adding_to_the_LazyCountCollection_does_not_cause_it_to_be_lazy_loaded_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;
                post.Comments.Add(new LazyComment());

                // Result is 4 because DetectChanges caused Load
                Assert.Equal(4, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void
            LazyCountCollection_Count_returns_count_even_when_collection_is_eager_loaded_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts
                    .Where(p => p.Id == 1)
                    .Include(p => p.Comments)
                    .Single();
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;

                Assert.Equal(3, post.Comments.Count);
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void
            QueryableCollection_can_be_used_for_First_without_loading_entire_collection_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;

                var firstComment = post.Comments
                    .AsQueryable()
                    .OrderBy(c => c.Id)
                    .FirstOrDefault();

                Assert.NotNull(firstComment);
                // Result is 3 because DetectChanges caused Load
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void QueryableCollection_can_be_used_to_load_filtered_results_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;

                var unicornComments = post.Comments
                    .AsQueryable()
                    .Where(c => c.Content.Contains("unicorn"))
                    .ToList();

                Assert.Equal(2, unicornComments.Count());
                // Result is 3 because DetectChanges caused Load
                Assert.Equal(3, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        [Fact]
        public void IHasIsLoaded_can_be_used_to_set_IsLoaded_after_a_filtered_query_with_Load_from_Enumerator()
        {
            using (var context = new LazyBlogContext())
            {
                var post = context.Posts.Find(1);
                ((QueryableCollection<LazyComment>)post.Comments).TestLoadInEnumerator = true;

                post.Comments.AsQueryable()
                    .Where(c => c.Content.Contains("unicorn"))
                    .Load();
                post.Comments.SetLoaded(true);

                Assert.Equal(2, post.Comments.Count); // Doesn't trigger further loading
                Assert.Equal(2, context.ChangeTracker.Entries<LazyComment>().Count());
            }
        }

        #endregion
    }
}
