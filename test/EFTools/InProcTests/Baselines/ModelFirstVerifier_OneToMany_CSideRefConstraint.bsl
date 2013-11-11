<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="OrderSet" EntityType="Model1.Store.OrderSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CustomerSet" EntityType="Model1.Store.CustomerSet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="CustomerOrderSet" Association="Model1.Store.CustomerOrder">
      <End Role="CustomerEnd" EntitySet="CustomerSet" />
      <End Role="OrderEnd" EntitySet="OrderSet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="OrderSet">
    <Key>
      <PropertyRef Name="OrderId" />
      <PropertyRef Name="CustomerName" />
      <PropertyRef Name="CustomerId" />
    </Key>
    <Property Name="OrderId" Type="int" Nullable="false" />
    <Property Name="Date" Type="datetime" Nullable="true" />
    <Property Name="CustomerName" Type="int" Nullable="false" />
    <Property Name="CustomerId" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="CustomerSet">
    <Key>
      <PropertyRef Name="CustomerName" />
      <PropertyRef Name="CustomerId" />
    </Key>
    <Property Name="Name" Type="varchar(max)" Nullable="true" />
    <Property Name="CustomerName" Type="int" Nullable="false" />
    <Property Name="CustomerId" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="CustomerOrder">
    <End Role="CustomerEnd" Type="Model1.Store.CustomerSet" Multiplicity="1" />
    <End Role="OrderEnd" Type="Model1.Store.OrderSet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="CustomerEnd">
        <PropertyRef Name="CustomerName" />
        <PropertyRef Name="CustomerId" />
      </Principal>
      <Dependent Role="OrderEnd">
        <PropertyRef Name="CustomerName" />
        <PropertyRef Name="CustomerId" />
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
          <ScalarProperty Name="OrderId" ColumnName="OrderId" />
          <ScalarProperty Name="CustomerName" ColumnName="CustomerName" />
          <ScalarProperty Name="CustomerId" ColumnName="CustomerId" />
          <ScalarProperty Name="Date" ColumnName="Date" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="CustomerSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Customer)">
        <MappingFragment StoreEntitySet="CustomerSet">
          <ScalarProperty Name="CustomerName" ColumnName="CustomerName" />
          <ScalarProperty Name="CustomerId" ColumnName="CustomerId" />
          <ScalarProperty Name="Name" ColumnName="Name" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="CustomerOrderSet" TypeName="Model1.CustomerOrder" StoreEntitySet="OrderSet">
      <EndProperty Name="CustomerEnd">
        <ScalarProperty Name="CustomerName" ColumnName="CustomerName" />
        <ScalarProperty Name="CustomerId" ColumnName="CustomerId" />
      </EndProperty>
      <EndProperty Name="OrderEnd">
        <ScalarProperty Name="OrderId" ColumnName="OrderId" />
        <ScalarProperty Name="CustomerName" ColumnName="CustomerName" />
        <ScalarProperty Name="CustomerId" ColumnName="CustomerId" />
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
    [OrderId] int  NOT NULL,
    [Date] datetime  NULL,
    [CustomerName] int  NOT NULL,
    [CustomerId] int  NOT NULL
);
GO

-- Creating table 'CustomerSet'
CREATE TABLE [dbo].[CustomerSet] (
    [Name] varchar(max)  NULL,
    [CustomerName] int  NOT NULL,
    [CustomerId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [OrderId], [CustomerName], [CustomerId] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [PK_OrderSet]
    PRIMARY KEY CLUSTERED ([OrderId], [CustomerName], [CustomerId] ASC);
GO

-- Creating primary key on [CustomerName], [CustomerId] in table 'CustomerSet'
ALTER TABLE [dbo].[CustomerSet]
ADD CONSTRAINT [PK_CustomerSet]
    PRIMARY KEY CLUSTERED ([CustomerName], [CustomerId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CustomerName], [CustomerId] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [FK_CustomerOrder]
    FOREIGN KEY ([CustomerName], [CustomerId])
    REFERENCES [dbo].[CustomerSet]
        ([CustomerName], [CustomerId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomerOrder'
CREATE INDEX [IX_FK_CustomerOrder]
ON [dbo].[OrderSet]
    ([CustomerName], [CustomerId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
