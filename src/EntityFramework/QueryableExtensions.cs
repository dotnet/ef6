// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Useful extension methods for use with Entity Framework LINQ queries.
    /// </summary>
    public static class QueryableExtensions
    {
        #region Private static fields

#if !NET40

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

        private static readonly MethodInfo _contains = GetMethod(
            "Contains", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    T
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

#endif

        #endregion

        #region Include

        private static readonly Type[] _stringIncludeTypes = new[] { typeof(string) };

        /// <summary>
        /// Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        /// This extension method calls the Include(String) method of the source <see cref="IQueryable{T}" /> object,
        /// if such a method exists. If the source <see cref="IQueryable{T}" /> does not have a matching method,
        /// then this method does nothing. The <see cref="ObjectQuery{T}" />, <see cref="ObjectSet{T}" />,
        /// <see cref="DbQuery{TResult}" /> and <see cref="DbSet{T}" /> types all have an appropriate Include method to call.
        /// Paths are all-inclusive. For example, if an include call indicates Include("Orders.OrderLines"), not only will
        /// OrderLines be included, but also Orders.  When you call the Include method, the query path is only valid on
        /// the returned instance of the <see cref="IQueryable{T}" />. Other instances of <see cref="IQueryable{T}" />
        /// and the object context itself are not affected. Because the Include method returns the query object,
        /// you can call this method multiple times on an <see cref="IQueryable{T}" /> to specify multiple paths for the query.
        /// </remarks>
        /// <typeparam name="T"> The type of entity being queried. </typeparam>
        /// <param name="source">
        /// The source <see cref="IQueryable{T}" /> on which to call Include.
        /// </param>
        /// <param name="path"> The dot-separated list of related objects to return in the query results. </param>
        /// <returns>
        /// A new <see cref="IQueryable{T}" /> with the defined query path.
        /// </returns>
        public static IQueryable<T> Include<T>(this IQueryable<T> source, string path) where T : class
        {
            Check.NotNull(source, "source");
            Check.NotEmpty(path, "path");

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
        /// Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        /// This extension method calls the Include(String) method of the source <see cref="IQueryable" /> object,
        /// if such a method exists. If the source <see cref="IQueryable" /> does not have a matching method,
        /// then this method does nothing. The <see cref="ObjectQuery" />, <see cref="ObjectSet{T}" />,
        /// <see cref="DbQuery" /> and <see cref="DbSet" /> types all have an appropriate Include method to call.
        /// Paths are all-inclusive. For example, if an include call indicates Include("Orders.OrderLines"), not only will
        /// OrderLines be included, but also Orders.  When you call the Include method, the query path is only valid on
        /// the returned instance of the <see cref="IQueryable" />. Other instances of <see cref="IQueryable" />
        /// and the object context itself are not affected. Because the Include method returns the query object,
        /// you can call this method multiple times on an <see cref="IQueryable" /> to specify multiple paths for the query.
        /// </remarks>
        /// <param name="source">
        /// The source <see cref="IQueryable" /> on which to call Include.
        /// </param>
        /// <param name="path"> The dot-separated list of related objects to return in the query results. </param>
        /// <returns>
        /// A new <see cref="IQueryable" /> with the defined query path.
        /// </returns>
        public static IQueryable Include(this IQueryable source, string path)
        {
            Check.NotNull(source, "source");
            Check.NotEmpty(path, "path");

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
        /// Common code for generic and non-generic string Include.
        /// </summary>
        private static T CommonInclude<T>(T source, string path)
        {
            DebugCheck.NotNull((object)source);

            var includeMethod = source.GetType().GetPublicInstanceMethod("Include", _stringIncludeTypes);
            if (includeMethod != null
                && typeof(T).IsAssignableFrom(includeMethod.ReturnType))
            {
                return (T)includeMethod.Invoke(source, new object[] { path });
            }
            return source;
        }

        /// <summary>
        /// Specifies the related objects to include in the query results.
        /// </summary>
        /// <remarks>
        /// The path expression must be composed of simple property access expressions together with calls to Select for
        /// composing additional includes after including a collection proprty.  Examples of possible include paths are:
        /// To include a single reference: query.Include(e => e.Level1Reference)
        /// To include a single collection: query.Include(e => e.Level1Collection)
        /// To include a reference and then a reference one level down: query.Include(e => e.Level1Reference.Level2Reference)
        /// To include a reference and then a collection one level down: query.Include(e => e.Level1Reference.Level2Collection)
        /// To include a collection and then a reference one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference))
        /// To include a collection and then a collection one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection))
        /// To include a collection and then a reference one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference))
        /// To include a collection and then a collection one level down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection))
        /// To include a collection, a reference, and a reference two levels down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference.Level3Reference))
        /// To include a collection, a collection, and a reference two levels down: query.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Select(l2 => l2.Level3Reference)))
        /// This extension method calls the Include(String) method of the source IQueryable object, if such a method exists.
        /// If the source IQueryable does not have a matching method, then this method does nothing.
        /// The Entity Framework ObjectQuery, ObjectSet, DbQuery, and DbSet types all have an appropriate Include method to call.
        /// When you call the Include method, the query path is only valid on the returned instance of the IQueryable&lt;T&gt;. Other
        /// instances of IQueryable&lt;T&gt; and the object context itself are not affected.  Because the Include method returns the
        /// query object, you can call this method multiple times on an IQueryable&lt;T&gt; to specify multiple paths for the query.
        /// </remarks>
        /// <typeparam name="T"> The type of entity being queried. </typeparam>
        /// <typeparam name="TProperty"> The type of navigation property being included. </typeparam>
        /// <param name="source"> The source IQueryable on which to call Include. </param>
        /// <param name="path"> A lambda expression representing the path to include. </param>
        /// <returns>
        /// A new IQueryable&lt;T&gt; with the defined query path.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IQueryable<T> Include<T, TProperty>(
            this IQueryable<T> source, Expression<Func<T, TProperty>> path) where T : class
        {
            Check.NotNull(source, "source");
            Check.NotNull(path, "path");

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
        /// Returns a new query where the entities returned will not be cached in the <see cref="DbContext" />
        /// or <see cref="ObjectContext" />.  This method works by calling the AsNoTracking method of the
        /// underlying query object.  If the underlying query object does not have an AsNoTracking method,
        /// then calling this method will have no affect.
        /// </summary>
        /// <typeparam name="T"> The element type. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <returns> A new query with NoTracking applied, or the source query if NoTracking is not supported. </returns>
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source) where T : class
        {
            Check.NotNull(source, "source");

            var asDbQuery = source as DbQuery<T>;
            return asDbQuery != null ? asDbQuery.AsNoTracking() : CommonAsNoTracking(source);
        }

        /// <summary>
        /// Returns a new query where the entities returned will not be cached in the <see cref="DbContext" />
        /// or <see cref="ObjectContext" />.  This method works by calling the AsNoTracking method of the
        /// underlying query object.  If the underlying query object does not have an AsNoTracking method,
        /// then calling this method will have no affect.
        /// </summary>
        /// <param name="source"> The source query. </param>
        /// <returns> A new query with NoTracking applied, or the source query if NoTracking is not supported. </returns>
        public static IQueryable AsNoTracking(this IQueryable source)
        {
            Check.NotNull(source, "source");

            var asDbQuery = source as DbQuery;
            return asDbQuery != null ? asDbQuery.AsNoTracking() : CommonAsNoTracking(source);
        }

        /// <summary>
        /// Common code for generic and non-generic AsNoTracking.
        /// </summary>
        private static T CommonAsNoTracking<T>(T source) where T : class
        {
            DebugCheck.NotNull(source);

            var asObjectQuery = source as ObjectQuery;
            if (asObjectQuery != null)
            {
                return (T)DbHelpers.CreateNoTrackingQuery(asObjectQuery);
            }

            var noTrackingMethod = source.GetType().GetPublicInstanceMethod("AsNoTracking", Type.EmptyTypes);
            if (noTrackingMethod != null
                && typeof(T).IsAssignableFrom(noTrackingMethod.ReturnType))
            {
                return (T)noTrackingMethod.Invoke(source, null);
            }

            return source;
        }

        #endregion

        #region AsStreaming

        /// <summary>
        /// Returns a new query that will stream the results instead of buffering. This method works by calling
        /// the AsStreaming method of the underlying query object. If the underlying query object does not have
        /// an AsStreaming method, then calling this method will have no affect.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to apply AsStreaming to.
        /// </param>
        /// <returns> A new query with AsStreaming applied, or the source query if AsStreaming is not supported. </returns>
        public static IQueryable<T> AsStreaming<T>(this IQueryable<T> source)
        {
            Check.NotNull(source, "source");

            var asDbQuery = source as DbQuery<T>;
            return asDbQuery != null ? asDbQuery.AsStreaming() : CommonAsStreaming(source);
        }

        /// <summary>
        /// Returns a new query that will stream the results instead of buffering. This method works by calling
        /// the AsStreaming method of the underlying query object. If the underlying query object does not have
        /// an AsStreaming method, then calling this method will have no affect.
        /// </summary>
        /// <param name="source">
        /// An <see cref="IQueryable" /> to apply AsStreaming to.
        /// </param>
        /// <returns> A new query with AsStreaming applied, or the source query if AsStreaming is not supported. </returns>
        public static IQueryable AsStreaming(this IQueryable source)
        {
            Check.NotNull(source, "source");

            var asDbQuery = source as DbQuery;
            return asDbQuery != null ? asDbQuery.AsStreaming() : CommonAsStreaming(source);
        }

        private static T CommonAsStreaming<T>(T source) where T : class
        {
            DebugCheck.NotNull(source);

            var asObjectQuery = source as ObjectQuery;
            if (asObjectQuery != null)
            {
                return (T)DbHelpers.CreateStreamingQuery(asObjectQuery);
            }

            var asStreamingMethod = source.GetType().GetPublicInstanceMethod("AsStreaming", Type.EmptyTypes);
            if (asStreamingMethod != null
                && typeof(T).IsAssignableFrom(asStreamingMethod.ReturnType))
            {
                return (T)asStreamingMethod.Invoke(source, null);
            }

            return source;
        }

        #endregion

        #region Load

        /// <summary>
        /// Enumerates the query such that for server queries such as those of <see cref="DbSet{T}" />,
        /// <see
        ///     cref="ObjectSet{T}" />
        /// ,
        /// <see cref="ObjectQuery{T}" />, and others the results of the query will be loaded into the associated
        /// <see
        ///     cref="DbContext" />
        /// ,
        /// <see cref="ObjectContext" /> or other cache on the client.
        /// This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name="source"> The source query. </param>
        public static void Load(this IQueryable source)
        {
            Check.NotNull(source, "source");

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

#if !NET40

        /// <summary>
        /// Asynchronously enumerates the query such that for server queries such as those of <see cref="DbSet{T}" />,
        /// <see
        ///     cref="ObjectSet{T}" />
        /// ,
        /// <see cref="ObjectQuery{T}" />, and others the results of the query will be loaded into the associated
        /// <see
        ///     cref="DbContext" />
        /// ,
        /// <see cref="ObjectContext" /> or other cache on the client.
        /// This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name="source"> The source query. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public static Task LoadAsync(this IQueryable source)
        {
            Check.NotNull(source, "source");

            return source.LoadAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously enumerates the query such that for server queries such as those of <see cref="DbSet{T}" />,
        /// <see
        ///     cref="ObjectSet{T}" />
        /// ,
        /// <see cref="ObjectQuery{T}" />, and others the results of the query will be loaded into the associated
        /// <see
        ///     cref="DbContext" />
        /// ,
        /// <see cref="ObjectContext" /> or other cache on the client.
        /// This is equivalent to calling ToList and then throwing away the list without the overhead of actually creating the list.
        /// </summary>
        /// <param name="source"> The source query. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public static Task LoadAsync(this IQueryable source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.ForEachAsync(e => { }, cancellationToken);
        }

#endif

        #endregion

        #region ForEachAsync

#if !NET40

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IQueryable" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task ForEachAsync(this IQueryable source, Action<object> action)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsDbAsyncEnumerable().ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IQueryable" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task ForEachAsync(this IQueryable source, Action<object> action, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsDbAsyncEnumerable().ForEachAsync(action, cancellationToken);
        }

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsDbAsyncEnumerable().ForEachAsync(action, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task ForEachAsync<T>(this IQueryable<T> source, Action<T> action, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsDbAsyncEnumerable().ForEachAsync(action, cancellationToken);
        }

#endif

        #endregion

        #region Async equivalents of IEnumerable extension methods

#if !NET40

        /// <summary>
        /// Creates a <see cref="List{Object}" /> from an <see cref="IQueryable" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IQueryable" /> to create a <see cref="List{Object}" /> from.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{Object}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<object>> ToListAsync(this IQueryable source)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToListAsync<object>();
        }

        /// <summary>
        /// Creates a <see cref="List{Object}" /> from an <see cref="IQueryable" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IQueryable" /> to create a <see cref="List{Object}" /> from.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{Object}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<object>> ToListAsync(this IQueryable source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToListAsync<object>(cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="List{T}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="List{T}" /> from.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToListAsync();
        }

        /// <summary>
        /// Creates a <see cref="List{T}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a list from.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Creates an array from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create an array from.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an array that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToArrayAsync();
        }

        /// <summary>
        /// Creates an array from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create an array from.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an array that contains elements from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.AsDbAsyncEnumerable().ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, comparer);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, comparer, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        /// The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        /// The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        /// The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, comparer);
        }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it asynchronously
        /// according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        /// The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsDbAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
        }

#endif

        #endregion

        #region Async equivalents of IQueryable extension methods

#if !NET40

        /// <summary>
        /// Asynchronously returns the first element of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" /> doesn't implement <see cref="IDbAsyncQueryProvider" />.
        /// </exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.FirstAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" /> that passes the test in
        /// <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// No element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.FirstAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" /> that passes the test in
        /// <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// No element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if
        /// <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.FirstOrDefaultAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if
        /// <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition
        /// or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if <paramref name="source" />
        /// is empty or if no element passes the test specified by <paramref name="predicate" /> ; otherwise, the first
        /// element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.FirstOrDefaultAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition
        /// or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if <paramref name="source" />
        /// is empty or if no element passes the test specified by <paramref name="predicate" /> ; otherwise, the first
        /// element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// has more than one element.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns the only element of a sequence, and throws an exception
        /// if there is not exactly one element in the sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.SingleAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the only element of a sequence, and throws an exception
        /// if there is not exactly one element in the sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// has more than one element.
        /// </exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns the only element of a sequence that satisfies a specified condition,
        /// and throws an exception if more than one such element exists.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence that satisfies the condition in
        /// <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// No element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// More than one element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.SingleAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the only element of a sequence that satisfies a specified condition,
        /// and throws an exception if more than one such element exists.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence that satisfies the condition in
        /// <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// No element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// More than one element satisfies the condition in
        /// <paramref name="predicate" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
        /// this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence, or <c>default</c> (<typeparamref name="TSource" />)
        /// if the sequence contains no elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// has more than one element.
        /// </exception>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
        /// this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence, or <c>default</c> (<typeparamref name="TSource" />)
        /// if the sequence contains no elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// has more than one element.
        /// </exception>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns the only element of a sequence that satisfies a specified condition or
        /// a default value if no such element exists; this method throws an exception if more than one element
        /// satisfies the condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence that satisfies the condition in
        /// <paramref name="predicate" />, or <c>default</c> ( <typeparamref name="TSource" /> ) if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the only element of a sequence that satisfies a specified condition or
        /// a default value if no such element exists; this method throws an exception if more than one element
        /// satisfies the condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the single element of the input sequence that satisfies the condition in
        /// <paramref name="predicate" />, or <c>default</c> ( <typeparamref name="TSource" /> ) if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="item"> The object to locate in the sequence. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the input sequence contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item)
        {
            Check.NotNull(source, "source");

            return source.ContainsAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="item"> The object to locate in the sequence. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the input sequence contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously determines whether a sequence contains any elements.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to check for being empty.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the source sequence contains any elements; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.AnyAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously determines whether a sequence contains any elements.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> to check for being empty.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the source sequence contains any elements; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.AnyAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously determines whether all the elements of a sequence satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if every element of the source sequence passes the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AllAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.AllAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously determines whether all the elements of a sequence satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if every element of the source sequence passes the test in the specified predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<bool> AllAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns the number of elements in a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.CountAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the number of elements in a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the sequence that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// that satisfy the condition in the predicate function
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> CountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.CountAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the sequence that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// that satisfy the condition in the predicate function
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> CountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns an <see cref="Int64" /> that represents the total number of elements in a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.LongCountAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns an <see cref="Int64" /> that represents the total number of elements in a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously returns an <see cref="Int64" /> that represents the number of elements in a sequence
        /// that satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the sequence that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// that satisfy the condition in the predicate function
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> LongCountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return source.LongCountAsync(predicate, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns an <see cref="Int64" /> that represents the number of elements in a sequence
        /// that satisfy a condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of elements in the sequence that satisfy the condition in the predicate function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="predicate" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// that satisfy the condition in the predicate function
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> LongCountAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

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

        /// <summary>
        /// Asynchronously returns the minimum value of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the minimum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.MinAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the minimum value of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the minimum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously invokes a projection function on each element of a sequence and returns the minimum resulting value.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the minimum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MinAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.MinAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously invokes a projection function on each element of a sequence and returns the minimum resulting value.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the minimum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MinAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously returns the maximum value of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the maximum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.MaxAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously returns the maximum value of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the maximum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously invokes a projection function on each element of a sequence and returns the maximum resulting value.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the maximum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MaxAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.MaxAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously invokes a projection function on each element of a sequence and returns the maximum resulting value.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the maximum value in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<TResult> MaxAsync<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int32" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains  the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        public static Task<int> SumAsync(this IQueryable<int> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int32" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync(this IQueryable<int?> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int64" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        public static Task<long> SumAsync(this IQueryable<long> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int64" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        public static Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync(this IQueryable<long?> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Single" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<float> SumAsync(this IQueryable<float> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Single" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync(this IQueryable<float?> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Double" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<double> SumAsync(this IQueryable<double> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Double" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync(this IQueryable<double?> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Decimal" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<decimal> SumAsync(this IQueryable<decimal> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Decimal" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal" /> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync(this IQueryable<decimal?> source)
        {
            Check.NotNull(source, "source");

            return source.SumAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of a sequence of nullable <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal" /> values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the values in the sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Decimal.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int32.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<int?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Int64.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<long?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Decimal.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Decimal.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Decimal.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.SumAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the sum of the sequence of nullable <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values of type <typeparamref name="TSource" /> .
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the sum of the projected values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="OverflowException">
        /// The number of elements in
        /// <paramref name="source" />
        /// is larger than
        /// <see cref="Decimal.MaxValue" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> SumAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int32" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<int> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int32" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<int?> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int32" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int64" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<long> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Int64" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<long?> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int64" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Single" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<float> AverageAsync(this IQueryable<float> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Single" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync(this IQueryable<float?> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Single" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Double" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<double> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Double" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<double?> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Double" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Decimal" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<decimal> AverageAsync(this IQueryable<decimal> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Decimal" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        public static Task<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal" /> values to calculate the average of.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source)
        {
            Check.NotNull(source, "source");

            return source.AverageAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Decimal" /> values.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal" /> values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int32" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Int64" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Single" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<float?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Double" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<double?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// contains no elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return source.AverageAsync(selector, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously computes the average of a sequence of nullable <see cref="Decimal" /> values that is obtained
        /// by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values to calculate the average of. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the average of the sequence of values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" />
        /// or
        /// <paramref name="selector" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="source" />
        /// doesn't implement
        /// <see cref="IDbAsyncQueryProvider" />
        /// .
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<decimal?> AverageAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

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

#endif

        #endregion

        #region Paging
        private static readonly MethodInfo _skip = GetMethod(
            "Skip", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(int)
                });

        private static readonly MethodInfo _take = GetMethod(
            "Take", (T) => new[]
                {
                    typeof(IQueryable<>).MakeGenericType(T),
                    typeof(int)
                });

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence to return elements from.</param>
        /// <param name="countAccessor">An expression that evaluates to the number of elements to skip.</param>
        /// <returns>A sequence that contains elements that occur after the specified index in the 
        /// input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IQueryable<TSource> Skip<TSource>(
            this IQueryable<TSource> source, Expression<Func<int>> countAccessor)
        {
            Check.NotNull(source, "source");
            Check.NotNull(countAccessor, "countAccessor");

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _skip.MakeGenericMethod(new[] { typeof(TSource) }),
                    new[] { source.Expression, countAccessor.Body }));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="countAccessor">An expression that evaluates to the number of elements 
        /// to return.</param>
        /// <returns>A sequence that contains the specified number of elements from the 
        /// start of the input sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IQueryable<TSource> Take<TSource>(
            this IQueryable<TSource> source, Expression<Func<int>> countAccessor)
        {
            Check.NotNull(source, "source");
            Check.NotNull(countAccessor, "countAccessor");

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    _take.MakeGenericMethod(new[] { typeof(TSource) }),
                    new[] { source.Expression, countAccessor.Body }));
        }
        #endregion

        #region Private and internal methods

        internal static ObjectQuery TryGetObjectQuery(this IQueryable source)
        {
            if (source == null)
            {
                return null;
            }

            var direct = source as ObjectQuery;
            if (direct != null)
            {
                return direct;
            }

            var indirect = source as IInternalQueryAdapter;
            if (indirect != null)
            {
                return indirect.InternalQuery.ObjectQuery;
            }

            return null;
        }

#if !NET40

        private static IDbAsyncEnumerable AsDbAsyncEnumerable(this IQueryable source)
        {
            DebugCheck.NotNull(source);

            var enumerable = source as IDbAsyncEnumerable;
            if (enumerable != null)
            {
                return enumerable;
            }
            else
            {
                throw Error.IQueryable_Not_Async(string.Empty);
            }
        }

        private static IDbAsyncEnumerable<T> AsDbAsyncEnumerable<T>(this IQueryable<T> source)
        {
            DebugCheck.NotNull(source);

            var enumerable = source as IDbAsyncEnumerable<T>;
            if (enumerable != null)
            {
                return enumerable;
            }
            else
            {
                throw Error.IQueryable_Not_Async("<" + typeof(T) + ">");
            }
        }

        private static MethodInfo GetMethod(string methodName, Func<Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 0);
        }

        private static MethodInfo GetMethod(string methodName, Func<Type, Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 2);
        }

#endif

        private static MethodInfo GetMethod(string methodName, Func<Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.Method, 1);
        }

        private static MethodInfo GetMethod(string methodName, MethodInfo getParameterTypesMethod, int genericArgumentsCount)
        {
            var candidates = typeof(Queryable).GetDeclaredMethods(methodName);

            foreach (MethodInfo candidate in candidates)
            {
                var genericArguments = candidate.GetGenericArguments();
                if (genericArguments.Length == genericArgumentsCount
                    && Matches(candidate, (Type[])getParameterTypesMethod.Invoke(null, genericArguments)))
                {
                    return candidate;
                }
            }

            Debug.Assert(
                false, String.Format(
                    "Method '{0}' with parameters '{1}' not found", methodName, PrettyPrint(getParameterTypesMethod, genericArgumentsCount)));

            return null;
        }

        private static bool Matches(MethodInfo methodInfo, Type[] parameterTypes)
        {
            return methodInfo.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called from an assert")]
        private static string PrettyPrint(MethodInfo getParameterTypesMethod, int genericArgumentsCount)
        {
            var dummyTypes = new Type[genericArgumentsCount];
            for (var i = 0; i < genericArgumentsCount; i++)
            {
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

        #endregion
    }
}
