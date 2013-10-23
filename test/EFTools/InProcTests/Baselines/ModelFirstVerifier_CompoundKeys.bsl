<StorageAndMappings>
  <Schema Namespace="CompoundKeys.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="CompoundKeysStoreContainer">
    <EntitySet Name="StudentSet" EntityType="CompoundKeys.Store.StudentSet" store:Type="Tables" Schema="dbo" />
  </EntityContainer>
  <EntityType Name="StudentSet">
    <Key>
      <PropertyRef Name="PersonId" />
      <PropertyRef Name="Name" />
    </Key>
    <Property Name="PersonId" Type="int" Nullable="false" />
    <Property Name="Name" Type="varchar" Nullable="false" MaxLength="50" />
    <Property Name="Address" Type="varchar(max)" Nullable="true" />
    <Property Name="Phone" Type="int" Nullable="true" />
  </EntityType>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="CompoundKeysStoreContainer" CdmEntityContainer="CompoundKeysContainer">
    <EntitySetMapping Name="StudentSet">
      <EntityTypeMapping TypeName="IsTypeOf(CompoundKeys.Student)">
        <MappingFragment StoreEntitySet="StudentSet">
          <ScalarProperty Name="PersonId" ColumnName="PersonId" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
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

-- Creating table 'StudentSet'
CREATE TABLE [dbo].[StudentSet] (
    [PersonId] int  NOT NULL,
    [Name] varchar(50)  NOT NULL,
    [Address] varchar(max)  NULL,
    [Phone] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [PersonId], [Name] in table 'StudentSet'
ALTER TABLE [dbo].[StudentSet]
ADD CONSTRAINT [PK_StudentSet]
    PRIMARY KEY CLUSTERED ([PersonId], [Name] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
