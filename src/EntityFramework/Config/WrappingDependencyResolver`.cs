// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

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
            return type == typeof(TService) ? (object)_serviceWrapper(_snapshot.GetService<TService>(key), key) : null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return type == typeof(TService)
                       ? (IEnumerable<object>)_snapshot.GetServices<TService>(key).Select(s => _serviceWrapper(s, key))
                       : Enumerable.Empty<object>();
        }
    }
}
