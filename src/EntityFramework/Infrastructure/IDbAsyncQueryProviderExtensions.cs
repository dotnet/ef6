// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity.Infrastructure
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public static class IDbAsyncQueryProviderExtensions
    {
        /// <summary>
        ///     Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <returns> A Task containing the value that results from executing the specified query. </returns>
        /// <param name="expression"> An expression tree that represents a LINQ query. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        public static Task<object> ExecuteAsync(this IDbAsyncQueryProvider provider, Expression expression)
        {
            return provider.ExecuteAsync(expression, CancellationToken.None);
        }

        /// <summary>
        ///     Asynchronously executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <returns> A Task containing the value that results from executing the specified query. </returns>
        /// <param name="expression"> An expression tree that represents a LINQ query. </param>
        /// <typeparam name="TResult"> The type of the value that results from executing the query. </typeparam>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        public static Task<TResult> ExecuteAsync<TResult>(this IDbAsyncQueryProvider provider, Expression expression)
        {
            return provider.ExecuteAsync<TResult>(expression, CancellationToken.None);
        }
    }
}

#endif
