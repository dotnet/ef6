// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using Moq;
    using System.Linq.Expressions;
    using Xunit;

    public class DbGeometryTests
    {
        [Fact]
        public void Provider_property_returns_spatial_provider_being_used()
        {
            var provider = new Mock<DbSpatialServices>();

            Assert.Same(provider.Object, new DbGeometry(provider.Object, "Foo").Provider);
        }

        [Fact]
        public void Public_members_check_for_null_arguments()
        {
            TestNullArgument("wellKnownBinary", () => DbGeometry.FromBinary(null));
            TestNullArgument("wellKnownBinary", () => DbGeometry.FromBinary(null, 1));
            TestNullArgument("lineWellKnownBinary", () => DbGeometry.LineFromBinary(null, 1));
            TestNullArgument("pointWellKnownBinary", () => DbGeometry.PointFromBinary(null, 1));
            TestNullArgument("polygonWellKnownBinary", () => DbGeometry.PolygonFromBinary(null, 1));
            TestNullArgument("multiLineWellKnownBinary", () => DbGeometry.MultiLineFromBinary(null, 1));
            TestNullArgument("multiPointWellKnownBinary", () => DbGeometry.MultiPointFromBinary(null, 1));
            TestNullArgument("multiPolygonWellKnownBinary", () => DbGeometry.MultiPolygonFromBinary(null, 1));
            TestNullArgument("geometryCollectionWellKnownBinary", () => DbGeometry.GeometryCollectionFromBinary(null, 1));

            TestNullArgument("geometryMarkup", () => DbGeometry.FromGml(null));
            TestNullArgument("geometryMarkup", () => DbGeometry.FromGml(null, 1));

            TestNullArgument("wellKnownText", () => DbGeometry.FromText(null));
            TestNullArgument("wellKnownText", () => DbGeometry.FromText(null, 1));
            TestNullArgument("lineWellKnownText", () => DbGeometry.LineFromText(null, 1));
            TestNullArgument("pointWellKnownText", () => DbGeometry.PointFromText(null, 1));
            TestNullArgument("polygonWellKnownText", () => DbGeometry.PolygonFromText(null, 1));
            TestNullArgument("multiLineWellKnownText", () => DbGeometry.MultiLineFromText(null, 1));
            TestNullArgument("multiPointWellKnownText", () => DbGeometry.MultiPointFromText(null, 1));
            TestNullArgument("multiPolygonWellKnownText", () => DbGeometry.MultiPolygonFromText(null, 1));
            TestNullArgument("geometryCollectionWellKnownText", () => DbGeometry.GeometryCollectionFromText(null, 1));

            TestNullArgument("other", s => s.SpatialEquals(null));
            TestNullArgument("other", s => s.Disjoint(null));
            TestNullArgument("other", s => s.Intersects(null));
            TestNullArgument("other", s => s.Touches(null));
            TestNullArgument("other", s => s.Crosses(null));
            TestNullArgument("other", s => s.Within(null));
            TestNullArgument("other", s => s.Contains(null));
            TestNullArgument("other", s => s.Overlaps(null));
            TestNullArgument("other", s => s.Relate(null, "Foo"));
            TestNullArgument("matrix", s => s.Relate(new DbGeometry(), null));

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

        private void TestNullArgument(string parameterName, Action<DbGeometry> test)
        {
            Assert.Equal(parameterName, Assert.Throws<ArgumentNullException>(() => test(new DbGeometry())).ParamName);
        }

        [Fact]
        public void Properties_correctly_delegate_to_spatial_provider()
        {
            PropertyDelegationHelper(g => g.Area, s => s.GetArea(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Boundary, s => s.GetBoundary(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Centroid, s => s.GetCentroid(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.ConvexHull, s => s.GetConvexHull(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.CoordinateSystemId, s => s.GetCoordinateSystemId(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Dimension, s => s.GetDimension(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.ElementCount, s => s.GetElementCount(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Elevation, s => s.GetElevation(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.EndPoint, s => s.GetEndPoint(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Envelope, s => s.GetEnvelope(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.ExteriorRing, s => s.GetExteriorRing(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.InteriorRingCount, s => s.GetInteriorRingCount(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.IsClosed, s => s.GetIsClosed(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.IsEmpty, s => s.GetIsEmpty(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.IsRing, s => s.GetIsRing(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.IsSimple, s => s.GetIsSimple(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.IsValid, s => s.GetIsValid(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Length, s => s.GetLength(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.Measure, s => s.GetMeasure(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.PointCount, s => s.GetPointCount(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.PointOnSurface, s => s.GetPointOnSurface(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.SpatialTypeName, s => s.GetSpatialTypeName(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.StartPoint, s => s.GetStartPoint(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.WellKnownValue, s => s.CreateWellKnownValue(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.XCoordinate, s => s.GetXCoordinate(It.IsAny<DbGeometry>()));
            PropertyDelegationHelper(g => g.YCoordinate, s => s.GetYCoordinate(It.IsAny<DbGeometry>()));
        }

        private void PropertyDelegationHelper(
            Func<DbGeometry, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geometry = new DbGeometry(spatialProviderMock.Object, DbGeometry.FromText("POINT(1 1)"));
            var result = operation(geometry);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Unary_methods_correctly_delegate_to_spatial_provider()
        {
            UnaryMethodDelegationHelper(g => g.AsBinary(), s => s.AsBinary(It.IsAny<DbGeometry>()));
            UnaryMethodDelegationHelper(g => g.AsGml(), s => s.AsGml(It.IsAny<DbGeometry>()));
            UnaryMethodDelegationHelper(g => g.AsText(), s => s.AsText(It.IsAny<DbGeometry>()));
            UnaryMethodDelegationHelper(g => g.AsTextIncludingElevationAndMeasure(), s => s.AsTextIncludingElevationAndMeasure(It.IsAny<DbGeometry>()));
        }

        private void UnaryMethodDelegationHelper(
            Func<DbGeometry, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geometry = new DbGeometry(spatialProviderMock.Object, DbGeometry.FromText("POINT(1 1)"));
            var result = operation(geometry);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Binary_methods_correctly_delegate_to_spatial_provider()
        {
            BinaryMethodDelegationHelper((g1, g2) => g1.Contains(g2), s => s.Contains(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Crosses(g2), s => s.Crosses(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Difference(g2), s => s.Difference(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Disjoint(g2), s => s.Disjoint(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Distance(g2), s => s.Distance(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Intersection(g2), s => s.Intersection(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Intersects(g2), s => s.Intersects(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Overlaps(g2), s => s.Overlaps(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.SpatialEquals(g2), s => s.SpatialEquals(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.SymmetricDifference(g2), s => s.SymmetricDifference(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Touches(g2), s => s.Touches(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Union(g2), s => s.Union(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
            BinaryMethodDelegationHelper((g1, g2) => g1.Within(g2), s => s.Within(It.IsAny<DbGeometry>(), It.IsAny<DbGeometry>()));
        }

        private void BinaryMethodDelegationHelper(
            Func<DbGeometry, DbGeometry, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geometry1 = new DbGeometry(spatialProviderMock.Object, DbGeometry.FromText("POINT(1 1)"));
            var geometry2 = new DbGeometry(spatialProviderMock.Object, DbGeometry.FromText("POINT(2 2)"));
            var result = operation(geometry1, geometry2);

            spatialProviderMock.Verify(verification, Times.Once());
        }

        [Fact]
        public void Methods_with_integer_arguments_correctly_delegate_to_spatial_provider()
        {
            MethodWithIntegertArgumentDelegationHelper((g, i) => g.ElementAt(i), s => s.ElementAt(It.IsAny<DbGeometry>(), It.IsAny<int>()));
            MethodWithIntegertArgumentDelegationHelper((g, i) => g.InteriorRingAt(i), s => s.InteriorRingAt(It.IsAny<DbGeometry>(), It.IsAny<int>()));
            MethodWithIntegertArgumentDelegationHelper((g, i) => g.PointAt(i), s => s.PointAt(It.IsAny<DbGeometry>(), It.IsAny<int>()));
        }

        private void MethodWithIntegertArgumentDelegationHelper(
            Func<DbGeometry, int, object> operation,
            Expression<Action<DbSpatialServices>> verification)
        {
            var spatialProviderMock = new Mock<DbSpatialServices>();
            var geometry = new DbGeometry(spatialProviderMock.Object, DbGeometry.FromText("POINT(1 1)"));
            var result = operation(geometry, 1);

            spatialProviderMock.Verify(verification, Times.Once());
        }
    }
}
