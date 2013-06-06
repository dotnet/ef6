// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class SpatialHelpers
    {
        internal static object GetSpatialValue(MetadataWorkspace workspace, DbDataReader reader, TypeUsage columnType, int columnOrdinal)
        {
            Debug.Assert(Helper.IsSpatialType(columnType));
            var spatialReader = CreateSpatialDataReader(workspace, reader);
            if (Helper.IsGeographicType((PrimitiveType)columnType.EdmType))
            {
                return spatialReader.GetGeography(columnOrdinal);
            }
            else
            {
                return spatialReader.GetGeometry(columnOrdinal);
            }
        }

#if !NET40

        internal static async Task<object> GetSpatialValueAsync(
            MetadataWorkspace workspace, DbDataReader reader,
            TypeUsage columnType, int columnOrdinal, CancellationToken cancellationToken)
        {
            Debug.Assert(Helper.IsSpatialType(columnType));
            var spatialReader = CreateSpatialDataReader(workspace, reader);
            if (Helper.IsGeographicType((PrimitiveType)columnType.EdmType))
            {
                return
                    await spatialReader.GetGeographyAsync(columnOrdinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            else
            {
                return
                    await spatialReader.GetGeometryAsync(columnOrdinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

#endif

        internal static DbSpatialDataReader CreateSpatialDataReader(MetadataWorkspace workspace, DbDataReader reader)
        {
            var storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
            var providerFactory = storeItemCollection.StoreProviderFactory;
            Debug.Assert(providerFactory != null, "GetProviderSpatialServices requires provider factory to have been initialized");

            var providerServices = providerFactory.GetProviderServices();
            var result = providerServices.GetSpatialDataReader(reader, storeItemCollection.StoreProviderManifestToken);

            if (result == null)
            {
                throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
            }

            return result;
        }
    }
}
