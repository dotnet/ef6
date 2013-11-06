<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="OrderSet" EntityType="Model1.Store.OrderSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProductSet" EntityType="Model1.Store.ProductSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="DiscontinuedItemSet" EntityType="Model1.Store.DiscontinuedItemSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProductSet_DiscontinuedProduct" EntityType="Model1.Store.ProductSet_DiscontinuedProduct" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Entity1Entity2Set" EntityType="Model1.Store.Entity1Entity2" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_Entity1Entity2Set_Entity1" Association="Model1.Store.FK_Entity1Entity2_Entity1">
      <End Role="Entity1" EntitySet="OrderSet" />
      <End Role="Entity1Entity2" EntitySet="Entity1Entity2Set" />
    </AssociationSet>
    <AssociationSet Name="FK_Entity1Entity2Set_Entity2" Association="Model1.Store.FK_Entity1Entity2_Entity2">
      <End Role="Entity2" EntitySet="ProductSet" />
      <End Role="Entity1Entity2" EntitySet="Entity1Entity2Set" />
    </AssociationSet>
    <AssociationSet Name="DiscontinuedProductDiscontinuedItems" Association="Model1.Store.DiscontinuedProductDiscontinuedItem">
      <End Role="DiscontinuedProduct" EntitySet="ProductSet_DiscontinuedProduct" />
      <End Role="DiscontinuedItem" EntitySet="DiscontinuedItemSet" />
    </AssociationSet>
    <AssociationSet Name="FK_DiscontinuedProduct_inherits_Product" Association="Model1.Store.FK_DiscontinuedProduct_inherits_Product">
      <End Role="Product" EntitySet="ProductSet" />
      <End Role="DiscontinuedProduct" EntitySet="ProductSet_DiscontinuedProduct" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="OrderSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Date" Type="datetime" Nullable="true" />
  </EntityType>
  <EntityType Name="ProductSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="varchar(max)" Nullable="true" />
  </EntityType>
  <EntityType Name="DiscontinuedItemSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="ProductSet_DiscontinuedProduct">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Discontinued" Type="datetime" Nullable="false" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedItem_Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="Entity1Entity2">
    <Key>
      <PropertyRef Name="Entity1_Id" />
      <PropertyRef Name="Entity2_Id" />
    </Key>
    <Property Name="Entity1_Id" Type="int" Nullable="false" />
    <Property Name="Entity2_Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="DiscontinuedProductDiscontinuedItem">
    <End Role="DiscontinuedProduct" Type="Model1.Store.ProductSet_DiscontinuedProduct" Multiplicity="*" />
    <End Role="DiscontinuedItem" Type="Model1.Store.DiscontinuedItemSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="DiscontinuedItem">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="DiscontinuedProduct">
        <PropertyRef Name="DiscontinuedItem_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Entity1Entity2_Entity1">
    <End Role="Entity1" Type="Model1.Store.OrderSet" Multiplicity="1" />
    <End Role="Entity1Entity2" Type="Model1.Store.Entity1Entity2" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Entity1">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Entity1Entity2">
        <PropertyRef Name="Entity1_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Entity1Entity2_Entity2">
    <End Role="Entity1Entity2" Type="Model1.Store.Entity1Entity2" Multiplicity="*" />
    <End Role="Entity2" Type="Model1.Store.ProductSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="Entity2">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Entity1Entity2">
        <PropertyRef Name="Entity2_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_DiscontinuedProduct_inherits_Product">
    <End Role="Product" Type="Model1.Store.ProductSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="DiscontinuedProduct" Type="Model1.Store.ProductSet_DiscontinuedProduct" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Product">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="DiscontinuedProduct">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="OrderSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Order)">
        <MappingFragment StoreEntitySet="OrderSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Date" ColumnName="Date" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="ProductSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Product)">
        <MappingFragment StoreEntitySet="ProductSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(Model1.DiscontinuedProduct)">
        <MappingFragment StoreEntitySet="ProductSet_DiscontinuedProduct">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Discontinued" ColumnName="Discontinued" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="DiscontinuedItemSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.DiscontinuedItem)">
        <MappingFragment StoreEntitySet="DiscontinuedItemSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="Entity1Entity2Set" TypeName="Model1.Entity1Entity2" StoreEntitySet="Entity1Entity2Set">
      <EndProperty Name="Entity1">
        <ScalarProperty Name="Id" ColumnName="Entity1_Id" />
      </EndProperty>
      <EndProperty Name="Entity2">
        <ScalarProperty Name="Id" ColumnName="Entity2_Id" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="DiscontinuedProductDiscontinuedItems" TypeName="Model1.DiscontinuedProductDiscontinuedItem" StoreEntitySet="ProductSet_DiscontinuedProduct">
      <EndProperty Name="DiscontinuedItem">
        <ScalarProperty Name="Id" ColumnName="DiscontinuedItem_Id" />
      </EndProperty>
      <EndProperty Name="DiscontinuedProduct">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
    </AssociationSetMapping>
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

-- Creating table 'OrderSet'
CREATE TABLE [dbo].[OrderSet] (
    [Id] int  NOT NULL,
    [Date] datetime  NULL
);
GO

-- Creating table 'ProductSet'
CREATE TABLE [dbo].[ProductSet] (
    [Id] int  NOT NULL,
    [Name] varchar(max)  NULL
);
GO

-- Creating table 'DiscontinuedItemSet'
CREATE TABLE [dbo].[DiscontinuedItemSet] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'ProductSet_DiscontinuedProduct'
CREATE TABLE [dbo].[ProductSet_DiscontinuedProduct] (
    [Discontinued] datetime  NOT NULL,
    [Id] int  NOT NULL,
    [DiscontinuedItem_Id] int  NOT NULL
);
GO

-- Creating table 'Entity1Entity2Set'
CREATE TABLE [dbo].[Entity1Entity2Set] (
    [Entity1_Id] int  NOT NULL,
    [Entity2_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [PK_OrderSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProductSet'
ALTER TABLE [dbo].[ProductSet]
ADD CONSTRAINT [PK_ProductSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DiscontinuedItemSet'
ALTER TABLE [dbo].[DiscontinuedItemSet]
ADD CONSTRAINT [PK_DiscontinuedItemSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProductSet_DiscontinuedProduct'
ALTER TABLE [dbo].[ProductSet_DiscontinuedProduct]
ADD CONSTRAINT [PK_ProductSet_DiscontinuedProduct]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Entity1_Id], [Entity2_Id] in table 'Entity1Entity2Set'
ALTER TABLE [dbo].[Entity1Entity2Set]
ADD CONSTRAINT [PK_Entity1Entity2Set]
    PRIMARY KEY CLUSTERED ([Entity1_Id], [Entity2_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Entity1_Id] in table 'Entity1Entity2Set'
ALTER TABLE [dbo].[Entity1Entity2Set]
ADD CONSTRAINT [FK_Entity1Entity2_Entity1]
    FOREIGN KEY ([Entity1_Id])
    REFERENCES [dbo].[OrderSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Entity2_Id] in table 'Entity1Entity2Set'
ALTER TABLE [dbo].[Entity1Entity2Set]
ADD CONSTRAINT [FK_Entity1Entity2_Entity2]
    FOREIGN KEY ([Entity2_Id])
    REFERENCES [dbo].[ProductSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Entity1Entity2_Entity2'
CREATE INDEX [IX_FK_Entity1Entity2_Entity2]
ON [dbo].[Entity1Entity2Set]
    ([Entity2_Id]);
GO

-- Creating foreign key on [DiscontinuedItem_Id] in table 'ProductSet_DiscontinuedProduct'
ALTER TABLE [dbo].[ProductSet_DiscontinuedProduct]
ADD CONSTRAINT [FK_DiscontinuedProductDiscontinuedItem]
    FOREIGN KEY ([DiscontinuedItem_Id])
    REFERENCES [dbo].[DiscontinuedItemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscontinuedProductDiscontinuedItem'
CREATE INDEX [IX_FK_DiscontinuedProductDiscontinuedItem]
ON [dbo].[ProductSet_DiscontinuedProduct]
    ([DiscontinuedItem_Id]);
GO

-- Creating foreign key on [Id] in table 'ProductSet_DiscontinuedProduct'
ALTER TABLE [dbo].[ProductSet_DiscontinuedProduct]
ADD CONSTRAINT [FK_DiscontinuedProduct_inherits_Product]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[ProductSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
