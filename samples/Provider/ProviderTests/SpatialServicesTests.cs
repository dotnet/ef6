// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SampleEntityFrameworkProvider;
using Xunit;

namespace ProviderTests
{
    public class SpatialServicesTests
    {
        private readonly DbSpatialServices spatialServices = SpatialServices.Instance;

        private const string PointWKT = "POINT (1 2)";

        private readonly byte[] PointWKB =
            new byte[]
                {
                    0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0xF0, 0x3F, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x40, 
                };

        private readonly XDocument GeographyPointGml =
            XDocument.Parse("<Point xmlns=\"http://www.opengis.net/gml\"><pos>2 1</pos></Point>");

        private readonly XDocument GeometryPointGml =
            XDocument.Parse("<Point xmlns=\"http://www.opengis.net/gml\"><pos>1 2</pos></Point>");

        private const string PolygonWKT = "POLYGON ((0 0, 5 0, 5 5, 0 5, 0 0))";

        private const int DefaultCoordinateSystemId = 4326;

        #region DbGeography tests

        [Fact]
        public void Verify_DbGeography_AsBinary_method()
        {
            Assert.True(spatialServices.GeographyFromText(PointWKT).AsBinary().SequenceEqual(PointWKB));
        }

        [Fact]
        public void Verify_DbGeography_AsGml_method()
        {
            var gml = XDocument.Parse(spatialServices.GeographyFromText(PointWKT).AsGml());
            Assert.True(XDocument.DeepEquals(gml, GeographyPointGml));
        }

        [Fact]
        public void Verify_DbGeography_AsText_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeographyFromText(PointWKT).AsText());
        }

        [Fact]
        public void Verify_DbGeography_Buffer_method()
        {
            const string line = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(
                line, 
                spatialServices.GeographyFromText(line).Buffer(0).AsText());
        }

        [Fact]
        public void Verify_DbGeography_CreateProviderValue_WKT_method()
        {
            var geographyWellKnownValue = new DbGeographyWellKnownValue()
                                              {
                                                  CoordinateSystemId = DefaultCoordinateSystemId,
                                                  WellKnownBinary = null,
                                                  WellKnownText = PointWKT
                                              };

            dynamic providerValue = spatialServices.CreateProviderValue(geographyWellKnownValue);
            Assert.Equal(PointWKT, providerValue.ToString());
        }

        [Fact]
        public void Verify_DbGeography_CreateProviderValue_WKB_method()
        {
            var geographyWellKnownValue = new DbGeographyWellKnownValue()
            {
                CoordinateSystemId = DefaultCoordinateSystemId,
                WellKnownBinary = PointWKB,
                WellKnownText = null
            };

            dynamic providerValue = spatialServices.CreateProviderValue(geographyWellKnownValue);
            Assert.Equal(PointWKT, providerValue.ToString());
        }

        [Fact]
        public void Verify_DbGeography_Distance_method()
        {
            var distance = spatialServices.Distance(
                spatialServices.GeographyFromText("POINT (1 2)"),
                spatialServices.GeographyFromText("POINT (2 1)"));

            Assert.True(distance > 156876.149075896D && distance < 156876.149075897D);
        }

        [Fact]
        public void Verify_DbGeography_FromBinary_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeographyFromBinary(PointWKB, DefaultCoordinateSystemId).AsText());
        }

        [Fact]
        public void Verify_DbGeography_FromGml_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeographyFromGml(GeographyPointGml.ToString(), DefaultCoordinateSystemId).AsText());
        }

        [Fact]
        public void Verify_DbGeography_FromProviderValue_method()
        {
            var providerValue = spatialServices.GeographyFromText(PointWKT).ProviderValue;

            Assert.Equal(
                PointWKT,
                spatialServices.GeographyFromProviderValue(providerValue).AsText());
        }

        [Fact]
        public void Verify_DbGeography_FromText_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeographyFromText(PointWKT).AsText());
        }

        [Fact]
        public void Verify_DbGeography_GetCoordinateSystemID_method()
        {
            Assert.Equal(
                DefaultCoordinateSystemId,
                spatialServices.GetCoordinateSystemId(spatialServices.GeographyFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeography_GetLatitude_method()
        {
            Assert.Equal(
                2,
                spatialServices.GetLatitude(spatialServices.GeographyFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeography_GetLongitude_method()
        {
            Assert.Equal(
                1,
                spatialServices.GetLongitude(spatialServices.GeographyFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeography_SpatialEquals_returns_true_for_equal_types()
        {
            Assert.True(
                spatialServices.SpatialEquals(
                    spatialServices.GeographyFromText(PointWKT),
                    spatialServices.GeographyFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeography_SpatialEquals_returns_false_for_not_equal_types()
        {
            Assert.False(
                spatialServices.SpatialEquals(
                    spatialServices.GeographyFromText("POINT(2 1)"),
                    spatialServices.GeographyFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeography_Area_method()
        {
            var area = spatialServices.GetArea(spatialServices.GeographyFromText(PolygonWKT));
            Assert.True(area > 307540516837.287 && area < 307540516837.289);
        }

        #endregion

        #region DbGeometry tests

        [Fact]
        public void Verify_DbGeometry_AsBinary_method()
        {
            Assert.True(spatialServices.GeometryFromText(PointWKT).AsBinary().SequenceEqual(PointWKB));
        }

        [Fact]
        public void Verify_DbGeometry_AsGml_method()
        {
            var gml = XDocument.Parse(spatialServices.GeometryFromText(PointWKT).AsGml());
            Assert.True(XDocument.DeepEquals(gml, GeometryPointGml));
        }

        [Fact]
        public void Verify_DbGeometry_AsText_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeometryFromText(PointWKT).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_Buffer_method()
        {
            const string line = "LINESTRING (-122.36 47.656, -122.343 47.656)";
            Assert.Equal(
                line,
                spatialServices.GeometryFromText(line).Buffer(0).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_CreateProviderValue_WKT_method()
        {
            var geometryWellKnownValue = new DbGeometryWellKnownValue()
            {
                CoordinateSystemId = DefaultCoordinateSystemId,
                WellKnownBinary = null,
                WellKnownText = PointWKT
            };

            dynamic providerValue = spatialServices.CreateProviderValue(geometryWellKnownValue);
            Assert.Equal(PointWKT, providerValue.ToString());
        }

        [Fact]
        public void Verify_DbGeometry_CreateProviderValue_WKB_method()
        {
            var geometryWellKnownValue = new DbGeometryWellKnownValue()
            {
                CoordinateSystemId = DefaultCoordinateSystemId,
                WellKnownBinary = PointWKB,
                WellKnownText = null
            };

            dynamic providerValue = spatialServices.CreateProviderValue(geometryWellKnownValue);
            Assert.Equal(PointWKT, providerValue.ToString());
        }

        [Fact]
        public void Verify_DbGeometry_Distance_method()
        {
            var distance = spatialServices.Distance(
                spatialServices.GeometryFromText("POINT (0 0)"),
                spatialServices.GeometryFromText("POINT (3 4)"));

            Assert.Equal(5, distance);
        }

        [Fact]
        public void Verify_DbGeometry_FromBinary_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeometryFromBinary(PointWKB, DefaultCoordinateSystemId).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_FromGml_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeometryFromGml(GeometryPointGml.ToString(), DefaultCoordinateSystemId).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_FromProviderValue_method()
        {
            var providerValue = spatialServices.GeometryFromText(PointWKT).ProviderValue;

            Assert.Equal(
                PointWKT,
                spatialServices.GeometryFromProviderValue(providerValue).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_FromText_method()
        {
            Assert.Equal(
                PointWKT,
                spatialServices.GeometryFromText(PointWKT).AsText());
        }

        [Fact]
        public void Verify_DbGeometry_GetCoordinateSystemID_method()
        {
            Assert.Equal(
                DefaultCoordinateSystemId,
                spatialServices.GetCoordinateSystemId(spatialServices.GeometryFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeometry_GetXCoordinate_method()
        {
            Assert.Equal(
                1,
                spatialServices.GetXCoordinate(spatialServices.GeometryFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeometry_GetYCoordinate_method()
        {
            Assert.Equal(
                2,
                spatialServices.GetYCoordinate(spatialServices.GeometryFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeometry_SpatialEquals_returns_true_for_equal_types()
        {
            Assert.True(
                spatialServices.SpatialEquals(
                    spatialServices.GeometryFromText(PointWKT),
                    spatialServices.GeometryFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeometry_SpatialEquals_returns_false_for_not_equal_types()
        {
            Assert.False(
                spatialServices.SpatialEquals(
                    spatialServices.GeometryFromText("POINT(2 1)"),
                    spatialServices.GeometryFromText(PointWKT)));
        }

        [Fact]
        public void Verify_DbGeometry_Area_method()
        {
            Assert.Equal(25, spatialServices.GetArea(spatialServices.GeometryFromText(PolygonWKT)));
        }

        #endregion
    }
}
