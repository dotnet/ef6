// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Data.Spatial;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SampleEntityFrameworkProvider
{
    internal sealed class SpatialServices : DbSpatialServices
    {
        internal static readonly SpatialServices Instance = new SpatialServices();

        private const int DefaultCoordinateId = 4326;

        private SpatialServices()
        {
        }

        #region DbSpatialServices overriden methods

        public override byte[] AsBinary(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geographyValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.STAsBinary().Value;
        }

        public override byte[] AsBinary(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.STAsBinary().Value;
        }

        public override string AsGml(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.AsGml().Value;
        }

        public override string AsGml(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.AsGml().Value;
        }

        public override string AsText(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.STAsText().ToSqlString().Value;
        }

        public override string AsText(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.STAsText().ToSqlString().Value;
        }

        public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return GeometryFromProviderValue(providerValue.STBuffer(distance));
        }

        public override DbGeography Buffer(DbGeography geographyValue, double distance)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return GeographyFromProviderValue(providerValue.STBuffer(distance));
        }

        public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
        {
            CheckParameterNotNull("wellKnownValue", wellKnownValue);

            if (wellKnownValue.WellKnownText != null)
            {
                return InvokeStaticFunction(
                    SqlTypes.SqlGeometryType,
                    "STGeomFromText",
                    SqlTypes.SqlCharsFromString(wellKnownValue.WellKnownText),
                    wellKnownValue.CoordinateSystemId);
            }
            else if (wellKnownValue.WellKnownBinary != null)
            {
                return InvokeStaticFunction(
                    SqlTypes.SqlGeometryType,
                    "STGeomFromWKB",
                    SqlTypes.SqlBytesFromByteArray(wellKnownValue.WellKnownBinary),
                    wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw new ArgumentException("wellKnownValue");
            }
        }

        public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
        {
            CheckParameterNotNull("wellKnownValue", wellKnownValue);

            if(wellKnownValue.WellKnownText != null)
            {
                return InvokeStaticFunction(
                    SqlTypes.SqlGeographyType,
                    "STGeomFromText",
                    SqlTypes.SqlCharsFromString(wellKnownValue.WellKnownText),
                    wellKnownValue.CoordinateSystemId);
            }
            else if(wellKnownValue.WellKnownBinary != null)
            {
                return InvokeStaticFunction(
                    SqlTypes.SqlGeographyType,
                    "STGeomFromWKB",
                    SqlTypes.SqlBytesFromByteArray(wellKnownValue.WellKnownBinary),
                    wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw new ArgumentException("wellKnownValue");
            }
        }

        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            Debug.Assert(geographyValue.AsBinary() != null, "WKB value must not be null.");
            Debug.Assert(geographyValue.AsText() != null, "WKT value must not be null.");

            return new DbGeographyWellKnownValue()
                       {
                           CoordinateSystemId = geographyValue.CoordinateSystemId,
                           WellKnownBinary = geographyValue.AsBinary(),
                           WellKnownText = geographyValue.AsText()
                       };
        }

        public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            dynamic otherProviderValue = otherGeometry == null ? null : otherGeometry.ProviderValue;
            return providerValue.STDistance(otherProviderValue).Value;
        }

        public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            dynamic otherProviderValue = otherGeography == null ? null : otherGeography.ProviderValue;
            return providerValue.STDistance(otherProviderValue).Value;
        }

        public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography ElementAt(DbGeography geographyValue, int index)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
        {
            CheckParameterNotNull("wellKnownBinary", wellKnownBinary);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeographyType,
                "STGeomFromWKB",
                SqlTypes.SqlBytesFromByteArray(wellKnownBinary),
                coordinateSystemId);

            return GeographyFromProviderValue(providerValue);
        }

        public override DbGeography GeographyFromBinary(byte[] wellKnownBinary)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId)
        {
            CheckParameterNotNull("geographyMarkup", geographyMarkup);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeographyType,
                "GeomFromGml",
                SqlTypes.SqlXmlFromString(geographyMarkup),
                coordinateSystemId);

            return GeographyFromProviderValue(providerValue);
        }

        public override DbGeography GeographyFromGml(string geographyMarkup)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyFromProviderValue(object providerValue)
        {
            return 
                providerValue == null || ((dynamic)providerValue).IsNull ? 
                    null : 
                    DbSpatialServices.CreateGeography(this, providerValue);
        }

        public override DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId)
        {
            CheckParameterNotNull("wellKnownText", wellKnownText);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeographyType, 
                "STGeomFromText",
                SqlTypes.SqlCharsFromString(wellKnownText),
                coordinateSystemId);

            return GeographyFromProviderValue(providerValue);
        }

        public override DbGeography GeographyFromText(string wellKnownText)
        {
            return GeographyFromText(wellKnownText, DefaultCoordinateId);
        }

        public override DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyMultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
        {
            CheckParameterNotNull("wellKnownBinary", wellKnownBinary);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeometryType,
                "STGeomFromWKB",
                SqlTypes.SqlBytesFromByteArray(wellKnownBinary),
                coordinateSystemId);

            return GeometryFromProviderValue(providerValue);
        }

        public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId)
        {
            CheckParameterNotNull("geometryMarkup", geometryMarkup);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeometryType,
                "GeomFromGml",
                SqlTypes.SqlXmlFromString(geometryMarkup),
                coordinateSystemId);

            return GeometryFromProviderValue(providerValue);
        }

        public override DbGeometry GeometryFromGml(string geometryMarkup)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryFromProviderValue(object providerValue)
        {
            return
                providerValue == null || ((dynamic)providerValue).IsNull ?
                    null :
                    DbSpatialServices.CreateGeometry(this, providerValue);
        }

        public override DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId)
        {
            CheckParameterNotNull("wellKnownText", wellKnownText);

            var providerValue = InvokeStaticFunction(
                SqlTypes.SqlGeometryType,
                "STGeomFromText",
                SqlTypes.SqlCharsFromString(wellKnownText),
                coordinateSystemId);

            return GeometryFromProviderValue(providerValue);
        }

        public override DbGeometry GeometryFromText(string wellKnownText)
        {
            return GeometryFromText(wellKnownText, DefaultCoordinateId);
        }

        public override DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetArea(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.STArea().Value;
        }

        public override double? GetArea(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.STArea().Value;
        }

        public override DbGeometry GetBoundary(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetCentroid(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetConvexHull(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override int GetCoordinateSystemId(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return (int)providerValue.STSrid;
        }

        public override int GetCoordinateSystemId(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return (int)providerValue.STSrid;
        }

        public override int GetDimension(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override int GetDimension(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override int? GetElementCount(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override int? GetElementCount(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetElevation(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetElevation(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetEndPoint(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GetEndPoint(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetEnvelope(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override int? GetInteriorRingCount(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool? GetIsClosed(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool? GetIsClosed(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetIsEmpty(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetIsEmpty(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool? GetIsRing(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetIsSimple(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetIsValid(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetLatitude(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.Lat.Value;
        }

        public override double? GetLength(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetLength(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetLongitude(DbGeography geographyValue)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            return providerValue.Long.Value;
        }

        public override double? GetMeasure(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetMeasure(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override int? GetPointCount(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override int? GetPointCount(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override string GetSpatialTypeName(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override string GetSpatialTypeName(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry GetStartPoint(DbGeometry geometryValue)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography GetStartPoint(DbGeography geographyValue)
        {
            throw new System.NotImplementedException();
        }

        public override double? GetXCoordinate(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.STX.Value;
        }

        public override double? GetYCoordinate(DbGeometry geometryValue)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            return providerValue.STY.Value;
        }

        public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry PointAt(DbGeometry geometryValue, int index)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography PointAt(DbGeography geographyValue, int index)
        {
            throw new System.NotImplementedException();
        }

        public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
        {
            throw new System.NotImplementedException();
        }

        public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            CheckParameterNotNull("geometryValue", geometryValue);

            dynamic providerValue = geometryValue.ProviderValue;
            dynamic otherProviderValue = otherGeometry == null ? null : otherGeometry.ProviderValue;
            return providerValue.STEquals(otherProviderValue).Value;
        }

        public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
        {
            CheckParameterNotNull("geographyValue", geographyValue);

            dynamic providerValue = geographyValue.ProviderValue;
            dynamic otherProviderValue = otherGeography == null ? null : otherGeography.ProviderValue;
            return providerValue.STEquals(otherProviderValue).Value;
        }

        public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw new System.NotImplementedException();
        }

        public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        private static void CheckParameterNotNull(string name, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        private static object InvokeStaticFunction(Type type, string functionName, params object[] parameters)
        {
            var methodInfo = type.GetMethod(functionName);
            Debug.Assert(methodInfo != null, "methodInfo != null");
            return methodInfo.Invoke(null, parameters);
        }
    }
}
