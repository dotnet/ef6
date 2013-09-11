// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class DbGeographyTests
    {
        [Fact]
        public void Provider_property_returns_spatial_provider_being_used()
        {
            var provider = new Mock<DbSpatialServices>();

            Assert.Same(provider.Object, new DbGeography(provider.Object, "Foo").Provider);
        }

        [Fact]
        public void Public_members_check_for_null_arguments()
        {
            TestNullArgument("wellKnownBinary", () => DbGeography.FromBinary(null));
            TestNullArgument("wellKnownBinary", () => DbGeography.FromBinary(null, 1));
            TestNullArgument("lineWellKnownBinary", () => DbGeography.LineFromBinary(null, 1));
            TestNullArgument("pointWellKnownBinary", () => DbGeography.PointFromBinary(null, 1));
            TestNullArgument("polygonWellKnownBinary", () => DbGeography.PolygonFromBinary(null, 1));
            TestNullArgument("multiLineWellKnownBinary", () => DbGeography.MultiLineFromBinary(null, 1));
            TestNullArgument("multiPointWellKnownBinary", () => DbGeography.MultiPointFromBinary(null, 1));
            TestNullArgument("multiPolygonWellKnownBinary", () => DbGeography.MultiPolygonFromBinary(null, 1));
            TestNullArgument("geographyCollectionWellKnownBinary", () => DbGeography.GeographyCollectionFromBinary(null, 1));

            TestNullArgument("geographyMarkup", () => DbGeography.FromGml(null));
            TestNullArgument("geographyMarkup", () => DbGeography.FromGml(null, 1));

            TestNullArgument("wellKnownText", () => DbGeography.FromText(null));
            TestNullArgument("wellKnownText", () => DbGeography.FromText(null, 1));
            TestNullArgument("lineWellKnownText", () => DbGeography.LineFromText(null, 1));
            TestNullArgument("pointWellKnownText", () => DbGeography.PointFromText(null, 1));
            TestNullArgument("polygonWellKnownText", () => DbGeography.PolygonFromText(null, 1));
            TestNullArgument("multiLineWellKnownText", () => DbGeography.MultiLineFromText(null, 1));
            TestNullArgument("multiPointWellKnownText", () => DbGeography.MultiPointFromText(null, 1));
            TestNullArgument("multiPolygonWellKnownText", () => DbGeography.MultiPolygonFromText(null, 1));
            TestNullArgument("geographyCollectionWellKnownText", () => DbGeography.GeographyCollectionFromText(null, 1));

            TestNullArgument("other", s => s.SpatialEquals(null));
            TestNullArgument("other", s => s.Disjoint(null));
            TestNullArgument("other", s => s.Intersects(null));

            TestNullArgument("distance", s => s.Buffer(null));
            TestNullArgument("other", s => s.Distance(null));
            TestNullArgument("other", s => s.Intersection(null));
            TestNullArgument("other", s => s.Union(null));
            TestNullArgument("other", s => s.Difference(null));
            TestNullArgument("other", s => s.SymmetricDifference(null));
        }

        private void TestNullArgument(string parameterName, Action test)
        {
            Assert.Equal(parameterName, Assert.Throws<ArgumentNullException>(() => test()).ParamName);
        }

        private void TestNullArgument(string parameterName, Action<DbGeography> test)
        {
            Assert.Equal(parameterName, Assert.Throws<ArgumentNullException>(() => test(new DbGeography())).ParamName);
        }

        [Fact]
        public void Properties_correctly_delegate_to_spatial_provider()
        {
            PropertyDelegationHelper(g => g.Area, s => s.GetArea(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.CoordinateSystemId, s => s.GetCoordinateSystemId(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Dimension, s => s.GetDimension(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.ElementCount, s => s.GetElementCount(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Elevation, s => s.GetElevation(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.EndPoint, s => s.GetEndPoint(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.IsClosed, s => s.GetIsClosed(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.IsEmpty, s => s.GetIsEmpty(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Latitude, s => s.GetLatitude(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Length, s => s.GetLength(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Longitude, s => s.GetLongitude(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.Measure, s => s.GetMeasure(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.PointCount, s => s.GetPointCount(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.SpatialTypeName, s => s.GetSpatialTypeName(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.StartPoint, s => s.GetStartPoint(It.IsAny<DbGeography>()));
            PropertyDelegationHelper(g => g.WellKnownValue, s => s.CreateWellKnownValue(It.IsAny<DbGeography>()));
        }

        private void PropertyDelegationHelper(
            Func<DbGeography, object> operation, 
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geography = new DbGeography(spatialProviderMock.Object, DbGeography.FromText("POINT(1 1)"));
            var result = operation(geography);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Unary_methods_correctly_delegate_to_spatial_provider()
        {
            UnaryMethodDelegationHelper(g => g.AsBinary(), s => s.AsBinary(It.IsAny<DbGeography>()));
            UnaryMethodDelegationHelper(g => g.AsGml(), s => s.AsGml(It.IsAny<DbGeography>()));
            UnaryMethodDelegationHelper(g => g.AsText(), s => s.AsText(It.IsAny<DbGeography>()));
            UnaryMethodDelegationHelper(g => g.AsTextIncludingElevationAndMeasure(), s => s.AsTextIncludingElevationAndMeasure(It.IsAny<DbGeography>()));
        }

        private void UnaryMethodDelegationHelper(
            Func<DbGeography, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geography = new DbGeography(spatialProviderMock.Object, DbGeography.FromText("POINT(1 1)"));
            var result = operation(geography);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Binary_methods_correctly_delegate_to_spatial_provider()
        {
            BinaryMethodDelegationHelper((g1, g2) => g1.Difference(g2), s => s.Difference(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Disjoint(g2), s => s.Disjoint(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Distance(g2), s => s.Distance(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Intersection(g2), s => s.Intersection(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Intersects(g2), s => s.Intersects(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.SpatialEquals(g2), s => s.SpatialEquals(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.SymmetricDifference(g2), s => s.SymmetricDifference(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Union(g2), s => s.Union(It.IsAny<DbGeography>(), It.IsAny<DbGeography>()));
        }

        private void BinaryMethodDelegationHelper(
            Func<DbGeography, DbGeography, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geography1 = new DbGeography(spatialProviderMock.Object, DbGeography.FromText("POINT(1 1)"));
            var geography2 = new DbGeography(spatialProviderMock.Object, DbGeography.FromText("POINT(2 2)"));
            var result = operation(geography1, geography2);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Methods_with_integer_arguments_correctly_delegate_to_spatial_provider()
        {
            MethodWithIntegertArgumentDelegationHelper((g, i) => g.ElementAt(i), s => s.ElementAt(It.IsAny<DbGeography>(), It.IsAny<int>()));
            MethodWithIntegertArgumentDelegationHelper((g, i) => g.PointAt(i), s => s.PointAt(It.IsAny<DbGeography>(), It.IsAny<int>()));
        }

        private void MethodWithIntegertArgumentDelegationHelper(
            Func<DbGeography, int, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geography = new DbGeography(spatialProviderMock.Object, DbGeography.FromText("POINT(1 1)"));
            var result = operation(geography, 1);

            spatialProviderMock.Verify(verification, Times.Once());
        }
    }
}
