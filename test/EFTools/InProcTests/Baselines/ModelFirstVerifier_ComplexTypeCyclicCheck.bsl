<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2009/02/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="Entity2Set" EntityType="Model1.Store.Entity2Set" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Entity2Set_Entity1" EntityType="Model1.Store.Entity2Set_Entity1" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_Entity1_inherits_Entity2" Association="Model1.Store.FK_Entity1_inherits_Entity2">
      <End Role="Entity2" EntitySet="Entity2Set" />
      <End Role="Entity1" EntitySet="Entity2Set_Entity1" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="Entity2Set">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" StoreGeneratedPattern="Identity" Nullable="false" />
    <Property Name="EntityProperty_ComplexProperty_3_ComplexProperty_2_ScalarProperty_1" Type="nvarchar(max)" Nullable="false" />
    <Property Name="EntityProperty_ComplexProperty_2_ComplexProperty_2_ScalarProperty_1" Type="nvarchar(max)" Nullable="false" />
  </EntityType>
  <EntityType Name="Entity2Set_Entity1">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Property" Type="int" Nullable="false" />
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="FK_Entity1_inherits_Entity2">
    <End Role="Entity2" Type="Model1.Store.Entity2Set" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Entity1" Type="Model1.Store.Entity2Set_Entity1" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Entity2">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Entity1">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2008/09/mapping/cs">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="Entity2Set">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity2)">
        <MappingFragment StoreEntitySet="Entity2Set">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ComplexProperty Name="EntityProperty" TypeName="Model1.ComplexType3">
            <ComplexProperty Name="ComplexProperty_3" TypeName="Model1.ComplexType2">
              <ComplexProperty Name="ComplexProperty_2" TypeName="Model1.ComplexType1">
                <ScalarProperty Name="ScalarProperty_1" ColumnName="EntityProperty_ComplexProperty_3_ComplexProperty_2_ScalarProperty_1" />
              </ComplexProperty>
            </ComplexProperty>
            <ComplexProperty Name="ComplexProperty_2" TypeName="Model1.ComplexType2">
              <ComplexProperty Name="ComplexProperty_2" TypeName="Model1.ComplexType1">
                <ScalarProperty Name="ScalarProperty_1" ColumnName="EntityProperty_ComplexProperty_2_ComplexProperty_2_ScalarProperty_1" />
              </ComplexProperty>
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Entity1)">
        <MappingFragment StoreEntitySet="Entity2Set_Entity1">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Property" ColumnName="Property" />
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

-- Creating table 'Entity2Set'
CREATE TABLE [dbo].[Entity2Set] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EntityProperty_ComplexProperty_3_ComplexProperty_2_ScalarProperty_1] nvarchar(max)  NOT NULL,
    [EntityProperty_ComplexProperty_2_ComplexProperty_2_ScalarProperty_1] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Entity2Set_Entity1'
CREATE TABLE [dbo].[Entity2Set_Entity1] (
    [Property] int  NOT NULL,
    [Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Entity2Set'
ALTER TABLE [dbo].[Entity2Set]
ADD CONSTRAINT [PK_Entity2Set]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Entity2Set_Entity1'
ALTER TABLE [dbo].[Entity2Set_Entity1]
ADD CONSTRAINT [PK_Entity2Set_Entity1]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Id] in table 'Entity2Set_Entity1'
ALTER TABLE [dbo].[Entity2Set_Entity1]
ADD CONSTRAINT [FK_Entity1_inherits_Entity2]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Entity2Set]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
