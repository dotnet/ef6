// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class InMemoryAsyncQueryProvider : IQueryProvider
#if !NET40
        , IDbAsyncQueryProvider
#endif
    {
        private static readonly MethodInfo _createQueryMethod
            = typeof(InMemoryAsyncQueryProvider).GetDeclaredMethods().Single(m => m.IsGenericMethodDefinition && m.Name == "CreateQuery");

        private static readonly MethodInfo _executeMethod
            = typeof(InMemoryAsyncQueryProvider).GetDeclaredMethods().Single(m => m.IsGenericMethodDefinition && m.Name == "Execute");

        private readonly IQueryProvider _provider;
        private readonly Action<string, IEnumerable> _include;

        public InMemoryAsyncQueryProvider(IQueryProvider provider, Action<string, IEnumerable> include = null)
        {
            _provider = provider;
            _include = include;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)_createQueryMethod
                .MakeGenericMethod(TryGetElementType(expression.Type))
                .Invoke(this, new object[] { expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new InMemoryAsyncQueryable<TElement>(_provider.CreateQuery<TElement>(expression), _include);
        }

        public object Execute(Expression expression)
        {
            return _executeMethod.MakeGenericMethod(expression.Type).Invoke(this, new object[] { expression });
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _provider.Execute<TResult>(expression);
        }

#if !NET40
        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute(expression));
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
#endif

        private static Type TryGetElementType(Type type)
        {
            if (!type.IsGenericTypeDefinition())
            {
                var interfaceImpl = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(t => t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (interfaceImpl != null)
                {
                    return interfaceImpl.GetGenericArguments().Single();
                }
            }

            return type;
        }
    }
}
