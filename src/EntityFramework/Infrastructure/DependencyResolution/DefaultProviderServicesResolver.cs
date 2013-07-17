// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Linq;

    internal class DefaultProviderServicesResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, object key)
        {
            if (type == typeof(DbProviderServices))
            {
                throw new InvalidOperationException(Strings.EF6Providers_NoProviderFound(CheckKey(key)));
            }

            return null;
        }

        private static string CheckKey(object key)
        {
            var name = key as string;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name));
            }
            return name;
        }

        public virtual IEnumerable<object> GetServices(Type type, object key)
        {
            if (type == typeof(DbProviderServices))
            {
                CheckKey(key);
            }

            return Enumerable.Empty<object>();
        }
    }
}
