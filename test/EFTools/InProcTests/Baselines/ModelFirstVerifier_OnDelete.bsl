<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2009/02/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="OrderSet" EntityType="Model1.Store.OrderSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProductSet" EntityType="Model1.Store.ProductSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderRCSet" EntityType="Model1.Store.OrderRCSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderLineItemRCSet" EntityType="Model1.Store.OrderLineItemRCSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderRCCorrectSet" EntityType="Model1.Store.OrderRCCorrectSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderLIRCCorrectSet" EntityType="Model1.Store.OrderLIRCCorrectSet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="OrderProduct" Association="Model1.Store.OrderProduct">
      <End Role="Order" EntitySet="OrderSet" />
      <End Role="Product" EntitySet="ProductSet" />
    </AssociationSet>
    <AssociationSet Name="OrderRCOrderLineItemRC" Association="Model1.Store.OrderRCOrderLineItemRC">
      <End Role="OrderRC" EntitySet="OrderRCSet" />
      <End Role="OrderLineItemRC" EntitySet="OrderLineItemRCSet" />
    </AssociationSet>
    <AssociationSet Name="OrderRCCorrectOrderLIRCCorrect" Association="Model1.Store.OrderRCCorrectOrderLIRCCorrect">
      <End Role="OrderRCCorrect" EntitySet="OrderRCCorrectSet" />
      <End Role="OrderLIRCCorrect" EntitySet="OrderLIRCCorrectSet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="OrderSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="ProductSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Order_Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderRCSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderLineItemRCSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderRCCorrectSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderLIRCCorrectSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="OrderProduct">
    <End Role="Order" Type="Model1.Store.OrderSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Product" Type="Model1.Store.ProductSet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Order">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Product">
        <PropertyRef Name="Order_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="OrderRCOrderLineItemRC">
    <End Role="OrderRC" Type="Model1.Store.OrderRCSet" Multiplicity="0..1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="OrderLineItemRC" Type="Model1.Store.OrderLineItemRCSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="OrderLineItemRC">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="OrderRC">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="OrderRCCorrectOrderLIRCCorrect">
    <End Role="OrderRCCorrect" Type="Model1.Store.OrderRCCorrectSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="OrderLIRCCorrect" Type="Model1.Store.OrderLIRCCorrectSet" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="OrderRCCorrect">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="OrderLIRCCorrect">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2008/09/mapping/cs">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="OrderSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Order)">
        <MappingFragment StoreEntitySet="OrderSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="ProductSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Product)">
        <MappingFragment StoreEntitySet="ProductSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderRCSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.OrderRC)">
        <MappingFragment StoreEntitySet="OrderRCSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderLineItemRCSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.OrderLineItemRC)">
        <MappingFragment StoreEntitySet="OrderLineItemRCSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderRCCorrectSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.OrderRCCorrect)">
        <MappingFragment StoreEntitySet="OrderRCCorrectSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderLIRCCorrectSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.OrderLIRCCorrect)">
        <MappingFragment StoreEntitySet="OrderLIRCCorrectSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="OrderProduct" TypeName="Model1.OrderProduct" StoreEntitySet="ProductSet">
      <EndProperty Name="Order">
        <ScalarProperty Name="Id" ColumnName="Order_Id" />
      </EndProperty>
      <EndProperty Name="Product">
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
    [Id] int  NOT NULL
);
GO

-- Creating table 'ProductSet'
CREATE TABLE [dbo].[ProductSet] (
    [Id] int  NOT NULL,
    [Order_Id] int  NOT NULL
);
GO

-- Creating table 'OrderRCSet'
CREATE TABLE [dbo].[OrderRCSet] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'OrderLineItemRCSet'
CREATE TABLE [dbo].[OrderLineItemRCSet] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'OrderRCCorrectSet'
CREATE TABLE [dbo].[OrderRCCorrectSet] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'OrderLIRCCorrectSet'
CREATE TABLE [dbo].[OrderLIRCCorrectSet] (
    [Id] int  NOT NULL
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

-- Creating primary key on [Id] in table 'OrderRCSet'
ALTER TABLE [dbo].[OrderRCSet]
ADD CONSTRAINT [PK_OrderRCSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderLineItemRCSet'
ALTER TABLE [dbo].[OrderLineItemRCSet]
ADD CONSTRAINT [PK_OrderLineItemRCSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderRCCorrectSet'
ALTER TABLE [dbo].[OrderRCCorrectSet]
ADD CONSTRAINT [PK_OrderRCCorrectSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderLIRCCorrectSet'
ALTER TABLE [dbo].[OrderLIRCCorrectSet]
ADD CONSTRAINT [PK_OrderLIRCCorrectSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Order_Id] in table 'ProductSet'
ALTER TABLE [dbo].[ProductSet]
ADD CONSTRAINT [FK_OrderProduct]
    FOREIGN KEY ([Order_Id])
    REFERENCES [dbo].[OrderSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrderProduct'
CREATE INDEX [IX_FK_OrderProduct]
ON [dbo].[ProductSet]
    ([Order_Id]);
GO

-- Creating foreign key on [Id] in table 'OrderRCSet'
ALTER TABLE [dbo].[OrderRCSet]
ADD CONSTRAINT [FK_OrderRCOrderLineItemRC]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[OrderLineItemRCSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'OrderLIRCCorrectSet'
ALTER TABLE [dbo].[OrderLIRCCorrectSet]
ADD CONSTRAINT [FK_OrderRCCorrectOrderLIRCCorrect]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[OrderRCCorrectSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
