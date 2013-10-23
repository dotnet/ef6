<StorageAndMappings>
  <Schema Namespace="ComplexInheritanceHierarchy.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="ComplexInheritanceHierarchyStoreContainer">
    <EntitySet Name="CarPartSet" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_EnginePart" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_EnginePart" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_SafetyPart" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_SafetyPart" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_Airbag" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_Airbag" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_Brake" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_Brake" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_AntiLockBrake" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_AntiLockBrake" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_SparkPlug" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_SparkPlug" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_Turbocharger" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_Turbocharger" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_Valve" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_Valve" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CarPartSet_Supercharger" EntityType="ComplexInheritanceHierarchy.Store.CarPartSet_Supercharger" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_EnginePart_inherits_CarPart" Association="ComplexInheritanceHierarchy.Store.FK_EnginePart_inherits_CarPart">
      <End Role="CarPart" EntitySet="CarPartSet" />
      <End Role="EnginePart" EntitySet="CarPartSet_EnginePart" />
    </AssociationSet>
    <AssociationSet Name="FK_SafetyPart_inherits_CarPart" Association="ComplexInheritanceHierarchy.Store.FK_SafetyPart_inherits_CarPart">
      <End Role="CarPart" EntitySet="CarPartSet" />
      <End Role="SafetyPart" EntitySet="CarPartSet_SafetyPart" />
    </AssociationSet>
    <AssociationSet Name="FK_Airbag_inherits_SafetyPart" Association="ComplexInheritanceHierarchy.Store.FK_Airbag_inherits_SafetyPart">
      <End Role="SafetyPart" EntitySet="CarPartSet_SafetyPart" />
      <End Role="Airbag" EntitySet="CarPartSet_Airbag" />
    </AssociationSet>
    <AssociationSet Name="FK_Brake_inherits_SafetyPart" Association="ComplexInheritanceHierarchy.Store.FK_Brake_inherits_SafetyPart">
      <End Role="SafetyPart" EntitySet="CarPartSet_SafetyPart" />
      <End Role="Brake" EntitySet="CarPartSet_Brake" />
    </AssociationSet>
    <AssociationSet Name="FK_AntiLockBrake_inherits_Brake" Association="ComplexInheritanceHierarchy.Store.FK_AntiLockBrake_inherits_Brake">
      <End Role="Brake" EntitySet="CarPartSet_Brake" />
      <End Role="AntiLockBrake" EntitySet="CarPartSet_AntiLockBrake" />
    </AssociationSet>
    <AssociationSet Name="FK_SparkPlug_inherits_EnginePart" Association="ComplexInheritanceHierarchy.Store.FK_SparkPlug_inherits_EnginePart">
      <End Role="EnginePart" EntitySet="CarPartSet_EnginePart" />
      <End Role="SparkPlug" EntitySet="CarPartSet_SparkPlug" />
    </AssociationSet>
    <AssociationSet Name="FK_Turbocharger_inherits_EnginePart" Association="ComplexInheritanceHierarchy.Store.FK_Turbocharger_inherits_EnginePart">
      <End Role="EnginePart" EntitySet="CarPartSet_EnginePart" />
      <End Role="Turbocharger" EntitySet="CarPartSet_Turbocharger" />
    </AssociationSet>
    <AssociationSet Name="FK_Valve_inherits_EnginePart" Association="ComplexInheritanceHierarchy.Store.FK_Valve_inherits_EnginePart">
      <End Role="EnginePart" EntitySet="CarPartSet_EnginePart" />
      <End Role="Valve" EntitySet="CarPartSet_Valve" />
    </AssociationSet>
    <AssociationSet Name="FK_Supercharger_inherits_Turbocharger" Association="ComplexInheritanceHierarchy.Store.FK_Supercharger_inherits_Turbocharger">
      <End Role="Turbocharger" EntitySet="CarPartSet_Turbocharger" />
      <End Role="Supercharger" EntitySet="CarPartSet_Supercharger" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="CarPartSet">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
    <Property Name="Cost" Type="decimal" Nullable="true" Precision="29" Scale="4" />
    <Property Name="DateCreated" Type="datetime" Nullable="true" />
  </EntityType>
  <EntityType Name="CarPartSet_EnginePart">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="CompatibleCylinders" Type="uniqueidentifier" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_SafetyPart">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="Protects" Type="varchar" Nullable="true" MaxLength="100" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_Airbag">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="TimeToDeploy" Type="int" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_Brake">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="TimeToBrake" Type="int" Nullable="true" />
    <Property Name="BrakeMaterial" Type="varchar" Nullable="true" MaxLength="100" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_AntiLockBrake">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="RainTest" Type="bit" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_SparkPlug">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="IgnitionResponseTime" Type="int" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_Turbocharger">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="TurbineRadius" Type="int" Nullable="false" />
    <Property Name="MaxAirIntake" Type="float" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_Valve">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="CompressionLeakage" Type="tinyint" Nullable="true" />
    <Property Name="SealData" Type="varbinary(max)" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <EntityType Name="CarPartSet_Supercharger">
    <Key>
      <PropertyRef Name="Id" />
      <PropertyRef Name="Name" />
      <PropertyRef Name="Manufacturer" />
    </Key>
    <Property Name="PlacementOnEngine" Type="bigint" Nullable="true" />
    <Property Name="Id" Type="int" Nullable="false" />
    <Property Name="Name" Type="char" Nullable="false" MaxLength="4" />
    <Property Name="Manufacturer" Type="nvarchar" Nullable="false" MaxLength="100" />
  </EntityType>
  <Association Name="FK_EnginePart_inherits_CarPart">
    <End Role="CarPart" Type="ComplexInheritanceHierarchy.Store.CarPartSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="EnginePart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_EnginePart" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="CarPart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="EnginePart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_SafetyPart_inherits_CarPart">
    <End Role="CarPart" Type="ComplexInheritanceHierarchy.Store.CarPartSet" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="SafetyPart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_SafetyPart" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="CarPart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="SafetyPart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Airbag_inherits_SafetyPart">
    <End Role="SafetyPart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_SafetyPart" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Airbag" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Airbag" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="SafetyPart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="Airbag">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Brake_inherits_SafetyPart">
    <End Role="SafetyPart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_SafetyPart" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Brake" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Brake" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="SafetyPart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="Brake">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_AntiLockBrake_inherits_Brake">
    <End Role="Brake" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Brake" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="AntiLockBrake" Type="ComplexInheritanceHierarchy.Store.CarPartSet_AntiLockBrake" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Brake">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="AntiLockBrake">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_SparkPlug_inherits_EnginePart">
    <End Role="EnginePart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_EnginePart" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="SparkPlug" Type="ComplexInheritanceHierarchy.Store.CarPartSet_SparkPlug" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="EnginePart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="SparkPlug">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Turbocharger_inherits_EnginePart">
    <End Role="EnginePart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_EnginePart" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Turbocharger" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Turbocharger" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="EnginePart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="Turbocharger">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Valve_inherits_EnginePart">
    <End Role="EnginePart" Type="ComplexInheritanceHierarchy.Store.CarPartSet_EnginePart" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Valve" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Valve" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="EnginePart">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="Valve">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Supercharger_inherits_Turbocharger">
    <End Role="Turbocharger" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Turbocharger" Multiplicity="1">
      <OnDelete Action="Cascade" />
    </End>
    <End Role="Supercharger" Type="ComplexInheritanceHierarchy.Store.CarPartSet_Supercharger" Multiplicity="0..1" />
    <ReferentialConstraint>
      <Principal Role="Turbocharger">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Principal>
      <Dependent Role="Supercharger">
        <PropertyRef Name="Id" />
        <PropertyRef Name="Name" />
        <PropertyRef Name="Manufacturer" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="ComplexInheritanceHierarchyStoreContainer" CdmEntityContainer="ComplexInheritanceHierarchyContainer">
    <EntitySetMapping Name="CarPartSet">
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.CarPart)">
        <MappingFragment StoreEntitySet="CarPartSet">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="Cost" ColumnName="Cost" />
          <ScalarProperty Name="DateCreated" ColumnName="DateCreated" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.EnginePart)">
        <MappingFragment StoreEntitySet="CarPartSet_EnginePart">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="CompatibleCylinders" ColumnName="CompatibleCylinders" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.SafetyPart)">
        <MappingFragment StoreEntitySet="CarPartSet_SafetyPart">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="Protects" ColumnName="Protects" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.Airbag)">
        <MappingFragment StoreEntitySet="CarPartSet_Airbag">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="TimeToDeploy" ColumnName="TimeToDeploy" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.Brake)">
        <MappingFragment StoreEntitySet="CarPartSet_Brake">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="TimeToBrake" ColumnName="TimeToBrake" />
          <ScalarProperty Name="BrakeMaterial" ColumnName="BrakeMaterial" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.AntiLockBrake)">
        <MappingFragment StoreEntitySet="CarPartSet_AntiLockBrake">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="RainTest" ColumnName="RainTest" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.SparkPlug)">
        <MappingFragment StoreEntitySet="CarPartSet_SparkPlug">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="IgnitionResponseTime" ColumnName="IgnitionResponseTime" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.Turbocharger)">
        <MappingFragment StoreEntitySet="CarPartSet_Turbocharger">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="TurbineRadius" ColumnName="TurbineRadius" />
          <ScalarProperty Name="MaxAirIntake" ColumnName="MaxAirIntake" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.Valve)">
        <MappingFragment StoreEntitySet="CarPartSet_Valve">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="CompressionLeakage" ColumnName="CompressionLeakage" />
          <ScalarProperty Name="SealData" ColumnName="SealData" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName="IsTypeOf(ComplexInheritanceHierarchy.Supercharger)">
        <MappingFragment StoreEntitySet="CarPartSet_Supercharger">
          <ScalarProperty Name="Id" ColumnName="Id" />
          <ScalarProperty Name="Name" ColumnName="Name" />
          <ScalarProperty Name="Manufacturer" ColumnName="Manufacturer" />
          <ScalarProperty Name="PlacementOnEngine" ColumnName="PlacementOnEngine" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
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

-- Creating table 'CarPartSet'
CREATE TABLE [dbo].[CarPartSet] (
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL,
    [Cost] decimal(29,4)  NULL,
    [DateCreated] datetime  NULL
);
GO

-- Creating table 'CarPartSet_EnginePart'
CREATE TABLE [dbo].[CarPartSet_EnginePart] (
    [CompatibleCylinders] uniqueidentifier  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_SafetyPart'
CREATE TABLE [dbo].[CarPartSet_SafetyPart] (
    [Protects] varchar(100)  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_Airbag'
CREATE TABLE [dbo].[CarPartSet_Airbag] (
    [TimeToDeploy] int  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_Brake'
CREATE TABLE [dbo].[CarPartSet_Brake] (
    [TimeToBrake] int  NULL,
    [BrakeMaterial] varchar(100)  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_AntiLockBrake'
CREATE TABLE [dbo].[CarPartSet_AntiLockBrake] (
    [RainTest] bit  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_SparkPlug'
CREATE TABLE [dbo].[CarPartSet_SparkPlug] (
    [IgnitionResponseTime] int  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_Turbocharger'
CREATE TABLE [dbo].[CarPartSet_Turbocharger] (
    [TurbineRadius] int  NOT NULL,
    [MaxAirIntake] float  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_Valve'
CREATE TABLE [dbo].[CarPartSet_Valve] (
    [CompressionLeakage] tinyint  NULL,
    [SealData] varbinary(max)  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'CarPartSet_Supercharger'
CREATE TABLE [dbo].[CarPartSet_Supercharger] (
    [PlacementOnEngine] bigint  NULL,
    [Id] int  NOT NULL,
    [Name] char(4)  NOT NULL,
    [Manufacturer] nvarchar(100)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet'
ALTER TABLE [dbo].[CarPartSet]
ADD CONSTRAINT [PK_CarPartSet]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_EnginePart'
ALTER TABLE [dbo].[CarPartSet_EnginePart]
ADD CONSTRAINT [PK_CarPartSet_EnginePart]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_SafetyPart'
ALTER TABLE [dbo].[CarPartSet_SafetyPart]
ADD CONSTRAINT [PK_CarPartSet_SafetyPart]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Airbag'
ALTER TABLE [dbo].[CarPartSet_Airbag]
ADD CONSTRAINT [PK_CarPartSet_Airbag]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Brake'
ALTER TABLE [dbo].[CarPartSet_Brake]
ADD CONSTRAINT [PK_CarPartSet_Brake]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_AntiLockBrake'
ALTER TABLE [dbo].[CarPartSet_AntiLockBrake]
ADD CONSTRAINT [PK_CarPartSet_AntiLockBrake]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_SparkPlug'
ALTER TABLE [dbo].[CarPartSet_SparkPlug]
ADD CONSTRAINT [PK_CarPartSet_SparkPlug]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Turbocharger'
ALTER TABLE [dbo].[CarPartSet_Turbocharger]
ADD CONSTRAINT [PK_CarPartSet_Turbocharger]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Valve'
ALTER TABLE [dbo].[CarPartSet_Valve]
ADD CONSTRAINT [PK_CarPartSet_Valve]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- Creating primary key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Supercharger'
ALTER TABLE [dbo].[CarPartSet_Supercharger]
ADD CONSTRAINT [PK_CarPartSet_Supercharger]
    PRIMARY KEY CLUSTERED ([Id], [Name], [Manufacturer] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_EnginePart'
ALTER TABLE [dbo].[CarPartSet_EnginePart]
ADD CONSTRAINT [FK_EnginePart_inherits_CarPart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_SafetyPart'
ALTER TABLE [dbo].[CarPartSet_SafetyPart]
ADD CONSTRAINT [FK_SafetyPart_inherits_CarPart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Airbag'
ALTER TABLE [dbo].[CarPartSet_Airbag]
ADD CONSTRAINT [FK_Airbag_inherits_SafetyPart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_SafetyPart]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Brake'
ALTER TABLE [dbo].[CarPartSet_Brake]
ADD CONSTRAINT [FK_Brake_inherits_SafetyPart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_SafetyPart]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_AntiLockBrake'
ALTER TABLE [dbo].[CarPartSet_AntiLockBrake]
ADD CONSTRAINT [FK_AntiLockBrake_inherits_Brake]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_Brake]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_SparkPlug'
ALTER TABLE [dbo].[CarPartSet_SparkPlug]
ADD CONSTRAINT [FK_SparkPlug_inherits_EnginePart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_EnginePart]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Turbocharger'
ALTER TABLE [dbo].[CarPartSet_Turbocharger]
ADD CONSTRAINT [FK_Turbocharger_inherits_EnginePart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_EnginePart]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Valve'
ALTER TABLE [dbo].[CarPartSet_Valve]
ADD CONSTRAINT [FK_Valve_inherits_EnginePart]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_EnginePart]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id], [Name], [Manufacturer] in table 'CarPartSet_Supercharger'
ALTER TABLE [dbo].[CarPartSet_Supercharger]
ADD CONSTRAINT [FK_Supercharger_inherits_Turbocharger]
    FOREIGN KEY ([Id], [Name], [Manufacturer])
    REFERENCES [dbo].[CarPartSet_Turbocharger]
        ([Id], [Name], [Manufacturer])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
