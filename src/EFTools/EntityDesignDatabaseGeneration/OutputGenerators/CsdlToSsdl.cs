// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Generates store schema definition language (SSDL) based on the provided conceptual schema definition language (CSDL).
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Csdl")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
    public class CsdlToSsdl : IGenerateActivityOutput
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Changing would require changes to GenerateActivityOutput public API")]
        private OutputGeneratorActivity _activity;
        private static string _ssdlUri, _essgUri;
        private static XNamespace _ssdl, _essg;
        private const string _storeGeneratedPattern = "StoreGeneratedPattern";

        #region Test code only

        internal static string SsdlNamespace
        {
            set
            {
                _ssdlUri = value;
                _ssdl = _ssdlUri;
            }
        }

        internal static string EssgNamespace
        {
            set
            {
                _essgUri = value;
                _essg = _essgUri;
            }
        }

        #endregion Test code only

        #region IGenerateActivityOutput Members

        // TODO perhaps build an in-memory "inference" model that keeps track of the assumptions we make (association/entity type names, etc.)
        /// <summary>
        ///     Generates store schema definition language (SSDL) based on the provided conceptual schema definition language (CSDL).
        /// </summary>
        /// <typeparam name="T"> The type of the activity output. </typeparam>
        /// <param name="owningActivity"> The currently executing activity. </param>
        /// <param name="context"> The activity context that contains the state of the workflow. </param>
        /// <param name="inputs"> Contains the incoming CSDL. </param>
        /// <returns> Store schema definition language (SSDL) of type T based on the provided conceptual schema definition language (CSDL). </returns>
        public T GenerateActivityOutput<T>(
            OutputGeneratorActivity owningActivity, NativeActivityContext context, IDictionary<string, object> inputs) where T : class
        {
            _activity = owningActivity;

            // First attempt to get the CSDL represented by the EdmItemCollection from the inputs
            object o;
            inputs.TryGetValue(EdmConstants.csdlInputName, out o);
            var edmItemCollection = o as EdmItemCollection;
            if (edmItemCollection == null)
            {
                throw new InvalidOperationException(Resources.ErrorCouldNotFindCSDL);
            }

            var symbolResolver = context.GetExtension<SymbolResolver>();
            var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
            if (edmParameterBag == null)
            {
                throw new InvalidOperationException(Resources.ErrorNoEdmParameterBag);
            }

            // Find the ProviderInvariantName parameter
            var providerInvariantName = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.ProviderInvariantName);
            if (String.IsNullOrEmpty(providerInvariantName))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined, EdmParameterBag.ParameterName.ProviderInvariantName));
            }

            // Find the ProviderManifestToken parameter
            var providerManifestToken = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.ProviderManifestToken);
            if (String.IsNullOrEmpty(providerManifestToken))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined, EdmParameterBag.ParameterName.ProviderManifestToken));
            }

            // Find the TargetVersion parameter
            var targetFrameworkVersion = edmParameterBag.GetParameter<Version>(EdmParameterBag.ParameterName.TargetVersion);
            if (targetFrameworkVersion == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined, EdmParameterBag.ParameterName.TargetVersion));
            }

            // Find the DatabaseSchemaName parameter
            var databaseSchemaName = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.DatabaseSchemaName);
            if (String.IsNullOrEmpty(databaseSchemaName))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined, EdmParameterBag.ParameterName.DatabaseSchemaName));
            }

            DbProviderManifest providerManifest = null;
            try
            {
                providerManifest =
                    DependencyResolver.GetService<DbProviderServices>(providerInvariantName).GetProviderManifest(providerManifestToken);
            }
            catch (ArgumentException ae)
            {
                // This can happen if the ProviderInvariantName is not valid
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorProviderManifestEx_ProviderInvariantName, providerInvariantName), ae);
            }
            catch (ProviderIncompatibleException pie)
            {
                // This can happen if the ProviderManifestToken is not valid
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorProviderManifestEx_ProviderManifestToken, providerManifestToken), pie);
            }

            if (providerManifest == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorCouldNotFindProviderManifest, providerInvariantName,
                        providerManifestToken));
            }

            // Resolve the SSDL namespace
            _ssdlUri = SchemaManager.GetSSDLNamespaceName(targetFrameworkVersion);
            _ssdl = _ssdlUri;

            // Resolve the ESSG namespace
            _essgUri = SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName();
            _essg = _essgUri;

            var csdlNamespace = edmItemCollection.GetNamespace();
            var ssdlNamespace = String.IsNullOrEmpty(csdlNamespace) ? "Store" : csdlNamespace + ".Store";

            var schemaElement = ConstructSchemaElement(providerInvariantName, providerManifestToken, ssdlNamespace);
            var entityContainerElement = ConstructEntityContainer(edmItemCollection, databaseSchemaName, csdlNamespace, ssdlNamespace);
            schemaElement.Add(entityContainerElement);

            var entityTypes = ConstructEntityTypes(edmItemCollection, providerManifest, targetFrameworkVersion);
            schemaElement.Add(entityTypes);

            var associations = ConstructAssociations(edmItemCollection, ssdlNamespace);
            schemaElement.Add(associations);

            var serializedSchemaElement = String.Empty;
            try
            {
                serializedSchemaElement = EdmExtension.SerializeXElement(schemaElement);
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorSerializing_CsdlToSsdl, e.Message), e);
            }
            return serializedSchemaElement as T;
        }

        #endregion

        internal static XElement ConstructSchemaElement
            (string providerInvariantName, string providerManifestToken, string ssdlNamespace)
        {
            var schemaElement =
                new XElement(
                    _ssdl + "Schema",
                    new XAttribute("Namespace", ssdlNamespace),
                    new XAttribute("Alias", "Self"),
                    new XAttribute("Provider", providerInvariantName),
                    // we'll have to create a DbConnection and set this as a parameter in the pipeline
                    new XAttribute("ProviderManifestToken", providerManifestToken),
                    new XAttribute(XNamespace.Xmlns + "store", _essgUri),
                    new XAttribute("xmlns", _ssdlUri));

            return schemaElement;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static List<XElement> ConstructEntityTypes(
            EdmItemCollection edmItemCollection, DbProviderManifest providerManifest, Version targetVersion)
        {
            var entityTypes = new List<XElement>();

            // Translate the CSDL EntityTypes into SSDL EntityTypes
            foreach (var csdlEntityType in edmItemCollection.GetAllEntityTypes())
            {
                var entityTypeElement =
                    new XElement(
                        _ssdl + "EntityType",
                        new XAttribute("Name", OutputGeneratorHelpers.GetStorageEntityTypeName(csdlEntityType, edmItemCollection)));

                // Add the keys
                if (csdlEntityType.GetRootOrSelf().GetKeyProperties().Any())
                {
                    var keyElement = new XElement(_ssdl + "Key");
                    foreach (EdmMember key in csdlEntityType.GetRootOrSelf().GetKeyProperties())
                    {
                        keyElement.Add(new XElement(_ssdl + "PropertyRef", new XAttribute("Name", key.Name)));
                    }
                    entityTypeElement.Add(keyElement);
                }

                // Add only the properties on the declared type but also add the keys that might be on the root type
                foreach (
                    var property in
                        csdlEntityType.Properties.Where(p => (p.DeclaringType == csdlEntityType))
                            .Union(csdlEntityType.GetRootOrSelf().GetKeyProperties()))
                {
                    // If we encounter a ComplexType, we need to recursively flatten it out
                    // into a list of all contained properties
                    if (property.IsComplexProperty())
                    {
                        property.VisitComplexProperty(
                            (namePrefix, nestedProperty) =>
                                {
                                    var propertyElement = new XElement(
                                        _ssdl + "Property",
                                        new XAttribute("Name", namePrefix),
                                        new XAttribute("Type", nestedProperty.GetStoreType(providerManifest)));

                                    // Add StoreGeneratedPattern if it exists, but only add this to the table created from
                                    // the root type
                                    if (property.DeclaringType == csdlEntityType)
                                    {
                                        propertyElement.Add(ConstructStoreGeneratedPatternAttribute(nestedProperty, targetVersion));
                                    }

                                    // Add the facets
                                    foreach (var facet in nestedProperty.InferSsdlFacetsForCsdlProperty(providerManifest))
                                    {
                                        if (facet.Value != null)
                                        {
                                            propertyElement.Add(new XAttribute(facet.Name, facet.Value));
                                        }
                                    }

                                    // We'll identify extended properties on all nested properties and migrate them.
                                    OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(nestedProperty, propertyElement);

                                    entityTypeElement.Add(propertyElement);
                                }, "_", true);
                    }
                    else
                    {
                        var propertyElement = new XElement(
                            _ssdl + "Property",
                            new XAttribute("Name", property.Name),
                            new XAttribute("Type", property.GetStoreType(providerManifest)));

                        // Add StoreGeneratedPattern if it exists, but only add this to the table created from
                        // the root type
                        if (property.DeclaringType == csdlEntityType)
                        {
                            propertyElement.Add(ConstructStoreGeneratedPatternAttribute(property, targetVersion));
                        }

                        // Add the facets
                        foreach (var facet in property.InferSsdlFacetsForCsdlProperty(providerManifest))
                        {
                            if (facet.Value != null)
                            {
                                var facetValue = facet.Value;

                                // for DateTime attributes, if allow XAttribute to use its default formatting we end up
                                // with attribute such as:
                                // Facet="yyyy-MM-ddTHH:mm:ssZ", but we need Facet="yyyy-MM-dd HH:mm:ss.fffZ" (note the
                                // space instead of 'T' and the fractions of seconds) to be valid SSDL
                                if (typeof(DateTime).Equals(facetValue.GetType()))
                                {
                                    facetValue = string.Format(
                                        CultureInfo.InvariantCulture, "{0:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff'Z'}", facet.Value);
                                }
                                propertyElement.Add(new XAttribute(facet.Name, facetValue));
                            }
                        }

                        OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(property, propertyElement);

                        entityTypeElement.Add(propertyElement);
                    }
                }

                // 1. If there is a Referential Constraint specified on the C-side then there is no need 
                //      to create foreign keys since the dependent end's primary keys are the foreign keys.
                // 2. In other cases, we will have to infer the foreign keys by examining any associations that
                //      the entity type participates in and add the principal keys as the foreign keys on the
                //      dependent end.
                foreach (var containedAssociation in edmItemCollection.GetAllAssociations()
                    .Where(
                        a => (a.IsManyToMany() == false) &&
                             (a.ReferentialConstraints.Count == 0) &&
                             (a.GetDependentEnd().GetEntityType() == csdlEntityType)))
                {
                    foreach (var keyProperty in containedAssociation.GetPrincipalEnd().GetEntityType().GetKeyProperties())
                    {
                        var propertyElement = new XElement(
                            _ssdl + "Property",
                            new XAttribute(
                                "Name",
                                OutputGeneratorHelpers.GetFkName(
                                    containedAssociation, containedAssociation.GetDependentEnd(), keyProperty.Name)),
                            new XAttribute("Type", keyProperty.GetStoreType(providerManifest)));

                        // Add the facets
                        foreach (var facet in keyProperty.InferSsdlFacetsForCsdlProperty(providerManifest))
                        {
                            if (facet.Value != null
                                && !facet.Name.Equals("Nullable", StringComparison.OrdinalIgnoreCase))
                            {
                                propertyElement.Add(new XAttribute(facet.Name, facet.Value));
                            }
                        }

                        // The Nullability of this property is dependent on the multiplicity of the association.
                        // If the principal end's multiplicity is 0..1, then this property is Nullable.
                        propertyElement.Add(
                            new XAttribute(
                                "Nullable",
                                containedAssociation.GetPrincipalEnd().RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne));

                        entityTypeElement.Add(propertyElement);
                    }
                }

                OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(csdlEntityType, entityTypeElement);

                entityTypes.Add(entityTypeElement);
            }

            // For all *:* Associations, we need a pair table and an associated pair EntityType
            foreach (var assoc in edmItemCollection.GetItems<AssociationType>().Where(a => a.IsManyToMany()))
            {
                var entityTypeElement = new XElement(_ssdl + "EntityType", new XAttribute("Name", assoc.Name));

                // We determine the properties as the aggregation of the primary keys from both ends of the association.
                // These properties are also the keys of this new EntityTyp
                var keyElement = new XElement(_ssdl + "Key");
                foreach (var key in assoc.GetEnd1().GetKeyProperties())
                {
                    keyElement.Add(
                        new XElement(
                            _ssdl + "PropertyRef",
                            new XAttribute("Name", OutputGeneratorHelpers.GetFkName(assoc, assoc.GetEnd2(), key.Name))));
                }
                foreach (var key in assoc.GetEnd2().GetKeyProperties())
                {
                    keyElement.Add(
                        new XElement(
                            _ssdl + "PropertyRef",
                            new XAttribute("Name", OutputGeneratorHelpers.GetFkName(assoc, assoc.GetEnd1(), key.Name))));
                }
                entityTypeElement.Add(keyElement);

                // These are foreign keys as well; we create 0..1 associations so these will be nullable keys
                foreach (var property in assoc.GetEnd1().GetKeyProperties())
                {
                    var propertyElement = new XElement(
                        _ssdl + "Property",
                        new XAttribute("Name", OutputGeneratorHelpers.GetFkName(assoc, assoc.GetEnd2(), property.Name)),
                        new XAttribute("Type", property.GetStoreType(providerManifest)));

                    // Add the facets
                    foreach (var facet in property.InferSsdlFacetsForCsdlProperty(providerManifest))
                    {
                        if (facet.Value != null)
                        {
                            propertyElement.Add(new XAttribute(facet.Name, facet.Value));
                        }
                    }

                    entityTypeElement.Add(propertyElement);
                }
                foreach (var property in assoc.GetEnd2().GetKeyProperties())
                {
                    var propertyElement = new XElement(
                        _ssdl + "Property",
                        new XAttribute("Name", OutputGeneratorHelpers.GetFkName(assoc, assoc.GetEnd1(), property.Name)),
                        new XAttribute("Type", property.GetStoreType(providerManifest)));

                    // Add the facets
                    foreach (var facet in property.InferSsdlFacetsForCsdlProperty(providerManifest))
                    {
                        if (facet.Value != null)
                        {
                            propertyElement.Add(new XAttribute(facet.Name, facet.Value));
                        }
                    }

                    entityTypeElement.Add(propertyElement);
                }

                entityTypes.Add(entityTypeElement);
            }

            return entityTypes;
        }

        internal static string TranslateMultiplicity(RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.One:
                    return "1";
                case RelationshipMultiplicity.ZeroOrOne:
                    return "0..1";
                case RelationshipMultiplicity.Many:
                    return "*";
                default:
                    return string.Empty;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static List<XElement> ConstructAssociations(EdmItemCollection edm, string ssdlNamespace)
        {
            var associations = new List<XElement>();

            // Ignore *:* associations for now, just translate the CSDL Associations into SSDL Associations
            foreach (var association in edm.GetItems<AssociationType>().Where(a => !a.IsManyToMany()))
            {
                var associationElement = new XElement(_ssdl + "Association", new XAttribute("Name", association.Name));

                var principalEnd = association.GetPrincipalEnd();
                var dependentEnd = association.GetDependentEnd();

                // 1. If we have a PK:PK relationship that has a ref constraint, then the multiplicity of the
                //      dependent end will always be 0..1.
                // 2. If we have a PK:PK relationship without a ref constraint, the multiplicity will
                //      always be *.
                // 3. If we have any other relationship, regardless of the ref constraint, we simply
                //      mirror the multiplicity from the C-side.
                if (principalEnd != null
                    && dependentEnd != null)
                {
                    foreach (var end in association.AssociationEndMembers)
                    {
                        var entityType = end.GetEntityType();
                        var multiplicity = TranslateMultiplicity(end.RelationshipMultiplicity);

                        if (end == dependentEnd
                            && association.IsPKToPK())
                        {
                            multiplicity = (association.ReferentialConstraints.Count > 0)
                                               ? TranslateMultiplicity(RelationshipMultiplicity.ZeroOrOne)
                                               : TranslateMultiplicity(RelationshipMultiplicity.Many);
                        }

                        var associationEnd = new XElement(
                            _ssdl + "End",
                            new XAttribute("Role", end.Name),
                            new XAttribute("Type", ssdlNamespace + "." + OutputGeneratorHelpers.GetStorageEntityTypeName(entityType, edm)),
                            new XAttribute("Multiplicity", multiplicity));

                        // Now we will attempt to add an OnDelete="Cascade" rule
                        if (end.GetOnDelete() == OperationAction.Cascade)
                        {
                            associationEnd.Add(
                                new XElement(
                                    _ssdl + "OnDelete",
                                    new XAttribute("Action", "Cascade")));
                        }

                        associationElement.Add(associationEnd);
                    }
                }

                // 1. If we have an existing ref constraint in the C-side, then we will simply mirror that in the SSDL.
                // 2. If we have a non *:* association without a ref constraint, then we specify the foreign keys' names of 
                //      the dependent entity type (which is the principal entity type's keys' names)
                if (association.ReferentialConstraints.Count > 0)
                {
                    var refConstraint = association.ReferentialConstraints.FirstOrDefault();
                    if (refConstraint != null)
                    {
                        associationElement.Add(
                            ConstructReferentialConstraintInternal(
                                refConstraint.FromRole.Name,
                                refConstraint.FromProperties.Select(fp => fp.Name),
                                refConstraint.ToRole.Name,
                                refConstraint.ToProperties.Select(tp => tp.Name)));
                    }
                }
                else
                {
                    associationElement.Add(
                        ConstructReferentialConstraint(
                            principalEnd.Name,
                            principalEnd,
                            dependentEnd.Name,
                            dependentEnd));
                }

                OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(association, associationElement);

                associations.Add(associationElement);
            }

            // Now let's tackle the *:* Associations. A *:* conceptual Association means that there is actually a pair table
            // in the database, and two relationships -- one from the first end to the pair table and another from the pair table
            // to the second end.
            // For *:* associations, we'll also identify any extended properties and migrate them to _both_ associations on the S-side.
            foreach (var m2mAssoc in edm.GetItems<AssociationType>().Where(a => a.IsManyToMany()))
            {
                if (m2mAssoc.GetEnd1() != null
                    && m2mAssoc.GetEnd2() != null)
                {
                    var entityType1 = m2mAssoc.GetEnd1().GetEntityType();
                    var entityType2 = m2mAssoc.GetEnd2().GetEntityType();
                    if (entityType1 != null
                        && entityType2 != null)
                    {
                        var associationElement1 = new XElement(
                            _ssdl + "Association",
                            new XAttribute("Name", OutputGeneratorHelpers.GetStorageAssociationNameFromManyToMany(m2mAssoc.GetEnd1())));
                        associationElement1.Add(
                            new XElement(
                                _ssdl + "End",
                                new XAttribute("Role", m2mAssoc.GetEnd1().Name),
                                new XAttribute(
                                    "Type", ssdlNamespace + "." + OutputGeneratorHelpers.GetStorageEntityTypeName(entityType1, edm)),
                                new XAttribute("Multiplicity", "1")));
                        associationElement1.Add(
                            new XElement(
                                _ssdl + "End",
                                new XAttribute("Role", m2mAssoc.Name),
                                new XAttribute("Type", ssdlNamespace + "." + m2mAssoc.Name),
                                new XAttribute("Multiplicity", "*")));
                        associationElement1.Add(
                            ConstructReferentialConstraint(m2mAssoc.GetEnd1().Name, m2mAssoc.GetEnd1(), m2mAssoc.Name, m2mAssoc.GetEnd2()));
                        OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(m2mAssoc, associationElement1);
                        associations.Add(associationElement1);

                        var associationElement2 = new XElement(
                            _ssdl + "Association",
                            new XAttribute("Name", OutputGeneratorHelpers.GetStorageAssociationNameFromManyToMany(m2mAssoc.GetEnd2())));
                        associationElement2.Add(
                            new XElement(
                                _ssdl + "End",
                                new XAttribute("Role", m2mAssoc.Name),
                                new XAttribute("Type", ssdlNamespace + "." + m2mAssoc.Name),
                                new XAttribute("Multiplicity", "*")));
                        associationElement2.Add(
                            new XElement(
                                _ssdl + "End",
                                new XAttribute("Role", m2mAssoc.GetEnd2().Name),
                                new XAttribute(
                                    "Type", ssdlNamespace + "." + OutputGeneratorHelpers.GetStorageEntityTypeName(entityType2, edm)),
                                new XAttribute("Multiplicity", "1")));
                        associationElement2.Add(
                            ConstructReferentialConstraint(m2mAssoc.GetEnd2().Name, m2mAssoc.GetEnd2(), m2mAssoc.Name, m2mAssoc.GetEnd1()));
                        OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(m2mAssoc, associationElement2);
                        associations.Add(associationElement2);
                    }
                }
            }

            // Finally, we will add PK:PK Associations for any inheritance found in the conceptual. These will translate into PK constraints
            // in the DDL which will round-trip back in an identifiable way. Base Type's role in this association will have OnDelete action
            // set to Cascade so that if you delete a row from the base table, any corresponding rows in the child table will also be deleted.
            foreach (var derivedType in edm.GetAllEntityTypes().Where(et => et.BaseType != null))
            {
                var pkAssociation = new XElement(
                    _ssdl + "Association",
                    new XAttribute(
                        "Name",
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.CodeViewFKConstraintDerivedType, derivedType.Name,
                            derivedType.BaseType.Name)));
                var baseTypeRole = new XElement(
                    _ssdl + "End",
                    new XAttribute("Role", derivedType.BaseType.Name),
                    new XAttribute(
                        "Type",
                        ssdlNamespace + "." + OutputGeneratorHelpers.GetStorageEntityTypeName(derivedType.BaseType as EntityType, edm)),
                    new XAttribute("Multiplicity", "1"));
                pkAssociation.Add(baseTypeRole);
                baseTypeRole.Add(new XElement(_ssdl + "OnDelete", new XAttribute("Action", "Cascade")));
                pkAssociation.Add(
                    new XElement(
                        _ssdl + "End",
                        new XAttribute("Role", derivedType.Name),
                        new XAttribute("Type", ssdlNamespace + "." + OutputGeneratorHelpers.GetStorageEntityTypeName(derivedType, edm)),
                        new XAttribute("Multiplicity", "0..1")));
                pkAssociation.Add(
                    ConstructReferentialConstraintInternal(
                        derivedType.BaseType.Name,
                        derivedType.GetRootOrSelf().GetKeyProperties().Select(k => k.Name),
                        derivedType.Name,
                        derivedType.GetRootOrSelf().GetKeyProperties().Select(k => k.Name)));
                associations.Add(pkAssociation);
            }

            return associations;
        }

        internal static XElement ConstructReferentialConstraint(
            string principalRole, AssociationEndMember principalEnd,
            string dependentRole, AssociationEndMember dependentEnd)
        {
            var refConstraintElement = new XElement(_ssdl + "ReferentialConstraint");

            if (dependentEnd != null
                && principalEnd != null)
            {
                var dependentEntityType = dependentEnd.GetEntityType();
                var principalEntityType = principalEnd.GetEntityType();
                if (dependentEntityType != null
                    && principalEntityType != null)
                {
                    refConstraintElement = ConstructReferentialConstraintInternal(
                        principalRole,
                        principalEntityType.GetKeyProperties().Select(k => k.Name),
                        dependentRole,
                        principalEntityType.GetKeyProperties().Select(
                            k => OutputGeneratorHelpers.GetFkName(
                                dependentEnd.DeclaringType as AssociationType,
                                dependentEnd,
                                k.Name)));
                }
            }

            return refConstraintElement;
        }

        internal static XElement ConstructReferentialConstraintInternal(
            string principalRole,
            IEnumerable<string> principalPropRefNames,
            string dependentRole,
            IEnumerable<string> dependentPropRefNames)
        {
            var refConstraintElement = new XElement(_ssdl + "ReferentialConstraint");
            var principalElement = new XElement(_ssdl + "Principal", new XAttribute("Role", principalRole));
            foreach (var keyName in principalPropRefNames)
            {
                principalElement.Add(new XElement(_ssdl + "PropertyRef", new XAttribute("Name", keyName)));
            }
            refConstraintElement.Add(principalElement);

            var dependentElement = new XElement(_ssdl + "Dependent", new XAttribute("Role", dependentRole));
            foreach (var keyName in dependentPropRefNames)
            {
                dependentElement.Add(new XElement(_ssdl + "PropertyRef", new XAttribute("Name", keyName)));
            }
            refConstraintElement.Add(dependentElement);
            return refConstraintElement;
        }

        internal static XElement ConstructEntityContainer(
            EdmItemCollection edm, string databaseSchemaName, string csdlNamespace, string ssdlNamespace)
        {
            var entityContainerElement =
                new XElement(
                    _ssdl + "EntityContainer",
                    new XAttribute("Name", OutputGeneratorHelpers.ConstructStorageEntityContainerName(csdlNamespace)));

            #region Constructing EntitySets

            // In TPT, we need to create the SSDL EntitySets from the EntityTypes; we create another table for the derived type.
            foreach (var entityType in edm.GetAllEntityTypes())
            {
                var entitySetElement = ConstructEntitySet(
                    ssdlNamespace, OutputGeneratorHelpers.GetStorageEntityTypeName(entityType, edm),
                    OutputGeneratorHelpers.GetStorageEntityTypeName(entityType, edm), "Tables", databaseSchemaName);

                // we would also tack on DefiningQueries here if we wanted
                entityContainerElement.Add(entitySetElement);
            }

            // Find all *:* Associations and create EntitySets in the SSDL
            foreach (var associationSet in edm.GetAllAssociationSets().Where(set => set.GetAssociation().IsManyToMany()))
            {
                var entitySetElement = ConstructEntitySet(
                    ssdlNamespace, associationSet.Name, associationSet.ElementType.Name, "Tables", databaseSchemaName);
                entityContainerElement.Add(entitySetElement);
            }

            #endregion

            #region Constructing AssociationSets

            foreach (var associationSet in edm.GetAllAssociationSets())
            {
                var assoc = associationSet.GetAssociation();

                if (assoc.GetEnd1() != null
                    && assoc.GetEnd2() != null)
                {
                    // *:* C-Space associations: we will have two S-space associations bound to the pair table corresponding to each end
                    if (assoc.IsManyToMany())
                    {
                        // create an association from the first end to the pair table
                        var associationSet1Element = ConstructAssociationSet(
                            ssdlNamespace,
                            OutputGeneratorHelpers.GetStorageAssociationSetNameFromManyToMany(associationSet, assoc.GetEnd1()),
                            OutputGeneratorHelpers.GetStorageAssociationNameFromManyToMany(assoc.GetEnd1()),
                            assoc.GetEnd1().Name,
                            OutputGeneratorHelpers.GetStorageEntityTypeName(assoc.GetEnd1().GetEntityType(), edm),
                            assoc.Name,
                            associationSet.Name);

                        // create an association from the second end to the pair table
                        var associationSet2Element = ConstructAssociationSet(
                            ssdlNamespace,
                            OutputGeneratorHelpers.GetStorageAssociationSetNameFromManyToMany(associationSet, assoc.GetEnd2()),
                            OutputGeneratorHelpers.GetStorageAssociationNameFromManyToMany(assoc.GetEnd2()),
                            assoc.GetEnd2().Name,
                            OutputGeneratorHelpers.GetStorageEntityTypeName(assoc.GetEnd2().GetEntityType(), edm),
                            assoc.Name,
                            associationSet.Name);

                        entityContainerElement.Add(associationSet1Element);
                        entityContainerElement.Add(associationSet2Element);
                    }

                        // All other associations: we essentially mirror the C-space associations
                    else
                    {
                        var associationSetElement = ConstructAssociationSet(
                            ssdlNamespace,
                            associationSet.Name,
                            assoc.Name,
                            assoc.GetEnd1().Name,
                            OutputGeneratorHelpers.GetStorageEntityTypeName(assoc.GetEnd1().GetEntityType(), edm),
                            assoc.GetEnd2().Name,
                            OutputGeneratorHelpers.GetStorageEntityTypeName(assoc.GetEnd2().GetEntityType(), edm));

                        entityContainerElement.Add(associationSetElement);
                    }
                }
            }

            // Now we will construct AssociationSets with PK:PK associations based off of inheritance
            foreach (var derivedType in edm.GetAllEntityTypes().Where(et => et.BaseType != null))
            {
                entityContainerElement.Add(
                    ConstructAssociationSet(
                        ssdlNamespace,
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.CodeViewFKConstraintDerivedType, derivedType.Name,
                            derivedType.BaseType.Name),
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.CodeViewFKConstraintDerivedType, derivedType.Name,
                            derivedType.BaseType.Name),
                        derivedType.BaseType.Name,
                        OutputGeneratorHelpers.GetStorageEntityTypeName(derivedType.BaseType as EntityType, edm),
                        derivedType.Name,
                        OutputGeneratorHelpers.GetStorageEntityTypeName(derivedType, edm)));
            }

            #endregion

            var csdlEntityContainer = edm.GetItems<EntityContainer>().FirstOrDefault();
            Debug.Assert(csdlEntityContainer != null, "Could not find the CSDL EntityContainer to migrate extended properties");
            if (csdlEntityContainer != null)
            {
                OutputGeneratorHelpers.CopyExtendedPropertiesToSsdlElement(csdlEntityContainer, entityContainerElement);
            }

            return entityContainerElement;
        }

        private static XElement ConstructEntitySet(
            string ssdlNamespace, string name, string entityType, string storeType, string databaseSchemaName)
        {
            return new XElement(
                _ssdl + "EntitySet",
                new XAttribute("Name", name),
                new XAttribute("EntityType", ssdlNamespace + "." + entityType),
                new XAttribute(_essg + "Type", storeType),
                new XAttribute("Schema", databaseSchemaName));
        }

        private static XElement ConstructAssociationSet(
            string ssdlNamespace, string name, string associationName, string end1Role, string end1EntitySet, string end2Role,
            string end2EntitySet)
        {
            return new XElement(
                _ssdl + "AssociationSet", new XAttribute("Name", name), new XAttribute("Association", ssdlNamespace + "." + associationName),
                new XElement(_ssdl + "End", new XAttribute("Role", end1Role), new XAttribute("EntitySet", end1EntitySet)),
                new XElement(_ssdl + "End", new XAttribute("Role", end2Role), new XAttribute("EntitySet", end2EntitySet)));
        }

        private static XAttribute ConstructStoreGeneratedPatternAttribute(EdmProperty property, Version targetVersion)
        {
            var sgpValue = property.GetStoreGeneratedPatternValue(targetVersion, DataSpace.CSpace);
            if (sgpValue != StoreGeneratedPattern.None)
            {
                return new XAttribute(_storeGeneratedPattern, Enum.GetName(typeof(StoreGeneratedPattern), sgpValue));
            }
            return null;
        }
    }
}
