// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ObjectQuery implements strongly-typed queries at the object-layer.
    /// Queries are specified using Entity-SQL strings and may be created by calling
    /// the Entity-SQL-based query builder methods declared by ObjectQuery.
    /// </summary>
    /// <typeparam name="T"> The result type of this ObjectQuery </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectQuery<T> : ObjectQuery, IOrderedQueryable<T>, IEnumerable<T>
#if !NET40
                                  , IDbAsyncEnumerable<T>
#endif
    {
        #region Private Static Members

        /// <summary>
        /// The default query name, which is used in query-building to refer to an
        /// element of the ObjectQuery; e.g., in a call to ObjectQuery.Where(), a predicate of
        /// the form "it.Name = 'Foo'" can be specified, where "it" refers to a T.
        /// Note that the query name may eventually become a parameter in the command
        /// tree, so it must conform to the parameter name restrictions enforced by
        /// ObjectParameter.ValidateParameterName(string).
        /// </summary>
        private const string DefaultName = "it";

        private static bool IsLinqQuery(ObjectQuery query)
        {
            return query.QueryState is ELinqQueryState;
        }

        #endregion

        #region Private Instance Fields

        /// <summary>
        /// The name of the current sequence, which defaults to "it". Used in query-
        /// builder methods that process an Entity-SQL command text fragment to refer to an
        /// instance of the return type of this query.
        /// </summary>
        private string _name = DefaultName;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance using the specified Entity SQL command as the initial query.
        /// </summary>
        /// <param name="commandText">The Entity SQL query.</param>
        /// <param name="context">
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> on which to execute the query.
        /// </param>
        public ObjectQuery(string commandText, ObjectContext context)
            : this(new EntitySqlQueryState(typeof(T), commandText, false, context, null, null))
        {
            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance using the specified Entity SQL command as the initial query and the specified merge option.
        /// </summary>
        /// <param name="commandText">The Entity SQL query.</param>
        /// <param name="context">
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> on which to execute the query.
        /// </param>
        /// <param name="mergeOption">
        /// Specifies how the entities that are retrieved through this query should be merged with the entities that have been returned from previous queries against the same
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        public ObjectQuery(string commandText, ObjectContext context, MergeOption mergeOption)
            : this(new EntitySqlQueryState(typeof(T), commandText, false, context, null, null))
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            QueryState.UserSpecifiedMergeOption = mergeOption;

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// This method creates a new ObjectQuery instance that represents a scan over
        /// the specified <paramref name="entitySet" />. This ObjectQuery carries the scan as <see cref="DbExpression" />
        /// and as Entity SQL. This is needed to allow case-sensitive metadata access (provided by the <see cref="DbExpression" /> by default).
        /// The context specifies the connection on which to execute the query as well as the metadata and result cache.
        /// The merge option specifies how the cache should be populated/updated.
        /// </summary>
        /// <param name="entitySet"> The entity set this query scans. </param>
        /// <param name="context">
        /// The ObjectContext containing the metadata workspace the query will be built against, the connection
        /// on which to execute the query, and the cache to store the results in.
        /// </param>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <returns> A new ObjectQuery instance. </returns>
        internal ObjectQuery(EntitySetBase entitySet, ObjectContext context, MergeOption mergeOption)
            : this(new EntitySqlQueryState(typeof(T), BuildScanEntitySetEsql(entitySet), entitySet.Scan(), false, context, null, null))
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            QueryState.UserSpecifiedMergeOption = mergeOption;

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), Assembly.GetCallingAssembly());
        }

        private static string BuildScanEntitySetEsql(EntitySetBase entitySet)
        {
            DebugCheck.NotNull(entitySet);
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name),
                EntityUtil.QuoteIdentifier(entitySet.Name));
        }

        internal ObjectQuery(ObjectQueryState queryState)
            : base(queryState)
        {
        }

        /// <summary>
        /// For testing.
        /// </summary>
        internal ObjectQuery()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>Gets or sets the name of this object query.</summary>
        /// <returns>
        /// A string value that is the name of this <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The value specified on set is not valid.</exception>
        public string Name
        {
            get { return _name; }
            set
            {
                Check.NotNull(value, "value");

                if (!ObjectParameter.ValidateParameterName(value))
                {
                    throw new ArgumentException(Strings.ObjectQuery_InvalidQueryName(value), "value");
                }

                _name = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Executes the object query with the specified merge option.</summary>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> that contains a collection of entity objects returned by the query.
        /// </returns>
        public new ObjectResult<T> Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return GetResults(mergeOption);
        }

#if !NET40

        /// <summary>
        /// Asynchronously executes the object query with the specified merge option.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> 
        /// that contains a collection of entity objects returned by the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption)
        {
            return ExecuteAsync(mergeOption, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously executes the object query with the specified merge option.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> 
        /// that contains a collection of entity objects returned by the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);

            return GetResultsAsync(mergeOption, cancellationToken);
        }

#endif

        /// <summary>Specifies the related objects to include in the query results.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> with the defined query path.
        /// </returns>
        /// <param name="path">Dot-separated list of related objects to return in the query results.</param>
        /// <exception cref="T:System.ArgumentNullException"> path  is null.</exception>
        /// <exception cref="T:System.ArgumentException"> path  is empty.</exception>
        public ObjectQuery<T> Include(string path)
        {
            Check.NotEmpty(path, "path");
            return new ObjectQuery<T>(QueryState.Include(this, path));
        }

        #region Query-builder Methods

        // ---------------------
        // Query-builder Methods
        // ---------------------

        /// <summary>Limits the query to unique results.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with SELECT DISTINCT applied.
        /// </returns>
        public ObjectQuery<T> Distinct()
        {
            if (IsLinqQuery(this))
            {
                return (ObjectQuery<T>)Queryable.Distinct(this);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Distinct(QueryState));
        }

        /// <summary>
        /// This query-builder method creates a new query whose results are all of
        /// the results of this query, except those that are also part of the other
        /// query specified.
        /// </summary>
        /// <param name="query"> A query representing the results to exclude. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Except(ObjectQuery<T> query)
        {
            Check.NotNull(query, "query");

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Except(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Except(QueryState, query.QueryState));
        }

        /// <summary>Groups the query results by the specified criteria.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance of type
        /// <see
        ///     cref="T:System.Data.Common.DbDataRecord" />
        /// that is equivalent to the original instance with GROUP BY applied.
        /// </returns>
        /// <param name="keys">The key columns by which to group the results.</param>
        /// <param name="projection">The list of selected properties that defines the projection. </param>
        /// <param name="parameters">Zero or more parameters that are used in this method.</param>
        /// <exception cref="T:System.ArgumentNullException">The  query  parameter is null or an empty string 
        /// or the  projection  parameter is null or an empty string.</exception>
        public ObjectQuery<DbDataRecord> GroupBy(string keys, string projection, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(keys, "keys");
            Check.NotEmpty(projection, "projection");
            Check.NotNull(parameters, "parameters");

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.GroupBy(QueryState, Name, keys, projection, parameters));
        }

        /// <summary>
        /// This query-builder method creates a new query whose results are those that
        /// are both in this query and the other query specified.
        /// </summary>
        /// <param name="query"> A query representing the results to intersect with. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Intersect(ObjectQuery<T> query)
        {
            Check.NotNull(query, "query");

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Intersect(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Intersect(QueryState, query.QueryState));
        }

        /// <summary>Limits the query to only results of a specific type.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with OFTYPE applied.
        /// </returns>
        /// <typeparam name="TResultType">
        /// The type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> returned when the query is executed with the applied filter.
        /// </typeparam>
        /// <exception cref="T:System.Data.Entity.Core.EntitySqlException">The type specified is not valid.</exception>
        public ObjectQuery<TResultType> OfType<TResultType>()
        {
            if (IsLinqQuery(this))
            {
                return (ObjectQuery<TResultType>)Queryable.OfType<TResultType>(this);
            }

            // SQLPUDT 484477: Make sure TResultType is loaded.
            QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), Assembly.GetCallingAssembly());

            // Retrieve the O-Space type metadata for the result type specified. If no
            // metadata can be found for the specified type, fail. Otherwise, if the
            // type metadata found for TResultType is not either an EntityType or a
            // ComplexType, fail - OfType() is not a valid operation on scalars,
            // enumerations, collections, etc.
            var clrOfType = typeof(TResultType);
            EdmType ofType;
            if (!QueryState.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace).TryGetType(
                clrOfType.Name, clrOfType.NestingNamespace() ?? string.Empty, out ofType)
                || !(Helper.IsEntityType(ofType) || Helper.IsComplexType(ofType)))
            {
                var message = Strings.ObjectQuery_QueryBuilder_InvalidResultType(typeof(TResultType).FullName);
                throw new EntitySqlException(message);
            }

            return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.OfType(QueryState, ofType, clrOfType));
        }

        /// <summary>Orders the query results by the specified criteria.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with ORDER BY applied.
        /// </returns>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="parameters">Zero or more parameters that are used in this method.</param>
        /// <exception cref="T:System.ArgumentNullException">The  keys  or  parameters  parameter is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  key  is an empty string.</exception>
        public ObjectQuery<T> OrderBy(string keys, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(keys, "keys");
            Check.NotNull(parameters, "parameters");

            return new ObjectQuery<T>(EntitySqlQueryBuilder.OrderBy(QueryState, Name, keys, parameters));
        }

        /// <summary>Limits the query results to only the properties that are defined in the specified projection.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance of type
        /// <see
        ///     cref="T:System.Data.Common.DbDataRecord" />
        /// that is equivalent to the original instance with SELECT applied.
        /// </returns>
        /// <param name="projection">The list of selected properties that defines the projection.</param>
        /// <param name="parameters">Zero or more parameters that are used in this method.</param>
        /// <exception cref="T:System.ArgumentNullException"> projection  is null or parameters is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  projection  is an empty string.</exception>
        public ObjectQuery<DbDataRecord> Select(string projection, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(projection, "projection");
            Check.NotNull(parameters, "parameters");

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.Select(QueryState, Name, projection, parameters));
        }

        /// <summary>Limits the query results to only the property specified in the projection.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance of a type compatible with the specific projection. The returned
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" />
        /// is equivalent to the original instance with SELECT VALUE applied.
        /// </returns>
        /// <param name="projection">The projection list.</param>
        /// <param name="parameters">An optional set of query parameters that should be in scope when parsing.</param>
        /// <typeparam name="TResultType">
        /// The type of the <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> returned by the
        /// <see
        ///     cref="M:System.Data.Entity.Core.Objects.ObjectQuery`1.SelectValue``1(System.String,System.Data.Entity.Core.Objects.ObjectParameter[])" />
        /// method.
        /// </typeparam>
        /// <exception cref="T:System.ArgumentNullException"> projection  is null or parameters  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  projection  is an empty string.</exception>
        public ObjectQuery<TResultType> SelectValue<TResultType>(string projection, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(projection, "projection");
            Check.NotNull(parameters, "parameters");

            // SQLPUDT 484974: Make sure TResultType is loaded.
            QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), Assembly.GetCallingAssembly());

            return
                new ObjectQuery<TResultType>(
                    EntitySqlQueryBuilder.SelectValue(QueryState, Name, projection, parameters, typeof(TResultType)));
        }

        /// <summary>Orders the query results by the specified criteria and skips a specified number of results.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with both ORDER BY and SKIP applied.
        /// </returns>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="count">The number of results to skip. This must be either a constant or a parameter reference.</param>
        /// <param name="parameters">An optional set of query parameters that should be in scope when parsing.</param>
        /// <exception cref="T:System.ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="T:System.ArgumentException"> keys  is an empty string or count  is an empty string.</exception>
        public ObjectQuery<T> Skip(string keys, string count, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(keys, "keys");
            Check.NotEmpty(count, "count");
            Check.NotNull(parameters, "parameters");

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Skip(QueryState, Name, keys, count, parameters));
        }

        /// <summary>Limits the query results to a specified number of items.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with TOP applied.
        /// </returns>
        /// <param name="count">The number of items in the results as a string. </param>
        /// <param name="parameters">An optional set of query parameters that should be in scope when parsing.</param>
        /// <exception cref="T:System.ArgumentNullException"> count  is null.</exception>
        /// <exception cref="T:System.ArgumentException"> count  is an empty string.</exception>
        public ObjectQuery<T> Top(string count, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(count, "count");

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Top(QueryState, Name, count, parameters));
        }

        /// <summary>
        /// This query-builder method creates a new query whose results are all of
        /// the results of this query, plus all of the results of the other query,
        /// without duplicates (i.e., results are unique).
        /// </summary>
        /// <param name="query"> A query representing the results to add. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Union(ObjectQuery<T> query)
        {
            Check.NotNull(query, "query");

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Union(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Union(QueryState, query.QueryState));
        }

        /// <summary>
        /// This query-builder method creates a new query whose results are all of
        /// the results of this query, plus all of the results of the other query,
        /// including any duplicates (i.e., results are not necessarily unique).
        /// </summary>
        /// <param name="query"> A query representing the results to add. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> UnionAll(ObjectQuery<T> query)
        {
            Check.NotNull(query, "query");

            return new ObjectQuery<T>(EntitySqlQueryBuilder.UnionAll(QueryState, query.QueryState));
        }

        /// <summary>Limits the query to results that match specified filtering criteria.</summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> instance that is equivalent to the original instance with WHERE applied.
        /// </returns>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="parameters">Zero or more parameters that are used in this method.</param>
        /// <exception cref="T:System.ArgumentNullException"> predicate  is null or parameters  is null.</exception>
        /// <exception cref="T:System.ArgumentException">The  predicate  is an empty string.</exception>
        public ObjectQuery<T> Where(string predicate, params ObjectParameter[] parameters)
        {
            Check.NotEmpty(predicate, "predicate");
            Check.NotNull(parameters, "parameters");

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Where(QueryState, Name, predicate, parameters));
        }

        #endregion

        #endregion

        #region IEnumerable<T> implementation

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();

            return new LazyEnumerator<T>(
                () =>
                    {
                        var disposableEnumerable = GetResults(null);
                        try
                        {
                            var result = disposableEnumerable.GetEnumerator();
                            return result;
                        }
                        catch
                        {
                            // if there is a problem creating the enumerator, we should dispose
                            // the enumerable (if there is no problem, the enumerator will take 
                            // care of the dispose)
                            disposableEnumerable.Dispose();
                            throw;
                        }
                    });
        }

        #endregion

        #region IDbAsyncEnumerable<T> implementation

#if !NET40

        /// <summary>
        /// Returns an <see cref="IDbAsyncEnumerator{T}" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();

            return new LazyAsyncEnumerator<T>(
                async cancellationToken =>
                    {
                        var disposableEnumerable =
                            await GetResultsAsync(null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                        try
                        {
                            return ((IDbAsyncEnumerable<T>)disposableEnumerable).GetAsyncEnumerator();
                        }
                        catch
                        {
                            // if there is a problem creating the enumerator, we should dispose
                            // the enumerable (if there is no problem, the enumerator will take 
                            // care of the dispose)
                            disposableEnumerable.Dispose();
                            throw;
                        }
                    });
        }

#endif

        #endregion

        #region ObjectQuery Overrides

        /// <inheritdoc />
        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

#if !NET40

        /// <inheritdoc />
        internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
        {
            return ((IDbAsyncEnumerable<T>)this).GetAsyncEnumerator();
        }

#endif

        /// <inheritdoc />
        internal override IList GetIListSourceListInternal()
        {
            return ((IListSource)GetResults(null)).GetList();
        }

        /// <inheritdoc />
        internal override ObjectResult ExecuteInternal(MergeOption mergeOption)
        {
            return GetResults(mergeOption);
        }

#if !NET40

        /// <inheritdoc />
        internal override async Task<ObjectResult> ExecuteInternalAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            return await GetResultsAsync(mergeOption, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        /// <summary>
        /// Retrieves the LINQ expression that backs this ObjectQuery for external consumption.
        /// It is important that the work to wrap the expression in an appropriate MergeAs call
        /// takes place in this method and NOT in ObjectQueryState.TryGetExpression which allows
        /// the unmodified expression (that does not include the MergeOption-preserving MergeAs call)
        /// to be retrieved and processed by the ELinq ExpressionConverter.
        /// </summary>
        /// <returns> The LINQ expression for this ObjectQuery, wrapped in a MergeOption-preserving call to the MergeAs method if the ObjectQuery.MergeOption property has been set. </returns>
        internal override Expression GetExpression()
        {
            // If this ObjectQuery is not backed by a LINQ Expression (it is an ESQL query),
            // then create a ConstantExpression that uses this ObjectQuery as its value.
            Expression retExpr;
            if (!QueryState.TryGetExpression(out retExpr))
            {
                retExpr = Expression.Constant(this);
            }

            var objectQueryType = typeof(ObjectQuery<T>);
            if (QueryState.UserSpecifiedMergeOption.HasValue)
            {
                var mergeAsMethod = objectQueryType.GetMethod("MergeAs", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(mergeAsMethod != null, "Could not retrieve ObjectQuery<T>.MergeAs method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, mergeAsMethod, Expression.Constant(QueryState.UserSpecifiedMergeOption.Value));
            }

            if (null != QueryState.Span)
            {
                var includeSpanMethod = objectQueryType.GetMethod("IncludeSpan", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(includeSpanMethod != null, "Could not retrieve ObjectQuery<T>.IncludeSpan method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, includeSpanMethod, Expression.Constant(QueryState.Span));
            }

            return retExpr;
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mergeOption")]
        internal ObjectQuery<T> MergeAs(MergeOption mergeOption)
        {
            throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        internal ObjectQuery<T> IncludeSpan(Span span)
        {
            throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
        }

        #endregion

        #region Private Methods

        private ObjectResult<T> GetResults(MergeOption? forMergeOption)
        {
            QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();
            var executionStrategy = DbProviderServices.GetExecutionStrategy(
                QueryState.ObjectContext.Connection, QueryState.ObjectContext.MetadataWorkspace);

            if (executionStrategy.RetriesOnFailure
                && QueryState.EffectiveStreamingBehaviour)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            return executionStrategy.Execute(
                () => QueryState.ObjectContext.ExecuteInTransaction(
                    () => QueryState.GetExecutionPlan(forMergeOption)
                                    .Execute<T>(QueryState.ObjectContext, QueryState.Parameters),
                    executionStrategy, startLocalTransaction: false,
                    releaseConnectionOnSuccess: !QueryState.EffectiveStreamingBehaviour));
        }

#if !NET40

        private Task<ObjectResult<T>> GetResultsAsync(MergeOption? forMergeOption, CancellationToken cancellationToken)
        {
            QueryState.ObjectContext.AsyncMonitor.EnsureNotEntered();

            var executionStrategy = DbProviderServices.GetExecutionStrategy(
                QueryState.ObjectContext.Connection, QueryState.ObjectContext.MetadataWorkspace);
            if (executionStrategy.RetriesOnFailure
                && QueryState.EffectiveStreamingBehaviour)
            {
                throw new InvalidOperationException(Strings.ExecutionStrategy_StreamingNotSupported(executionStrategy.GetType().Name));
            }

            return GetResultsAsync(forMergeOption, executionStrategy, cancellationToken);
        }

        private async Task<ObjectResult<T>> GetResultsAsync(
            MergeOption? forMergeOption, IDbExecutionStrategy executionStrategy, CancellationToken cancellationToken)
        {
            var mergeOption = forMergeOption.HasValue
                                  ? forMergeOption.Value
                                  : QueryState.EffectiveMergeOption;
            if (mergeOption != MergeOption.NoTracking)
            {
                QueryState.ObjectContext.AsyncMonitor.Enter();
            }

            try
            {
                return await executionStrategy.ExecuteAsync(
                    () => QueryState.ObjectContext.ExecuteInTransactionAsync(
                        () => QueryState.GetExecutionPlan(forMergeOption)
                                        .ExecuteAsync<T>(QueryState.ObjectContext, QueryState.Parameters, cancellationToken),
                              executionStrategy,
                              /*startLocalTransaction:*/ false, /*releaseConnectionOnSuccess:*/ !QueryState.EffectiveStreamingBehaviour,
                        cancellationToken),
                    cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            finally
            {
                if (mergeOption != MergeOption.NoTracking)
                {
                    QueryState.ObjectContext.AsyncMonitor.Exit();
                }
            }
        }

#endif

        #endregion
    }
}
