<StorageAndMappings>
  <Schema Namespace="NorthwindModel.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="NorthwindModelStoreContainer">
    <EntitySet Name="CustomerDemographics" EntityType="NorthwindModel.Store.CustomerDemographics" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Customers" EntityType="NorthwindModel.Store.Customers" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CustomerCustomerDemo" EntityType="NorthwindModel.Store.CustomerCustomerDemo" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_CustomerCustomerDemo_CustomerDemographics" Association="NorthwindModel.Store.FK_CustomerCustomerDemo_CustomerDemographics">
      <End Role="CustomerDemographics" EntitySet="CustomerDemographics" />
      <End Role="CustomerCustomerDemo" EntitySet="CustomerCustomerDemo" />
    </AssociationSet>
    <AssociationSet Name="FK_CustomerCustomerDemo_Customers" Association="NorthwindModel.Store.FK_CustomerCustomerDemo_Customers">
      <End Role="Customers" EntitySet="Customers" />
      <End Role="CustomerCustomerDemo" EntitySet="CustomerCustomerDemo" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="CustomerDemographics">
    <Key>
      <PropertyRef Name="CustomerTypeID" />
    </Key>
    <Property Name="CustomerTypeID" Type="nchar" Nullable="false" MaxLength="10" />
    <Property Name="CustomerDesc" Type="nvarchar(max)" Nullable="true" />
  </EntityType>
  <EntityType Name="Customers">
    <Key>
      <PropertyRef Name="CustomerID" />
      <PropertyRef Name="CompanyName" />
      <PropertyRef Name="ContactName" />
    </Key>
    <Property Name="CustomerID" Type="nchar" Nullable="false" MaxLength="5" />
    <Property Name="CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="ContactName" Type="nvarchar" Nullable="false" MaxLength="30" />
    <Property Name="ContactTitle" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="Address" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="City" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Region" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="PostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="Country" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Phone" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="Fax" Type="nvarchar" Nullable="true" MaxLength="24" />
  </EntityType>
  <EntityType Name="CustomerCustomerDemo">
    <Key>
      <PropertyRef Name="CustomerDemographics_CustomerTypeID" />
      <PropertyRef Name="Customers_CustomerID" />
      <PropertyRef Name="Customers_CompanyName" />
      <PropertyRef Name="Customers_ContactName" />
    </Key>
    <Property Name="CustomerDemographics_CustomerTypeID" Type="nchar" Nullable="false" MaxLength="10" />
    <Property Name="Customers_CustomerID" Type="nchar" Nullable="false" MaxLength="5" />
    <Property Name="Customers_CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="Customers_ContactName" Type="nvarchar" Nullable="false" MaxLength="30" />
  </EntityType>
  <Association Name="FK_CustomerCustomerDemo_CustomerDemographics">
    <End Role="CustomerDemographics" Type="NorthwindModel.Store.CustomerDemographics" Multiplicity="1" />
    <End Role="CustomerCustomerDemo" Type="NorthwindModel.Store.CustomerCustomerDemo" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="CustomerDemographics">
        <PropertyRef Name="CustomerTypeID" />
      </Principal>
      <Dependent Role="CustomerCustomerDemo">
        <PropertyRef Name="CustomerDemographics_CustomerTypeID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_CustomerCustomerDemo_Customers">
    <End Role="CustomerCustomerDemo" Type="NorthwindModel.Store.CustomerCustomerDemo" Multiplicity="*" />
    <End Role="Customers" Type="NorthwindModel.Store.Customers" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="Customers">
        <PropertyRef Name="CustomerID" />
        <PropertyRef Name="CompanyName" />
        <PropertyRef Name="ContactName" />
      </Principal>
      <Dependent Role="CustomerCustomerDemo">
        <PropertyRef Name="Customers_CustomerID" />
        <PropertyRef Name="Customers_CompanyName" />
        <PropertyRef Name="Customers_ContactName" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="NorthwindModelStoreContainer" CdmEntityContainer="NorthwindEntities1">
    <EntitySetMapping Name="CustomerDemographics">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.CustomerDemographics)">
        <MappingFragment StoreEntitySet="CustomerDemographics">
          <ScalarProperty Name="CustomerTypeID" ColumnName="CustomerTypeID" />
          <ScalarProperty Name="CustomerDesc" ColumnName="CustomerDesc" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Customers">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Customers)">
        <MappingFragment StoreEntitySet="Customers">
          <ScalarProperty Name="CustomerID" ColumnName="CustomerID" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="ContactName" ColumnName="ContactName" />
          <ScalarProperty Name="ContactTitle" ColumnName="ContactTitle" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="City" ColumnName="City" />
          <ScalarProperty Name="Region" ColumnName="Region" />
          <ScalarProperty Name="PostalCode" ColumnName="PostalCode" />
          <ScalarProperty Name="Country" ColumnName="Country" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
          <ScalarProperty Name="Fax" ColumnName="Fax" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="CustomerCustomerDemo" TypeName="NorthwindModel.CustomerCustomerDemo" StoreEntitySet="CustomerCustomerDemo">
      <EndProperty Name="CustomerDemographics">
        <ScalarProperty Name="CustomerTypeID" ColumnName="CustomerDemographics_CustomerTypeID" />
      </EndProperty>
      <EndProperty Name="Customers">
        <ScalarProperty Name="CustomerID" ColumnName="Customers_CustomerID" />
        <ScalarProperty Name="CompanyName" ColumnName="Customers_CompanyName" />
        <ScalarProperty Name="ContactName" ColumnName="Customers_ContactName" />
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

-- Creating table 'CustomerDemographics'
CREATE TABLE [dbo].[CustomerDemographics] (
    [CustomerTypeID] nchar(10)  NOT NULL,
    [CustomerDesc] nvarchar(max)  NULL
);
GO

-- Creating table 'Customers'
CREATE TABLE [dbo].[Customers] (
    [CustomerID] nchar(5)  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NOT NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL
);
GO

-- Creating table 'CustomerCustomerDemo'
CREATE TABLE [dbo].[CustomerCustomerDemo] (
    [CustomerDemographics_CustomerTypeID] nchar(10)  NOT NULL,
    [Customers_CustomerID] nchar(5)  NOT NULL,
    [Customers_CompanyName] nvarchar(40)  NOT NULL,
    [Customers_ContactName] nvarchar(30)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CustomerTypeID] in table 'CustomerDemographics'
ALTER TABLE [dbo].[CustomerDemographics]
ADD CONSTRAINT [PK_CustomerDemographics]
    PRIMARY KEY CLUSTERED ([CustomerTypeID] ASC);
GO

-- Creating primary key on [CustomerID], [CompanyName], [ContactName] in table 'Customers'
ALTER TABLE [dbo].[Customers]
ADD CONSTRAINT [PK_Customers]
    PRIMARY KEY CLUSTERED ([CustomerID], [CompanyName], [ContactName] ASC);
GO

-- Creating primary key on [CustomerDemographics_CustomerTypeID], [Customers_CustomerID], [Customers_CompanyName], [Customers_ContactName] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [PK_CustomerCustomerDemo]
    PRIMARY KEY CLUSTERED ([CustomerDemographics_CustomerTypeID], [Customers_CustomerID], [Customers_CompanyName], [Customers_ContactName] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CustomerDemographics_CustomerTypeID] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [FK_CustomerCustomerDemo_CustomerDemographics]
    FOREIGN KEY ([CustomerDemographics_CustomerTypeID])
    REFERENCES [dbo].[CustomerDemographics]
        ([CustomerTypeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Customers_CustomerID], [Customers_CompanyName], [Customers_ContactName] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [FK_CustomerCustomerDemo_Customers]
    FOREIGN KEY ([Customers_CustomerID], [Customers_CompanyName], [Customers_ContactName])
    REFERENCES [dbo].[Customers]
        ([CustomerID], [CompanyName], [ContactName])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomerCustomerDemo_Customers'
CREATE INDEX [IX_FK_CustomerCustomerDemo_Customers]
ON [dbo].[CustomerCustomerDemo]
    ([Customers_CustomerID], [Customers_CompanyName], [Customers_ContactName]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
