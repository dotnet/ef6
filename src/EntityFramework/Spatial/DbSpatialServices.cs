// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    [Serializable]
    public abstract class DbSpatialServices
    {
        private static readonly Lazy<DbSpatialServices> _defaultServices = new Lazy<DbSpatialServices>(
            () => new SpatialServicesLoader(DbConfiguration.DependencyResolver).LoadDefaultServices(), isThreadSafe: true);

        /// <summary>
        /// Gets the default services for the <see cref="T:System.Data.Entity.Spatial.DbSpatialServices" />.
        /// </summary>
        /// <returns>The default services.</returns>
        public static DbSpatialServices Default
        {
            get { return _defaultServices.Value; }
        }

        /// <summary>
        /// Override this property to allow the spatial provider to fail fast when native types or other
        /// resources needed for the spatial provider to function correctly are not available.
        /// The default value is <code>true</code> which means that EF will continue with the assumption
        /// that the provider has the necessary types/resources rather than failing fast.
        /// </summary>
        public virtual bool NativeTypesAvailable
        {
            get { return true; }
        }

        #region Geography API

        /// <summary>
        /// This method is intended for use by derived implementations of
        /// <see
        ///     cref="M:System.Data.Entity.Spatial.DbSpatialServices.GeographyFromProviderValue(System.Object)" />
        /// after suitable validation of the specified provider value to ensure it is suitable for use with the derived implementation.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> instance that contains the specified providerValue and uses the specified spatialServices as its spatial implementation.
        /// </returns>
        /// <param name="spatialServices">
        /// The spatial services instance that the returned <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value will depend on for its implementation of spatial functionality.
        /// </param>
        /// <param name="providerValue">The provider value.</param>
        protected static DbGeography CreateGeography(DbSpatialServices spatialServices, object providerValue)
        {
            Check.NotNull(spatialServices, "spatialServices");
            Check.NotNull(providerValue, "providerValue");
            return new DbGeography(spatialServices, providerValue);
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on a provider-specific value that is compatible with this spatial services implementation.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value backed by this spatial services implementation and the specified provider value.
        /// </returns>
        /// <param name="providerValue">A provider-specific value that this spatial services implementation is capable of interpreting as a geography value.</param>
        /// <returns> A new DbGeography value backed by this spatial services implementation and the specified provider value. </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="providerValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="providerValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography GeographyFromProviderValue(object providerValue);

        /// <summary>
        /// Creates a provider-specific value compatible with this spatial services implementation based on the specified well-known
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// representation.
        /// </summary>
        /// <returns>A provider-specific value that encodes the information contained in wellKnownValue in a fashion compatible with this spatial services implementation.</returns>
        /// <param name="wellKnownValue">
        /// An instance of <see cref="T:System.Data.Entity.Spatial.DbGeographyWellKnownValue" /> that contains the well-known representation of a geography value.
        /// </param>
        public abstract object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue);

        /// <summary>
        /// Creates an instance of <see cref="T:System.Data.Entity.Spatial.DbGeographyWellKnownValue" /> that represents the specified
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value using one or both of the standard well-known spatial formats.
        /// </summary>
        /// <returns>
        /// The well-known representation of geographyValue, as a new
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeographyWellKnownValue" />
        /// .
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue);

        #region Geography Constructors - well known binary

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well-known binary value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>        
        public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> line value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="lineWellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> point value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="pointWellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> polygon value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multiline value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// The new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multiline value.
        /// </returns>
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
        public abstract DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multipoint value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multipoint value.
        /// </returns>
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
        public abstract DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multi polygon value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multi polygon value.
        /// </returns>
        /// <param name="multiPolygonWellKnownBinary">The well-known binary value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> collection value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geographyCollectionWellKnownBinary">A byte array that contains a well-known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId);

        #endregion

        #region Geography Constructors - well known text

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well-known text value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownText">A string that contains a well-known text representation of the geography value.</param>
        public abstract DbGeography GeographyFromText(string wellKnownText);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownText">A string that contains a well-known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> line value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="lineWellKnownText">A string that contains a well-known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> point value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="pointWellKnownText">A string that contains a well-known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> polygon value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="polygonWellKnownText">A string that contains a well-known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multiline value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multiline value.
        /// </returns>
        /// <param name="multiLineWellKnownText">The well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multipoint value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multipoint value.
        /// </returns>
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
        public abstract DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multi polygon value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> multi polygon value.
        /// </returns>
        /// <param name="multiPolygonWellKnownText">The well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> collection value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geographyCollectionWellKnownText">A string that contains a well-known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        public abstract DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId);

        #endregion

        #region Geography Constructors - Geography Markup Language (GML)

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the GML value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geographyMarkup">A string that contains a Geometry Markup Language (GML) representation of the geography value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeography GeographyFromGml(string geographyMarkup);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value as defined by the GML value with the specified coordinate system identifier (SRID).
        /// </returns>
        /// <param name="geographyMarkup">A string that contains a Geometry Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value should use.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId);

        #endregion

        #region Geography Instance Property Accessors

        /// <summary>
        /// Returns the coordinate system identifier of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </summary>
        /// <returns>
        /// The coordinate system identifier of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int GetCoordinateSystemId(DbGeography geographyValue);

        /// <summary>
        /// Gets the dimension of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        /// <returns>
        /// The dimension of geographyValue, or the largest element dimension if
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// is a collection.
        /// </returns>
        /// <param name="geographyValue">The geography value for which the dimension value should be retrieved.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int GetDimension(DbGeography geographyValue);

        /// <summary>
        /// Returns a value that indicates the spatial type name of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value.
        /// </summary>
        /// <returns>
        /// The spatial type name of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract string GetSpatialTypeName(DbGeography geographyValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value is empty.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value is empty; otherwise, false.
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool GetIsEmpty(DbGeography geographyValue);

        #endregion

        #region Geography Well Known Format Conversion

        /// <summary>
        /// Gets the well-known text representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value. This value should include only the Longitude and Latitude of points.
        /// </summary>
        /// <returns>A string containing the well-known text representation of geographyValue.</returns>
        /// <param name="geographyValue">The geography value for which the well-known text should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract string AsText(DbGeography geographyValue);

        /// <summary>
        /// Returns a text representation of <see cref="T:System.Data.Entity.Spatial.DbSpatialServices" /> with elevation and measure.
        /// </summary>
        /// <returns>
        /// A text representation of <see cref="T:System.Data.Entity.Spatial.DbSpatialServices" />.
        /// </returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public virtual string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
        {
            return null;
        }

        /// <summary>
        /// Gets the well-known binary representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </summary>
        /// <returns>
        /// The well-known binary representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value for which the well-known binary should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract byte[] AsBinary(DbGeography geographyValue);

        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value.
        /// </summary>
        /// <returns>A string containing the GML representation of this DbGeography value.</returns>
        /// <param name="geographyValue">The geography value for which the GML should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract string AsGml(DbGeography geographyValue);

        #endregion

        #region Geography Instance Methods - Spatial Relation

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values are spatially equal.
        /// </summary>
        /// <returns>true if geographyValue is spatially equal to otherGeography; otherwise false.</returns>
        /// <param name="geographyValue">The first geography value to compare for equality.</param>
        /// <param name="otherGeography">The second geography value to compare for equality.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values are spatially disjoint.
        /// </summary>
        /// <returns>true if geographyValue is disjoint from otherGeography; otherwise false.</returns>
        /// <param name="geographyValue">The first geography value to compare for disjointness.</param>
        /// <param name="otherGeography">The second geography value to compare for disjointness.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Disjoint(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values spatially intersect.
        /// </summary>
        /// <returns>true if geographyValue intersects otherGeography; otherwise false.</returns>
        /// <param name="geographyValue">The first geography value to compare for intersection.</param>
        /// <param name="otherGeography">The second geography value to compare for intersection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Intersects(DbGeography geographyValue, DbGeography otherGeography);

        #endregion

        #region Geography Instance Methods - Spatial Analysis

        /// <summary>
        /// Creates a geography value representing all points less than or equal to distance from the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value.
        /// </summary>
        /// <returns>A new DbGeography value representing all points less than or equal to distance from geographyValue.</returns>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="distance">A double value specifying how far from geographyValue to buffer.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography Buffer(DbGeography geographyValue, double distance);

        /// <summary>
        /// Computes the distance between the closest points in two <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values.
        /// </summary>
        /// <returns>A double value that specifies the distance between the two closest points in geographyValue and otherGeography.</returns>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double Distance(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the intersection of two <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value representing the intersection of geographyValue and otherGeography.
        /// </returns>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the union of two <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value representing the union of geographyValue and otherGeography.
        /// </returns>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography Union(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the difference of two <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values.
        /// </summary>
        /// <returns>A new DbGeography value representing the difference of geographyValue and otherGeography.</returns>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the symmetric difference of two <see cref="T:System.Data.Entity.Spatial.DbGeography" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value representing the symmetric difference of geographyValue and otherGeography.
        /// </returns>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// or
        /// <paramref name="otherGeography" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography);

        #endregion

        #region Geography Collection

        /// <summary>
        /// Returns the number of elements in the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a geography collection.
        /// </summary>
        /// <returns>The number of elements in geographyValue, if it represents a collection of other geography values; otherwise null.</returns>
        /// <param name="geographyValue">The geography value, which need not represent a geography collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int? GetElementCount(DbGeography geographyValue);

        /// <summary>
        /// Returns an element of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a geography collection.
        /// </summary>
        /// <returns>The element in geographyValue at position index, if it represents a collection of other geography values; otherwise null.</returns>
        /// <param name="geographyValue">The geography value, which need not represent a geography collection.</param>
        /// <param name="index">The position within the geography value from which the element should be taken.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography ElementAt(DbGeography geographyValue, int index);

        #endregion

        #region Point

        /// <summary>
        /// Returns the Latitude coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The Latitude coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetLatitude(DbGeography geographyValue);

        /// <summary>
        /// Returns the Longitude coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The Longitude coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetLongitude(DbGeography geographyValue);

        /// <summary>
        /// Returns the elevation (Z coordinate) of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a point.
        /// </summary>
        /// <returns>The elevation (Z coordinate) of geographyValue, if it represents a point; otherwise null.</returns>
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetElevation(DbGeography geographyValue);

        /// <summary>
        /// Returns the M (Measure) coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The M (Measure) coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetMeasure(DbGeography geographyValue);

        #endregion

        #region Curve

        /// <summary>
        /// Returns a nullable double value that indicates the length of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// The length of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetLength(DbGeography geographyValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value that represents the start point of the given DbGeography value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// The start point of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography GetStartPoint(DbGeography geographyValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value that represents the end point of the given DbGeography value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>The end point of geographyValue, if it represents a curve; otherwise null.</returns>
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography GetEndPoint(DbGeography geographyValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value is closed, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value is closed; otherwise, false.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool? GetIsClosed(DbGeography geographyValue);

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Returns the number of points in the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a linestring or linear ring.
        /// </summary>
        /// <returns>
        /// The number of points in the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a linestring or linear ring.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int? GetPointCount(DbGeography geographyValue);

        /// <summary>
        /// Returns a point element of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value, if it represents a linestring or linear ring.
        /// </summary>
        /// <returns>The point in geographyValue at position index, if it represents a linestring or linear ring; otherwise null.</returns>
        /// <param name="geographyValue">The geography value, which need not represent a linestring or linear ring.</param>
        /// <param name="index">The position within the geography value from which the element should be taken.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeography PointAt(DbGeography geographyValue, int index);

        #endregion

        #region Surface

        /// <summary>
        /// Returns a nullable double value that indicates the area of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value, which may be null if the value does not represent a surface.
        /// </summary>
        /// <returns>
        /// A nullable double value that indicates the area of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geographyValue">The geography value, which need not represent a surface.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geographyValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geographyValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetArea(DbGeography geographyValue);

        #endregion

        #endregion

        #region Geometry API

        /// <summary>
        /// This method is intended for use by derived implementations of
        /// <see
        ///     cref="M:System.Data.Entity.Spatial.DbSpatialServices.GeometryFromProviderValue(System.Object)" />
        /// after suitable validation of the specified provider value to ensure it is suitable for use with the derived implementation.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> instance that contains the specified providerValue and uses the specified spatialServices as its spatial implementation.
        /// </returns>
        /// <param name="spatialServices">
        /// The spatial services instance that the returned <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value will depend on for its implementation of spatial functionality.
        /// </param>
        /// <param name="providerValue">A provider value.</param>
        protected static DbGeometry CreateGeometry(DbSpatialServices spatialServices, object providerValue)
        {
            Check.NotNull(spatialServices, "spatialServices");
            Check.NotNull(providerValue, "providerValue");
            return new DbGeometry(spatialServices, providerValue);
        }

        /// <summary>
        /// Creates a provider-specific value compatible with this spatial services implementation based on the specified well-known
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// representation.
        /// </summary>
        /// <returns>A provider-specific value that encodes the information contained in wellKnownValue in a fashion compatible with this spatial services implementation.</returns>
        /// <param name="wellKnownValue">
        /// An instance of <see cref="T:System.Data.Entity.Spatial.DbGeometryWellKnownValue" /> that contains the well-known representation of a geometry value.
        /// </param>        
        public abstract object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue);

        /// <summary>
        /// Creates an instance of <see cref="T:System.Data.Entity.Spatial.DbGeometryWellKnownValue" /> that represents the specified
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value using one or both of the standard well-known spatial formats.
        /// </summary>
        /// <returns>
        /// The well-known representation of geometryValue, as a new
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometryWellKnownValue" />
        /// .
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on a provider-specific value that is compatible with this spatial services implementation.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value backed by this spatial services implementation and the specified provider value.
        /// </returns>
        /// <param name="providerValue">A provider-specific value that this spatial services implementation is capable of interpreting as a geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="providerValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="providerValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GeometryFromProviderValue(object providerValue);

        #region Geometry Constructors - well known binary

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified well-known binary value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> line value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="lineWellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> point value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="pointWellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> polygon value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multiline value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// The new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multiline value
        /// </returns>
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
        public abstract DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multipoint value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multipoint value.
        /// </returns>
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
        public abstract DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multi polygon value based on the specified well-known binary value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multi polygon value.
        /// </returns>
        /// <param name="multiPolygonWellKnownBinary">The well-known binary value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> collection value based on the specified well-known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known binary value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geometryCollectionWellKnownBinary">A byte array that contains a well-known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" />value should use.
        /// </param>
        public abstract DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId);

        #endregion

        #region Geometry Constructors - well known text

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified well-known text value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        public abstract DbGeometry GeometryFromText(string wellKnownText);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="wellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> line value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="lineWellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> point value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="pointWellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> polygon value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="polygonWellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multiline value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multiline value
        /// </returns>
        /// <param name="multiLineWellKnownText">The well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multipoint value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multipoint value.
        /// </returns>
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
        public abstract DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multi polygon value based on the specified well-known text value and coordinate system identifier.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> multi polygon value.
        /// </returns>
        /// <param name="multiPolygonKnownText">The well-known text value.</param>
        /// <param name="coordinateSystemId">The coordinate system identifier.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
            Justification = "Match OGC, EDM")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi",
            Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> collection value based on the specified well-known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the well-known text value with the specified coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geometryCollectionWellKnownText">A string that contains a well-known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        public abstract DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId);

        #endregion

        #region Geometry Constructors - Geography Markup Language (GML)

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the GML value with the default
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// coordinate system identifier (SRID) (
        /// <see
        ///     cref="P:System.Data.Entity.Spatial.DbGeometry.DefaultCoordinateSystemId" />
        /// ).
        /// </returns>
        /// <param name="geometryMarkup">A string that contains a Geography Markup Language (GML) representation of the geometry value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeometry GeometryFromGml(string geometryMarkup);

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value as defined by the GML value with the specified coordinate system identifier (SRID).
        /// </returns>
        /// <param name="geometryMarkup">A string that contains a Geography Markup Language (GML) representation of the geometry value.</param>
        /// <param name="coordinateSystemId">
        /// The identifier of the coordinate system that the new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value should use.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId);

        #endregion

        #region Geometry Instance Property Accessors

        /// <summary>
        /// Returns the coordinate system identifier of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </summary>
        /// <returns>
        /// The coordinate system identifier of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int GetCoordinateSystemId(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable double value that indicates the boundary of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value.
        /// </summary>
        /// <returns>
        /// The boundary of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetBoundary(DbGeometry geometryValue);

        /// <summary>
        /// Gets the dimension of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        /// <returns>
        /// The dimension of geometryValue, or the largest element dimension if
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// is a collection.
        /// </returns>
        /// <param name="geometryValue">The geometry value for which the dimension value should be retrieved.</param>
        public abstract int GetDimension(DbGeometry geometryValue);

        /// <summary>
        /// Gets the envelope (minimum bounding box) of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, as a geometry value.
        /// </summary>
        /// <returns>
        /// The envelope of geometryValue, as a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value for which the envelope value should be retrieved.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetEnvelope(DbGeometry geometryValue);

        /// <summary>
        /// Returns a value that indicates the spatial type name of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value.
        /// </summary>
        /// <returns>
        /// The spatial type name of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract string GetSpatialTypeName(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is empty.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is empty; otherwise, false.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool GetIsEmpty(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is simple.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is simple; otherwise, false.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool GetIsSimple(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is valid.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is valid; otherwise, false.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>        
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool GetIsValid(DbGeometry geometryValue);

        #endregion

        #region Geometry Well Known Format Conversion

        /// <summary>
        /// Gets the well-known text representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, including only X and Y coordinates for points.
        /// </summary>
        /// <returns>A string containing the well-known text representation of geometryValue.</returns>
        /// <param name="geometryValue">The geometry value for which the well-known text should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract string AsText(DbGeometry geometryValue);

        /// <summary>
        /// Returns a text representation of <see cref="T:System.Data.Entity.Spatial.DbSpatialServices" /> with elevation and measure.
        /// </summary>
        /// <returns>
        /// A text representation of <see cref="T:System.Data.Entity.Spatial.DbSpatialServices" /> with elevation and measure.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public virtual string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
        {
            return null;
        }

        /// <summary>
        /// Gets the well-known binary representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </summary>
        /// <returns>
        /// The well-known binary representation of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value for which the well-known binary should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract byte[] AsBinary(DbGeometry geometryValue);

        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value.
        /// </summary>
        /// <returns>A string containing the GML representation of this DbGeometry value.</returns>
        /// <param name="geometryValue">The geometry value for which the GML should be generated.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract string AsGml(DbGeometry geometryValue);

        #endregion

        #region Geometry Instance Methods - Spatial Relation

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values are spatially equal.
        /// </summary>
        /// <returns>true if geometryValue is spatially equal to otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value to compare for equality.</param>
        /// <param name="otherGeometry">The second geometry value to compare for equality.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values are spatially disjoint.
        /// </summary>
        /// <returns>true if geometryValue is disjoint from otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value to compare for disjointness.</param>
        /// <param name="otherGeometry">The second geometry value to compare for disjointness.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values spatially intersect.
        /// </summary>
        /// <returns>true if geometryValue intersects otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value to compare for intersection.</param>
        /// <param name="otherGeometry">The second geometry value to compare for intersection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values spatially touch.
        /// </summary>
        /// <returns>true if geometryValue touches otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values spatially cross.
        /// </summary>
        /// <returns>true if geometryValue crosses otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether one <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is spatially within the other.
        /// </summary>
        /// <returns>true if geometryValue is within otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Within(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether one <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value spatially contains the other.
        /// </summary>
        /// <returns>true if geometryValue contains otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values spatially overlap.
        /// </summary>
        /// <returns>true if geometryValue overlaps otherGeometry; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values are spatially related according to the given Dimensionally Extended Nine-Intersection Model (DE-9IM) intersection pattern.
        /// </summary>
        /// <returns>true if this geometryValue value relates to otherGeometry according to the specified intersection pattern matrix; otherwise false.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The geometry value that should be compared with the first geometry value for relation.</param>
        /// <param name="matrix">A string that contains the text representation of the (DE-9IM) intersection pattern that defines the relation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// ,
        /// <paramref name="otherGeometry" />
        /// or
        /// <paramref name="matrix" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix);

        #endregion

        #region Geometry Instance Methods - Spatial Analysis

        /// <summary>
        /// Creates a geometry value representing all points less than or equal to distance from the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value.
        /// </summary>
        /// <returns>A new DbGeometry value representing all points less than or equal to distance from geometryValue.</returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="distance">A double value specifying how far from geometryValue to buffer.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry Buffer(DbGeometry geometryValue, double distance);

        /// <summary>
        /// Computes the distance between the closest points in two <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values.
        /// </summary>
        /// <returns>A double value that specifies the distance between the two closest points in geometryValue and otherGeometry.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double Distance(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Returns a nullable double value that indicates the convex hull of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeography" />
        /// value.
        /// </summary>
        /// <returns>
        /// The convex hull of the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetConvexHull(DbGeometry geometryValue);

        /// <summary>
        /// Computes the intersection of two <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value representing the intersection of geometryValue and otherGeometry.
        /// </returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the union of two <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value representing the union of geometryValue and otherGeometry.
        /// </returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the difference between two <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values.
        /// </summary>
        /// <returns>A new DbGeometry value representing the difference between geometryValue and otherGeometry.</returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the symmetric difference between two <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> values.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value representing the symmetric difference between geometryValue and otherGeometry.
        /// </returns>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// or
        /// <paramref name="otherGeometry" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry);

        #endregion

        #region Geometry Collection

        /// <summary>
        /// Returns the number of elements in the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a geometry collection.
        /// </summary>
        /// <returns>The number of elements in geometryValue, if it represents a collection of other geometry values; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a geometry collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int? GetElementCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns an element of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a geometry collection.
        /// </summary>
        /// <returns>The element in geometryValue at position index, if it represents a collection of other geometry values; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a geometry collection.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry ElementAt(DbGeometry geometryValue, int index);

        #endregion

        #region Point

        /// <summary>
        /// Returns the X coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The X coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetXCoordinate(DbGeometry geometryValue);

        /// <summary>
        /// Returns the Y coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The Y coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetYCoordinate(DbGeometry geometryValue);

        /// <summary>
        /// Returns the elevation (Z) of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a point.
        /// </summary>
        /// <returns>The elevation (Z) of geometryValue, if it represents a point; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetElevation(DbGeometry geometryValue);

        /// <summary>
        /// Returns the M (Measure) coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a point.
        /// </summary>
        /// <returns>
        /// The M (Measure) coordinate of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetMeasure(DbGeometry geometryValue);

        #endregion

        #region Curve

        /// <summary>
        /// Returns a nullable double value that indicates the length of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// The length of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetLength(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents the start point of the given DbGeometry value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// The start point of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetStartPoint(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents the end point of the given DbGeometry value, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>The end point of geometryValue, if it represents a curve; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetEndPoint(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is closed, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value is closed; otherwise, false.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool? GetIsClosed(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is a ring, which may be null if the value does not represent a curve.
        /// </summary>
        /// <returns>
        /// True if the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value is a ring; otherwise, false.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract bool? GetIsRing(DbGeometry geometryValue);

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Returns the number of points in the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a linestring or linear ring.
        /// </summary>
        /// <returns>
        /// The number of points in the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a linestring or linear ring.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int? GetPointCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns a point element of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a linestring or linear ring.
        /// </summary>
        /// <returns>The point in geometryValue at position index, if it represents a linestring or linear ring; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a linestring or linear ring.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry PointAt(DbGeometry geometryValue, int index);

        #endregion

        #region Surface

        /// <summary>
        /// Returns a nullable double value that indicates the area of the given
        /// <see
        ///     cref="T:System.Data.Entity.Spatial.DbGeometry" />
        /// value, which may be null if the value does not represent a surface.
        /// </summary>
        /// <returns>
        /// A nullable double value that indicates the area of the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract double? GetArea(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents the centroid of the given DbGeometry value, which may be null if the value does not represent a surface.
        /// </summary>
        /// <returns>The centroid of geometryValue, if it represents a surface; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Centroid",
            Justification = "Naming convention prescribed by OGC specification")]
        public abstract DbGeometry GetCentroid(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents a point on the surface of the given DbGeometry value, which may be null if the value does not represent a surface.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents a point on the surface of the given DbGeometry value.
        /// </returns>
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetPointOnSurface(DbGeometry geometryValue);

        #endregion

        #region Polygon

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value that represents the exterior ring of the given DbGeometry value, which may be null if the value does not represent a polygon.
        /// </summary>
        /// <returns>A DbGeometry value representing the exterior ring on geometryValue, if it represents a polygon; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry GetExteriorRing(DbGeometry geometryValue);

        /// <summary>
        /// Returns the number of interior rings in the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a polygon.
        /// </summary>
        /// <returns>The number of elements in geometryValue, if it represents a polygon; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract int? GetInteriorRingCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns an interior ring from the given <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value, if it represents a polygon.
        /// </summary>
        /// <returns>The interior ring in geometryValue at position index, if it represents a polygon; otherwise null.</returns>
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="geometryValue" />
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="geometryValue" />
        /// is not compatible with this spatial services implementation.
        /// </exception>
        public abstract DbGeometry InteriorRingAt(DbGeometry geometryValue, int index);

        #endregion

        #endregion
    }
}
