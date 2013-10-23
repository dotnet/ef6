<StorageAndMappings>
  <Schema Namespace="SchoolDataLib.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="SchoolDataLibStoreContainer">
    <EntitySet Name="Departments" EntityType="SchoolDataLib.Store.Departments" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Departments_DeptBusiness" EntityType="SchoolDataLib.Store.Departments_DeptBusiness" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Departments_DeptEngineering" EntityType="SchoolDataLib.Store.Departments_DeptEngineering" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Departments_DeptMusic" EntityType="SchoolDataLib.Store.Departments_DeptMusic" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="People" EntityType="SchoolDataLib.Store.People" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="People_Student" EntityType="SchoolDataLib.Store.People_Student" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="People_Instructor" EntityType="SchoolDataLib.Store.People_Instructor" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="People_Administrator" EntityType="SchoolDataLib.Store.People_Administrator" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_Department_Administrator" Association="SchoolDataLib.Store.FK_Department_Administrator">
      <End Role="Person" EntitySet="People" />
      <End Role="Department" EntitySet="Departments" />
    </AssociationSet>
    <AssociationSet Name="FK_DeptBusiness_inherits_Department" Association="SchoolDataLib.Store.FK_DeptBusiness_inherits_Department">
      <End Role="Department" EntitySet="Departments" />
      <End Role="DeptBusiness" EntitySet="Departments_DeptBusiness" />
    </AssociationSet>
    <AssociationSet Name="FK_DeptEngineering_inherits_Department" Association="SchoolDataLib.Store.FK_DeptEngineering_inherits_Department">
      <End Role="Department" EntitySet="Departments" />
      <End Role="DeptEngineering" EntitySet="Departments_DeptEngineering" />
    </AssociationSet>
    <AssociationSet Name="FK_DeptMusic_inherits_Department" Association="SchoolDataLib.Store.FK_DeptMusic_inherits_Department">
      <End Role="Department" EntitySet="Departments" />
      <End Role="DeptMusic" EntitySet="Departments_DeptMusic" />
    </AssociationSet>
    <AssociationSet Name="FK_Student_inherits_Person" Association="SchoolDataLib.Store.FK_Student_inherits_Person">
      <End Role="Person" EntitySet="People" />
      <End Role="Student" EntitySet="People_Student" />
    </AssociationSet>
    <AssociationSet Name="FK_Instructor_inherits_Person" Association="SchoolDataLib.Store.FK_Instructor_inherits_Person">
      <End Role="Person" EntitySet="People" />
      <End Role="Instructor" EntitySet="People_Instructor" />
    </AssociationSet>
    <AssociationSet Name="FK_Administrator_inherits_Person" Association="SchoolDataLib.Store.FK_Administrator_inherits_Person">
      <End Role="Person" EntitySet="People" />
      <End Role="Administrator" EntitySet="People_Administrator" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="Departments">
    <Key>
      <PropertyRef Name="DepartmentID" />
    </Key>
    <Property Name="DepartmentID" Type="int" Nullable="false" />
    <Property Name="Name" Type="nvarchar(max)" Nullable="false" />
    <Property Name="Budget" Type="decimal" Nullable="false" />
    <Property Name="StartDate" Type="datetime" Nullable="false" />
    <Property Name="Administrator_PersonID" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="Departments_DeptBusiness">
    <Key>
      <PropertyRef Name="DepartmentID" />
    </Key>
    <Property Name="LegalBudget" Type="decimal" Nullable="false" />
    <Property Name="AccountingBudget" Type="decimal" Nullable="false" />
    <Property Name="DepartmentID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="Departments_DeptEngineering">
    <Key>
      <PropertyRef Name="DepartmentID" />
    </Key>
    <Property Name="FiberOpticsBudget" Type="decimal" Nullable="false" />
    <Property Name="LabBudget" Type="decimal" Nullable="false" />
    <Property Name="DepartmentID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="Departments_DeptMusic">
    <Key>
      <PropertyRef Name="DepartmentID" />
    </Key>
    <Property Name="TheaterBudget" Type="decimal" Nullable="false" />
    <Property Name="InstrumentBudget" Type="decimal" Nullable="false" />
    <Property Name="DepartmentID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="People">
    <Key>
      <PropertyRef Name="PersonID" />
    </Key>
    <Property Name="PersonID" Type="int" Nullable="false" />
    <Property Name="FirstName" Type="nvarchar(max)" Nullable="false" />
    <Property Name="LastName" Type="nvarchar(max)" Nullable="false" />
  </EntityType>
  <EntityType Name="People_Student">
    <Key>
      <PropertyRef Name="PersonID" />
    </Key>
    <Property Name="EnrollmentDate" Type="datetime" Nullable="true" />
    <Property Name="PersonID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="People_Instructor">
    <Key>
      <PropertyRef Name="PersonID" />
    </Key>
    <Property Name="HireDate" Type="datetime" Nullable="true" />
    <Property Name="PersonID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="People_Administrator">
    <Key>
      <PropertyRef Name="PersonID" />
    </Key>
    <Property Name="AdminDate" Type="datetime" Nullable="true" />
    <Property Name="PersonID" Type="int" Nullable="false" />
  </EntityType>
  <Association Name="FK_Department_Administrator">
    <End Role="Person" Type="SchoolDataLib.Store.People" Multiplicity="0..1" />
    <End Role="Department" Type="SchoolDataLib.Store.Departments" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Person">
        <PropertyRef Name="PersonID" />
      </Principal>
      <Dependent Role="Department">
        <PropertyRef Name="Administrator_PersonID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_DeptBusiness_inherits_Department">
    <End Role="Department" Type="SchoolDataLib.Store.Departments" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="DeptBusiness" Type="SchoolDataLib.Store.Departments_DeptBusiness" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Department">
        <PropertyRef Name="DepartmentID" />
      </Principal>
      <Dependent Role="DeptBusiness">
        <PropertyRef Name="DepartmentID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_DeptEngineering_inherits_Department">
    <End Role="Department" Type="SchoolDataLib.Store.Departments" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="DeptEngineering" Type="SchoolDataLib.Store.Departments_DeptEngineering" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Department">
        <PropertyRef Name="DepartmentID" />
      </Principal>
      <Dependent Role="DeptEngineering">
        <PropertyRef Name="DepartmentID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_DeptMusic_inherits_Department">
    <End Role="Department" Type="SchoolDataLib.Store.Departments" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="DeptMusic" Type="SchoolDataLib.Store.Departments_DeptMusic" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Department">
        <PropertyRef Name="DepartmentID" />
      </Principal>
      <Dependent Role="DeptMusic">
        <PropertyRef Name="DepartmentID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Student_inherits_Person">
    <End Role="Person" Type="SchoolDataLib.Store.People" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Student" Type="SchoolDataLib.Store.People_Student" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Person">
        <PropertyRef Name="PersonID" />
      </Principal>
      <Dependent Role="Student">
        <PropertyRef Name="PersonID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Instructor_inherits_Person">
    <End Role="Person" Type="SchoolDataLib.Store.People" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Instructor" Type="SchoolDataLib.Store.People_Instructor" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Person">
        <PropertyRef Name="PersonID" />
      </Principal>
      <Dependent Role="Instructor">
        <PropertyRef Name="PersonID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Administrator_inherits_Person">
    <End Role="Person" Type="SchoolDataLib.Store.People" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Administrator" Type="SchoolDataLib.Store.People_Administrator" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Person">
        <PropertyRef Name="PersonID" />
      </Principal>
      <Dependent Role="Administrator">
        <PropertyRef Name="PersonID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="SchoolDataLibStoreContainer" CdmEntityContainer="SchoolDataLibContainer">
    <EntitySetMapping Name="Departments">
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.Department)">
        <MappingFragment StoreEntitySet="Departments">
          <ScalarProperty Name="DepartmentID" ColumnName="DepartmentID" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Budget" ColumnName="Budget" />
          <ScalarProperty Name="StartDate" ColumnName="StartDate" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.DeptBusiness)">
        <MappingFragment StoreEntitySet="Departments_DeptBusiness">
          <ScalarProperty Name="DepartmentID" ColumnName="DepartmentID" />
          <ScalarProperty Name="LegalBudget" ColumnName="LegalBudget" />
          <ScalarProperty Name="AccountingBudget" ColumnName="AccountingBudget" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.DeptEngineering)">
        <MappingFragment StoreEntitySet="Departments_DeptEngineering">
          <ScalarProperty Name="DepartmentID" ColumnName="DepartmentID" />
          <ScalarProperty Name="FiberOpticsBudget" ColumnName="FiberOpticsBudget" />
          <ScalarProperty Name="LabBudget" ColumnName="LabBudget" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.DeptMusic)">
        <MappingFragment StoreEntitySet="Departments_DeptMusic">
          <ScalarProperty Name="DepartmentID" ColumnName="DepartmentID" />
          <ScalarProperty Name="TheaterBudget" ColumnName="TheaterBudget" />
          <ScalarProperty Name="InstrumentBudget" ColumnName="InstrumentBudget" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="People">
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.Person)">
        <MappingFragment StoreEntitySet="People">
          <ScalarProperty Name="PersonID" ColumnName="PersonID" />
          <ScalarProperty Name="FirstName" ColumnName="FirstName" />
          <ScalarProperty Name="LastName" ColumnName="LastName" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.Student)">
        <MappingFragment StoreEntitySet="People_Student">
          <ScalarProperty Name="PersonID" ColumnName="PersonID" />
          <ScalarProperty Name="EnrollmentDate" ColumnName="EnrollmentDate" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.Instructor)">
        <MappingFragment StoreEntitySet="People_Instructor">
          <ScalarProperty Name="PersonID" ColumnName="PersonID" />
          <ScalarProperty Name="HireDate" ColumnName="HireDate" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(SchoolDataLib.Administrator)">
        <MappingFragment StoreEntitySet="People_Administrator">
          <ScalarProperty Name="PersonID" ColumnName="PersonID" />
          <ScalarProperty Name="AdminDate" ColumnName="AdminDate" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="FK_Department_Administrator" TypeName="SchoolDataLib.FK_Department_Administrator" StoreEntitySet="Departments">
      <EndProperty Name="Person">
        <ScalarProperty Name="PersonID" ColumnName="Administrator_PersonID" />
      </EndProperty>
      <EndProperty Name="Department">
        <ScalarProperty Name="DepartmentID" ColumnName="DepartmentID" />
      </EndProperty>
      <Condition ColumnName="Administrator_PersonID" IsNull="false" />
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

-- Creating table 'Departments'
CREATE TABLE [dbo].[Departments] (
    [DepartmentID] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Budget] decimal(18,0)  NOT NULL,
    [StartDate] datetime  NOT NULL,
    [Administrator_PersonID] int  NULL
);
GO

-- Creating table 'Departments_DeptBusiness'
CREATE TABLE [dbo].[Departments_DeptBusiness] (
    [LegalBudget] decimal(18,0)  NOT NULL,
    [AccountingBudget] decimal(18,0)  NOT NULL,
    [DepartmentID] int  NOT NULL
);
GO

-- Creating table 'Departments_DeptEngineering'
CREATE TABLE [dbo].[Departments_DeptEngineering] (
    [FiberOpticsBudget] decimal(18,0)  NOT NULL,
    [LabBudget] decimal(18,0)  NOT NULL,
    [DepartmentID] int  NOT NULL
);
GO

-- Creating table 'Departments_DeptMusic'
CREATE TABLE [dbo].[Departments_DeptMusic] (
    [TheaterBudget] decimal(18,0)  NOT NULL,
    [InstrumentBudget] decimal(18,0)  NOT NULL,
    [DepartmentID] int  NOT NULL
);
GO

-- Creating table 'People'
CREATE TABLE [dbo].[People] (
    [PersonID] int  NOT NULL,
    [FirstName] nvarchar(max)  NOT NULL,
    [LastName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'People_Student'
CREATE TABLE [dbo].[People_Student] (
    [EnrollmentDate] datetime  NULL,
    [PersonID] int  NOT NULL
);
GO

-- Creating table 'People_Instructor'
CREATE TABLE [dbo].[People_Instructor] (
    [HireDate] datetime  NULL,
    [PersonID] int  NOT NULL
);
GO

-- Creating table 'People_Administrator'
CREATE TABLE [dbo].[People_Administrator] (
    [AdminDate] datetime  NULL,
    [PersonID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [DepartmentID] in table 'Departments'
ALTER TABLE [dbo].[Departments]
ADD CONSTRAINT [PK_Departments]
    PRIMARY KEY CLUSTERED ([DepartmentID] ASC);
GO

-- Creating primary key on [DepartmentID] in table 'Departments_DeptBusiness'
ALTER TABLE [dbo].[Departments_DeptBusiness]
ADD CONSTRAINT [PK_Departments_DeptBusiness]
    PRIMARY KEY CLUSTERED ([DepartmentID] ASC);
GO

-- Creating primary key on [DepartmentID] in table 'Departments_DeptEngineering'
ALTER TABLE [dbo].[Departments_DeptEngineering]
ADD CONSTRAINT [PK_Departments_DeptEngineering]
    PRIMARY KEY CLUSTERED ([DepartmentID] ASC);
GO

-- Creating primary key on [DepartmentID] in table 'Departments_DeptMusic'
ALTER TABLE [dbo].[Departments_DeptMusic]
ADD CONSTRAINT [PK_Departments_DeptMusic]
    PRIMARY KEY CLUSTERED ([DepartmentID] ASC);
GO

-- Creating primary key on [PersonID] in table 'People'
ALTER TABLE [dbo].[People]
ADD CONSTRAINT [PK_People]
    PRIMARY KEY CLUSTERED ([PersonID] ASC);
GO

-- Creating primary key on [PersonID] in table 'People_Student'
ALTER TABLE [dbo].[People_Student]
ADD CONSTRAINT [PK_People_Student]
    PRIMARY KEY CLUSTERED ([PersonID] ASC);
GO

-- Creating primary key on [PersonID] in table 'People_Instructor'
ALTER TABLE [dbo].[People_Instructor]
ADD CONSTRAINT [PK_People_Instructor]
    PRIMARY KEY CLUSTERED ([PersonID] ASC);
GO

-- Creating primary key on [PersonID] in table 'People_Administrator'
ALTER TABLE [dbo].[People_Administrator]
ADD CONSTRAINT [PK_People_Administrator]
    PRIMARY KEY CLUSTERED ([PersonID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Administrator_PersonID] in table 'Departments'
ALTER TABLE [dbo].[Departments]
ADD CONSTRAINT [FK_Department_Administrator]
    FOREIGN KEY ([Administrator_PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Department_Administrator'
CREATE INDEX [IX_FK_Department_Administrator]
ON [dbo].[Departments]
    ([Administrator_PersonID]);
GO

-- Creating foreign key on [DepartmentID] in table 'Departments_DeptBusiness'
ALTER TABLE [dbo].[Departments_DeptBusiness]
ADD CONSTRAINT [FK_DeptBusiness_inherits_Department]
    FOREIGN KEY ([DepartmentID])
    REFERENCES [dbo].[Departments]
        ([DepartmentID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [DepartmentID] in table 'Departments_DeptEngineering'
ALTER TABLE [dbo].[Departments_DeptEngineering]
ADD CONSTRAINT [FK_DeptEngineering_inherits_Department]
    FOREIGN KEY ([DepartmentID])
    REFERENCES [dbo].[Departments]
        ([DepartmentID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [DepartmentID] in table 'Departments_DeptMusic'
ALTER TABLE [dbo].[Departments_DeptMusic]
ADD CONSTRAINT [FK_DeptMusic_inherits_Department]
    FOREIGN KEY ([DepartmentID])
    REFERENCES [dbo].[Departments]
        ([DepartmentID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PersonID] in table 'People_Student'
ALTER TABLE [dbo].[People_Student]
ADD CONSTRAINT [FK_Student_inherits_Person]
    FOREIGN KEY ([PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PersonID] in table 'People_Instructor'
ALTER TABLE [dbo].[People_Instructor]
ADD CONSTRAINT [FK_Instructor_inherits_Person]
    FOREIGN KEY ([PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PersonID] in table 'People_Administrator'
ALTER TABLE [dbo].[People_Administrator]
ADD CONSTRAINT [FK_Administrator_inherits_Person]
    FOREIGN KEY ([PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
