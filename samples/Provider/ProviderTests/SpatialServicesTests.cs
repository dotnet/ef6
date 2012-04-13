using System;
using System.Collections.Generic;
using System.Data.Spatial;
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

        private readonly XDocument PointGml =
            XDocument.Parse("<Point xmlns=\"http://www.opengis.net/gml\"><pos>2 1</pos></Point>");

        private const int DefaultCoordinateSystemId = 4326;

        [Fact]
        public void Verify_DbGeography_AsBinary_method()
        {
            Assert.True(
                spatialServices
                .GeographyFromText(PointWKT)
                .AsBinary()
                .SequenceEqual(PointWKB));
        }

        [Fact]
        public void Verify_DbGeography_AsGml_method()
        {
            var gml = XDocument.Parse(
                spatialServices
                    .GeographyFromText(PointWKT)
                    .AsGml());

            XNamespace gmlNs = "http://www.opengis.net/gml";

            Assert.Equal(
                "2 1", 
                (string)gml.Element(gmlNs + "Point").Element(gmlNs + "pos"));
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
                spatialServices.GeographyFromGml(PointGml.ToString(), DefaultCoordinateSystemId).AsText());
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
        public void Verify_DbGeography_SpatialEquals_method()
        {
            Assert.True(
                spatialServices.SpatialEquals(
                    spatialServices.GeographyFromText(PointWKT),
                    spatialServices.GeographyFromText(PointWKT)));
        }
    }
}
