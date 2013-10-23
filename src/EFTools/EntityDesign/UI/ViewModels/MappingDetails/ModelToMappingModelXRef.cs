// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;

    internal class ModelToMappingModelXRef : ContextItem
    {
        private static Dictionary<Type, Type> _modelTypeToViewModelType;
        private readonly Dictionary<EFElement, MappingEFElement> _dict = new Dictionary<EFElement, MappingEFElement>();

        private static Dictionary<Type, Type> ModelTypeToViewModelType
        {
            get
            {
                if (_modelTypeToViewModelType == null)
                {
                    _modelTypeToViewModelType = new Dictionary<Type, Type>();
                    _modelTypeToViewModelType[typeof(Condition)] = typeof(MappingCondition);
                    _modelTypeToViewModelType[typeof(Association)] = typeof(MappingAssociation);
                    _modelTypeToViewModelType[typeof(AssociationSet)] = typeof(MappingAssociationSet);
                    _modelTypeToViewModelType[typeof(AssociationSetEnd)] = typeof(MappingAssociationSetEnd);
                    _modelTypeToViewModelType[typeof(ModificationFunction)] = typeof(MappingModificationFunctionMapping);
                    _modelTypeToViewModelType[typeof(FunctionScalarProperty)] = typeof(MappingFunctionScalarProperty);
                    _modelTypeToViewModelType[typeof(ResultBinding)] = typeof(MappingResultBinding);
                    _modelTypeToViewModelType[typeof(FunctionImportMapping)] = typeof(MappingFunctionImport);
                    _modelTypeToViewModelType[typeof(FunctionImportScalarProperty)] = typeof(MappingFunctionImportScalarProperty);
                }

                return _modelTypeToViewModelType;
            }
        }

        internal static ModelToMappingModelXRef GetModelToMappingModelXRef(EditingContext context)
        {
            var xref = context.Items.GetValue<ModelToMappingModelXRef>();
            if (xref == null)
            {
                xref = new ModelToMappingModelXRef();
                context.Items.SetValue(xref);
            }
            return xref;
        }

        internal static MappingEFElement GetNewOrExisting(EditingContext context, EFElement modelElement, MappingEFElement parent)
        {
            MappingEFElement result;

            var xref = GetModelToMappingModelXRef(context);
            result = xref.GetExisting(modelElement);
            if (result != null)
            {
                result.Parent = parent;
            }
            else
            {
                Type viewModelType;
                ModelTypeToViewModelType.TryGetValue(modelElement.GetType(), out viewModelType);
                if (viewModelType == null)
                {
                    // try the base class type
                    ModelTypeToViewModelType.TryGetValue(modelElement.GetType().BaseType, out viewModelType);
                }

                if (viewModelType != null)
                {
                    result = Activator.CreateInstance(viewModelType, context, modelElement, parent) as MappingEFElement;
                    xref.Add(modelElement, result);
                }
                else
                {
                    // implement a special case for entity type
                    // create the correct C- or S-space entity type in our view model
                    var entityType = modelElement as EntityType;
                    if (entityType != null)
                    {
                        var mappingDetailsInfo = context.Items.GetValue<MappingDetailsInfo>();
                        if (mappingDetailsInfo.EntityMappingMode == EntityMappingModes.Tables)
                        {
                            var entityModel = entityType.Parent as BaseEntityModel;
                            Debug.Assert(
                                entityModel != null,
                                "entityType's parent should be an EntityModel but received type "
                                + (entityType.Parent == null ? "NULL" : entityType.Parent.GetType().FullName));

                            if (entityModel.IsCSDL)
                            {
                                result =
                                    Activator.CreateInstance(typeof(MappingConceptualEntityType), context, modelElement, parent) as
                                    MappingEFElement;
                            }
                            else
                            {
                                result =
                                    Activator.CreateInstance(typeof(MappingStorageEntityType), context, modelElement, parent) as
                                    MappingEFElement;
                            }
                        }
                        else
                        {
                            result =
                                Activator.CreateInstance(typeof(MappingFunctionEntityType), context, modelElement, parent) as
                                MappingEFElement;
                        }
                        xref.Add(modelElement, result);
                    }

                    // special case for scalar properties
                    var scalarProperty = modelElement as ScalarProperty;
                    if (scalarProperty != null)
                    {
                        if (scalarProperty.Parent is MappingFragment
                            || scalarProperty.Parent is ComplexProperty)
                        {
                            result =
                                Activator.CreateInstance(typeof(MappingScalarProperty), context, modelElement, parent) as MappingEFElement;
                        }
                        else
                        {
                            result =
                                Activator.CreateInstance(typeof(MappingEndScalarProperty), context, modelElement, parent) as
                                MappingEFElement;
                        }
                        xref.Add(modelElement, result);
                    }
                }
            }

            return result;
        }

        internal override Type ItemType
        {
            get { return typeof(ModelToMappingModelXRef); }
        }

        internal void Add(EFElement modelElement, MappingEFElement mapElement)
        {
            _dict.Add(modelElement, mapElement);
        }

        internal void Set(EFElement modelElement, MappingEFElement mapElement)
        {
            if (GetExisting(modelElement) == null)
            {
                _dict.Add(modelElement, mapElement);
            }
            else
            {
                _dict[modelElement] = mapElement;
            }
        }

        internal void Remove(EFElement modelElement)
        {
            _dict.Remove(modelElement);
        }

        internal MappingEFElement GetExisting(EFElement modelElement)
        {
            MappingEFElement result;
            _dict.TryGetValue(modelElement, out result);
            return result;
        }

        internal MappingEFElement GetExistingOrParent(EFElement modelElement)
        {
            MappingEFElement result = null;
            while (result == null
                   && modelElement != null)
            {
                if (!_dict.TryGetValue(modelElement, out result))
                {
                    modelElement = modelElement.Parent as EFElement;
                }
            }
            return result;
        }

        internal void Clear()
        {
            _dict.Clear();
        }
    }
}
