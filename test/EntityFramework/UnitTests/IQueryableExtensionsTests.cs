// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
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
            ArgumentNullTest("predicate", () => IQueryableExtensions.FirstAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.FirstAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.FirstOrDefaultAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.FirstOrDefaultAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LastAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LastAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LastOrDefaultAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LastOrDefaultAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.SingleAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.SingleAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.SingleOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.SingleOrDefaultAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.SingleOrDefaultAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtAsync<int>(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtAsync<int>(null, 1, new CancellationToken()));
            ArgumentOutOfRangeTest("index", () => IQueryableExtensions.ElementAtAsync(Source(), -1));
            ArgumentOutOfRangeTest("index", () => IQueryableExtensions.ElementAtAsync(Source(), -1, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtOrDefaultAsync<int>(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtOrDefaultAsync<int>(null, 1, new CancellationToken()));
            ArgumentOutOfRangeTest("index", () => IQueryableExtensions.ElementAtOrDefaultAsync(Source(), -1));
            ArgumentOutOfRangeTest("index", () => IQueryableExtensions.ElementAtOrDefaultAsync(Source(), -1, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, null));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, null, new CancellationToken()));

            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source()));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), new CancellationToken()));
            ArgumentNullTest("source2", () => IQueryableExtensions.SequenceEqualAsync(Source(), null));
            ArgumentNullTest("source2", () => IQueryableExtensions.SequenceEqualAsync(Source(), null, new CancellationToken()));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), null));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), null, new CancellationToken()));
            ArgumentNullTest("source2", () => IQueryableExtensions.SequenceEqualAsync(Source(), null, null));
            ArgumentNullTest("source2", () => IQueryableExtensions.SequenceEqualAsync(Source(), null, null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.AnyAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.AnyAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.AnyAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AllAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.AllAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.AllAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.AllAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.CountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.CountAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.CountAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LongCountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LongCountAsync(Source(), null));
            ArgumentNullTest("predicate", () => IQueryableExtensions.LongCountAsync(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.MinAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.MinAsync<int, bool>(Source(), null));
            ArgumentNullTest("selector", () => IQueryableExtensions.MinAsync<int, bool>(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.MaxAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.MaxAsync<int, bool>(Source(), null));
            ArgumentNullTest("selector", () => IQueryableExtensions.MaxAsync<int, bool>(Source(), null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<int?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<long?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<float?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<double?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.SumAsync((IQueryable<decimal?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<int>(), (Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<int>(), (Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<int?>(), (Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<int?>(), (Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<long>(), (Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<long>(), (Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<long?>(), (Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<long?>(), (Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<float>(), (Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<float>(), (Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<float?>(), (Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<float?>(), (Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<double>(), (Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<double>(), (Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<double?>(), (Expression<Func<double?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<double?>(), (Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<decimal>(), (Expression<Func<decimal, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<decimal>(), (Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<decimal?>(), (Expression<Func<decimal?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.SumAsync(Source<decimal?>(), (Expression<Func<decimal?, int>>)null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal?>)null));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal?>)null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<int?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<long?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<float?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<double?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal?>)null, i => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AverageAsync((IQueryable<decimal?>)null, i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<int>(), (Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<int>(), (Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<int?>(), (Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<int?>(), (Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<long>(), (Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<long>(), (Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<long?>(), (Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<long?>(), (Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<float>(), (Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<float>(), (Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<float?>(), (Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<float?>(), (Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<double>(), (Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<double>(), (Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<double?>(), (Expression<Func<double?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<double?>(), (Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<decimal>(), (Expression<Func<decimal, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<decimal>(), (Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<decimal?>(), (Expression<Func<decimal?, int>>)null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AverageAsync(Source<decimal?>(), (Expression<Func<decimal?, int>>)null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int>(null, (e1, e2) => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int>(null, (e1, e2) => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int>(null, 0, (e1, e2) => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int>(null, 0, (e1, e2) => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int, int>(null, 0, (e1, e2) => 0, e => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int, int>(null, 0, (e1, e2) => 0, e => 0, new CancellationToken()));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), null));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), null, new CancellationToken()));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), 0, null));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), 0, null, new CancellationToken()));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), 0, null, e => 0));
            ArgumentNullTest("func", () => IQueryableExtensions.AggregateAsync(Source(), 0, null, e => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => IQueryableExtensions.AggregateAsync<int, int, int>(Source(), 0, (e1, e2) => 0, null));
            ArgumentNullTest("selector", () => IQueryableExtensions.AggregateAsync<int, int, int>(Source(), 0, (e1, e2) => 0, null, new CancellationToken()));
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

        private static void ArgumentOutOfRangeTest(string paramName, Action test)
        {
            Assert.Equal(paramName, Assert.Throws<ArgumentOutOfRangeException>(() => test()).ParamName);
        }
    }
}
