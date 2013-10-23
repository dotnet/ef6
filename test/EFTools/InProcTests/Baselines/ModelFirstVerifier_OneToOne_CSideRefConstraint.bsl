<StorageAndMappings>
  <Schema Namespace="PkToPk.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="PkToPkStoreContainer">
    <EntitySet Name="DiscontinuedProductSet" EntityType="PkToPk.Store.DiscontinuedProductSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="DiscontinuedItemSet" EntityType="PkToPk.Store.DiscontinuedItemSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="PersonSet" EntityType="PkToPk.Store.PersonSet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="DiscontinuedProductDiscontinuedItem" Association="PkToPk.Store.DiscontinuedProductDiscontinuedItem">
      <End Role="DiscontinuedProduct" EntitySet="DiscontinuedProductSet" />
      <End Role="DiscontinuedItem" EntitySet="DiscontinuedItemSet" />
    </AssociationSet>
    <AssociationSet Name="SelfTestSelfTest" Association="PkToPk.Store.SelfTestSelfTest">
      <End Role="Boyfriend" EntitySet="PersonSet" />
      <End Role="Girlfriend" EntitySet="PersonSet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="DiscontinuedProductSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="DiscontinuedDate" Type="datetime" Nullable="true" />
  </EntityType>
  <EntityType Name="DiscontinuedItemSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="ItemName" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="PersonSet">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="int" Nullable="false" />
    <Property Name="Description" Type="varchar" Nullable="true" MaxLength="2000" />
    <Property Name="Title" Type="varchar" Nullable="true" MaxLength="1000" />
  </EntityType>
  <Association Name="DiscontinuedProductDiscontinuedItem">
    <End Role="DiscontinuedProduct" Type="PkToPk.Store.DiscontinuedProductSet" Multiplicity="1" />
    <End Role="DiscontinuedItem" Type="PkToPk.Store.DiscontinuedItemSet" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="DiscontinuedProduct">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="DiscontinuedItem">
        <PropertyRef Name="Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="SelfTestSelfTest">
    <End Role="Boyfriend" Type="PkToPk.Store.PersonSet" Multiplicity="1" />
    <End Role="Girlfriend" Type="PkToPk.Store.PersonSet" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Boyfriend">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
      </Principal>
      <Dependent Role="Girlfriend">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
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
    <EntitySetMapping Name="PersonSet">
      <EntityTypeMapping TypeName="IsTypeOf(PkToPk.Person)">
        <MappingFragment StoreEntitySet="PersonSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Description" ColumnName="Description" />
          <ScalarProperty Name="Title" ColumnName="Title" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="DiscontinuedProductDiscontinuedItem" TypeName="PkToPk.DiscontinuedProductDiscontinuedItem" StoreEntitySet="DiscontinuedItemSet">
      <EndProperty Name="DiscontinuedProduct">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
      <EndProperty Name="DiscontinuedItem">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="SelfTestSelfTest" TypeName="PkToPk.SelfTestSelfTest" StoreEntitySet="PersonSet">
      <EndProperty Name="Boyfriend">
        <ScalarProperty Name="Id" ColumnName="Id" />
        <ScalarProperty Name="Name" ColumnName="Name" />
      </EndProperty>
      <EndProperty Name="Girlfriend">
        <ScalarProperty Name="Id" ColumnName="Id" />
        <ScalarProperty Name="Name" ColumnName="Name" />
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
    [DiscontinuedDate] datetime  NULL
);
GO

-- Creating table 'DiscontinuedItemSet'
CREATE TABLE [dbo].[DiscontinuedItemSet] (
    [Id] int  NOT NULL,
    [ItemName] int  NULL
);
GO

-- Creating table 'PersonSet'
CREATE TABLE [dbo].[PersonSet] (
    [Id] int  NOT NULL,
    [Name] int  NOT NULL,
    [Description] varchar(2000)  NULL,
    [Title] varchar(1000)  NULL
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

-- Creating primary key on [Id], [Name] in table 'PersonSet'
ALTER TABLE [dbo].[PersonSet]
ADD CONSTRAINT [PK_PersonSet]
    PRIMARY KEY CLUSTERED ([Id], [Name] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Id] in table 'DiscontinuedItemSet'
ALTER TABLE [dbo].[DiscontinuedItemSet]
ADD CONSTRAINT [FK_DiscontinuedProductDiscontinuedItem]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[DiscontinuedProductSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name] in table 'PersonSet'
ALTER TABLE [dbo].[PersonSet]
ADD CONSTRAINT [FK_SelfTestSelfTest]
    FOREIGN KEY ([Id], [Name])
    REFERENCES [dbo].[PersonSet]
        ([Id], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
