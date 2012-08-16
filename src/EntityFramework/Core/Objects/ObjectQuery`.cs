// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     ObjectQuery implements strongly-typed queries at the object-layer.
    ///     Queries are specified using Entity-SQL strings and may be created by calling
    ///     the Entity-SQL-based query builder methods declared by ObjectQuery.
    /// </summary>
    /// <typeparam name="T"> The result type of this ObjectQuery </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectQuery<T> : ObjectQuery, IOrderedQueryable<T>, IEnumerable<T>, IDbAsyncEnumerable<T>
    {
        #region Private Static Members

        /// <summary>
        ///     The default query name, which is used in query-building to refer to an
        ///     element of the ObjectQuery; e.g., in a call to ObjectQuery.Where(), a predicate of
        ///     the form "it.Name = 'Foo'" can be specified, where "it" refers to a T.
        ///     Note that the query name may eventually become a parameter in the command
        ///     tree, so it must conform to the parameter name restrictions enforced by
        ///     ObjectParameter.ValidateParameterName(string).
        /// </summary>
        private const string DefaultName = "it";

        private static bool IsLinqQuery(ObjectQuery query)
        {
            return query.QueryState is ELinqQueryState;
        }

        #endregion

        #region Private Instance Fields

        /// <summary>
        ///     The name of the current sequence, which defaults to "it". Used in query-
        ///     builder methods that process an Entity-SQL command text fragment to refer to an
        ///     instance of the return type of this query.
        /// </summary>
        private string _name = DefaultName;

        #endregion

        #region Constructors

        /// <summary>
        ///     This constructor creates a new ObjectQuery instance using the specified Entity-SQL
        ///     command as the initial query. The context specifies the connection on
        ///     which to execute the query as well as the metadata and result cache.
        /// </summary>
        /// <param name="commandText"> The Entity-SQL query string that initially defines the query. </param>
        /// <param name="context"> The ObjectContext containing the metadata workspace the query will be built against, the connection on which to execute the query, and the cache to store the results in. </param>
        /// <returns> A new ObjectQuery instance. </returns>
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
        ///     This constructor creates a new ObjectQuery instance using the specified Entity-SQL
        ///     command as the initial query. The context specifies the connection on
        ///     which to execute the query as well as the metadata and result cache.
        ///     The merge option specifies how the cache should be populated/updated.
        /// </summary>
        /// <param name="commandText"> The Entity-SQL query string that initially defines the query. </param>
        /// <param name="context"> The ObjectContext containing the metadata workspace the query will be built against, the connection on which to execute the query, and the cache to store the results in. </param>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <returns> A new ObjectQuery instance. </returns>
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
        ///     This method creates a new ObjectQuery instance that represents a scan over
        ///     the specified <paramref name="entitySet" />. This ObjectQuery carries the scan as <see cref="DbExpression" /> 
        ///     and as Entity SQL. This is needed to allow case-sensitive metadata access (provided by the <see cref="DbExpression" /> by default).
        ///     The context specifies the connection on which to execute the query as well as the metadata and result cache.
        ///     The merge option specifies how the cache should be populated/updated.
        /// </summary>
        /// <param name="entitySet"> The entity set this query scans. </param>
        /// <param name="context"> The ObjectContext containing the metadata workspace the query will be built against, the connection on which to execute the query, and the cache to store the results in. </param>
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
            Contract.Requires(entitySet != null);
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

        #endregion

        #region Public Properties

        /// <summary>
        ///     The name of the query, which can be used to identify the current sequence
        ///     by name in query-builder methods. By default, the value is "it".
        /// </summary>
        /// <exception cref="ArgumentException">If the value specified on set is invalid.</exception>
        public string Name
        {
            get { return _name; }
            set
            {
                Contract.Requires(value != null);

                if (!ObjectParameter.ValidateParameterName(value))
                {
                    throw new ArgumentException(Strings.ObjectQuery_InvalidQueryName(value), "value");
                }

                _name = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     This method allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <returns> An enumerable for the ObjectQuery results. </returns>
        public new ObjectResult<T> Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return GetResults(mergeOption);
        }

        /// <summary>
        ///     An asynchronous version of Execute, which
        ///     allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <returns> A Task containing an enumerable for the ObjectQuery results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption)
        {
            return ExecuteAsync(mergeOption, CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of Execute, which
        ///     allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing an enumerable for the ObjectQuery results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public new Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);

            return GetResultsAsync(mergeOption, cancellationToken);
        }

        /// <summary>
        ///     Adds a path to the set of navigation property span paths included in the results of this query
        /// </summary>
        /// <param name="path"> The new span path </param>
        /// <returns> A new ObjectQuery that includes the specified span path </returns>
        public ObjectQuery<T> Include(string path)
        {
            EntityUtil.CheckStringArgument(path, "path");
            return new ObjectQuery<T>(QueryState.Include(this, path));
        }

        #region Query-builder Methods

        // ---------------------
        // Query-builder Methods
        // ---------------------

        /// <summary>
        ///     This query-builder method creates a new query whose results are the
        ///     unique results of this query.
        /// </summary>
        /// <returns> a new ObjectQuery instance. </returns>
        public ObjectQuery<T> Distinct()
        {
            if (IsLinqQuery(this))
            {
                return (ObjectQuery<T>)Queryable.Distinct(this);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Distinct(QueryState));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are all of
        ///     the results of this query, except those that are also part of the other
        ///     query specified.
        /// </summary>
        /// <param name="query"> A query representing the results to exclude. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Except(ObjectQuery<T> query)
        {
            Contract.Requires(query != null);

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Except(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Except(QueryState, query.QueryState));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are the results
        ///     of this query, grouped by some criteria.
        /// </summary>
        /// <param name="keys"> The group keys. </param>
        /// <param name="projection"> The projection list. To project the group, use the keyword "group". </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        public ObjectQuery<DbDataRecord> GroupBy(string keys, string projection, params ObjectParameter[] parameters)
        {
            Contract.Requires(keys != null);
            Contract.Requires(projection != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidGroupKeyList, "keys");
            }

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.GroupBy(QueryState, Name, keys, projection, parameters));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are those that
        ///     are both in this query and the other query specified.
        /// </summary>
        /// <param name="query"> A query representing the results to intersect with. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Intersect(ObjectQuery<T> query)
        {
            Contract.Requires(query != null);

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Intersect(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Intersect(QueryState, query.QueryState));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are filtered
        ///     to include only those of the specified type.
        /// </summary>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="EntitySqlException">If the type specified is invalid.</exception>
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
            EdmType ofType = null;
            if (
                !QueryState.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace).TryGetType(
                    clrOfType.Name, clrOfType.Namespace ?? string.Empty, out ofType)
                || !(Helper.IsEntityType(ofType) || Helper.IsComplexType(ofType)))
            {
                var message = Strings.ObjectQuery_QueryBuilder_InvalidResultType(typeof(TResultType).FullName);
                throw new EntitySqlException(message);
            }

            return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.OfType(QueryState, ofType, clrOfType));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are the
        ///     results of this query, ordered by some criteria. Note that any relational
        ///     operations performed after an OrderBy have the potential to "undo" the
        ///     ordering, so OrderBy should be considered a terminal query-building
        ///     operation.
        /// </summary>
        /// <param name="keys"> The sort keys. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If either argument is null.</exception>
        /// <exception cref="ArgumentException">If the sort key command text is empty.</exception>
        public ObjectQuery<T> OrderBy(string keys, params ObjectParameter[] parameters)
        {
            Contract.Requires(keys != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidSortKeyList, "keys");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.OrderBy(QueryState, Name, keys, parameters));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are data
        ///     records containing selected fields of the results of this query.
        /// </summary>
        /// <param name="projection"> The projection list. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If either argument is null.</exception>
        /// <exception cref="ArgumentException">If the projection list command text is empty.</exception>
        public ObjectQuery<DbDataRecord> Select(string projection, params ObjectParameter[] parameters)
        {
            Contract.Requires(projection != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.Select(QueryState, Name, projection, parameters));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are a sequence
        ///     of values projected from the results of this query.
        /// </summary>
        /// <param name="projection"> The projection list. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If either argument is null.</exception>
        /// <exception cref="ArgumentException">If the projection list command text is empty.</exception>
        public ObjectQuery<TResultType> SelectValue<TResultType>(string projection, params ObjectParameter[] parameters)
        {
            Contract.Requires(projection != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            // SQLPUDT 484974: Make sure TResultType is loaded.
            QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), Assembly.GetCallingAssembly());

            return
                new ObjectQuery<TResultType>(
                    EntitySqlQueryBuilder.SelectValue(QueryState, Name, projection, parameters, typeof(TResultType)));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are the
        ///     results of this query, ordered by some criteria and with the specified
        ///     number of results 'skipped', or paged-over.
        /// </summary>
        /// <param name="keys"> The sort keys. </param>
        /// <param name="count"> Specifies the number of results to skip. This must be either a constant or a parameter reference. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If any argument is null.</exception>
        /// <exception cref="ArgumentException">If the sort key or skip count command text is empty.</exception>
        public ObjectQuery<T> Skip(string keys, string count, params ObjectParameter[] parameters)
        {
            Contract.Requires(keys != null);
            Contract.Requires(count != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidSortKeyList, "keys");
            }

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(count))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidSkipCount, "count");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Skip(QueryState, Name, keys, count, parameters));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are the
        ///     first 'count' results of this query.
        /// </summary>
        /// <param name="count"> Specifies the number of results to return. This must be either a constant or a parameter reference. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the top count command text is null.</exception>
        /// <exception cref="ArgumentException">If the top count command text is empty.</exception>
        public ObjectQuery<T> Top(string count, params ObjectParameter[] parameters)
        {
            Contract.Requires(count != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(count))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidTopCount, "count");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Top(QueryState, Name, count, parameters));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are all of
        ///     the results of this query, plus all of the results of the other query,
        ///     without duplicates (i.e., results are unique).
        /// </summary>
        /// <param name="query"> A query representing the results to add. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> Union(ObjectQuery<T> query)
        {
            Contract.Requires(query != null);

            if (IsLinqQuery(this)
                || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Union(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Union(QueryState, query.QueryState));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are all of
        ///     the results of this query, plus all of the results of the other query,
        ///     including any duplicates (i.e., results are not necessarily unique).
        /// </summary>
        /// <param name="query"> A query representing the results to add. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If the query parameter is null.</exception>
        public ObjectQuery<T> UnionAll(ObjectQuery<T> query)
        {
            Contract.Requires(query != null);

            return new ObjectQuery<T>(EntitySqlQueryBuilder.UnionAll(QueryState, query.QueryState));
        }

        /// <summary>
        ///     This query-builder method creates a new query whose results are the
        ///     results of this query filtered by some criteria.
        /// </summary>
        /// <param name="predicate"> The filter predicate. </param>
        /// <param name="parameters"> An optional set of query parameters that should be in scope when parsing. </param>
        /// <returns> a new ObjectQuery instance. </returns>
        /// <exception cref="ArgumentNullException">If either argument is null.</exception>
        /// <exception cref="ArgumentException">If the filter predicate command text is empty.</exception>
        public ObjectQuery<T> Where(string predicate, params ObjectParameter[] parameters)
        {
            Contract.Requires(predicate != null);
            Contract.Requires(parameters != null);

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(predicate))
            {
                throw new ArgumentException(Strings.ObjectQuery_QueryBuilder_InvalidFilterPredicate, "predicate");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Where(QueryState, Name, predicate, parameters));
        }

        #endregion

        #endregion

        #region IEnumerable<T> implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator{T}" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
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

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator{T}" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return new LazyAsyncEnumerator<T>(
                async cancellationToken =>
                {
                    var disposableEnumerable = await GetResultsAsync(null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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

        #endregion

        #region ObjectQuery Overrides

        /// <inheritdoc />
        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        /// <inheritdoc />
        internal override IDbAsyncEnumerator GetAsyncEnumeratorInternal()
        {
            return ((IDbAsyncEnumerable<T>)this).GetAsyncEnumerator();
        }

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

        /// <inheritdoc />
        internal override async Task<ObjectResult> ExecuteInternalAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            return await GetResultsAsync(mergeOption, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        ///     Retrieves the LINQ expression that backs this ObjectQuery for external consumption.
        ///     It is important that the work to wrap the expression in an appropriate MergeAs call
        ///     takes place in this method and NOT in ObjectQueryState.TryGetExpression which allows
        ///     the unmodified expression (that does not include the MergeOption-preserving MergeAs call)
        ///     to be retrieved and processed by the ELinq ExpressionConverter.
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
            QueryState.ObjectContext.EnsureConnection();

            try
            {
                var execPlan = QueryState.GetExecutionPlan(forMergeOption);
                return execPlan.Execute<T>(QueryState.ObjectContext, QueryState.Parameters);
            }
            catch
            {
                QueryState.ObjectContext.ReleaseConnection();
                throw;
            }
        }

        private async Task<ObjectResult<T>> GetResultsAsync(MergeOption? forMergeOption, CancellationToken cancellationToken)
        {
            await QueryState.ObjectContext.EnsureConnectionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                var execPlan = QueryState.GetExecutionPlan(forMergeOption);
                return await execPlan.ExecuteAsync<T>(QueryState.ObjectContext, QueryState.Parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch
            {
                QueryState.ObjectContext.ReleaseConnection();
                throw;
            }
        }

        #endregion
    }
}
