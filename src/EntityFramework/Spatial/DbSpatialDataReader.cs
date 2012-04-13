namespace System.Data.Entity.Spatial
{
    /// <summary>
    /// A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    public abstract class DbSpatialDataReader
    {
        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeography"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the column that contains the geography value</param>
        /// <returns>The instance of DbGeography at the specified column value</returns>
        public abstract DbGeography GetGeography(int ordinal);

        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeometry"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the data record column that contains the provider-specific geometry data</param>
        /// <returns>The instance of DbGeometry at the specified column value</returns>
        public abstract DbGeometry GetGeometry(int ordinal);
    }
}
