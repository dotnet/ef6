// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Diagnostics;

    internal class SpatialServicesLoader
    {
        private readonly IDbDependencyResolver _resolver;

        public SpatialServicesLoader(IDbDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        ///     Ask for a spatial provider. If one has been registered then we will use it, otherwise we will
        ///     fall back on using the SQL provider and if this is not available then the default provider.
        /// </summary>
        public virtual DbSpatialServices LoadDefaultServices()
        {
            var spatialProvider = _resolver.GetService<DbSpatialServices>();
            if (spatialProvider != null)
            {
                return spatialProvider;
            }

            var efProvider = _resolver.GetService<DbProviderServices>("System.Data.SqlClient");
            Debug.Assert(efProvider != null);

            try
            {
                spatialProvider = efProvider.GetSpatialServicesInternal(new Lazy<IDbDependencyResolver>(() => _resolver), "2008");
                if (spatialProvider.NativeTypesAvailable)
                {
                    return spatialProvider;
                }
            }
            catch (ProviderIncompatibleException)
            {
                // Thrown if the provider doesn't support spatial, in which case we fall back to the default.
            }

            return DefaultSpatialServices.Instance;
        }
    }
}
