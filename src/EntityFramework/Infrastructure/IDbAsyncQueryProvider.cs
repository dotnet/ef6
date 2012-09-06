// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines methods to create and asynchronously execute queries that are described by an <see
    ///      cref="T:System.Linq.IQueryable" /> object.
    /// </summary>
    [ContractClass(typeof(IDbAsyncQueryProviderContracts))]
    public interface IDbAsyncQueryProvider : IQueryProvider
    {
        /// <summary>
        ///     Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <returns> A Task containing the value that results from executing the specified query. </returns>
        /// <param name="expression"> An expression tree that represents a LINQ query. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken);

        /// <summary>
        ///     Asynchronously executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <returns> A Task containing the value that results from executing the specified query. </returns>
        /// <param name="expression"> An expression tree that represents a LINQ query. </param>
        /// <typeparam name="TResult"> The type of the value that results from executing the query. </typeparam>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbAsyncQueryProvider))]
    internal abstract class IDbAsyncQueryProviderContracts : IDbAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Contract.Requires(expression != null);

            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Contract.Requires(expression != null);

            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}

#endif
