// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Legacy = System.Data.Common;

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper;

    internal class LegacyDbProviderServicesResolver : IDbDependencyResolver
    {
        public object GetService(Type type, object key)
        {
            var providerInvariantName = key as string;

            if (type == typeof(DbProviderServices)
                && providerInvariantName != null)
            {
                var factory = Legacy.DbProviderFactories.GetFactory(providerInvariantName);

                var legacyProviderServices = ((IServiceProvider)factory).GetService(typeof(Legacy.DbProviderServices));

                Debug.Assert(
                    legacyProviderServices != null,
                    "Provided DbProviderFactory does not support legacy EF provider model");

                return new LegacyDbProviderServicesWrapper((Legacy.DbProviderServices)legacyProviderServices);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key);
            return service != null ? new[] { service } : Enumerable.Empty<object>();
        }
    }
}
