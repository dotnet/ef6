// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
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
            DebugCheck.NotNull(spatialServices);
            DebugCheck.NotNull(spatialProviderValue);

            _spatialProvider = spatialServices;
            _providerValue = spatialProviderValue;
        }

        /// <summary> Gets the default coordinate system id (SRID) for geography values (WGS 84) </summary>
        /// <returns>The default coordinate system id (SRID) for geography values (WGS 84)</returns>
        public static int DefaultCoordinateSystemId
        {
            get { return 4326; /* WGS 84 */ }
        }

        /// <summary> Gets a representation of this DbGeography value that is specific to the underlying provider that constructed it. </summary>
        /// <returns>A representation of this DbGeography value.</returns>
        public object ProviderValue
        {
            get { return _providerValue; }
        }

        /// <summary>
        ///     Gets the spatial provider that will be used for operations on this spatial type.
        /// </summary>
        public virtual DbSpatialServices Provider
        {
            get { return _spatialProvider; }
        }

        /// <summary> Gets or sets a data contract serializable well known representation of this DbGeography value. </summary>
        /// <returns>A data contract serializable well known representation of this DbGeography value.</returns>
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
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well known binary value.
        /// </summary>
        /// <returns>
        ///     A new DbGeography value as defined by the well known binary value with the default geography coordinate system identifier (SRID)(
        ///     <see
        ///         cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        ///     ).
        /// </returns>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        public static DbGeography FromBinary(byte[] wellKnownBinary)
        {
            Check.NotNull(wellKnownBinary, "wellKnownBinary");
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography FromBinary(byte[] wellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(wellKnownBinary, "wellKnownBinary");
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> line value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <param name="lineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography LineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(lineWellKnownBinary, "lineWellKnownBinary");
            return DbSpatialServices.Default.GeographyLineFromBinary(lineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> point value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <param name="pointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography PointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(pointWellKnownBinary, "pointWellKnownBinary");
            return DbSpatialServices.Default.GeographyPointFromBinary(pointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> polygon value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography PolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownBinary, "polygonWellKnownBinary");
            return DbSpatialServices.Default.GeographyPolygonFromBinary(polygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>Returns the multiline value from a binary value.</summary>
        /// <returns>The multiline value from a binary value.</returns>
        /// <param name="multiLineWellKnownBinary">The well-known binary value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
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
            Check.NotNull(multiLineWellKnownBinary, "multiLineWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiLineFromBinary(multiLineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>Returns the multipoint value from a well-known binary value.</summary>
        /// <returns>The multipoint value from a well-known binary value.</returns>
        /// <param name="multiPointWellKnownBinary">The well-known binary value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
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
            Check.NotNull(multiPointWellKnownBinary, "multiPointWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiPointFromBinary(multiPointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>Returns the multi polygon value from a well-known binary value.</summary>
        /// <returns>The multi polygon value from a well-known binary value.</returns>
        /// <param name="multiPolygonWellKnownBinary">The multi polygon well-known binary value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownBinary, "multiPolygonWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiPolygonFromBinary(multiPolygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> collection value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <param name="geographyCollectionWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
        {
            Check.NotNull(geographyCollectionWellKnownBinary, "geographyCollectionWellKnownBinary");
            return DbSpatialServices.Default.GeographyCollectionFromBinary(geographyCollectionWellKnownBinary, coordinateSystemId);
        }

        #endregion

        #region GML Static Constructors

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <returns>
        ///     A new DbGeography value as defined by the GML value with the default geography coordinate system identifier (SRID) (
        ///     <see
        ///         cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        ///     ).
        /// </returns>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the GML value with the specified coordinate system identifier.</returns>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup, int coordinateSystemId)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup, coordinateSystemId);
        }

        #endregion

        #region Well Known Text Static Constructors

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well known text value.
        /// </summary>
        /// <returns>
        ///     A new DbGeography value as defined by the well known text value with the default geography coordinate system identifier (SRID) (
        ///     <see
        ///         cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        ///     ).
        /// </returns>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        public static DbGeography FromText(string wellKnownText)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            return DbSpatialServices.Default.GeographyFromText(wellKnownText);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography FromText(string wellKnownText, int coordinateSystemId)
        {
            Check.NotNull(wellKnownText, "wellKnownText");
            return DbSpatialServices.Default.GeographyFromText(wellKnownText, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> line value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <param name="lineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography LineFromText(string lineWellKnownText, int coordinateSystemId)
        {
            Check.NotNull(lineWellKnownText, "lineWellKnownText");
            return DbSpatialServices.Default.GeographyLineFromText(lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> point value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <param name="pointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography PointFromText(string pointWellKnownText, int coordinateSystemId)
        {
            Check.NotNull(pointWellKnownText, "pointWellKnownText");
            return DbSpatialServices.Default.GeographyPointFromText(pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> polygon value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <param name="polygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography PolygonFromText(string polygonWellKnownText, int coordinateSystemId)
        {
            Check.NotNull(polygonWellKnownText, "polygonWellKnownText");
            return DbSpatialServices.Default.GeographyPolygonFromText(polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>Returns the multiline value from a well-known text value.</summary>
        /// <returns>The multiline value from a well-known text value.</returns>
        /// <param name="multiLineWellKnownText">The well-known text.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
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
            Check.NotNull(multiLineWellKnownText, "multiLineWellKnownText");
            return DbSpatialServices.Default.GeographyMultiLineFromText(multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>Returns the multipoint value from a well-known text value.</summary>
        /// <returns>The multipoint value from a well-known text value.</returns>
        /// <param name="multiPointWellKnownText">The well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
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
            Check.NotNull(multiPointWellKnownText, "multiPointWellKnownText");
            return DbSpatialServices.Default.GeographyMultiPointFromText(multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>Returns the multi polygon value from a well-known text value.</summary>
        /// <returns>The multi polygon value from a well-known text value.</returns>
        /// <param name="multiPolygonWellKnownText">The multi polygon well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
        {
            Check.NotNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
            return DbSpatialServices.Default.GeographyMultiPolygonFromText(multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        ///     Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> collection value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <param name="geographyCollectionWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        public static DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
        {
            Check.NotNull(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
            return DbSpatialServices.Default.GeographyCollectionFromText(geographyCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Geography Instance Properties

        /// <summary>Gets the identifier associated with the coordinate system.</summary>
        /// <returns>The identifier associated with the coordinate system.</returns>
        public int CoordinateSystemId
        {
            get { return _spatialProvider.GetCoordinateSystemId(this); }
        }

        /// <summary>
        ///     Gets the dimension of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        /// <returns>
        ///     The dimension of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        public int Dimension
        {
            get { return _spatialProvider.GetDimension(this); }
        }

        /// <summary>Gets the spatial type name of the DBGeography.</summary>
        /// <returns>The spatial type name of the DBGeography.</returns>
        public string SpatialTypeName
        {
            get { return _spatialProvider.GetSpatialTypeName(this); }
        }

        /// <summary>Gets a nullable Boolean value indicating whether this DbGeography value is empty.</summary>
        /// <returns>True if this DbGeography value is empty; otherwise, false.</returns>
        public bool IsEmpty
        {
            get { return _spatialProvider.GetIsEmpty(this); }
        }

        #endregion

        #region Geography Well Known Format Conversion

        /// <summary> Generates the well known text representation of this DbGeography value.  Includes only Longitude and Latitude for points. </summary>
        /// <returns>A string containing the well known text representation of this DbGeography value.</returns>
        public virtual string AsText()
        {
            return _spatialProvider.AsText(this);
        }

        /// <summary>
        ///     Generates the well known text representation of this DbGeography value.  Includes Longitude, Latitude, Elevation (Z) and Measure (M) for points.
        /// </summary>
        /// <returns> A string containing the well known text representation of this DbGeography value. </returns>
        internal string AsTextIncludingElevationAndMeasure()
        {
            return _spatialProvider.AsTextIncludingElevationAndMeasure(this);
        }

        /// <summary> Generates the well known binary representation of this DbGeography value. </summary>
        /// <returns>The well-known binary representation of this DbGeography value.</returns>
        public byte[] AsBinary()
        {
            return _spatialProvider.AsBinary(this);
        }

        // Non-OGC
        /// <summary> Generates the Geography Markup Language (GML) representation of this DbGeography value. </summary>
        /// <returns>A string containing the GML representation of this DbGeography value.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public string AsGml()
        {
            return _spatialProvider.AsGml(this);
        }

        #endregion

        #region Geography Operations - Spatial Relation

        /// <summary> Determines whether this DbGeography is spatially equal to the specified DbGeography argument. </summary>
        /// <returns>true if other is spatially equal to this geography value; otherwise false.</returns>
        /// <param name="other">The geography value that should be compared with this geography value for equality.</param>
        public bool SpatialEquals(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.SpatialEquals(this, other);
        }

        /// <summary> Determines whether this DbGeography is spatially disjoint from the specified DbGeography argument. </summary>
        /// <returns>true if other is disjoint from this geography value; otherwise false.</returns>
        /// <param name="other">The geography value that should be compared with this geography value for disjointness.</param>
        public bool Disjoint(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Disjoint(this, other);
        }

        /// <summary> Determines whether this DbGeography value spatially intersects the specified DbGeography argument. </summary>
        /// <returns>true if other intersects this geography value; otherwise false.</returns>
        /// <param name="other">The geography value that should be compared with this geography value for intersection.</param>
        public bool Intersects(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Intersects(this, other);
        }

        #endregion

        #region Geography Operations - Spatial Analysis

        /// <summary>Returns a geography object that represents the union of all points whose distance from a geography instance is less than or equal to a specified value.</summary>
        /// <returns>A geography object that represents the union of all points</returns>
        /// <param name="distance">The distance.</param>
        public DbGeography Buffer(double? distance)
        {
            Check.NotNull(distance, "distance");

            return _spatialProvider.Buffer(this, distance.Value);
        }

        /// <summary> Computes the distance between the closest points in this DbGeography value and another DbGeography value. </summary>
        /// <returns>A double value that specifies the distance between the two closest points in this geography value and other.</returns>
        /// <param name="other">The geography value for which the distance from this value should be computed.</param>
        public double? Distance(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Distance(this, other);
        }

        /// <summary> Computes the intersection of this DbGeography value and another DbGeography value. </summary>
        /// <returns>A new DbGeography value representing the intersection between this geography value and other.</returns>
        /// <param name="other">The geography value for which the intersection with this value should be computed.</param>
        public DbGeography Intersection(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Intersection(this, other);
        }

        /// <summary> Computes the union of this DbGeography value and another DbGeography value. </summary>
        /// <returns>A new DbGeography value representing the union between this geography value and other.</returns>
        /// <param name="other">The geography value for which the union with this value should be computed.</param>
        public DbGeography Union(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Union(this, other);
        }

        /// <summary> Computes the difference of this DbGeography value and another DbGeography value. </summary>
        /// <returns>A new DbGeography value representing the difference between this geography value and other.</returns>
        /// <param name="other">The geography value for which the difference with this value should be computed.</param>
        public DbGeography Difference(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.Difference(this, other);
        }

        /// <summary> Computes the symmetric difference of this DbGeography value and another DbGeography value. </summary>
        /// <returns>A new DbGeography value representing the symmetric difference between this geography value and other.</returns>
        /// <param name="other">The geography value for which the symmetric difference with this value should be computed.</param>
        public DbGeography SymmetricDifference(DbGeography other)
        {
            Check.NotNull(other, "other");
            return _spatialProvider.SymmetricDifference(this, other);
        }

        #endregion

        #region Geography Collection

        /// <summary> Gets the number of elements in this DbGeography value, if it represents a geography collection. &lt;returns&gt;The number of elements in this geography value, if it represents a collection of other geography values; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The number of elements in this DbGeography value.</returns>
        public int? ElementCount
        {
            get { return _spatialProvider.GetElementCount(this); }
        }

        /// <summary> Returns an element of this DbGeography value from a specific position, if it represents a geography collection. &lt;param name="index"&gt;The position within this geography value from which the element should be taken.&lt;/param&gt;&lt;returns&gt;The element in this geography value at the specified position, if it represents a collection of other geography values; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>An element of this DbGeography value from a specific position</returns>
        /// <param name="index">The index.</param>
        public DbGeography ElementAt(int index)
        {
            return _spatialProvider.ElementAt(this, index);
        }

        #endregion

        #region Point

        /// <summary> Gets the Latitude coordinate of this DbGeography value, if it represents a point. &lt;returns&gt;The Latitude coordinate value of this geography value, if it represents a point; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The Latitude coordinate of this DbGeography value.</returns>
        public double? Latitude
        {
            get { return _spatialProvider.GetLatitude(this); }
        }

        /// <summary> Gets the Longitude coordinate of this DbGeography value, if it represents a point. &lt;returns&gt;The Longitude coordinate value of this geography value, if it represents a point; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The Longitude coordinate of this DbGeography value.</returns>
        public double? Longitude
        {
            get { return _spatialProvider.GetLongitude(this); }
        }

        /// <summary> Gets the elevation (Z coordinate) of this DbGeography value, if it represents a point. &lt;returns&gt;The elevation (Z coordinate) value of this geography value, if it represents a point; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The elevation (Z coordinate) of this DbGeography value.</returns>
        public double? Elevation
        {
            get { return _spatialProvider.GetElevation(this); }
        }

        /// <summary> Gets the M (Measure) coordinate of this DbGeography value, if it represents a point. &lt;returns&gt;The M (Measure) coordinate value of this geography value, if it represents a point; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The M (Measure) coordinate of this DbGeography value.</returns>
        public double? Measure
        {
            get { return _spatialProvider.GetMeasure(this); }
        }

        #endregion

        #region Curve

        /// <summary> Gets a nullable double value that indicates the length of this DbGeography value, which may be null if this value does not represent a curve. </summary>
        /// <returns>A nullable double value that indicates the length of this DbGeography value.</returns>
        public double? Length
        {
            get { return _spatialProvider.GetLength(this); }
        }

        /// <summary> Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve. </summary>
        /// <returns>A DbGeography value representing the start point of this value.</returns>
        public DbGeography StartPoint
        {
            get { return _spatialProvider.GetStartPoint(this); }
        }

        /// <summary> Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve. </summary>
        /// <returns>A DbGeography value representing the start point of this value.</returns>
        public DbGeography EndPoint
        {
            get { return _spatialProvider.GetEndPoint(this); }
        }

        /// <summary> Gets a nullable Boolean value indicating whether this DbGeography value is closed, which may be null if this value does not represent a curve. </summary>
        /// <returns>True if this DbGeography value is closed; otherwise, false.</returns>
        public bool? IsClosed
        {
            get { return _spatialProvider.GetIsClosed(this); }
        }

        #endregion

        #region LineString, Line, LinearRing

        /// <summary> Gets the number of points in this DbGeography value, if it represents a linestring or linear ring. &lt;returns&gt;The number of elements in this geography value, if it represents a linestring or linear ring; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>The number of points in this DbGeography value.</returns>
        public int? PointCount
        {
            get { return _spatialProvider.GetPointCount(this); }
        }

        /// <summary> Returns an element of this DbGeography value from a specific position, if it represents a linestring or linear ring. &lt;param name="index"&gt;The position within this geography value from which the element should be taken.&lt;/param&gt;&lt;returns&gt;The element in this geography value at the specified position, if it represents a linestring or linear ring; otherwise null.&lt;/returns&gt;</summary>
        /// <returns>An element of this DbGeography value from a specific position</returns>
        /// <param name="index">The index.</param>
        public DbGeography PointAt(int index)
        {
            return _spatialProvider.PointAt(this, index);
        }

        #endregion

        #region Surface

        /// <summary> Gets a nullable double value that indicates the area of this DbGeography value, which may be null if this value does not represent a surface. </summary>
        /// <returns>A nullable double value that indicates the area of this DbGeography value.</returns>
        public double? Area
        {
            get { return _spatialProvider.GetArea(this); }
        }

        #endregion

        #region ToString

        /// <summary> Returns a string representation of the geography value. </summary>
        /// <returns>A string representation of the geography value.</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "SRID={1};{0}", WellKnownValue.WellKnownText ?? base.ToString(), CoordinateSystemId);
        }

        #endregion
    }
}
