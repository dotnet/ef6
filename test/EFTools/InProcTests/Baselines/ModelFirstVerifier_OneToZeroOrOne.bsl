<StorageAndMappings>
  <Schema Namespace="OneToZeroOrOne.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="OneToZeroOrOneStoreContainer">
    <EntitySet Name="UserSet" EntityType="OneToZeroOrOne.Store.UserSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="ProfileSet" EntityType="OneToZeroOrOne.Store.ProfileSet" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="UserProfile" Association="OneToZeroOrOne.Store.UserProfile">
      <End Role="User" EntitySet="UserSet" />
      <End Role="Profile" EntitySet="ProfileSet" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="UserSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Username" Type="varchar" Nullable="true" MaxLength="100" />
    <Property Name="IsModerator" Type="bit" Nullable="true" />
  </EntityType>
  <EntityType Name="ProfileSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="ProfileText" Type="varchar(max)" Nullable="true" />
    <Property Name="User_Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="UserProfile">
    <End Role="User" Type="OneToZeroOrOne.Store.UserSet" Multiplicity="1" />
    <End Role="Profile" Type="OneToZeroOrOne.Store.ProfileSet" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="User">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="Profile">
        <PropertyRef Name="User_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="OneToZeroOrOneStoreContainer" CdmEntityContainer="OneToZeroOrOneContainer">
    <EntitySetMapping Name="UserSet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToZeroOrOne.User)">
        <MappingFragment StoreEntitySet="UserSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Username" ColumnName="Username" />
          <ScalarProperty Name="IsModerator" ColumnName="IsModerator" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="ProfileSet">
      <EntityTypeMapping TypeName="IsTypeOf(OneToZeroOrOne.Profile)">
        <MappingFragment StoreEntitySet="ProfileSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="ProfileText" ColumnName="ProfileText" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="UserProfile" TypeName="OneToZeroOrOne.UserProfile" StoreEntitySet="ProfileSet">
      <EndProperty Name="User">
        <ScalarProperty Name="Id" ColumnName="User_Id" />
      </EndProperty>
      <EndProperty Name="Profile">
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

-- Creating table 'UserSet'
CREATE TABLE [dbo].[UserSet] (
    [Id] int  NOT NULL,
    [Username] varchar(100)  NULL,
    [IsModerator] bit  NULL
);
GO

-- Creating table 'ProfileSet'
CREATE TABLE [dbo].[ProfileSet] (
    [Id] int  NOT NULL,
    [ProfileText] varchar(max)  NULL,
    [User_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [PK_UserSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProfileSet'
ALTER TABLE [dbo].[ProfileSet]
ADD CONSTRAINT [PK_ProfileSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [User_Id] in table 'ProfileSet'
ALTER TABLE [dbo].[ProfileSet]
ADD CONSTRAINT [FK_UserProfile]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfile'
CREATE INDEX [IX_FK_UserProfile]
ON [dbo].[ProfileSet]
    ([User_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
