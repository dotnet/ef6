<?xml version="1.0" encoding="utf-8"?>
<Dsl dslVersion="1.0.0.0" Id="eaf6b23c-b622-40ae-9e7e-91cf0cb73708" Description="ADO.NET Entity Framework Designer" Name="MicrosoftDataEntityDesign" DisplayName="Microsoft Data Entity Design" Namespace="Microsoft.Data.Entity.Design.EntityDesigner" ProductName="Microsoft Data Entity Design" CompanyName="Microsoft Corporation" PackageGuid="8889e051-b7f9-4781-bb33-2a36a9bdb3a5" PackageNamespace="Microsoft.Data.Entity.Design.Package" xmlns="http://schemas.microsoft.com/VisualStudio/2005/DslTools/DslDefinitionModel">
  <Classes>
    <DomainClass Id="1f4b5cb5-f13b-48d2-8dbe-a9cc4bf162f4" Description="" Name="NameableItem" DisplayName="Nameable Item" InheritanceModifier="Abstract" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <Attributes>
        <ClrAttribute Name="System.ComponentModel.TypeDescriptionProvider">
          <Parameters>
            <AttributeParameter Value="typeof(NameableItemDescriptionProvider)" />
          </Parameters>
        </ClrAttribute>
      </Attributes>
      <Properties>
        <DomainProperty Id="2bb18667-db4e-48b5-b050-ba1f2f4f5c9a" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.NameableItem.Name" Name="Name" DisplayName="Name" IsElementName="true" IsBrowsable="false">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
          <ElementNameProvider>
            <ExternalTypeMoniker Name="NameableItemNameProvider" />
          </ElementNameProvider>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="a460641b-c69f-4e0f-8da2-fe3a96845fac" Description="" Name="EntityDesignerViewModel" DisplayName="Entity Designer View Model" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <Properties>
        <DomainProperty Id="4c4e5500-1eae-47ee-80ff-1045f13834d7" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityDesignerViewModel.Namespace" Name="Namespace" DisplayName="Namespace" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="EntityType" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>Microsoft.Data.Entity.Design.EntityDesigner.EntityDesignerViewModelHasEntityTypes.EntityTypes</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="4b6e96fa-a795-440e-b657-dc1828d259c9" Description="" Name="EntityType" DisplayName="Entity Type" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel" GeneratesDoubleDerived="true">
      <BaseClass>
        <DomainClassMoniker Name="NameableItem" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="ad7054fd-2b49-4f29-9a4e-0263fdd5d711" Description="Computed name of the base type. Empty if no base type" Name="BaseTypeName" DisplayName="Base Type Name" Kind="Calculated" SetterAccessModifier="Assembly" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="3838f73b-98b7-47be-936d-d11f2aba997a" Description="Indicates if this has a base type" Name="HasBaseType" DisplayName="Has Base Type" Kind="Calculated" SetterAccessModifier="Assembly" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/Boolean" />
          </Type>
        </DomainProperty>
      </Properties>
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Property" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>EntityTypeHasProperties.Properties</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="NavigationProperty" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>EntityTypeHasNavigationProperties.NavigationProperties</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="b88c8fdf-39e9-42fc-9ee9-43d55223619a" Description="" Name="Property" DisplayName="Property" InheritanceModifier="Abstract" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <BaseClass>
        <DomainClassMoniker Name="PropertyBase" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="81852ed4-2a73-4509-9803-244a7abd6bd0" Description="Specifies the EDM type of this property. The property window only displays facets specific to this type." Name="Type" DisplayName="Type" DefaultValue="String" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="57498f30-3645-4c7a-8e7a-f501e83da1b5" Description="" Name="PropertyBase" DisplayName="Property Base" InheritanceModifier="Abstract" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <BaseClass>
        <DomainClassMoniker Name="NameableItem" />
      </BaseClass>
    </DomainClass>
    <DomainClass Id="1eb61091-eaa5-4256-a931-b71c42b85a0c" Description="" Name="NavigationProperty" DisplayName="Navigation Property" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <BaseClass>
        <DomainClassMoniker Name="PropertyBase" />
      </BaseClass>
    </DomainClass>
    <DomainClass Id="ffb5ef4e-3b9a-4e6f-9c26-67b8f2e1f13d" Description="" Name="ComplexProperty" DisplayName="Complex Property" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <BaseClass>
        <DomainClassMoniker Name="Property" />
      </BaseClass>
    </DomainClass>
    <DomainClass Id="c4075739-14d5-40af-9cd8-0b3c73dacd4f" Description="" Name="ScalarProperty" DisplayName="Property" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <BaseClass>
        <DomainClassMoniker Name="Property" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="a07effa9-80a6-452f-af52-e4f369f0735f" Description="" Name="EntityKey" DisplayName="Entity Key" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/Boolean" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
  </Classes>
  <Relationships>
    <DomainRelationship Id="c48c6a47-40b2-4e85-a889-b0df52a37f3e" Description="" Name="EntityTypeHasProperties" DisplayName="Entity Type Has Properties" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel" IsEmbedding="true">
      <Source>
        <DomainRole Id="30e974ac-5354-45d4-8045-8b725fd65ecf" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasProperties.EntityType" Name="EntityType" DisplayName="Entity Type" PropertyName="Properties" IsPropertyBrowsable="false" PropertyDisplayName="Properties">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="9b6fd932-cfcc-47a7-8108-15500f9dd1fc" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasProperties.Property" Name="Property" DisplayName="Property" PropertyName="EntityType" Multiplicity="One" PropagatesDelete="true" PropagatesCopy="true" IsPropertyBrowsable="false" PropertyDisplayName="Entity Type">
          <RolePlayer>
            <DomainClassMoniker Name="Property" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="e3036261-bd10-4be9-8121-53e95b66e582" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.EntityDesignerViewModelHasEntityTypes" Name="EntityDesignerViewModelHasEntityTypes" DisplayName="Entity Designer View Model Has Entity Types" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner" IsEmbedding="true">
      <Source>
        <DomainRole Id="dd4caca7-8da5-48eb-ba33-b006b2393f22" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.EntityDesignerViewModelHasEntityTypes.EntityDesignerViewModel" Name="EntityDesignerViewModel" DisplayName="Entity Designer View Model" PropertyName="EntityTypes" IsPropertyBrowsable="false" PropertyDisplayName="Entity Types">
          <RolePlayer>
            <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityDesignerViewModel" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="fe5a70fc-12dd-438d-813b-8fe2ad58567f" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.EntityDesignerViewModelHasEntityTypes.EntityType" Name="EntityType" DisplayName="Entity Type" PropertyName="EntityDesignerViewModel" Multiplicity="One" PropagatesDelete="true" PropagatesCopy="true" IsPropertyBrowsable="false" PropertyDisplayName="Entity Designer View Model">
          <RolePlayer>
            <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="f9cc6d75-f51d-447a-bd83-fba56fed8b44" Description="" Name="Association" DisplayName="Association" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel" AllowsDuplicates="true">
      <Attributes>
        <ClrAttribute Name="System.ComponentModel.TypeDescriptionProvider">
          <Parameters>
            <AttributeParameter Value="typeof(AssociationDescriptionProvider)" />
          </Parameters>
        </ClrAttribute>
      </Attributes>
      <Properties>
        <DomainProperty Id="39e88f59-c849-4810-8b9e-b9e0798d979b" Description="Name" Name="Name" DisplayName="Name" IsElementName="true" IsBrowsable="false">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="cb259944-6001-4696-9ea7-19a804f2b2c1" Description="Computed source display text" Name="SourceMultiplicity" DisplayName="Source Multiplicity" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="141f87f4-9156-4a6f-a5c8-7f64f4df69c8" Description="Computed target display text" Name="TargetMultiplicity" DisplayName="Target Multiplicity" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
      <Source>
        <DomainRole Id="b4225a71-8c58-4bde-82a7-1ebdfb15a373" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Association.SourceEntityType" Name="SourceEntityType" DisplayName="Source Entity Type" PropertyName="AssociationTargets" IsPropertyBrowsable="false" PropertyDisplayName="Association Targets">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="45245da1-b4fc-4879-8a59-c225e233bdfe" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Association.TargetEntityType" Name="TargetEntityType" DisplayName="Target Entity Type" PropertyName="AssociationSources" IsPropertyBrowsable="false" PropertyDisplayName="Association Sources">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="81514557-7408-48c9-b3cd-cee5d27efe6e" Description="" Name="EntityTypeHasNavigationProperties" DisplayName="Entity Type Has Navigation Properties" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel" IsEmbedding="true">
      <Source>
        <DomainRole Id="573dadad-1cf6-491b-a674-00cf97910bbd" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasNavigationProperties.EntityType" Name="EntityType" DisplayName="Entity Type" PropertyName="NavigationProperties" IsPropertyBrowsable="false" PropertyDisplayName="Navigation Properties">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="7309dfe7-8356-46f3-952e-165009efafe6" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasNavigationProperties.NavigationProperty" Name="NavigationProperty" DisplayName="Navigation Property" PropertyName="EntityType" Multiplicity="One" PropagatesDelete="true" PropagatesCopy="true" IsPropertyBrowsable="false" PropertyDisplayName="Entity Type">
          <RolePlayer>
            <DomainClassMoniker Name="NavigationProperty" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="9f7eeb7a-4385-41d3-8c3e-c99fe852eb38" Description="" Name="Inheritance" DisplayName="Inheritance" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel">
      <Source>
        <DomainRole Id="ce879f7d-559d-48a6-814f-3157a4c39bbc" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Inheritance.SourceEntityType" Name="SourceEntityType" DisplayName="Source Entity Type" PropertyName="DerivedTypes" IsPropertyBrowsable="false" PropertyDisplayName="Derived Types">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="1483ad20-8271-4cb4-b7c5-a0a21e13a69d" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Inheritance.TargetEntityType" Name="TargetEntityType" DisplayName="Target Entity Type" PropertyName="BaseType" Multiplicity="ZeroOne" IsPropertyBrowsable="false" PropertyDisplayName="Base Type">
          <RolePlayer>
            <DomainClassMoniker Name="EntityType" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
  </Relationships>
  <Types>
    <ExternalType Name="DateTime" Namespace="System" />
    <ExternalType Name="String" Namespace="System" />
    <ExternalType Name="Int16" Namespace="System" />
    <ExternalType Name="Int32" Namespace="System" />
    <ExternalType Name="Int64" Namespace="System" />
    <ExternalType Name="UInt16" Namespace="System" />
    <ExternalType Name="UInt32" Namespace="System" />
    <ExternalType Name="UInt64" Namespace="System" />
    <ExternalType Name="SByte" Namespace="System" />
    <ExternalType Name="Byte" Namespace="System" />
    <ExternalType Name="Double" Namespace="System" />
    <ExternalType Name="Single" Namespace="System" />
    <ExternalType Name="Guid" Namespace="System" />
    <ExternalType Name="Boolean" Namespace="System" />
    <ExternalType Name="Char" Namespace="System" />
    <ExternalType Name="DateTimeKind" Namespace="System" />
    <ExternalType Name="NameableItemNameProvider" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.ViewModel" />
    <ExternalType Name="Color" Namespace="System.Drawing" />
  </Types>
  <Shapes>
    <CompartmentShape Id="b1385551-bc3f-4f60-8722-75d686a8f8f6" Description="" Name="EntityTypeShape" DisplayName="Entity Type Shape" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.View" GeneratesDoubleDerived="true" FixedTooltipText="Entity Type Shape" FillColor="0, 122, 204" OutlineColor="24, 143, 222" InitialHeight="0.4" OutlineThickness="0.01" FillGradientMode="None" ExposesFillColorAsProperty="true" Geometry="Rectangle">
      <Properties>
        <DomainProperty Id="590355a5-1c6e-4dc8-af7f-51209c2fa259" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.View.EntityTypeShape.Fill Color" Name="FillColor" DisplayName="Fill Color" Kind="CustomStorage">
          <Type>
            <ExternalTypeMoniker Name="/System.Drawing/Color" />
          </Type>
        </DomainProperty>
      </Properties>
      <ShapeHasDecorators Position="InnerTopRight" HorizontalOffset="0" VerticalOffset="0">
        <ExpandCollapseDecorator Name="ExpandCollapse" DisplayName="Expand Collapse" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="InnerTopLeft" HorizontalOffset="0.4" VerticalOffset="0.125">
        <TextDecorator Name="BaseTypeName" DisplayName="Base Type Name" DefaultText="BaseTypeName" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="InnerTopLeft" HorizontalOffset="0" VerticalOffset="0">
        <IconDecorator Name="IconDecorator" DisplayName="Icon Decorator" DefaultIcon="Resources\EntityTool.gif" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="InnerTopLeft" HorizontalOffset="0.25" VerticalOffset="0">
        <TextDecorator Name="Name" DisplayName="Name" DefaultText="Name" FontStyle="Bold" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="InnerTopLeft" HorizontalOffset="0.25" VerticalOffset="0.125">
        <IconDecorator Name="BaseTypeIconDecorator" DisplayName="Base Type Icon Decorator" DefaultIcon="Resources\BaseTypeIcon.bmp" />
      </ShapeHasDecorators>
      <Compartment TitleFillColor="Gainsboro" Name="Properties" Title="Properties" FillColor="WhiteSmoke" />
      <Compartment TitleFillColor="Gainsboro" Name="Navigation" Title="Navigation Properties" FillColor="WhiteSmoke" />
    </CompartmentShape>
  </Shapes>
  <Connectors>
    <Connector Id="bc29ae42-ae95-434c-8b7c-166482d358f9" Description="" Name="InheritanceConnector" DisplayName="Inheritance Connector" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.View" TooltipType="Variable" FixedTooltipText="Inheritance Connector" Color="LightSlateGray" SourceEndStyle="HollowArrow" Thickness="0.01" />
    <Connector Id="9239a0fb-b26c-4db1-8324-f7c1a75e7499" Description="" Name="AssociationConnector" DisplayName="Association Connector" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.View" TooltipType="Variable" FixedTooltipText="Association Connector" Color="LightSlateGray" DashStyle="Dash" SourceEndStyle="EmptyDiamond" TargetEndStyle="EmptyDiamond" Thickness="0.003" GeneratesDoubleDerived="true">
      <ConnectorHasDecorators Position="SourceBottom" OffsetFromShape="0" OffsetFromLine="0">
        <TextDecorator Name="SourceEndDisplayText" DisplayName="Source End Display Text" DefaultText="SourceEndDisplayText" FontStyle="Bold" />
      </ConnectorHasDecorators>
      <ConnectorHasDecorators Position="TargetBottom" OffsetFromShape="0" OffsetFromLine="0">
        <TextDecorator Name="TargetEndDisplayText" DisplayName="Target End Display Text" DefaultText="TargetEndDisplayText" FontStyle="Bold" />
      </ConnectorHasDecorators>
    </Connector>
  </Connectors>
  <XmlSerializationBehavior Name="MicrosoftDataEntityDesignSerializationBehavior" Namespace="Microsoft.Data.Entity.Design.EntityDesigner">
    <ClassData>
      <XmlClassData TypeName="EntityDesignerDiagram" MonikerAttributeName="" MonikerElementName="entityDesignerDiagramMoniker" ElementName="EntityDesignerDiagram" MonikerTypeName="EntityDesignerDiagramMoniker">
        <DiagramMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/EntityDesignerDiagram" />
        <ElementData>
          <XmlPropertyData XmlName="title">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/EntityDesignerDiagram/Title" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="NameableItem" MonikerAttributeName="" MonikerElementName="nameableItemMoniker" ElementName="nameableItem" MonikerTypeName="NameableItemMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NameableItem" />
        <ElementData>
          <XmlPropertyData XmlName="name">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NameableItem/Name" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityDesignerViewModel" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityDesignerViewModelMoniker" ElementName="entityDesignerViewModel" MonikerTypeName="EntityDesignerViewModelMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityDesignerViewModel" />
        <ElementData>
          <XmlRelationshipData RoleElementName="entityTypes">
            <DomainRelationshipMoniker Name="EntityDesignerViewModelHasEntityTypes" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="namespace">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityDesignerViewModel/Namespace" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityType" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityTypeMoniker" ElementName="entityType" MonikerTypeName="EntityTypeMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
        <ElementData>
          <XmlRelationshipData RoleElementName="properties">
            <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityTypeHasProperties" />
          </XmlRelationshipData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="associationTargets">
            <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association" />
          </XmlRelationshipData>
          <XmlRelationshipData RoleElementName="navigationProperties">
            <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityTypeHasNavigationProperties" />
          </XmlRelationshipData>
          <XmlRelationshipData RoleElementName="derivedTypes">
            <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Inheritance" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="baseTypeName" Representation="Ignore">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType/BaseTypeName" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="hasBaseType" Representation="Ignore">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType/HasBaseType" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Property" MonikerAttributeName="" MonikerElementName="propertyMoniker" ElementName="property" MonikerTypeName="PropertyMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Property" />
        <ElementData>
          <XmlPropertyData XmlName="type">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Property/Type" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityTypeHasProperties" MonikerAttributeName="" MonikerElementName="entityTypeHasPropertiesMoniker" ElementName="entityTypeHasProperties" MonikerTypeName="EntityTypeHasPropertiesMoniker">
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityTypeHasProperties" />
      </XmlClassData>
      <XmlClassData TypeName="EntityDesignerViewModelHasEntityTypes" MonikerAttributeName="" MonikerElementName="entityDesignerViewModelHasEntityTypesMoniker" ElementName="entityDesignerViewModelHasEntityTypes" MonikerTypeName="EntityDesignerViewModelHasEntityTypesMoniker">
        <DomainRelationshipMoniker Name="EntityDesignerViewModelHasEntityTypes" />
      </XmlClassData>
      <XmlClassData TypeName="PropertyBase" MonikerAttributeName="" MonikerElementName="propertyBaseMoniker" ElementName="propertyBase" MonikerTypeName="PropertyBaseMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/PropertyBase" />
      </XmlClassData>
      <XmlClassData TypeName="NavigationProperty" MonikerAttributeName="" MonikerElementName="navigationPropertyMoniker" ElementName="navigationProperty" MonikerTypeName="NavigationPropertyMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NavigationProperty" />
      </XmlClassData>
      <XmlClassData TypeName="Association" MonikerAttributeName="" SerializeId="true" MonikerElementName="associationMoniker" ElementName="association" MonikerTypeName="AssociationMoniker">
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association" />
        <ElementData>
          <XmlPropertyData XmlName="name">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association/Name" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="sourceMultiplicity">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association/SourceMultiplicity" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="targetMultiplicity">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association/TargetMultiplicity" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityTypeHasNavigationProperties" MonikerAttributeName="" MonikerElementName="entityTypeHasNavigationPropertiesMoniker" ElementName="entityTypeHasNavigationProperties" MonikerTypeName="EntityTypeHasNavigationPropertiesMoniker">
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityTypeHasNavigationProperties" />
      </XmlClassData>
      <XmlClassData TypeName="Inheritance" MonikerAttributeName="" MonikerElementName="inheritanceMoniker" ElementName="inheritance" MonikerTypeName="InheritanceMoniker">
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Inheritance" />
      </XmlClassData>
      <XmlClassData TypeName="EntityTypeShape" MonikerAttributeName="" MonikerElementName="entityTypeShapeMoniker" ElementName="entityTypeShape" MonikerTypeName="EntityTypeShapeMoniker">
        <CompartmentShapeMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/EntityTypeShape" />
        <ElementData>
          <XmlPropertyData XmlName="fillColor">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/EntityTypeShape/FillColor" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="InheritanceConnector" MonikerAttributeName="" MonikerElementName="inheritanceConnectorMoniker" ElementName="inheritanceConnector" MonikerTypeName="InheritanceConnectorMoniker">
        <ConnectorMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/InheritanceConnector" />
      </XmlClassData>
      <XmlClassData TypeName="AssociationConnector" MonikerAttributeName="" MonikerElementName="associationConnectorMoniker" ElementName="associationConnector" MonikerTypeName="AssociationConnectorMoniker">
        <ConnectorMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/AssociationConnector" />
      </XmlClassData>
      <XmlClassData TypeName="ComplexProperty" MonikerAttributeName="" MonikerElementName="complexPropertyMoniker" ElementName="complexProperty" MonikerTypeName="ComplexPropertyMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/ComplexProperty" />
      </XmlClassData>
      <XmlClassData TypeName="ScalarProperty" MonikerAttributeName="" MonikerElementName="scalarPropertyMoniker" ElementName="scalarProperty" MonikerTypeName="ScalarPropertyMoniker">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/ScalarProperty" />
        <ElementData>
          <XmlPropertyData XmlName="entityKey">
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/ScalarProperty/EntityKey" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
    </ClassData>
  </XmlSerializationBehavior>
  <ConnectionBuilders>
    <ConnectionBuilder Name="AssociationBuilder">
      <LinkConnectDirective>
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association" />
        <SourceDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </SourceDirectives>
        <TargetDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </TargetDirectives>
      </LinkConnectDirective>
    </ConnectionBuilder>
    <ConnectionBuilder Name="InheritanceBuilder">
      <LinkConnectDirective>
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Inheritance" />
        <SourceDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </SourceDirectives>
        <TargetDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </TargetDirectives>
      </LinkConnectDirective>
    </ConnectionBuilder>
  </ConnectionBuilders>
  <Diagram Id="3daa738d-a8aa-4f0c-b98b-db1382985264" Description="" Name="EntityDesignerDiagram" DisplayName="ADO.NET Entity Designer" AccessModifier="Assembly" Namespace="Microsoft.Data.Entity.Design.EntityDesigner.View" GeneratesDoubleDerived="true" FillColor="WhiteSmoke" >
    <Properties>
      <DomainProperty Id="83fe8dc3-d17f-471c-8bf7-128799399af3" Description="Description for Microsoft.Data.Entity.Design.EntityDesigner.View.EntityDesignerDiagram.Title" Name="Title" DisplayName="Title">
        <Type>
          <ExternalTypeMoniker Name="/System/String" />
        </Type>
      </DomainProperty>
    </Properties>
    <Class>
      <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityDesignerViewModel" />
    </Class>
    <ShapeMaps>
      <CompartmentShapeMap>
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
        <ParentElementPath>
          <DomainPath>Microsoft.Data.Entity.Design.EntityDesigner.EntityDesignerViewModelHasEntityTypes.EntityDesignerViewModel/!EntityDesignerViewModel</DomainPath>
        </ParentElementPath>
        <DecoratorMap>
          <IconDecoratorMoniker Name="EntityTypeShape/BaseTypeIconDecorator" />
          <VisibilityPropertyPath>
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType/HasBaseType" />
            <PropertyFilters>
              <PropertyFilter FilteringValue="True" />
            </PropertyFilters>
          </VisibilityPropertyPath>
        </DecoratorMap>
        <DecoratorMap>
          <TextDecoratorMoniker Name="EntityTypeShape/BaseTypeName" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType/BaseTypeName" />
            </PropertyPath>
          </PropertyDisplayed>
          <VisibilityPropertyPath>
            <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType/HasBaseType" />
            <PropertyFilters>
              <PropertyFilter FilteringValue="True" />
            </PropertyFilters>
          </VisibilityPropertyPath>
        </DecoratorMap>
        <DecoratorMap>
          <TextDecoratorMoniker Name="EntityTypeShape/Name" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NameableItem/Name" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <CompartmentShapeMoniker Name="EntityTypeShape" />
        <CompartmentMap>
          <CompartmentMoniker Name="EntityTypeShape/Navigation" />
          <ElementsDisplayed>
            <DomainPath>Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasNavigationProperties.NavigationProperties/!NavigationProperty</DomainPath>
          </ElementsDisplayed>
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NameableItem/Name" />
            </PropertyPath>
          </PropertyDisplayed>
        </CompartmentMap>
        <CompartmentMap DisplaysCustomString="true">
          <CompartmentMoniker Name="EntityTypeShape/Properties" />
          <ElementsDisplayed>
            <DomainPath>Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityTypeHasProperties.Properties/!Property</DomainPath>
          </ElementsDisplayed>
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/NameableItem/Name" />
            </PropertyPath>
          </PropertyDisplayed>
        </CompartmentMap>
      </CompartmentShapeMap>
    </ShapeMaps>
    <ConnectorMaps>
      <ConnectorMap>
        <ConnectorMoniker Name="AssociationConnector" />
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association" />
        <DecoratorMap>
          <TextDecoratorMoniker Name="AssociationConnector/SourceEndDisplayText" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association/SourceMultiplicity" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <DecoratorMap>
          <TextDecoratorMoniker Name="AssociationConnector/TargetEndDisplayText" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Association/TargetMultiplicity" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
      </ConnectorMap>
      <ConnectorMap>
        <ConnectorMoniker Name="InheritanceConnector" />
        <DomainRelationshipMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/Inheritance" />
      </ConnectorMap>
    </ConnectorMaps>
  </Diagram>
  <Designer FileExtension="edmx" EditorGuid="c99aea30-8e36-4515-b76f-496f5a48a6aa">
    <RootClass>
      <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityDesignerViewModel" />
    </RootClass>
    <XmlSerializationDefinition CustomPostLoad="false">
      <XmlSerializationBehaviorMoniker Name="MicrosoftDataEntityDesignSerializationBehavior" />
    </XmlSerializationDefinition>
    <ToolboxTab TabText="Entity Framework">
      <ElementTool Name="EntityTool" ToolboxIcon="Resources\EntityTool.bmp" Caption="Entity" Tooltip="Create a new Entity">
        <DomainClassMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.ViewModel/EntityType" />
      </ElementTool>
      <ConnectionTool Name="AssociationTool" ToolboxIcon="Resources\AssociationTool.bmp" Caption="Association" Tooltip="Create an Association between two Entities" SourceCursorIcon="Resources\ConnectorSourceSearch.cur" TargetCursorIcon="Resources\ConnectorTargetSearch.cur">
        <ConnectionBuilderMoniker Name="MicrosoftDataEntityDesign/AssociationBuilder" />
      </ConnectionTool>
      <ConnectionTool Name="InheritanceTool" ToolboxIcon="Resources\InheritanceTool.bmp" Caption="Inheritance" Tooltip="Create an Inheritance relationship between two Entities" ReversesDirection="true" SourceCursorIcon="Resources\ConnectorSourceSearch.cur" TargetCursorIcon="Resources\ConnectorTargetSearch.cur">
        <ConnectionBuilderMoniker Name="MicrosoftDataEntityDesign/InheritanceBuilder" />
      </ConnectionTool>
    </ToolboxTab>
    <Validation UsesMenu="false" UsesOpen="true" UsesSave="true" UsesCustom="true" UsesLoad="true" />
    <DiagramMoniker Name="/Microsoft.Data.Entity.Design.EntityDesigner.View/EntityDesignerDiagram" />
  </Designer>
</Dsl>