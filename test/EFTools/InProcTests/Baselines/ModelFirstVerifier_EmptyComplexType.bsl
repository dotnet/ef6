<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2009/02/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="Entity1Set" EntityType="Model1.Store.Entity1Set" store:Type="Tables" Schema="dbo" />
  </EntityContainer>
  <EntityType Name="Entity1Set">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="NestedComplexWithScalar_ComplexProperty_Property" Type="nvarchar(max)" Nullable="false" />
  </EntityType>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2008/09/mapping/cs">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="Entity1Set">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity1)">
        <MappingFragment StoreEntitySet="Entity1Set">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ComplexProperty Name="NestedComplexWithScalar" TypeName="Model1.ComplexType4">
            <ComplexProperty Name="ComplexProperty" TypeName="Model1.CTWithSP">
              <ScalarProperty Name="Property" ColumnName="NestedComplexWithScalar_ComplexProperty_Property" />
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
  </EntityContainerMapping>
</Mapping></StorageAndMappings>

The generated DDL:
SET QUOTED_IDENTIFIER OFF;
GO
USE [TestDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Entity1Set'
CREATE TABLE [dbo].[Entity1Set] (
    [Id] int  NOT NULL,
    [NestedComplexWithScalar_ComplexProperty_Property] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Entity1Set'
ALTER TABLE [dbo].[Entity1Set]
ADD CONSTRAINT [PK_Entity1Set]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
