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
            ArgumentNullTest("predicate", () => Source().FirstAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LastAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().LastAsync(null));
            ArgumentNullTest("predicate", () => Source().LastAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => IQueryableExtensions.LastOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().LastOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().LastOrDefaultAsync(null, new CancellationToken()));

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

            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtAsync<int>(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtAsync<int>(null, 1, new CancellationToken()));
            ArgumentOutOfRangeTest("index", () => Source().ElementAtAsync(-1));
            ArgumentOutOfRangeTest("index", () => Source().ElementAtAsync(-1, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtOrDefaultAsync<int>(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ElementAtOrDefaultAsync<int>(null, 1, new CancellationToken()));
            ArgumentOutOfRangeTest("index", () => Source().ElementAtOrDefaultAsync(-1));
            ArgumentOutOfRangeTest("index", () => Source().ElementAtOrDefaultAsync(-1, new CancellationToken()));

            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, null));
            ArgumentNullTest("source", () => IQueryableExtensions.ContainsAsync(null, 1, null, new CancellationToken()));

            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source()));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), new CancellationToken()));
            ArgumentNullTest("source2", () => Source().SequenceEqualAsync(null));
            ArgumentNullTest("source2", () => Source().SequenceEqualAsync(null, new CancellationToken()));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), null));
            ArgumentNullTest("source1", () => IQueryableExtensions.SequenceEqualAsync(null, Source(), null, new CancellationToken()));
            ArgumentNullTest("source2", () => Source().SequenceEqualAsync(null, null));
            ArgumentNullTest("source2", () => Source().SequenceEqualAsync(null, null, new CancellationToken()));

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

            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int>(null, (e1, e2) => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int>(null, (e1, e2) => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int>(null, 0, (e1, e2) => 0));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int>(null, 0, (e1, e2) => 0, new CancellationToken()));
            ArgumentNullTest("source", () => IQueryableExtensions.AggregateAsync<int, int, int>(null, 0, (e1, e2) => 0, e => 0));
            ArgumentNullTest(
                "source", () => IQueryableExtensions.AggregateAsync<int, int, int>(null, 0, (e1, e2) => 0, e => 0, new CancellationToken()));
            ArgumentNullTest("func", () => Source().AggregateAsync(null));
            ArgumentNullTest("func", () => Source().AggregateAsync(null, new CancellationToken()));
            ArgumentNullTest("func", () => Source().AggregateAsync(0, null));
            ArgumentNullTest("func", () => Source().AggregateAsync(0, null, new CancellationToken()));
            ArgumentNullTest("func", () => Source().AggregateAsync(0, null, e => 0));
            ArgumentNullTest("func", () => Source().AggregateAsync(0, null, e => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => Source().AggregateAsync<int, int, int>(0, (e1, e2) => 0, null));
            ArgumentNullTest("selector", () => Source().AggregateAsync<int, int, int>(0, (e1, e2) => 0, null, new CancellationToken()));
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
