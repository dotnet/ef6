// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class IncludeTests : FunctionalTestBase
    {
        [Fact]
        public void Nested_include()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include("OwnedRun.Tasks");
                var results = query.ToList();
                var tasksForOwners = context.Owners.Select(o => o.OwnedRun.Tasks).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(tasksForOwners[i].Count, results[i].OwnedRun.Tasks.Count);
                    var expectedTasks = tasksForOwners[i].Select(t => t.Id).ToList();
                    var actualTasks = results[i].OwnedRun.Tasks.Select(t => t.Id).ToList();
                    Enumerable.SequenceEqual(expectedTasks, actualTasks);
                }
            }
        }

        [Fact]
        public void Include_propagation_over_filter()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include(o => o.OwnedRun).Where(o => o.Id == 1);
                var results = query.ToList();
                Assert.NotNull(results.First().OwnedRun);
            }
        }

        [Fact]
        public void Include_propagation_over_sort()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include(o => o.OwnedRun).OrderBy(o => o.Id == 1);
                var results = query.ToList();
                Assert.NotNull(results.First().OwnedRun);
            }
        }

        [Fact]
        public void Include_propagation_over_type_filter()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Configs.Include(o => o.Failures).OfType<ArubaMachineConfig>();
                var results = query.ToList();
                Assert.True(results.Any(r => r.Failures.Count > 0));
            }
        }

        [Fact]
        public void Include_propagation_over_first()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).First();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_with_predicate()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).First(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).FirstOrDefault();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default_with_predicate()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).FirstOrDefault(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_from_concat_combined()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Failures.Include(f => f.Bugs).Concat(context.Failures.Include(f => f.Configs));
                var results = query.ToList();
                Assert.True(results.Any(r => r.Bugs.Count > 0));
                Assert.True(results.Any(r => r.Configs.Count > 0));
            }
        }

        [Fact]
        public void Include_from_union_combined()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Failures.Include(f => f.Bugs).Union(context.Failures.Include(f => f.Configs));
                var results = query.ToList();
                Assert.True(results.Any(r => r.Bugs.Count > 0));
                Assert.True(results.Any(r => r.Configs.Count > 0));
            }
        }
    }
}
