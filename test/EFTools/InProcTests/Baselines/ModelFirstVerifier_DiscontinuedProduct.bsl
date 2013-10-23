<StorageAndMappings>
  <Schema Namespace="DiscProductModel.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="DiscProductModelStoreContainer">
    <EntitySet Name="DiscontinuedCategorySet" EntityType="DiscProductModel.Store.DiscontinuedCategorySet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProductSet" EntityType="DiscProductModel.Store.ProductSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProductSet_DiscontinuedProduct" EntityType="DiscProductModel.Store.ProductSet_DiscontinuedProduct" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="DiscontinuedCategoryDiscontinuedProduct" Association="DiscProductModel.Store.DiscontinuedCategoryDiscontinuedProduct">
      <End Role="DiscontinuedCategory" EntitySet="DiscontinuedCategorySet" />
      <End Role="DiscontinuedProduct" EntitySet="ProductSet_DiscontinuedProduct" />
    </AssociationSet>
    <AssociationSet Name="FK_DiscontinuedProduct_inherits_Product" Association="DiscProductModel.Store.FK_DiscontinuedProduct_inherits_Product">
      <End Role="Product" EntitySet="ProductSet" />
      <End Role="DiscontinuedProduct" EntitySet="ProductSet_DiscontinuedProduct" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="DiscontinuedCategorySet">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="nvarchar(max)" Nullable="false" />
    <Property Name="Description" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="ProductSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Property" Type="int" Nullable="true" />
    <Property Name="Name" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Description" Type="nvarchar(max)" Nullable="true" />
    <Property Name="NumInStock" Type="int" Nullable="true" />
    <Property Name="Price" Type="decimal" Nullable="true" />
  </EntityType>
  <EntityType Name="ProductSet_DiscontinuedProduct">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="DateDiscontinued" Type="datetime" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedCategory_Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedCategory_Name" Type="nvarchar(max)" Nullable="false" />
  </EntityType>
  <Association Name="DiscontinuedCategoryDiscontinuedProduct">
    <End Role="DiscontinuedCategory" Type="DiscProductModel.Store.DiscontinuedCategorySet" Multiplicity="1" />
    <End Role="DiscontinuedProduct" Type="DiscProductModel.Store.ProductSet_DiscontinuedProduct" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="DiscontinuedCategory">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
      </Principal>
      <Dependent Role="DiscontinuedProduct">
        <PropertyRef Name="DiscontinuedCategory_Id" />
        <PropertyRef Name="DiscontinuedCategory_Name" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_DiscontinuedProduct_inherits_Product">
    <End Role="Product" Type="DiscProductModel.Store.ProductSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="DiscontinuedProduct" Type="DiscProductModel.Store.ProductSet_DiscontinuedProduct" Multiplicity="0..1" />
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
  <EntityContainerMapping StorageEntityContainer="DiscProductModelStoreContainer" CdmEntityContainer="DiscProductModelContainer">
    <EntitySetMapping Name="DiscontinuedCategorySet">
      <EntityTypeMapping TypeName="IsTypeOf(DiscProductModel.DiscontinuedCategory)">
        <MappingFragment StoreEntitySet="DiscontinuedCategorySet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Description" ColumnName="Description" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="ProductSet">
      <EntityTypeMapping TypeName="IsTypeOf(DiscProductModel.Product)">
        <MappingFragment StoreEntitySet="ProductSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Property" ColumnName="Property" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Description" ColumnName="Description" />
          <ScalarProperty Name="NumInStock" ColumnName="NumInStock" />
          <ScalarProperty Name="Price" ColumnName="Price" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(DiscProductModel.DiscontinuedProduct)">
        <MappingFragment StoreEntitySet="ProductSet_DiscontinuedProduct">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="DateDiscontinued" ColumnName="DateDiscontinued" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="DiscontinuedCategoryDiscontinuedProduct" TypeName="DiscProductModel.DiscontinuedCategoryDiscontinuedProduct" StoreEntitySet="ProductSet_DiscontinuedProduct">
      <EndProperty Name="DiscontinuedCategory">
        <ScalarProperty Name="Id" ColumnName="DiscontinuedCategory_Id" />
        <ScalarProperty Name="Name" ColumnName="DiscontinuedCategory_Name" />
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

-- Creating table 'DiscontinuedCategorySet'
CREATE TABLE [dbo].[DiscontinuedCategorySet] (
    [Id] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Description] int  NULL
);
GO

-- Creating table 'ProductSet'
CREATE TABLE [dbo].[ProductSet] (
    [Id] int  NOT NULL,
    [Property] int  NULL,
    [Name] nvarchar(max)  NULL,
    [Description] nvarchar(max)  NULL,
    [NumInStock] int  NULL,
    [Price] decimal(18,0)  NULL
);
GO

-- Creating table 'ProductSet_DiscontinuedProduct'
CREATE TABLE [dbo].[ProductSet_DiscontinuedProduct] (
    [DateDiscontinued] datetime  NULL,
    [Id] int  NOT NULL,
    [DiscontinuedCategory_Id] int  NOT NULL,
    [DiscontinuedCategory_Name] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id], [Name] in table 'DiscontinuedCategorySet'
ALTER TABLE [dbo].[DiscontinuedCategorySet]
ADD CONSTRAINT [PK_DiscontinuedCategorySet]
    PRIMARY KEY CLUSTERED ([Id], [Name] ASC);
GO

-- Creating primary key on [Id] in table 'ProductSet'
ALTER TABLE [dbo].[ProductSet]
ADD CONSTRAINT [PK_ProductSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProductSet_DiscontinuedProduct'
ALTER TABLE [dbo].[ProductSet_DiscontinuedProduct]
ADD CONSTRAINT [PK_ProductSet_DiscontinuedProduct]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [DiscontinuedCategory_Id], [DiscontinuedCategory_Name] in table 'ProductSet_DiscontinuedProduct'
ALTER TABLE [dbo].[ProductSet_DiscontinuedProduct]
ADD CONSTRAINT [FK_DiscontinuedCategoryDiscontinuedProduct]
    FOREIGN KEY ([DiscontinuedCategory_Id], [DiscontinuedCategory_Name])
    REFERENCES [dbo].[DiscontinuedCategorySet]
        ([Id], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscontinuedCategoryDiscontinuedProduct'
CREATE INDEX [IX_FK_DiscontinuedCategoryDiscontinuedProduct]
ON [dbo].[ProductSet_DiscontinuedProduct]
    ([DiscontinuedCategory_Id], [DiscontinuedCategory_Name]);
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
