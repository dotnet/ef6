// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Utilities;

    internal class WrappingDependencyResolver<TService> : IDbDependencyResolver
    {
        private readonly IDbDependencyResolver _snapshot;
        private readonly Func<TService, object, TService> _serviceWrapper;

        public WrappingDependencyResolver(IDbDependencyResolver snapshot, Func<TService, object, TService> serviceWrapper)
        {
            DebugCheck.NotNull(snapshot);
            DebugCheck.NotNull(serviceWrapper);

            _snapshot = snapshot;
            _serviceWrapper = serviceWrapper;
        }

        public object GetService(Type type, object key)
        {
            if (type == typeof(TService))
            {
                return _serviceWrapper(_snapshot.GetService<TService>(key), key);
            }

            return null;
        }
    }
}
