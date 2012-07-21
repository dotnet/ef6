// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class DbGeography
    {
        private DbSpatialServices _spatialProvider;
        private object _providerValue;

        internal DbGeography()
        {
        }

        internal DbGeography(DbSpatialServices spatialServices, object spatialProviderValue)
        {
            Contract.Requires(spatialServices != null);
            Contract.Requires(spatialProviderValue != null);

            _spatialProvider = spatialServices;
            _providerValue = spatialProviderValue;
        }

        /// <summary>
        /// Gets the default coordinate system id (SRID) for geography values (WGS 84)
        /// </summary>
        public static int DefaultCoordinateSystemId
        {
            get { return 4326; /* WGS 84 */ }
        }

        /// <summary>
        /// Gets a representation of this DbGeography value that is specific to the underlying provider that constructed it.
        /// </summary>
        public object ProviderValue
        {
            get { return _providerValue; }
        }

        /// <summary>
        /// Gets the spatial provider that will be used for operations on this spatial type.
        /// </summary>
        public virtual DbSpatialServices Provider
        {
            get { return _spatialProvider; }
        }

        /// <summary>
        /// Gets or sets a data contract serializable well known representation of this DbGeography value.
        /// </summary>
        [DataMember(Name = "Geography")]
        public DbGeographyWellKnownValue WellKnownValue
        {
            get { return _spatialProvider.CreateWellKnownValue(this); }
            set
            {
                if (_spatialProvider != null)
                {
                    throw new InvalidOperationException(Strings.Spatial_WellKnownValueSerializationPropertyNotDirectlySettable);
                }

                var resolvedServices = DbSpatialServices.Default;
                _providerValue = resolvedServices.CreateProviderValue(value);
                _spatialProvider = resolvedServices;
            }
        }

        #region Well Known Binary Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value. 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the default geography coordinate system identifier (SRID)(<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        public static DbGeography FromBinary(byte[] wellKnownBinary)
        {
            Contract.Requires(wellKnownBinary != null);
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography FromBinary(byte[] wellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(wellKnownBinary != null);
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography LineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(lineWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyLineFromBinary(lineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="pointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(pointWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyPointFromBinary(pointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(polygonWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyPolygonFromBinary(polygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiLine value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiLineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(multiLineWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyMultiLineFromBinary(multiLineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPoint value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(multiPointWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyMultiPointFromBinary(multiPointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPolygon value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiPolygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(multiPolygonWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyMultiPolygonFromBinary(multiPolygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geographyCollectionWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
        {
            Contract.Requires(geographyCollectionWellKnownBinary != null);
            return DbSpatialServices.Default.GeographyCollectionFromBinary(geographyCollectionWellKnownBinary, coordinateSystemId);
        }

        #endregion

        #region GML Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the default geography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup)
        {
            Contract.Requires(geographyMarkup != null);
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup, int coordinateSystemId)
        {
            Contract.Requires(geographyMarkup != null);
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup, coordinateSystemId);
        }

        #endregion

        #region Well Known Text Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value. 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the default geography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        public static DbGeography FromText(string wellKnownText)
        {
            Contract.Requires(wellKnownText != null);
            return DbSpatialServices.Default.GeographyFromText(wellKnownText);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography FromText(string wellKnownText, int coordinateSystemId)
        {
            Contract.Requires(wellKnownText != null);
            return DbSpatialServices.Default.GeographyFromText(wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography LineFromText(string lineWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(lineWellKnownText != null);
            return DbSpatialServices.Default.GeographyLineFromText(lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="pointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PointFromText(string pointWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(pointWellKnownText != null);
            return DbSpatialServices.Default.GeographyPointFromText(pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="polygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PolygonFromText(string polygonWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(polygonWellKnownText != null);
            return DbSpatialServices.Default.GeographyPolygonFromText(polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiLine value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(multiLineWellKnownText != null);
            return DbSpatialServices.Default.GeographyMultiLineFromText(multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPoint value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiPointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(multiPointWellKnownText != null);
            return DbSpatialServices.Default.GeographyMultiPointFromText(multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPolygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(multiPolygonWellKnownText != null);
            return DbSpatialServices.Default.GeographyMultiPolygonFromText(multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geographyCollectionWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
        {
            Contract.Requires(geographyCollectionWellKnownText != null);
            return DbSpatialServices.Default.GeographyCollectionFromText(geographyCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Geography Instance Properties

        /// </summary>
        /// Gets the Spatial Reference System Identifier (Coordinate System Id) of the spatial reference system used by this DbGeography value.
        /// </summary>
        public int CoordinateSystemId
        {
            get { return _spatialProvider.GetCoordinateSystemId(this); }
        }

        /// <summary>
        /// Gets the dimension of the given <see cref="DbGeography"/> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        public int Dimension
        {
            get { return _spatialProvider.GetDimension(this); }
        }

        /// </summary>
        /// Gets the spatial type name, as a string, of this DbGeography value.
        /// </summary>
        public string SpatialTypeName
        {
            get { return _spatialProvider.GetSpatialTypeName(this); }
        }

        /// </summary>
        /// Gets a Boolean value indicating whether this DbGeography value represents the empty geography.
        /// </summary>
        public bool IsEmpty
        {
            get { return _spatialProvider.GetIsEmpty(this); }
        }

        #endregion

        #region Geography Well Known Format Conversion

        /// <summary>
        /// Generates the well known text representation of this DbGeography value.  Includes only Longitude and Latitude for points.
        /// </summary>
        /// <returns>A string containing the well known text representation of this DbGeography value.</returns>
        public virtual string AsText()
        {
            return _spatialProvider.AsText(this);
        }

        /// <summary>
        /// Generates the well known text representation of this DbGeography value.  Includes Longitude, Latitude, Elevation (Z) and Measure (M) for points.
        /// </summary>
        /// <returns>A string containing the well known text representation of this DbGeography value.</returns>
        internal string AsTextIncludingElevationAndMeasure()
        {
            return _spatialProvider.AsTextIncludingElevationAndMeasure(this);
        }

        /// <summary>
        /// Generates the well known binary representation of this DbGeography value.
        /// </summary>
        /// <returns>A byte array containing the well known binary representation of this DbGeography value.</returns>
        public byte[] AsBinary()
        {
            return _spatialProvider.AsBinary(this);
        }

        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this DbGeography value.
        /// </summary>
        /// <returns>A string containing the GML representation of this DbGeography value.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public string AsGml()
        {
            return _spatialProvider.AsGml(this);
        }

        #endregion

        #region Geography Operations - Spatial Relation

        /// <summary>
        /// Determines whether this DbGeography is spatially equal to the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for equality.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is spatially equal to this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SpatialEquals(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.SpatialEquals(this, other);
        }

        /// <summary>
        /// Determines whether this DbGeography is spatially disjoint from the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for disjointness.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is disjoint from this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Disjoint(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Disjoint(this, other);
        }

        /// <summary>
        /// Determines whether this DbGeography value spatially intersects the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for intersection.</param>
        /// <returns><c>true</c> if <paramref name="other"/> intersects this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Intersects(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Intersects(this, other);
        }

        #endregion

        #region Geography Operations - Spatial Analysis

        /// <summary>
        /// Creates a geography value representing all points less than or equal to <paramref name="distance"/> from this DbGeography value.
        /// </summary>
        /// <param name="distance">A double value specifying how far from this geography value to buffer.</param>
        /// <returns>A new DbGeography value representing all points less than or equal to <paramref name="distance"/> from this geography value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="distance"/> is null.</exception>
        public DbGeography Buffer(double? distance)
        {
            if (!distance.HasValue)
            {
                throw new ArgumentNullException("distance");
            }
            return _spatialProvider.Buffer(this, distance.Value);
        }

        /// <summary>
        /// Computes the distance between the closest points in this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the distance from this value should be computed.</param>
        /// <returns>A double value that specifies the distance between the two closest points in this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public double? Distance(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Distance(this, other);
        }

        /// <summary>
        /// Computes the intersection of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the intersection with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the intersection between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Intersection(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Intersection(this, other);
        }

        /// <summary>
        /// Computes the union of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the union with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the union between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Union(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Union(this, other);
        }

        /// <summary>
        /// Computes the difference of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the difference with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the difference between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Difference(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.Difference(this, other);
        }

        /// <summary>
        /// Computes the symmetric difference of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the symmetric difference with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the symmetric difference between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography SymmetricDifference(DbGeography other)
        {
            Contract.Requires(other != null);
            return _spatialProvider.SymmetricDifference(this, other);
        }

        #endregion

        #region Geography Collection

        /// <summary>
        /// Gets the number of elements in this DbGeography value, if it represents a geography collection.
        /// <returns>The number of elements in this geography value, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        public int? ElementCount
        {
            get { return _spatialProvider.GetElementCount(this); }
        }

        /// <summary>
        /// Returns an element of this DbGeography value from a specific position, if it represents a geography collection.
        /// <param name="index">The position within this geography value from which the element should be taken.</param>
        /// <returns>The element in this geography value at the specified position, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        public DbGeography ElementAt(int index)
        {
            return _spatialProvider.ElementAt(this, index);
        }

        #endregion

        #region Point

        /// <summary>
        /// Gets the Latitude coordinate of this DbGeography value, if it represents a point.
        /// </summary>
        /// <returns>The Latitude coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        public double? Latitude
        {
            get { return _spatialProvider.GetLatitude(this); }
        }

        /// <summary>
        /// Gets the Longitude coordinate of this DbGeography value, if it represents a point.
        /// </summary>
        /// <returns>The Longitude coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        public double? Longitude
        {
            get { return _spatialProvider.GetLongitude(this); }
        }

        /// <summary>
        /// Gets the elevation (Z coordinate) of this DbGeography value, if it represents a point.
        /// </summary>
        /// <returns>The elevation (Z coordinate) value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        public double? Elevation
        {
            get { return _spatialProvider.GetElevation(this); }
        }

        /// <summary>
        /// Gets the M (Measure) coordinate of this DbGeography value, if it represents a point.
        /// </summary>
        /// <returns>The M (Measure) coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        public double? Measure
        {
            get { return _spatialProvider.GetMeasure(this); }
        }

        #endregion

        #region Curve

        /// <summary>
        /// Gets a nullable double value that indicates the length of this DbGeography value, which may be null if this value does not represent a curve.
        /// </summary>
        public double? Length
        {
            get { return _spatialProvider.GetLength(this); }
        }

        /// <summary>
        /// Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve.
        /// </summary>
        public DbGeography StartPoint
        {
            get { return _spatialProvider.GetStartPoint(this); }
        }

        /// <summary>
        /// Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve.
        /// </summary>
        public DbGeography EndPoint
        {
            get { return _spatialProvider.GetEndPoint(this); }
        }

        /// <summary>
        /// Gets a nullable Boolean value indicating whether this DbGeography value is closed, which may be null if this value does not represent a curve.
        /// </summary>
        public bool? IsClosed
        {
            get { return _spatialProvider.GetIsClosed(this); }
        }

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Gets the number of points in this DbGeography value, if it represents a linestring or linear ring.
        /// <returns>The number of elements in this geography value, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        public int? PointCount
        {
            get { return _spatialProvider.GetPointCount(this); }
        }

        /// <summary>
        /// Returns an element of this DbGeography value from a specific position, if it represents a linestring or linear ring.
        /// <param name="index">The position within this geography value from which the element should be taken.</param>
        /// <returns>The element in this geography value at the specified position, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        public DbGeography PointAt(int index)
        {
            return _spatialProvider.PointAt(this, index);
        }

        #endregion

        #region Surface

        /// <summary>
        /// Gets a nullable double value that indicates the area of this DbGeography value, which may be null if this value does not represent a surface.
        /// </summary>
        public double? Area
        {
            get { return _spatialProvider.GetArea(this); }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns a string representation of the geography value.
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "SRID={1};{0}", WellKnownValue.WellKnownText ?? base.ToString(), CoordinateSystemId);
        }

        #endregion
    }
}
