namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    public abstract class DbSpatialDataReader
    {
        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeography"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the column that contains the geography value.</param>
        /// <returns>The instance of DbGeography at the specified column value.</returns>
        public abstract DbGeography GetGeography(int ordinal);

        /// <summary>
        /// An asynchronous version of GetGeography, which
        /// reads an instance of <see cref="DbGeography"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the column that contains the geography value.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the instance of DbGeography at the specified column value.</returns>
        public Task<DbGeography> GetGeographyAsync(int ordinal)
        {
            return GetGeographyAsync(ordinal, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of GetGeography, which
        /// reads an instance of <see cref="DbGeography"/> from the column at the specified column ordinal.
        /// Providers should override with an appropriate implementation. The <paramref name="cancellationToken"/>
        /// may optionally be ignored. The default implementation invokes the synchronous
        /// GetGeography method and returns a completed task, blocking the calling thread.
        /// </summary>
        /// <param name="ordinal">The ordinal of the column that contains the geography value.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the instance of DbGeography at the specified column value.</returns>
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

        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeometry"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the data record column that contains the provider-specific geometry data.</param>
        /// <returns>The instance of DbGeometry at the specified column value.</returns>
        public abstract DbGeometry GetGeometry(int ordinal);

        /// <summary>
        /// An asynchronous version of GetGeometry, which
        /// reads an instance of <see cref="DbGeometry"/> from the column at the specified column ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal of the data record column that contains the provider-specific geometry data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the instance of DbGeometry at the specified column value.</returns>
        public Task<DbGeometry> GetGeometryAsync(int ordinal)
        {
            return GetGeometryAsync(ordinal, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of GetGeometry, which
        /// reads an instance of <see cref="DbGeometry"/> from the column at the specified column ordinal. 
        /// Providers should override with an appropriate implementation. The <paramref name="cancellationToken"/>
        /// may optionally be ignored. The default implementation invokes the synchronous
        /// GetGeometry method and returns a completed task, blocking the calling thread.
        /// </summary>
        /// <param name="ordinal">The ordinal of the data record column that contains the provider-specific geometry data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the instance of DbGeometry at the specified column value.</returns>
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
    }
}
