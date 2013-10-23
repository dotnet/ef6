// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class MappingViewModelHelper
    {
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        internal static MappingViewModel CreateViewModel(EditingContext ctx, EFObject selection)
        {
            // clear out the xref so its clean for this new view model
            var xref = ModelToMappingModelXRef.GetModelToMappingModelXRef(ctx);
            xref.Clear();

            // we might be creating a view model for an entity or an association or a FunctionImport
            var entityType = selection as EntityType;
            var association = selection as Association;
            var fim = selection as FunctionImportMapping;

            // create the view model root
            MappingEFElement root = null;
            if (entityType != null)
            {
                root = ModelToMappingModelXRef.GetNewOrExisting(ctx, entityType, null);
            }
            else if (association != null)
            {
                root = ModelToMappingModelXRef.GetNewOrExisting(ctx, association, null);
            }
            else if (fim != null)
            {
                root = ModelToMappingModelXRef.GetNewOrExisting(ctx, fim, null);
            }
            else
            {
                throw new ArgumentException("selection");
            }

            return new MappingViewModel(ctx, root);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool CanEditMappingsForEntityType(ConceptualEntityType entityType, ref string errorMessage)
        {
            // make sure that the MSL is not using Ghost Nodes
            if (ModelHelper.IsAntiDepPartOfGhostMappingNode<EntityTypeMapping>(entityType))
            {
                errorMessage = Resources.MappingDetails_ErrMslUsesGhostNodes;
                return false;
            }

            // make sure that we have an EntitySet (or can find one)
            if (entityType.EntitySet == null)
            {
                errorMessage = Resources.MappingDetails_ErrMslCantFindEntitySet;
                return false;
            }

            // make sure that there is just one EntitySetMapping for our set
            var esms = entityType.EntitySet.GetAntiDependenciesOfType<EntitySetMapping>();
            if (esms.Count > 1)
            {
                errorMessage = Resources.MappingDetails_ErrMslTooManyEntitySetMappings;
                return false;
            }
            else
            {
                foreach (var esm in esms)
                {
                    if (EnsureResolvedStatus(esm, ref errorMessage) == false)
                    {
                        return false;
                    }

                    // if the EntitySetMapping contains QueryView
                    if (esm.HasQueryViewElement)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslEntitySetMappingHasQueryView;
                        return false;
                    }
                }
            }

            // load a collection of this entity's keys
            var baseMostType = entityType.ResolvableTopMostBaseType;
            var keys = new List<Property>();
            if (baseMostType.Key != null)
            {
                foreach (var keyRef in baseMostType.Key.PropertyRefs)
                {
                    if (keyRef.Name.Target != null)
                    {
                        keys.Add(keyRef.Name.Target);
                    }
                }
            }

            // Check whether the entity type is a derived type and has keys defined.
            if (entityType.HasResolvableBaseType
                && null != entityType.Key
                && null != entityType.Key.PropertyRefs
                && entityType.Key.PropertyRefs.Count > 0)
            {
                errorMessage = Resources.MappingDetails_ErrKeyInDerivedType;
                return false;
            }

            // make sure that:
            // 1. we have at most one Default ETM, one IsTypeOf ETM, and one Function ETM
            // 2. that each ETM only points to a single EntityType
            // 3. make sure that we don't have any C-side conditions
            // 4. make sure that function mappings are in their own ETM
            // 5. make sure that we don't have both a default and an IsTypeOf ETM
            //    Removed #5 per Bug 563490: In some hybrid TPH scenarios, this is actually valid
            // 6. ensure that the EntitySetMapping is hooked up to the entity's entity set
            // 7. ensure that ETMs only have one MappingFragment for a given table 
            var foundDefaultETM = false;
            var foundIsTypeOfETM = false;
            var foundFunctionETM = false;

            foreach (var etm in entityType.GetAntiDependenciesOfType<EntityTypeMapping>())
            {
                if (etm.Kind == EntityTypeMappingKind.Default)
                {
                    // 1. we have at most one Default ETM, one IsTypeOf ETM and one Function ETM
                    if (foundDefaultETM)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslTooManyDefaultEtms;
                        return false;
                    }
                    foundDefaultETM = true;

                    // 3. make sure that we don't have any C-side conditions
                    // 4. make sure that function mappings are in their own ETM
                    // 7. ensure that ETMs only have one MappingFragment
                    if (CommonEntityTypeMappingRules(etm, ref errorMessage) == false)
                    {
                        return false;
                    }
                }
                else if (etm.Kind == EntityTypeMappingKind.IsTypeOf)
                {
                    // 1. we have at most one Default ETM, one IsTypeOf ETM and one Function ETM
                    if (foundIsTypeOfETM)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslTooManyIsTypeOfETMs;
                        return false;
                    }
                    foundIsTypeOfETM = true;

                    // 3. make sure that we don't have any C-side conditions
                    // 4. make sure that function mappings are in their own ETM
                    // 7. ensure that ETMs only have one MappingFragment
                    if (CommonEntityTypeMappingRules(etm, ref errorMessage) == false)
                    {
                        return false;
                    }
                }
                else if (etm.Kind == EntityTypeMappingKind.Function)
                {
                    // 1. we have at most one Default ETM, one IsTypeOf ETM and one Function ETM
                    if (foundFunctionETM)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslTooManyFunctionETMs;
                        return false;
                    }

                    foundFunctionETM = true;

                    // 4. make sure that function mappings are in their own ETM
                    if (etm.MappingFragments().Count != 0)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslFunctionMappingsShouldBeSeparate;
                        return false;
                    }
                }

                // 2. that each ETM only points to a single EntityType
                if (etm.TypeName.IsTypeOfs.Count > 1)
                {
                    errorMessage = Resources.MappingDetails_ErrMslEtmRefsMultipleTypes;
                    return false;
                }

                // 6. ensure that the EntitySetMapping is hooked up to the entity's entity set
                if (!(etm.EntitySetMapping.Name.Status == BindingStatus.Known &&
                      etm.EntitySetMapping.Name.Target == entityType.EntitySet))
                {
                    errorMessage = Resources.MappingDetails_ErrMslBadEntitySetMapping;
                    return false;
                }
            }

            string duplicatePropertyName;
            if (!CheckDuplicateEntityProperty(entityType, out duplicatePropertyName))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture, Resources.MappingDetails_ErrDupePropertyNames, entityType.LocalName.Value,
                    duplicatePropertyName);
                return false;
            }
            return true;
        }

        internal static bool CanEditMappingsForAssociation(
            Association association, MappingDetailsWindowContainer windowContainer, ref TreeGridDesignerWatermarkInfo watermarkInfo,
            bool allowMappingEditWithFKs)
        {
            if (association == null)
            {
                throw new ArgumentNullException("association");
            }

            // make sure that we have an EntitySet (or can find one)
            if (association.AssociationSet == null)
            {
                var errorMessage = String.Format(
                    CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral,
                    Resources.MappingDetails_ErrMslCantFindAssociationSet);
                watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                return false;
            }

            var asms = association.AssociationSet.GetAntiDependenciesOfType<AssociationSetMapping>();

            var rc = association.ReferentialConstraint;
            if (rc != null)
            {
                // allowMappingEditWithFKs is an override flag that suppresses the following watermarks.
                if (allowMappingEditWithFKs == false)
                {
                    if (EdmFeatureManager.GetForeignKeysInModelFeatureState(association.Artifact.SchemaVersion)
                            .IsEnabled())
                    {
                        // set up watermarks association mapping window watermarks when FKs are supported
                        if (rc.IsPrimaryKeyToPrimaryKey())
                        {
                            if (asms.Count > 0)
                            {
                                watermarkInfo = CreateWatermarkInfoPKToPKRC_WithASM(windowContainer);
                            }
                            else
                            {
                                watermarkInfo = CreateWatermarkInfoPKToPKRC_NoASM(windowContainer);
                            }
                            return false;
                        }
                        else
                        {
                            if (asms.Count > 0)
                            {
                                watermarkInfo = CreateWatermarkInfoPKToFKRC_WithASM(windowContainer);
                            }
                            else
                            {
                                watermarkInfo = CreateWatermarkInfoPKToFKRC_NoASM();
                            }
                            return false;
                        }
                    }
                    else
                    {
                        // targeting netfx 3.5.  FKs are not supported
                        if (rc.IsPrimaryKeyToPrimaryKey() == false)
                        {
                            if (asms.Count > 0)
                            {
                                watermarkInfo = CreateWatermarkInfoPKToFKRC_WithASM(windowContainer);
                            }
                            else
                            {
                                watermarkInfo = CreateWatermarkInfoPKToFKRC_NoASM();
                            }
                            return false;
                        }
                    }
                }
            }

            // make sure that there is just one AssociationSetMapping for our set
            if (asms.Count > 1)
            {
                var errorMessage = String.Format(
                    CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral,
                    Resources.MappingDetails_ErrMslTooManyAssociationSetMappings);
                watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                return false;
            }
            else
            {
                foreach (var asm in asms)
                {
                    // if AssociationSetMapping contains QueryView
                    if (asm.HasQueryViewElement)
                    {
                        var errorMessage = String.Format(
                            CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral,
                            Resources.MappingDetails_ErrMslAssociationSetMappingHasQueryView);
                        watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                        return false;
                    }

                    // make sure that we fully resolve
                    var errorMsg = string.Empty;
                    if (EnsureResolvedStatus(asm, ref errorMsg) == false)
                    {
                        var errorMessage = String.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral, errorMsg);
                        watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                        return false;
                    }

                    // ensure that we don't have any C-side conditions
                    foreach (var cond in asm.Conditions())
                    {
                        if (cond.Name.RefName != null)
                        {
                            var errorMessage = String.Format(
                                CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral,
                                Resources.MappingDetails_ErrMslUnsupportedCondition);
                            watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                            return false;
                        }
                    }
                }
            }

            foreach (var associationEnd in association.AssociationEnds())
            {
                string duplicatePropertyName;
                var entityType = associationEnd.Type.Target;
                if (!CheckDuplicateEntityProperty(entityType, out duplicatePropertyName))
                {
                    var errorMessage = String.Format(
                        CultureInfo.CurrentCulture, Resources.MappingDetails_ErrDuplicatePropertyNameForAssociationEndEntity,
                        entityType.LocalName.Value, duplicatePropertyName);
                    errorMessage = String.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ErrMslGeneral, errorMessage);
                    watermarkInfo = new TreeGridDesignerWatermarkInfo(errorMessage);
                    return false;
                }
            }

            return true;
        }

        private static TreeGridDesignerWatermarkInfo CreateWatermarkInfoPKToPKRC_WithASM(MappingDetailsWindowContainer windowContainer)
        {
            var errorMessage = String.Format(
                CultureInfo.CurrentCulture,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Delete,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Display);
            var idx1 = errorMessage.IndexOf(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Delete, StringComparison.Ordinal);
            var idx2 = errorMessage.IndexOf(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Display, StringComparison.Ordinal);
            var watermarkInfo = new TreeGridDesignerWatermarkInfo(
                errorMessage,
                new TreeGridDesignerWatermarkInfo.LinkData(
                    idx1,
                    Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Delete.Length,
                    windowContainer.watermarkLabel_LinkClickedDeleteAssociation),
                new TreeGridDesignerWatermarkInfo.LinkData(
                    idx2, Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_ASM_Display.Length,
                    windowContainer.watermarkLabel_LinkClickedDisplayAssociation));
            return watermarkInfo;
        }

        private static TreeGridDesignerWatermarkInfo CreateWatermarkInfoPKToPKRC_NoASM(MappingDetailsWindowContainer windowContainer)
        {
            var errorMessage = String.Format(
                CultureInfo.CurrentCulture,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_NoASM,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_NoASM_Display);
            var idx1 = errorMessage.IndexOf(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_NoASM_Display, StringComparison.Ordinal);
            var watermarkInfo = new TreeGridDesignerWatermarkInfo(
                errorMessage,
                new TreeGridDesignerWatermarkInfo.LinkData(
                    idx1,
                    Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToPK_NoASM_Display.Length,
                    windowContainer.watermarkLabel_LinkClickedDisplayAssociation));
            return watermarkInfo;
        }

        private static TreeGridDesignerWatermarkInfo CreateWatermarkInfoPKToFKRC_WithASM(MappingDetailsWindowContainer windowContainer)
        {
            var errorMessage = String.Format(
                CultureInfo.CurrentCulture,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Delete,
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Display);
            var idx1 = errorMessage.IndexOf(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Delete, StringComparison.Ordinal);
            var idx2 = errorMessage.IndexOf(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Display, StringComparison.Ordinal);
            var watermarkInfo = new TreeGridDesignerWatermarkInfo(
                errorMessage,
                new TreeGridDesignerWatermarkInfo.LinkData(
                    idx1,
                    Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Delete.Length,
                    windowContainer.watermarkLabel_LinkClickedDeleteAssociation),
                new TreeGridDesignerWatermarkInfo.LinkData(
                    idx2, Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_ASM_Display.Length,
                    windowContainer.watermarkLabel_LinkClickedDisplayAssociation));
            return watermarkInfo;
        }

        private static TreeGridDesignerWatermarkInfo CreateWatermarkInfoPKToFKRC_NoASM()
        {
            var watermarkInfo = new TreeGridDesignerWatermarkInfo(
                Resources.MappingDetails_ReferentialConstraintOnAssociation_PKToFK_NoASM);
            return watermarkInfo;
        }

        internal static bool CanEditMappingsForFunctionImport(FunctionImport fi, ref string errorMessage)
        {
            // make sure that we have a FunctionImportMapping defined
            if (fi.FunctionImportMapping == null)
            {
                errorMessage = Resources.MappingDetails_ErrMslCantFindFunctionImportMapping;
                return false;
            }

            // make sure that there we can find mapped s-side Function
            if (fi.Function == null)
            {
                errorMessage = Resources.MappingDetails_ErrMslCantFindMappedFunction;
                return false;
            }

            return true;
        }

        private static bool CommonEntityTypeMappingRules(EntityTypeMapping etm, ref string errorMessage)
        {
            // 3. make sure that we don't have any C-side conditions
            foreach (var frag in etm.MappingFragments())
            {
                foreach (var cond in frag.Conditions())
                {
                    if (cond.Name.RefName != null)
                    {
                        errorMessage = Resources.MappingDetails_ErrMslUnsupportedCondition;
                        return false;
                    }
                }
            }

            // 4. make sure that function mappings are in their own ETM
            if (etm.ModificationFunctionMapping != null)
            {
                errorMessage = Resources.MappingDetails_ErrMslFunctionMappingsShouldBeSeparate;
                return false;
            }

            // 7. ensure that ETMs only have one MappingFragment for a given table
            var mappedStorageEntitySets = new HashSet<EntitySet>();
            foreach (var fragment in etm.MappingFragments())
            {
                if (fragment.StoreEntitySet.Target != null)
                {
                    if (mappedStorageEntitySets.Contains(fragment.StoreEntitySet.Target))
                    {
                        errorMessage = Resources.MappingDetails_ErrMslTooManyFragments;
                        return false;
                    }
                    else
                    {
                        mappedStorageEntitySets.Add(fragment.StoreEntitySet.Target);
                    }
                }
            }

            return true;
        }

        private static bool EnsureResolvedStatus(EFContainer container, ref string errorMessage)
        {
            if (container == null)
            {
                return true;
            }

            if (container.State != EFElementState.Resolved)
            {
                errorMessage = Resources.MappingDetails_ErrMslUnresolvedItems;
                return false;
            }

            foreach (var child in container.Children)
            {
                if (EnsureResolvedStatus(child as EFContainer, ref errorMessage) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckDuplicateEntityProperty(EntityType entityType, out string duplicatePropertyName)
        {
            // check that there are no properties with the same name in the inheritance
            // hierarchy for this EntityType. This will produce an error on validation
            // but we may not have run validation yet and it will also confuse the
            // drop-downs for the MappingDetail window when it doesn't know which of the
            // repeated properties to put in the drop-down
            var propertyNames = new HashSet<string>();
            var dupePropertyNamesForErrorMsg = new SortedList<string, int>(); // used for error msg only, int is ignored
            var type = entityType;
            duplicatePropertyName = String.Empty;

            IEnumerable<Property> allProps;
            var cet = type as ConceptualEntityType;
            if (cet != null)
            {
                allProps = cet.SafeInheritedAndDeclaredProperties;
            }
            else
            {
                allProps = type.Properties();
            }

            foreach (var prop in allProps)
            {
                var propertyName = prop.LocalName.Value;
                if (propertyNames.Contains(propertyName))
                {
                    // can duplicate more than once - if so only include in error message once
                    if (!dupePropertyNamesForErrorMsg.ContainsKey(propertyName))
                    {
                        dupePropertyNamesForErrorMsg.Add(propertyName, 0);
                    }
                }
                else
                {
                    propertyNames.Add(propertyName);
                }
            }

            if (dupePropertyNamesForErrorMsg.Count > 0)
            {
                var sb = new StringBuilder();
                var first = true;
                foreach (var propName in dupePropertyNamesForErrorMsg.Keys)
                {
                    if (!first)
                    {
                        sb.Append(Resources.MappingDetails_ErrDupePropertyNamesSeparator);
                    }
                    else
                    {
                        first = false;
                    }

                    sb.Append(propName);
                }

                duplicatePropertyName = sb.ToString();
                return false;
            }
            return true;
        }
    }
}
