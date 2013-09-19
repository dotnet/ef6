// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;

    internal class SpatialServicesLoader
    {
        private readonly IDbDependencyResolver _resolver;

        public SpatialServicesLoader(IDbDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Ask for a spatial provider. If one has been registered then we will use it, otherwise we will
        /// fall back on using the SQL provider and if this is not available then the default provider.
        /// </summary>
        public virtual DbSpatialServices LoadDefaultServices()
        {
            var spatialProvider = _resolver.GetService<DbSpatialServices>();
            if (spatialProvider != null)
            {
                return spatialProvider;
            }

            spatialProvider = _resolver.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlClient", "2012"));
            if (spatialProvider != null && spatialProvider.NativeTypesAvailable)
            {
                return spatialProvider;
            }

            return DefaultSpatialServices.Instance;
        }
    }
}
