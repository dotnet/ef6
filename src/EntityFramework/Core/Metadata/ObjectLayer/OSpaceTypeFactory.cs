// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    // <summary>
    // This is an extraction of the code that was in <see cref="ObjectItemConventionAssemblyLoader" /> such that
    // it can be used outside of the context of the traditional assembly loaders--notably the CLR types to load
    // from are provided by Code First.
    // </summary>
    internal abstract class OSpaceTypeFactory
    {
        public abstract List<Action> ReferenceResolutions { get; }

        public abstract void LogLoadMessage(string message, EdmType relatedType);

        public abstract void LogError(string errorMessage, EdmType relatedType);

        public abstract void TrackClosure(Type type);

        public abstract Dictionary<EdmType, EdmType> CspaceToOspace { get; }

        public abstract Dictionary<string, EdmType> LoadedTypes { get; }

        public abstract void AddToTypesInAssembly(EdmType type);

        public virtual EdmType TryCreateType(Type type, EdmType cspaceType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(cspaceType);
            Debug.Assert(cspaceType is StructuralType || Helper.IsEnumType(cspaceType), "Structural or enum type expected");

            // if one of the types is an enum while the other is not there is no match
            if (Helper.IsEnumType(cspaceType)
                ^ type.IsEnum)
            {
                LogLoadMessage(
                    Strings.Validator_OSpace_Convention_SSpaceOSpaceTypeMismatch(cspaceType.FullName, cspaceType.FullName),
                    cspaceType);
                return null;
            }

            EdmType newOSpaceType;
            if (Helper.IsEnumType(cspaceType))
            {
                TryCreateEnumType(type, (EnumType)cspaceType, out newOSpaceType);
                return newOSpaceType;
            }

            Debug.Assert(cspaceType is StructuralType);
            TryCreateStructuralType(type, (StructuralType)cspaceType, out newOSpaceType);
            return newOSpaceType;
        }

        private bool TryCreateEnumType(Type enumType, EnumType cspaceEnumType, out EdmType newOSpaceType)
        {
            DebugCheck.NotNull(enumType);
            Debug.Assert(enumType.IsEnum, "enum type expected");
            DebugCheck.NotNull(cspaceEnumType);
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");

            newOSpaceType = null;

            // Check if the OSpace and CSpace enum type match
            if (!UnderlyingEnumTypesMatch(enumType, cspaceEnumType)
                || !EnumMembersMatch(enumType, cspaceEnumType))
            {
                return false;
            }

            newOSpaceType = new ClrEnumType(enumType, cspaceEnumType.NamespaceName, cspaceEnumType.Name);

            LoadedTypes.Add(enumType.FullName, newOSpaceType);

            return true;
        }

        private bool TryCreateStructuralType(Type type, StructuralType cspaceType, out EdmType newOSpaceType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(cspaceType);

            var referenceResolutionListForCurrentType = new List<Action>();
            newOSpaceType = null;

            StructuralType ospaceType;
            if (Helper.IsEntityType(cspaceType))
            {
                ospaceType = new ClrEntityType(type, cspaceType.NamespaceName, cspaceType.Name);
            }
            else
            {
                Debug.Assert(Helper.IsComplexType(cspaceType), "Invalid type attribute encountered");
                ospaceType = new ClrComplexType(type, cspaceType.NamespaceName, cspaceType.Name);
            }

            if (cspaceType.BaseType != null)
            {
                if (TypesMatchByConvention(type.BaseType, cspaceType.BaseType))
                {
                    TrackClosure(type.BaseType);
                    referenceResolutionListForCurrentType.Add(
                        () => ospaceType.BaseType = ResolveBaseType((StructuralType)cspaceType.BaseType, type));
                }
                else
                {
                    var message = Strings.Validator_OSpace_Convention_BaseTypeIncompatible(
                        type.BaseType.FullName, type.FullName, cspaceType.BaseType.FullName);
                    LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            // Load the properties for this type
            if (!TryCreateMembers(type, cspaceType, ospaceType, referenceResolutionListForCurrentType))
            {
                return false;
            }

            // Add this to the known type map so we won't try to load it again
            LoadedTypes.Add(type.FullName, ospaceType);

            // we only add the referenceResolution to the list unless we structrually matched this type
            foreach (var referenceResolution in referenceResolutionListForCurrentType)
            {
                ReferenceResolutions.Add(referenceResolution);
            }

            newOSpaceType = ospaceType;
            return true;
        }

        internal static bool TypesMatchByConvention(Type type, EdmType cspaceType)
        {
            return type.Name == cspaceType.Name;
        }

        private bool UnderlyingEnumTypesMatch(Type enumType, EnumType cspaceEnumType)
        {
            DebugCheck.NotNull(enumType);
            Debug.Assert(enumType.IsEnum, "expected enum OSpace type");
            DebugCheck.NotNull(cspaceEnumType);
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");

            // Note that TryGetPrimitiveType() will return false not only for types that are not primitive 
            // but also for CLR primitive types that are valid underlying enum types in CLR but are not 
            // a valid Edm primitive types (e.g. ulong) 
            PrimitiveType underlyingEnumType;
            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(enumType.GetEnumUnderlyingType(), out underlyingEnumType))
            {
                LogLoadMessage(
                    Strings.Validator_UnsupportedEnumUnderlyingType(enumType.GetEnumUnderlyingType().FullName),
                    cspaceEnumType);

                return false;
            }
            else if (underlyingEnumType.PrimitiveTypeKind
                     != cspaceEnumType.UnderlyingType.PrimitiveTypeKind)
            {
                LogLoadMessage(
                    Strings.Validator_OSpace_Convention_NonMatchingUnderlyingTypes, cspaceEnumType);

                return false;
            }

            return true;
        }

        private bool EnumMembersMatch(Type enumType, EnumType cspaceEnumType)
        {
            DebugCheck.NotNull(enumType);
            Debug.Assert(enumType.IsEnum, "expected enum OSpace type");
            DebugCheck.NotNull(cspaceEnumType);
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");
            Debug.Assert(
                cspaceEnumType.UnderlyingType.ClrEquivalentType == enumType.GetEnumUnderlyingType(),
                "underlying types should have already been checked");

            var enumUnderlyingType = enumType.GetEnumUnderlyingType();

            var cspaceSortedEnumMemberEnumerator = cspaceEnumType.Members.OrderBy(m => m.Name).GetEnumerator();
            var ospaceSortedEnumMemberNamesEnumerator = enumType.GetEnumNames().OrderBy(n => n).GetEnumerator();

            // no checks required if edm enum type does not have any members 
            if (!cspaceSortedEnumMemberEnumerator.MoveNext())
            {
                return true;
            }

            while (ospaceSortedEnumMemberNamesEnumerator.MoveNext())
            {
                if (cspaceSortedEnumMemberEnumerator.Current.Name == ospaceSortedEnumMemberNamesEnumerator.Current
                    &&
                    cspaceSortedEnumMemberEnumerator.Current.Value.Equals(
                        Convert.ChangeType(
                            Enum.Parse(enumType, ospaceSortedEnumMemberNamesEnumerator.Current), enumUnderlyingType,
                            CultureInfo.InvariantCulture)))
                {
                    if (!cspaceSortedEnumMemberEnumerator.MoveNext())
                    {
                        return true;
                    }
                }
            }

            LogLoadMessage(
                Strings.Mapping_Enum_OCMapping_MemberMismatch(
                    enumType.FullName,
                    cspaceSortedEnumMemberEnumerator.Current.Name,
                    cspaceSortedEnumMemberEnumerator.Current.Value,
                    cspaceEnumType.FullName), cspaceEnumType);

            return false;
        }

        private bool TryCreateMembers(
            Type type, StructuralType cspaceType, StructuralType ospaceType, List<Action> referenceResolutionListForCurrentType)
        {
            var clrProperties = (cspaceType.BaseType == null
                                     ? type.GetRuntimeProperties()
                                     : type.GetDeclaredProperties()).Where(p => !p.IsStatic());

            // required properties scalar properties first
            if (!TryFindAndCreatePrimitiveProperties(type, cspaceType, ospaceType, clrProperties))
            {
                return false;
            }

            if (!TryFindAndCreateEnumProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            if (!TryFindComplexProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            if (!TryFindNavigationProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            return true;
        }

        private bool TryFindComplexProperties(
            Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties,
            List<Action> referenceResolutionListForCurrentType)
        {
            var typeClosureToTrack =
                new List<KeyValuePair<EdmProperty, PropertyInfo>>();
            foreach (
                var cspaceProperty in cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(m => Helper.IsComplexType(m.TypeUsage.EdmType))
                )
            {
                var clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    typeClosureToTrack.Add(
                        new KeyValuePair<EdmProperty, PropertyInfo>(
                            cspaceProperty, clrProperty));
                }
                else
                {
                    var message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);
                // prevent the lifting of these closure variables
                var ot = ospaceType;
                var cp = typeToTrack.Key;
                var clrp = typeToTrack.Value;
                referenceResolutionListForCurrentType.Add(() => CreateAndAddComplexType(type, ot, cp, clrp));
            }

            return true;
        }

        private bool TryFindNavigationProperties(
            Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties,
            List<Action> referenceResolutionListForCurrentType)
        {
            var typeClosureToTrack =
                new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
            foreach (var cspaceProperty in cspaceType.GetDeclaredOnlyMembers<NavigationProperty>())
            {
                var clrProperty = clrProperties.FirstOrDefault(p => NonPrimitiveMemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    var needsSetter = cspaceProperty.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many;
                    if (clrProperty.CanRead
                        && (!needsSetter || clrProperty.CanWriteExtended()))
                    {
                        typeClosureToTrack.Add(
                            new KeyValuePair<NavigationProperty, PropertyInfo>(
                                cspaceProperty, clrProperty));
                    }
                }
                else
                {
                    var message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(
                        cspaceProperty.Name, type.FullName);
                    LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);

                // keep from lifting these closure variables
                var ct = cspaceType;
                var ot = ospaceType;
                var cp = typeToTrack.Key;

                referenceResolutionListForCurrentType.Add(() => CreateAndAddNavigationProperty(ct, ot, cp));
            }

            return true;
        }

        private EdmType ResolveBaseType(StructuralType baseCSpaceType, Type type)
        {
            EdmType ospaceType;
            var foundValue = CspaceToOspace.TryGetValue(baseCSpaceType, out ospaceType);
            if (!foundValue)
            {
                LogError(Strings.Validator_OSpace_Convention_BaseTypeNotLoaded(type, baseCSpaceType), baseCSpaceType);
            }

            Debug.Assert(!foundValue || ospaceType is StructuralType, "Structural type expected (if found).");

            return ospaceType;
        }

        private bool TryFindAndCreatePrimitiveProperties(
            Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties)
        {
            foreach (
                var cspaceProperty in
                    cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(p => Helper.IsPrimitiveType(p.TypeUsage.EdmType)))
            {
                var clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    PrimitiveType propertyType;
                    if (TryGetPrimitiveType(clrProperty.PropertyType, out propertyType))
                    {
                        if (clrProperty.CanRead
                            && clrProperty.CanWriteExtended())
                        {
                            AddScalarMember(type, clrProperty, ospaceType, cspaceProperty, propertyType);
                        }
                        else
                        {
                            var message = Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(
                                clrProperty.Name, type.FullName, type.Assembly.FullName);
                            LogLoadMessage(message, cspaceType);
                            return false;
                        }
                    }
                    else
                    {
                        var message = Strings.Validator_OSpace_Convention_NonPrimitiveTypeProperty(
                            clrProperty.Name, type.FullName, clrProperty.PropertyType.FullName);
                        LogLoadMessage(message, cspaceType);
                        return false;
                    }
                }
                else
                {
                    var message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    LogLoadMessage(message, cspaceType);
                    return false;
                }
            }
            return true;
        }

        protected static bool TryGetPrimitiveType(Type type, out PrimitiveType primitiveType)
        {
            return ClrProviderManifest.Instance.TryGetPrimitiveType(Nullable.GetUnderlyingType(type) ?? type, out primitiveType);
        }

        private bool TryFindAndCreateEnumProperties(
            Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties,
            List<Action> referenceResolutionListForCurrentType)
        {
            var typeClosureToTrack = new List<KeyValuePair<EdmProperty, PropertyInfo>>();

            foreach (
                var cspaceProperty in cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(p => Helper.IsEnumType(p.TypeUsage.EdmType)))
            {
                var clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    typeClosureToTrack.Add(new KeyValuePair<EdmProperty, PropertyInfo>(cspaceProperty, clrProperty));
                }
                else
                {
                    var message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);
                // prevent the lifting of these closure variables
                var ot = ospaceType;
                var cp = typeToTrack.Key;
                var clrp = typeToTrack.Value;
                referenceResolutionListForCurrentType.Add(() => CreateAndAddEnumProperty(type, ot, cp, clrp));
            }

            return true;
        }

        private static bool MemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
        {
            return clrProperty.Name == cspaceMember.Name;
        }

        private void CreateAndAddComplexType(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
        {
            EdmType propertyType;
            if (CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out propertyType))
            {
                Debug.Assert(propertyType is StructuralType, "Structural type expected.");

                var property = new EdmProperty(
                    cspaceProperty.Name, TypeUsage.Create(
                        propertyType, new FacetValues
                            {
                                Nullable = false
                            }), clrProperty, type);
                ospaceType.AddMember(property);
            }
            else
            {
                LogError(
                    Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName),
                    cspaceProperty.TypeUsage.EdmType);
            }
        }

        private static bool NonPrimitiveMemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
        {
            return !clrProperty.PropertyType.IsValueType && !clrProperty.PropertyType.IsAssignableFrom(typeof(string))
                   && clrProperty.Name == cspaceMember.Name;
        }

        private void CreateAndAddNavigationProperty(
            StructuralType cspaceType, StructuralType ospaceType, NavigationProperty cspaceProperty)
        {
            EdmType ospaceRelationship;
            if (CspaceToOspace.TryGetValue(cspaceProperty.RelationshipType, out ospaceRelationship))
            {
                Debug.Assert(ospaceRelationship is StructuralType, "Structural type expected.");

                var foundTarget = false;
                EdmType targetType = null;
                if (Helper.IsCollectionType(cspaceProperty.TypeUsage.EdmType))
                {
                    EdmType findType;
                    foundTarget =
                        CspaceToOspace.TryGetValue(
                            ((CollectionType)cspaceProperty.TypeUsage.EdmType).TypeUsage.EdmType, out findType);
                    if (foundTarget)
                    {
                        Debug.Assert(findType is StructuralType, "Structural type expected.");

                        targetType = findType.GetCollectionType();
                    }
                }
                else
                {
                    EdmType findType;
                    foundTarget = CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out findType);
                    if (foundTarget)
                    {
                        Debug.Assert(findType is StructuralType, "Structural type expected.");

                        targetType = findType;
                    }
                }

                Debug.Assert(
                    foundTarget,
                    "Since the relationship will only be created if it can find the types for both ends, we will never fail to find one of the ends");

                var navigationProperty = new NavigationProperty(cspaceProperty.Name, TypeUsage.Create(targetType));
                var relationshipType = (RelationshipType)ospaceRelationship;
                navigationProperty.RelationshipType = relationshipType;

                // we can use First because o-space relationships are created directly from 
                // c-space relationship
                navigationProperty.ToEndMember =
                    (RelationshipEndMember)relationshipType.Members.First(e => e.Name == cspaceProperty.ToEndMember.Name);
                navigationProperty.FromEndMember =
                    (RelationshipEndMember)relationshipType.Members.First(e => e.Name == cspaceProperty.FromEndMember.Name);
                ospaceType.AddMember(navigationProperty);
            }
            else
            {
                var missingType =
                    cspaceProperty.RelationshipType.RelationshipEndMembers.Select(e => ((RefType)e.TypeUsage.EdmType).ElementType).First(
                        e => e != cspaceType);
                LogError(
                    Strings.Validator_OSpace_Convention_RelationshipNotLoaded(
                        cspaceProperty.RelationshipType.FullName, missingType.FullName),
                    missingType);
            }
        }

        // <summary>
        // Creates an Enum property based on <paramref name="clrProperty" /> and adds it to the parent structural type.
        // </summary>
        // <param name="type">
        // CLR type owning <paramref name="clrProperty" /> .
        // </param>
        // <param name="ospaceType"> OSpace type the created property will be added to. </param>
        // <param name="cspaceProperty"> Corresponding property from CSpace. </param>
        // <param name="clrProperty"> CLR property used to build an Enum property. </param>
        private void CreateAndAddEnumProperty(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
        {
            EdmType propertyType;
            if (CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out propertyType))
            {
                if (clrProperty.CanRead
                    && clrProperty.CanWriteExtended())
                {
                    AddScalarMember(type, clrProperty, ospaceType, cspaceProperty, propertyType);
                }
                else
                {
                    LogError(
                        Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(
                            clrProperty.Name, type.FullName, type.Assembly.FullName),
                        cspaceProperty.TypeUsage.EdmType);
                }
            }
            else
            {
                LogError(
                    Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName),
                    cspaceProperty.TypeUsage.EdmType);
            }
        }

        private static void AddScalarMember(
            Type type, PropertyInfo clrProperty, StructuralType ospaceType, EdmProperty cspaceProperty, EdmType propertyType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(clrProperty);
            Debug.Assert(clrProperty.CanRead && clrProperty.CanWriteExtended(), "The clr property has to have a setter and a getter.");
            DebugCheck.NotNull(ospaceType);
            DebugCheck.NotNull(cspaceProperty);
            DebugCheck.NotNull(propertyType);
            Debug.Assert(Helper.IsScalarType(propertyType), "Property has to be primitive or enum.");

            var cspaceType = cspaceProperty.DeclaringType;

            var isKeyMember = Helper.IsEntityType(cspaceType) && ((EntityType)cspaceType).KeyMemberNames.Contains(clrProperty.Name);

            // the property is nullable only if it is not a key and can actually be set to null (i.e. is not a value type or is a nullable value type)
            var nullableFacetValue = !isKeyMember
                                     &&
                                     (!clrProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(clrProperty.PropertyType) != null);

            var ospaceProperty =
                new EdmProperty(
                    cspaceProperty.Name,
                    TypeUsage.Create(
                        propertyType, new FacetValues
                            {
                                Nullable = nullableFacetValue
                            }),
                    clrProperty,
                    type);

            if (isKeyMember)
            {
                ((EntityType)ospaceType).AddKeyMember(ospaceProperty);
            }
            else
            {
                ospaceType.AddMember(ospaceProperty);
            }
        }

        public virtual void CreateRelationships(EdmItemCollection edmItemCollection)
        {
            foreach (var cspaceAssociation in edmItemCollection.GetItems<AssociationType>())
            {
                Debug.Assert(cspaceAssociation.RelationshipEndMembers.Count == 2, "Relationships are assumed to have exactly two ends");

                if (CspaceToOspace.ContainsKey(cspaceAssociation))
                {
                    // don't try to load relationships that we already know about
                    continue;
                }

                var ospaceEndTypes = new EdmType[2];
                if (CspaceToOspace.TryGetValue(
                    GetRelationshipEndType(cspaceAssociation.RelationshipEndMembers[0]), out ospaceEndTypes[0])
                    && CspaceToOspace.TryGetValue(
                        GetRelationshipEndType(cspaceAssociation.RelationshipEndMembers[1]), out ospaceEndTypes[1]))
                {
                    Debug.Assert(ospaceEndTypes[0] is StructuralType);
                    Debug.Assert(ospaceEndTypes[1] is StructuralType);

                    // if we can find both ends of the relationship, then create it

                    var ospaceAssociation = new AssociationType(
                        cspaceAssociation.Name, cspaceAssociation.NamespaceName, cspaceAssociation.IsForeignKey, DataSpace.OSpace);
                    for (var i = 0; i < cspaceAssociation.RelationshipEndMembers.Count; i++)
                    {
                        var ospaceEndType = (EntityType)ospaceEndTypes[i];
                        var cspaceEnd = cspaceAssociation.RelationshipEndMembers[i];

                        ospaceAssociation.AddKeyMember(
                            new AssociationEndMember(cspaceEnd.Name, ospaceEndType.GetReferenceType(), cspaceEnd.RelationshipMultiplicity));
                    }

                    AddToTypesInAssembly(ospaceAssociation);
                    LoadedTypes.Add(ospaceAssociation.FullName, ospaceAssociation);
                    CspaceToOspace.Add(cspaceAssociation, ospaceAssociation);
                }
            }
        }

        private static StructuralType GetRelationshipEndType(RelationshipEndMember relationshipEndMember)
        {
            return ((RefType)relationshipEndMember.TypeUsage.EdmType).ElementType;
        }
    }
}
