// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;

    /// <summary>
    ///     This class will loop over all mappings from Storage-side (S-side) properties to
    ///     Conceptual-side (C-side) properties where the C-side is within an EntityType,
    ///     and will update various C-side property facets to match the S-side ones.
    /// </summary>
    internal class PropagateStoragePropertyFacetsToConceptualModel : IIntegrityCheck
    {
        private readonly CommandProcessorContext _cpc;
        private readonly EFArtifact _artifact;

        // map of S-side type name to map of facet name to facet's default value at runtime
        private IDictionary<string, IDictionary<string, object>> _sSideFacetDefaults;
        // map of C-side type name to map of facet name to facet's default value at runtime
        private IDictionary<string, IDictionary<string, object>> _cSideFacetDefaults;
        // list of facet-synchronizer delegates which will get called later
        private IList<SynchronizeConceptualFacet> _facetSynchronizers;

        internal PropagateStoragePropertyFacetsToConceptualModel(CommandProcessorContext cpc, EFArtifact artifact)
        {
            _cpc = cpc;
            _artifact = artifact;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as PropagateStoragePropertyFacetsToConceptualModel;
            if (typedOtherCheck != null)
            {
                if (typedOtherCheck._artifact.Uri.Equals(_artifact.Uri))
                {
                    return true;
                }
            }

            return false;
        }

        private void Initialize(StorageEntityModel sem)
        {
            // initialize S-side facet default values from runtime for all valid S-side Property types
            _sSideFacetDefaults = new Dictionary<string, IDictionary<string, object>>();
            var allStoreSideTypes = sem.StoreTypeNameToStoreTypeMap.Values;
            foreach (var sSidePrimitiveType in allStoreSideTypes)
            {
                // fill out S-side facets defaults
                var sSideTypeMap = _sSideFacetDefaults[sSidePrimitiveType.Name] = new Dictionary<string, object>();
                var sSideTypeUsage = TypeUsage.CreateDefaultTypeUsage(sSidePrimitiveType);
                Debug.Assert(null != sSideTypeUsage, "null sSideTypeUsage for sSidePrimitiveType " + sSidePrimitiveType.FullName);
                if (null != sSideTypeUsage)
                {
                    foreach (var facet in sSideTypeUsage.Facets)
                    {
                        var fd = facet.Description;
                        if (null != fd)
                        {
                            var name = fd.FacetName;
                            var defaultValue = fd.DefaultValue;

                            // Special treatment for MaxLength facet. The default for MaxLength for some S-side types
                            // is int.MaxValue or int.MaxValue/2. If the S-side is defaulted we should really propagate
                            // the value "Max" instead. So override here.
                            if (Property.AttributeMaxLength.Equals(name, StringComparison.Ordinal))
                            {
                                // value could be an Int32 or a string - but need to pass in as a UInt32 to ModelHelper.GetMaxLengthFacetValue()
                                uint defaultValueAsUInt;
                                if (null != defaultValue
                                    && uint.TryParse(defaultValue.ToString(), out defaultValueAsUInt))
                                {
                                    defaultValue = ModelHelper.GetMaxLengthFacetValue(defaultValueAsUInt);
                                }
                            }

                            sSideTypeMap[name] = defaultValue;
                        }
                    }
                }
            }

            // initialize C-side facet default values from runtime for all valid C-side Property types
            _cSideFacetDefaults = new Dictionary<string, IDictionary<string, object>>();
            var edmCollection = new EdmItemCollection(new XmlReader[] { });
            foreach (var cSidePrimitiveType in edmCollection.GetPrimitiveTypes())
            {
                // fill out C-side facets defaults
                var cSideTypeMap = _cSideFacetDefaults[cSidePrimitiveType.Name] = new Dictionary<string, object>();
                var cSideTypeUsage = TypeUsage.CreateDefaultTypeUsage(cSidePrimitiveType);
                Debug.Assert(null != cSideTypeUsage, "null cSideTypeUsage for cSidePrimitiveType " + cSidePrimitiveType.FullName);
                if (null != cSideTypeUsage)
                {
                    foreach (var facet in cSideTypeUsage.Facets)
                    {
                        var fd = facet.Description;
                        if (null != fd)
                        {
                            var name = fd.FacetName;
                            var defaultValue = fd.DefaultValue;
                            cSideTypeMap[name] = defaultValue;
                        }
                    }
                }
            }

            // initialize the list of facet-synchronizers
            _facetSynchronizers = new List<SynchronizeConceptualFacet>();

            // add the synchronizer for Precision
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.Precision, conceptualProperty.TypeName, conceptualProperty.Precision));
            // add the synchronizer for Scale
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.Scale, conceptualProperty.TypeName, conceptualProperty.Scale));
            // add the synchronizer for MaxLength
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.MaxLength, conceptualProperty.TypeName, conceptualProperty.MaxLength));
            // add the synchronizer for Nullable
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.Nullable, conceptualProperty.TypeName, conceptualProperty.Nullable));
            // add the synchronizer for Unicode
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.Unicode, conceptualProperty.TypeName, conceptualProperty.Unicode));
            // add the synchronizer for FixedLength
            _facetSynchronizers.Add(
                (storageProperty, conceptualProperty) => SynchronizeFacet(
                    storageProperty.TypeName, storageProperty.FixedLength, conceptualProperty.TypeName,
                    conceptualProperty.FixedLength));
        }

        // helper method to return the runtime facet default for a particular S-side Property type and facet
        private T GetStorageFacetDefault<T>(string propertyType, DefaultableValue<T> defaultableValue) where T : class
        {
            return GetFacetDefault(_sSideFacetDefaults, propertyType, defaultableValue);
        }

        // helper method to return the runtime facet default for a particular C-side Property type and facet
        private T GetConceptualFacetDefault<T>(string propertyType, DefaultableValue<T> defaultableValue) where T : class
        {
            return GetFacetDefault(_cSideFacetDefaults, propertyType, defaultableValue);
        }

        // helper method to return the runtime facet default for a Property type (either S- or C-side) and facet
        // (Note: this is different from defaultableValue.DefaultValue which returns what to show in the
        // Designer as the default - usually "(None)")
        private static T GetFacetDefault<T>(
            IDictionary<string, IDictionary<string, object>> propertyTypeToFacetsMap, string propertyType,
            DefaultableValue<T> defaultableValue) where T : class
        {
            if (string.IsNullOrWhiteSpace(propertyType))
            {
                // cannot return facet defaults if type is not defined
                return null;
            }

            IDictionary<string, object> facetToDefaultMap;
            if (propertyTypeToFacetsMap.TryGetValue(propertyType, out facetToDefaultMap))
            {
                object facetDefaultAsObject;
                if (null != facetToDefaultMap
                    && facetToDefaultMap.TryGetValue(defaultableValue.AttributeName, out facetDefaultAsObject))
                {
                    var facetDefault =
                        (null == facetDefaultAsObject ? null : defaultableValue.ConvertStringToValue(facetDefaultAsObject.ToString()));
                    return facetDefault;
                }
            }

            return null;
        }

        // helper method: checks whether a facet with a given name is supposed to exist for the given C-side property type
        // (Note: need to distinguish between that and when the facet does exist but the default is null)
        private bool ConceptualFacetExists(string cSidePropertyType, string cSideFacetName)
        {
            IDictionary<string, object> facetDefaultValueMap;
            if (_cSideFacetDefaults.TryGetValue(cSidePropertyType, out facetDefaultValueMap))
            {
                object facetDefault;
                if (facetDefaultValueMap.TryGetValue(cSideFacetName, out facetDefault))
                {
                    return true;
                }
            }

            return false;
        }

        public void Invoke()
        {
            if (_artifact != null)
            {
                Initialize(_artifact.StorageModel());
                PropagateAllStoragePropertyFacets(_artifact);
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, EFArtifact artifact)
        {
            if (artifact != null)
            {
                IIntegrityCheck check = new PropagateStoragePropertyFacetsToConceptualModel(cpc, artifact);
                cpc.AddIntegrityCheck(check);
            }
        }

        private void PropagateAllStoragePropertyFacets(EFArtifact artifact)
        {
            var sModel = artifact.StorageModel();
            if (null == sModel)
            {
                Debug.Fail("null StorageEntityModel");
                return;
            }

            var cModel = artifact.ConceptualModel();
            if (null == cModel)
            {
                Debug.Fail("null ConceptualEntityModel");
                return;
            }

            // loop over every S-side Property
            foreach (var sSideEntityType in sModel.EntityTypes())
            {
                foreach (var sSideProperty in sSideEntityType.Properties())
                {
                    // add every mapped C-side Property whose parent is a C-side EntityType (as opposed to a ComplexType)
                    var cSideProperties = new HashSet<Property>();
                    foreach (var scalarProp in sSideProperty.GetAntiDependenciesOfType<ScalarProperty>())
                    {
                        // only count mappings through EntitySetMapping and EntityTypeMapping (MappingFragment only
                        // appears as a child in these kinds of mappings). Mappings through e.g.
                        // AssociationSetMapping do not identify the C-side properties to be updated.
                        if (scalarProp.GetParentOfType(typeof(MappingFragment)) != null)
                        {
                            var cSideProperty = scalarProp.Name.Target;
                            if (null != cSideProperty
                                && null != cSideProperty.GetParentOfType(typeof(EntityType))
                                && false == cSideProperties.Contains(cSideProperty))
                            {
                                cSideProperties.Add(cSideProperty);
                            }
                        }
                    }

                    // propagate the facets for this S-side Property to all the C-side Properties that are mapped to it
                    foreach (var mappedCSideProperty in cSideProperties)
                    {
                        // for each mapped C-side Property, loop over all the facet-synchronizers invoking them
                        foreach (var facetSynchronizer in _facetSynchronizers)
                        {
                            facetSynchronizer.Invoke(sSideProperty, mappedCSideProperty);
                        }
                    }
                }
            }
        }

        // helper method which compares values and synchronizes the C-side facet to the S-side value if they differ
        private void SynchronizeFacet<T>(
            string sSidePropertyType, DefaultableValue<T> sSideDefaultableValue,
            string cSidePropertyType, DefaultableValue<T> cSideDefaultableValue) where T : class
        {
            if (null == sSidePropertyType)
            {
                Debug.Fail("sSidePropertyType cannot be null");
                return;
            }

            if (null == sSideDefaultableValue)
            {
                Debug.Fail("sSideDefaultableValue cannot be null");
                return;
            }

            if (null == cSidePropertyType)
            {
                Debug.Fail("cSidePropertyType cannot be null");
                return;
            }

            if (null == cSideDefaultableValue)
            {
                Debug.Fail("cSideDefaultableValue cannot be null");
                return;
            }

            if (false == ConceptualFacetExists(cSidePropertyType, cSideDefaultableValue.AttributeName))
            {
                // this facet does not apply to a C-side property of this type - so do not attempt to propagate the S-side value
                // (this can happen e.g. if an S-side property is mapped to a C-side property with an inconsistent type)
                return;
            }

            // get values that S- and C-side DefaultableValues will assume at runtime if they are absent
            // (Note: this is different from DefaultableValue.DefaultValue which returns what
            // to show in the Designer as the default - usually "(None)")
            var sSideDefaultableValueDefaultValue = GetStorageFacetDefault(sSidePropertyType, sSideDefaultableValue);

            var sSideValueToPropagate = sSideDefaultableValue.Value;
            var preExistingCSideValue = cSideDefaultableValue.Value;
            T valueToSet;
            if (sSideDefaultableValue.IsDefaulted
                ||
                null == sSideValueToPropagate
                ||
                sSideValueToPropagate.Equals(sSideDefaultableValueDefaultValue))
            {
                // sometimes the default value for a facet on the C-side is the same as that for the same
                // facet on the S-side, but sometimes they are different. Here the S-side has been defaulted
                // so we need to do different things dependent on whether these default values are the same.
                var cSideDefaultableValueDefaultValue = GetConceptualFacetDefault(cSidePropertyType, cSideDefaultableValue);
                if ((null == sSideDefaultableValueDefaultValue && null == cSideDefaultableValueDefaultValue)
                    ||
                    (null != sSideDefaultableValueDefaultValue
                     && sSideDefaultableValueDefaultValue.Equals(cSideDefaultableValueDefaultValue)))
                {
                    // default value for C-side facet is the same as the default value for the S-side facet
                    if (cSideDefaultableValue.IsDefaulted
                        ||
                        null == preExistingCSideValue
                        ||
                        preExistingCSideValue.Equals(cSideDefaultableValueDefaultValue))
                    {
                        // the attribute on both sides is either absent or explicitly set to the default value
                        // (note: the null case is when the default value itself is null)- so no need to update the C-side.
                        // This prevents "absent" on the S-side causing an update when the pre-existing C-side value is
                        // the default value and vice versa.
                        return;
                    }

                    // if absent on the S-side, set valueToSet to null which will cause the C-side attribute to be removed,
                    // otherwise use the S-side's explicit value.
                    valueToSet = (sSideDefaultableValue.IsDefaulted ? null : sSideValueToPropagate);
                }
                else if (sSideDefaultableValue.IsDefaulted
                         && null == sSideDefaultableValueDefaultValue)
                {
                    // S-side is defaulted but we have no default value to propagate
                    Debug.Fail(
                        "S-side DefaultableValue is defaulted but default value of S-side DefaultableValue is null, so defaulting C-side DefaultableValue would cause it to assume different default value: "
                        + cSideDefaultableValueDefaultValue);
                    return;
                }
                else
                {
                    // default value for C-side facet is different from the default value for the S-side facet
                    // so need to explicitly set the C-side. Only reason for not updating is if the value is
                    // already what we would set it to.
                    if (false == cSideDefaultableValue.IsDefaulted
                        && sSideDefaultableValueDefaultValue.Equals(preExistingCSideValue))
                    {
                        return;
                    }

                    valueToSet = (sSideDefaultableValue.IsDefaulted ? sSideDefaultableValueDefaultValue : sSideValueToPropagate);
                }
            }
            else
            {
                // we are synchronizing an explicit (i.e. non-default) value from the S-side to the C-side -
                // only reason for not updating is if the value is already what we would set it to.
                if (sSideValueToPropagate.Equals(preExistingCSideValue))
                {
                    // values are the same - so no need to update
                    return;
                }

                valueToSet = sSideValueToPropagate;
            }

            // now update the C-side facet
            var cmd = new UpdateDefaultableValueCommand<T>(cSideDefaultableValue, valueToSet);
            CommandProcessor.InvokeSingleCommand(_cpc, cmd);
        }

        private delegate void SynchronizeConceptualFacet(Property storageProperty, Property conceptualProperty);
    }
}
