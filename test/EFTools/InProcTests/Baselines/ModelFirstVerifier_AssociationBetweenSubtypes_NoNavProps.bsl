<StorageAndMappings>
  <Schema Namespace="AssociationBetweenSubtypes.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="AssociationBetweenSubtypesStoreContainer">
    <EntitySet Name="StudentSet" EntityType="AssociationBetweenSubtypes.Store.StudentSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="BookSet" EntityType="AssociationBetweenSubtypes.Store.BookSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="StudentSet_CompSciStudent" EntityType="AssociationBetweenSubtypes.Store.StudentSet_CompSciStudent" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="BookSet_CompSciBook" EntityType="AssociationBetweenSubtypes.Store.BookSet_CompSciBook" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="CompSciStudentCompSciBook" Association="AssociationBetweenSubtypes.Store.CompSciStudentCompSciBook">
      <End Role="CompSciStudent" EntitySet="StudentSet_CompSciStudent" />
      <End Role="CompSciBook" EntitySet="BookSet_CompSciBook" />
    </AssociationSet>
    <AssociationSet Name="FK_CompSciStudent_inherits_Student" Association="AssociationBetweenSubtypes.Store.FK_CompSciStudent_inherits_Student">
      <End Role="Student" EntitySet="StudentSet" />
      <End Role="CompSciStudent" EntitySet="StudentSet_CompSciStudent" />
    </AssociationSet>
    <AssociationSet Name="FK_CompSciBook_inherits_Book" Association="AssociationBetweenSubtypes.Store.FK_CompSciBook_inherits_Book">
      <End Role="Book" EntitySet="BookSet" />
      <End Role="CompSciBook" EntitySet="BookSet_CompSciBook" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="StudentSet">
    <Key>
      <PropertyRef Name="PersonId" />
      <PropertyRef Name="Name" />
    </Key>
    <Property Name="PersonId" Type="int" Nullable="false" />
    <Property Name="Name" Type="varchar(max)" Nullable="false" />
    <Property Name="Address" Type="varchar(max)" Nullable="true" />
    <Property Name="Phone" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="BookSet">
    <Key>
      <PropertyRef Name="ISBN" />
    </Key>
    <Property Name="ISBN" Type="int" Nullable="false" />
    <Property Name="Title" Type="varchar(max)" Nullable="true" />
    <Property Name="Author" Type="varchar(max)" Nullable="true" />
    <Property Name="Description" Type="varchar(max)" Nullable="true" />
    <Property Name="Pages" Type="int" Nullable="true" />
    <Property Name="Cost" Type="decimal" Nullable="true" Precision="29" Scale="29" />
  </EntityType>
  <EntityType Name="StudentSet_CompSciStudent">
    <Key>
      <PropertyRef Name="PersonId" />
      <PropertyRef Name="Name" />
    </Key>
    <Property Name="HoursCoded" Type="int" Nullable="true" />
    <Property Name="NumProjects" Type="int" Nullable="true" />
    <Property Name="PersonId" Type="int" Nullable="false" />
    <Property Name="Name" Type="varchar(max)" Nullable="false" />
  </EntityType>
  <EntityType Name="BookSet_CompSciBook">
    <Key>
      <PropertyRef Name="ISBN" />
    </Key>
    <Property Name="ACMAward" Type="bit" Nullable="true" />
    <Property Name="NumCodingExercises" Type="int" Nullable="true" />
    <Property Name="ISBN" Type="int" Nullable="false" />
    <Property Name="CompSciStudentCompSciBook_CompSciBook_PersonId" Type="int" Nullable="false" />
    <Property Name="CompSciStudentCompSciBook_CompSciBook_Name" Type="varchar(max)" Nullable="false" />
  </EntityType>
  <Association Name="CompSciStudentCompSciBook">
    <End Role="CompSciStudent" Type="AssociationBetweenSubtypes.Store.StudentSet_CompSciStudent" Multiplicity="1" />
    <End Role="CompSciBook" Type="AssociationBetweenSubtypes.Store.BookSet_CompSciBook" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="CompSciStudent">
        <PropertyRef Name="PersonId" />
        <PropertyRef Name="Name" />
      </Principal>
      <Dependent Role="CompSciBook">
        <PropertyRef Name="CompSciStudentCompSciBook_CompSciBook_PersonId" />
        <PropertyRef Name="CompSciStudentCompSciBook_CompSciBook_Name" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_CompSciStudent_inherits_Student">
    <End Role="Student" Type="AssociationBetweenSubtypes.Store.StudentSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="CompSciStudent" Type="AssociationBetweenSubtypes.Store.StudentSet_CompSciStudent" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Student">
        <PropertyRef Name="PersonId" />
        <PropertyRef Name="Name" />
      </Principal>
      <Dependent Role="CompSciStudent">
        <PropertyRef Name="PersonId" />
        <PropertyRef Name="Name" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_CompSciBook_inherits_Book">
    <End Role="Book" Type="AssociationBetweenSubtypes.Store.BookSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="CompSciBook" Type="AssociationBetweenSubtypes.Store.BookSet_CompSciBook" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Book">
        <PropertyRef Name="ISBN" />
      </Principal>
      <Dependent Role="CompSciBook">
        <PropertyRef Name="ISBN" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="AssociationBetweenSubtypesStoreContainer" CdmEntityContainer="AssociationBetweenSubtypesContainer">
    <EntitySetMapping Name="StudentSet">
      <EntityTypeMapping TypeName="IsTypeOf(AssociationBetweenSubtypes.Student)">
        <MappingFragment StoreEntitySet="StudentSet">
          <ScalarProperty Name="PersonId" ColumnName="PersonId" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(AssociationBetweenSubtypes.CompSciStudent)">
        <MappingFragment StoreEntitySet="StudentSet_CompSciStudent">
          <ScalarProperty Name="PersonId" ColumnName="PersonId" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="HoursCoded" ColumnName="HoursCoded" />
          <ScalarProperty Name="NumProjects" ColumnName="NumProjects" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="BookSet">
      <EntityTypeMapping TypeName="IsTypeOf(AssociationBetweenSubtypes.Book)">
        <MappingFragment StoreEntitySet="BookSet">
          <ScalarProperty Name="ISBN" ColumnName="ISBN" />
          <ScalarProperty Name="Title" ColumnName="Title" />
          <ScalarProperty Name="Author" ColumnName="Author" />
          <ScalarProperty Name="Description" ColumnName="Description" />
          <ScalarProperty Name="Pages" ColumnName="Pages" />
          <ScalarProperty Name="Cost" ColumnName="Cost" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(AssociationBetweenSubtypes.CompSciBook)">
        <MappingFragment StoreEntitySet="BookSet_CompSciBook">
          <ScalarProperty Name="ISBN" ColumnName="ISBN" />
          <ScalarProperty Name="ACMAward" ColumnName="ACMAward" />
          <ScalarProperty Name="NumCodingExercises" ColumnName="NumCodingExercises" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="CompSciStudentCompSciBook" TypeName="AssociationBetweenSubtypes.CompSciStudentCompSciBook" StoreEntitySet="BookSet_CompSciBook">
      <EndProperty Name="CompSciStudent">
        <ScalarProperty Name="PersonId" ColumnName="CompSciStudentCompSciBook_CompSciBook_PersonId" />
        <ScalarProperty Name="Name" ColumnName="CompSciStudentCompSciBook_CompSciBook_Name" />
      </EndProperty>
      <EndProperty Name="CompSciBook">
        <ScalarProperty Name="ISBN" ColumnName="ISBN" />
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

-- Creating table 'StudentSet'
CREATE TABLE [dbo].[StudentSet] (
    [PersonId] int  NOT NULL,
    [Name] varchar(max)  NOT NULL,
    [Address] varchar(max)  NULL,
    [Phone] int  NULL
);
GO

-- Creating table 'BookSet'
CREATE TABLE [dbo].[BookSet] (
    [ISBN] int  NOT NULL,
    [Title] varchar(max)  NULL,
    [Author] varchar(max)  NULL,
    [Description] varchar(max)  NULL,
    [Pages] int  NULL,
    [Cost] decimal(29,29)  NULL
);
GO

-- Creating table 'StudentSet_CompSciStudent'
CREATE TABLE [dbo].[StudentSet_CompSciStudent] (
    [HoursCoded] int  NULL,
    [NumProjects] int  NULL,
    [PersonId] int  NOT NULL,
    [Name] varchar(max)  NOT NULL
);
GO

-- Creating table 'BookSet_CompSciBook'
CREATE TABLE [dbo].[BookSet_CompSciBook] (
    [ACMAward] bit  NULL,
    [NumCodingExercises] int  NULL,
    [ISBN] int  NOT NULL,
    [CompSciStudentCompSciBook_CompSciBook_PersonId] int  NOT NULL,
    [CompSciStudentCompSciBook_CompSciBook_Name] varchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [PersonId], [Name] in table 'StudentSet'
ALTER TABLE [dbo].[StudentSet]
ADD CONSTRAINT [PK_StudentSet]
    PRIMARY KEY CLUSTERED ([PersonId], [Name] ASC);
GO

-- Creating primary key on [ISBN] in table 'BookSet'
ALTER TABLE [dbo].[BookSet]
ADD CONSTRAINT [PK_BookSet]
    PRIMARY KEY CLUSTERED ([ISBN] ASC);
GO

-- Creating primary key on [PersonId], [Name] in table 'StudentSet_CompSciStudent'
ALTER TABLE [dbo].[StudentSet_CompSciStudent]
ADD CONSTRAINT [PK_StudentSet_CompSciStudent]
    PRIMARY KEY CLUSTERED ([PersonId], [Name] ASC);
GO

-- Creating primary key on [ISBN] in table 'BookSet_CompSciBook'
ALTER TABLE [dbo].[BookSet_CompSciBook]
ADD CONSTRAINT [PK_BookSet_CompSciBook]
    PRIMARY KEY CLUSTERED ([ISBN] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CompSciStudentCompSciBook_CompSciBook_PersonId], [CompSciStudentCompSciBook_CompSciBook_Name] in table 'BookSet_CompSciBook'
ALTER TABLE [dbo].[BookSet_CompSciBook]
ADD CONSTRAINT [FK_CompSciStudentCompSciBook]
    FOREIGN KEY ([CompSciStudentCompSciBook_CompSciBook_PersonId], [CompSciStudentCompSciBook_CompSciBook_Name])
    REFERENCES [dbo].[StudentSet_CompSciStudent]
        ([PersonId], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CompSciStudentCompSciBook'
CREATE INDEX [IX_FK_CompSciStudentCompSciBook]
ON [dbo].[BookSet_CompSciBook]
    ([CompSciStudentCompSciBook_CompSciBook_PersonId], [CompSciStudentCompSciBook_CompSciBook_Name]);
GO

-- Creating foreign key on [PersonId], [Name] in table 'StudentSet_CompSciStudent'
ALTER TABLE [dbo].[StudentSet_CompSciStudent]
ADD CONSTRAINT [FK_CompSciStudent_inherits_Student]
    FOREIGN KEY ([PersonId], [Name])
    REFERENCES [dbo].[StudentSet]
        ([PersonId], [Name])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ISBN] in table 'BookSet_CompSciBook'
ALTER TABLE [dbo].[BookSet_CompSciBook]
ADD CONSTRAINT [FK_CompSciBook_inherits_Book]
    FOREIGN KEY ([ISBN])
    REFERENCES [dbo].[BookSet]
        ([ISBN])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
