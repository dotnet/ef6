<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="Entity1Set" EntityType="Model1.Store.Entity1Set" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Entity1Set_Entity2" EntityType="Model1.Store.Entity1Set_Entity2" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_Entity2_inherits_Entity1" Association="Model1.Store.FK_Entity2_inherits_Entity1">
      <End Role="Entity1" EntitySet="Entity1Set" />
      <End Role="Entity2" EntitySet="Entity1Set_Entity2" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="Entity1Set">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="Entity1Set_Entity2">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Name" Type="int" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="FK_Entity2_inherits_Entity1">
    <End Role="Entity1" Type="Model1.Store.Entity1Set" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Entity2" Type="Model1.Store.Entity1Set_Entity2" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Entity1">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Entity2">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="Entity1Set">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity1)">
        <MappingFragment StoreEntitySet="Entity1Set">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity2)">
        <MappingFragment StoreEntitySet="Entity1Set_Entity2">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
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
    [Id] int  NOT NULL
);
GO

-- Creating table 'Entity1Set_Entity2'
CREATE TABLE [dbo].[Entity1Set_Entity2] (
    [Name] int  NULL,
    [Id] int  NOT NULL
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

-- Creating primary key on [Id] in table 'Entity1Set_Entity2'
ALTER TABLE [dbo].[Entity1Set_Entity2]
ADD CONSTRAINT [PK_Entity1Set_Entity2]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Id] in table 'Entity1Set_Entity2'
ALTER TABLE [dbo].[Entity1Set_Entity2]
ADD CONSTRAINT [FK_Entity2_inherits_Entity1]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Entity1Set]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
