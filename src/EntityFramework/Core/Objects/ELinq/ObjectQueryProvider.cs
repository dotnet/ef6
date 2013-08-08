// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// LINQ query provider implementation.
    /// </summary>
    internal class ObjectQueryProvider : IQueryProvider
#if !NET40
, IDbAsyncQueryProvider
#endif
    {
        // Although ObjectQuery contains a reference to ObjectContext, it is possible
        // that IQueryProvider methods be directly invoked from the ObjectContext.
        // This requires having a separate field to store ObjectContext reference.
        private readonly ObjectContext _context;
        private readonly ObjectQuery _query;

        /// <summary>
        /// Constructs a new provider with the given context. This constructor can be
        /// called directly when initializing ObjectContext or indirectly when initializing
        /// ObjectQuery.
        /// </summary>
        /// <param name="context"> The ObjectContext of the provider. </param>
        internal ObjectQueryProvider(ObjectContext context)
        {
            DebugCheck.NotNull(context);
            _context = context;
        }

        /// <summary>
        /// Constructs a new provider with the given ObjectQuery. This ObjectQuery instance
        /// is used to transfer state information to the new ObjectQuery instance created using
        /// the private CreateQuery method overloads.
        /// </summary>
        internal ObjectQueryProvider(ObjectQuery query)
            : this(query.Context)
        {
            DebugCheck.NotNull(query);
            _query = query;
        }

        /// <summary>
        /// Creates a new query from an expression.
        /// </summary>
        /// <typeparam name="TElement"> The element type of the query. </typeparam>
        /// <param name="expression"> Expression forming the query. </param>
        /// <returns>
        /// A new <see cref="ObjectQuery{S}" /> instance.
        /// </returns>
        internal virtual ObjectQuery<TElement> CreateQuery<TElement>(Expression expression)
        {
            return GetObjectQueryState(_query, expression, typeof(TElement)).CreateObjectQuery<TElement>();
        }

        /// <summary>
        /// Provides an untyped method capable of creating a strong-typed ObjectQuery
        /// (based on the <paramref name="ofType" /> argument) and returning it as an
        /// instance of the untyped (in a generic sense) ObjectQuery base class.
        /// </summary>
        /// <param name="expression"> The LINQ expression that defines the new query </param>
        /// <param name="ofType"> The result type of the new ObjectQuery </param>
        /// <returns>
        /// A new <see cref="ObjectQuery{ofType}" /> , as an instance of ObjectQuery
        /// </returns>
        internal virtual ObjectQuery CreateQuery(Expression expression, Type ofType)
        {
            return GetObjectQueryState(_query, expression, ofType).CreateQuery();
        }

        private ObjectQueryState GetObjectQueryState(ObjectQuery query, Expression expression, Type ofType)
        {
            return query == null
                       ? new ELinqQueryState(ofType, _context, expression)
                       : new ELinqQueryState(ofType, _query, expression);
        }

        #region IQueryProvider

        /// <summary>
        /// Creates a new query instance using the given LINQ expresion.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <typeparam name="TElement"> Element type for query result. </typeparam>
        /// <param name="expression"> LINQ expression forming the query. </param>
        /// <returns> ObjectQuery implementing the expression logic. </returns>
        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            Check.NotNull(expression, "expression");

            if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException(Strings.ELinq_ExpressionMustBeIQueryable, "expression");
            }

            return CreateQuery<TElement>(expression);
        }

        /// <summary>
        /// Executes the given LINQ expression returning a single value, or null if the query yields
        /// no results. If the return type is unexpected, raises a cast exception.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <typeparam name="TResult"> Type of returned value. </typeparam>
        /// <param name="expression"> Expression to evaluate. </param>
        /// <returns> Single result from execution. </returns>
        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            Check.NotNull(expression, "expression");

            var query = CreateQuery<TResult>(expression);

            return ExecuteSingle(query, expression);
        }

        /// <summary>
        /// Creates a new query instance using the given LINQ expresion.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <param name="expression"> Expression forming the query. </param>
        /// <returns> ObjectQuery instance implementing the given expression. </returns>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Check.NotNull(expression, "expression");

            if (!typeof(IQueryable).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException(Strings.ELinq_ExpressionMustBeIQueryable, "expression");
            }

            // Determine the type of the query instance by binding generic parameter in Query<>.Queryable
            // (based on element type of expression)
            var elementType = TypeSystem.GetElementType(expression.Type);

            return CreateQuery(expression, elementType);
        }

        /// <summary>
        /// Executes the given LINQ expression returning a single value, or null if the query yields
        /// no results.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <param name="expression"> Expression to evaluate. </param>
        /// <returns> Single result from execution. </returns>
        object IQueryProvider.Execute(Expression expression)
        {
            Check.NotNull(expression, "expression");

            var query = CreateQuery(expression, expression.Type);
            var objQuery = ((IEnumerable)query).Cast<object>();
            return ExecuteSingle(objQuery, expression);
        }

        #endregion

#if !NET40

        #region IDbAsyncQueryProvider

        Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            var query = CreateQuery<TResult>(expression);

            return ExecuteSingleAsync(query, expression, cancellationToken);
        }

        Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            var query = CreateQuery(expression, expression.Type);
            var objQuery = ((IDbAsyncEnumerable)query).Cast<object>();
            return ExecuteSingleAsync(objQuery, expression, cancellationToken);
        }

        #endregion

#endif

        #region Internal Utility API

        /// <summary>
        /// Uses an expression-specific 'materialization' function to produce
        /// a singleton result from an IEnumerable query result. The function
        /// used depends on the semantics required by the expression that is
        /// the root of the query. First, FirstOrDefault and SingleOrDefault are
        /// currently handled as special cases, and the default behavior is to
        /// use the Enumerable.Single materialization pattern.
        /// </summary>
        /// <typeparam name="TResult"> The expected result type and the required element type of the IEnumerable collection </typeparam>
        /// <param name="query"> The query result set </param>
        /// <param name="queryRoot"> The expression that is the root of the LINQ query expression tree </param>
        /// <returns> An instance of TResult if evaluation of the expression-specific singleton-producing function is successful </returns>
        internal static TResult ExecuteSingle<TResult>(IEnumerable<TResult> query, Expression queryRoot)
        {
            return GetElementFunction<TResult>(queryRoot)(query);
        }

        private static Func<IEnumerable<TResult>, TResult> GetElementFunction<TResult>(Expression queryRoot)
        {
            SequenceMethod seqMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(queryRoot, true /*unwrapLambdas*/, out seqMethod))
            {
                switch (seqMethod)
                {
                    case SequenceMethod.First:
                    case SequenceMethod.FirstPredicate:
                        return (sequence) => { return sequence.First(); };

                    case SequenceMethod.FirstOrDefault:
                    case SequenceMethod.FirstOrDefaultPredicate:
                        return (sequence) => { return sequence.FirstOrDefault(); };

                    case SequenceMethod.SingleOrDefault:
                    case SequenceMethod.SingleOrDefaultPredicate:
                        return (sequence) => { return sequence.SingleOrDefault(); };
                }
            }

            return (sequence) => { return sequence.Single(); };
        }

#if !NET40

        internal static Task<TResult> ExecuteSingleAsync<TResult>(
            IDbAsyncEnumerable<TResult> query, Expression queryRoot, CancellationToken cancellationToken)
        {
            return GetAsyncElementFunction<TResult>(queryRoot)(query, cancellationToken);
        }

        private static Func<IDbAsyncEnumerable<TResult>, CancellationToken, Task<TResult>> GetAsyncElementFunction<TResult>(
            Expression queryRoot)
        {
            SequenceMethod seqMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(queryRoot, true /*unwrapLambdas*/, out seqMethod))
            {
                switch (seqMethod)
                {
                    case SequenceMethod.First:
                    case SequenceMethod.FirstPredicate:
                        return (sequence, cancellationToken) => { return sequence.FirstAsync(cancellationToken); };

                    case SequenceMethod.FirstOrDefault:
                    case SequenceMethod.FirstOrDefaultPredicate:
                        return (sequence, cancellationToken) => { return sequence.FirstOrDefaultAsync(cancellationToken); };

                    case SequenceMethod.SingleOrDefault:
                    case SequenceMethod.SingleOrDefaultPredicate:
                        return (sequence, cancellationToken) => { return sequence.SingleOrDefaultAsync(cancellationToken); };
                }
            }

            return (sequence, cancellationToken) => { return sequence.SingleAsync(cancellationToken); };
        }

#endif

        #endregion
    }
}
