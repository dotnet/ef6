// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;

    internal class DefaultProviderServicesResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, object key)
        {
            if (type == typeof(DbProviderServices))
            {
                var name = key as string;

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Strings.DbDependencyResolver_NoProviderInvariantName(typeof(DbProviderServices).Name));
                }

                throw new InvalidOperationException(Strings.EF6Providers_NoProviderFound(name));
            }

            return null;
        }
    }
}
