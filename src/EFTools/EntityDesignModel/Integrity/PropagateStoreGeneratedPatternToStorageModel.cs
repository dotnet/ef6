// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class should be registered in a transaction where we are required to propagate C-side
    ///     StoreGeneratedPattern values into the S-side ones. Applies either to an individual S-side
    ///     Property or to the whole artifact (in which case we will loop over all S-side Properties)
    /// </summary>
    internal class PropagateStoreGeneratedPatternToStorageModel : IIntegrityCheck
    {
        private readonly CommandProcessorContext _cpc;
        private readonly bool _propagateNoneSGP;
        // Note: only one of _artifact and _storageProperty should be non-null
        private readonly EFArtifact _artifact;
        private readonly StorageProperty _storageProperty;

        internal PropagateStoreGeneratedPatternToStorageModel(CommandProcessorContext cpc, EFArtifact artifact, bool propagateNoneSGP)
        {
            _cpc = cpc;
            _artifact = artifact;
            _propagateNoneSGP = propagateNoneSGP;
        }

        internal PropagateStoreGeneratedPatternToStorageModel(
            CommandProcessorContext cpc, StorageProperty storageProperty, bool propagateNoneSGP)
        {
            _cpc = cpc;
            _storageProperty = storageProperty;
            _propagateNoneSGP = propagateNoneSGP;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as PropagateStoreGeneratedPatternToStorageModel;
            if (typedOtherCheck == null)
            {
                return false;
            }

            if (_artifact != null)
            {
                if (typedOtherCheck._artifact != null
                    && _artifact.Uri.Equals(typedOtherCheck._artifact.Uri))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (_storageProperty != null)
            {
                if (typedOtherCheck._storageProperty != null
                    && _storageProperty.Equals(typedOtherCheck._storageProperty))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Debug.Fail("Both _artifact and _storageProperty are null");
            return false;
        }

        public void Invoke()
        {
            if (_artifact == null
                && _storageProperty == null)
            {
                Debug.Fail("both _artifact and _storageProperty are null");
                return;
            }
            else if (_artifact != null
                     && _storageProperty != null)
            {
                Debug.Fail("both _artifact and _storageProperty are non-null");
                return;
            }

            if (_storageProperty != null)
            {
                // propagate SGP for just this StorageProperty
                PropagateConceptualSGPToStorageProperty(_storageProperty);
            }
            else
            {
                // propagate SGP for all StorageProperties in the artifact
                if (_artifact.StorageModel() != null)
                {
                    foreach (var et in _artifact.StorageModel().EntityTypes())
                    {
                        foreach (var prop in et.Properties())
                        {
                            var sProp = prop as StorageProperty;
                            Debug.Assert(
                                null != sProp,
                                "property of Storage Model EntityType has type " + prop.GetType().FullName + ", should be "
                                + typeof(StorageProperty).FullName);
                            if (null != sProp)
                            {
                                PropagateConceptualSGPToStorageProperty(sProp);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, EFArtifact artifact, bool propagateNoneSGP)
        {
            if (null != artifact)
            {
                IIntegrityCheck check = new PropagateStoreGeneratedPatternToStorageModel(cpc, artifact, propagateNoneSGP);
                cpc.AddIntegrityCheck(check);
            }
        }

        // add one PropagateConceptualSGPToStorageProperty for each StorageProperty mapped to this ConceptualProperty
        internal static void AddRule(CommandProcessorContext cpc, ConceptualProperty cProp, bool propagateNoneSGP)
        {
            if (null != cProp)
            {
                foreach (var sProp in MappedStorageProperties(cProp))
                {
                    IIntegrityCheck check = new PropagateStoreGeneratedPatternToStorageModel(cpc, sProp, propagateNoneSGP);
                    cpc.AddIntegrityCheck(check);
                }
            }
        }

        private void PropagateConceptualSGPToStorageProperty(StorageProperty sProp)
        {
            if (null == sProp)
            {
                Debug.Fail("null StorageProperty");
                return;
            }

            if (sProp.IsDisposed)
            {
                // this StorageProperty was deleted after this integrity check was created but
                // before it was invoked - not an error - just ignore this property
                return;
            }

            if (sProp.IsKeyProperty)
            {
                if (IsStorageForNonRootEntityType(sProp))
                {
                    // this StorageProperty is for a key column on a table which acts as storage for a non-root EntityType
                    // (e.g. in a TPT hierarchy). So do not set SGP - it should be set only on the root column.
                    return;
                }

                if (IsDependentSidePropertyInAssociation(sProp))
                {
                    // this StorageProperty is used in the dependent side of an Association.
                    // So do not set SGP - it should be set only on the principal side.
                    return;
                }
            }

            // loop over ConceptualProperties mapped to this StorageProperty
            foreach (var sp in sProp.GetAntiDependenciesOfType<ScalarProperty>())
            {
                // only use ScalarProperty elements inside an EntitySetMapping or EntityTypeMapping
                // (MappingFragment is only used by those types of mappings)
                if (null != sp.GetParentOfType(typeof(MappingFragment)))
                {
                    // only propagate values from non-key C-side properties
                    var cProp = sp.Name.Target as ConceptualProperty;
                    if (cProp != null)
                    {
                        var cSideSGPValue = cProp.StoreGeneratedPattern.Value;
                        if (_propagateNoneSGP
                            || false == ModelConstants.StoreGeneratedPattern_None.Equals(cSideSGPValue, StringComparison.Ordinal))
                        {
                            // have found a C-side property whose SGP value should be propagated
                            if (false == cSideSGPValue.Equals(sProp.StoreGeneratedPattern.Value, StringComparison.Ordinal))
                            {
                                var cmd = new UpdateDefaultableValueCommand<string>(sProp.StoreGeneratedPattern, cSideSGPValue);
                                CommandProcessor.InvokeSingleCommand(_cpc, cmd);
                            }

                            return;
                        }

                        // Otherwise have found a C-side SGP but it has value "None" and have been told not to propagate "None" values
                        // (will apply to where SGP is absent too since "None" is the default value).
                        // So loop round looking for the next C-side SGP to see if it applies.
                    }
                }
            }
        }

        /// <summary>
        ///     returns list of StorageProperties mapped to a ConceptualProperty via an EntitySetMapping
        ///     or an EntityTypeMapping
        /// </summary>
        /// <param name="cProp"></param>
        /// <returns></returns>
        private static IEnumerable<StorageProperty> MappedStorageProperties(ConceptualProperty cProp)
        {
            Debug.Assert(null != cProp, "null ConceptualProperty");
            if (null != cProp)
            {
                // loop over ConceptualProperties mapped to this ConceptualProperty
                foreach (var sp in cProp.GetAntiDependenciesOfType<ScalarProperty>())
                {
                    // only use ScalarProperty elements inside an EntitySetMapping or EntityTypeMapping
                    // (MappingFragment is only used by those types of mappings)
                    if (null != sp.GetParentOfType(typeof(MappingFragment)))
                    {
                        var sProp = sp.ColumnName.Target as StorageProperty;
                        if (null != sProp)
                        {
                            yield return sProp;
                        }
                    }
                }
            }
        }

        // detects whether the S-side Property is within the table which provides storage for a C-side non-root EntityType (e.g. in a TPT hierarchy)
        private static bool IsStorageForNonRootEntityType(Property storageProperty)
        {
            if (storageProperty == null
                || storageProperty.EntityModel.IsCSDL)
            {
                Debug.Fail(
                    "should only receive a storage-side Property. Received property: "
                    + (storageProperty == null ? "NULL" : storageProperty.ToPrettyString()));
                return true; // returning true will mean this table's SGP settings will not be adjusted
            }

            foreach (var sp in storageProperty.GetAntiDependenciesOfType<ScalarProperty>())
            {
                var etm = sp.GetParentOfType(typeof(EntityTypeMapping)) as EntityTypeMapping;
                if (etm == null)
                {
                    // no EntityTypeMapping parent - so don't count this mapping
                    continue;
                }

                var etmKind = etm.Kind;
                if (EntityTypeMappingKind.Default != etmKind
                    && EntityTypeMappingKind.IsTypeOf != etmKind)
                {
                    // EntityTypeMapping is not for Default or IsTypeOf mapping - so don't count for finding the corresponding C-side EntityType
                    continue;
                }

                var cSideEntityType = etm.FirstBoundConceptualEntityType;
                if (cSideEntityType != null
                    && cSideEntityType.HasResolvableBaseType)
                {
                    var inheritanceStrategy = ModelHelper.DetermineCurrentInheritanceStrategy(cSideEntityType);
                    if (InheritanceMappingStrategy.TablePerType == inheritanceStrategy)
                    {
                        // C-side EntityType has TPT inheritance strategy and is not the base-most type in its inheritance hierarchy
                        return true;
                    }
                }
            }

            return false;
        }

        // detects whether the S-side Property represents the dependent side of an Association (e.g. for Split Entity or TPT or C-side Conditions)
        private static bool IsDependentSidePropertyInAssociation(Property storageProperty)
        {
            if (storageProperty == null
                || storageProperty.EntityModel.IsCSDL)
            {
                Debug.Fail(
                    "should only receive a storage-side Property. Received property: "
                    + (storageProperty == null ? "NULL" : storageProperty.ToPrettyString()));
                return true; // returning true will mean this table's SGP settings will not be adjusted
            }

            foreach (var propRef in storageProperty.GetAntiDependenciesOfType<PropertyRef>())
            {
                var assoc = propRef.GetParentOfType(typeof(Association)) as Association;
                if (assoc == null)
                {
                    // no Association parent - so ignore this PropertyRef
                    continue;
                }

                foreach (var prop in assoc.DependentRoleProperties)
                {
                    if (storageProperty.Equals(prop))
                    {
                        // found storage property on the dependent side which is part of an association
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
