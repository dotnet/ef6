<StorageAndMappings>
  <Schema Namespace="OneToMany.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="OneToManyStoreContainer">
    <EntitySet Name="CustomerSet" EntityType="OneToMany.Store.CustomerSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderSet" EntityType="OneToMany.Store.OrderSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="PersonSet" EntityType="OneToMany.Store.PersonSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OnlineIdentitySet" EntityType="OneToMany.Store.OnlineIdentitySet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="Entity1Entity2Set" Association="OneToMany.Store.Entity1Entity2">
      <End Role="Entity1" EntitySet="CustomerSet" />
      <End Role="Entity2" EntitySet="OrderSet" />
    </AssociationSet>
    <AssociationSet Name="PersonOnlineIdentity" Association="OneToMany.Store.PersonOnlineIdentity">
      <End Role="Person" EntitySet="PersonSet" />
      <End Role="OnlineIdentity" EntitySet="OnlineIdentitySet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="CustomerSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="varchar(max)" Nullable="true" />
    <Property Name="Address" Type="varchar(max)" Nullable="true" />
    <Property Name="Phone" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="OrderSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Date" Type="datetime" Nullable="true" />
    <Property Name="Entity1_Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="PersonSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="RealName" Type="varchar" Nullable="true" MaxLength="100" />
  </EntityType>
  <EntityType Name="OnlineIdentitySet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Username" Type="varchar" Nullable="true" MaxLength="20" />
    <Property Name="Password" Type="varchar" Nullable="true" MaxLength="10" />
    <Property Name="Person_Id" Type="int" Nullable="true" />
  </EntityType>
  <Association Name="Entity1Entity2">
    <End Role="Entity1" Type="OneToMany.Store.CustomerSet" Multiplicity="1" />
    <End Role="Entity2" Type="OneToMany.Store.OrderSet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Entity1">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Entity2">
        <PropertyRef Name="Entity1_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="PersonOnlineIdentity">
    <End Role="Person" Type="OneToMany.Store.PersonSet" Multiplicity="0..1" />
    <End Role="OnlineIdentity" Type="OneToMany.Store.OnlineIdentitySet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Person">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="OnlineIdentity">
        <PropertyRef Name="Person_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="OneToManyStoreContainer" CdmEntityContainer="OneToManyContainer">
    <EntitySetMapping Name="CustomerSet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToMany.Customer)">
        <MappingFragment StoreEntitySet="CustomerSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderSet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToMany.Order)">
        <MappingFragment StoreEntitySet="OrderSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Date" ColumnName="Date" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="PersonSet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToMany.Person)">
        <MappingFragment StoreEntitySet="PersonSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="RealName" ColumnName="RealName" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OnlineIdentitySet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToMany.OnlineIdentity)">
        <MappingFragment StoreEntitySet="OnlineIdentitySet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Username" ColumnName="Username" />
          <ScalarProperty Name="Password" ColumnName="Password" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="Entity1Entity2Set" TypeName="OneToMany.Entity1Entity2" StoreEntitySet="OrderSet">
      <EndProperty Name="Entity1">
        <ScalarProperty Name="Id" ColumnName="Entity1_Id" />
      </EndProperty>
      <EndProperty Name="Entity2">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="PersonOnlineIdentity" TypeName="OneToMany.PersonOnlineIdentity" StoreEntitySet="OnlineIdentitySet">
      <EndProperty Name="Person">
        <ScalarProperty Name="Id" ColumnName="Person_Id" />
      </EndProperty>
      <EndProperty Name="OnlineIdentity">
        <ScalarProperty Name="Id" ColumnName="Id" />
      </EndProperty>
      <Condition ColumnName="Person_Id" IsNull="false" />
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

-- Creating table 'CustomerSet'
CREATE TABLE [dbo].[CustomerSet] (
    [Id] int  NOT NULL,
    [Name] varchar(max)  NULL,
    [Address] varchar(max)  NULL,
    [Phone] int  NULL
);
GO

-- Creating table 'OrderSet'
CREATE TABLE [dbo].[OrderSet] (
    [Id] int  NOT NULL,
    [Date] datetime  NULL,
    [Entity1_Id] int  NOT NULL
);
GO

-- Creating table 'PersonSet'
CREATE TABLE [dbo].[PersonSet] (
    [Id] int  NOT NULL,
    [RealName] varchar(100)  NULL
);
GO

-- Creating table 'OnlineIdentitySet'
CREATE TABLE [dbo].[OnlineIdentitySet] (
    [Id] int  NOT NULL,
    [Username] varchar(20)  NULL,
    [Password] varchar(10)  NULL,
    [Person_Id] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'CustomerSet'
ALTER TABLE [dbo].[CustomerSet]
ADD CONSTRAINT [PK_CustomerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [PK_OrderSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'PersonSet'
ALTER TABLE [dbo].[PersonSet]
ADD CONSTRAINT [PK_PersonSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OnlineIdentitySet'
ALTER TABLE [dbo].[OnlineIdentitySet]
ADD CONSTRAINT [PK_OnlineIdentitySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Entity1_Id] in table 'OrderSet'
ALTER TABLE [dbo].[OrderSet]
ADD CONSTRAINT [FK_Entity1Entity2]
    FOREIGN KEY ([Entity1_Id])
    REFERENCES [dbo].[CustomerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Entity1Entity2'
CREATE INDEX [IX_FK_Entity1Entity2]
ON [dbo].[OrderSet]
    ([Entity1_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'OnlineIdentitySet'
ALTER TABLE [dbo].[OnlineIdentitySet]
ADD CONSTRAINT [FK_PersonOnlineIdentity]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[PersonSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonOnlineIdentity'
CREATE INDEX [IX_FK_PersonOnlineIdentity]
ON [dbo].[OnlineIdentitySet]
    ([Person_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
