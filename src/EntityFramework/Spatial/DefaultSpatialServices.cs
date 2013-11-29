// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    internal sealed class DefaultSpatialServices : DbSpatialServices
    {
        #region Provider Value Type

        [Serializable]
        private sealed class ReadOnlySpatialValues
        {
            private readonly int srid;
            private readonly byte[] wkb;
            private readonly string wkt;
            private readonly string gml;

            internal ReadOnlySpatialValues(int spatialRefSysId, string textValue, byte[] binaryValue, string gmlValue)
            {
                srid = spatialRefSysId;
                wkb = (binaryValue == null ? null : (byte[])binaryValue.Clone());
                wkt = textValue;
                gml = gmlValue;
            }

            internal int CoordinateSystemId
            {
                get { return srid; }
            }

            internal byte[] CloneBinary()
            {
                return (wkb == null ? null : (byte[])wkb.Clone());
            }

            internal string Text
            {
                get { return wkt; }
            }

            internal string GML
            {
                get { return gml; }
            }
        }

        #endregion

        internal static readonly DefaultSpatialServices Instance = new DefaultSpatialServices();

        private DefaultSpatialServices()
        {
        }

        private static Exception SpatialServicesUnavailable()
        {
            return new NotImplementedException(Strings.SpatialProviderNotUsable);
        }

        private static ReadOnlySpatialValues CheckProviderValue(object providerValue)
        {
            var expectedValue = providerValue as ReadOnlySpatialValues;
            if (expectedValue == null)
            {
                throw new ArgumentException(Strings.Spatial_ProviderValueNotCompatibleWithSpatialServices, "providerValue");
            }
            return expectedValue;
        }

        private static ReadOnlySpatialValues CheckCompatible(DbGeography geographyValue)
        {
            DebugCheck.NotNull(geographyValue);
            if (geographyValue != null)
            {
                var expectedValue = geographyValue.ProviderValue as ReadOnlySpatialValues;
                if (expectedValue != null)
                {
                    return expectedValue;
                }
            }
            throw new ArgumentException(Strings.Spatial_GeographyValueNotCompatibleWithSpatialServices, "geographyValue");
        }

        private static ReadOnlySpatialValues CheckCompatible(DbGeometry geometryValue)
        {
            DebugCheck.NotNull(geometryValue);
            if (geometryValue != null)
            {
                var expectedValue = geometryValue.ProviderValue as ReadOnlySpatialValues;
                if (expectedValue != null)
                {
                    return expectedValue;
                }
            }
            throw new ArgumentException(Strings.Spatial_GeometryValueNotCompatibleWithSpatialServices, "geometryValue");
        }

        #region Geography API

        public override DbGeography GeographyFromProviderValue(object providerValue)
        {
            Check.NotNull(providerValue, "providerValue");
            var expectedValue = CheckProviderValue(providerValue);
            return CreateGeography(this, expectedValue);
        }

        public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
        {
            Check.NotNull(wellKnownValue, "wellKnownValue");
            return new ReadOnlySpatialValues(
                wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, gmlValue: null);
        }

        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            var backingValue = CheckCompatible(geographyValue);
            return new DbGeographyWellKnownValue
                {
                    CoordinateSystemId = backingValue.CoordinateSystemId,
                    WellKnownBinary = backingValue.CloneBinary(),
                    WellKnownText = backingValue.Text
                };
        }

        #region Static Constructors - Well Known Binary (WKB)

        public override DbGeography GeographyFromBinary(byte[] geographyBinary)
        {
            Check.NotNull(geographyBinary, "geographyBinary");
            var backingValue = new ReadOnlySpatialValues(
                DbGeography.DefaultCoordinateSystemId, textValue: null, binaryValue: geographyBinary, gmlValue: null);
            return CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            Check.NotNull(geographyBinary, "geographyBinary");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: null, binaryValue: geographyBinary, gmlValue: null);
            return CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon",
            Justification = "Match MultiPoint, MultiLine")]
        public override DbGeography GeographyMultiPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyCollectionFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - Well Known Text (WKT)

        public override DbGeography GeographyFromText(string geographyText)
        {
            Check.NotNull(geographyText, "geographyText");
            var backingValue = new ReadOnlySpatialValues(
                DbGeography.DefaultCoordinateSystemId, textValue: geographyText, binaryValue: null, gmlValue: null);
            return CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromText(string geographyText, int spatialReferenceSystemId)
        {
            Check.NotNull(geographyText, "geographyText");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: geographyText, binaryValue: null, gmlValue: null);
            return CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyLineFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPointFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPolygonFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiLineFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiPointFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon",
            Justification = "Match MultiPoint, MultiLine")]
        public override DbGeography GeographyMultiPolygonFromText(string multiPolygonKnownText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyCollectionFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - GML

        public override DbGeography GeographyFromGml(string geographyMarkup)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            var backingValue = new ReadOnlySpatialValues(
                DbGeography.DefaultCoordinateSystemId, textValue: null, binaryValue: null, gmlValue: geographyMarkup);
            return CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromGml(string geographyMarkup, int spatialReferenceSystemId)
        {
            Check.NotNull(geographyMarkup, "geographyMarkup");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: null, binaryValue: null, gmlValue: geographyMarkup);
            return CreateGeography(this, backingValue);
        }

        #endregion

        #region Geography Instance Property Accessors

        public override int GetCoordinateSystemId(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            var backingValue = CheckCompatible(geographyValue);
            return backingValue.CoordinateSystemId;
        }

        public override int GetDimension(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override string GetSpatialTypeName(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsEmpty(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geography Well Known Format Conversion

        public override string AsText(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            var expectedValue = CheckCompatible(geographyValue);
            return expectedValue.Text;
        }

        public override byte[] AsBinary(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            var expectedValue = CheckCompatible(geographyValue);
            return expectedValue.CloneBinary();
        }

        public override string AsGml(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");
            var expectedValue = CheckCompatible(geographyValue);
            return expectedValue.GML;
        }

        #endregion

        #region Geography Instance Methods - Spatial Relation

        public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geography Instance Methods - Spatial Analysis

        public override DbGeography Buffer(DbGeography geographyValue, double distance)
        {
            throw SpatialServicesUnavailable();
        }

        public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geography Collection

        public override int? GetElementCount(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography ElementAt(DbGeography geographyValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Point

        public override double? GetLatitude(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetLongitude(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetElevation(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetMeasure(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Curve

        public override double? GetLength(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GetEndPoint(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GetStartPoint(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsClosed(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region LineString, Line, LinearRing

        public override int? GetPointCount(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography PointAt(DbGeography geographyValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Surface

        public override double? GetArea(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #endregion

        #region Geometry API

        public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
        {
            Check.NotNull(wellKnownValue, "wellKnownValue");
            return new ReadOnlySpatialValues(
                wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, gmlValue: null);
        }

        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var backingValue = CheckCompatible(geometryValue);
            return new DbGeometryWellKnownValue
                {
                    CoordinateSystemId = backingValue.CoordinateSystemId,
                    WellKnownBinary = backingValue.CloneBinary(),
                    WellKnownText = backingValue.Text
                };
        }

        public override DbGeometry GeometryFromProviderValue(object providerValue)
        {
            Check.NotNull(providerValue, "providerValue");
            var expectedValue = CheckProviderValue(providerValue);
            return CreateGeometry(this, expectedValue);
        }

        #region Static Constructors - Well Known Binary (WKB)

        public override DbGeometry GeometryFromBinary(byte[] geometryBinary)
        {
            Check.NotNull(geometryBinary, "geometryBinary");
            var backingValue = new ReadOnlySpatialValues(
                DbGeometry.DefaultCoordinateSystemId, textValue: null, binaryValue: geometryBinary, gmlValue: null);
            return CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            Check.NotNull(geometryBinary, "geometryBinary");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: null, binaryValue: geometryBinary, gmlValue: null);
            return CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon",
            Justification = "Match MultiPoint, MultiLine")]
        public override DbGeometry GeometryMultiPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryCollectionFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - Well Known Text (WKT)

        public override DbGeometry GeometryFromText(string geometryText)
        {
            Check.NotNull(geometryText, "geometryText");
            var backingValue = new ReadOnlySpatialValues(
                DbGeometry.DefaultCoordinateSystemId, textValue: geometryText, binaryValue: null, gmlValue: null);
            return CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromText(string geometryText, int spatialReferenceSystemId)
        {
            Check.NotNull(geometryText, "geometryText");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: geometryText, binaryValue: null, gmlValue: null);
            return CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryLineFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPointFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPolygonFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiLineFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiPointFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon",
            Justification = "Match MultiPoint, MultiLine")]
        public override DbGeometry GeometryMultiPolygonFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryCollectionFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - GML

        public override DbGeometry GeometryFromGml(string geometryMarkup)
        {
            Check.NotNull(geometryMarkup, "geometryMarkup");
            var backingValue = new ReadOnlySpatialValues(
                DbGeometry.DefaultCoordinateSystemId, textValue: null, binaryValue: null, gmlValue: geometryMarkup);
            return CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromGml(string geometryMarkup, int spatialReferenceSystemId)
        {
            Check.NotNull(geometryMarkup, "geometryMarkup");
            var backingValue = new ReadOnlySpatialValues(
                spatialReferenceSystemId, textValue: null, binaryValue: null, gmlValue: geometryMarkup);
            return CreateGeometry(this, backingValue);
        }

        #endregion

        #region Geometry Instance Property Accessors

        public override int GetCoordinateSystemId(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var backingValue = CheckCompatible(geometryValue);
            return backingValue.CoordinateSystemId;
        }

        public override DbGeometry GetBoundary(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override int GetDimension(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetEnvelope(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override string GetSpatialTypeName(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsEmpty(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsSimple(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsValid(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Well Known Format Conversion

        public override string AsText(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var expectedValue = CheckCompatible(geometryValue);
            return expectedValue.Text;
        }

        public override byte[] AsBinary(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var expectedValue = CheckCompatible(geometryValue);
            return expectedValue.CloneBinary();
        }

        public override string AsGml(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var expectedValue = CheckCompatible(geometryValue);
            return expectedValue.GML;
        }

        #endregion

        #region Geometry Instance Methods - Spatial Relation

        public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Spatial Analysis

        public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
        {
            throw SpatialServicesUnavailable();
        }

        public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetConvexHull(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Geometry Collection

        public override int? GetElementCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Geometry Collection

        public override double? GetXCoordinate(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetYCoordinate(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetElevation(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetMeasure(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Curve

        public override double? GetLength(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetEndPoint(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetStartPoint(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsClosed(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsRing(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region LineString, Line, LinearRing

        public override int? GetPointCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry PointAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Surface

        public override double? GetArea(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetCentroid(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Polygon

        public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override int? GetInteriorRingCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #endregion
    }
}
