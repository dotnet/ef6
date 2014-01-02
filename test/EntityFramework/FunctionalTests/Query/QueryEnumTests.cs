// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class QueryEnumTests : FunctionalTestBase
    {
        private static readonly string csdl =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""MessageModel"">
  <EntityContainer Name=""MessageContainer"">
    <EntitySet Name=""MessageSet"" EntityType=""MessageModel.Message"" />
  </EntityContainer>
  <EntityType Name=""Message"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
    <Property Name=""MessageType"" Type=""MessageModel.MessageType"" Nullable=""false""/>
  </EntityType>
  <EnumType Name=""MessageType"" IsFlags=""false"">
    <Member Name=""Express"" />
    <Member Name=""Priority"" />
    <Member Name=""Ground"" />
  </EnumType>
</Schema>";

        private static readonly string ssdl =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""MessageStore"" Alias=""Self"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <EntityContainer Name=""MessageContainer_Store"">
    <EntitySet Name=""MessageSet"" EntityType=""Self.Message""  Schema=""dbo"" Table=""Message"" />
  </EntityContainer>
  <EntityType Name=""Message"">
    <Key>
      <PropertyRef Name=""Id""/>
    </Key>
    <Property Name=""Id"" Type=""int""  Nullable=""false""/>
    <Property Name=""MessageType"" Type=""int"" Nullable=""false""/>
  </EntityType>
</Schema>";

        private static readonly string msl =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"" Space=""C-S"">
  <EntityContainerMapping CdmEntityContainer=""MessageContainer"" StorageEntityContainer=""MessageContainer_Store"">
    <EntitySetMapping Name=""MessageSet"">
      <EntityTypeMapping TypeName=""MessageModel.Message"">
        <MappingFragment StoreEntitySet=""MessageSet"">
          <ScalarProperty Name=""Id"" ColumnName=""Id"" />
          <ScalarProperty Name=""MessageType"" ColumnName=""MessageType"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
  </EntityContainerMapping>
</Mapping>";

        private static readonly MetadataWorkspace workspace = QueryTestHelpers.CreateMetadataWorkspace(csdl, ssdl, msl);

        [Fact]
        public void Simple_scan_with_Enum()
        {
            var entitySet = workspace.GetEntityContainer("MessageContainer", DataSpace.CSpace).GetEntitySetByName("MessageSet", false);

            var query = entitySet.Scan();
            var expectedSql = "SELECT [Extent1].[Id] AS [Id], [Extent1].[MessageType] AS [MessageType]FROM [dbo].[Message] AS [Extent1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Scan_with_casting_Enum_to_integer()
        {
            var entitySet = workspace.GetEntityContainer("MessageContainer", DataSpace.CSpace).GetEntitySetByName("MessageSet", false);

            var query = entitySet.Scan()
                .Where(
                    c =>
                    c.Property("Id").Equal(
                        c.Property("MessageType").CastTo(
                            TypeUsage.CreateDefaultTypeUsage(workspace.GetPrimitiveTypes(DataSpace.CSpace).Single(t => t.Name == "Int32")))));

            var expectedSql =
                "SELECT [Extent1].[Id] AS [Id], [Extent1].[MessageType] AS [MessageType] FROM [dbo].[Message] AS [Extent1] WHERE [Extent1].[Id] =  CAST( [Extent1].[MessageType] AS int)";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Constant_integer_based_Enum_in_where_clause()
        {
            var entitySet = workspace.GetEntityContainer("MessageContainer", DataSpace.CSpace).GetEntitySetByName("MessageSet", false);

            var query = entitySet.Scan()
                .Where(c => c.Property("MessageType").Equal(c.Property("MessageType").ResultType.Constant(-5)));

            var expectedSql =
                "SELECT [Extent1].[Id] AS [Id], [Extent1].[MessageType] AS [MessageType] FROM [dbo].[Message] AS [Extent1] WHERE [Extent1].[MessageType] = -5";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        public enum MessageType
        {
            Express,
            Priority,
            Ground
        };

        [Fact]
        public void Constant_Enum_value_in_where_clause()
        {
            var entitySet = workspace.GetEntityContainer("MessageContainer", DataSpace.CSpace).GetEntitySetByName("MessageSet", false);

            var query = entitySet.Scan()
                .Where(c => c.Property("MessageType").Equal(c.Property("MessageType").ResultType.Constant(MessageType.Express)));

            var expectedSql =
                "SELECT [Extent1].[Id] AS [Id], [Extent1].[MessageType] AS [MessageType] FROM [dbo].[Message] AS [Extent1] WHERE [Extent1].[MessageType] = 0";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Null_Enum_value_in_where_clause()
        {
            var entitySet = workspace.GetEntityContainer("MessageContainer", DataSpace.CSpace).GetEntitySetByName("MessageSet", false);

            var query = entitySet.Scan()
                .Where(c => c.Property("MessageType").Equal(c.Property("MessageType").ResultType.Null()));

            var expectedSql =
                "SELECT [Extent1].[Id] AS [Id], [Extent1].[MessageType] AS [MessageType] FROM [dbo].[Message] AS [Extent1] WHERE [Extent1].[MessageType] = (CAST(NULLASint))";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }
    }
}
