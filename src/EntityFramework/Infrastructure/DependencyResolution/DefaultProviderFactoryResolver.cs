// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Linq;

    internal class DefaultProviderFactoryResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, object key)
        {
            return GetService(type, key, (e, n) => { throw new ArgumentException(Strings.EntityClient_InvalidStoreProvider(n), e); });
        }

        private static object GetService(Type type, object key, Func<ArgumentException, string, object> handleFailedLookup)
        {
            if (type == typeof(DbProviderFactory))
            {
                var name = key as string;

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderFactory).Name));
                }

                try
                {
                    return DbProviderFactories.GetFactory(name);
                }
                catch (ArgumentException e)
                {
                    return handleFailedLookup(e, name);
                }
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key, (e, n) => null);
            return service == null ? Enumerable.Empty<object>() : new[] { service };
        }
    }
}
