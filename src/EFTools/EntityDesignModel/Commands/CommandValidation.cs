// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class contains common validation methods for Commands.
    /// </summary>
    internal static class CommandValidation
    {
        internal static void ValidateConceptualEntityContainer(ConceptualEntityContainer conceptualEntityContainer)
        {
            ValidateEFElement(conceptualEntityContainer);
            var conceptualModel = conceptualEntityContainer.Parent as BaseEntityModel;
            Debug.Assert(conceptualModel != null, "The ConceptualEntityContainer sent must be in a BaseEntityModel");
            Debug.Assert(conceptualModel.IsCSDL, "The ConceptualEntityContainer sent must be passed in from the conceptual model");
        }

        internal static void ValidateEntityType(EntityType entityType)
        {
            ValidateEFElement(entityType);
        }

        internal static void ValidateAssociation(Association association)
        {
            ValidateEFElement(association);
        }

        internal static void ValidateComplexType(ComplexType complexType)
        {
            ValidateEFElement(complexType);
        }

        internal static void ValidateEnumType(EnumType enumType)
        {
            ValidateEFElement(enumType);
        }

        internal static void ValidateConceptualEntityType(EntityType conceptualEntityType)
        {
            ValidateEntityType(conceptualEntityType);
            Debug.Assert(conceptualEntityType.EntityModel.IsCSDL, "The EntityType passed in must be from the conceptual model");
        }

        internal static void ValidateConceptualEntitySet(ConceptualEntitySet conceptualEntitySet)
        {
            ValidateEFElement(conceptualEntitySet);
            Debug.Assert(conceptualEntitySet.EntityType.Target != null, "The passed in C-Side EntitySet has unknown entity binding");
        }

        internal static void ValidateStorageEntityType(EntityType storageEntityType)
        {
            ValidateEntityType(storageEntityType);
            Debug.Assert(storageEntityType.EntityModel.IsCSDL == false, "The EntityType passed in must be from the storage model");
        }

        internal static void ValidateStorageEntitySet(StorageEntitySet storageEntitySet)
        {
            ValidateEFElement(storageEntitySet);
            Debug.Assert(storageEntitySet.EntityType.Target != null, "The passed in S-Side EntitySet has unknown entity binding");
        }

        internal static void ValidateProperty(Property property)
        {
            ValidateEFElement(property);
        }

        internal static void ValidateConceptualProperty(Property conceptualProperty)
        {
            ValidateProperty(conceptualProperty);
            Debug.Assert(
                conceptualProperty.EntityModel.IsCSDL, "The Property (conceptualProperty) passed in must be from the conceptual model");
        }

        internal static void ValidateEntityProperty(Property property)
        {
            ValidateEFElement(property);
            Debug.Assert(property.EntityType != null, "The EntityType property of this Property is null");
        }

        internal static void ValidateConceptualEntityProperty(Property conceptualProperty)
        {
            ValidateEntityProperty(conceptualProperty);
            Debug.Assert(
                conceptualProperty.EntityModel.IsCSDL, "The Property (conceptualProperty) passed in must be from the conceptual model");
        }

        internal static void ValidateStorageProperty(Property storageProperty)
        {
            ValidateEntityProperty(storageProperty);
            Debug.Assert(
                storageProperty.EntityModel.IsCSDL == false, "The Property (storageProperty) passed in must be from the storage model");
        }

        internal static void ValidateNavigationProperty(NavigationProperty navigationProperty)
        {
            ValidateEFElement(navigationProperty);
        }

        internal static void ValidateComplexTypeProperty(Property property)
        {
            ValidateEFElement(property);
            Debug.Assert(property.Parent is ComplexType, "This property parent must be a ComplexType");
        }

        internal static void ValidateTableColumn(Property tableColumn)
        {
            ValidateEntityProperty(tableColumn);
            Debug.Assert(tableColumn.EntityModel.IsCSDL != true, "The Property (tableColumn) passed in must be from the storage model");
        }

        internal static void ValidateEntityContainerMapping(EntityContainerMapping ecm)
        {
            ValidateEFElement(ecm);
            Debug.Assert(ecm.CdmEntityContainer.Target != null, "The passed in EntityContainerMapping does not reference a known C Model");
            Debug.Assert(
                ecm.StorageEntityContainer.Target != null, "The passed in EntityContainerMapping does not reference a known S Model");
        }

        internal static void ValidateEntitySetMapping(EntitySetMapping esm)
        {
            ValidateEFElement(esm);
            Debug.Assert(esm.Name.Target != null, "The passed in EntitySetMapping does not reference a known EntitySet");
        }

        internal static void ValidateEntityTypeMapping(EntityTypeMapping etm)
        {
            ValidateEFElement(etm);
            Debug.Assert(
                etm.EntitySetMapping != null, "The passed in EntityTypeMapping has a null parent or is not a child of an EntitySetMapping");
        }

        internal static void ValidateMappingFragment(MappingFragment mappingFragment)
        {
            ValidateEFElement(mappingFragment);
            Debug.Assert(
                mappingFragment.EntityTypeMapping != null,
                "The passed in MappingFragment has a null parent or is not a child of an EntityTypeMapping");
            Debug.Assert(
                mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType != null,
                "The parent EntityTypeMapping of the passed in MappingFragment is not bound to an EntityTypes");
        }

        internal static void ValidateComplexProperty(ComplexProperty complexProperty)
        {
            ValidateEFElement(complexProperty);
        }

        internal static void ValidateParameter(Parameter parm)
        {
            ValidateEFElement(parm);
            Debug.Assert(parm.Parent is Function, "The Parameter sent must be in a Function");
        }

        internal static void ValidateFunctionComplexProperty(FunctionComplexProperty fcp)
        {
            ValidateEFElement(fcp);
        }

        internal static void ValidateFunctionScalarProperty(FunctionScalarProperty sp)
        {
            ValidateEFElement(sp);
        }

        internal static void ValidateResultBinding(ResultBinding binding)
        {
            ValidateEFElement(binding);
        }

        internal static void ValidateFunctionImport(FunctionImport functionImport)
        {
            ValidateEFElement(functionImport);
        }

        internal static void ValidateFunctionImportMapping(FunctionImportMapping functionImportMapping)
        {
            ValidateEFElement(functionImportMapping);
        }

        internal static void ValidateFunction(Function function)
        {
            ValidateEFElement(function);
            // below shouldn't be hit, Functions can only be in S-Side at this point
            Debug.Assert(function.EntityModel.IsCSDL != true, "Function should be S-side");
        }

        internal static void ValidateModificationFunction(ModificationFunction mf)
        {
            ValidateEFElement(mf);
        }

        internal static void ValidateCondition(Condition cond)
        {
            ValidateEFElement(cond);
        }

        internal static void ValidateScalarProperty(ScalarProperty sp)
        {
            ValidateEFElement(sp);
        }

        internal static void ValidateAssociationEnd(AssociationEnd end)
        {
            ValidateEFElement(end);
        }

        internal static void ValidateAssociationSetMapping(AssociationSetMapping asm)
        {
            ValidateEFElement(asm);
        }

        internal static void ValidateAssociationSet(AssociationSet set)
        {
            ValidateEFElement(set);
        }

        internal static void ValidateAssociationSetEnd(AssociationSetEnd setend)
        {
            ValidateEFElement(setend);
        }

        internal static void ValidateEndProperty(EndProperty end)
        {
            ValidateEFElement(end);
        }

        internal static void ValidateFunctionImportTypeMapping(FunctionImportTypeMapping typeMapping)
        {
            ValidateEFElement(typeMapping);
        }

        private static void ValidateEFElement(EFElement element)
        {
            Debug.Assert(element != null, string.Format(CultureInfo.CurrentCulture, "You must pass in an {0}.", typeof(EFElement).Name));
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
        }
    }
}
