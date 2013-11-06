<StorageAndMappings>
  <Schema Namespace="PkToPk.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="PkToPkStoreContainer">
    <EntitySet Name="DiscontinuedProductSet" EntityType="PkToPk.Store.DiscontinuedProductSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="DiscontinuedItemSet" EntityType="PkToPk.Store.DiscontinuedItemSet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="DiscontinuedProductDiscontinuedItem" Association="PkToPk.Store.DiscontinuedProductDiscontinuedItem">
      <End Role="DiscontinuedProduct" EntitySet="DiscontinuedProductSet" />
      <End Role="DiscontinuedItem" EntitySet="DiscontinuedItemSet" />
    </AssociationSet>
    <AssociationSet Name="DiscontinuedProductDiscontinuedProduct" Association="PkToPk.Store.DiscontinuedProductDiscontinuedProduct">
      <End Role="DiscontinuedProduct" EntitySet="DiscontinuedProductSet" />
      <End Role="DiscontinuedProduct1" EntitySet="DiscontinuedProductSet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="DiscontinuedProductSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedDate" Type="datetime" Nullable="true" />
    <Property Name="DiscontinuedItem_Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedProduct_1_Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="DiscontinuedItemSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="ItemName" Type="int" Nullable="true" />
  </EntityType>
  <Association Name="DiscontinuedProductDiscontinuedItem">
    <End Role="DiscontinuedProduct" Type="PkToPk.Store.DiscontinuedProductSet" Multiplicity="*" />
    <End Role="DiscontinuedItem" Type="PkToPk.Store.DiscontinuedItemSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="DiscontinuedItem">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="DiscontinuedProduct">
        <PropertyRef Name="DiscontinuedItem_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="DiscontinuedProductDiscontinuedProduct">
    <End Role="DiscontinuedProduct" Type="PkToPk.Store.DiscontinuedProductSet" Multiplicity="*" />
    <End Role="DiscontinuedProduct1" Type="PkToPk.Store.DiscontinuedProductSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="DiscontinuedProduct1">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="DiscontinuedProduct">
        <PropertyRef Name="DiscontinuedProduct_1_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="PkToPkStoreContainer" CdmEntityContainer="PkToPkContainer">
    <EntitySetMapping Name="DiscontinuedProductSet">
      <EntityTypeMapping TypeName="IsTypeOf(PkToPk.DiscontinuedProduct)">
        <MappingFragment StoreEntitySet="DiscontinuedProductSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="DiscontinuedDate" ColumnName="DiscontinuedDate" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="DiscontinuedItemSet">
      <EntityTypeMapping TypeName="IsTypeOf(PkToPk.DiscontinuedItem)">
        <MappingFragment StoreEntitySet="DiscontinuedItemSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="ItemName" ColumnName="ItemName" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="DiscontinuedProductDiscontinuedItem" TypeName="PkToPk.DiscontinuedProductDiscontinuedItem" StoreEntitySet="DiscontinuedProductSet">
      <EndProperty Name="DiscontinuedItem">
        <ScalarProperty Name="Id" ColumnName="DiscontinuedItem_Id" />
      </EndProperty>
      <EndProperty Name="DiscontinuedProduct">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="DiscontinuedProductDiscontinuedProduct" TypeName="PkToPk.DiscontinuedProductDiscontinuedProduct" StoreEntitySet="DiscontinuedProductSet">
      <EndProperty Name="DiscontinuedProduct1">
        <ScalarProperty Name="Id" ColumnName="DiscontinuedProduct_1_Id" />
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

-- Creating table 'DiscontinuedProductSet'
CREATE TABLE [dbo].[DiscontinuedProductSet] (
    [Id] int  NOT NULL,
    [DiscontinuedDate] datetime  NULL,
    [DiscontinuedItem_Id] int  NOT NULL,
    [DiscontinuedProduct_1_Id] int  NOT NULL
);
GO

-- Creating table 'DiscontinuedItemSet'
CREATE TABLE [dbo].[DiscontinuedItemSet] (
    [Id] int  NOT NULL,
    [ItemName] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'DiscontinuedProductSet'
ALTER TABLE [dbo].[DiscontinuedProductSet]
ADD CONSTRAINT [PK_DiscontinuedProductSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DiscontinuedItemSet'
ALTER TABLE [dbo].[DiscontinuedItemSet]
ADD CONSTRAINT [PK_DiscontinuedItemSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [DiscontinuedItem_Id] in table 'DiscontinuedProductSet'
ALTER TABLE [dbo].[DiscontinuedProductSet]
ADD CONSTRAINT [FK_DiscontinuedProductDiscontinuedItem]
    FOREIGN KEY ([DiscontinuedItem_Id])
    REFERENCES [dbo].[DiscontinuedItemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscontinuedProductDiscontinuedItem'
CREATE INDEX [IX_FK_DiscontinuedProductDiscontinuedItem]
ON [dbo].[DiscontinuedProductSet]
    ([DiscontinuedItem_Id]);
GO

-- Creating foreign key on [DiscontinuedProduct_1_Id] in table 'DiscontinuedProductSet'
ALTER TABLE [dbo].[DiscontinuedProductSet]
ADD CONSTRAINT [FK_DiscontinuedProductDiscontinuedProduct]
    FOREIGN KEY ([DiscontinuedProduct_1_Id])
    REFERENCES [dbo].[DiscontinuedProductSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscontinuedProductDiscontinuedProduct'
CREATE INDEX [IX_FK_DiscontinuedProductDiscontinuedProduct]
ON [dbo].[DiscontinuedProductSet]
    ([DiscontinuedProduct_1_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
