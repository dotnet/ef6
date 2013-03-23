// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
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

        [Fact]
        public void GetStoreType_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SqlProviderManifest("2008").GetStoreType(null));
        }

        [Fact]
        public void GetEdmType_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SqlProviderManifest("2008").GetEdmType(null));
        }

        [Fact]
        public void EscapeLikeArgument_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SqlProviderManifest("2008").EscapeLikeArgument(null));
        }

        [Fact]
        public void GetStoreTypes_includes_SQL_Server_2008_types_on_SQL_Server_2008()
        {
            GetStoreTypesTest("2008", expectTypes: true);
        }

        [Fact]
        public void GetStoreTypes_includes_SQL_Server_2008_types_on_SQL_Server_2012()
        {
            GetStoreTypesTest("2012", expectTypes: true);
        }

        [Fact]
        public void GetStoreTypes_includes_SQL_Server_2008_types_on_SQL_Azure()
        {
            GetStoreTypesTest("2012.Azure", expectTypes: true);
        }

        [Fact]
        public void GetStoreTypes_excludes_SQL_Server_2008_types_on_SQL_Server_2005()
        {
            GetStoreTypesTest("2005", expectTypes: false);
        }

        [Fact]
        public void GetStoreTypes_excludes_SQL_Server_2008_types_on_SQL_Server_2000()
        {
            GetStoreTypesTest("2000", expectTypes: false);
        }

        private static void GetStoreTypesTest(string manifestToken, bool expectTypes)
        {
            var types = new SqlProviderManifest(manifestToken).GetStoreTypes();

            var expectedCount = expectTypes ? 1 : 0;
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "time"));
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "date"));
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "datetime2"));
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "datetimeoffset"));
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "geography"));
            Assert.Equal(expectedCount, types.Count(t => t.Name.ToLowerInvariant() == "geometry"));
        }

        [Fact]
        public void GetStoreTypes_only_includes_XML_type_on_SQL_Server_2005_and_later()
        {
            Assert.Equal(0, new SqlProviderManifest("2000").GetStoreTypes().Count(t => t.Name.ToLowerInvariant() == "xml"));
            Assert.Equal(1, new SqlProviderManifest("2005").GetStoreTypes().Count(t => t.Name.ToLowerInvariant() == "xml"));
            Assert.Equal(1, new SqlProviderManifest("2008").GetStoreTypes().Count(t => t.Name.ToLowerInvariant() == "xml"));
            Assert.Equal(1, new SqlProviderManifest("2012").GetStoreTypes().Count(t => t.Name.ToLowerInvariant() == "xml"));
            Assert.Equal(1, new SqlProviderManifest("2012.Azure").GetStoreTypes().Count(t => t.Name.ToLowerInvariant() == "xml"));
        }

        private static readonly string[] _functionsOnlyIn2008AndUp =
            {
                "COUNT collection[Edm.DateTimeOffset(Nullable=True,DefaultValue=,Precision=)]",
                "COUNT collection[Edm.Time(Nullable=True,DefaultValue=,Precision=)]",
                "COUNT_BIG collection[Edm.DateTimeOffset(Nullable=True,DefaultValue=,Precision=)]",
                "COUNT_BIG collection[Edm.Time(Nullable=True,DefaultValue=,Precision=)]",
                "MAX collection[Edm.Time(Nullable=True,DefaultValue=,Precision=)]",
                "MAX collection[Edm.DateTimeOffset(Nullable=True,DefaultValue=,Precision=)]",
                "MIN collection[Edm.Time(Nullable=True,DefaultValue=,Precision=)]",
                "MIN collection[Edm.DateTimeOffset(Nullable=True,DefaultValue=,Precision=)]",
                "DATEADD String Double Time",
                "DATEADD String Double DateTimeOffset",
                "DATEDIFF String DateTimeOffset DateTimeOffset",
                "DATEDIFF String Time Time",
                "DATEDIFF String String DateTimeOffset",
                "DATEDIFF String String Time",
                "DATEDIFF String Time String",
                "DATEDIFF String DateTimeOffset String",
                "DATEDIFF String Time DateTime",
                "DATEDIFF String Time DateTimeOffset",
                "DATEDIFF String DateTime Time",
                "DATEDIFF String DateTimeOffset Time",
                "DATEDIFF String DateTime DateTimeOffset",
                "DATEDIFF String DateTimeOffset DateTime",
                "DATENAME String Time",
                "DATENAME String DateTimeOffset",
                "DATEPART String DateTimeOffset",
                "DATEPART String Time",
                "DAY DateTimeOffset",
                "SYSDATETIME",
                "SYSUTCDATETIME",
                "SYSDATETIMEOFFSET",
                "MONTH DateTimeOffset",
                "YEAR DateTimeOffset",
                "DATALENGTH Time",
                "DATALENGTH DateTimeOffset",
                "CHECKSUM Time",
                "CHECKSUM DateTimeOffset",
                "CHECKSUM Time Time",
                "CHECKSUM DateTimeOffset DateTimeOffset",
                "CHECKSUM DateTimeOffset DateTimeOffset DateTimeOffset",
                "CHECKSUM Time Time Time",
                "POINTGEOGRAPHY Double Double Int32",
                "ASTEXTZM Geography",
                "BUFFERWITHTOLERANCE Geography Double Double Boolean",
                "ENVELOPEANGLE Geography",
                "ENVELOPECENTER Geography",
                "FILTER Geography Geography",
                "INSTANCEOF Geography String",
                "NUMRINGS Geography",
                "REDUCE Geography Double",
                "RINGN Geography Int32",
                "POINTGEOMETRY Double Double Int32",
                "ASTEXTZM Geometry",
                "BUFFERWITHTOLERANCE Geometry Double Double Boolean",
                "INSTANCEOF Geometry String",
                "FILTER Geometry Geometry",
                "MAKEVALID Geometry",
                "REDUCE Geometry Double",
            };

        private static readonly string[] _functionsOnlyIn2005AndUp =
            {
                "COUNT collection[Edm.Guid(Nullable=True,DefaultValue=)]",
                "COUNT_BIG collection[Edm.Guid(Nullable=True,DefaultValue=)]",
                "CHARINDEX String String Int64",
                "CHARINDEX Binary Binary Int64",
            };

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2008_functions_on_SQL_Server_2008()
        {
            FunctionsIncludedTest("2008", _functionsOnlyIn2008AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2008_functions_on_SQL_Server_2012()
        {
            FunctionsIncludedTest("2012", _functionsOnlyIn2008AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2008_functions_on_SQL_Azure()
        {
            FunctionsIncludedTest("2012.Azure", _functionsOnlyIn2008AndUp);
        }

        [Fact]
        public void GetStoreFunctions_does_not_include_SQL_Server_2008_functions_on_SQL_Server_2005()
        {
            FunctionsNotIncludedTest("2005", _functionsOnlyIn2008AndUp);
        }

        [Fact]
        public void GetStoreFunctions_does_not_include_SQL_Server_2008_functions_on_SQL_Server_2000()
        {
            FunctionsNotIncludedTest("2000", _functionsOnlyIn2008AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2005_functions_on_SQL_Server_2005()
        {
            FunctionsIncludedTest("2005", _functionsOnlyIn2005AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2005_functions_on_SQL_Server_2008()
        {
            FunctionsIncludedTest("2008", _functionsOnlyIn2005AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2005_functions_on_SQL_Server_2012()
        {
            FunctionsIncludedTest("2012", _functionsOnlyIn2005AndUp);
        }

        [Fact]
        public void GetStoreFunctions_includes_SQL_Server_2005_functions_on_SQL_Azure()
        {
            FunctionsIncludedTest("2012.Azure", _functionsOnlyIn2005AndUp);
        }

        [Fact]
        public void GetStoreFunctions_does_not_include_SQL_Server_2005_functions_on_SQL_Server_2000()
        {
            FunctionsNotIncludedTest("2000", _functionsOnlyIn2005AndUp);
        }

        private static void FunctionsIncludedTest(string manifestToken, string[] expectedFunctions)
        {
            var actualFunctions = new SqlProviderManifest(manifestToken).GetStoreFunctions().Select(FunctionString).ToList();
            foreach (var function in expectedFunctions)
            {
                Assert.Contains(function, actualFunctions);
            }
        }

        private static void FunctionsNotIncludedTest(string manifestToken, string[] notExpectedFunctions)
        {
            var actualFunctions = new SqlProviderManifest(manifestToken).GetStoreFunctions().Select(FunctionString).ToList();
            foreach (var function in actualFunctions)
            {
                Assert.DoesNotContain(function, notExpectedFunctions);
            }
        }

        private static string FunctionString(EdmFunction function)
        {
            return function.Name + function.Parameters.Select(p => p.TypeUsage.EdmType.Name).Aggregate("", (a, n) => a + " " + n);
        }
    }
}
