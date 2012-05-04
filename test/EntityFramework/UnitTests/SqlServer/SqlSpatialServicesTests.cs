namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Xml;
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
    }

    public class SqlProviderManifestTests
    {
        [Fact]
        public void GetProviderManifest_loads_ProviderManifest_xml()
        {
            TestReadResource(
                SqlProviderManifest.GetProviderManifest(),
                "ProviderManifest",
                @"http://schemas.microsoft.com/ado/2006/04/edm/providermanifest",
                x => x.Name == "Type" && x.GetAttribute("Name") == "geography", true);
        }

        [Fact]
        public void GetStoreSchemaMapping_loads_V2_schema_mapping_xml()
        {
            TestReadResource(
                SqlProviderManifest.GetStoreSchemaMapping("StoreSchemaMapping"),
                "Mapping",
                @"urn:schemas-microsoft-com:windows:storage:mapping:CS",
                x => x.Name == "EntitySetMapping" && x.GetAttribute("Name") == "FunctionReturnTableColumns", false);
        }

        [Fact]
        public void GetStoreSchemaMapping_loads_V3_schema_mapping_xml()
        {
            TestReadResource(
                SqlProviderManifest.GetStoreSchemaMapping("StoreSchemaMappingVersion3"),
                "Mapping",
                @"urn:schemas-microsoft-com:windows:storage:mapping:CS",
                x => x.Name == "EntitySetMapping" && x.GetAttribute("Name") == "FunctionReturnTableColumns", true);
        }

        [Fact]
        public void GetStoreSchemaDescription_loads_V2_schema_xml()
        {
            // Test it's for V2
            TestReadResource(
                new SqlProviderManifest("2008").GetStoreSchemaDescription("StoreSchemaDefinition"),
                "Schema",
                @"http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                x => x.Name == "EntitySet" && x.GetAttribute("Name") == "SFunctionReturnTableColumns", false,
                x => x.Name == "Property" && x.GetAttribute("Type") == "ntext", false);
        }

        [Fact]
        public void GetStoreSchemaDescription_loads_V3_schema_xml()
        {
            // Test it's for V3
            TestReadResource(
                new SqlProviderManifest("2008").GetStoreSchemaDescription("StoreSchemaDefinitionVersion3"),
                "Schema",
                @"http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                x => x.Name == "EntitySet" && x.GetAttribute("Name") == "SFunctionReturnTableColumns", true,
                x => x.Name == "Property" && x.GetAttribute("Type") == "ntext", false);
        }

        [Fact]
        public void GetStoreSchemaDescription_loads_V2_schema_xml_for_SQL_2000()
        {
            // Test it's for V2
            TestReadResource(
                new SqlProviderManifest("2000").GetStoreSchemaDescription("StoreSchemaDefinition"),
                "Schema",
                @"http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                x => x.Name == "EntitySet" && x.GetAttribute("Name") == "SFunctionReturnTableColumns", false,
                x => x.Name == "Property" && x.GetAttribute("Type") == "ntext", true);
        }

        [Fact]
        public void GetStoreSchemaDescription_loads_V3_schema_xml_SQL_2000()
        {
            TestReadResource(
                new SqlProviderManifest("2000").GetStoreSchemaDescription("StoreSchemaDefinitionVersion3"),
                "Schema",
                @"http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                x => x.Name == "EntitySet" && x.GetAttribute("Name") == "SFunctionReturnTableColumns", true,
                x => x.Name == "Property" && x.GetAttribute("Type") == "ntext", true);
        }

        private static void TestReadResource(
            XmlReader xmlReader,
            string name,
            string namespaceUri,
            Func<XmlReader, bool> identify1,
            bool expected1,
            Func<XmlReader, bool> identify2 = null,
            bool expected2 = false)
        {
            using (xmlReader)
            {
                while (xmlReader.Read()
                       && xmlReader.NodeType != XmlNodeType.Element)
                {
                }
                Assert.Equal(name, xmlReader.Name);
                Assert.Equal(namespaceUri, xmlReader.NamespaceURI);

                var ident1 = false;
                var ident2 = false;
                while (xmlReader.Read())
                {
                    if (identify1(xmlReader))
                    {
                        ident1 = true;
                    }

                    if (identify2 != null
                        && identify2(xmlReader))
                    {
                        ident2 = true;
                    }
                }

                Assert.Equal(expected1, ident1);
                if (identify2 != null)
                {
                    Assert.Equal(expected2, ident2);
                }
            }
        }
    }
}