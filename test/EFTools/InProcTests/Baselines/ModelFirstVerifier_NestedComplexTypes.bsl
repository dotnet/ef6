<StorageAndMappings>
  <Schema Namespace="CustomerComplexAddress.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="CustomerComplexAddressStoreContainer">
    <EntitySet Name="CCustomers" EntityType="CustomerComplexAddress.Store.CCustomers" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderSet" EntityType="CustomerComplexAddress.Store.OrderSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderSet_SpecialOrder" EntityType="CustomerComplexAddress.Store.OrderSet_SpecialOrder" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="CCustomerOrder" Association="CustomerComplexAddress.Store.CCustomerOrder">
      <End Role="CCustomer" EntitySet="CCustomers" />
      <End Role="Order" EntitySet="OrderSet" />
    </AssociationSet>
    <AssociationSet Name="FK_SpecialOrder_inherits_Order" Association="CustomerComplexAddress.Store.FK_SpecialOrder_inherits_Order">
      <End Role="Order" EntitySet="OrderSet" />
      <End Role="SpecialOrder" EntitySet="OrderSet_SpecialOrder" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="CCustomers">
    <Key>
      <PropertyRef Name="CustomerId" />
    </Key>
    <Property Name="CustomerId" Type="int" Nullable="false" />
    <Property Name="CompanyName" Type="nvarchar(max)" Nullable="true" />
    <Property Name="ContactName" Type="nvarchar(max)" Nullable="true" />
    <Property Name="ContactTitle" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_City" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_Region" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_PostalCode" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_Country" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_Phone" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_Fax" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Address_StreetAddress_AptNumber" Type="int" Nullable="false" />
    <Property Name="Address_StreetAddress_StreetName" Type="nvarchar(max)" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="BillingAddress_City" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_Region" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_PostalCode" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_Country" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_Phone" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_Fax" Type="nvarchar(max)" Nullable="true" />
    <Property Name="BillingAddress_StreetAddress_AptNumber" Type="int" Nullable="false" />
    <Property Name="BillingAddress_StreetAddress_StreetName" Type="nvarchar(max)" Nullable="false" />
    <Property Name="CCustomer_CustomerId" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderSet_SpecialOrder">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="HiddenAddress_City" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_Region" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_PostalCode" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_Country" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_Phone" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_Fax" Type="nvarchar(max)" Nullable="true" />
    <Property Name="HiddenAddress_StreetAddress_AptNumber" Type="int" Nullable="false" />
    <Property Name="HiddenAddress_StreetAddress_StreetName" Type="nvarchar(max)" Nullable="false" />
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="CCustomerOrder">
    <End Role="CCustomer" Type="CustomerComplexAddress.Store.CCustomers" Multiplicity="1" />
    <End Role="Order" Type="CustomerComplexAddress.Store.OrderSet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="CCustomer">
        <PropertyRef Name="CustomerId" />
      </Principal>
      <Dependent Role="Order">
        <PropertyRef Name="CCustomer_CustomerId" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_SpecialOrder_inherits_Order">
    <End Role="Order" Type="CustomerComplexAddress.Store.OrderSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="SpecialOrder" Type="CustomerComplexAddress.Store.OrderSet_SpecialOrder" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Order">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="SpecialOrder">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="CustomerComplexAddressStoreContainer" CdmEntityContainer="CustomerComplexAddressContext">
    <EntitySetMapping Name="CCustomers">
      <EntityTypeMapping TypeName="IsTypeOf(CustomerComplexAddress.CCustomer)">
        <MappingFragment StoreEntitySet="CCustomers">
          <ScalarProperty Name="CustomerId" ColumnName="CustomerId" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="ContactName" ColumnName="ContactName" />
          <ScalarProperty Name="ContactTitle" ColumnName="ContactTitle" />
          <ComplexProperty Name="Address" TypeName="CustomerComplexAddress.CAddress">
            <ScalarProperty Name="City" ColumnName="Address_City" />
            <ScalarProperty Name="Region" ColumnName="Address_Region" />
            <ScalarProperty Name="PostalCode" ColumnName="Address_PostalCode" />
            <ScalarProperty Name="Country" ColumnName="Address_Country" />
            <ScalarProperty Name="Phone" ColumnName="Address_Phone" />
            <ScalarProperty Name="Fax" ColumnName="Address_Fax" />
            <ComplexProperty Name="StreetAddress" TypeName="CustomerComplexAddress.CStreetAddress">
              <ScalarProperty Name="AptNumber" ColumnName="Address_StreetAddress_AptNumber" />
              <ScalarProperty Name="StreetName" ColumnName="Address_StreetAddress_StreetName" />
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderSet">
      <EntityTypeMapping TypeName="IsTypeOf(CustomerComplexAddress.Order)">
        <MappingFragment StoreEntitySet="OrderSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ComplexProperty Name="BillingAddress" TypeName="CustomerComplexAddress.CAddress">
            <ScalarProperty Name="City" ColumnName="BillingAddress_City" />
            <ScalarProperty Name="Region" ColumnName="BillingAddress_Region" />
            <ScalarProperty Name="PostalCode" ColumnName="BillingAddress_PostalCode" />
            <ScalarProperty Name="Country" ColumnName="BillingAddress_Country" />
            <ScalarProperty Name="Phone" ColumnName="BillingAddress_Phone" />
            <ScalarProperty Name="Fax" ColumnName="BillingAddress_Fax" />
            <ComplexProperty Name="StreetAddress" TypeName="CustomerComplexAddress.CStreetAddress">
              <ScalarProperty Name="AptNumber" ColumnName="BillingAddress_StreetAddress_AptNumber" />
              <ScalarProperty Name="StreetName" ColumnName="BillingAddress_StreetAddress_StreetName" />
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(CustomerComplexAddress.SpecialOrder)">
        <MappingFragment StoreEntitySet="OrderSet_SpecialOrder">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ComplexProperty Name="HiddenAddress" TypeName="CustomerComplexAddress.CAddress">
            <ScalarProperty Name="City" ColumnName="HiddenAddress_City" />
            <ScalarProperty Name="Region" ColumnName="HiddenAddress_Region" />
            <ScalarProperty Name="PostalCode" ColumnName="HiddenAddress_PostalCode" />
            <ScalarProperty Name="Country" ColumnName="HiddenAddress_Country" />
            <ScalarProperty Name="Phone" ColumnName="HiddenAddress_Phone" />
            <ScalarProperty Name="Fax" ColumnName="HiddenAddress_Fax" />
            <ComplexProperty Name="StreetAddress" TypeName="CustomerComplexAddress.CStreetAddress">
              <ScalarProperty Name="AptNumber" ColumnName="HiddenAddress_StreetAddress_AptNumber" />
              <ScalarProperty Name="StreetName" ColumnName="HiddenAddress_StreetAddress_StreetName" />
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="CCustomerOrder" TypeName="CustomerComplexAddress.CCustomerOrder" StoreEntitySet="OrderSet">
      <EndProperty Name="CCustomer">
        <ScalarProperty Name="CustomerId" ColumnName="CCustomer_CustomerId" />
      </EndProperty>
      <EndProperty Name="Order">
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

-- Creating table 'CCustomers'
CREATE TABLE [dbo].[CCustomers] (
    [CustomerId] int  NOT NULL,
    [CompanyName] nvarchar(max)  NULL,
    [ContactName] nvarchar(max)  NULL,
    [ContactTitle] nvarchar(max)  NULL,
    [Address_City] nvarchar(max)  NULL,
    [Address_Region] nvarchar(max)  NULL,
    [Address_PostalCode] nvarchar(max)  NULL,
    [Address_Country] nvarchar(max)  NULL,
    [Address_Phone] nvarchar(max)  NULL,
    [Address_Fax] nvarchar(max)  NULL,
    [Address_StreetAddress_AptNumber] int  NOT NULL,
    [Address_StreetAddress_StreetName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'OrderSet'
CREATE TABLE [dbo].[OrderSet] (
    [Id] int  NOT NULL,
    [BillingAddress_City] nvarchar(max)  NULL,
    [BillingAddress_Region] nvarchar(max)  NULL,
    [BillingAddress_PostalCode] nvarchar(max)  NULL,
    [BillingAddress_Country] nvarchar(max)  NULL,
    [BillingAddress_Phone] nvarchar(max)  NULL,
    [BillingAddress_Fax] nvarchar(max)  NULL,
    [BillingAddress_StreetAddress_AptNumber] int  NOT NULL,
    [BillingAddress_StreetAddress_StreetName] nvarchar(max)  NOT NULL,
    [CCustomer_CustomerId] int  NOT NULL
);
GO

-- Creating table 'OrderSet_SpecialOrder'
CREATE TABLE [dbo].[OrderSet_SpecialOrder] (
    [HiddenAddress_City] nvarchar(max)  NULL,
    [HiddenAddress_Region] nvarchar(max)  NULL,
    [HiddenAddress_PostalCode] nvarchar(max)  NULL,
    [HiddenAddress_Country] nvarchar(max)  NULL,
    [HiddenAddress_Phone] nvarchar(max)  NULL,
    [HiddenAddress_Fax] nvarchar(max)  NULL,
    [HiddenAddress_StreetAddress_AptNumber] int  NOT NULL,
    [HiddenAddress_StreetAddress_StreetName] nvarchar(max)  NOT NULL,
    [Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CustomerId] in table 'CCustomers'
ALTER TABLE [dbo].[CCustomers]
ADD CONSTRAINT [PK_CCustomers]
    PRIMARY KEY CLUSTERED ([CustomerId] ASC);
GO

-- Creating primary key on [Id] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [PK_OrderSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderSet_SpecialOrder'
ALTER TABLE [dbo].[OrderSet_SpecialOrder]
ADD CONSTRAINT [PK_OrderSet_SpecialOrder]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CCustomer_CustomerId] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [FK_CCustomerOrder]
    FOREIGN KEY ([CCustomer_CustomerId])
    REFERENCES [dbo].[CCustomers]
        ([CustomerId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CCustomerOrder'
CREATE INDEX [IX_FK_CCustomerOrder]
ON [dbo].[OrderSet]
    ([CCustomer_CustomerId]);
GO

-- Creating foreign key on [Id] in table 'OrderSet_SpecialOrder'
ALTER TABLE [dbo].[OrderSet_SpecialOrder]
ADD CONSTRAINT [FK_SpecialOrder_inherits_Order]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[OrderSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
