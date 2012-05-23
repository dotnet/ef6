namespace System.Data.Entity
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    public static class IQueryableExtensions
    {
        #region Include

        private static readonly Type[] StringIncludeTypes = new[] { typeof(string) };

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
        ///                                                                                  Because the Include method returns the query object, you can call this method multiple times on an IQueryable<T> to
        ///                                                                                                                                                                                                   specify multiple paths for the query.
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
        ///                                                                                  Because the Include method returns the query object, you can call this method multiple times on an IQueryable<T> to
        ///                                                                                                                                                                                                   specify multiple paths for the query.
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
            var includeMethod = source.GetType().GetMethod("Include", StringIncludeTypes);
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
        ///                                                                                                                  instances of IQueryable<T> and the object context itself are not affected.  Because the Include method returns the
        ///                                                                                                                                             query object, you can call this method multiple times on an IQueryable<T> to specify multiple paths for the query.
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

        #endregion
    }
}
