<StorageAndMappings>
  <Schema Namespace="Model1.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2009/02/edm/ssdl">
  <EntityContainer Name="Model1StoreContainer">
    <EntitySet Name="EmployeeSet" EntityType="Model1.Store.EmployeeSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="EmployeeManagers" EntityType="Model1.Store.EmployeeManagers" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_EmployeeManagers_Employee" Association="Model1.Store.FK_EmployeeManagers_Employee">
      <End Role="Employee" EntitySet="EmployeeSet" />
      <End Role="EmployeeManagers" EntitySet="EmployeeManagers" />
    </AssociationSet>
    <AssociationSet Name="FK_EmployeeManagers_Manager" Association="Model1.Store.FK_EmployeeManagers_Manager">
      <End Role="Manager" EntitySet="EmployeeSet" />
      <End Role="EmployeeManagers" EntitySet="EmployeeManagers" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="EmployeeSet">
    <Key>
      <PropertyRef Name="Id" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="EmployeeManagers">
    <Key>
      <PropertyRef Name="Managers_Id" />
      <PropertyRef Name="Employees_Id" />
    </Key>
    <Property Name="Managers_Id" Type="int" Nullable="false" />
    <Property Name="Employees_Id" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="FK_EmployeeManagers_Employee">
    <End Role="Employee" Type="Model1.Store.EmployeeSet" Multiplicity="1" />
    <End Role="EmployeeManagers" Type="Model1.Store.EmployeeManagers" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Employee">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="EmployeeManagers">
        <PropertyRef Name="Managers_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_EmployeeManagers_Manager">
    <End Role="EmployeeManagers" Type="Model1.Store.EmployeeManagers" Multiplicity="*" />
    <End Role="Manager" Type="Model1.Store.EmployeeSet" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="Manager">
        <PropertyRef Name="Id" />
      </Principal>
      <Dependent Role="EmployeeManagers">
        <PropertyRef Name="Employees_Id" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2008/09/mapping/cs">
  <EntityContainerMapping StorageEntityContainer="Model1StoreContainer" CdmEntityContainer="Model1Container">
    <EntitySetMapping Name="EmployeeSet">
      <EntityTypeMapping TypeName="IsTypeOf(Model1.Employee)">
        <MappingFragment StoreEntitySet="EmployeeSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="EmployeeManagers" TypeName="Model1.EmployeeManagers" StoreEntitySet="EmployeeManagers">
      <EndProperty Name="Employee">
        <ScalarProperty Name="Id" ColumnName="Managers_Id" />
      </EndProperty>
      <EndProperty Name="Manager">
        <ScalarProperty Name="Id" ColumnName="Employees_Id" />
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

-- Creating table 'EmployeeSet'
CREATE TABLE [dbo].[EmployeeSet] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'EmployeeManagers'
CREATE TABLE [dbo].[EmployeeManagers] (
    [Managers_Id] int  NOT NULL,
    [Employees_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'EmployeeSet'
ALTER TABLE [dbo].[EmployeeSet]
ADD CONSTRAINT [PK_EmployeeSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Managers_Id], [Employees_Id] in table 'EmployeeManagers'
ALTER TABLE [dbo].[EmployeeManagers]
ADD CONSTRAINT [PK_EmployeeManagers]
    PRIMARY KEY CLUSTERED ([Managers_Id], [Employees_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Managers_Id] in table 'EmployeeManagers'
ALTER TABLE [dbo].[EmployeeManagers]
ADD CONSTRAINT [FK_EmployeeManagers_Employee]
    FOREIGN KEY ([Managers_Id])
    REFERENCES [dbo].[EmployeeSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Employees_Id] in table 'EmployeeManagers'
ALTER TABLE [dbo].[EmployeeManagers]
ADD CONSTRAINT [FK_EmployeeManagers_Manager]
    FOREIGN KEY ([Employees_Id])
    REFERENCES [dbo].[EmployeeSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeManagers_Manager'
CREATE INDEX [IX_FK_EmployeeManagers_Manager]
ON [dbo].[EmployeeManagers]
    ([Employees_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
