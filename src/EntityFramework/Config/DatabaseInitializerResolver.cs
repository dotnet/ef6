// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;

    internal class DatabaseInitializerResolver : IDbDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, object> _initializers =
            new ConcurrentDictionary<Type, object>();

        public virtual object GetService(Type type, string name)
        {
            var contextType = type.TryGetElementType(typeof(IDatabaseInitializer<>));
            if (contextType != null)
            {
                object initializer;
                if (_initializers.TryGetValue(contextType, out initializer))
                {
                    return initializer;
                }
            }

            return null;
        }

        public virtual void Release(object service)
        {
        }

        public virtual void SetInitializer(Type contextType, object initializer)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(initializer != null);

            _initializers.AddOrUpdate(contextType, initializer, (c, i) => initializer);
        }
    }
}
