namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Reflection;
    using Xunit;

    public class SqlSpatialServicesTests
    {
        [Fact]
        public void Public_members_check_for_null_arguments()
        {
            TestNullArgument("geographyValue", s => s.GetCoordinateSystemId((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetSpatialTypeName((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetDimension((DbGeography)null));
            TestNullArgument("geographyValue", s => s.AsBinary((DbGeography)null));
            TestNullArgument("geographyValue", s => s.AsGml((DbGeography)null));
            TestNullArgument("geographyValue", s => s.AsText((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetIsEmpty((DbGeography)null));
            TestNullArgument("geographyValue", s => s.SpatialEquals(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Disjoint(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Intersects(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Buffer((DbGeography)null, 0.0));
            TestNullArgument("geographyValue", s => s.Distance(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Intersection(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Union(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.Difference(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.SymmetricDifference(null, new DbGeography()));
            TestNullArgument("geographyValue", s => s.GetElementCount((DbGeography)null));
            TestNullArgument("geographyValue", s => s.ElementAt((DbGeography)null, 1));
            TestNullArgument("geographyValue", s => s.GetLatitude(null));
            TestNullArgument("geographyValue", s => s.GetLongitude(null));
            TestNullArgument("geographyValue", s => s.GetElevation((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetMeasure((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetLength((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetStartPoint((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetEndPoint((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetIsClosed((DbGeography)null));
            TestNullArgument("geographyValue", s => s.GetPointCount((DbGeography)null));
            TestNullArgument("geographyValue", s => s.PointAt((DbGeography)null, 1));
            TestNullArgument("geographyValue", s => s.GetArea((DbGeography)null));

            TestNullArgument("geometryValue", s => s.GetCoordinateSystemId((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetSpatialTypeName((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetDimension((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetEnvelope(null));
            TestNullArgument("geometryValue", s => s.AsBinary((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.AsGml((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.AsText((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetIsEmpty((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetIsSimple(null));
            TestNullArgument("geometryValue", s => s.GetBoundary(null));
            TestNullArgument("geometryValue", s => s.GetIsValid(null));
            TestNullArgument("geometryValue", s => s.SpatialEquals(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Disjoint(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Intersects(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Touches(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Crosses(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Within(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Contains(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Overlaps(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Relate(null, new DbGeometry(), "Foo"));
            TestNullArgument("geometryValue", s => s.Buffer((DbGeometry)null, 0.0));
            TestNullArgument("geometryValue", s => s.Distance(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.GetConvexHull(null));
            TestNullArgument("geometryValue", s => s.Intersection(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Union(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.Difference(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.SymmetricDifference(null, new DbGeometry()));
            TestNullArgument("geometryValue", s => s.GetElementCount((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.ElementAt((DbGeometry)null, 1));
            TestNullArgument("geometryValue", s => s.GetXCoordinate(null));
            TestNullArgument("geometryValue", s => s.GetYCoordinate(null));
            TestNullArgument("geometryValue", s => s.GetElevation((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetMeasure((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetLength((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetStartPoint((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetEndPoint((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetIsClosed((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetIsRing(null));
            TestNullArgument("geometryValue", s => s.GetPointCount((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.PointAt((DbGeometry)null, 1));
            TestNullArgument("geometryValue", s => s.GetArea((DbGeometry)null));
            TestNullArgument("geometryValue", s => s.GetCentroid(null));
            TestNullArgument("geometryValue", s => s.GetPointOnSurface(null));
            TestNullArgument("geometryValue", s => s.GetExteriorRing(null));
            TestNullArgument("geometryValue", s => s.GetInteriorRingCount(null));
            TestNullArgument("geometryValue", s => s.InteriorRingAt(null, 1));

            TestNullArgument("wellKnownValue", s => s.CreateProviderValue((DbGeographyWellKnownValue)null));
            TestNullArgument("wellKnownValue", s => s.CreateProviderValue((DbGeometryWellKnownValue)null));
            TestNullArgument("geographyValue", s => s.CreateWellKnownValue((DbGeography)null));
            TestNullArgument("geometryValue", s => s.CreateWellKnownValue((DbGeometry)null));
            TestNullArgument("providerValue", s => s.GeographyFromProviderValue(null));
            TestNullArgument("providerValue", s => s.GeometryFromProviderValue(null));
        }

        private void TestNullArgument(string parameterName, Action<SqlSpatialServices> test)
        {
            Assert.Equal(parameterName, Assert.Throws<ArgumentNullException>(() => test(new SqlSpatialServices())).ParamName);
        }

        [Fact]
        public void GeographyFromProviderValue_returns_null_for_null_value()
        {
            var nullSqlGeography = SqlProviderServices.GetSqlTypesAssembly().SqlGeographyType
                .GetProperty("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            var convertedDbGeography = SqlSpatialServices.Instance.GeographyFromProviderValue(nullSqlGeography.GetValue(null));

            Assert.Same(null, convertedDbGeography);
        }

        [Fact]
        public void GeographyFromProviderValue_returns_DbGeography_for_non_null_value()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var sqlGeography = SqlProviderServices.GetSqlTypesAssembly().ConvertToSqlTypesGeography(dbGeography);

            var convertedDbGeography = SqlSpatialServices.Instance.GeographyFromProviderValue(sqlGeography);

            Assert.Equal(dbGeography.ProviderValue, convertedDbGeography.ProviderValue);
            Assert.Equal(dbGeography.CoordinateSystemId, convertedDbGeography.CoordinateSystemId);
        }
        
        [Fact]
        public void GeometryFromProviderValue_returns_null_for_null_value()
        {
            var nullSqlGeometry = SqlProviderServices.GetSqlTypesAssembly().SqlGeometryType
                .GetProperty("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            var convertedDbGeometry = SqlSpatialServices.Instance.GeometryFromProviderValue(nullSqlGeometry.GetValue(null));

            Assert.Same(null, convertedDbGeometry);
        }

        [Fact]
        public void GeometryFromProviderValue_returns_DbGeometry_for_non_null_value()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var sqlGeometry = SqlProviderServices.GetSqlTypesAssembly().ConvertToSqlTypesGeometry(dbGeometry);

            var convertedDbGeometry = SqlSpatialServices.Instance.GeometryFromProviderValue(sqlGeometry);

            Assert.Equal(dbGeometry.ProviderValue, convertedDbGeometry.ProviderValue);
            Assert.Equal(dbGeometry.CoordinateSystemId, convertedDbGeometry.CoordinateSystemId);
        }
    }
}