// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using Moq;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class IQueryableExtensionsTests
    {
        [Fact]
        public void Extension_methods_validate_arguments()
        {
            ArgumentNullTest("source", () => IQueryableExtensions.FirstAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().FirstAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().SingleAsync(null));
            ArgumentNullTest("predicate", () => Source().SingleAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().AnyAsync(null));
            ArgumentNullTest("predicate", () => Source().AnyAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AllAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.AllAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().AllAsync(null));
            ArgumentNullTest("predicate", () => Source().AllAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().CountAsync(null));
            ArgumentNullTest("predicate", () => Source().CountAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().LongCountAsync(null));
            ArgumentNullTest("predicate", () => Source().LongCountAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null));
            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null));
            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null, new CancellationToken()));

            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null));
            ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null));
            ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null));
            ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));

            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));
        }

        [Fact]
        public void Extension_methods_throw_on_non_async_source()
        {
            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().AnyAsync());
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0));
            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().CountAsync());
            SourceNonAsyncQueryableTest(() => Source().CountAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().FirstAsync());
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync());
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType(), new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => Source().LoadAsync());
            SourceNonAsyncEnumerableTest(() => Source().LoadAsync(new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().LongCountAsync());
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().MaxAsync());
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().MinAsync());
            SourceNonAsyncQueryableTest(() => Source().MinAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().SingleAsync());
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync());
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToArrayAsync());
            SourceNonAsyncEnumerableTest<int>(() => Source().ToArrayAsync(new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e,
                new Mock<IEqualityComparer<int>>().Object));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e,
                new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e,
                new Mock<IEqualityComparer<int>>().Object));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e,
                new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync());
            SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync(new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType()));
            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType(), new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ToListAsync<double>());
            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ToListAsync<double>(new CancellationToken()));
        }

        [Fact]
        public void Extension_methods_call_provider_ExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true));
            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, bool>(value => value.AnyAsync());
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, double>(value => value.AverageAsync());
            VerifyProducedExpression<int, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync());
            VerifyProducedExpression<long, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync());
            VerifyProducedExpression<float, float>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync());
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync());
            VerifyProducedExpression<double, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync());
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync());
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0));
            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.CountAsync());
            VerifyProducedExpression<int, int>(value => value.CountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.FirstAsync());
            VerifyProducedExpression<int, int>(value => value.FirstAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync());
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, long>(value => value.LongCountAsync());
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.MaxAsync());
            VerifyProducedExpression<int, int>(value => value.MaxAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.MinAsync());
            VerifyProducedExpression<int, int>(value => value.MinAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleAsync());
            VerifyProducedExpression<int, int>(value => value.SingleAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync());
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SumAsync());
            VerifyProducedExpression<int, int>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync());
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync());
            VerifyProducedExpression<long, long>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e));
            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync());
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync());
            VerifyProducedExpression<float, float>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e));
            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync());
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync());
            VerifyProducedExpression<double, double>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e));
            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync());
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync());
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync());
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        }

        private static IQueryable<T> Source<T>()
        {
            return new Mock<IQueryable<T>>().Object;
        }

        private static IQueryable<int> Source()
        {
            return Source<int>();
        }

        private static void ArgumentNullTest(string paramName, Action test)
        {
            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(() => test()).ParamName);
        }

        private static void SourceNonAsyncQueryableTest(Action test)
        {
            Assert.Equal(Strings.IQueryable_Provider_Not_Async, Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void SourceNonAsyncEnumerableTest(Action test)
        {
            Assert.Equal(Strings.IQueryable_Not_Async(string.Empty), Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void SourceNonAsyncEnumerableTest<T>(Action test)
        {
            Assert.Equal(Strings.IQueryable_Not_Async("<" + typeof(T) + ">"), Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void VerifyProducedExpression<TElement, TResult>(
            Expression<Func<IQueryable<TElement>, Task<TResult>>> testExpression)
        {
            var queryableMock = new Mock<IQueryable<TElement>>();
            var providerMock = new Mock<IDbAsyncQueryProvider>();
            providerMock.Setup(m => m.ExecuteAsync<TResult>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                .Returns<Expression, CancellationToken>(
                    (e, ct) =>
                    {
                        var expectedMethodCall = (MethodCallExpression)testExpression.Body;
                        var actualMethodCall = (MethodCallExpression)e;

                        Assert.Equal(
                            expectedMethodCall.Method.Name,
                            actualMethodCall.Method.Name + "Async");

                        var lastArgument = expectedMethodCall.Arguments[expectedMethodCall.Arguments.Count - 1] as MemberExpression;

                        bool cancellationTokenPresent = lastArgument != null && lastArgument.Type == typeof(CancellationToken);

                        if (cancellationTokenPresent)
                        {
                            Assert.NotEqual(ct, CancellationToken.None);
                        }
                        else
                        {
                            Assert.Equal(ct, CancellationToken.None);
                        }

                        var expectedNumberOfArguments = cancellationTokenPresent
                                                   ? expectedMethodCall.Arguments.Count - 1
                                                   : expectedMethodCall.Arguments.Count;
                        Assert.Equal(expectedNumberOfArguments, actualMethodCall.Arguments.Count);
                        for (int i = 1; i < expectedNumberOfArguments; i++)
                        {
                            var expectedArgument = expectedMethodCall.Arguments[i];
                            var actualArgument = actualMethodCall.Arguments[i];
                            Assert.Equal(expectedArgument.ToString(), actualArgument.ToString());
                        }

                        return Task.FromResult(default(TResult));
                    });

            queryableMock.Setup(m => m.Provider).Returns(providerMock.Object);

            queryableMock.Setup(m => m.Expression).Returns(Expression.Constant(queryableMock.Object, typeof(IQueryable<TElement>)));

            testExpression.Compile()(queryableMock.Object);
        }
    }
}
