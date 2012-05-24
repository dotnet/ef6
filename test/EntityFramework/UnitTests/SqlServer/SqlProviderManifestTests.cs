namespace System.Data.Entity.SqlServer
{
    using System.Xml;
    using Xunit;

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
