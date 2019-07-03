// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using AssociationSet = Microsoft.Data.Entity.Design.Model.Entity.AssociationSet;
    using ComplexType = Microsoft.Data.Entity.Design.Model.Entity.ComplexType;
    using EntitySet = Microsoft.Data.Entity.Design.Model.Entity.EntitySet;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using EnumType = Microsoft.Data.Entity.Design.Model.Entity.EnumType;
    using NavigationProperty = Microsoft.Data.Entity.Design.Model.Entity.NavigationProperty;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class ModelHelper
    {
        private const int DefaultPropertySuffixSeed = 1;
        private const string CollectionCaptureName = "entityname";

        private static readonly Regex _collectionReturnTypePattern =
            new Regex(@"Collection\((\w+\.)*(?<" + CollectionCaptureName + @">\w+)\)");

        // Return fully qualified name.
        private static readonly Regex _collectionNamespaceQualifiedReturnTypePattern =
            new Regex(@"Collection\((?<" + CollectionCaptureName + @">(\w+\.)*\w+)\)");

        // Contains all scalar EDM type names. Perf note: AllPrimitiveTypes() lazily loads these once per Version - after that just return HashSet
        private static readonly Dictionary<Version, HashSet<string>> _edmPrimitiveTypes = new Dictionary<Version, HashSet<string>>();

        private static HashSet<Type> _underlyingEnumTypes;

        /// <summary>
        ///     Returns true if the type can support Identity for StoreGeneratedPattern
        /// </summary>
        internal static bool CanTypeSupportIdentity(string conceptualTypeName)
        {
            var type = GetPrimitiveTypeFromString(conceptualTypeName);
            return type != null
                   && (type.PrimitiveTypeKind == PrimitiveTypeKind.Int16 ||
                       type.PrimitiveTypeKind == PrimitiveTypeKind.Int32 ||
                       type.PrimitiveTypeKind == PrimitiveTypeKind.Int64 ||
                       type.PrimitiveTypeKind == PrimitiveTypeKind.Decimal ||
                       type.PrimitiveTypeKind == PrimitiveTypeKind.Byte);
        }

        /// <summary>
        ///     Creates an AssociationEnd name for a FK using our naming convention.
        /// </summary>
        internal static string CreateFKAssociationEndName(string tableName)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}", tableName, "FK");
        }

        /// <summary>
        ///     Creates an AssociationEnd name for a PK using our naming convention.
        /// </summary>
        internal static string CreatePKAssociationEndName(string tableName)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}", tableName, "PK");
        }

        /// <summary>
        ///     Returns a string that can be used as TSimpleIdentifier.
        /// </summary>
        internal static string CreateValidSimpleIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("Identifier is null or empty!");
            }

            // Replace anything that doesn't adhere to the ECMA specification for identifiers with an underscore,
            // unless it's the first which the schema definition says must be a letter...
            // System.Data.Resource.CSMSL_2.xsd - [\p{L}\p{Nl}][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]{0,}
            var isFirst = true;
            var requiresPrefix = false;
            var charsToReplace = new List<char>();
            var firstRegex = new Regex(@"[\p{L}\p{Nl}]");
            var subsequentRegex = new Regex(@"[\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]");
            foreach (var c in identifier)
            {
                if (isFirst)
                {
                    if (!firstRegex.IsMatch(c.ToString()))
                    {
                        requiresPrefix = true;

                        // If we're adding a prefix, this character is no longer the first so we need to match it
                        // against the second regex
                        if (!charsToReplace.Contains(c)
                            && !subsequentRegex.IsMatch(c.ToString()))
                        {
                            charsToReplace.Add(c);
                        }
                    }

                    isFirst = false;
                }
                else
                {
                    if (!charsToReplace.Contains(c)
                        && !subsequentRegex.IsMatch(c.ToString()))
                    {
                        charsToReplace.Add(c);
                    }
                }
            }

            StringBuilder resultStringBuilder;

            if (requiresPrefix)
            {
                // Need to start with a letter... just going to copy what EF is doing, not sure if there are any better alternatives...
                resultStringBuilder = new StringBuilder("C");
                resultStringBuilder.Append(identifier);
            }
            else
            {
                resultStringBuilder = new StringBuilder(identifier);
            }

            foreach (var c in charsToReplace)
            {
                resultStringBuilder = resultStringBuilder.Replace(c, '_');
            }

            return resultStringBuilder.ToString();
        }

        /// <summary>
        ///     Check to see if a new name for an existing item will be unique within the passed in container.
        /// </summary>
        internal static bool IsUniqueName(
            Type type, EFContainer container, string proposedName, bool uniquenessIsCaseSensitive, out string errorMessage)
        {
            return IsUniqueNameInternal(type, container, null, proposedName, uniquenessIsCaseSensitive, out errorMessage);
        }

        /// <summary>
        ///     Check to see if a new name for an existing item will be unique.
        /// </summary>
        internal static bool IsUniqueNameForExistingItem(
            EFObject existingItem, string proposedName, bool uniquenessIsCaseSensitive, out string errorMessage)
        {
            return IsUniqueNameInternal(
                existingItem.GetType(), existingItem.Parent, existingItem, proposedName, uniquenessIsCaseSensitive, out errorMessage);
        }

        private static bool IsUniqueNameInternal(
            Type type, EFContainer container, EFObject existingItem, string proposedName, bool uniquenessIsCaseSensitive,
            out string errorMessage)
        {
            // default error message
            errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.NAME_NOT_UNIQUE, proposedName);

            if ((typeof(EntityType)).IsAssignableFrom(type))
            {
                // type is derived from or equal to EntityType. 

                if (existingItem != null)
                {
                    var et = existingItem as EntityType;

                    Debug.Assert(
                        et != null,
                        "For type assignable from EntityType Unexpected existingItem type of " + existingItem.GetType().Name + " for type "
                        + type.Name);
                    Debug.Assert(et.Parent == container, "et.Parent != container!");
                    if (et != null)
                    {
                        // for entity types, we need to check if the proposed name will conflict with any existing entity types in the container, or any properties in the entity type
                        return (XmlModelHelper.IsUniqueNameInsideContainer(container, proposedName, uniquenessIsCaseSensitive)
                                && XmlModelHelper.IsUniqueNameInsideContainer(et, proposedName, uniquenessIsCaseSensitive));
                    }
                }
            }
            else if ((typeof(ComplexType)).IsAssignableFrom(type))
            {
                if (existingItem != null)
                {
                    var complexType = existingItem as ComplexType;

                    Debug.Assert(
                        complexType != null,
                        "For type assignable from ComplexType Unexpected existingItem type of " + existingItem.GetType().Name + " for type "
                        + type.Name);
                    Debug.Assert(complexType.Parent == container, "complexType.Parent != container!");
                    if (complexType != null)
                    {
                        // for complex types, we need to check if the proposed name will conflict with any existing complex, enum or  entity types in the container, or any properties in the entity type
                        return (XmlModelHelper.IsUniqueNameInsideContainer(container, proposedName, uniquenessIsCaseSensitive)
                                && XmlModelHelper.IsUniqueNameInsideContainer(complexType, proposedName, uniquenessIsCaseSensitive));
                    }
                }
            }
            else if ((typeof(EnumType)).IsAssignableFrom(type))
            {
                if (existingItem != null)
                {
                    var enumType = existingItem as EnumType;

                    Debug.Assert(
                        enumType != null,
                        "For type assignable from EnumType Unexpected existingItem type of " + existingItem.GetType().Name + " for type "
                        + type.Name);
                    Debug.Assert(enumType.Parent == container, "enumType.Parent != container!");

                    if (enumType != null)
                    {
                        // for enum types, we need to check if the proposed name will conflict with any existing complex, enum or entity types in the container.
                        return (XmlModelHelper.IsUniqueNameInsideContainer(container, proposedName, uniquenessIsCaseSensitive));
                    }
                }
            }
            else if ((typeof(PropertyBase)).IsAssignableFrom(type))
            {
                // type is derived from or equal to PropertyBase (eg, a Property or a NavigationProperty)
                var et = container as EntityType;
                if (et != null)
                {
                    string message;
                    return ValidateEntityPropertyName(et, proposedName, uniquenessIsCaseSensitive, out message);
                }
                else
                {
                    var complexType = container as ComplexType;
                    Debug.Assert(complexType != null, "container should be a ComplexType");
                    string message;
                    return ValidateComplexTypePropertyName(complexType, proposedName, uniquenessIsCaseSensitive, out message);
                }
            }
            else if ((typeof(BaseEntityContainer)).IsAssignableFrom(type))
            {
                var baseEntityModel = container as BaseEntityModel;
                Debug.Assert(baseEntityModel != null, "container should be a BaseEntityModel");
                if (baseEntityModel != null)
                {
                    if (baseEntityModel.Namespace.Value == proposedName)
                    {
                        errorMessage = string.Format(
                            CultureInfo.CurrentCulture, Resources.EntityContainerNameConflictsWithNamespaceName, proposedName);
                        return false;
                    }
                }
            }

            // fall through case...
            return XmlModelHelper.IsUniqueNameInsideContainer(container, proposedName, uniquenessIsCaseSensitive);
        }

        internal static bool ValidatePropertyName(
            Property property, string proposedName, bool uniquenessIsCaseSensitive, out string errorMessage)
        {
            if (property.IsComplexTypeProperty)
            {
                return ValidateComplexTypePropertyName(
                    property.Parent as ComplexType, proposedName, uniquenessIsCaseSensitive, out errorMessage);
            }
            else
            {
                Debug.Assert(property.IsEntityTypeProperty, "Unexpected parent of property - property is not an EntityType property");
                return ValidateEntityPropertyName(property.EntityType, proposedName, uniquenessIsCaseSensitive, out errorMessage);
            }
        }

        /// <summary>
        ///     Validate entity property name value.
        /// </summary>
        /// <param name="entityType">Property's EntityType instance.</param>
        /// <param name="proposedName">The proposed property name.</param>
        /// <param name="uniquenessIsCaseSensitive">A flag that indicates whether case sensitive string comparison is used to check the uniqueness of the property name.</param>
        /// <param name="errorMessage">Property name validation error message.</param>
        /// <returns>Return true if the property name is a valid CSDL property name and a unique name, return false otherwise.</returns>
        internal static bool ValidateEntityPropertyName(
            EntityType entityType, string proposedName, bool uniquenessIsCaseSensitive, out string errorMessage)
        {
            // Check if the name is a valid CSDL property name.
            if (!EscherAttributeContentValidator.IsValidCsdlPropertyName(proposedName))
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.Error_PropertyNameInvalid, proposedName);
                return false;
            }
                // Check if the property name is not equal to parent entity name.
            else if (entityType.LocalName.Value.Equals(proposedName))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture, Resources.Error_MemberNameSameAsParent, proposedName, entityType.LocalName.Value);
                return false;
            }
                // Check if the property name is unique within the passed in EntityType scope.
            else if (!IsUniquePropertyName(entityType, proposedName, uniquenessIsCaseSensitive))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture, Resources.Error_MemberNameNotUnique, proposedName, entityType.LocalName.Value);
                return false;
            }
            else
            {
                errorMessage = string.Empty;
                return true;
            }
        }

        /// <summary>
        ///     Validate ComplexType property name value.
        /// </summary>
        /// <param name="entityType">Property's ComplexType instance.</param>
        /// <param name="proposedName">The proposed property name.</param>
        /// <param name="uniquenessIsCaseSensitive">A flag that indicates whether case sensitive string comparison is used to check the uniqueness of the property name.</param>
        /// <param name="errorMessage">Property name validation error message.</param>
        /// <returns>Return true if the property name is a valid CSDL property name and a unique name, return false otherwise.</returns>
        internal static bool ValidateComplexTypePropertyName(
            ComplexType complexType, string proposedName, bool uniquenessIsCaseSensitive, out string errorMessage)
        {
            Debug.Assert(complexType != null, "ComplexType is null");

            // Check if the name is a valid CSDL property name.
            if (!EscherAttributeContentValidator.IsValidCsdlPropertyName(proposedName))
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.Error_PropertyNameInvalid, proposedName);
                return false;
            }
                // Check if the property name is not equal to parent entity name.
            else if (complexType.LocalName.Value.Equals(proposedName))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture, Resources.Error_MemberNameSameAsParent, proposedName, complexType.LocalName.Value);
                return false;
            }
                // Check if the property name is unique within the passed in EntityType scope.
            else if (!IsUniqueComplexTypePropertyName(complexType, proposedName, uniquenessIsCaseSensitive))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture, Resources.Error_ComplexTypePropertyNameNotUnique, proposedName, complexType.LocalName.Value);
                return false;
            }
            else
            {
                errorMessage = string.Empty;
                return true;
            }
        }

        // Check to see if this name for the property will be unique within the passed in EntityType and it's inheritance hierarchy
        internal static bool IsUniquePropertyName(
            EntityType entityType, string proposedName, bool uniquenessIsCaseSensitive, HashSet<EFObject> childEFObjectsToIgnore = null)
        {
            if (string.IsNullOrEmpty(proposedName)
                || entityType == null)
            {
                return false;
            }

            var cet = entityType as ConceptualEntityType;
            if (cet != null)
            {
                // check the base type hierarchy
                var parentEntity = cet.BaseType.Target;
                while (parentEntity != null)
                {
                    if (!XmlModelHelper.IsUniqueNameInsideContainer(
                        parentEntity, proposedName, uniquenessIsCaseSensitive, childEFObjectsToIgnore))
                    {
                        return false;
                    }

                    // properties can't have the same name as an entity type
                    if (proposedName.Equals(parentEntity.LocalName.Value))
                    {
                        return false;
                    }

                    parentEntity = parentEntity.BaseType.Target;
                }
            }
            return IsUniquePropertyNameInSubTypes(entityType, proposedName, uniquenessIsCaseSensitive, childEFObjectsToIgnore);
        }

        // Check to see if this name for the property will be unique within the passed in ComplexType
        internal static bool IsUniqueComplexTypePropertyName(ComplexType complexType, string proposedName, bool uniquenessIsCaseSensitive)
        {
            if (string.IsNullOrEmpty(proposedName)
                || complexType == null)
            {
                return false;
            }

            return XmlModelHelper.IsUniqueNameInsideContainer(complexType, proposedName, uniquenessIsCaseSensitive);
        }

        // Check to see if this name for the property will be unique within the passed in EntityType and it's subtypes
        private static bool IsUniquePropertyNameInSubTypes(
            EntityType entityType, string proposedName, bool uniquenessIsCaseSensitive, HashSet<EFObject> childEFObjectsToIgnore = null)
        {
            if (string.IsNullOrEmpty(proposedName)
                ||
                entityType == null)
            {
                return false;
            }

            // check uniqueness inside given EntityType
            if (!XmlModelHelper.IsUniqueNameInsideContainer(entityType, proposedName, uniquenessIsCaseSensitive, childEFObjectsToIgnore))
            {
                return false;
            }

            // properties can't have the same name as an entity type
            if (proposedName.Equals(entityType.LocalName.Value))
            {
                return false;
            }

            // check subtypes
            foreach (var entity in entityType.GetAntiDependenciesOfType<EntityType>())
            {
                if (!IsUniquePropertyNameInSubTypes(entity, proposedName, uniquenessIsCaseSensitive, childEFObjectsToIgnore))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets unique name for element within the passed container, based on proposed name
        /// </summary>
        internal static string GetUniqueName(Type type, EFContainer container, string proposedName, HashSet<string> namesToAvoid = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(proposedName), "Please specify proposedName");
            var uniqueName = proposedName;

            string msg = null;
            var childCount = container.Children.Count();
            for (var i = 1; i <= childCount; ++i)
            {
                if (IsUniqueName(type, container, uniqueName, false, out msg)
                    && (namesToAvoid == null || !namesToAvoid.Contains(uniqueName)))
                {
                    break;
                }
                uniqueName = proposedName + i;
            }

            return uniqueName;
        }

        /// <summary>
        ///     Gets unique name for element within the passed container, based on default name followed by a unique number
        /// </summary>
        internal static string GetUniqueNameWithNumber(Type type, EFContainer item, string defaultName)
        {
            Debug.Assert(!String.IsNullOrEmpty(defaultName), "Please specify defaultName");
            var uniqueName = defaultName + 1;

            string msg = null;
            var childCountPlusOne = item.Children.Count() + 1; // offset by one because we start at 2
            for (var i = 2; i <= childCountPlusOne; ++i)
            {
                if (IsUniqueName(type, item, uniqueName, false, out msg))
                {
                    break;
                }

                uniqueName = defaultName + i;
            }

            return uniqueName;
        }

        internal static string GetUniqueTableNameInSchema(
            string schemaName, string proposedTableName, StorageEntityModel storageModel, HashSet<string> namesToAvoid)
        {
            Debug.Assert(!String.IsNullOrEmpty(proposedTableName), "Please specify proposedTableName");
            var uniqueTableName = proposedTableName;

            var i = 1;
            var ec = storageModel.FirstEntityContainer as StorageEntityContainer;
            var existingTableNames = new HashSet<string>(
                ec.EntitySets()
                    .Cast<StorageEntitySet>()
                    .Where(es => es.Schema.Value == schemaName)
                    .Select(es => es.Table.Value));

            while (existingTableNames.Contains(uniqueTableName)
                   || namesToAvoid.Contains(uniqueTableName))
            {
                uniqueTableName = proposedTableName + i;
                ++i;
            }
            return uniqueTableName;
        }

        internal static PrimitiveType GetPrimitiveTypeFromString(string primitiveTypeName)
        {
            return
                PrimitiveType
                    .GetEdmPrimitiveTypes()
                    .FirstOrDefault(primType => primType.Name == primitiveTypeName);
        }

        internal static bool IsValidStorageFacet(StorageEntityModel storageModel, string storagePrimTypeString, string facetName)
        {
            Debug.Assert(storageModel != null, "storageModel != null");
            Debug.Assert(!string.IsNullOrEmpty(facetName), "facetName must not be null or empty string");

            var storagePrimType = storageModel.GetStoragePrimitiveType(storagePrimTypeString);
            if (storagePrimType == null)
            {
                return false;
            }

            // Now see if the requested facet is appropriate to that type
            FacetDescription facet;
            return TryGetFacet(storagePrimType, facetName, out facet);
        }

        internal static bool IsValidModelFacet(string primTypeString, string facetName)
        {
            FacetDescription facet;
            return TryGetFacet(GetPrimitiveTypeFromString(primTypeString), facetName, out facet);
        }

        internal static bool TryGetFacet(PrimitiveType primType, string facetName, out FacetDescription facetDescription)
        {
            if (primType != null)
            {
                foreach (var facet in primType.FacetDescriptions.Where(facet => facet.FacetName == facetName))
                {
                    facetDescription = facet;
                    return true;
                }
            }

            facetDescription = null;
            return false;
        }

        internal static bool IsInConceptualModel(EFObject efObject)
        {
            while (efObject != null)
            {
                if (efObject is ConceptualEntityModel)
                {
                    return true;
                }
                efObject = efObject.Parent;
            }
            return false;
        }

        internal static HashSet<string> AllPrimitiveTypes(Version targetSchemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(targetSchemaVersion), "invalid schema version");

            HashSet<string> primitiveTypeSet;
            if (!_edmPrimitiveTypes.TryGetValue(targetSchemaVersion, out primitiveTypeSet))
            {
                // lazily load primitive types
                var edmCollection = new EdmItemCollection(new XmlReader[] { });

                primitiveTypeSet = new HashSet<string>(
                    edmCollection.GetPrimitiveTypes(EntityFrameworkVersion.VersionToDouble(targetSchemaVersion))
                        .Select(t => t.Name));

                _edmPrimitiveTypes.Add(targetSchemaVersion, primitiveTypeSet);
            }

            return primitiveTypeSet;
        }

        // Primitive types for given Version, alpha case-insensitive sorted
        internal static string[] AllPrimitiveTypesSorted(Version targetSchemaVersion)
        {
            var primTypes = AllPrimitiveTypes(targetSchemaVersion);
            var sortedEdmPrimitiveTypes = primTypes.ToArray();
            Array.Sort(sortedEdmPrimitiveTypes, StringComparer.CurrentCultureIgnoreCase);
            return sortedEdmPrimitiveTypes;
        }

        /// <returns>
        ///     The list of "real" primitive types, not including the "quasi-primitive" types
        ///     such as Geometry and Geography - which are not allowed as PK types
        /// </returns>
        internal static HashSet<string> AllPrimaryKeyPrimitiveTypes()
        {
            // TODO: this is a hack (also is incorrect for V1 where Binary is not a valid key type) - should be fixed - see StoreModelBuilder.IsValidKeyType()
            return AllPrimitiveTypes(EntityFrameworkVersion.Version2);
        }

        internal static HashSet<Type> UnderlyingEnumTypes
        {
            get
            {
                if (_underlyingEnumTypes == null)
                {
                    _underlyingEnumTypes = new HashSet<Type>();
                    _underlyingEnumTypes.Add(typeof(Int32));
                    _underlyingEnumTypes.Add(typeof(Int16));
                    _underlyingEnumTypes.Add(typeof(Int64));
                    _underlyingEnumTypes.Add(typeof(Byte));
                    _underlyingEnumTypes.Add(typeof(SByte));
                }
                return _underlyingEnumTypes;
            }
        }

        /// <summary>
        ///     A helper method that tries to find one of the two types of ETMs
        /// </summary>
        internal static EntityTypeMapping FindEntityTypeMapping(
            CommandProcessorContext cpc, EntityType conceptualEntityType, EntityTypeMappingKind kind, bool createIfNoneFound)
        {
            foreach (var etm in conceptualEntityType.GetAntiDependenciesOfType<EntityTypeMapping>())
            {
                if (etm.EntitySetMapping.Name.Target == conceptualEntityType.EntitySet)
                {
                    if (etm.ModificationFunctionMapping != null)
                    {
                        if (kind == EntityTypeMappingKind.Function)
                        {
                            return etm;
                        }
                        continue;
                    }

                    if (etm.TypeName.Status == (int)BindingStatus.Known)
                    {
                        // v1 assumption, ETM is bound to a single type
                        Debug.Assert(
                            etm.TypeName.Bindings.Count <= 1 && etm.TypeName.IsTypeOfs.Count <= 1,
                            "etm.TypeName.Bindings.Count(" + etm.TypeName.Bindings.Count
                            + ") should be <= 1 and etm.TypeName.IsTypeOfs.Count(" + etm.TypeName.IsTypeOfs.Count + ") should be <= 1");

                        // if we are fully bound, then we can just check to see if the binding is using IsTypeOfs
                        if (etm.TypeName.IsTypeOfs[0]
                            && kind == EntityTypeMappingKind.IsTypeOf)
                        {
                            return etm;
                        }
                        else if (etm.TypeName.IsTypeOfs[0] == false
                                 && kind == EntityTypeMappingKind.Default)
                        {
                            return etm;
                        }
                    }
                    else
                    {
                        // if we aren't bound, then all we can do is a string compare
                        if (etm.TypeName.RefName.Contains(EntityTypeMapping.IsTypeOf)
                            && kind == EntityTypeMappingKind.IsTypeOf)
                        {
                            return etm;
                        }
                        else if (kind == EntityTypeMappingKind.Default)
                        {
                            return etm;
                        }
                    }
                }
            }

            // if couldn't find an existing fragment, create it
            if (createIfNoneFound && cpc != null)
            {
                var createETM = new CreateEntityTypeMappingCommand(
                    conceptualEntityType as ConceptualEntityType,
                    kind);
                CommandProcessor.InvokeSingleCommand(cpc, createETM);
                Debug.Assert(createETM.EntityTypeMapping != null, "createETM.EntityTypeMapping should not be null");
                return createETM.EntityTypeMapping;
            }

            return null;
        }

        /// <summary>
        ///     Helper method to find a fragment inside either a Default or an IsTypeOf ETM that maps a C and S entity,
        ///     and optionally creates one if none exists.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="conceptualEntityType">The conceptual side entity</param>
        /// <param name="storageEntityType">The storage side entity</param>
        /// <param name="createIfNoneFound">If you send true, then a fragment will be created inside an IsTypeOf ETM if no other exists</param>
        /// <returns>May return null if you pass false to 'createIfNoneFound'</returns>
        internal static MappingFragment FindMappingFragment(
            CommandProcessorContext cpc, EntityType conceptualEntityType, EntityType storageEntityType, bool createIfNoneFound)
        {
            // first see if we have a Default ETM
            var mappingFragment = FindMappingFragment(cpc, conceptualEntityType, storageEntityType, EntityTypeMappingKind.Default, false);
            if (mappingFragment == null)
            {
                // if we don't have a default, then find or create an IsTypeOf ETM to put this in
                mappingFragment = FindMappingFragment(
                    cpc, conceptualEntityType, storageEntityType, EntityTypeMappingKind.IsTypeOf, createIfNoneFound);
            }

            return mappingFragment;
        }

        /// <summary>
        ///     Helper method to find a fragment inside an ETM of a certain kind that maps a C and S entity, and optionally
        ///     creates one if none exists.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="conceptualEntityType">The conceptual side entity</param>
        /// <param name="storageEntityType">The storage side entity</param>
        /// <param name="kind">Looks for an EntityTypeMapping of this kind and then looks for a fragment inside it</param>
        /// <param name="createIfNoneFound">Create one of the Kind passed in if none of that kind are found</param>
        /// <returns>May return null if you pass false to 'createIfNoneFound'</returns>
        internal static MappingFragment FindMappingFragment(
            CommandProcessorContext cpc, EntityType conceptualEntityType, EntityType storageEntityType, EntityTypeMappingKind kind,
            bool createIfNoneFound)
        {
            Debug.Assert(conceptualEntityType != null, "null conceptualEntityType");
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "conceptualEntityType is not CSDL");
            Debug.Assert(storageEntityType != null, "null storageEntityType");
            Debug.Assert(storageEntityType.EntityModel.IsCSDL != true, "storageEntityType is not SSDL");

            // the S-Side entitySet
            var ses = storageEntityType.EntitySet as StorageEntitySet;
            Debug.Assert(ses != null, "FindMappingFragment: StorageEntitySet is null");

            // find/create the ETM for this entity
            var entityTypeMapping = FindEntityTypeMapping(cpc, conceptualEntityType, kind, false);
            MappingFragment mappingFragment = null;
            if (entityTypeMapping == null)
            {
                // we don't want to create, and we don't even have an ETM, then just return null
                if (createIfNoneFound == false)
                {
                    return null;
                }

                var createETM = new CreateEntityTypeMappingCommand(
                    conceptualEntityType as ConceptualEntityType,
                    kind);
                CommandProcessor.InvokeSingleCommand(cpc, createETM);
                entityTypeMapping = createETM.EntityTypeMapping;
            }
            else
            {
                // found an existing ETM, is there also a mapping fragment already?
                foreach (var frag in entityTypeMapping.MappingFragments())
                {
                    if (frag.StoreEntitySet.Target == ses)
                    {
                        mappingFragment = frag;
                        break;
                    }
                }
            }
            Debug.Assert(entityTypeMapping != null, "entityTypeMapping is null");

            // if couldn't find an existing fragment, create it
            if (mappingFragment == null
                && createIfNoneFound
                && cpc != null)
            {
                var cmd = new CreateMappingFragmentCommand(conceptualEntityType, storageEntityType, kind);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                mappingFragment = cmd.MappingFragment;
                Debug.Assert(mappingFragment != null, "MappingFragment from CreateMappingFragmentCommand should not be null");
            }

            return mappingFragment;
        }

        internal static ScalarProperty FindFragmentScalarProperty(EntityType conceptualEntityType, Property tableColumn)
        {
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "conceptualEntityType.EntityModel should be C-side");
            Debug.Assert(tableColumn.EntityModel.IsCSDL == false, "tableColumn.EntityModel should not be C-side");

            foreach (var sp in tableColumn.GetAntiDependenciesOfType<ScalarProperty>())
            {
                if (sp.MappingFragment != null
                    && sp.MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType == conceptualEntityType)
                {
                    return sp;
                }
            }

            return null;
        }

        internal static ComplexProperty FindFragmentComplexProperty(EntityType conceptualEntityType, Property entityProperty)
        {
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "conceptualEntityType.EntityModel should be C-side");
            Debug.Assert(entityProperty is ComplexConceptualProperty, "Only ComplexConceptualProperty can be mapped to ComplexProperty");

            foreach (var cp in entityProperty.GetAntiDependenciesOfType<ComplexProperty>())
            {
                if (cp.MappingFragment != null
                    && cp.MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType == conceptualEntityType)
                {
                    return cp;
                }
            }

            return null;
        }

        internal static Condition FindFragmentCondition(EntityType conceptualEntityType, Property tableColumn)
        {
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "conceptualEntityType.EntityModel should be C-side");
            Debug.Assert(tableColumn.EntityModel.IsCSDL == false, "tableColumn.EntityModel should not be C-side");

            foreach (var cond in tableColumn.GetAntiDependenciesOfType<Condition>())
            {
                if (cond.MappingFragment != null
                    && cond.MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType == conceptualEntityType)
                {
                    return cond;
                }
            }

            return null;
        }

        internal static ModificationFunction FindModificationFunction(
            CommandProcessorContext cpc, EntityType conceptualEntityType, Function storageFunction, ModificationFunctionType functionType)
        {
            Debug.Assert(conceptualEntityType != null, "conceptualEntityType should not be null");
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "conceptualEntityType.EntityModel should be C-side");
            Debug.Assert(storageFunction != null, "storageFunction should not be null");
            Debug.Assert(functionType != ModificationFunctionType.None, "You cannot pass the None type");

            // find/create the ETM for this entity
            var entityTypeMapping = FindEntityTypeMapping(cpc, conceptualEntityType, EntityTypeMappingKind.Function, false);
            ModificationFunction modificationFunction = null;
            if (entityTypeMapping == null)
            {
                return null;
            }
            else
            {
                Debug.Assert(
                    entityTypeMapping.ModificationFunctionMapping != null,
                    "entityTypeMapping.ModificationFunctionMapping should not be null");
                var mfm = entityTypeMapping.ModificationFunctionMapping;

                if (mfm.InsertFunction != null
                    && mfm.InsertFunction.FunctionName.Target == storageFunction
                    && functionType == ModificationFunctionType.Insert)
                {
                    modificationFunction = mfm.InsertFunction;
                }
                else if (mfm.UpdateFunction != null
                         && mfm.UpdateFunction.FunctionName.Target == storageFunction
                         && functionType == ModificationFunctionType.Update)
                {
                    modificationFunction = mfm.UpdateFunction;
                }
                else if (mfm.DeleteFunction != null
                         && mfm.DeleteFunction.FunctionName.Target == storageFunction
                         && functionType == ModificationFunctionType.Delete)
                {
                    modificationFunction = mfm.DeleteFunction;
                }
            }

            return modificationFunction;
        }

        /// <summary>
        ///     This method gets all the Anti-Deps of type T of the passed in EFObject, and then
        ///     looks to see if those anti-deps are part of a mapping ghost node.
        /// </summary>
        /// <typeparam name="T">Must be an EFElement or derived type.</typeparam>
        /// <param name="obj">The object for which we are checking anti-deps</param>
        /// <returns>True if it found a ghost node</returns>
        internal static bool IsAntiDepPartOfGhostMappingNode<T>(EFObject obj) where T : EFElement
        {
            foreach (EFObject antiDep in obj.GetAntiDependenciesOfType<T>())
            {
                if (IsPartOfGhostMappingNode(antiDep))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     This should only be called for Mapping items.  This will walk up until it finds
        ///     the parent EntitySetMapping for the object passed in.  If doesn't find one, this
        ///     returns false.  If it finds an ESM, it will recursively walk the children of the ESM
        ///     looking to see if any children are ghost nodes.
        /// </summary>
        /// <param name="obj">Should be a child item of an EntitySetMappings, or else this always returns false</param>
        /// <returns>True if it found a ghost node</returns>
        internal static bool IsPartOfGhostMappingNode(EFObject obj)
        {
            var esm = obj.GetParentOfType(typeof(EntitySetMapping)) as EntitySetMapping;
            if (esm == null)
            {
                return false;
            }
            else
            {
                var foundGhostNode = false;
                IsPartOfGhostMappingNodeRecurse(esm, ref foundGhostNode);

                return foundGhostNode;
            }
        }

        /// <summary>
        ///     Private method that recurses into children looking for ghost nodes.
        /// </summary>
        private static void IsPartOfGhostMappingNodeRecurse(EFObject obj, ref bool foundGhostNode)
        {
            var element = obj as EFElement;
            if (element != null)
            {
                if (element.IsGhostNode)
                {
                    foundGhostNode = true;
                    return;
                }

                foreach (var child in element.Children)
                {
                    IsPartOfGhostMappingNodeRecurse(child, ref foundGhostNode);
                    if (foundGhostNode)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     Get entity model property max length facet value.
        /// </summary>
        /// <param name="columnSize"></param>
        /// <returns></returns>
        internal static StringOrPrimitive<uint> GetMaxLengthFacetValue(uint? columnSize)
        {
            StringOrPrimitive<uint> maxSize = null;
            if (columnSize != null)
            {
                // Decide whether to return "Max" or the columnSize parameter value.
                // Ideally we just need to check if columnSize value is equal to Int32.MaxValue; but unfortunately this does not work for some DB unicode types for example NText.
                // For NText and other unicode type, if the columnSize is Int32.MaxValue/2, MaxLength should be set to "Max".
                // I think it is pretty safe to assume if the columnSize >= Int32.MaxValue/2 then set the MaxLength to "Max" since in SQL Server you could not set column size to be greater than 8000.
                if (columnSize >= (Int32.MaxValue / 2))
                {
                    maxSize = Property.MaxLengthMaxValueObject;
                }
                else
                {
                    maxSize = new StringOrPrimitive<uint>(columnSize.Value);
                }
            }
            return maxSize;
        }

        /// <summary>
        ///     Get entity model property max length facet text.
        /// </summary>
        /// <param name="columnSize"></param>
        /// <returns></returns>
        internal static string GetMaxLengthFacetText(uint? columnSize)
        {
            if (columnSize == null)
            {
                return String.Empty;
            }
                // Decide whether to return "Max" or the columnSize parameter value.
                // Ideally we just need to check if columnSize value is equal to Int32.MaxValue; but unfortunately this does not work for some DB unicode types for example NText.
                // For NText and other unicode type, if the columnSize is Int32.MaxValue/2, MaxLength should be set to "Max".
                // I think it is pretty safe to assume if the columnSize >= Int32.MaxValue/2 then set the MaxLength to "Max" since in SQL Server you could not set a column size to be greater than 8000.
            else if (columnSize >= (Int32.MaxValue / 2))
            {
                return Property.MaxLengthMaxValue;
            }
            else
            {
                return columnSize.ToString();
            }
        }

        internal static bool CheckForCircularInheritance(ConceptualEntityType derivedType, ConceptualEntityType baseType)
        {
            Debug.Assert(derivedType != null, "derivedType should not be null");
            var circularInheritance = false;

            var visited = new Dictionary<EntityType, int>();
            visited.Add(derivedType, 0);

            while (baseType != null)
            {
                if (visited.ContainsKey(baseType))
                {
                    circularInheritance = true;
                    break;
                }
                visited.Add(baseType, 0);
                baseType = baseType.BaseType.Target;
            }

            return circularInheritance;
        }

        /// <summary>
        ///     Checks if there exists a circular loop in the Complex Type definition
        /// </summary>
        /// <param name="complexType"></param>
        /// <returns>True if there is a cycle that includes passed ComplexType</returns>
        internal static bool ContainsCircularComplexTypeDefinition(ComplexType complexType)
        {
            var visited = new HashSet<ComplexType>();
            foreach (var property in complexType.Properties())
            {
                var complexProperty = property as ComplexConceptualProperty;
                if (complexProperty != null
                    && complexProperty.ComplexType.Status == BindingStatus.Known)
                {
                    if (ContainsCircularComplexTypeDefinition(complexType, complexProperty.ComplexType.Target, visited))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if adding a new complex property would create a circular loop in the Complex Type definition
        /// </summary>
        /// <param name="complexType"></param>
        /// <param name="newPropertyType"></param>
        /// <returns>True if there adding new property would introduce a cycle</returns>
        internal static bool ContainsCircularComplexTypeDefinition(ComplexType complexType, ComplexType newPropertyType)
        {
            return ContainsCircularComplexTypeDefinition(complexType, newPropertyType, new HashSet<ComplexType>());
        }

        /// <summary>
        ///     Looks for a cycle in the ComplexTypes graph that includes baseComplexType using DFS algorithm
        /// </summary>
        /// <param name="baseComplexType"></param>
        /// <param name="currentComplexType"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private static bool ContainsCircularComplexTypeDefinition(
            ComplexType baseComplexType, ComplexType currentComplexType, HashSet<ComplexType> visited)
        {
            if (visited.Contains(currentComplexType))
            {
                return false;
            }
            if (baseComplexType == currentComplexType)
            {
                return true;
            }
            visited.Add(currentComplexType);
            foreach (var property in currentComplexType.Properties())
            {
                var complexProperty = property as ComplexConceptualProperty;
                if (complexProperty != null
                    && complexProperty.ComplexType.Status == BindingStatus.Known)
                {
                    if (ContainsCircularComplexTypeDefinition(baseComplexType, complexProperty.ComplexType.Target, visited))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region Error reporting

        internal static void InvalidSchemaError(string formatString, params object[] args)
        {
            var message = string.Format(CultureInfo.CurrentCulture, formatString, args)
                + " " + Resources.OperationRequiresValidSchema;
            throw new InvalidSchemaException(message);
        }

        /// <summary>
        ///     Exception thrown upon encountering an unresolved reference
        ///     Used in operations that require a valid model
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        [Serializable]
        internal class InvalidSchemaException : Exception
        {
            internal InvalidSchemaException(string message)
                : base(message)
            {
            }
        }

        #endregion

        #region Unique property and entity type names

        /// <summary>
        ///     Produces a property name that does not exist in the given entity type
        ///     A suffix is added to given candidate name to make it unique if necessary
        /// </summary>
        /// <param name="propertyNameCandidate">Candidate property name</param>
        /// <param name="entityType">Entity type in which the property name should be unique</param>
        /// <param name="namesToAvoid">Entity type in which the property name should be unique</param>
        /// <param name="childEFObjectsToIgnore">Entity type in which the property name should be unique</param>
        /// <param name="alwaysAddSuffix">Entity type in which the property name should be unique</param>
        /// <returns>Unique property name</returns>
        internal static string GetUniqueConceptualPropertyName(
            string propertyNameCandidate, EntityType entityType, HashSet<string> namesToAvoid = null,
            HashSet<EFObject> childEFObjectsToIgnore = null, bool alwaysAddSuffix = false)
        {
            Debug.Assert(
                EscherAttributeContentValidator.IsValidCsdlPropertyName(
                    alwaysAddSuffix ? propertyNameCandidate + DefaultPropertySuffixSeed : propertyNameCandidate),
                    "Candidate property name is not valid");

            var cEntityType = entityType as ConceptualEntityType;
            Debug.Assert(cEntityType != null, "Why isn't the entity type a conceptual entity type?");

            var allDistinctPropertyNamesInInheritanceTree =
                (from p in cEntityType.SafePropertiesInInheritanceHierarchy
                 select p.LocalName.Value).Distinct();

            namesToAvoid = namesToAvoid != null ? 
                new HashSet<string>(namesToAvoid.Union(allDistinctPropertyNamesInInheritanceTree)) : 
                new HashSet<string>(allDistinctPropertyNamesInInheritanceTree);

            var propertyName = GetUniquePropertyName(
                propertyNameCandidate, entityType, namesToAvoid, childEFObjectsToIgnore,
                alwaysAddSuffix ? DefaultPropertySuffixSeed : (int?)null);

            Debug.Assert(
                EscherAttributeContentValidator.IsValidCsdlPropertyName(propertyName),
                "ModelHelper.GetUniqueConceptualPropertyName(): Generated non-valid unique property name");

            return propertyName;
        }

        private static string GetUniquePropertyName(
            string propertyNameCandidate, EntityType entityType, HashSet<string> namesToAvoid,
            HashSet<EFObject> childEFObjectsToIgnore = null, int? initialSuffix = null)
        {
            string propertyName = null;
            var suffix = initialSuffix == null ? "" : "" + initialSuffix.Value;

            namesToAvoid = namesToAvoid ?? new HashSet<string>();

            // Properties cannot have the same name as their EntityType.
            if (namesToAvoid.Contains(entityType.Name.Value, StringComparer.CurrentCulture) == false)
            {
                namesToAvoid.Add(entityType.Name.Value);
            }

            for (var i = DefaultPropertySuffixSeed;; i++)
            {
                propertyName = propertyNameCandidate + suffix;

                if (IsUniquePropertyName(entityType, propertyName, true /* case sensitive */, childEFObjectsToIgnore))
                {
                    // check if the propertyName is not equal to the entity name
                    if (!entityType.LocalName.Value.Equals(propertyName, StringComparison.CurrentCulture))
                    {
                        if (namesToAvoid == null
                            || !namesToAvoid.Contains(propertyName))
                        {
                            break;
                        }
                    }
                }

                suffix = i.ToString(CultureInfo.CurrentCulture);
            }

            return propertyName;
        }

        /// <summary>
        ///     Constructs a proposed EntitySet name given an EntityType name
        /// </summary>
        internal static string ConstructProposedEntitySetName(EFArtifact artifact, string entityTypeName)
        {
            IPluralizationService pluralizationService = null;
            var pluralize = GetDesignerPropertyValueFromArtifactAsBool(
                OptionsDesignerInfo.ElementName,
                OptionsDesignerInfo.AttributeEnablePluralization, OptionsDesignerInfo.EnablePluralizationDefault, artifact);
            if (pluralize)
            {
                pluralizationService = DependencyResolver.GetService<IPluralizationService>();
            }

            return ConstructProposedEntitySetName(pluralizationService, entityTypeName);
        }

        /// <summary>
        ///     Constructs a proposed EntitySet name given an EntityType name
        /// </summary>
        internal static string ConstructProposedEntitySetName(IPluralizationService pluralizationService, string entityTypeName)
        {
            if (string.IsNullOrEmpty(entityTypeName))
            {
                // if entityTypeName is null or empty just return same value
                return entityTypeName;
            }

            if (pluralizationService == null)
            {
                // indicates pluralization not needed so just return entityTypeName with "Set" suffix
                return entityTypeName + Resources.Model_DefaultEntitySetSuffix;
            }

            if (!char.IsLetterOrDigit(entityTypeName[entityTypeName.Length - 1]))
            {
                // if entityTypeName ends with a non-alphanumeric character then add "Set" to end
                return entityTypeName + Resources.Model_DefaultEntitySetSuffix;
            }
            else
            {
                // otherwise attempt to pluralize the entityTypeName
                return pluralizationService.Pluralize(entityTypeName);
            }
        }

        /// <summary>
        ///     Constructs a proposed NavigationProperty name given an EntityType name and the multiplicity
        /// </summary>
        /// <param name="pluralizationService"></param>
        /// <param name="otherEndEntityName">name of the EntityType to which this NavigationProperty will point</param>
        /// <param name="otherEndMultiplicity">Note: should be a constant value from ModelConstants, not a localized text string</param>
        /// <returns></returns>
        internal static string ConstructProposedNavigationPropertyName(
            IPluralizationService pluralizationService,
            string otherEndEntityName, string otherEndMultiplicity)
        {
            Debug.Assert(!string.IsNullOrEmpty(otherEndEntityName), "otherEndEntityName should not be null or empty");
            Debug.Assert(!string.IsNullOrEmpty(otherEndMultiplicity), "otherEndMultiplicity should not be null or empty");
            if (string.IsNullOrEmpty(otherEndEntityName))
            {
                // should never happen - just for safety
                return string.Empty;
            }
            if (string.IsNullOrEmpty(otherEndMultiplicity))
            {
                // cannot determine other end multiplicity - just return otherEndEntityName
                return otherEndEntityName;
            }

            if (pluralizationService == null)
            {
                // indicates pluralization not needed so just return otherEndEntityName
                return otherEndEntityName;
            }
            else
            {
                if (ModelConstants.Multiplicity_Many.Equals(otherEndMultiplicity))
                {
                    // other end multiplicity is plural and other end has non-plural name so pluralize the name
                    return pluralizationService.Pluralize(otherEndEntityName);
                }
                else
                {
                    // other end multiplicity is non-plural and other end has plural name so singularize the name
                    return pluralizationService.Singularize(otherEndEntityName);
                }
            }
        }

        /// <summary>
        ///     Create a mapping between a column name and the generated property name.
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        internal static IDictionary<string, string> ConstructComplexTypePropertyNameToColumnNameMapping(IList<string> columnNames)
        {
            IDictionary<string, string> map = new Dictionary<string, string>();
            foreach (var columnName in columnNames)
            {
                var propertyName = CreateValidSimpleIdentifier(columnName);
                Debug.Assert(false == map.ContainsKey(propertyName), "Duplicate candidate property name is found: " + propertyName);
                if (false == map.ContainsKey(propertyName))
                {
                    map.Add(propertyName, columnName);
                }
            }

            return map;
        }

        /// <summary>
        ///     Compare if an entity model property is equivalent to the schema column.
        ///     The comparison will check that the types are equivalent and that the following facets match:
        ///     - Nullable
        ///     - Size
        ///     - Scale
        ///     - Precision
        /// </summary>
        /// <param name="storageModel"></param>
        /// <param name="property"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static bool IsPropertyEquivalentToSchemaColumn(
            StorageEntityModel storageModel, Property property, IRawDataSchemaColumn column)
        {
            Debug.Assert(column != null, "The passed in data schema column is null");
            Debug.Assert(property != null, "The passed in entity property column is null");

            if (column != null
                && property != null)
            {
                var columnPrimitiveType = GetPrimitiveType(storageModel, column.NativeDataType, column.ProviderDataType);
                var propertyPrimitiveType = GetPrimitiveTypeFromString(property.TypeName);

                // if columnPrimitiveType is null, this could be because the column type is unsupported.
                if (columnPrimitiveType == null)
                {
                    return false;
                }

                // if propertyPrimitiveType is null, this could be because the property is a complex property.
                if (propertyPrimitiveType == null)
                {
                    return false;
                }

                // check their CLR equivalent type.
                if (columnPrimitiveType.ClrEquivalentType != propertyPrimitiveType.ClrEquivalentType)
                {
                    return false;
                }

                // Check Nullable attribute
                if (BoolOrNoneComparison.Equal != property.Nullable.CompareToUsingDefault(column.IsNullable))
                {
                    return false;
                }

                // Check MaxLength/size attribute.
                if (column.Size != null
                    && GetMaxLengthFacetText(column.Size) != property.MaxLength.Value.ToString())
                {
                    return false;
                }

                // Check Scale attribute
                if (column.Scale != null
                    && column.Scale.Value != property.Scale.GetAsNullableUInt())
                {
                    return false;
                }

                // Check Precision attribute
                if (column.Precision != null
                    && column.Precision.Value != property.Precision.GetAsNullableUInt())
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        internal static NavigationProperty FindNavigationPropertyForFunctionAssociationEnd(FunctionAssociationEnd functionAssocEnd)
        {
            if (null == functionAssocEnd)
            {
                Debug.Fail("Null FunctionAssociationEnd");
                return null;
            }

            var functionAssocSetFromEnd = functionAssocEnd.From.Target;
            if (null == functionAssocSetFromEnd)
            {
                Debug.Fail("Null From Target for FunctionAssociationEnd " + functionAssocEnd.ToPrettyString());
                return null;
            }

            var functionAssocSetToEnd = functionAssocEnd.To.Target;
            if (null == functionAssocSetFromEnd)
            {
                Debug.Fail("Null To Target for FunctionAssociationEnd " + functionAssocEnd.ToPrettyString());
                return null;
            }

            var functionAssocFromEnd = functionAssocSetFromEnd.Role.Target;
            if (null == functionAssocFromEnd)
            {
                Debug.Fail("Null Role Target for From AssociationSetEnd " + functionAssocSetFromEnd.ToPrettyString());
                return null;
            }

            var functionAssocToEnd = functionAssocSetToEnd.Role.Target;
            if (null == functionAssocToEnd)
            {
                Debug.Fail("Null Role Target for To AssociationSetEnd " + functionAssocSetToEnd.ToPrettyString());
                return null;
            }

            var assocSet = functionAssocEnd.AssociationSet.Target;
            if (null == assocSet)
            {
                Debug.Fail("Null AssociationSet for FunctionAssociationEnd " + functionAssocEnd.ToPrettyString());
                return null;
            }

            foreach (var assocSetEnd in assocSet.AssociationSetEnds())
            {
                var assocEnd = assocSetEnd.Role.Target;
                if (null != assocEnd)
                {
                    // Note: if we have an inheritance tree with 1 NavProp pointing from
                    // one class of the tree (C1) to another class (C2) of the tree and
                    // another NavProp pointing back from C2 to C1 then need to check
                    // the From and To roles to ensure we get the NavProp.
                    var navPropsFromAssocEnd =
                        assocEnd.GetAntiDependenciesOfType<NavigationProperty>();
                    if (null != navPropsFromAssocEnd)
                    {
                        foreach (var navProp in navPropsFromAssocEnd)
                        {
                            var navPropFromRoleAssocEnd = navProp.FromRole.Target;
                            var navPropToRoleAssocEnd = navProp.ToRole.Target;
                            if (functionAssocFromEnd == navPropFromRoleAssocEnd
                                && functionAssocToEnd == navPropToRoleAssocEnd)
                            {
                                return navProp;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // Note: need fromRoleEnd to distinguish NavigationProperties in self-associations
        internal static NavigationProperty FindNavigationPropertyForAssociationEnd(ConceptualEntityType et, AssociationEnd fromRoleEnd)
        {
            Debug.Assert(et != null, "Null EntityType");
            Debug.Assert(fromRoleEnd != null, "Null FromRole AssociationEnd");

            var assoc = fromRoleEnd.Parent as Association;
            Debug.Assert(assoc != null, "FromRole AssociationEnd " + fromRoleEnd.ToPrettyString() + " has null Association");

            foreach (var selfOrBaseType in et.SafeSelfAndBaseTypes)
            {
                foreach (var navProp in selfOrBaseType.NavigationProperties())
                {
                    if (navProp.Relationship != null
                        && navProp.Relationship.Target == assoc
                        && navProp.FromRole != null
                        && navProp.FromRole.Target == fromRoleEnd)
                    {
                        return navProp;
                    }
                }
            }

            return null;
        }

        internal static NavigationProperty FindNavigationPropertyByName(ConceptualEntityType et, string navPropName)
        {
            foreach (var selfOrBaseType in et.SafeSelfAndBaseTypes)
            {
                foreach (var navProp in selfOrBaseType.NavigationProperties())
                {
                    if (navProp.LocalName.Value == navPropName)
                    {
                        return navProp;
                    }
                }
            }

            return null;
        }

        internal static NavigationProperty FindNavigationPropertyForFunctionScalarProperty(FunctionScalarProperty fsp)
        {
            // this function scalar property does not exist within an association
            if (fsp.AssociationEnd == null)
            {
                return null;
            }

            var navProp = FindNavigationPropertyForFunctionAssociationEnd(fsp.AssociationEnd);
            return navProp;
        }

        internal static EntitySetMapping FindEntitySetMappingForEntityType(EntityType et)
        {
            Debug.Assert(et != null, "Null EntityType");

            var entitySet = et.EntitySet;
            if (entitySet == null)
            {
                return null;
            }

            // return first found EntitySetMapping
            var entitySetMappings = entitySet.GetAntiDependenciesOfType<EntitySetMapping>();
            foreach (var esm in entitySetMappings)
            {
                return esm;
            }

            return null;
        }

        internal static AssociationSetMapping FindAssociationSetMappingForConceptualAssociation(Association assoc)
        {
            Debug.Assert(assoc != null, "Null Association");
            Debug.Assert(assoc.EntityModel.IsCSDL, "Association must be a C-side Association");

            var assocSet = assoc.AssociationSet;
            if (assocSet == null)
            {
                return null;
            }

            var asms = assoc.GetAntiDependenciesOfType<AssociationSetMapping>();
            foreach (var asm in asms)
            {
                return asm;
            }

            return null;
        }

        internal static Property FindPropertyForEntityTypeMapping(EntityTypeMapping etm, string propName)
        {
            Debug.Assert(etm != null, "Null EntityTypeMapping");

            var et = etm.FirstBoundConceptualEntityType;
            if (et == null)
            {
                return null;
            }

            var prop = FindProperty(et, propName);
            if (prop != null)
            {
                return prop;
            }

            // if can't find property on FirstBoundConceptualEntityType
            // then it could be a key property on the base type
            et = et.ResolvableTopMostBaseType;
            if (et != null)
            {
                foreach (var p in et.ResolvableKeys)
                {
                    if (propName == p.LocalName.Value)
                    {
                        return p;
                    }
                }
            }

            return null;
        }

        internal static Property FindProperty(EntityType et, string propName)
        {
            Debug.Assert(et != null, "Null EntityType");
            if (et != null)
            {
                foreach (var prop in et.Properties())
                {
                    if (propName == prop.LocalName.Value)
                    {
                        return prop;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Searches for set of properties defined by provided names.
        /// </summary>
        /// <param name="entity">Entity to find properties of</param>
        /// <param name="propertyNames">Set of names of properties to search for</param>
        /// <returns>Set of found properties or null if at least one property was not found</returns>
        internal static IEnumerable<Property> FindProperties(EntityType entity, IEnumerable<string> propertyNames)
        {
            var list = new List<Property>();
            foreach (var name in propertyNames)
            {
                var property = FindProperty(entity, name);
                if (property != null)
                {
                    list.Add(property);
                }
                else
                {
                    Debug.Fail("Could not find property named " + name + " in Entity " + entity);
                    return null;
                }
            }
            return list;
        }

        internal static Function FindFunction(StorageEntityModel sem, DatabaseObject functionIdentity)
        {
            Debug.Assert(sem != null, "StorageEntityModel should not be null");

            foreach (var f in sem.Functions())
            {
                var dbObj = DatabaseObject.CreateFromFunction(f);
                if (dbObj.Equals(functionIdentity))
                {
                    return f;
                }
            }

            return null;
        }

        /// <summary>
        ///     Finds the first scalar Property with the specified local name in the Entity Properties tree
        ///     (i.e. including all Properties from all Complex Properties on any level)
        /// </summary>
        /// <param name="entity">Entity to search in</param>
        /// <param name="propName">Searched Property local name</param>
        /// <param name="properties">Path to the found Property in the Entity Properties tree</param>
        /// <returns>True if the scalar Property is found</returns>
        internal static bool FindScalarPropertyPathByLocalName(EntityType entity, string propName, out List<Property> properties)
        {
            properties = new List<Property>();
            foreach (var property in entity.Properties())
            {
                var complexProperty = property as ComplexConceptualProperty;
                if (complexProperty != null)
                {
                    if (FindScalarPropertyPathByLocalName(complexProperty, propName, properties))
                    {
                        properties.Insert(0, complexProperty);
                        return true;
                    }
                }
                else
                {
                    if (property.LocalName.Value.Equals(propName, StringComparison.CurrentCulture))
                    {
                        properties.Add(property);
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindScalarPropertyPathByLocalName(
            ComplexConceptualProperty complexProperty, string propName, List<Property> properties)
        {
            if (complexProperty.ComplexType.Status == BindingStatus.Known)
            {
                foreach (var property in complexProperty.ComplexType.Target.Properties())
                {
                    var cp = property as ComplexConceptualProperty;
                    if (cp != null)
                    {
                        if (FindScalarPropertyPathByLocalName(cp, propName, properties))
                        {
                            properties.Insert(0, cp);
                            return true;
                        }
                    }
                    else
                    {
                        if (property.LocalName.Value.Equals(propName, StringComparison.CurrentCulture))
                        {
                            properties.Add(property);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static Association FindAssociation(BaseEntityModel entityModel, string name)
        {
            Debug.Assert(entityModel != null, "Null BaseEntityModel");

            if (entityModel != null
                && entityModel.Associations().Any())
            {
                return entityModel.Associations().FirstOrDefault(
                    a =>
                    String.Compare(name, a.LocalName.Value, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(name, a.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase) == 0);
            }

            return null;
        }

        internal static AssociationEnd FindAssociationEnd(Association assoc, string name)
        {
            Debug.Assert(assoc != null, "Null Association");

            foreach (var assocEnd in assoc.AssociationEnds())
            {
                if (name == assocEnd.Role.Value)
                {
                    return assocEnd;
                }
            }

            return null;
        }

        internal static AssociationSet FindAssociationSet(BaseEntityModel entityModel, string name)
        {
            Debug.Assert(entityModel != null, "Null BaseEntityModel");

            if (entityModel != null
                && entityModel.FirstEntityContainer != null)
            {
                return entityModel.FirstEntityContainer.AssociationSets().FirstOrDefault(
                    a =>
                    String.Compare(name, a.LocalName.Value, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(name, a.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase) == 0);
            }

            return null;
        }

        internal static EntityType FindEntityType(BaseEntityModel entityModel, string name)
        {
            Debug.Assert(entityModel != null, "Null BaseEntityModel");

            foreach (var et in entityModel.EntityTypes())
            {
                if (String.Compare(name, et.LocalName.Value, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(name, et.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return et;
                }
            }

            return null;
        }

        internal static EntityType FindEntityTypeViaSymbol(BaseEntityModel entityModel, string name)
        {
            EntityType foundEntityType = null;
            var normalizedEntityName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(entityModel, name);
            if (normalizedEntityName != null)
            {
                foundEntityType = entityModel.Artifact.ArtifactSet.LookupSymbol(normalizedEntityName.Symbol) as EntityType;
            }
            return foundEntityType;
        }

        /// <summary>
        ///     Find complex-type in the model.
        /// </summary>
        internal static ComplexType FindComplexType(ConceptualEntityModel cModel, string name)
        {
            Debug.Assert(cModel != null, "Null ConceptualEntityModel");

            foreach (var ct in cModel.ComplexTypes())
            {
                if (String.Compare(name, ct.LocalName.Value, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(name, ct.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ct;
                }
            }

            return null;
        }

        /// <summary>
        ///     Find enum type in the model.
        /// </summary>
        internal static EnumType FindEnumType(ConceptualEntityModel cModel, string name)
        {
            Debug.Assert(cModel != null, "Null ConceptualEntityModel");

            foreach (var et in cModel.EnumTypes())
            {
                if (String.Equals(name, et.LocalName.Value, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(name, et.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase))
                {
                    return et;
                }
            }

            return null;
        }

        /// <summary>
        ///     This will return either the ComplexType, EntityType, null, or the primitive type name
        /// </summary>
        internal static object FindComplexTypeEntityTypeOrPrimitiveTypeForFunctionImportReturnType(
            ConceptualEntityModel cModel, string returnTypeAsString)
        {
            if (returnTypeAsString == Tools.XmlDesignerBase.Resources.NoneDisplayValueUsedForUX)
            {
                return Tools.XmlDesignerBase.Resources.NoneDisplayValueUsedForUX;
            }

            // this will remove the 'Collection' around the return type. It will now either be 'None',
            // the primitive type name, complex type name, or entity name.
            var desanitizedReturnType = UnwrapCollectionAroundFunctionImportReturnType(returnTypeAsString, true);

            // if this is a complex type name, attempt to get the complex type object
            object returnTypeObject = FindComplexType(cModel, desanitizedReturnType);
            if (null != returnTypeObject)
            {
                return returnTypeObject;
            }

            // if this is an entity type name, attempt to get the entity type object
            returnTypeObject = FindEntityType(cModel, desanitizedReturnType);
            if (null != returnTypeObject)
            {
                return returnTypeObject;
            }

            // then this is a primitive type. Do a double-check here...
            Debug.Assert(
                AllPrimitiveTypes(cModel.Artifact.SchemaVersion).Contains(desanitizedReturnType),
                "Why isn't the return type of the function import either 'None', a primitive type, or an entity type?");

            returnTypeObject = desanitizedReturnType;
            return returnTypeObject;
        }

        /// <summary>
        ///     Given the runtime format for the return type: "Collection([namespace-qualified return type])", return back either:
        ///     1. The namespace-(Un)qualified EntityType name.
        ///     2. The namespace-(Un)qualified ComplexType name.
        ///     3. The primitive type name
        ///     4. "(None)" if the passed in string is null.
        /// </summary>
        internal static string UnwrapCollectionAroundFunctionImportReturnType(string returnTypeAsString, bool returnFullyQualifiedName)
        {
            if (returnTypeAsString == null)
            {
                return Tools.XmlDesignerBase.Resources.NoneDisplayValueUsedForUX;
            }
            Match collectionReturnTypeMatch = null;

            if (returnFullyQualifiedName)
            {
                collectionReturnTypeMatch = _collectionNamespaceQualifiedReturnTypePattern.Match(returnTypeAsString);
            }
            else
            {
                collectionReturnTypeMatch = _collectionReturnTypePattern.Match(returnTypeAsString);
            }

            if (collectionReturnTypeMatch.Success)
            {
                var gcoll = collectionReturnTypeMatch.Groups;
                return gcoll[CollectionCaptureName].Value;
            }
            return returnTypeAsString;
        }

        internal static string UnwrapCollectionAroundFunctionImportReturnType(string returnTypeAsString)
        {
            return UnwrapCollectionAroundFunctionImportReturnType(returnTypeAsString, false);
        }

        /// <summary>
        ///     Given an artifact and a ModelSpace, this will return a specific EntityModel
        /// </summary>
        /// <returns></returns>
        internal static BaseEntityModel GetEntityModel(EFArtifact artifact, Command.ModelSpace modelSpace)
        {
            switch (modelSpace)
            {
                case Command.ModelSpace.Conceptual:
                    return artifact.ConceptualModel();
                case Command.ModelSpace.Storage:
                    return artifact.StorageModel();
            }

            Debug.Fail("Unknown data space");
            return null;
        }

        /// <summary>
        ///     A method for setting either the Value or the IsNull attributes on a condition.
        /// </summary>
        internal static void SetConditionPredicate(Condition cond, bool? isNull, string conditionValue)
        {
            Debug.Assert(cond != null, "cond should not be null");
            Debug.Assert(
                (isNull != null && string.IsNullOrEmpty(conditionValue)) ||
                (isNull == null && string.IsNullOrEmpty(conditionValue) == false) ||
                (isNull == null && string.IsNullOrEmpty(conditionValue)),
                "isNull(" + isNull + ") and conditionValue(" + (conditionValue == null ? "NULL" : conditionValue)
                + ") combination is incorrect");

            if (isNull == null)
            {
                // this will cause the XAttribute to be removed
                cond.IsNull.Value = null;

                // don't assert non-null Value here; condition fragments are created in this way
                cond.Value.Value = conditionValue;
            }
            else
            {
                // this will cause the XAttribute to be removed
                Debug.Assert(string.IsNullOrEmpty(conditionValue), "IsNull and Value condition predicates can't be set at the same time");
                cond.Value.Value = null;

                if (isNull == true)
                {
                    cond.IsNull.Value = Condition.IsNullConstant;
                }
                else
                {
                    cond.IsNull.Value = Condition.IsNotNullConstant;
                }
            }
        }

        /// <summary>
        ///     Return true if the property's type is a complex type.
        ///     Return false otherwise.
        ///     Note: for consistent behavior with the older designer, this method will also return true if the type reference is not found.
        /// </summary>
        internal static bool IsElementComplexProperty(XElement elem, ConceptualEntityModel conceptualModel)
        {
            string attrValue = null;
            var attr = elem.FirstAttribute;
            while (attr != null)
            {
                if (attr.Name.LocalName == Property.AttributeType)
                {
                    attrValue = attr.Value;
                    break;
                }
                attr = attr.NextAttribute;
            }

            // attrValue is null if there is no type attribute, obviously a mal-formed document, but possible
            if (attrValue != null)
            {
                // Check if it is one of the primitive types.
                var schemaVersion = SchemaManager.GetSchemaVersion(elem.Name.Namespace);
                if (AllPrimitiveTypes(schemaVersion).Contains(attrValue, StringComparer.Ordinal))
                {
                    return false;
                }
                    // Check if it is one of the enum types.
                else if (conceptualModel.EnumTypes().Any(et => attrValue.EndsWith(et.LocalName.Value, StringComparison.Ordinal)))
                {
                    return false;
                }
                // Since a name has to be unique across all enum types and complex types, we assume that the type must be a complex type.
                return true;
            }

            return true;
        }

        internal static PrimitiveType GetPrimitiveType(StorageEntityModel storageModel, string nativeDataType, int providerDataType)
        {
            PrimitiveType primitiveType = null;

            if (storageModel != null)
            {
                if (!string.IsNullOrEmpty(nativeDataType))
                {
                    primitiveType = storageModel.GetStoragePrimitiveType(nativeDataType);
                }
                else
                {
                    if (storageModel.Provider.Value == "System.Data.SqlClient")
                    {
                        var dataType = (SqlDbType)(providerDataType);
                        primitiveType = storageModel.GetStoragePrimitiveType(dataType.ToString());
                    }
                }
            }

            return primitiveType;
        }

        internal static string GetDesignerPropertyValueFromArtifact(
            string designerInfoElementName, string designerPropertyName, EFArtifact artifact)
        {
            DesignerInfo designerInfo;

            // First we will try to find the OptionsDesignerInfo corresponding to the <Options> element in the artifact.
            var designerInfoRoot = artifact.DesignerInfo();
            if (designerInfoRoot != null)
            {
                var foundDesignerInfo = designerInfoRoot.TryGetDesignerInfo(designerInfoElementName, out designerInfo);

                // Now we will attempt to look for a <DesignerInfoPropertySet> under the <Options> element. There may not be one.
                if (foundDesignerInfo
                    && designerInfo != null
                    && designerInfo.PropertySet != null)
                {
                    // If this succeeds, then we will try to find the DesignerProperty corresponding to "GenerateDatabaseScriptWorkflowPath"
                    DesignerProperty designerProperty;
                    if (designerInfo.PropertySet.TryGetDesignerProperty(designerPropertyName, out designerProperty))
                    {
                        // Finally if all of this succeeds, we'll return back the value of the GenerateDatabaseScriptWorkflowPath DesignerProperty
                        Debug.Assert(
                            designerProperty.ValueAttr != null, "DesignerProperty " + designerProperty.LocalName + "'s ValueAttr is null");
                        if (designerProperty.ValueAttr != null)
                        {
                            return designerProperty.ValueAttr.Value;
                        }
                    }
                }
            }

            return String.Empty;
        }

        internal static bool GetDesignerPropertyValueFromArtifactAsBool(
            string designerInfoElementName, string designerPropertyName, bool defaultValue, EFArtifact artifact)
        {
            var rtrn = defaultValue;
            var stringValue = GetDesignerPropertyValueFromArtifact(designerInfoElementName, designerPropertyName, artifact);
            if (!string.IsNullOrEmpty(stringValue))
            {
                try
                {
                    rtrn = bool.Parse(stringValue);
                }
                catch (FormatException)
                {
                    // this value cannot be interpreted as a bool; just assume default
                    Debug.Fail("Cannot interpret string value " + stringValue + " as a bool");
                    return defaultValue;
                }
            }

            return rtrn;
        }

        internal static Command CreateSetDesignerPropertyValueCommandFromArtifact(
            EFArtifact artifact, string designerInfoElementName, string designerPropertyName, string designerPropertyValue)
        {
            Debug.Assert(artifact != null, "artifact cannot be null");
            DesignerInfo designerInfo;
            var foundDesignerInfo = artifact.DesignerInfo().TryGetDesignerInfo(designerInfoElementName, out designerInfo);
            Debug.Assert(
                foundDesignerInfo && designerInfo != null,
                "Could not find the DesignerInfo: " + designerInfoElementName + ". Edmx might be corrupted");

            if (foundDesignerInfo)
            {
                return CreateSetDesignerPropertyCommandInsideDesignerInfo(designerInfo, designerPropertyName, designerPropertyValue);
            }

            return null;
        }

        internal static Command CreateSetDesignerPropertyCommandInsideDesignerInfo(
            DesignerInfo designerInfo, string designerPropertyName, string designerPropertyValue)
        {
            DesignerProperty designerProperty;
            if (designerInfo.PropertySet.TryGetDesignerProperty(designerPropertyName, out designerProperty)
                && designerProperty.ValueAttr != null)
            {
                if (!string.Equals(designerPropertyValue, designerProperty.ValueAttr.Value))
                {
                    return new UpdateDefaultableValueCommand<string>(designerProperty.ValueAttr, designerPropertyValue);
                }
            }
            else
            {
                if (designerPropertyValue != null)
                {
                    return new ChangeDesignerPropertyCommand(designerPropertyName, designerPropertyValue, designerInfo);
                }
            }

            return null;
        }

        internal enum DeterminePrincipalDependentAssociationEndsScenario
        {
            InferReferentialConstraint,
            CreateForeignKeyProperties
        }

        /// <summary>
        ///     see spec section 4.3.1 (RC) or 3.4.2 (FKs)
        ///     determine which is the principal and which is the dependent end, to do this
        ///     we inspect the end multiplicities:
        ///     End1 Multiplicity | End2 Multiplicity | Principal Role
        ///     1                 | 0..1              | 1
        ///     1                 | *                 | 1
        ///     1                 | 1                 | See below
        ///     *                 | *                 | n/a
        ///     0..1              | *                 | 0..1
        /// </summary>
        /// <param name="association">association to check</param>
        /// <param name="principal">will be set to one of the ends, could be null</param>
        /// <param name="dependent">will be set to one of the ends, could be null</param>
        /// <param name="scenario">the scenario for which this is used</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static void DeterminePrincipalDependentAssociationEnds(
            Association association, out AssociationEnd principal, out AssociationEnd dependent,
            DeterminePrincipalDependentAssociationEndsScenario scenario)
        {
            principal = null;
            dependent = null;

            // local shortcuts
            var end1 = association.AssociationEnds()[0];
            var end2 = association.AssociationEnds()[1];

            Debug.Assert(end1 != null && end2 != null, "Null end found");
            if (end1 == null
                || end2 == null)
            {
                return;
            }

            // End1 Multiplicity | End2 Multiplicity | Principal Role
            // 1                 | 1                 | See below
            if (end1.Multiplicity.Value == ModelConstants.Multiplicity_One
                && end2.Multiplicity.Value == ModelConstants.Multiplicity_One)
            {
                if (scenario == DeterminePrincipalDependentAssociationEndsScenario.InferReferentialConstraint)
                {
                    var associationSet = association.AssociationSet;
                    if (associationSet != null)
                    {
                        var asm = associationSet.AssociationSetMapping;
                        if (asm != null
                            && asm.StoreEntitySet.Target != null
                            && end1.Type.Target != null
                            && end2.Type.Target != null)
                        {
                            var asmTable = asm.StoreEntitySet.Target.EntityType.Target;
                            var tablesMappedToEnd1 = GetTablesMappedFrom(end1.Type.Target);
                            var tablesMappedToEnd2 = GetTablesMappedFrom(end2.Type.Target);

                            // From Spec: When associations use multiplicities of 1:1, the dependent side is the one that 
                            // the association is mapped to. For example, consider a 1:1 association between TypeA and TypeB. 
                            // If the association is mapped to the same table that TypeB is mapped to, then TypeB’s end is the 
                            // dependent and TypeA’s the principal.
                            if (tablesMappedToEnd2.Contains(asmTable))
                            {
                                principal = end1;
                                dependent = end2;
                            }
                            else if (tablesMappedToEnd1.Contains(asmTable))
                            {
                                principal = end2;
                                dependent = end1;
                            }
                        }
                    }
                }
                else if (scenario == DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties)
                {
                    principal = end1;
                    dependent = end2;
                }
            }
            else if (end1.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne
                     && end2.Multiplicity.Value == ModelConstants.Multiplicity_Many
                     && scenario == DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties)
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // 0..1              | *                 | 0..1
                principal = end1;
                dependent = end2;
            }
            else if (end2.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne
                     && end1.Multiplicity.Value == ModelConstants.Multiplicity_Many
                     && scenario == DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties)
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // *                 | 0..1              | 0..1
                principal = end2;
                dependent = end1;
            }
            else if (end1.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne
                     && end2.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne
                     && scenario == DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties)
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // 0..1              | 0..1              | just pick end1
                principal = end1;
                dependent = end2;
            }
            else
            {
                if (end1.Multiplicity.Value == ModelConstants.Multiplicity_One
                    && (end2.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne ||
                        end2.Multiplicity.Value == ModelConstants.Multiplicity_Many))
                {
                    // End1 Multiplicity | End2 Multiplicity | Principal Role
                    // 1                 | 0..1              | 1
                    // 1                 | *                 | 1
                    principal = end1;
                    dependent = end2;
                }
                else if (end2.Multiplicity.Value == ModelConstants.Multiplicity_One
                         && (end1.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne ||
                             end1.Multiplicity.Value == ModelConstants.Multiplicity_Many))
                {
                    // End1 Multiplicity | End2 Multiplicity | Principal Role
                    // 0..1              | 1                 | 1
                    // *                 | 1                 | 1
                    principal = end2;
                    dependent = end1;
                }
            }
        }

        internal static void DeterminePrincipalDependentEndsForAnyAssociationType(
            Association association, out AssociationEnd principal, out AssociationEnd dependent)
        {
            if (association.IsManyToMany)
            {
                // We can't call DeterminePrincipalDependentAssociationEnds on ManyToMany associations, since there is no principal or dependent end for ManyToMany.
                // Instead we'll just assign the first end as the "principal" for the purposes of this method.
                principal = association.AssociationEnds()[0];
                dependent = association.AssociationEnds()[1];
            }
            else
            {
                DeterminePrincipalDependentAssociationEnds(
                    association, out principal, out dependent, DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties);
            }
        }

        /// <summary>
        ///     Returns the "non-EDMX" xml attributes & elements that are direct children of this element.
        /// </summary>
        internal static IEnumerable<XObject> GetStructuredAnnotationsForElement(EFElement efElement)
        {
            var annotationManager = new AnnotationManager(efElement.XElement, efElement.Artifact.SchemaVersion);

            return annotationManager.GetAnnotations();
        }

        /// <summary>
        ///     Return the list of entity-types that are related (through association or direct inheritance) to the passed in entity-type.
        ///     TODO: there are common functionality between this method and EntityTypeShape's CreateEntityTypeShapeAndConnectorsInDiagram,
        ///     We should be able to extract common functionality that can be shared between 2 methods.
        /// </summary>
        internal static IEnumerable<EntityType> GetRelatedEntityTypes(EntityType entityType)
        {
            var relatedEntityTypes = new HashSet<EntityType>();

            // Add related entity-types (through) association to the list.
            ICollection<Association> participatingAssociations = Association.GetAssociationsForEntityType(entityType);
            Debug.Assert(participatingAssociations != null, "Association's GetAssociationsForEntityType returns null.");
            if (participatingAssociations != null)
            {
                foreach (var association in participatingAssociations)
                {
                    foreach (var et in association.AssociationEnds().Select(ae => ae.Type.SafeTarget).ToList())
                    {
                        // check if the entity-type is not already in the list and is not equal to the passed in entity-type (Self association scenario).
                        if (relatedEntityTypes.Contains(et) == false
                            && et != entityType)
                        {
                            relatedEntityTypes.Add(et);
                        }
                    }
                }
            }

            // Check if the entity-type is a conceptual entity-type, if yes we need to examine the inheritance relationship too.
            var cet = entityType as ConceptualEntityType;
            if (cet != null)
            {
                // check the immediate base type.
                if (cet.SafeBaseType != null
                    && relatedEntityTypes.Contains(cet.SafeBaseType) == false)
                {
                    relatedEntityTypes.Add(cet.SafeBaseType);
                }

                // check the derived types
                foreach (var derivedEntityType in cet.ResolvableDirectDerivedTypes)
                {
                    if (relatedEntityTypes.Contains(derivedEntityType) == false)
                    {
                        relatedEntityTypes.Add(derivedEntityType);
                    }
                }
            }
            return relatedEntityTypes;
        }

        /// <summary>
        ///     Returns the list of association connectors between the passed in entity-type-shape and other entity-type-shapes or itself.
        /// </summary>
        /// <param name="entityTypeShape"></param>
        /// <returns></returns>
        internal static IEnumerable<AssociationConnector> GetListOfAssociationConnectorsForEntityTypeShape(EntityTypeShape entityTypeShape)
        {
            var associationConnectors = new HashSet<AssociationConnector>();

            if (entityTypeShape == null)
            {
                Debug.Fail("argument entityTypeShape must not be null");
                return associationConnectors;
            }

            if (entityTypeShape.EntityType.SafeTarget == null)
            {
                Debug.Fail("argument entityTypeShape must reference an entity type.");
                return associationConnectors;
            }

            // Find all associations which the entity type participates.
            IEnumerable<Association> participatingAssociations =
                Association.GetAssociationsForEntityType(entityTypeShape.EntityType.SafeTarget);

            // Find all associationConnectors in the diagram.
            foreach (var association in participatingAssociations)
            {
                foreach (
                    var connector in
                        association.GetAntiDependenciesOfType<AssociationConnector>()
                            .Where(ac => ac.Diagram.Id == entityTypeShape.Diagram.Id))
                {
                    if (associationConnectors.Contains(connector) == false)
                    {
                        associationConnectors.Add(connector);
                    }
                }
            }
            return associationConnectors;
        }

        /// <summary>
        ///     Return the list of inheritance connectors between the passed in entity-type=shape and its derived or base entity-type-shapes.
        /// </summary>
        /// <param name="entityTypeShape"></param>
        /// <returns></returns>
        internal static IEnumerable<InheritanceConnector> GetListOfInheritanceConnectorsForEntityTypeShape(EntityTypeShape entityTypeShape)
        {
            var inheritanceConnectors = new HashSet<InheritanceConnector>();

            if (entityTypeShape == null)
            {
                Debug.Fail("argument entityTypeShape must not be null");
                return inheritanceConnectors;
            }

            if (entityTypeShape.EntityType.SafeTarget == null)
            {
                Debug.Fail("argument entityTypeShape must reference an entity type.");
                return inheritanceConnectors;
            }

            var conceptualEntityType = entityTypeShape.EntityType.SafeTarget as ConceptualEntityType;
            Debug.Assert(conceptualEntityType != null, "entityTypeShape.EntityType.SafeTarget should be a ConceptualEntityType");

            if (conceptualEntityType != null)
            {
                // Add inheritance connector to the base type in the diagram.
                var ic =
                    conceptualEntityType.GetAntiDependenciesOfType<InheritanceConnector>()
                        .Where(c => c.Diagram.Id == entityTypeShape.Diagram.Id)
                        .FirstOrDefault();
                if (ic != null)
                {
                    inheritanceConnectors.Add(ic);
                }

                // Add inheritance connector to the derived type in the diagram.
                foreach (var derivedEntityType in conceptualEntityType.ResolvableDirectDerivedTypes)
                {
                    ic =
                        derivedEntityType.GetAntiDependenciesOfType<InheritanceConnector>()
                            .Where(c => c.Diagram.Id == entityTypeShape.Diagram.Id)
                            .FirstOrDefault();
                    if (ic != null
                        && inheritanceConnectors.Contains(ic) == false)
                    {
                        inheritanceConnectors.Add(ic);
                    }
                }
            }
            return inheritanceConnectors;
        }

        // Returns a Dictionary mapping StorageEntitySet to a bool.
        // The keys of the Dictionary are the set of StorageEntitySets from Mapping Fragments
        // within EntityTypeMappings mapped to directly (i.e. with EntityTypeMappingKind either
        // IsTypeOf or Default but mentioning the ConceptualEntityType by name, not just 
        // applying to this ConceptualEntityType because the ConceptualEntityType's ancestor has
        // an IsTypeOf mapping)
        // Note: this may contain tables also mapped by ancestors in the case where both have mappings
        // to the same table
        // The bool is true if the number of conceptual _non-key_ properties that the given Mapping Fragment
        // maps which belong to an _ancestor_ conceptual Entity Type > 0 (useful to spot TPC mapping)
        internal static Dictionary<StorageEntitySet, bool> FindTablesMappedDirectlyToConceptualEntityType(ConceptualEntityType cet)
        {
            var result = new Dictionary<StorageEntitySet, bool>();
            foreach (var etm in cet.GetAntiDependenciesOfType<EntityTypeMapping>())
            {
                if (EntityTypeMappingKind.IsTypeOf == etm.Kind
                    || EntityTypeMappingKind.Default == etm.Kind)
                {
                    foreach (var mf in etm.MappingFragments())
                    {
                        var ses = (mf.StoreEntitySet == null ? null : mf.StoreEntitySet.Target as StorageEntitySet);
                        if (ses != null)
                        {
                            if (false == result.ContainsKey(ses))
                            {
                                result.Add(ses, false);
                            }

                            foreach (var sp in mf.ScalarProperties())
                            {
                                var conceptualProperty = (sp.Name == null ? null : sp.Name.Target);
                                if (conceptualProperty != null
                                    && !conceptualProperty.IsKeyProperty
                                    && conceptualProperty.Parent != cet)
                                {
                                    // have found a mapping between a column in ses and a conceptual non-key
                                    // property which belongs to an EntityType other than the one passed in
                                    // (i.e. an ancestor)
                                    result[ses] = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        // returns the set of all StorageEntitySets mapped to by all of cet's ancestors
        // (this uses the Direct method above but will pick up all indirect as well because
        // it is scanning all the way to the top of the inheritance hierarchy)
        internal static HashSet<StorageEntitySet> FindTablesMappedByAncestors(ConceptualEntityType cet)
        {
            var result = new HashSet<StorageEntitySet>();
            var baseType = (cet.BaseType == null ? null : cet.BaseType.Target);
            while (baseType != null)
            {
                result.UnionWith(FindTablesMappedDirectlyToConceptualEntityType(baseType).Keys);
                baseType = (baseType.BaseType == null ? null : baseType.BaseType.Target);
            }

            return result;
        }

        // Determine how the ConceptualEntityType is currently mapped (relative to its ancestors).
        // Note: this method has to be valid _before_ EnforceEntitySetMappingRules is invoked because
        //       EnforceEntitySetMappingRules calls it to determine what strategy is being attempted.
        internal static InheritanceMappingStrategy DetermineCurrentInheritanceStrategy(ConceptualEntityType cet)
        {
            if (cet.BaseType == null
                || cet.BaseType.Target == null)
            {
                // this EntityType does not inherit from anything
                return InheritanceMappingStrategy.NoInheritance;
            }

            // first check whether our ancestors are mapped
            var tablesMappedToAncestors = FindTablesMappedByAncestors(cet);
            if (tablesMappedToAncestors.Count == 0)
            {
                // no ancestors are currently mapped - use TablePerHierarchy as default
                return InheritanceMappingStrategy.TablePerHierarchy;
            }

            // find the list of tables mapped _only_ to this EntityType and not mapped by any ancestor
            var tablesMappedToThisEntityTypeWithAncestorInfo =
                FindTablesMappedDirectlyToConceptualEntityType(cet);
            if (tablesMappedToThisEntityTypeWithAncestorInfo.Count == 0)
            {
                // at least one ancestor is mapped but this ConceptualEntityType is not currently
                // mapped to any tables (other than potentially indirectly through IsTypeOf() 
                // mappings on its base type(s)). Use TablePerType as default.
                return InheritanceMappingStrategy.TablePerType;
            }

            // at this point both the ancestors and the Conceptual Entity Type are mapped 
            // to at least one table each

            // if the number of EntityTypeMappings directly mapped to this Conceptual Entity Type
            // is greater than 1 then we are in "Mixed" inheritance mode
            // Note: only include IsTypeOf and Default mappings, ignore EntityTypeMappings for Function mappings
            // which are allowed if the user sets up both Table mappings and Function mappings
            var numMappingsDirectlyMapped = cet.GetAntiDependenciesOfType<EntityTypeMapping>().
                Where<EntityTypeMapping>(etm => (EntityTypeMappingKind.IsTypeOf == etm.Kind || EntityTypeMappingKind.Default == etm.Kind))
                .Count();
            if (numMappingsDirectlyMapped > 1)
            {
                Debug.Fail(
                    "Mixed mode - this should only happen if the user has hand-edited the file in which case the Mapping Details " +
                    "window should be in safe mode and so this code should not be executed");
                return InheritanceMappingStrategy.Mixed;
            }

            // Set up tablesMappedToThisEntityType to only have tables which are _not_ used by ancestors
            var tablesMappedToThisEntityType =
                new HashSet<StorageEntitySet>(tablesMappedToThisEntityTypeWithAncestorInfo.Keys);
            tablesMappedToThisEntityType.ExceptWith(tablesMappedToAncestors);
            if (tablesMappedToThisEntityType.Count == 0)
            {
                // this ConceptualEntityType is mapped to table(s) and so is at least one ancestor, 
                // and this ConceptualEntityType uses _only_ the same table(s) mapped by its ancestor(s)
                return InheritanceMappingStrategy.TablePerHierarchy;
            }

            // at this point mapping strategy must be either TPT or TPC

            // TODO: below should be commented back in if we decide to support TPC
            //// if any non-key ancestor conceptual properties are mapped to a table
            //// not mapped by an ancestor then it's TPC, otherwise TPT
            //bool hasAncestorConceptualPropertiesMappedToTableNotUsedByAncestors = false;
            //foreach (StorageEntitySet ses in tablesMappedToThisEntityType)
            //{
            //    if (true == tablesMappedToThisEntityTypeWithAncestorInfo[ses])
            //    {
            //        hasAncestorConceptualPropertiesMappedToTableNotUsedByAncestors = true;
            //        break;
            //    }
            //}

            //if (hasAncestorConceptualPropertiesMappedToTableNotUsedByAncestors)
            //{
            //    return InheritanceMappingStrategy.TablePerConcreteClass;
            //}

            return InheritanceMappingStrategy.TablePerType;
        }

        /// <summary>
        ///     Sort the properties in the order of the properties' XElement in the XDocument.
        /// </summary>
        /// <returns></returns>
        internal static IList<T> GetListOfPropertiesInTheirXElementsOrder<T>(IList<T> properties) where T : PropertyBase
        {
            IList<T> sortedProperties = new List<T>();

            if (properties != null
                && properties.Count > 0)
            {
                var property = properties[0];

                // Move to the first property.
                while (property.PreviousSiblingInPropertyXElementOrder != null)
                {
                    property = property.PreviousSiblingInPropertyXElementOrder as T;
                }

                // Traverse the properties in the entity and if the property is what we want to move, add it to the sorted list.
                while (property != null)
                {
                    if (properties.Contains(property))
                    {
                        sortedProperties.Add(property);
                    }
                    property = property.NextSiblingInPropertyXElementOrder as T;
                }
            }
            return sortedProperties;
        }

        internal static EntityType GetStoreEntityType(MappingFragment mappingFragment)
        {
            return mappingFragment.StoreEntitySet.SafeTarget.EntityType.SafeTarget;
        }

        // TODO: inefficient
        internal static IEnumerable<EntityTypeMapping> GetEntityTypeMappings(EntityType conceptualEntityType)
        {
            return from entityTypeMap in GetEntityTypeMappings(conceptualEntityType.Artifact)
                   where GetMappedTypesWithDuplicates(entityTypeMap).Contains(conceptualEntityType)
                   select entityTypeMap;
        }

        internal static IEnumerable<MappingFragment> GetMappingFragments(EntityType conceptualEntityType)
        {
            return from entityTypeMap in GetEntityTypeMappings(conceptualEntityType)
                   from fragment in entityTypeMap.MappingFragments()
                   select fragment;
        }

        internal static IEnumerable<EntityTypeMapping> GetEntityTypeMappings(EFArtifact artifact)
        {
            return from containerMap in artifact.MappingModel().EntityContainerMappings()
                   from entitySetMap in containerMap.EntitySetMappings()
                   from entityTypeMap in entitySetMap.EntityTypeMappings()
                   select entityTypeMap;
        }

        internal static IEnumerable<EntityType> GetMappedTypesWithDuplicates(EntityTypeMapping entityTypeMapping)
        {
            var assignments = entityTypeMapping.TypeName;
            for (var i = 0; i < assignments.Bindings.Count; i++)
            {
                var bindingTarget = assignments.Bindings[i].Target;
                var mappedType = bindingTarget as ConceptualEntityType;
                Debug.Assert(bindingTarget != null ? mappedType != null : true, "EntityType bindingTarget is not a ConceptualEntityType");

                if (mappedType == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, Resources.UnresolvedTypeReferences_0, assignments.RefName));
                }
                yield return mappedType;

                if (assignments.IsTypeOf(assignments.Bindings[i]))
                {
                    foreach (EntityType derivedType in mappedType.DerivedTypes)
                    {
                        yield return derivedType;
                    }
                }
            }
        }

        internal static HashSet<EntityType> GetTablesMappedFrom(EntityType entityType)
        {
            var tableTypes = new HashSet<EntityType>();
            foreach (var mappingFragment in GetMappingFragments(entityType))
            {
                tableTypes.Add(GetStoreEntityType(mappingFragment));
            }
            return tableTypes;
        }

        internal static AssociationEnd InferDependentEnd(Association association)
        {
            Debug.Assert(
                association.IsManyToMany == false, "Cannot infer a dependent end on a *:* association; this method should not be called");
            if (association.IsManyToMany)
            {
                throw new InvalidOperationException("Cannot infer a dependent end for a *:* association");
            }

            if (association.ReferentialConstraint != null)
            {
                Debug.Assert(association.ReferentialConstraint.Dependent != null, "Dependent end of ref constraint is null");
                if (association.ReferentialConstraint.Dependent != null)
                {
                    return association.ReferentialConstraint.Dependent.Role.Target;
                }
            }
            else
            {
                // Dependency is implied by OnDelete in 1:1 associations
                if (association.End1.Multiplicity.Value == ModelConstants.Multiplicity_One
                    && association.End2.Multiplicity.Value == ModelConstants.Multiplicity_One)
                {
                    if (association.End1.OnDeleteAction != null)
                    {
                        return association.End1.OnDeleteAction.Action.Value == ModelConstants.OnDeleteAction_Cascade
                                   ? association.End2
                                   : association.End1;
                    }
                }

                // Dependency can also be implied by the multiplicity of the association
                if ((association.End1.Multiplicity.Value == ModelConstants.Multiplicity_Many)
                    || (association.End1.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne
                        && association.End2.Multiplicity.Value == ModelConstants.Multiplicity_One))
                {
                    return association.End1;
                }
                return association.End2;
            }

            Debug.Fail("Could not determine the dependent end of this association");
            return null;
        }

        /// <summary>
        ///     Retrieve the root node ("Schema", "ConceptualMerge", "StorageMerge", or "MappingMerge").  If there is no root node, this will return null.
        /// </summary>
        /// <returns></returns>
        internal static BaseEntityModel GetBaseModelRoot(EFObject node)
        {
            var currNode = node;
            BaseEntityModel model = null;
            while (currNode != null)
            {
                model = currNode as BaseEntityModel;
                if (model != null)
                {
                    break;
                }

                currNode = currNode.Parent;
            }

            return model;
        }

        internal static bool IsValidValueForType(Type type, string value)
        {
            var isValueValid = false;

            if (type != null)
            {
                var tryParseMethod = type.GetMethod(
                    "TryParse"
                    , BindingFlags.Public | BindingFlags.Static
                    , Type.DefaultBinder
                    , new[] { typeof(string), type.MakeByRefType() }
                    , null);

                Debug.Assert(tryParseMethod != null, "Unable to find method TryParse from type: " + type.Name);

                if (tryParseMethod != null)
                {
                    isValueValid = (bool)tryParseMethod.Invoke(null, new object[] { value, null });
                }
            }
            return isValueValid;
        }

        internal static EntitySet FindEntitySet(BaseEntityModel entityModel, string name)
        {
            Debug.Assert(entityModel != null, "Null BaseEntityModel");

            foreach (var et in entityModel.FirstEntityContainer.EntitySets())
            {
                if (String.Compare(name, et.LocalName.Value, StringComparison.OrdinalIgnoreCase) == 0
                    || String.Compare(name, et.NormalizedNameExternal, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return et;
                }
            }

            return null;
        }
    }
}
