<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2009/02/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="Entity1Set" EntityType="Model1.Store.Entity1Set" store:Type="Tables" Schema="dbo" />
  </EntityContainer>
  <EntityType Name="Entity1Set">
    <Key>
      <PropertyRef Name="IdIdentity" />
    </Key>
    <Property Name="IdIdentity" Type="int" StoreGeneratedPattern="Identity" Nullable="false" />
    <Property Name="StringNone" Type="nvarchar(max)" Nullable="false" />
    <Property Name="BigIntComputed" Type="bigint" StoreGeneratedPattern="Computed" Nullable="false" />
    <Property Name="SmallIntNone" Type="smallint" Nullable="false" />
    <Property Name="DecimalComputed" Type="decimal" StoreGeneratedPattern="Computed" Nullable="false" Precision="29" Scale="29" />
    <Property Name="Double" Type="float" Nullable="false" />
    <Property Name="Single" Type="real" Nullable="false" />
    <Property Name="Byte" Type="tinyint" Nullable="false" />
    <Property Name="DecimalIdentity" Type="decimal" StoreGeneratedPattern="Identity" Nullable="false" Precision="29" Scale="29" />
    <Property Name="DecimalNone" Type="decimal" Nullable="false" Precision="29" Scale="29" />
    <Property Name="StringIdentity" Type="nvarchar(max)" StoreGeneratedPattern="Identity" Nullable="false" />
    <Property Name="StringComputed" Type="nvarchar(max)" StoreGeneratedPattern="Computed" Nullable="false" />
  </EntityType>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2008/09/mapping/cs">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container12">
    <EntitySetMapping Name="Entity1Set">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity1)">
        <MappingFragment StoreEntitySet="Entity1Set">
          <ScalarProperty Name="IdIdentity" ColumnName="IdIdentity" />
          <ScalarProperty Name="StringNone" ColumnName="StringNone" />
          <ScalarProperty Name="BigIntComputed" ColumnName="BigIntComputed" />
          <ScalarProperty Name="SmallIntNone" ColumnName="SmallIntNone" />
          <ScalarProperty Name="DecimalComputed" ColumnName="DecimalComputed" />
          <ScalarProperty Name="Double" ColumnName="Double" />
          <ScalarProperty Name="Single" ColumnName="Single" />
          <ScalarProperty Name="Byte" ColumnName="Byte" />
          <ScalarProperty Name="DecimalIdentity" ColumnName="DecimalIdentity" />
          <ScalarProperty Name="DecimalNone" ColumnName="DecimalNone" />
          <ScalarProperty Name="StringIdentity" ColumnName="StringIdentity" />
          <ScalarProperty Name="StringComputed" ColumnName="StringComputed" />
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
    [IdIdentity] int IDENTITY(1,1) NOT NULL,
    [StringNone] nvarchar(max)  NOT NULL,
    [BigIntComputed] bigint  NOT NULL,
    [SmallIntNone] smallint  NOT NULL,
    [DecimalComputed] decimal(29,29)  NOT NULL,
    [Double] float  NOT NULL,
    [Single] real  NOT NULL,
    [Byte] tinyint  NOT NULL,
    [DecimalIdentity] decimal(29,29) IDENTITY(1,1) NOT NULL,
    [DecimalNone] decimal(29,29)  NOT NULL,
    [StringIdentity] nvarchar(max)  NOT NULL,
    [StringComputed] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [IdIdentity] in table 'Entity1Set'
ALTER TABLE [dbo].[Entity1Set]
ADD CONSTRAINT [PK_Entity1Set]
    PRIMARY KEY CLUSTERED ([IdIdentity] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
