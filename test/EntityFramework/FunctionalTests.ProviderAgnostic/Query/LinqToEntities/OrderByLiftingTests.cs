// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.Linq;
    using Xunit;

    public class OrderByLiftingTests
    {
        [Fact]
        public void OrderBy_ThenBy_lifted_above_projection()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 });
                var baseline = context.Owners.Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_lifted_above_type_filter()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Configs.OrderByDescending(p => p.Arch).ThenBy(p => p.Id).OfType<MachineConfig>();
                var baseline = context.Configs.OfType<MachineConfig>().OrderByDescending(p => p.Arch).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Configs.ToList().OrderByDescending(p => p.Arch).ThenBy(p => p.Id).OfType<MachineConfig>().ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_lifted_above_projection()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 });
                var baseline = context.Owners.Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_projection()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 });
                var baseline = context.Owners.Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);

            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Take_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Take(10).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_projection()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 });
                var baseline = context.Owners.Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Select(p => new { p.FirstName, p.Id, Foo = p.Id * 5 }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_filter_with_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }

        [Fact]
        public void OrderBy_ThenBy_Skip_Take_lifted_above_filter_without_clr_null_semantics()
        {
            using (var context = new ProviderAgnosticContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = false;

                var query = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0);
                var baseline = context.Owners.OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0).OrderByDescending(p => p.FirstName).ThenBy(p => p.Id);
                Assert.Equal(baseline.ToString(), query.ToString());

                var results = query.ToList();
                var expected = context.Owners.ToList().OrderByDescending(p => p.FirstName).ThenBy(p => p.Id).Skip(5).Take(10).Where(p => p.Id % 2 == 0).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Id == i.Id);
            }
        }
    }
}
