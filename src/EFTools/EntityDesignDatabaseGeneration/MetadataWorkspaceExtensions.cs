// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Extension methods for the MetadataWorkspace API
    /// </summary>
    public static class MetadataWorkspaceExtensions
    {
        /// <summary>
        ///     Retrieves the namespace of this ItemCollection by examining the first-found StructuralType
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The namespace as a string, or null if there are no StructuralTypes to retrieve the namespace</returns>
        public static string GetNamespace(this ItemCollection itemCollection)
        {
            var edmType = itemCollection.GetItems<StructuralType>().FirstOrDefault();
            if (edmType == null)
            {
                return String.Empty;
            }
            return edmType.NamespaceName;
        }

        /// <summary>
        ///     Retrieves the 'Name' attribute of the &lt;EntityContainer/&gt; element in this ItemCollection.
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The name of the entity container.</returns>
        /// <exception cref="InvalidDataException">if an EntityContainer element cannot be found</exception>
        public static string GetEntityContainerName(this ItemCollection itemCollection)
        {
            var entityContainer = itemCollection.GetItems<EntityContainer>().FirstOrDefault();
            if (entityContainer == null)
            {
                throw new InvalidDataException(Resources.ErrorNoEntityContainer);
            }
            return entityContainer.Name;
        }

        /// <summary>
        ///     Retrieves an enumerable collection of all EntitySet elements in this ItemCollection
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The list of EntitySet elements.</returns>
        public static IEnumerable<EntitySet> GetAllEntitySets(this ItemCollection itemCollection)
        {
            var allEntitySets = new List<EntitySet>();

            var edmContainer = itemCollection.GetItems<EntityContainer>().FirstOrDefault();
            if (edmContainer != null)
            {
                allEntitySets.AddRange(edmContainer.BaseEntitySets.OfType<EntitySet>());
            }

            return allEntitySets;
        }

        /// <summary>
        ///     Retrieves an enumerable collection of all EntityType elements in this ItemCollection
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The list of EntityType elements.</returns>
        public static IEnumerable<EntityType> GetAllEntityTypes(this ItemCollection itemCollection)
        {
            return itemCollection.GetItems<EntityType>();
        }

        /// <summary>
        ///     Retrieves an enumerable collection of all AssociationType elements in this ItemCollection
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The list of AssociationType elements.</returns>
        public static IEnumerable<AssociationType> GetAllAssociations(this ItemCollection itemCollection)
        {
            return itemCollection.GetItems<AssociationType>();
        }

        /// <summary>
        ///     Retrieves an enumerable collection of all AssociationSet elements in this ItemCollection
        /// </summary>
        /// <param name="itemCollection">The ItemCollection.</param>
        /// <returns>The list of AssociationSet elements.</returns>
        public static IEnumerable<AssociationSet> GetAllAssociationSets(this ItemCollection itemCollection)
        {
            var allAssocSets = new List<AssociationSet>();

            var edmContainer = itemCollection.GetItems<EntityContainer>().FirstOrDefault();
            if (edmContainer != null)
            {
                allAssocSets.AddRange(edmContainer.BaseEntitySets.OfType<AssociationSet>());
            }

            return allAssocSets;
        }

        /// <summary>
        ///     Retrieves an enumerable collection of all ReferentialConstraints in this SSDL/StoreItemCollection
        /// </summary>
        /// <param name="storeItemCollection">StoreItemCollection representing the SSDL</param>
        /// <returns>The list of ReferentialConstraints.</returns>
        public static IEnumerable<ReferentialConstraint> GetAllReferentialConstraints(this StoreItemCollection storeItemCollection)
        {
            var refConstraints = new List<ReferentialConstraint>();
            foreach (AssociationType association in storeItemCollection.GetAllAssociations())
            {
                refConstraints.AddRange(association.ReferentialConstraints);
            }
            return refConstraints;
        }

        /// <summary>
        ///     Returns the Association in an AssociationSet
        /// </summary>
        /// <param name="associationSet">The AssociationSet.</param>
        /// <returns>The Association.</returns>
        public static AssociationType GetAssociation(this AssociationSet associationSet)
        {
            return associationSet.ElementType;
        }

        /// <summary>
        ///     If this is a property that participates in the principal end of a referential constraint, this method will return
        ///     the corresponding property on the dependent end.
        /// </summary>
        /// <example>
        ///     <ReferentialConstraint>
        ///         <Principal Role="DiscontinuedProduct">
        ///             <PropertyRef Name="ProductId" />
        ///             <PropertyRef Name="ProductName" />
        ///         </Principal>
        ///         <Dependent Role="DiscontinuedItem">
        ///             <PropertyRef Name="ItemId" />
        ///             <PropertyRef Name="ItemName" />
        ///         </Dependent>
        ///     </ReferentialConstraint>
        ///     In this example, if 'ProductName' were passed into this method, 'ItemName' would be returned.
        /// </example>
        /// <param name="property">The property on the principal end of the referential constraint</param>
        /// <param name="refConstraint">The referential constraint.</param>
        /// <returns>The property on the dependent end of the referentail constraint corresponding to the property on the principal end</returns>
        /// <exception cref="ArgumentNullException">if the ref constraint is null</exception>
        /// <exception cref="InvalidOperationException">if the property cannot be found among the properties on the principal end of the referential constraint</exception>
        public static EdmProperty GetDependentProperty(this EdmProperty property, ReferentialConstraint refConstraint)
        {
            if (refConstraint == null)
            {
                throw new ArgumentNullException("refConstraint");
            }

            var indexInFromProperties = refConstraint.FromProperties.IndexOf(property);
            if (indexInFromProperties == -1)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoPropertyInRefConstraint, property.Name, refConstraint.FromRole.Name,
                        refConstraint.ToRole.Name));
            }

            return refConstraint.ToProperties.ElementAt(indexInFromProperties);
        }

        /// <summary>
        ///     Determines if this property is a ComplexProperty; that is, its type is a ComplexType
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns>true if the property is a complex property; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an extension class.")]
        public static bool IsComplexProperty(this EdmProperty property)
        {
            return property.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType;
        }

        // <summary>
        //     A delegate type that is used by the VisitComplexProperty method to allow user-defined control over how
        //     CSDL scalar properties within CSDL complex properties are named in the SSDL
        // </summary>
        // <param name="namePrefix">The prefix of the complex property's name.</param>
        // <param name="property">A scalar property within the complex property.</param>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        internal delegate void ScalarInComplexPropertyVisitorDelegate(string namePrefix, EdmProperty property);

        // <summary>
        //     If this property is a ComplexProperty, this method will visit all scalar properties within a complex property,
        //     leaving the actions performed on that scalar property up to the caller.
        // </summary>
        // <param name="property">Complex Property</param>
        // <param name="visitorDelegate">ScalarInComplexPropertyVisitorDelegate which defines how the user wants to react to the scalar property</param>
        // <param name="delimiter">Defines the naming convention for the resulting scalar property (for example, if delimiter = '_', then the scalar property would be: 'complexProp_scalarProp'</param>
        // <param name="recursive">Boolean to define whether to step into complex properties contained within the complex property (true) or only visit the declared scalar properties (false)</param>
        internal static void VisitComplexProperty(
            this EdmProperty property, ScalarInComplexPropertyVisitorDelegate visitorDelegate, string delimiter, bool recursive)
        {
            var visitedProperties = new HashSet<EdmProperty>();
            VisitComplexPropertyInternal(property, visitorDelegate, property.Name, delimiter, recursive, visitedProperties);
        }

        private static void VisitComplexPropertyInternal(
            this EdmProperty property, ScalarInComplexPropertyVisitorDelegate visitorDelegate, string namePrefix, string delimiter,
            bool recursive, HashSet<EdmProperty> visitedProperties)
        {
            var complexType = property.TypeUsage.EdmType as ComplexType;
            if (complexType != null)
            {
                foreach (var subProperty in complexType.Properties)
                {
                    var newNamePrefix = String.Format(
                        CultureInfo.CurrentCulture,
                        "{0}{1}{2}", namePrefix, delimiter, subProperty.Name);
                    if (subProperty.IsComplexProperty() && recursive)
                    {
                        if (visitedProperties.Contains(subProperty))
                        {
                            if (subProperty != null
                                && subProperty.TypeUsage != null
                                && subProperty.TypeUsage.EdmType.Name != null)
                            {
                                throw new InvalidOperationException(
                                    String.Format(
                                        CultureInfo.CurrentCulture, Resources.ErrorComplexTypeCycle, subProperty.Name,
                                        subProperty.TypeUsage.EdmType.Name));
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonSpecificComplexTypeCycle, subProperty.Name));
                            }
                        }
                        visitedProperties.Add(subProperty);

                        subProperty.VisitComplexPropertyInternal(
                            visitorDelegate,
                            newNamePrefix,
                            delimiter,
                            recursive,
                            visitedProperties);

                        visitedProperties.Remove(subProperty);
                    }
                    else
                    {
                        visitorDelegate(newNamePrefix, subProperty);
                    }
                }
            }
        }

        /// <summary>
        ///     Infer SSDL facets from a CSDL property
        /// </summary>
        /// <param name="csdlProperty">The CSDL property.</param>
        /// <param name="providerManifest">The DbProviderManifest to use.</param>
        /// <returns>The list of facets.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Csdl")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "csdl")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IEnumerable<Facet> InferSsdlFacetsForCsdlProperty(this EdmProperty csdlProperty, DbProviderManifest providerManifest)
        {
            var storeType = csdlProperty.GetStoreTypeUsage(providerManifest);
            Dictionary<string, Facet> storeFacetLookup = storeType.Facets.ToDictionary(f => f.Name, f => f);

            // Note that there are some facets that exist in the C-side but not in the store side (Collation/ConcurrencyMode)
            // Also the IsStrict facet can exist on the C-side but should not be used on the S-side despite the fact that e.g. SqlServer.geometry defines that facet
            return
                csdlProperty.TypeUsage.Facets.Where(
                    f =>
                    (storeFacetLookup.ContainsKey(f.Name) && storeFacetLookup[f.Name].Description.IsConstant == false
                     && !"IsStrict".Equals(f.Name, StringComparison.Ordinal)));
        }

        /// <summary>
        ///     Get the StoreType from an EdmMember's EdmType through the DbProviderManifest
        /// </summary>
        /// <param name="edmMember">The EdmMember.</param>
        /// <param name="providerManifest">The DbProviderManifest.</param>
        /// <returns>The StoreType.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static string GetStoreType(this EdmMember edmMember, DbProviderManifest providerManifest)
        {
            var storeType = edmMember.GetStoreTypeUsage(providerManifest);

            if (storeType != null
                && storeType.EdmType != null
                && storeType.EdmType.Name != null)
            {
                return storeType.EdmType.Name.TrimStart('.');
            }
            return String.Empty;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static TypeUsage GetStoreTypeUsage(this EdmMember edmMember, DbProviderManifest providerManifest)
        {
            TypeUsage storeType = null;

            var conceptualType = edmMember.TypeUsage;
            Debug.Assert(conceptualType != null, "EdmMember's TypeUsage is null");
            if (conceptualType != null)
            {
                // if the EDM type is an enum, then we need to pass in the underlying type to the GetStoreType API.
                var enumType = conceptualType.EdmType as EnumType;
                storeType = (enumType != null)
                                ? providerManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(enumType.UnderlyingType))
                                : providerManifest.GetStoreType(conceptualType);
            }
            return storeType;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static PrimitiveType ThrowGetStorePrimitiveType(
            Dictionary<string, PrimitiveType> storeNameToPrimitive, string typeName, EdmMember edmMember)
        {
            PrimitiveType storePrimitiveType;
            if (false == storeNameToPrimitive.TryGetValue(typeName, out storePrimitiveType))
            {
                throw new NotSupportedException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorIncompatibleTypeForProvider, edmMember.TypeUsage.EdmType.Name,
                        edmMember.Name));
            }
            return storePrimitiveType;
        }

        /// <summary>
        ///     This will get the value of the OnDelete element on the AssociationEnd
        ///     1. This will return 'None' if the end and its sibling end have 'Cascade' specified
        ///     2. This will return 'None' if the end's multiplicity is Many (*)
        /// </summary>
        /// <param name="end">The AssociationEnd.</param>
        /// <returns>The value of the OnDelete element.</returns>
        public static OperationAction GetOnDelete(this AssociationEndMember end)
        {
            var association = end.DeclaringType as AssociationType;
            if (association != null)
            {
                AssociationEndMember otherEnd = association.GetOtherEnd(end);
                if (otherEnd != null)
                {
                    if (end.DeleteBehavior == OperationAction.Cascade
                        &&
                        otherEnd.DeleteBehavior == OperationAction.Cascade)
                    {
                        return OperationAction.None;
                    }
                }
            }

            if (end.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return OperationAction.None;
            }

            return end.DeleteBehavior;
        }

        /// <summary>
        ///     Translate an EDM Type to a SQL type, taking into account facets.
        ///     Note that certain facets were already taken into account when we obtained
        ///     them from the DbProviderManifest (FixedLength, Unicode)
        /// </summary>
        /// <param name="property">A property from which to determine the EDM Type.</param>
        /// <returns>The corresponding SQL type.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string ToStoreType(this EdmProperty property)
        {
            var sqlTypeName = String.Empty;
            var storeTypeUsage = property.TypeUsage;
            Debug.Assert(storeTypeUsage != null, "TypeUsage for property: " + property.Name + " is null");
            if (storeTypeUsage != null)
            {
                var edmType = storeTypeUsage.EdmType;
                Debug.Assert(edmType != null, "Edm Type for: " + storeTypeUsage + " is null");
                if (edmType != null)
                {
                    sqlTypeName = storeTypeUsage.EdmType.Name;
                    var primType = storeTypeUsage.EdmType as PrimitiveType;
                    if (primType != null)
                    {
                        Facet maxLengthFacet = null;
                        Facet precisionFacet = null;
                        Facet scaleFacet = null;
                        switch (primType.PrimitiveTypeKind)
                        {
                            case PrimitiveTypeKind.Binary:
                                storeTypeUsage.Facets.TryGetValue(EdmConstants.facetNameMaxLength, false, out maxLengthFacet);
                                Debug.Assert(
                                    maxLengthFacet != null, "MaxLength facet should exist for binary Store Type: " + storeTypeUsage);
                                if (maxLengthFacet != null
                                    && maxLengthFacet.Description.IsConstant == false)
                                {
                                    sqlTypeName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", sqlTypeName, maxLengthFacet.Value);
                                }
                                break;
                            case PrimitiveTypeKind.String:
                                storeTypeUsage.Facets.TryGetValue(EdmConstants.facetNameMaxLength, false, out maxLengthFacet);
                                Debug.Assert(
                                    maxLengthFacet != null, "MaxLength facet should exist for string Store Type: " + storeTypeUsage);
                                if (maxLengthFacet != null
                                    && maxLengthFacet.Description.IsConstant == false)
                                {
                                    sqlTypeName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", sqlTypeName, maxLengthFacet.Value);
                                }
                                break;
                            case PrimitiveTypeKind.Decimal:
                                storeTypeUsage.Facets.TryGetValue(EdmConstants.facetNamePrecision, false, out precisionFacet);
                                storeTypeUsage.Facets.TryGetValue(EdmConstants.facetNameScale, false, out scaleFacet);
                                Debug.Assert(
                                    precisionFacet != null, "Precision facet should exist for decimal Store Type: " + storeTypeUsage);
                                Debug.Assert(scaleFacet != null, "Scale facet should exist for decimal Store Type: " + storeTypeUsage);
                                if (precisionFacet != null
                                    &&
                                    scaleFacet != null
                                    &&
                                    precisionFacet.Description.IsConstant == false)
                                {
                                    sqlTypeName = String.Format(
                                        CultureInfo.CurrentCulture,
                                        "{0}({1},{2})", sqlTypeName, precisionFacet.Value, scaleFacet.Value);
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return sqlTypeName;
        }

        /// <summary>
        ///     Determines if an association's multiplicity is *:*
        /// </summary>
        /// <param name="association">The association.</param>
        /// <returns>true if association's multiplicity is *:*, false otherwise</returns>
        public static bool IsManyToMany(this AssociationType association)
        {
            return association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.Many
                   && association.GetEnd2().RelationshipMultiplicity == RelationshipMultiplicity.Many;
        }

        /// <summary>
        ///     Returns the first <see cref="AssociationEndMember" /> in the AssociationEndMembers property of the specified
        ///     <see
        ///         cref="AssociationType" />
        ///     .
        /// </summary>
        /// <param name="association">An association in a conceptual model.</param>
        /// <returns>
        ///     The first <see cref="AssociationEndMember" /> in the AssociationEndMembers property of the specified
        ///     <see
        ///         cref="AssociationType" />
        ///     .
        /// </returns>
        public static AssociationEndMember GetEnd1(this AssociationType association)
        {
            try
            {
                return association.AssociationEndMembers[0];
            }
            catch (IndexOutOfRangeException iore)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidAssociationEnds, association.Name), iore);
            }
        }

        /// <summary>
        ///     Returns the second <see cref="AssociationEndMember" /> in the AssociationEndMembers property of the specified
        ///     <see
        ///         cref="AssociationType" />
        ///     .
        /// </summary>
        /// <param name="association">An association in a conceptual model.</param>
        /// <returns>
        ///     The second <see cref="AssociationEndMember" /> in the AssociationEndMembers property of the specified
        ///     <see
        ///         cref="AssociationType" />
        ///     .
        /// </returns>
        public static AssociationEndMember GetEnd2(this AssociationType association)
        {
            try
            {
                return association.AssociationEndMembers[1];
            }
            catch (IndexOutOfRangeException iore)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidAssociationEnds, association.Name), iore);
            }
        }

        /// <summary>
        ///     Determines if an association is across solely primary keys
        /// </summary>
        /// <param name="association">The association.</param>
        /// <returns>true if the multiplicity of the association is 1:1, 1:0..1, or 0..1:0..1, and false otherwise</returns>
        public static bool IsPKToPK(this AssociationType association)
        {
            if ((association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.One
                 || association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
                &&
                (association.GetEnd2().RelationshipMultiplicity == RelationshipMultiplicity.One
                 || association.GetEnd2().RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Given an AssociationEndMember of this Association, this method will return the other end participating
        ///     in the association
        /// </summary>
        /// <param name="association">The Association.</param>
        /// <param name="end">An AssociationEndMember.</param>
        /// <returns>The other end.</returns>
        public static AssociationEndMember GetOtherEnd(this AssociationType association, AssociationEndMember end)
        {
            if (end != null)
            {
                return end == association.GetEnd1() ? association.GetEnd2() : association.GetEnd1();
            }
            return null;
        }

        /// <summary>
        ///     Retrieves the principal end of this association
        /// </summary>
        /// <param name="association">The association.</param>
        /// <returns>The principal end.</returns>
        public static AssociationEndMember GetPrincipalEnd(this AssociationType association)
        {
            var dependentEnd = association.GetDependentEnd();
            if (dependentEnd != null)
            {
                return association.GetOtherEnd(dependentEnd);
            }
            return null;
        }

        /// <summary>
        ///     Retrieves the dependent end of this association, given the following rules in order of priority:
        ///     1. If there is a referential constraint defined on the association, this returns the DependentEnd.
        ///     2. If the association's multiplicity is 1:1 and OnDelete='Cascade' is defined on the first end, then this returns the second end.
        ///     If OnDelete='Cascade' is not defined on the first end, this returns the first end.
        ///     3. In a 1:* or 0..1:* association, this returns the end with the * multiplicity.
        ///     4. In a 0..1:1 association, this returns the end with the 0..1 multiplicity.
        /// </summary>
        /// <param name="association">The association.</param>
        /// <returns>The dependent end.</returns>
        /// <exception cref="InvalidOperationException">if this association is *:*</exception>
        public static AssociationEndMember GetDependentEnd(this AssociationType association)
        {
            Debug.Assert(false == association.IsManyToMany(), Resources.ErrorGetDependentEndOnManyToMany);
            if (association.IsManyToMany())
            {
                throw new InvalidOperationException(Resources.ErrorGetDependentEndOnManyToMany);
            }

            if (association.ReferentialConstraints.Count > 0)
            {
                return association.ReferentialConstraints.FirstOrDefault().ToRole as AssociationEndMember;
            }
            else
            {
                // Dependency is implied by OnDelete in 1:1 associations
                if (association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.One
                    && association.GetEnd2().RelationshipMultiplicity == RelationshipMultiplicity.One)
                {
                    return association.GetEnd1().GetOnDelete() == OperationAction.Cascade ? association.GetEnd2() : association.GetEnd1();
                }

                // Dependency can also be implied by the multiplicity of the association
                if ((association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    ||
                    (association.GetEnd1().RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne
                     && association.GetEnd2().RelationshipMultiplicity == RelationshipMultiplicity.One))
                {
                    return association.GetEnd1();
                }
                return association.GetEnd2();
            }
        }

        /// <summary>
        ///     Retrieves the EntityType on an AssociationEnd.
        /// </summary>
        /// <param name="end">The AssociationEnd.</param>
        /// <returns>The EntityType.</returns>
        /// <exception cref="InvalidOperationException">if there was an error parsing this end's TypeUsage</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EntityType GetEntityType(this RelationshipEndMember end)
        {
            if (end.TypeUsage == null
                || end.TypeUsage.EdmType == null
                || !(end.TypeUsage.EdmType is RefType))
            {
                throw new InvalidOperationException(Resources.ErrorFindingEntityTypeForEnd);
            }

            var refType = end.TypeUsage.EdmType as RefType;
            var entityTypeForEnd = refType.ElementType as EntityType;

            if (entityTypeForEnd == null)
            {
                throw new InvalidOperationException(Resources.ErrorFindingEntityTypeForEnd);
            }

            return entityTypeForEnd;
        }

        /// <summary>
        ///     Gets the key properties declared directly on the EntityType
        /// </summary>
        /// <param name="entityType">The EntityType.</param>
        /// <returns>The list of key properties.</returns>
        public static IEnumerable<EdmProperty> GetKeyProperties(this EntityType entityType)
        {
            foreach (var keyMember in entityType.KeyMembers)
            {
                var property = keyMember as EdmProperty;
                if (property != null)
                {
                    yield return property;
                }
            }
        }

        /// <summary>
        ///     Gets the key properties declared directly on the EntityType within an AssociationEnd
        /// </summary>
        /// <param name="end">The AssociationEnd.</param>
        /// <returns>The list of key properties.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IEnumerable<EdmProperty> GetKeyProperties(this AssociationEndMember end)
        {
            IEnumerable<EdmProperty> keyProperties = new List<EdmProperty>();
            var entityType = end.GetEntityType();
            if (entityType != null)
            {
                keyProperties = entityType.GetKeyProperties();
            }
            return keyProperties;
        }

        /// <summary>
        ///     Determines whether the specified entity type is a derived type.
        /// </summary>
        /// <param name="entityType">An entity type in the conceptual model.</param>
        /// <returns>True if the specified entity type has a base type; false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool IsDerivedType(this EntityType entityType)
        {
            return (entityType.BaseType != null);
        }

        // <summary>
        //     This method will attempt to figure out if the given EntityType is an ancestor. It will return a list of ancestors:
        //     1. If the given EntityType is an ancestor, a list of ancestors up to and including the EntityType
        //     2. If the given EntityType is not an ancestor, all the ancestors up to the root
        // </summary>
        // <param name="entityType">The EntityType.</param>
        // <param name="rootType">The type of which to check whether this EntityType is an ancestor.</param>
        // <param name="selfAndAncestors">The list of ancestors.</param>
        // <returns>true if the given EntityType is an ancestor; otherwise, false.</returns>
        private static bool IsDerivedTypeOf(this EntityType entityType, EntityType rootType, out IList<EntityType> selfAndAncestors)
        {
            selfAndAncestors = new List<EntityType>();
            selfAndAncestors.Add(entityType);
            var baseType = entityType.BaseType as EntityType;
            var foundMatchingAncestor = false;
            while (baseType != null)
            {
                if (baseType == rootType)
                {
                    foundMatchingAncestor = true;
                    break;
                }
                selfAndAncestors.Add(baseType);
                baseType = baseType.BaseType as EntityType;
            }
            return foundMatchingAncestor;
        }

        private static IEnumerable<EntityType> GetDerivedTypes(this EntityType entityType, ItemCollection itemCollection)
        {
            var derivedTypes = new HashSet<EntityType>();
            IList<EntityType> tempAncestorList = new List<EntityType>();
            var traversedEntityTypes = new HashSet<EntityType>();
            foreach (EntityType et in itemCollection.GetAllEntityTypes())
            {
                if (traversedEntityTypes.Contains(et))
                {
                    continue;
                }
                if (et.IsDerivedTypeOf(entityType, out tempAncestorList))
                {
                    derivedTypes.UnionWith(tempAncestorList);
                }
                traversedEntityTypes.UnionWith(tempAncestorList);
                tempAncestorList.Clear();
            }
            return derivedTypes;
        }

        /// <summary>
        ///     Get all EntityTypes within this EntitySet
        /// </summary>
        /// <param name="set">The EntitySet</param>
        /// <param name="itemCollection">The ItemCollection containing the EntitySet.</param>
        /// <returns>A list of EntityTypes.</returns>
        public static IEnumerable<EntityType> GetContainingTypes(this EntitySet set, ItemCollection itemCollection)
        {
            var containingTypes = new List<EntityType>();
            var rootType = set.ElementType;
            containingTypes.Add(rootType);
            containingTypes.AddRange(rootType.GetDerivedTypes(itemCollection));
            return containingTypes;
        }

        /// <summary>
        ///     Returns this EntityType if it has no base type. Otherwise, returns the top-most base type.
        /// </summary>
        /// <param name="entityType">The EntityType.</param>
        /// <returns>The top-most base type.</returns>
        public static EntityType GetRootOrSelf(this EntityType entityType)
        {
            var baseType = entityType;
            while (baseType.BaseType != null)
            {
                baseType = baseType.BaseType as EntityType;
            }
            return baseType;
        }

        // TODO: in order to properly identify this we may need to add custom annotations in the SSDL 
        /// <summary>
        ///     We can infer that something is a join table in the SSDL if:
        ///     1. There are two associations originating from it
        ///     2. The two ends on the table are *
        ///     3. The other ends on the associations are 1
        ///     4. The number of properties in the table is equal to the sum of all the key properties on the other ends of both associations
        ///     5. All properties in the table are key properties
        /// </summary>
        /// <param name="entityType">The EntityType to test.</param>
        /// <param name="store">The StoreItemCollection containing EntityType.</param>
        /// <returns>true if the specified EntityType is a join table; otherwise, false.</returns>
        public static bool IsJoinTable(this EntityType entityType, StoreItemCollection store)
        {
            var associations = store.GetAllAssociations().Where(a => a.IsAssociatedWithEntityType(entityType));
            if (associations.Count() == 2)
            {
                var sumOfAllKeyProperties = 0;
                var numEndsWithOneMultiplicity = 0;
                foreach (AssociationType association in associations)
                {
                    AssociationEndMember end1 = association.GetEnd1();
                    AssociationEndMember end2 = association.GetEnd2();
                    if ((end1.GetEntityType() == entityType)
                        && (end1.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                        && (end2.RelationshipMultiplicity == RelationshipMultiplicity.One))
                    {
                        sumOfAllKeyProperties += end2.GetEntityType().GetKeyProperties().Count();
                        numEndsWithOneMultiplicity++;
                    }
                    else if ((end2.GetEntityType() == entityType)
                             && (end2.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                             && (end1.RelationshipMultiplicity == RelationshipMultiplicity.One))
                    {
                        sumOfAllKeyProperties += end1.GetEntityType().GetKeyProperties().Count();
                        numEndsWithOneMultiplicity++;
                    }
                }

                int intersectCount = entityType.GetKeyProperties().Intersect(entityType.Properties).Count();

                if (numEndsWithOneMultiplicity == 2
                    && entityType.GetKeyProperties().Count() == sumOfAllKeyProperties
                    && entityType.GetKeyProperties().Count() == intersectCount
                    && entityType.Properties.Count == intersectCount)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Obtains the StoreGeneratedPattern value of an EdmProperty, given a target version and DataSpace
        /// </summary>
        /// <param name="property">The EdmProperty.</param>
        /// <param name="targetVersion">Used to correctly look up the StoreGeneratedPattern value in the EdmProperty</param>
        /// <param name="dataSpace">DataSpace where the EdmProperty lives (either CSDL or SSDL)</param>
        /// <returns>One of the StoreGeneratedPattern values, or String.Empty if the attribute or value does not exist</returns>
        public static StoreGeneratedPattern GetStoreGeneratedPatternValue(
            this EdmMember property, Version targetVersion, DataSpace dataSpace)
        {
            if (targetVersion == null)
            {
                throw new ArgumentNullException("targetVersion");
            }

            if (dataSpace == DataSpace.CSSpace
                || dataSpace == DataSpace.OCSpace
                || dataSpace == DataSpace.OSpace)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidDataSpace, dataSpace.ToString()));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                // In the CSDL, StoreGeneratedPattern exists as an annotation in the EntityStoreSchemaGeneratorNamespace
                var sgpNamespace = SchemaManager.GetAnnotationNamespaceName();
                if (String.IsNullOrEmpty(sgpNamespace))
                {
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, targetVersion));
                }

                MetadataProperty sgpMetadataProperty = null;
                if (property.MetadataProperties.TryGetValue(
                    sgpNamespace + ":" + EdmConstants.facetNameStoreGeneratedPattern, false, out sgpMetadataProperty))
                {
                    var sgpValue = sgpMetadataProperty.Value as string;
                    Debug.Assert(
                        false == String.IsNullOrEmpty(sgpValue),
                        "If we found the StoreGeneratedPattern annotation in the CSDL, why weren't we able to find a value?");
                    if (false == String.IsNullOrEmpty(sgpValue))
                    {
                        return (StoreGeneratedPattern)Enum.Parse(typeof(StoreGeneratedPattern), sgpValue, false);
                    }
                }
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                // In the SSDL, StoreGeneratedPattern exists as a facet
                Facet item = null;
                if (property.TypeUsage.Facets.TryGetValue(EdmConstants.facetNameStoreGeneratedPattern, false, out item))
                {
                    return (StoreGeneratedPattern)item.Value;
                }
            }

            return StoreGeneratedPattern.None;
        }

        private static bool IsAssociatedWithEntityType(this AssociationType association, EntityType entityType)
        {
            var end1 = association.GetEnd1();
            AssociationEndMember end2 = association.GetEnd2();
            return (end1.GetEntityType() == entityType || end2.GetEntityType() == entityType);
        }

        /// <summary>
        ///     Retrieve the schema name for this EntitySet, stored in its MetadataProperties
        /// </summary>
        /// <param name="entitySet">The EntitySet.</param>
        /// <returns>The schema name.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string GetSchemaName(this EntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException("entitySet");
            }

            MetadataProperty schemaProperty;
            if (entitySet.MetadataProperties.TryGetValue("Schema", false, out schemaProperty))
            {
                string schemaPropertyValue = null;
                if (schemaProperty != null
                    && schemaProperty.Value != null
                    && ((schemaPropertyValue = schemaProperty.Value as string) != null)
                    && !String.IsNullOrEmpty(schemaPropertyValue))
                {
                    return schemaPropertyValue;
                }
            }

            return entitySet.EntityContainer.Name;
        }

        /// <summary>
        ///     Retrieve the table name for this EntitySet, stored in its MetadataProperties
        /// </summary>
        /// <param name="entitySet">The EntitySet.</param>
        /// <returns>The table name.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string GetTableName(this EntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException("entitySet");
            }
            MetadataProperty tableProperty;
            if (entitySet.MetadataProperties.TryGetValue("Table", false, out tableProperty))
            {
                string tablePropertyValue = null;
                if (tableProperty != null
                    && tableProperty.Value != null
                    && ((tablePropertyValue = tableProperty.Value as string) != null)
                    && !String.IsNullOrEmpty(tablePropertyValue))
                {
                    return tablePropertyValue;
                }
            }
            return entitySet.Name;
        }
    }
}
