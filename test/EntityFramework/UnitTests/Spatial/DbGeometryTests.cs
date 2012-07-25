// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Spatial
{
    using Moq;
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
    }
}