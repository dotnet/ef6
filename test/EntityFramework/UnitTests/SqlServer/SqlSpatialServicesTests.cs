// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Moq;
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
            var nullSqlGeography = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().SqlGeographyType
                                                               .GetProperty(
                                                                   "Null",
                                                                   BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            var convertedDbGeography = SqlSpatialServices.Instance.GeographyFromProviderValue(nullSqlGeography.GetValue(null, null));

            Assert.Same(null, convertedDbGeography);
        }

        [Fact]
        public void GeographyFromProviderValue_returns_DbGeography_for_non_null_value()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var sqlGeography = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().ConvertToSqlTypesGeography(dbGeography);

            var convertedDbGeography = SqlSpatialServices.Instance.GeographyFromProviderValue(sqlGeography);

            Assert.Equal(dbGeography.ProviderValue, convertedDbGeography.ProviderValue);
            Assert.Equal(dbGeography.CoordinateSystemId, convertedDbGeography.CoordinateSystemId);
        }

        [Fact]
        public void GeometryFromProviderValue_returns_null_for_null_value()
        {
            var nullSqlGeometry = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().SqlGeometryType
                                                              .GetProperty(
                                                                  "Null",
                                                                  BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            var convertedDbGeometry = SqlSpatialServices.Instance.GeometryFromProviderValue(nullSqlGeometry.GetValue(null, null));

            Assert.Same(null, convertedDbGeometry);
        }

        [Fact]
        public void GeometryFromProviderValue_returns_DbGeometry_for_non_null_value()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var sqlGeometry = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().ConvertToSqlTypesGeometry(dbGeometry);

            var convertedDbGeometry = SqlSpatialServices.Instance.GeometryFromProviderValue(sqlGeometry);

            Assert.Equal(dbGeometry.ProviderValue, convertedDbGeometry.ProviderValue);
            Assert.Equal(dbGeometry.CoordinateSystemId, convertedDbGeometry.CoordinateSystemId);
        }

        [Fact]
        public void SqlSpatialServices_Singleton_uses_SQL_2008_types_on_dev_machine()
        {
            Assert.Equal(
                "Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                SqlSpatialServices.Instance.GeometryFromText("POINT (90 50)").ProviderValue.GetType().AssemblyQualifiedName);
        }

        [Fact]
        public void SqlSpatialServices_does_not_throw_until_attempt_is_made_to_use_missing_SQL_types_assembly()
        {
            var services = new SqlSpatialServices(new SqlTypesAssemblyLoader(new[] { "SomeMissingAssembly" }));

            Assert.Equal(
                Strings.SqlProvider_SqlTypesAssemblyNotFound,
                Assert.Throws<InvalidOperationException>(
                    () => services.GeometryFromText("POINT (90 50)")).Message);
        }

        [Fact]
        public void NativeTypesAvailable_returns_true_on_dev_machine_with_SQL_native_types_installed()
        {
            Assert.True(SqlSpatialServices.Instance.NativeTypesAvailable);
        }

        [Fact]
        public void NativeTypesAvailable_returns_true_if_loader_finds_native_types()
        {
            var mockLoader = new Mock<SqlTypesAssemblyLoader>(Enumerable.Empty<string>());
            mockLoader.Setup(m => m.TryGetSqlTypesAssembly()).Returns(new Mock<SqlTypesAssembly>().Object);

            Assert.True(new SqlSpatialServices(mockLoader.Object).NativeTypesAvailable);
        }

        [Fact]
        public void NativeTypesAvailable_returns_false_if_loader_does_not_find_native_types()
        {
            Assert.False(new SqlSpatialServices(new Mock<SqlTypesAssemblyLoader>(Enumerable.Empty<string>()).Object).NativeTypesAvailable);
        }

        [Fact]
        public void SqlTypes_returns_default_SQL_types_assembly_on_dev_machine()
        {
            Assert.Same(SqlSpatialServices.Instance.SqlTypes, SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly());
        }

        [Fact]
        public void SqlTypes_returns_assembly_from_loader()
        {
            var mockLoader = new Mock<SqlTypesAssemblyLoader>(Enumerable.Empty<string>());
            var sqlTypesAssembly = new Mock<SqlTypesAssembly>().Object;
            mockLoader.Setup(m => m.GetSqlTypesAssembly()).Returns(sqlTypesAssembly);

            Assert.Same(sqlTypesAssembly, new SqlSpatialServices(mockLoader.Object).SqlTypes);
        }

        [Fact]
        public void SqlSpatialServices_can_be_serialized()
        {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, SqlSpatialServices.Instance);

                stream.Seek(0, SeekOrigin.Begin);

                var deserialized = (SqlSpatialServices)formatter.Deserialize(stream);

                var geometry = deserialized.GeometryFromText("POINT (90 50)");

                Assert.Equal(90, geometry.XCoordinate);
                Assert.Equal(50, geometry.YCoordinate);
                Assert.Equal(
                    "Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                    geometry.ProviderValue.GetType().AssemblyQualifiedName);
            }
        }
    }
}
