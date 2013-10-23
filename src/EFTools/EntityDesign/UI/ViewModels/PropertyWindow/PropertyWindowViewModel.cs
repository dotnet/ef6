// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    /// <summary>
    ///     provides the information required for displaying
    ///     and editing properties of EFElement items
    /// </summary>
    internal static class PropertyWindowViewModel
    {
        private static Dictionary<Type, Type> _objectDescriptorTypes;

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public static Dictionary<Type, Type> ObjectDescriptorTypes
        {
            get
            {
                if (_objectDescriptorTypes == null)
                {
                    // build a dictionary associating all relevant EFElement derived types 
                    // to the corresponding derived type of ItemDescriptor<TEFElement>
                    _objectDescriptorTypes = new Dictionary<Type, Type>();

                    _objectDescriptorTypes[typeof(Association)] = typeof(EFAssociationDescriptor);
                    _objectDescriptorTypes[typeof(AssociationSet)] = typeof(EFAssociationSetDescriptor);
                    _objectDescriptorTypes[typeof(ConceptualEntityContainer)] = typeof(EFEntityContainerDescriptor);
                    _objectDescriptorTypes[typeof(ConceptualEntityModel)] = typeof(EFEntityModelDescriptor);
                    _objectDescriptorTypes[typeof(ConceptualEntitySet)] = typeof(EFEntitySetDescriptor);
                    _objectDescriptorTypes[typeof(ConceptualEntityType)] = typeof(EFEntityTypeDescriptor);
                    _objectDescriptorTypes[typeof(ComplexType)] = typeof(EFComplexTypeDescriptor);
                    _objectDescriptorTypes[typeof(EnumType)] = typeof(EFEnumTypeDescriptor);
                    _objectDescriptorTypes[typeof(Function)] = typeof(EFSFunctionDescriptor);
                    _objectDescriptorTypes[typeof(FunctionImport)] = typeof(EFFunctionImportDescriptor);
                    _objectDescriptorTypes[typeof(NavigationProperty)] = typeof(EFNavigationPropertyDescriptor);
                    _objectDescriptorTypes[typeof(StorageEntityContainer)] = typeof(EFSEntityContainerDescriptor);
                    _objectDescriptorTypes[typeof(StorageEntityModel)] = typeof(EFSEntityModelDescriptor);
                    _objectDescriptorTypes[typeof(StorageEntityType)] = typeof(EFEntityTypeDescriptor);
                    _objectDescriptorTypes[typeof(EntityTypeBaseType)] = typeof(EFEntityTypeBaseTypeDescriptor);
                    _objectDescriptorTypes[typeof(Diagram)] = typeof(EFDiagramDescriptor);
                    _objectDescriptorTypes[typeof(EntityTypeShape)] = typeof(EFEntityTypeShapeDescriptor);
                }

                return _objectDescriptorTypes;
            }
        }

        /// <summary>
        ///     Returns a wrapper for the specified EFObject. The wrapper is the type descriptor
        ///     that describes the properties that should be displayed for the EFObject.
        ///     The returned wrapper should be handed to a property window control
        /// </summary>
        public static ObjectDescriptor GetObjectDescriptor(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            if (obj != null)
            {
                Type objectDescriptorType;

                if (ObjectDescriptorTypes.TryGetValue(obj.GetType(), out objectDescriptorType))
                {
                    // create the descriptor wrapper for the EFObject object
                    var descriptor = (ObjectDescriptor)TypeDescriptor.CreateInstance(null, objectDescriptorType, null, null);
                    descriptor.Initialize(obj, editingContext, runningInVS);
                    return descriptor;
                }
                else
                {
                    // special case for Property
                    var property = obj as Property;
                    if (property != null)
                    {
                        ObjectDescriptor descriptor = null;
                        if (property is ComplexConceptualProperty)
                        {
                            descriptor =
                                (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFComplexPropertyDescriptor), null, null);
                        }
                        else if (property.EntityModel.IsCSDL)
                        {
                            descriptor = (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFPropertyDescriptor), null, null);
                        }
                        else
                        {
                            descriptor = (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFSPropertyDescriptor), null, null);
                        }
                        descriptor.Initialize(obj, editingContext, runningInVS);
                        return descriptor;
                    }

                    // special case for Parameter
                    var parameter = obj as Parameter;
                    if (parameter != null)
                    {
                        ObjectDescriptor descriptor = null;
                        if (parameter.Parent is FunctionImport)
                        {
                            descriptor = (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFParameterDescriptor), null, null);
                            descriptor.Initialize(obj, editingContext, runningInVS);
                            return descriptor;
                        }
                        else if (parameter.Parent is Function)
                        {
                            //Stored procedure parameter descriptor (EFSParameterDescriptor)
                            descriptor = (ObjectDescriptor)TypeDescriptor.CreateInstance(null, typeof(EFSParameterDescriptor), null, null);
                            descriptor.Initialize(obj, editingContext, runningInVS);
                            return descriptor;
                        }
                    }
                }
            }

            return null;
        }
    }
}
