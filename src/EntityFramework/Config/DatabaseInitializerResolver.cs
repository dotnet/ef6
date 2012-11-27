// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Utilities;

    internal class DatabaseInitializerResolver : IDbDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, object> _initializers =
            new ConcurrentDictionary<Type, object>();

        public virtual object GetService(Type type, object key)
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

        public virtual void SetInitializer(Type contextType, object initializer)
        {
            DebugCheck.NotNull(contextType);
            DebugCheck.NotNull(initializer);

            _initializers.AddOrUpdate(contextType, initializer, (c, i) => initializer);
        }
    }
}
