// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Visitor;

    internal static class EscherModelValidator
    {
        internal static void ValidateEscherModel(EFArtifactSet set, bool forceValidation)
        {
            // TODO: Check if the validity is dirty for certain Escher error classes and validate
            // only against those. For now there is no immediate need for this.
            if (!set.IsValidityDirtyForErrorClass(ErrorClass.Escher_All)
                && !forceValidation)
            {
                return;
            }

            ClearErrors(set);
            var artifact = set.GetEntityDesignArtifact();
            if (artifact != null)
            {
                var visitor = new EscherModelValidatorVisitor(set);
                visitor.Traverse(artifact);
                artifact.SetValidityDirtyForErrorClass(ErrorClass.Escher_All, false);
            }
        }

        internal static void ClearErrors(EFArtifactSet artifactSet)
        {
            artifactSet.ClearErrors(ErrorClass.Escher_All);
        }

        internal static bool IsOpenInEditorError(ErrorInfo ei)
        {
            if ((ei.ErrorClass & ErrorClass.Escher_All) != 0)
            {
                if (ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_CIRCULAR_INHERITANCE
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_CIRCULAR_COMPLEX_TYPE_DEFINITION
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_ENTITY_TYPE_WITHOUT_ENTITY_SET
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_MULTIPE_ENTITY_SETS_PER_TYPE
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_ASSOCIATION_WITHOUT_ASSOCIATION_SET
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_INCLUDES_USING
                    || ei.ErrorCode == ErrorCodes.NON_QUALIFIED_ELEMENT)
                {
                    return true;
                }
            }
            else if (ei.ErrorClass == ErrorClass.ParseError)
            {
                if (ei.ErrorCode == ErrorCodes.ModelParse_GhostNodeNotSupportedByDesigner)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsSkipRuntimeValidationError(ErrorInfo ei)
        {
            if ((ei.ErrorClass & ErrorClass.Escher_All) != 0)
            {
                if (ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ENTITY_TYPE
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_PROPERTY
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END_KEY
                    || ei.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_CONDITION_ON_PRIMARY_KEY
                    || ei.ErrorCode == ErrorCodes.NON_QUALIFIED_ELEMENT
                    )
                {
                    return true;
                }
            }
            return false;
        }

        internal class EscherModelValidatorVisitor : Visitor
        {
            private readonly EFArtifactSet _artifactSet;

            internal EscherModelValidatorVisitor(EFArtifactSet artifactSet)
            {
                _artifactSet = artifactSet;
            }

            private EFArtifactSet ArtifactSet
            {
                get { return _artifactSet; }
            }

            internal override void Visit(IVisitable visitable)
            {
                var obj = visitable as EFObject;
                if (obj != null)
                {
                    CheckAll(obj);
                }
            }

            private void CheckAll(EFObject obj)
            {
                CheckEntityType(obj);
                CheckAssociation(obj);
                CheckEntityModel(obj);
                CheckComplexType(obj);
            }

            private void CheckEntityType(EFObject obj)
            {
                var et = obj as EntityType;
                if (et != null)
                {
                    CheckForEntityTypesWithoutEntitySets(et);
                    CheckForMultipleEntitySetsPerType(et);
                    CheckConceptualEntityType(et);
                }
            }

            private void CheckComplexType(EFObject obj)
            {
                var complexType = obj as ComplexType;
                if (complexType != null)
                {
                    CheckForCircularComplexTypeDefinition(complexType);
                    CheckForEnumPropertiesWithStoreGeneratedPattern(complexType);
                }
            }

            private void CheckConceptualEntityType(EntityType et)
            {
                var cet = et as ConceptualEntityType;
                if (cet != null)
                {
                    CheckForUnmappedEntityType(cet);
                    CheckForCircularInheritance(cet);
                    CheckForEnumPropertiesWithStoreGeneratedPattern(cet);
                }
            }

            private void CheckAssociation(EFObject obj)
            {
                var a = obj as Association;
                if (a != null)
                {
                    CheckForAssociationWithoutAssociationSet(a);
                    CheckForUnmappedAssociation(a);
                    CheckAssociationForUnmappedEntityTypeKeys(a);
                }
            }

            private void CheckEntityModel(EFObject obj)
            {
                var model = obj as ConceptualEntityModel;
                if (model != null)
                {
                    if (model.UsingCount > 0)
                    {
                        ArtifactSet.AddError(
                            new ErrorInfo(
                                ErrorInfo.Severity.ERROR, Resources.EscherValidation_UsingNotSupported, obj,
                                ErrorCodes.ESCHER_VALIDATOR_INCLUDES_USING, ErrorClass.Escher_CSDL));
                    }
                }
            }

            internal void CheckForCircularInheritance(ConceptualEntityType t)
            {
                if (t != null)
                {
                    var baseTypes = new HashSet<ConceptualEntityType>();
                    while (t != null)
                    {
                        if (baseTypes.Contains(t))
                        {
                            var msg = String.Format(
                                CultureInfo.CurrentCulture, Resources.EscherValidation_CiricularInheritance,
                                NameableItemsToCommaSeparatedString(baseTypes));
                            ArtifactSet.AddError(
                                new ErrorInfo(
                                    ErrorInfo.Severity.ERROR, msg, t, ErrorCodes.ESCHER_VALIDATOR_CIRCULAR_INHERITANCE,
                                    ErrorClass.Escher_CSDL));
                            break;
                        }
                        baseTypes.Add(t);
                        t = t.BaseType.Target;
                    }
                }
            }

            internal void CheckForCircularComplexTypeDefinition(ComplexType complexType)
            {
                if (ModelHelper.ContainsCircularComplexTypeDefinition(complexType))
                {
                    var msg = String.Format(
                        CultureInfo.CurrentCulture, Resources.EscherValidation_CiricularComplexTypeDefinition, complexType.LocalName.Value);
                    ArtifactSet.AddError(
                        new ErrorInfo(
                            ErrorInfo.Severity.ERROR, msg, complexType, ErrorCodes.ESCHER_VALIDATOR_CIRCULAR_COMPLEX_TYPE_DEFINITION,
                            ErrorClass.Escher_CSDL));
                }
            }

            internal void CheckForMultipleEntitySetsPerType(EntityType t)
            {
                if (t != null)
                {
                    var allEntitySets = new LinkedList<EntitySet>();
                    foreach (var es in t.AllEntitySets)
                    {
                        allEntitySets.AddLast(es);
                    }

                    if (allEntitySets.Count > 1)
                    {
                        var msg = String.Format(
                            CultureInfo.CurrentCulture, Resources.EscherValidation_MultipleEntitySetsPerType, t.LocalName.Value,
                            NameableItemsToCommaSeparatedString(allEntitySets));
                        var errorClass = ErrorClass.Escher_CSDL;
                        if (t is StorageEntityType)
                        {
                            errorClass = ErrorClass.Escher_SSDL;
                        }
                        ArtifactSet.AddError(
                            new ErrorInfo(
                                ErrorInfo.Severity.WARNING, msg, t, ErrorCodes.ESCHER_VALIDATOR_MULTIPE_ENTITY_SETS_PER_TYPE, errorClass));
                    }
                }
            }

            internal void CheckForEntityTypesWithoutEntitySets(EntityType t)
            {
                if (t != null)
                {
                    if (t.EntitySet == null)
                    {
                        var msg = String.Format(
                            CultureInfo.CurrentCulture, Resources.EscherValidation_EntityTypesWithoutEntitySets, t.LocalName.Value);
                        var errorClass = ErrorClass.Escher_CSDL;
                        if (t is StorageEntityType)
                        {
                            errorClass = ErrorClass.Escher_SSDL;
                        }
                        ArtifactSet.AddError(
                            new ErrorInfo(
                                ErrorInfo.Severity.WARNING, msg, t, ErrorCodes.ESCHER_VALIDATOR_ENTITY_TYPE_WITHOUT_ENTITY_SET, errorClass));
                    }
                }
            }

            internal void CheckForAssociationWithoutAssociationSet(Association a)
            {
                if (a.RuntimeModelRoot() is ConceptualEntityModel)
                {
                    if (a != null)
                    {
                        if (a.AssociationSet == null)
                        {
                            var msg = String.Format(
                                CultureInfo.CurrentCulture, Resources.EscherValidation_AssociationWithoutAssociationSet, a.LocalName.Value);
                            ArtifactSet.AddError(
                                new ErrorInfo(
                                    ErrorInfo.Severity.WARNING, msg, a, ErrorCodes.ESCHER_VALIDATOR_ASSOCIATION_WITHOUT_ASSOCIATION_SET,
                                    ErrorClass.Escher_CSDL));
                        }
                    }
                }
            }

            internal void CheckForUnmappedEntityType(ConceptualEntityType et)
            {
                if (!et.IsAbstract)
                {
                    var severity = ValidationHelper.IsStorageModelEmpty(et.Artifact) ? ErrorInfo.Severity.WARNING : ErrorInfo.Severity.ERROR;

                    var etms = et.GetAntiDependenciesOfType<EntityTypeMapping>();
                    if (etms.Count == 0)
                    {
                        // Check whether the Entity is mapped using QueryView
                        if (et.EntitySet != null)
                        {
                            var ces = et.EntitySet as ConceptualEntitySet;
                            if (ces != null
                                && ces.EntitySetMapping != null
                                && ces.EntitySetMapping.HasQueryViewElement)
                            {
                                return;
                            }
                        }

                        var msg = String.Format(
                            CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedEntityType, et.LocalName.Value);
                        ArtifactSet.AddError(
                            new ErrorInfo(severity, msg, et, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ENTITY_TYPE, ErrorClass.Escher_MSL));
                        return;
                    }

                    // entity type is mapped, so check for unmapped properties
                    foreach (var p in et.Properties())
                    {
                        var ccp = p as ComplexConceptualProperty;
                        if (ccp == null)
                        {
                            var sps = p.GetAntiDependenciesOfType<ScalarProperty>();
                            if (sps.Count == 0)
                            {
                                var msg = String.Format(
                                    CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedProperty, p.LocalName.Value);
                                ArtifactSet.AddError(
                                    new ErrorInfo(severity, msg, p, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_PROPERTY, ErrorClass.Escher_MSL));
                            }
                        }
                        else
                        {
                            var complexType = ccp.ComplexType.Target;
                            if (complexType != null
                                && ModelHelper.ContainsCircularComplexTypeDefinition(complexType) == false)
                            {
                                CheckForUnmappedComplexProperty(ccp, ccp, ccp.LocalName.Value);
                            }
                        }
                    }
                }
            }

            // Check whether all the ScalarProperties in this ComplexProperty are mapped
            private void CheckForUnmappedComplexProperty(
                ComplexConceptualProperty entityProperty, ComplexConceptualProperty currentProperty, string propertyName)
            {
                if (currentProperty.ComplexType.Status == BindingStatus.Known)
                {
                    var severity = ValidationHelper.IsStorageModelEmpty(currentProperty.Artifact)
                                       ? ErrorInfo.Severity.WARNING
                                       : ErrorInfo.Severity.ERROR;
                    foreach (var property in currentProperty.ComplexType.Target.Properties())
                    {
                        var ccp = property as ComplexConceptualProperty;
                        if (ccp == null)
                        {
                            var unmapped = true;
                            foreach (var scalarProperty in property.GetAntiDependenciesOfType<ScalarProperty>())
                            {
                                var etm = scalarProperty.GetParentOfType(typeof(EntityTypeMapping)) as EntityTypeMapping;
                                if (etm != null
                                    && etm.FirstBoundConceptualEntityType == entityProperty.Parent)
                                {
                                    unmapped = false;
                                    break;
                                }
                            }
                            if (unmapped)
                            {
                                var msg = String.Format(
                                    CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedProperty,
                                    propertyName + "." + property.LocalName.Value);
                                ArtifactSet.AddError(
                                    new ErrorInfo(
                                        severity, msg, entityProperty, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_PROPERTY, ErrorClass.Escher_MSL));
                            }
                        }
                        else
                        {
                            CheckForUnmappedComplexProperty(entityProperty, ccp, propertyName + "." + ccp.LocalName.Value);
                        }
                    }
                }
            }

            internal void CheckForUnmappedAssociation(Association a)
            {
                if (a.RuntimeModelRoot() is ConceptualEntityModel)
                {
                    var severity = ValidationHelper.IsStorageModelEmpty(a.Artifact) ? ErrorInfo.Severity.WARNING : ErrorInfo.Severity.ERROR;
                    var asms = a.GetAntiDependenciesOfType<AssociationSetMapping>();
                    if (asms.Count == 0)
                    {
                        if (a.ReferentialConstraint != null
                            && EdmFeatureManager.GetForeignKeysInModelFeatureState(a.Artifact.SchemaVersion).IsEnabled())
                        {
                            // no ASM, but we have a ref constraint.
                            return;
                        }
                        // check whether Association is mapped using QueryView
                        if (null != a.AssociationSet)
                        {
                            asms = a.AssociationSet.GetAntiDependenciesOfType<AssociationSetMapping>();
                            foreach (var asm in asms)
                            {
                                if (asm.HasQueryViewElement)
                                {
                                    return;
                                }
                            }
                        }

                        var msg = String.Format(
                            CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedAssociation, a.LocalName.Value);
                        ArtifactSet.AddError(
                            new ErrorInfo(severity, msg, a, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION, ErrorClass.Escher_MSL));
                        return;
                    }

                    // association is mapped, so check for unmapped association ends
                    foreach (var ae in a.AssociationEnds())
                    {
                        var ases = ae.GetAntiDependenciesOfType<AssociationSetEnd>();
                        foreach (var ase in ases)
                        {
                            var eps = ase.GetAntiDependenciesOfType<EndProperty>();
                            if (eps.Count == 0)
                            {
                                // Association end is not mapped.  Find nav prop to locate the error
                                EFObject objectForError = null;

                                foreach (var np in ae.GetAntiDependenciesOfType<NavigationProperty>())
                                {
                                    objectForError = np;
                                    break;
                                }

                                if (objectForError == null)
                                {
                                    objectForError = a;
                                }

                                var msg = String.Format(
                                    CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedAssociationEnd,
                                    ae.GetNameAttribute().Value);
                                ArtifactSet.AddError(
                                    new ErrorInfo(
                                        severity, msg, objectForError, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END,
                                        ErrorClass.Escher_MSL));
                            }
                        }
                    }
                }
            }

            internal void CheckAssociationForUnmappedEntityTypeKeys(Association a)
            {
                // This check verifies that each key property on the end of the association is mapped.

                foreach (var end in a.AssociationEnds())
                {
                    var et = end.Type.Target;
                    if (et != null
                        && et.Key != null)
                    {
                        EndProperty endProperty = null;
                        var associationSetEnd = GetFirstAntiDependencyOfType<AssociationSetEnd>(end);
                        if (associationSetEnd != null)
                        {
                            endProperty = GetFirstAntiDependencyOfType<EndProperty>(associationSetEnd);
                        }

                        if (endProperty != null)
                        {
                            var severity = ValidationHelper.IsStorageModelEmpty(et.Artifact)
                                               ? ErrorInfo.Severity.WARNING
                                               : ErrorInfo.Severity.ERROR;
                            // check each key property in the entity type
                            foreach (var pr in et.Key.PropertyRefs)
                            {
                                var found = false;
                                var prop = pr.Name.Target;
                                if (prop != null)
                                {
                                    foreach (var sp in endProperty.ScalarProperties())
                                    {
                                        if (sp.Name.Target != null)
                                        {
                                            if (sp.Name.Target.Equals(prop))
                                            {
                                                found = true;
                                            }
                                        }
                                    }

                                    if (!found)
                                    {
                                        // didn't find the key property mapped so add an error
                                        var msg = String.Format(
                                            CultureInfo.CurrentCulture, Resources.EscherValidation_UnmappedAssociationEndKey,
                                            prop.LocalName.Value);
                                        ArtifactSet.AddError(
                                            new ErrorInfo(
                                                severity, msg, a, ErrorCodes.ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END_KEY,
                                                ErrorClass.Escher_MSL));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void CheckForEnumPropertiesWithStoreGeneratedPattern(EntityType entity)
            {
                CheckForEnumPropertiesWithStoreGeneratedPattern(entity.Properties().OfType<ConceptualProperty>().Where(p => p.IsEnumType));
            }

            private void CheckForEnumPropertiesWithStoreGeneratedPattern(ComplexType complexType)
            {
                CheckForEnumPropertiesWithStoreGeneratedPattern(
                    complexType.Properties().OfType<ConceptualProperty>().Where(p => p.IsEnumType));
            }

            private void CheckForEnumPropertiesWithStoreGeneratedPattern(IEnumerable<Property> properties)
            {
                foreach (var property in properties)
                {
                    if (property.StoreGeneratedPattern.Value != ModelConstants.StoreGeneratedPattern_None)
                    {
                        ArtifactSet.AddError(
                            new ErrorInfo(
                                ErrorInfo.Severity.WARNING,
                                Resources.EnumPropertyHasNonEmptyStoreGeneratedPattern,
                                property,
                                ErrorCodes.ESCHER_VALIDATOR_ENUM_PROPERTY_WITH_STOREGENERATEDPATTERN,
                                ErrorClass.Escher_CSDL));
                    }
                }
            }

            private static string NameableItemsToCommaSeparatedString<T>(ICollection<T> types) where T : EFNameableItem
            {
                var i = 0;
                var sb = new StringBuilder();
                foreach (EFNameableItem et in types)
                {
                    sb.Append(et.LocalName.Value);
                    i++;
                    if (i < types.Count)
                    {
                        sb.Append(Resources.SeparatorCharacterForMultipleItemsInAnErrorMessage);
                    }
                }
                return sb.ToString();
            }

            private static T GetFirstAntiDependencyOfType<T>(EFObject obj) where T : EFObject
            {
                var antiDeps = obj.GetAntiDependenciesOfType<T>();
                if (antiDeps != null)
                {
                    foreach (var t in antiDeps)
                    {
                        return t;
                    }
                }
                return null;
            }
        }
    }
}
