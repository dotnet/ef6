// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.Linq;
    using Xunit;

    public class IncludeTests
    {
        [Fact]
        public void Include_on_one_to_many_relationship()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Runs.OrderBy(r => r.Id).Include(c => c.Tasks);
                var results = query.ToList();
                var tasksForRuns = context.Runs.OrderBy(r => r.Id).Select(r => r.Tasks).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(tasksForRuns[i].Count, results[i].Tasks.Count);
                    var expectedTasks = tasksForRuns[i].Select(t => t.Id).ToList().OrderBy(t => t);
                    var actualTasks = results[i].Tasks.Select(t => t.Id).ToList().OrderBy(t => t);

                    Assert.True(Enumerable.SequenceEqual(expectedTasks, actualTasks));
                }
            }
        }

        [Fact]
        public void Include_on_many_to_many_relationship()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Configs.OrderBy(c => c.Id).Include(c => c.Failures);
                var results = query.ToList();
                var failuresForConfigs = context.Configs.Select(c => c.Failures).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(failuresForConfigs[i].Count, results[i].Failures.Count);
                    var expectedFailures = failuresForConfigs[i].Select(t => t.Id).ToList().OrderBy(f => f);
                    var actualFailures = results[i].Failures.Select(t => t.Id).ToList().OrderBy(f => f);
                    Assert.True(Enumerable.SequenceEqual(expectedFailures, actualFailures));
                }
            }
        }

        [Fact]
        public void Include_one_to_one_relationship()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.OrderBy(o => o.Id).Include(o => o.OwnedRun);
                var results = query.ToList();
                var runsForOwners = context.Owners.OrderBy(o => o.Id).Select(o => o.OwnedRun).ToList();
                Enumerable.SequenceEqual(runsForOwners, results.Select(r => r.OwnedRun));
            }
        }

        [Fact]
        public void Multiple_includes()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Failures.OrderBy(f => f.Id).Include(f => f.Configs).Include(f => f.Bugs);
                var results = query.ToList();
                var configsForFailures = context.Failures.OrderBy(f => f.Id).Select(r => r.Configs).ToList();
                var bugsForFailures = context.Failures.OrderBy(f => f.Id).Select(r => r.Bugs).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(bugsForFailures[i].Count, results[i].Bugs.Count);
                    Assert.Equal(configsForFailures[i].Count, results[i].Configs.Count);
                    var expectedBugs = bugsForFailures[i].Select(b => b.Id).ToList().OrderBy(b => b);
                    var expectedConfigs = configsForFailures[i].Select(c => c.Id).OrderBy(c => c).ToList();
                    var actualBugs = results[i].Bugs.Select(b => b.Id).ToList().OrderBy(b => b);
                    var actualConfigs = results[i].Configs.Select(c => c.Id).ToList().OrderBy(c => c);
                    Enumerable.SequenceEqual(expectedBugs, actualBugs);
                    Enumerable.SequenceEqual(expectedConfigs, actualConfigs);
                }
            }
        }

        [Fact]
        public void Include_with_string_overload()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Runs.OrderBy(r => r.Id).Include("Tasks");
                var results = query.ToList();
                var tasksForRuns = context.Runs.OrderBy(r => r.Id).Select(r => r.Tasks).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(tasksForRuns[i].Count, results[i].Tasks.Count);
                    var expectedTasks = tasksForRuns[i].Select(t => t.Id).ToList().OrderBy(t => t);
                    var actualTasks = results[i].Tasks.Select(t => t.Id).ToList().OrderBy(t => t);
                    Enumerable.SequenceEqual(expectedTasks, actualTasks);
                }
            }
        }

        [Fact]
        public void Include_propagation_over_filter()
        {
            using (var context = new ProviderAgnosticContext())
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
            using (var context = new ProviderAgnosticContext())
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
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Configs.Include(c => c.Failures).OfType<MachineConfig>();
                var results = query.ToList();
                Assert.True(results.Any(r => r.Failures.Count > 0));
            }
        }

        [Fact]
        public void Include_propagation_over_first()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(c => c.Failures).First();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_with_predicate()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(c => c.Failures).First(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(c => c.Failures).FirstOrDefault();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default_with_predicate()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(c => c.Failures).FirstOrDefault(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_from_concat_combined()
        {
            using (var context = new ProviderAgnosticContext())
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
            using (var context = new ProviderAgnosticContext())
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
