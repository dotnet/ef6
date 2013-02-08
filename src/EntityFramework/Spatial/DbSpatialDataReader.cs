// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    public abstract class DbSpatialDataReader
    {
        /// <summary>
        ///     Reads an instance of <see cref="DbGeography" /> from the column at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column that contains the geography value. </param>
        /// <returns> The instance of DbGeography at the specified column value. </returns>
        public abstract DbGeography GetGeography(int ordinal);

#if !NET40

        /// <summary>
        ///     An asynchronous version of <see cref="GetGeography"/>, which
        ///     reads an instance of <see cref="DbGeography" /> from the column at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal"> The ordinal of the column that contains the geography value. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task"/> containing the instance of <see cref="DbGeography" /> at the specified column value. </returns>
        public Task<DbGeography> GetGeographyAsync(int ordinal)
        {
            return GetGeographyAsync(ordinal, CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of <see cref="GetGeography"/>, which
        ///     reads an instance of <see cref="DbGeography" /> from the column at the specified column ordinal.
        /// </summary>
        /// <remarks>
        ///     Providers should override with an appropriate implementation.
        ///     The default implementation invokes the synchronous <see cref="GetGeography"/> method and returns
        ///     a completed task, blocking the calling thread.
        /// </remarks>
        /// <param name="ordinal"> The ordinal of the column that contains the geography value. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task"/> containing the instance of <see cref="DbGeography" /> at the specified column value. </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception provided in the returned task.")]
        public virtual Task<DbGeography> GetGeographyAsync(int ordinal, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelper.FromCancellation<DbGeography>();
            }

            try
            {
                return Task.FromResult(GetGeography(ordinal));
            }
            catch (Exception e)
            {
                return TaskHelper.FromException<DbGeography>(e);
            }
        }

#endif

        /// <summary>
        ///     Reads an instance of <see cref="DbGeometry" /> from the column at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal"> The ordinal of the data record column that contains the provider-specific geometry data. </param>
        /// <returns> The instance of DbGeometry at the specified column value. </returns>
        public abstract DbGeometry GetGeometry(int ordinal);

#if !NET40

        /// <summary>
        ///     An asynchronous version of <see cref="GetGeometry"/>, which
        ///     reads an instance of <see cref="DbGeometry" /> from the column at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal"> The ordinal of the data record column that contains the provider-specific geometry data. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task"/> containing the instance of <see cref="DbGeometry" /> at the specified column value. </returns>
        public Task<DbGeometry> GetGeometryAsync(int ordinal)
        {
            return GetGeometryAsync(ordinal, CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of <see cref="GetGeometry"/>, which
        ///     reads an instance of <see cref="DbGeometry" /> from the column at the specified column ordinal.
        /// </summary>
        /// <remarks>
        ///     Providers should override with an appropriate implementation.
        ///     The default implementation invokes the synchronous <see cref="GetGeometry"/> method and returns
        ///     a completed task, blocking the calling thread.
        /// </remarks>
        /// <param name="ordinal"> The ordinal of the data record column that contains the provider-specific geometry data. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A <see cref="Task"/> containing the instance of <see cref="DbGeometry" /> at the specified column value. </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception provided in the returned task.")]
        public virtual Task<DbGeometry> GetGeometryAsync(int ordinal, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelper.FromCancellation<DbGeometry>();
            }

            try
            {
                return Task.FromResult(GetGeometry(ordinal));
            }
            catch (Exception e)
            {
                return TaskHelper.FromException<DbGeometry>(e);
            }
        }

#endif

        /// <summary>
        ///     Returns whether the column at the specified column ordinal is of geography type
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns>
        ///     <c>true</c> if the column at the specified column ordinal is of geography type;
        ///     <c>false</c> otherwise.
        /// </returns>
        public abstract bool IsGeographyColumn(int ordinal);

        /// <summary>
        ///     Returns whether the column at the specified column ordinal is of geometry type
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns>
        ///     <c>true</c> if the column at the specified column ordinal is of geometry type;
        ///     <c>false</c> otherwise.
        /// </returns>
        public abstract bool IsGeometryColumn(int ordinal);
    }
}
