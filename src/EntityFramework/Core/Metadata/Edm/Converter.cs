// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.SchemaObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Helper Class for converting SOM objects to metadata objects
    ///     This class should go away once we have completely integrated SOM and metadata
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class Converter
    {
        /// <summary>
        ///     Static constructor for creating FacetDescription objects that we use
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Converter()
        {
            Debug.Assert(Enum.GetUnderlyingType(typeof(ConcurrencyMode)) == typeof(int), "Please update underlying type below accordingly.");

            // Create the enum types that we will need
            var concurrencyModeType = new EnumType(
                EdmProviderManifest.ConcurrencyModeFacetName,
                EdmConstants.EdmNamespace,
                underlyingType: PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                isFlags: false,
                dataSpace: DataSpace.CSpace);

            foreach (var name in Enum.GetNames(typeof(ConcurrencyMode)))
            {
                concurrencyModeType.AddMember(
                    new EnumMember(
                        name,
                        (int)Enum.Parse(typeof(ConcurrencyMode), name, false)));
            }

            Debug.Assert(
                Enum.GetUnderlyingType(typeof(StoreGeneratedPattern)) == typeof(int), "Please update underlying type below accordingly.");

            var storeGeneratedPatternType = new EnumType(
                EdmProviderManifest.StoreGeneratedPatternFacetName,
                EdmConstants.EdmNamespace,
                underlyingType: PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                isFlags: false,
                dataSpace: DataSpace.CSpace);

            foreach (var name in Enum.GetNames(typeof(StoreGeneratedPattern)))
            {
                storeGeneratedPatternType.AddMember(
                    new EnumMember(
                        name,
                        (int)Enum.Parse(typeof(StoreGeneratedPattern), name, false)));
            }

            // Now create the facet description objects
            ConcurrencyModeFacet = new FacetDescription(
                EdmProviderManifest.ConcurrencyModeFacetName,
                concurrencyModeType,
                null,
                null,
                ConcurrencyMode.None);
            StoreGeneratedPatternFacet = new FacetDescription(
                EdmProviderManifest.StoreGeneratedPatternFacetName,
                storeGeneratedPatternType,
                null,
                null,
                StoreGeneratedPattern.None);
            CollationFacet = new FacetDescription(
                DbProviderManifest.CollationFacetName,
                MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String),
                null,
                null,
                string.Empty);
        }

        internal static readonly FacetDescription ConcurrencyModeFacet;
        internal static readonly FacetDescription StoreGeneratedPatternFacet;
        internal static readonly FacetDescription CollationFacet;

        /// <summary>
        ///     Converts a schema from SOM into Metadata
        /// </summary>
        /// <param name="somSchema"> The SOM schema to convert </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="itemCollection"> The item collection for currently existing metadata objects </param>
        internal static IEnumerable<GlobalItem> ConvertSchema(
            Schema somSchema,
            DbProviderManifest providerManifest,
            ItemCollection itemCollection)
        {
            var newGlobalItems = new Dictionary<SchemaElement, GlobalItem>();
            ConvertSchema(somSchema, providerManifest, new ConversionCache(itemCollection), newGlobalItems);
            return newGlobalItems.Values;
        }

        internal static IEnumerable<GlobalItem> ConvertSchema(
            IList<Schema> somSchemas,
            DbProviderManifest providerManifest,
            ItemCollection itemCollection)
        {
            var newGlobalItems = new Dictionary<SchemaElement, GlobalItem>();
            var conversionCache = new ConversionCache(itemCollection);

            foreach (var somSchema in somSchemas)
            {
                ConvertSchema(somSchema, providerManifest, conversionCache, newGlobalItems);
            }

            return newGlobalItems.Values;
        }

        private static void ConvertSchema(
            Schema somSchema, DbProviderManifest providerManifest,
            ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            var funcsWithUnresolvedTypes = new List<Function>();
            foreach (var element in somSchema.SchemaTypes)
            {
                if (null == LoadSchemaElement(element, providerManifest, convertedItemCache, newGlobalItems))
                {
                    var function = element as Function;
                    if (function != null)
                    {
                        funcsWithUnresolvedTypes.Add(function);
                    }
                }
            }

            foreach (var element in somSchema.SchemaTypes.OfType<SchemaEntityType>())
            {
                LoadEntityTypePhase2(element, providerManifest, convertedItemCache, newGlobalItems);
            }

            foreach (var function in funcsWithUnresolvedTypes)
            {
                if (null == LoadSchemaElement(function, providerManifest, convertedItemCache, newGlobalItems))
                {
                    Debug.Fail("Could not load model function definition"); //this should never happen.
                }
            }

            if (convertedItemCache.ItemCollection.DataSpace
                == DataSpace.CSpace)
            {
                var edmCollection = (EdmItemCollection)convertedItemCache.ItemCollection;
                edmCollection.EdmVersion = somSchema.SchemaVersion;
            }
            else
            {
                Debug.Assert(convertedItemCache.ItemCollection.DataSpace == DataSpace.SSpace, "Did you add a new space?");
                // when converting the ProviderManifest, the DataSpace is SSpace, but the ItemCollection is EmptyItemCollection, 
                // not StoreItemCollection
                var storeCollection = convertedItemCache.ItemCollection as StoreItemCollection;
                if (storeCollection != null)
                {
                    storeCollection.StoreSchemaVersion = somSchema.SchemaVersion;
                }
            }
        }

        /// <summary>
        ///     Loads a schema element
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The item resulting from the load </returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static MetadataItem LoadSchemaElement(
            SchemaType element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            DebugCheck.NotNull(providerManifest);
            // Try to fetch from the collection first
            GlobalItem item;

            Debug.Assert(
                !convertedItemCache.ItemCollection.TryGetValue(element.FQName, false, out item),
                "Som should have checked for duplicate items");

            // Try to fetch in our collection of new GlobalItems
            if (newGlobalItems.TryGetValue(element, out item))
            {
                return item;
            }

            var entityContainer = element as SchemaObjectModel.EntityContainer;
            // Perform different conversion depending on the type of the SOM object
            if (entityContainer != null)
            {
                item = ConvertToEntityContainer(
                    entityContainer,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
            }
            else if (element is SchemaEntityType)
            {
                item = ConvertToEntityType(
                    (SchemaEntityType)element,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
            }
            else if (element is Relationship)
            {
                item = ConvertToAssociationType(
                    (Relationship)element,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
            }
            else if (element is SchemaComplexType)
            {
                item = ConvertToComplexType(
                    (SchemaComplexType)element,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
            }
            else if (element is Function)
            {
                item = ConvertToFunction(
                    (Function)element, providerManifest,
                    convertedItemCache, null, newGlobalItems);
            }
            else if (element is SchemaEnumType)
            {
                item = ConvertToEnumType((SchemaEnumType)element, newGlobalItems);
            }
            else
            {
                // the only type we don't handle is the ProviderManifest TypeElement
                // if it is anything else, it is probably a mistake
                Debug.Assert(
                    element is TypeElement &&
                    element.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel,
                    "Unknown Type in somschema");
                return null;
            }

            return item;
        }

        /// <summary>
        ///     Converts an entity container from SOM to metadata
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The entity container object resulting from the convert </returns>
        private static EntityContainer ConvertToEntityContainer(
            SchemaObjectModel.EntityContainer element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            // Creating a new entity container object and populate with converted entity set objects
            var entityContainer = new EntityContainer(element.Name, GetDataSpace(providerManifest));
            newGlobalItems.Add(element, entityContainer);

            foreach (var entitySet in element.EntitySets)
            {
                entityContainer.AddEntitySetBase(
                    ConvertToEntitySet(
                        entitySet,
                        providerManifest,
                        convertedItemCache,
                        newGlobalItems));
            }

            // Populate with converted relationship set objects
            foreach (var relationshipSet in element.RelationshipSets)
            {
                Debug.Assert(
                    relationshipSet.Relationship.RelationshipKind == RelationshipKind.Association,
                    "We do not support containment set");

                entityContainer.AddEntitySetBase(
                    ConvertToAssociationSet(
                        relationshipSet,
                        providerManifest,
                        convertedItemCache,
                        entityContainer,
                        newGlobalItems));
            }

            // Populate with converted function imports
            foreach (var functionImport in element.FunctionImports)
            {
                entityContainer.AddFunctionImport(
                    ConvertToFunction(
                        functionImport,
                        providerManifest, convertedItemCache, entityContainer, newGlobalItems));
            }

            // Extract the optional Documentation
            if (element.Documentation != null)
            {
                entityContainer.Documentation = ConvertToDocumentation(element.Documentation);
            }

            AddOtherContent(element, entityContainer);

            return entityContainer;
        }

        /// <summary>
        ///     Converts an entity type from SOM to metadata
        ///     This method should only build the internally contained and vertical part of the EntityType (keys, properties, and base types) but not
        ///     sideways parts (NavigationProperties) that go between types or we risk trying to access and EntityTypes keys, from the referential constraint,
        ///     before the base type, which has the keys, is setup yet.
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The entity type object resulting from the convert </returns>
        private static EntityType ConvertToEntityType(
            SchemaEntityType element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            string[] keyMembers = null;
            // Check if this type has keys
            if (element.DeclaredKeyProperties.Count != 0)
            {
                keyMembers = new string[element.DeclaredKeyProperties.Count];
                for (var i = 0; i < keyMembers.Length; i++)
                {
                    //Add the name of the key property to the list of
                    //key properties
                    keyMembers[i] = (element.DeclaredKeyProperties[i].Property.Name);
                }
            }

            var properties = new EdmProperty[element.Properties.Count];
            var index = 0;

            foreach (var somProperty in element.Properties)
            {
                properties[index++] = ConvertToProperty(
                    somProperty,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
            }

            var entityType = new EntityType(
                element.Name,
                element.Namespace,
                GetDataSpace(providerManifest),
                keyMembers,
                properties);

            if (element.BaseType != null)
            {
                entityType.BaseType = (EdmType)(LoadSchemaElement(
                    element.BaseType,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems));
            }

            // set the abstract and sealed type values for the entity type
            entityType.Abstract = element.IsAbstract;
            // Extract the optional Documentation
            if (element.Documentation != null)
            {
                entityType.Documentation = ConvertToDocumentation(element.Documentation);
            }
            AddOtherContent(element, entityType);
            newGlobalItems.Add(element, entityType);
            return entityType;
        }

        private static void LoadEntityTypePhase2(
            SchemaEntityType element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            var entityType = (EntityType)newGlobalItems[element];

            // Since Navigation properties are internal and not part of member collection, we
            // need to initialize the base class first before we start adding the navigation property
            // this will ensure that all the base navigation properties are initialized
            foreach (var somNavigationProperty in element.NavigationProperties)
            {
                entityType.AddMember(
                    ConvertToNavigationProperty(
                        entityType,
                        somNavigationProperty,
                        providerManifest,
                        convertedItemCache,
                        newGlobalItems));
            }
        }

        /// <summary>
        ///     Converts an complex type from SOM to metadata
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The complex type object resulting from the convert </returns>
        private static ComplexType ConvertToComplexType(
            SchemaComplexType element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            var complexType = new ComplexType(
                element.Name,
                element.Namespace,
                GetDataSpace(providerManifest));
            newGlobalItems.Add(element, complexType);

            foreach (var somProperty in element.Properties)
            {
                complexType.AddMember(
                    ConvertToProperty(
                        somProperty,
                        providerManifest,
                        convertedItemCache,
                        newGlobalItems));
            }

            // set the abstract and sealed type values for the entity type
            complexType.Abstract = element.IsAbstract;

            if (element.BaseType != null)
            {
                complexType.BaseType = (EdmType)(LoadSchemaElement(
                    element.BaseType,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems));
            }

            // Extract the optional Documentation
            if (element.Documentation != null)
            {
                complexType.Documentation = ConvertToDocumentation(element.Documentation);
            }
            AddOtherContent(element, complexType);

            return complexType;
        }

        /// <summary>
        ///     Converts an association type from SOM to metadata
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The association type object resulting from the convert </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static AssociationType ConvertToAssociationType(
            Relationship element,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            Debug.Assert(element.RelationshipKind == RelationshipKind.Association);

            var associationType = new AssociationType(
                element.Name,
                element.Namespace,
                element.IsForeignKey,
                GetDataSpace(providerManifest));
            newGlobalItems.Add(element, associationType);

            foreach (RelationshipEnd end in element.Ends)
            {
                SchemaType entityTypeElement = end.Type;
                var endEntityType = (EntityType)LoadSchemaElement(
                    entityTypeElement,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);

                var endMember = InitializeAssociationEndMember(associationType, end, endEntityType);
                AddOtherContent(end, endMember);
                // Loop through and convert the operations
                foreach (var operation in end.Operations)
                {
                    // Process only the ones that we recognize
                    if (operation.Operation
                        != Operation.Delete)
                    {
                        continue;
                    }

                    // Determine the action for this operation
                    var action = OperationAction.None;
                    switch (operation.Action)
                    {
                        case Action.Cascade:
                            action = OperationAction.Cascade;
                            break;
                        case Action.None:
                            action = OperationAction.None;
                            break;
                        default:
                            Debug.Fail("Operation action not supported.");
                            break;
                    }
                    endMember.DeleteBehavior = action;
                }

                // Extract optional Documentation from the end element
                if (end.Documentation != null)
                {
                    endMember.Documentation = ConvertToDocumentation(end.Documentation);
                }
            }

            Debug.Assert(associationType.ReferentialConstraints.Count == 0, "This must never have been initialized");

            for (var i = 0; i < element.Constraints.Count; i++)
            {
                var constraint = element.Constraints[i];
                var fromMember = (AssociationEndMember)associationType.Members[constraint.PrincipalRole.Name];
                var toMember = (AssociationEndMember)associationType.Members[constraint.DependentRole.Name];
                var fromEntityType = ((RefType)fromMember.TypeUsage.EdmType).ElementType;
                var toEntityType = ((RefType)toMember.TypeUsage.EdmType).ElementType;

                var referentialConstraint = new ReferentialConstraint(
                    fromMember, toMember,
                    GetProperties(fromEntityType, constraint.PrincipalRole.RoleProperties),
                    GetProperties(toEntityType, constraint.DependentRole.RoleProperties));

                // Attach the optional Documentation
                if (constraint.Documentation != null)
                {
                    referentialConstraint.Documentation = ConvertToDocumentation(constraint.Documentation);
                }
                if (constraint.PrincipalRole.Documentation != null)
                {
                    referentialConstraint.FromRole.Documentation = ConvertToDocumentation(constraint.PrincipalRole.Documentation);
                }
                if (constraint.DependentRole.Documentation != null)
                {
                    referentialConstraint.ToRole.Documentation = ConvertToDocumentation(constraint.DependentRole.Documentation);
                }

                associationType.AddReferentialConstraint(referentialConstraint);
                AddOtherContent(element.Constraints[i], referentialConstraint);
            }

            // Extract the optional Documentation
            if (element.Documentation != null)
            {
                associationType.Documentation = ConvertToDocumentation(element.Documentation);
            }
            AddOtherContent(element, associationType);

            return associationType;
        }

        /// <summary>
        ///     Initialize the end member if its not initialized already
        /// </summary>
        /// <param name="associationType"> </param>
        /// <param name="end"> </param>
        /// <param name="endMemberType"> </param>
        private static AssociationEndMember InitializeAssociationEndMember(
            AssociationType associationType, IRelationshipEnd end,
            EntityType endMemberType)
        {
            AssociationEndMember associationEnd;

            EdmMember member;
            // make sure that the end is not initialized as of yet
            if (!associationType.Members.TryGetValue(end.Name, false /*ignoreCase*/, out member))
            {
                // Create the end member and add the operations
                associationEnd = new AssociationEndMember(
                    end.Name,
                    endMemberType.GetReferenceType(),
                    end.Multiplicity.Value);
                associationType.AddKeyMember(associationEnd);
            }
            else
            {
                associationEnd = (AssociationEndMember)member;
            }

            //Extract the optional Documentation
            var relationshipEnd = end as RelationshipEnd;

            if (relationshipEnd != null
                && (relationshipEnd.Documentation != null))
            {
                associationEnd.Documentation = ConvertToDocumentation(relationshipEnd.Documentation);
            }

            return associationEnd;
        }

        private static EdmProperty[] GetProperties(EntityTypeBase entityType, IList<PropertyRefElement> properties)
        {
            Debug.Assert(properties.Count != 0);
            var result = new EdmProperty[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                result[i] = (EdmProperty)entityType.Members[properties[i].Name];
            }

            return result;
        }

        private static void AddOtherContent(SchemaElement element, MetadataItem item)
        {
            if (element.OtherContent.Count > 0)
            {
                item.AddMetadataProperties(element.OtherContent);
            }
        }

        /// <summary>
        ///     Converts an entity set from SOM to metadata
        /// </summary>
        /// <param name="set"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The entity set object resulting from the convert </returns>
        private static EntitySet ConvertToEntitySet(
            EntityContainerEntitySet set,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            var entitySet = new EntitySet(
                set.Name, set.DbSchema, set.Table, set.DefiningQuery,
                (EntityType)LoadSchemaElement(
                    set.EntityType,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems));

            // Extract the optional Documentation
            if (set.Documentation != null)
            {
                entitySet.Documentation = ConvertToDocumentation(set.Documentation);
            }
            AddOtherContent(set, entitySet);

            return entitySet;
        }

        /// <summary>
        ///     Converts an entity set from SOM to metadata
        /// </summary>
        /// <param name="set"> The SOM element to process </param>
        /// <param name="container"> </param>
        /// <returns> The entity set object resulting from the convert </returns>
        private static EntitySet GetEntitySet(EntityContainerEntitySet set, EntityContainer container)
        {
            return container.GetEntitySetByName(set.Name, false);
        }

        /// <summary>
        ///     Converts an association set from SOM to metadata
        /// </summary>
        /// <param name="relationshipSet"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <param name="container"> </param>
        /// <returns> The association set object resulting from the convert </returns>
        private static AssociationSet ConvertToAssociationSet(
            EntityContainerRelationshipSet relationshipSet,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            EntityContainer container,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            Debug.Assert(relationshipSet.Relationship.RelationshipKind == RelationshipKind.Association);

            var associationType = (AssociationType)LoadSchemaElement(
                (SchemaType)relationshipSet.Relationship,
                providerManifest,
                convertedItemCache,
                newGlobalItems);

            var associationSet = new AssociationSet(relationshipSet.Name, associationType);

            foreach (var end in relationshipSet.Ends)
            {
                //-- need the EntityType for the end
                var endEntityType = (EntityType)LoadSchemaElement(
                    end.EntitySet.EntityType,
                    providerManifest,
                    convertedItemCache,
                    newGlobalItems);
                //-- need to get the end member
                var endMember = (AssociationEndMember)associationType.Members[end.Name];
                //-- create the end
                var associationSetEnd = new AssociationSetEnd(
                    GetEntitySet(end.EntitySet, container),
                    associationSet,
                    endMember);

                AddOtherContent(end, associationSetEnd);
                associationSet.AddAssociationSetEnd(associationSetEnd);

                // Extract optional Documentation from the end element
                if (end.Documentation != null)
                {
                    associationSetEnd.Documentation = ConvertToDocumentation(end.Documentation);
                }
            }

            // Extract the optional Documentation
            if (relationshipSet.Documentation != null)
            {
                associationSet.Documentation = ConvertToDocumentation(relationshipSet.Documentation);
            }
            AddOtherContent(relationshipSet, associationSet);

            return associationSet;
        }

        /// <summary>
        ///     Converts a property from SOM to metadata
        /// </summary>
        /// <param name="somProperty"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The property object resulting from the convert </returns>
        private static EdmProperty ConvertToProperty(
            StructuredProperty somProperty,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            EdmProperty property;

            // Get the appropriate type object for this type, for primitive and enum types, get the facet values for the type
            // property as a type usage object as well                  
            TypeUsage typeUsage = null;

            var scalarType = somProperty.Type as ScalarType;

            if (scalarType != null
                && somProperty.Schema.DataModel != SchemaDataModelOption.EntityDataModel)
            {
                // parsing ssdl
                typeUsage = somProperty.TypeUsage;
                UpdateSentinelValuesInFacets(ref typeUsage);
            }
            else
            {
                EdmType propertyType;

                if (scalarType != null)
                {
                    Debug.Assert(somProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);
                    // try to get the instance of the primitive type from the item collection so that it back pointer is set.
                    propertyType = convertedItemCache.ItemCollection.GetItem<PrimitiveType>(somProperty.TypeUsage.EdmType.FullName);
                }
                else
                {
                    propertyType = (EdmType)LoadSchemaElement(somProperty.Type, providerManifest, convertedItemCache, newGlobalItems);
                }

                if (somProperty.CollectionKind
                    != CollectionKind.None)
                {
                    typeUsage = TypeUsage.Create(new CollectionType(propertyType));
                }
                else
                {
                    var enumType = scalarType == null ? somProperty.Type as SchemaEnumType : null;
                    typeUsage = TypeUsage.Create(propertyType);
                    if (enumType != null)
                    {
                        somProperty.EnsureEnumTypeFacets(convertedItemCache, newGlobalItems);
                    }

                    if (somProperty.TypeUsage != null)
                    {
                        ApplyTypePropertyFacets(somProperty.TypeUsage, ref typeUsage);
                    }
                }
            }

            PopulateGeneralFacets(somProperty, ref typeUsage);
            property = new EdmProperty(somProperty.Name, typeUsage);

            // Extract the optional Documentation
            if (somProperty.Documentation != null)
            {
                property.Documentation = ConvertToDocumentation(somProperty.Documentation);
            }
            AddOtherContent(somProperty, property);

            return property;
        }

        /// <summary>
        ///     Converts a navigation property from SOM to metadata
        /// </summary>
        /// <param name="declaringEntityType"> entity type on which this navigation property was declared </param>
        /// <param name="somNavigationProperty"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The property object resulting from the convert </returns>
        private static NavigationProperty ConvertToNavigationProperty(
            EntityType declaringEntityType,
            SchemaObjectModel.NavigationProperty somNavigationProperty,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            // Navigation properties cannot be primitive types, so we can ignore the possibility of having primitive type
            // facets
            var toEndEntityType = (EntityType)LoadSchemaElement(
                somNavigationProperty.Type,
                providerManifest,
                convertedItemCache,
                newGlobalItems);

            EdmType edmType = toEndEntityType;

            // Also load the relationship Type that this navigation property represents
            var relationshipType = (AssociationType)LoadSchemaElement(
                (Relationship)somNavigationProperty.Relationship,
                providerManifest, convertedItemCache, newGlobalItems);

            IRelationshipEnd somRelationshipEnd = null;
            somNavigationProperty.Relationship.TryGetEnd(somNavigationProperty.ToEnd.Name, out somRelationshipEnd);
            if (somRelationshipEnd.Multiplicity
                == RelationshipMultiplicity.Many)
            {
                edmType = toEndEntityType.GetCollectionType();
            }
            else
            {
                Debug.Assert(somRelationshipEnd.Multiplicity != RelationshipMultiplicity.Many);
                edmType = toEndEntityType;
            }

            TypeUsage typeUsage;
            if (somRelationshipEnd.Multiplicity
                == RelationshipMultiplicity.One)
            {
                typeUsage = TypeUsage.Create(
                    edmType,
                    new FacetValues
                        {
                            Nullable = false
                        });
            }
            else
            {
                typeUsage = TypeUsage.Create(edmType);
            }

            // We need to make sure that both the ends of the relationtype are initialized. If there are not, then we should
            // initialize them here
            InitializeAssociationEndMember(relationshipType, somNavigationProperty.ToEnd, toEndEntityType);
            InitializeAssociationEndMember(relationshipType, somNavigationProperty.FromEnd, declaringEntityType);

            // The type of the navigation property must be a ref or collection depending on which end they belong to
            var navigationProperty = new NavigationProperty(somNavigationProperty.Name, typeUsage);
            navigationProperty.RelationshipType = relationshipType;
            navigationProperty.ToEndMember = (RelationshipEndMember)relationshipType.Members[somNavigationProperty.ToEnd.Name];
            navigationProperty.FromEndMember = (RelationshipEndMember)relationshipType.Members[somNavigationProperty.FromEnd.Name];

            // Extract the optional Documentation
            if (somNavigationProperty.Documentation != null)
            {
                navigationProperty.Documentation = ConvertToDocumentation(somNavigationProperty.Documentation);
            }
            AddOtherContent(somNavigationProperty, navigationProperty);

            return navigationProperty;
        }

        /// <summary>
        ///     Converts a function from SOM to metadata
        /// </summary>
        /// <param name="somFunction"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="functionImportEntityContainer"> For function imports, the entity container including the function declaration </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The function object resulting from the convert </returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static EdmFunction ConvertToFunction(
            Function somFunction,
            DbProviderManifest providerManifest,
            ConversionCache convertedItemCache,
            EntityContainer functionImportEntityContainer,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            // If we already have it, don't bother converting
            GlobalItem globalItem = null;

            // if we are converted the function import, we need not check the global items collection,
            // since the function imports are local to the entity container
            if (!somFunction.IsFunctionImport
                && newGlobalItems.TryGetValue(somFunction, out globalItem))
            {
                return (EdmFunction)globalItem;
            }

            var areConvertingForProviderManifest = somFunction.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel;
            var returnParameters = new List<FunctionParameter>();
            if (somFunction.ReturnTypeList != null)
            {
                var i = 0;
                foreach (var somReturnType in somFunction.ReturnTypeList)
                {
                    var returnType = GetFunctionTypeUsage(
                        somFunction is ModelFunction,
                        somFunction,
                        somReturnType,
                        providerManifest,
                        areConvertingForProviderManifest,
                        somReturnType.Type,
                        somReturnType.CollectionKind,
                        somReturnType.IsRefType /*isRefType*/,
                        convertedItemCache,
                        newGlobalItems);
                    if (null != returnType)
                    {
                        // Create the return parameter object, need to set the declaring type explicitly on the return parameter
                        // because we aren't adding it to the members collection
                        var modifier = i == 0 ? string.Empty : i.ToString(CultureInfo.InvariantCulture);
                        i++;
                        var returnParameter = new FunctionParameter(
                            EdmConstants.ReturnType + modifier, returnType, ParameterMode.ReturnValue);
                        AddOtherContent(somReturnType, returnParameter);
                        returnParameters.Add(returnParameter);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
                // this case must be second to avoid calling somFunction.Type when returnTypeList has more than one element.
            else if (somFunction.Type != null)
            {
                var returnType = GetFunctionTypeUsage(
                    somFunction is ModelFunction,
                    somFunction,
                    null,
                    providerManifest,
                    areConvertingForProviderManifest,
                    somFunction.Type,
                    somFunction.CollectionKind,
                    somFunction.IsReturnAttributeReftype /*isRefType*/,
                    convertedItemCache,
                    newGlobalItems);
                if (null != returnType)
                {
                    // Create the return parameter object, need to set the declaring type explicitly on the return parameter
                    // because we aren't adding it to the members collection                    
                    returnParameters.Add(new FunctionParameter(EdmConstants.ReturnType, returnType, ParameterMode.ReturnValue));
                }
                else
                {
                    //Return type was specified but we could not find a type usage
                    return null;
                }
            }

            string functionNamespace;
            EntitySet[] entitySets = null;
            if (somFunction.IsFunctionImport)
            {
                var somFunctionImport = (FunctionImportElement)somFunction;
                functionNamespace = somFunctionImport.Container.Name;
                if (null != somFunctionImport.EntitySet)
                {
                    EntityContainer entityContainer;
                    Debug.Assert(
                        somFunctionImport.ReturnTypeList == null || somFunctionImport.ReturnTypeList.Count == 1,
                        "EntitySet cannot be specified on a FunctionImport if there are multiple ReturnType children");

                    Debug.Assert(
                        functionImportEntityContainer != null,
                        "functionImportEntityContainer must be specified during function import conversion");
                    entityContainer = functionImportEntityContainer;
                    entitySets = new[] { GetEntitySet(somFunctionImport.EntitySet, entityContainer) };
                }
                else if (null != somFunctionImport.ReturnTypeList)
                {
                    Debug.Assert(
                        functionImportEntityContainer != null,
                        "functionImportEntityContainer must be specified during function import conversion");
                    var entityContainer = functionImportEntityContainer;
                    entitySets = somFunctionImport.ReturnTypeList
                                                  .Select(
                                                      returnType => null != returnType.EntitySet
                                                                        ? GetEntitySet(returnType.EntitySet, functionImportEntityContainer)
                                                                        : null)
                                                  .ToArray();
                }
            }
            else
            {
                functionNamespace = somFunction.Namespace;
            }

            var parameters = new List<FunctionParameter>();
            foreach (var somParameter in somFunction.Parameters)
            {
                var parameterType = GetFunctionTypeUsage(
                    somFunction is ModelFunction,
                    somFunction,
                    somParameter,
                    providerManifest,
                    areConvertingForProviderManifest,
                    somParameter.Type,
                    somParameter.CollectionKind,
                    somParameter.IsRefType,
                    convertedItemCache,
                    newGlobalItems);
                if (parameterType == null)
                {
                    return null;
                }

                var parameter = new FunctionParameter(
                    somParameter.Name,
                    parameterType,
                    GetParameterMode(somParameter.ParameterDirection));
                AddOtherContent(somParameter, parameter);

                if (somParameter.Documentation != null)
                {
                    parameter.Documentation = ConvertToDocumentation(somParameter.Documentation);
                }
                parameters.Add(parameter);
            }

            var function = new EdmFunction(
                somFunction.Name,
                functionNamespace,
                GetDataSpace(providerManifest),
                new EdmFunctionPayload
                    {
                        Schema = somFunction.DbSchema,
                        StoreFunctionName = somFunction.StoreFunctionName,
                        CommandText = somFunction.CommandText,
                        EntitySets = entitySets,
                        IsAggregate = somFunction.IsAggregate,
                        IsBuiltIn = somFunction.IsBuiltIn,
                        IsNiladic = somFunction.IsNiladicFunction,
                        IsComposable = somFunction.IsComposable,
                        IsFromProviderManifest = areConvertingForProviderManifest,
                        IsFunctionImport = somFunction.IsFunctionImport,
                        ReturnParameters = returnParameters.ToArray(),
                        Parameters = parameters.ToArray(),
                        ParameterTypeSemantics = somFunction.ParameterTypeSemantics,
                    });

            // Add this function to new global items, only if it is not a function import
            if (!somFunction.IsFunctionImport)
            {
                newGlobalItems.Add(somFunction, function);
            }

            //Check if we already converted functions since we are loading it from 
            //ssdl we could see functions many times.
            GlobalItem returnFunction = null;
            Debug.Assert(
                !convertedItemCache.ItemCollection.TryGetValue(function.Identity, false, out returnFunction),
                "Function duplicates must be checked by som");

            // Extract the optional Documentation
            if (somFunction.Documentation != null)
            {
                function.Documentation = ConvertToDocumentation(somFunction.Documentation);
            }
            AddOtherContent(somFunction, function);

            return function;
        }

        /// <summary>
        ///     Converts SchemaEnumType instance to Metadata EnumType.
        /// </summary>
        /// <param name="somEnumType"> SchemaEnumType to be covnerted. </param>
        /// <param name="newGlobalItems"> Global item objects where newly created Metadata EnumType will be added. </param>
        /// <returns> </returns>
        private static EnumType ConvertToEnumType(SchemaEnumType somEnumType, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            DebugCheck.NotNull(somEnumType);
            DebugCheck.NotNull(newGlobalItems);
            Debug.Assert(
                somEnumType.UnderlyingType is ScalarType,
                "At this point the underlying type should have already been validated and should be ScalarType");

            var enumUnderlyingType = (ScalarType)somEnumType.UnderlyingType;

            // note that enums don't live in SSpace so there is no need to GetDataSpace() for it.
            var enumType = new EnumType(
                somEnumType.Name,
                somEnumType.Namespace,
                enumUnderlyingType.Type,
                somEnumType.IsFlags,
                DataSpace.CSpace);

            var clrEnumUnderlyingType = enumUnderlyingType.Type.ClrEquivalentType;

            foreach (var somEnumMember in somEnumType.EnumMembers)
            {
                Debug.Assert(somEnumMember.Value != null, "value must not be null at this point");
                var enumMember = new EnumMember(
                    somEnumMember.Name, Convert.ChangeType(somEnumMember.Value, clrEnumUnderlyingType, CultureInfo.InvariantCulture));

                if (somEnumMember.Documentation != null)
                {
                    enumMember.Documentation = ConvertToDocumentation(somEnumMember.Documentation);
                }

                AddOtherContent(somEnumMember, enumMember);
                enumType.AddMember(enumMember);
            }

            if (somEnumType.Documentation != null)
            {
                enumType.Documentation = ConvertToDocumentation(somEnumType.Documentation);
            }
            AddOtherContent(somEnumType, enumType);

            newGlobalItems.Add(somEnumType, enumType);
            return enumType;
        }

        /// <summary>
        ///     Converts an SOM Documentation node to a metadata Documentation construct
        /// </summary>
        /// <param name="element"> The SOM element to process </param>
        /// <param name="providerManifest"> The provider manifest to be used for conversion </param>
        /// <param name="convertedItemCache"> The item collection for currently existing metadata objects </param>
        /// <param name="newGlobalItems"> The new GlobalItem objects that are created as a result of this conversion </param>
        /// <returns> The Documentation object resulting from the convert operation </returns>
        private static Documentation ConvertToDocumentation(DocumentationElement element)
        {
            DebugCheck.NotNull(element);
            return element.MetadataDocumentation;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static TypeUsage GetFunctionTypeUsage(
            bool isModelFunction,
            Function somFunction,
            FacetEnabledSchemaElement somParameter,
            DbProviderManifest providerManifest,
            bool areConvertingForProviderManifest,
            SchemaType type,
            CollectionKind collectionKind,
            bool isRefType,
            ConversionCache convertedItemCache,
            Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            if (null != somParameter
                && areConvertingForProviderManifest
                && somParameter.HasUserDefinedFacets)
            {
                return somParameter.TypeUsage;
            }

            if (null == type)
            {
                if (isModelFunction
                    && somParameter != null
                    && somParameter is Parameter)
                {
                    ((Parameter)somParameter).ResolveNestedTypeNames(convertedItemCache, newGlobalItems);
                    return somParameter.TypeUsage;
                }
                else if (somParameter != null
                         && somParameter is ReturnType)
                {
                    ((ReturnType)somParameter).ResolveNestedTypeNames(convertedItemCache, newGlobalItems);
                    return somParameter.TypeUsage;
                }
                else
                {
                    return null;
                }
            }

            EdmType edmType;
            if (!areConvertingForProviderManifest)
            {
                // SOM verifies the type is either scalar, row, or entity
                var scalarType = type as ScalarType;
                if (null != scalarType)
                {
                    if (isModelFunction && somParameter != null)
                    {
                        if (somParameter.TypeUsage == null)
                        {
                            somParameter.ValidateAndSetTypeUsage(scalarType);
                        }
                        return somParameter.TypeUsage;
                    }
                    else if (isModelFunction)
                    {
                        var modelFunction = somFunction as ModelFunction;
                        if (modelFunction.TypeUsage == null)
                        {
                            modelFunction.ValidateAndSetTypeUsage(scalarType);
                        }
                        return modelFunction.TypeUsage;
                    }
                    else if (somParameter != null
                             && somParameter.HasUserDefinedFacets
                             && somFunction.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
                    {
                        somParameter.ValidateAndSetTypeUsage(scalarType);
                        return somParameter.TypeUsage;
                    }
                    else
                    {
                        edmType = GetPrimitiveType(scalarType, providerManifest);
                    }
                }
                else
                {
                    edmType = (EdmType)LoadSchemaElement(
                        type,
                        providerManifest,
                        convertedItemCache,
                        newGlobalItems);

                    // Neither FunctionImport nor its Parameters can have facets when defined in CSDL so for enums, 
                    // since they are only a CSpace concept, we need to process facets only on model functions 
                    if (isModelFunction && type is SchemaEnumType)
                    {
                        Debug.Assert(somFunction.Schema.DataModel == SchemaDataModelOption.EntityDataModel, "Enums live only in CSpace");

                        if (somParameter != null)
                        {
                            somParameter.ValidateAndSetTypeUsage(edmType);
                            return somParameter.TypeUsage;
                        }
                        else if (somFunction != null)
                        {
                            var modelFunction = ((ModelFunction)somFunction);
                            modelFunction.ValidateAndSetTypeUsage(edmType);
                            return modelFunction.TypeUsage;
                        }
                        else
                        {
                            Debug.Fail("Should never get here.");
                        }
                    }
                }
            }
            else if (type is TypeElement)
            {
                var typeElement = type as TypeElement;
                edmType = typeElement.PrimitiveType;
            }
            else
            {
                var typeElement = type as ScalarType;
                edmType = typeElement.Type;
            }

            //Construct type usage
            TypeUsage usage;
            if (collectionKind != CollectionKind.None)
            {
                usage = convertedItemCache.GetCollectionTypeUsageWithNullFacets(edmType);
            }
            else
            {
                var entityType = edmType as EntityType;
                if (entityType != null && isRefType)
                {
                    usage = TypeUsage.Create(new RefType(entityType));
                }
                else
                {
                    usage = convertedItemCache.GetTypeUsageWithNullFacets(edmType);
                }
            }

            return usage;
        }

        /// <summary>
        ///     Converts the ParameterDirection into a ParameterMode
        /// </summary>
        /// <param name="parameterDirection"> The ParameterDirection to convert </param>
        /// <returns> ParameterMode </returns>
        private static ParameterMode GetParameterMode(ParameterDirection parameterDirection)
        {
            Debug.Assert(
                parameterDirection == ParameterDirection.Input
                || parameterDirection == ParameterDirection.InputOutput
                || parameterDirection == ParameterDirection.Output,
                "Inconsistent metadata error");

            switch (parameterDirection)
            {
                case ParameterDirection.Input:
                    return ParameterMode.In;

                case ParameterDirection.Output:
                    return ParameterMode.Out;

                case ParameterDirection.InputOutput:
                default:
                    return ParameterMode.InOut;
            }
        }

        /// <summary>
        ///     Apply the facet values
        /// </summary>
        /// <param name="sourceType"> The source TypeUsage </param>
        /// <param name="targetType"> The primitive or enum type of the target </param>
        private static void ApplyTypePropertyFacets(TypeUsage sourceType, ref TypeUsage targetType)
        {
            var newFacets = targetType.Facets.ToDictionary(f => f.Name);
            var madeChange = false;
            foreach (var sourceFacet in sourceType.Facets)
            {
                Facet targetFacet;
                if (newFacets.TryGetValue(sourceFacet.Name, out targetFacet))
                {
                    if (!targetFacet.Description.IsConstant)
                    {
                        madeChange = true;
                        newFacets[targetFacet.Name] = Facet.Create(targetFacet.Description, sourceFacet.Value);
                    }
                }
                else
                {
                    madeChange = true;
                    newFacets.Add(sourceFacet.Name, sourceFacet);
                }
            }

            if (madeChange)
            {
                targetType = TypeUsage.Create(targetType.EdmType, newFacets.Values);
            }
        }

        /// <summary>
        ///     Populate the facets on the TypeUsage object for a property
        /// </summary>
        /// <param name="somProperty"> The property containing the information </param>
        /// <param name="propertyTypeUsage"> The type usage object where to populate facet </param>
        private static void PopulateGeneralFacets(
            StructuredProperty somProperty,
            ref TypeUsage propertyTypeUsage)
        {
            var madeChanges = false;
            var facets = propertyTypeUsage.Facets.ToDictionary(f => f.Name);
            if (!somProperty.Nullable)
            {
                facets[DbProviderManifest.NullableFacetName] = Facet.Create(MetadataItem.NullableFacetDescription, false);
                madeChanges = true;
            }

            if (somProperty.Default != null)
            {
                facets[DbProviderManifest.DefaultValueFacetName] = Facet.Create(
                    MetadataItem.DefaultValueFacetDescription, somProperty.DefaultAsObject);
                madeChanges = true;
            }

            //This is not really a general facet
            //If we are dealing with a 1.1 Schema, Add a facet for CollectionKind
            if (somProperty.Schema.SchemaVersion
                == XmlConstants.EdmVersionForV1_1)
            {
                var newFacet = Facet.Create(MetadataItem.CollectionKindFacetDescription, somProperty.CollectionKind);
                facets.Add(newFacet.Name, newFacet);
                madeChanges = true;
            }

            if (madeChanges)
            {
                propertyTypeUsage = TypeUsage.Create(propertyTypeUsage.EdmType, facets.Values);
            }
        }

        private static DataSpace GetDataSpace(DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(providerManifest);
            // Target attributes is for types and sets in target space.
            if (providerManifest is EdmProviderManifest)
            {
                return DataSpace.CSpace;
            }
            else
            {
                return DataSpace.SSpace;
            }
        }

        /// <summary>
        ///     Get a primitive type when converting a CSDL schema
        /// </summary>
        /// <param name="scalarType"> The schema type representing the primitive type </param>
        /// <param name="providerManifest"> The provider manifest for retrieving the store types </param>
        private static PrimitiveType GetPrimitiveType(
            ScalarType scalarType,
            DbProviderManifest providerManifest)
        {
            PrimitiveType returnValue = null;
            var scalarTypeName = scalarType.Name;

            foreach (var primitiveType in providerManifest.GetStoreTypes())
            {
                if (primitiveType.Name == scalarTypeName)
                {
                    returnValue = primitiveType;
                    break;
                }
            }

            Debug.Assert(scalarType != null, "Som scalar type should always resolve to a primitive type");
            return returnValue;
        }

        // This will update the sentinel values in the facets if required
        private static void UpdateSentinelValuesInFacets(ref TypeUsage typeUsage)
        {
            // For string and decimal types, replace the sentinel by the max possible value
            var primitiveType = (PrimitiveType)typeUsage.EdmType;
            if (primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String
                ||
                primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary)
            {
                var maxLengthFacet = typeUsage.Facets[DbProviderManifest.MaxLengthFacetName];
                if (Helper.IsUnboundedFacetValue(maxLengthFacet))
                {
                    typeUsage = typeUsage.ShallowCopy(
                        new FacetValues
                            {
                                MaxLength = Helper.GetFacet(
                                    primitiveType.FacetDescriptions,
                                    DbProviderManifest.MaxLengthFacetName).MaxValue
                            });
                }
            }
        }

        /// <summary>
        ///     Cache containing item collection and type usages to support looking up and generating
        ///     metadata types.
        /// </summary>
        internal class ConversionCache
        {
            internal readonly ItemCollection ItemCollection;
            private readonly Dictionary<EdmType, TypeUsage> _nullFacetsTypeUsage;
            private readonly Dictionary<EdmType, TypeUsage> _nullFacetsCollectionTypeUsage;

            internal ConversionCache(ItemCollection itemCollection)
            {
                ItemCollection = itemCollection;
                _nullFacetsTypeUsage = new Dictionary<EdmType, TypeUsage>();
                _nullFacetsCollectionTypeUsage = new Dictionary<EdmType, TypeUsage>();
            }

            /// <summary>
            ///     Gets type usage for the given type with null facet values. Caches usage to avoid creating
            ///     redundant type usages.
            /// </summary>
            internal TypeUsage GetTypeUsageWithNullFacets(EdmType edmType)
            {
                // check for cached result
                TypeUsage result;
                if (_nullFacetsTypeUsage.TryGetValue(edmType, out result))
                {
                    return result;
                }

                // construct result
                result = TypeUsage.Create(edmType, FacetValues.NullFacetValues);

                // cache result
                _nullFacetsTypeUsage.Add(edmType, result);

                return result;
            }

            /// <summary>
            ///     Gets collection type usage for the given type with null facet values. Caches usage to avoid creating
            ///     redundant type usages.
            /// </summary>
            internal TypeUsage GetCollectionTypeUsageWithNullFacets(EdmType edmType)
            {
                // check for cached result
                TypeUsage result;
                if (_nullFacetsCollectionTypeUsage.TryGetValue(edmType, out result))
                {
                    return result;
                }

                // construct collection type from cached element type
                var elementTypeUsage = GetTypeUsageWithNullFacets(edmType);
                result = TypeUsage.Create(new CollectionType(elementTypeUsage), FacetValues.NullFacetValues);

                // cache result
                _nullFacetsCollectionTypeUsage.Add(edmType, result);

                return result;
            }
        }
    }
}
