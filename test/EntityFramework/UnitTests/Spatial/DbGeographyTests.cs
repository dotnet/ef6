namespace System.Data.Entity.Spatial
{
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
    }
}
