// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public static class IQueryableExtensions
    {
        #region Private static fields

        private static readonly MethodInfo _first = GetMethod(
            "First", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _first_Predicate = GetMethod(
            "First", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _firstOrDefault = GetMethod(
            "FirstOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _firstOrDefault_Predicate = GetMethod(
            "FirstOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _last = GetMethod(
            "Last", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _last_Predicate = GetMethod(
            "Last", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _lastOrDefault = GetMethod(
            "LastOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _lastOrDefault_Predicate = GetMethod(
            "LastOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _single = GetMethod(
            "Single", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _single_Predicate = GetMethod(
            "Single", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _singleOrDefault = GetMethod(
            "SingleOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _singleOrDefault_Predicate = GetMethod(
            "SingleOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _elementAt = GetMethod(
            "ElementAt", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T), typeof(int)
                });

        private static readonly MethodInfo _elementAtOrDefault = GetMethod(
            "ElementAtOrDefault", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T), typeof(int)
                });

        private static readonly MethodInfo _contains = GetMethod(
            "Contains", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    T
                });

        private static readonly MethodInfo _contains_Comparer = GetMethod(
            "Contains", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    T,
                    typeof(IEqualityComparer<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _sequenceEqual = GetMethod(
            "SequenceEqual", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(IEnumerable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _sequenceEqual_Comparer = GetMethod(
            "SequenceEqual", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(IEnumerable<>).MakeGenericType(T),
                    typeof(IEqualityComparer<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _any = GetMethod(
            "Any", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _any_Predicate = GetMethod(
            "Any", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _all_Predicate = GetMethod(
            "All", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _count = GetMethod(
            "Count", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _count_Predicate = GetMethod(
            "Count", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _longCount = GetMethod(
            "LongCount", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _longCount_Predicate = GetMethod(
            "LongCount", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(bool)))
                });

        private static readonly MethodInfo _min = GetMethod(
            "Min", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _min_Selector = GetMethod(
            "Min", (T, U) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, U))
                });

        private static readonly MethodInfo _max = GetMethod(
            "Max", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T)
                });

        private static readonly MethodInfo _max_Selector = GetMethod(
            "Max", (T, U) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, U))
                });

        private static readonly MethodInfo _sum_Int = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<int>)
                });

        private static readonly MethodInfo _sum_IntNullable = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<int?>)
                });

        private static readonly MethodInfo _sum_Long = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<long>)
                });

        private static readonly MethodInfo _sum_LongNullable = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<long?>)
                });

        private static readonly MethodInfo _sum_Float = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<float>)
                });

        private static readonly MethodInfo _sum_FloatNullable = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<float?>)
                });

        private static readonly MethodInfo _sum_Double = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<double>)
                });

        private static readonly MethodInfo _sum_DoubleNullable = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<double?>)
                });

        private static readonly MethodInfo _sum_Decimal = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<decimal>)
                });

        private static readonly MethodInfo _sum_DecimalNullable = GetMethod(
            "Sum", () => new[]
                {
                    typeof(IQueryable<decimal?>)
                });

        private static readonly MethodInfo _sum_Int_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(int)))
                });

        private static readonly MethodInfo _sum_IntNullable_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(int?)))
                });

        private static readonly MethodInfo _sum_Long_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(long)))
                });

        private static readonly MethodInfo _sum_LongNullable_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(long?)))
                });

        private static readonly MethodInfo _sum_Float_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(float)))
                });

        private static readonly MethodInfo _sum_FloatNullable_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(float?)))
                });

        private static readonly MethodInfo _sum_Double_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(double)))
                });

        private static readonly MethodInfo _sum_DoubleNullable_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(double?)))
                });

        private static readonly MethodInfo _sum_Decimal_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(decimal)))
                });

        private static readonly MethodInfo _sum_DecimalNullable_Selector = GetMethod(
            "Sum", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(decimal?)))
                });

        private static readonly MethodInfo _average_Int = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<int>)
                });

        private static readonly MethodInfo _average_IntNullable = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<int?>)
                });

        private static readonly MethodInfo _average_Long = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<long>)
                });

        private static readonly MethodInfo _average_LongNullable = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<long?>)
                });

        private static readonly MethodInfo _average_Float = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<float>)
                });

        private static readonly MethodInfo _average_FloatNullable = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<float?>)
                });

        private static readonly MethodInfo _average_Double = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<double>)
                });

        private static readonly MethodInfo _average_DoubleNullable = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<double?>)
                });

        private static readonly MethodInfo _average_Decimal = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<decimal>)
                });

        private static readonly MethodInfo _average_DecimalNullable = GetMethod(
            "Average", () => new[]
                {
                    typeof(IQueryable<decimal?>)
                });

        private static readonly MethodInfo _average_Int_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(int)))
                });

        private static readonly MethodInfo _average_IntNullable_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(int?)))
                });

        private static readonly MethodInfo _average_Long_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(long)))
                });

        private static readonly MethodInfo _average_LongNullable_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(long?)))
                });

        private static readonly MethodInfo _average_Float_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(float)))
                });

        private static readonly MethodInfo _average_FloatNullable_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(float?)))
                });

        private static readonly MethodInfo _average_Double_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(double)))
                });

        private static readonly MethodInfo _average_DoubleNullable_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(double?)))
                });

        private static readonly MethodInfo _average_Decimal_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(decimal)))
                });

        private static readonly MethodInfo _average_DecimalNullable_Selector = GetMethod(
            "Average", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(T, typeof(decimal?)))
                });

        private static readonly MethodInfo _aggregate = GetMethod(
            "Aggregate", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,,>).MakeGenericType(T, T, T))
                });

        private static readonly MethodInfo _aggregate_Seed = GetMethod(
            "Aggregate", (T, U) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    U,
                    typeof(Expression<>).MakeGenericType(typeof(Func<,,>).MakeGenericType(U, T, U))
                });

        private static readonly MethodInfo _aggregate_Seed_Selector = GetMethod(
            "Aggregate", (T, U, V) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    U,
                    typeof(Expression<>).MakeGenericType(typeof(Func<,,>).MakeGenericType(U, T, U)),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(U, V))
                });

        #endregion

        #region Include

        private static readonly Type[] _stringIncludeTypes = new[] { typeof(string) };

        /// <summary>
        ///     Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        ///     This extension method calls the Include(String) method of the source IQueryable object, if such a method exists.
        ///     If the source IQueryable does not have a matching method, then this method does nothing.
        ///     The Entity Framework ObjectQuery, ObjectSet, DbQuery, and DbSet types all have an appropriate Include method to call.
        ///     Paths are all-inclusive. For example, if an include call indicates Include("Orders.OrderLines"), not only will
        ///     OrderLines be included, but also Orders.  When you call the Include method, the query path is only valid on
        ///     the returned instance of the IQueryable<T>. Other instances of IQueryable<T> and the object context itself are not affected.
        ///     Because the Include method returns the query object, you can call this method multiple times on an IQueryable<T> to
        ///     specify multiple paths for the query.
        /// </remarks>
        /// <typeparam name = "T">The type of entity being queried.</typeparam>
        /// <param name = "source">The source IQueryable on which to call Include.</param>
        /// <param name = "path">The dot-separated list of related objects to return in the query results.</param>
        /// <returns>A new IQueryable<T> with the defined query path.</returns>
        public static IQueryable<T> Include<T>(this IQueryable<T> source, string path) where T : class
        {
            Contract.Requires(source != null);
            // Explicitly not checking the value of path since we don't care for the extension method.

            // We could use dynamic here, but the problem is that we want to do nothing if the method
            // isn't found or is somehow incompatible, which appears to involve catching the RuntimeBinderException
            // and ignoring it, which isn't great.  Also, if only the return type of the Include method is wrong,
            // then using dynamic will still result in the method being called before the exception is thrown.

            // Special case the types we know about to avoid reflection, then use reflection for any other
            // IQueryable that has an Include method.

            var asDbQuery = source as DbQuery<T>;
            if (asDbQuery != null)
            {
                return asDbQuery.Include(path);
            }

            var asObjectQuery = source as ObjectQuery<T>;
            if (asObjectQuery != null)
            {
                return asObjectQuery.Include(path);
            }

            return CommonInclude(source, path);
        }

        /// <summary>
        ///     Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        ///     This extension method calls the Include(String) method of the source IQueryable object, if such a method exists.
        ///     If the source IQueryable does not have a matching method, then this method does nothing.
        ///     The Entity Framework ObjectQuery, ObjectSet, DbQuery, and DbSet types all have an appropriate Include method to call.
        ///     Paths are all-inclusive. For example, if an include call indicates Include("Orders.OrderLines"), not only will
        ///     OrderLines be included, but also Orders.  When you call the Include method, the query path is only valid on
        ///     the returned instance of the IQueryable<T>. Other instances of IQueryable<T> and the object context itself are not affected.
        ///     Because the Include method returns the query object, you can call this method multiple times on an IQueryable<T> to
        ///     specify multiple paths for the query.
        /// </remarks>
        /// <param name = "source">The source IQueryable on which to call Include.</param>
        /// <param name = "path">The dot-separated list of related objects to return in the query results.</param>
        /// <returns>A new IQueryable with the defined query path.</returns>
        public static IQueryable Include(this IQueryable source, string path)
        {
            Contract.Requires(source != null);
            // Explicitly not checking the value of path since we don't care for the extension method.

            // We could use dynamic here, but the problem is that we want to do nothing if the method
            // isn't found or is somehow incompatible, which appears to involve catching the RuntimeBinderException
            // and ignoring it, which isn't great.  Also, if only the return type of the Include method is wrong,
            // then using dynamic will still result in the method being called before the exception is thrown.

            // Special case the types we know about to avoid reflection, then use reflection for any other
            // IQueryable that has an Include method.

            var asDbQuery = source as DbQuery;
            return asDbQuery != null ? asDbQuery.Include(path) : CommonInclude(source, path);
        }

        /// <summary>
        ///     Common code for generic and non-generic string Include.
        /// </summary>
        private static T CommonInclude<T>(T source, string path)
        {
            Contract.Requires(source != null);

            var includeMethod = source.GetType().GetMethod("Include", _stringIncludeTypes);
            if (includeMethod != null
                && typeof(T).IsAssignableFrom(includeMethod.ReturnType))
            {
                return (T)includeMethod.Invoke(source, new object[] { path });
            }
            return source;
        }

        /// <summary>
        ///     Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        ///     The path expression must be composed of simple property access expressions together with calls to Select for
        ///     composing additional includes after including a collection proprty.  Examples of possible include paths are:
        ///     To include a single reference: query.Include(e => e.Level1Reference)
        ///     To include a single collection: query.Include(e => e.Level1Collection)
        ///     To include a reference and then a reference one level down: query.Include(e => e.Level1Reference.Level2Reference)
        ///     To include a reference and then a collection one level down: query.Include(e => e.Level1Reference.Level2Collection)
        ///     To include a collection and then a reference one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference))
        ///     To include a collection and then a collection one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection))
        ///     To include a collection and then a reference one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference))
        ///     To include a collection and then a collection one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection))
        ///     To include a collection, a reference, and a reference two levels down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference.Level3Reference))
        ///     To include a collection, a collection, and a reference two levels down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Select(l2 => l2.Level3Reference)))
        /// 
        ///     This extension method calls the Include(String) method of the source IQueryable object, if such a method exists.
        ///     If the source IQueryable does not have a matching method, then this method does nothing.
        ///     The Entity Framework ObjectQuery, ObjectSet, DbQuery, and DbSet types all have an appropriate Include method to call.
        ///     When you call the Include method, the query path is only valid on the returned instance of the IQueryable<T>. Other
        ///     instances of IQueryable<T> and the object context itself are not affected.  Because the Include method returns the
        ///     query object, you can call this method multiple times on an IQueryable<T> to specify multiple paths for the query.
        /// </remarks>
        /// <typeparam name = "T">The type of entity being queried.</typeparam>
        /// <typeparam name = "TProperty">The type of navigation property being included.</typeparam>
        /// <param name = "source">The source IQueryable on which to call Include.</param>
        /// <param name = "path">A lambda expression representing the path to include.</param>
        /// <returns>A new IQueryable<T> with the defined query path.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IQueryable<T> Include<T, TProperty>(
            this IQueryable<T> source, Expression<Func<T, TProperty>> path) where T : class
        {
            Contract.Requires(source != null);
            Contract.Requires(path != null);

            string include;
            if (!DbHelpers.TryParsePath(path.Body, out include)
                || include == null)
            {
                throw new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path");
            }

            return Include(source, include);
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     Returns a new query where the entities returned will not be cached in the <see cref = "DbContext" />
        ///     or <see cref = "ObjectContext" />.  This method works by calling the AsNoTracking method of the
        ///     underlying query object.  If the underlying query object does not have a AsNoTracking method,
        ///     then calling this method will have no affect.
        /// </summary>
        /// <typeparam name = "T">The element type.</typeparam>
        /// <param name = "source">The source query.</param>
        /// <returns>A new query with NoTracking applied, or the source query if NoTracking is not supported.</returns>
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source) where T : class
        {
            Contract.Requires(source != null);

            var asDbQuery = source as DbQuery<T>;
            return asDbQuery != null ? asDbQuery.AsNoTracking() : CommonAsNoTracking(source);
        }

        /// <summary>
        ///     Returns a new query where the entities returned will not be cached in the <see cref = "DbContext" />
        ///     or <see cref = "ObjectContext" />.  This method works by calling the AsNoTracking method of the
        ///     underlying query object.  If the underlying query object does not have a AsNoTracking method,
        ///     then calling this method will have no affect.
        /// </summary>
        /// <param name = "source">The source query.</param>
        /// <returns>A new query with NoTracking applied, or the source query if NoTracking is not supported.</returns>
        public static IQueryable AsNoTracking(this IQueryable source)
        {
            Contract.Requires(source != null);

            var asDbQuery = source as DbQuery;
            return asDbQuery != null ? asDbQuery.AsNoTracking() : CommonAsNoTracking(source);
        }

        /// <summary>
        ///     Common code for generic and non-generic AsNoTracking.
        /// </summary>
        private static T CommonAsNoTracking<T>(T source) where T : class
        {
            Contract.Requires(source != null);

            var asObjectQuery = source as ObjectQuery;
            if (asObjectQuery != null)
            {
                return (T)DbHelpers.CreateNoTrackingQuery(asObjectQuery);
            }

            var noTrackingMethod = source.GetType().GetMethod("AsNoTracking", Type.EmptyTypes);
            if (noTrackingMethod != null
                && typeof(T).IsAssignableFrom(noTrackingMethod.ReturnType))
            {
                return (T)noTrackingMethod.Invoke(source, null);
            }

            return source;
        }

        #endregion

        #region Load

        /// <summary>
        ///     Enumerates the query such that for server queries such as those of <see cref = "DbSet{T}" />, <see cref = "ObjectSet{T}" />,
        ///     <see cref = "ObjectQuery{T}" />, and others the results of the query will be loaded into the associated <see cref = "DbContext" />,
        ///     <see cref = "ObjectContext" /> or other cache on the client.
        ///     This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name = "source">The source query.</param>
        public static void Load(this IQueryable source)
        {
            Contract.Requires(source != null);

            var enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                }
            }
            finally
            {
                var asDisposable = enumerator as IDisposable;
                if (asDisposable != null)
                {
                    asDisposable.Dispose();
                }
            }
        }

        /// <summary>
        ///     Enumerates the query asynchronously such that for server queries such as those of <see cref = "DbSet{T}" />, <see cref = "ObjectSet{T}" />,
        ///     <see cref = "ObjectQuery{T}" />, and others the results of the query will be loaded into the associated <see cref = "DbContext" />,
        ///     <see cref = "ObjectContext" /> or other cache on the client.
        ///     This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name = "source">The source query.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task LoadAsync(this IQueryable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return source.LoadAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Enumerates the query asynchronously such that for server queries such as those of <see cref = "DbSet{T}" />, <see cref = "ObjectSet{T}" />,
        ///     <see cref = "ObjectQuery{T}" />, and others the results of the query will be loaded into the associated <see cref = "DbContext" />,
        ///     <see cref = "ObjectContext" /> or other cache on the client.
        ///     This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name = "source">The source query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task LoadAsync(this IQueryable source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return source.ForEachAsync(e => { }, cancellationToken);
        }

        #endregion

        #region ForEachAsync

        /// <summary>
        ///     Enumerates the <see cref = "IQueryable" /> asynchronously and executes the provided action on each element.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task ForEachAsync(this IQueryable source, Action<object> action)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return source.ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        ///     Enumerates the <see cref = "IQueryable" /> asynchronously and executes the provided action on each element.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task ForEachAsync(this IQueryable source, Action<object> action, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            var enumerable = source as IDbAsyncEnumerable;

            if (enumerable != null)
            {
                return enumerable.ForEachAsync(action, cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Not_Async(string.Empty);
            }
        }

        /// <summary>
        ///     Enumerates the <see cref = "IQueryable" /> asynchronously and executes the provided action on each element.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name = "T">The type of entity being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return source.ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        ///     Enumerates the <see cref = "IQueryable" /> asynchronously and executes the provided action on each element.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name = "T">The type of entity being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            var enumerable = source as IDbAsyncEnumerable<T>;
            if (enumerable != null)
            {
                return enumerable.ForEachAsync(action, cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Not_Async("<" + typeof(T) + ">");
            }
        }

        #endregion

        #region Async equivalents of IEnumerable extension methods

        /// <summary>
        ///     Creates a <see cref = "List{Object}" /> from an <see cref = "IQueryable" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <returns>A Task containing a <see cref = "List{Object}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<object>> ToListAsync(this IQueryable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<List<object>>>() != null);

            return source.ToListAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref = "List{Object}" /> from an <see cref = "IQueryable" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a <see cref = "List{Object}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static async Task<List<object>> ToListAsync(this IQueryable source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            DbHelpers.ThrowIfNull(source, "source");

            var list = new List<object>();
            await source.ForEachAsync(list.Add, cancellationToken);
            return list;
        }

        /// <summary>
        ///     Creates a <see cref = "List{T}" /> from an <see cref = "IQueryable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The source query.</param>
        /// <returns>A Task containing a <see cref = "List{T}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<List<T>>>() != null);

            return source.ToListAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a <see cref = "List{T}" /> from an <see cref = "IQueryable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a <see cref = "List{T}" /> that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<List<T>>>() != null);
            DbHelpers.ThrowIfNull(source, "source");

            var list = new List<T>();
            await source.ForEachAsync(list.Add, cancellationToken);
            return list;
        }

        /// <summary>
        ///     Creates a object[] from an <see cref = "IQueryable" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <returns>A Task containing a object[] that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<object[]> ToArrayAsync(this IQueryable source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<object[]>>() != null);

            return source.ToArrayAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a object[] from an <see cref = "IQueryable" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a object[] that contains elements from the input sequence.</returns>
        public static async Task<object[]> ToArrayAsync(this IQueryable source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<object[]>>() != null);
            DbHelpers.ThrowIfNull(source, "source");

            var list = await source.ToListAsync(cancellationToken);
            return list.ToArray();
        }

        /// <summary>
        ///     Creates a T[] from an <see cref = "IQueryable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The source query.</param>
        /// <returns>A Task containing a T[] that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<T[]> ToArrayAsync<T>(this IQueryable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<T[]>>() != null);

            return source.ToArrayAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Creates a T[] from an <see cref = "IQueryable{T}" /> by enumerating it asynchronously.
        ///     If the underlying type doesn't support asynchronous enumeration it will be enumerated synchronously.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing a T[] that contains elements from the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static async Task<T[]> ToArrayAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Ensures(Contract.Result<Task<T[]>>() != null);
            DbHelpers.ThrowIfNull(source, "source");

            var list = await source.ToListAsync(cancellationToken);
            return list.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TSource}"/> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, CancellationToken.None);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TSource}"/> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, null, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TSource}"/> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, CancellationToken.None);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function and a comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TSource}"/> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TSource>>>() != null);

            return ToDictionaryAsync(source, keySelector, IdentityFunction<TSource>.Instance, comparer, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TElement}"/> that contains values of type
        /// <typeparamref name="TElement"/> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, CancellationToken.None);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TElement}"/> that contains values of type
        /// <typeparamref name="TElement"/> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TElement}"/> that contains values of type
        /// <typeparamref name="TElement"/> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            Contract.Requires(source != null);
            Contract.Requires(keySelector != null);
            Contract.Requires(elementSelector != null);
            Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);

            return ToDictionaryAsync(source, keySelector, elementSelector, comparer, CancellationToken.None);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/> by enumerating it asynchronously
        /// according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A Task containing a <see cref="Dictionary{TKey, TElement}"/> that contains values of type
        /// <typeparamref name="TElement"/> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            // TODO: Uncomment when code contracts support async
            //Contract.Requires(source != null);
            //Contract.Requires(keySelector != null);
            //Contract.Requires(elementSelector != null);
            //Contract.Ensures(Contract.Result<Task<Dictionary<TKey, TElement>>>() != null);
            DbHelpers.ThrowIfNull(source, "source");
            DbHelpers.ThrowIfNull(keySelector, "keySelector");
            DbHelpers.ThrowIfNull(elementSelector, "elementSelector");

            var d = new Dictionary<TKey, TElement>(comparer);
            await source.ForEachAsync(element => d.Add(keySelector(element), elementSelector(element)), cancellationToken);
            return d;
        }

        #endregion

        #region Async equivalents of IQueryable extension methods

        // TODO: XML comments for the methods in this region

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstAsync(CancellationToken.None);
        }

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _first.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _first_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstOrDefaultAsync(CancellationToken.None);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _firstOrDefault.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _firstOrDefault_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.LastAsync(CancellationToken.None);
        }

        public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _last.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> LastAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.LastAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> LastAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _last_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.LastOrDefaultAsync(CancellationToken.None);
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _lastOrDefault.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> LastOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.LastOrDefaultAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> LastOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _lastOrDefault_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleAsync(CancellationToken.None);
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _single.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleAsync(predicate);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _single_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _singleOrDefault.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _singleOrDefault_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> ElementAtAsync<TSource>(this IQueryable<TSource> source, int index)
        {
            Contract.Requires(source != null);
            Contract.Requires(index >= 0);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.ElementAtAsync(index, CancellationToken.None);
        }

        public static Task<TSource> ElementAtAsync<TSource>(this IQueryable<TSource> source, int index, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(index >= 0);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _elementAt.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Constant(index) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> ElementAtOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, int index)
        {
            Contract.Requires(source != null);
            Contract.Requires(index >= 0);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.ElementAtOrDefaultAsync(index, CancellationToken.None);
        }

        public static Task<TSource> ElementAtOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, int index, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(index >= 0);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _elementAtOrDefault.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Constant(index) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.ContainsAsync(item, CancellationToken.None);
        }

        public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _contains.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Constant(item, typeof(TSource)) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<bool> ContainsAsync<TSource>(
            this IQueryable<TSource> source, TSource item, IEqualityComparer<TSource> comparer)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.ContainsAsync(item, comparer, CancellationToken.None);
        }

        public static Task<bool> ContainsAsync<TSource>(
            this IQueryable<TSource> source, TSource item, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _contains_Comparer.MakeGenericMethod(typeof(TSource)),
                        new[]
                            {
                                source.Expression, Expression.Constant(item, typeof(TSource)),
                                Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))
                            }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source1.SequenceEqualAsync(source2, CancellationToken.None);
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2, CancellationToken cancellationToken)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source1.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _sequenceEqual.MakeGenericMethod(typeof(TSource)),
                        new[] { source1.Expression, GetSourceExpression(source2) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source1.SequenceEqualAsync(source2, comparer, CancellationToken.None);
        }

        public static Task<bool> SequenceEqualAsync<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source1.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _sequenceEqual_Comparer.MakeGenericMethod(typeof(TSource)),
                        new[]
                            {
                                source1.Expression,
                                GetSourceExpression(source2),
                                Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))
                            }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AnyAsync(CancellationToken.None);
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _any.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AnyAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _any_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AllAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            return source.AllAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AllAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<bool>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _all_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.CountAsync(CancellationToken.None);
        }

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                        null,
                        _count.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> CountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.CountAsync(predicate, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> CountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                        null,
                        _count_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.LongCountAsync(CancellationToken.None);
        }

        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long>(
                    Expression.Call(
                        null,
                        _longCount.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> LongCountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.LongCountAsync(predicate);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> LongCountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long>(
                    Expression.Call(
                        null,
                        _longCount_Predicate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(predicate) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.MinAsync(CancellationToken.None);
        }

        public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _min.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MinAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            return source.MinAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MinAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        null,
                        _min_Selector.MakeGenericMethod(typeof(TSource), typeof(TResult)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.MaxAsync(CancellationToken.None);
        }

        public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _max.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MaxAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.MaxAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MaxAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        null,
                        _max_Selector.MakeGenericMethod(typeof(TSource), typeof(TResult)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<int> SumAsync(this IQueryable<int> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                        null,
                        _sum_Int,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync(this IQueryable<int?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<int?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int?>(
                    Expression.Call(
                        null,
                        _sum_IntNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<long> SumAsync(this IQueryable<long> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        public static Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long>(
                    Expression.Call(
                        null,
                        _sum_Long,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync(this IQueryable<long?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<long?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long?>(
                    Expression.Call(
                        null,
                        _sum_LongNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<float> SumAsync(this IQueryable<float> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float>(
                    Expression.Call(
                        null,
                        _sum_Float,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync(this IQueryable<float?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float?>(
                    Expression.Call(
                        null,
                        _sum_FloatNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<double> SumAsync(this IQueryable<double> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        public static Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _sum_Double,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync(this IQueryable<double?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _sum_DoubleNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<decimal> SumAsync(this IQueryable<decimal> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal>(
                    Expression.Call(
                        null,
                        _sum_Decimal,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync(this IQueryable<decimal?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.SumAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal?>(
                    Expression.Call(
                        null,
                        _sum_DecimalNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<int>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                        null,
                        _sum_Int_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<int?>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<int?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int?>(
                    Expression.Call(
                        null,
                        _sum_IntNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<long>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long>(
                    Expression.Call(
                        null,
                        _sum_Long_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<long?>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<long?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<long?>(
                    Expression.Call(
                        null,
                        _sum_LongNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float>(
                    Expression.Call(
                        null,
                        _sum_Float_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float?>(
                    Expression.Call(
                        null,
                        _sum_FloatNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _sum_Double_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _sum_DoubleNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal>(
                    Expression.Call(
                        null,
                        _sum_Decimal_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.SumAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal?>(
                    Expression.Call(
                        null,
                        _sum_DecimalNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<double> AverageAsync(this IQueryable<int> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        public static Task<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Int,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<int?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_IntNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<double> AverageAsync(this IQueryable<long> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        public static Task<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Long,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<long?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_LongNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<float> AverageAsync(this IQueryable<float> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        public static Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float>(
                    Expression.Call(
                        null,
                        _average_Float,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync(this IQueryable<float?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float?>(
                    Expression.Call(
                        null,
                        _average_FloatNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<double> AverageAsync(this IQueryable<double> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        public static Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Double,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<double?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_DoubleNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        public static Task<decimal> AverageAsync(this IQueryable<decimal> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        public static Task<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal>(
                    Expression.Call(
                        null,
                        _average_Decimal,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.AverageAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal?>(
                    Expression.Call(
                        null,
                        _average_DecimalNullable,
                        new[] { source.Expression }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Int_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_IntNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Long_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_LongNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float>(
                    Expression.Call(
                        null,
                        _average_Float_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<float?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<float?>(
                    Expression.Call(
                        null,
                        _average_FloatNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double>(
                    Expression.Call(
                        null,
                        _average_Double_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<double?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<double?>(
                    Expression.Call(
                        null,
                        _average_DoubleNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal>(
                    Expression.Call(
                        null,
                        _average_Decimal_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            return source.AverageAsync(selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<decimal?>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<decimal?>(
                    Expression.Call(
                        null,
                        _average_DecimalNullable_Selector.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> AggregateAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, TSource, TSource>> func)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.AggregateAsync(func, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> AggregateAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, TSource, TSource>> func, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _aggregate.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression, Expression.Quote(func) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(
            this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.AggregateAsync(seed, func, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(
            this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func,
            CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TAccumulate>(
                    Expression.Call(
                        null,
                        _aggregate_Seed.MakeGenericMethod(typeof(TSource), typeof(TAccumulate)),
                        new[] { source.Expression, Expression.Constant(seed), Expression.Quote(func) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(
            this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func,
            Expression<Func<TAccumulate, TResult>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            return source.AggregateAsync(seed, func, selector, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(
            this IQueryable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func,
            Expression<Func<TAccumulate, TResult>> selector, CancellationToken cancellationToken)
        {
            Contract.Requires(source != null);
            Contract.Requires(func != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TSource>>() != null);

            var provider = source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        null,
                        _aggregate_Seed_Selector.MakeGenericMethod(typeof(TSource), typeof(TAccumulate), typeof(TResult)),
                        new[] { source.Expression, Expression.Constant(seed), Expression.Quote(func), Expression.Quote(selector) }
                        ),
                    cancellationToken);
            }
            else
            {
                throw Error.IQueryable_Provider_Not_Async();
            }
        }

        #endregion

        #region Private methods

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
        {
            var q = source as IQueryable<TSource>;
            if (q != null)
            {
                return q.Expression;
            }
            return Expression.Constant(source, typeof(IEnumerable<TSource>));
        }

        private static MethodInfo GetMethod(string methodName, Func<Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 0);
        }

        private static MethodInfo GetMethod(string methodName, Func<Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 1);
        }

        private static MethodInfo GetMethod(string methodName, Func<Type, Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 2);
        }

        private static MethodInfo GetMethod(string methodName, Func<Type, Type, Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 3);
        }

        private static MethodInfo GetMethod(string methodName, MethodInfo getParameterTypesMethod, int genericArgumentsCount)
        {
            var candidates = typeof(Queryable).GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);

            foreach (MethodInfo candidate in candidates)
            {
                var genericArguments = candidate.GetGenericArguments();
                if (genericArguments.Length == genericArgumentsCount
                    && Matches(candidate, (Type[])getParameterTypesMethod.Invoke(null, genericArguments)))
                {
                    return candidate;
                }
            }

            Contract.Assert(
                false,
                String.Format(
                    "Method '{0}' with parameters '{1}' not found", methodName, PrettyPrint(getParameterTypesMethod, genericArgumentsCount)));

            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called from an assert")]
        private static string PrettyPrint(MethodInfo getParameterTypesMethod, int genericArgumentsCount)
        {
            var dummyTypes = new Type[genericArgumentsCount];
            for (var i = 0; i < genericArgumentsCount; i++)
            {
                // TODO: Replace the dummy types with T1, T2, etc.
                dummyTypes[i] = typeof(object);
            }

            var parameterTypes = (Type[])getParameterTypesMethod.Invoke(null, dummyTypes);
            var textRepresentations = new string[parameterTypes.Length];

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                textRepresentations[i] = parameterTypes[i].ToString();
            }

            return "(" + string.Join(", ", textRepresentations) + ")";
        }

        private static bool Matches(MethodInfo methodInfo, Type[] parameterTypes)
        {
            return methodInfo.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes);
        }

        #endregion

        #region Nested classes

        private static class IdentityFunction<TElement>
        {
            public static Func<TElement, TElement> Instance
            {
                get { return x => x; }
            }
        }

        #endregion
    }
}
