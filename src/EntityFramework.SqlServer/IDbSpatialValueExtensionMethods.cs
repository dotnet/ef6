// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Diagnostics.Contracts;

    internal static class IDbSpatialValueExtensionMethods
    {
        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeography"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geographyValue">The geography instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geography value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeography geographyValue)
        {
            Contract.Requires(geographyValue != null);

            return new DbGeographyAdapter(geographyValue);
        }

        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeometry"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geometryValue">The geometry instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geometry value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeometry geometryValue)
        {
            Contract.Requires(geometryValue != null);

            return new DbGeometryAdapter(geometryValue);
        }
    }
}
