// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    /// <summary>
    ///     type converter to be associated to properties that are expandable in
    ///     the property window (i.e. a property that has sub-properties)
    /// </summary>
    internal class ExpandablePropertyConverter : ExpandableObjectConverter
    {
        private object _lastComponent;
        private PropertyDescriptorCollection _properties;

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            if (_lastComponent != component)
            {
                // one single instance of the type converter may be used for querying the 
                // properties of different component instances, but the list of properties is 
                // specific to each component, so invalidate the cached list of properties if 
                // the component is not the same as the last queried one.

                // TODO: verify if this can be solved by deriving from IResettableConverter
                _properties = null;
                _lastComponent = component;
            }

            if (_properties == null)
            {
                EditingContext editingContext = null;
                foreach (var typeDescriptor in PropertyWindowViewModelHelper.GetObjectsFromSelection<ObjectDescriptor>(context.Instance))
                {
                    editingContext = typeDescriptor.EditingContext;
                    break;
                }

                Debug.Assert(editingContext != null);

                if (editingContext != null)
                {
                    _properties = new PropertyDescriptorCollection(null);

                    // get list of properties through reflection
                    PropertyWindowViewModelHelper.AddReflectedProperties(_properties, component, attributes, editingContext);

                    // add properties from extender objects
                    PropertyWindowViewModelHelper.AddExtendedProperties(
                        component as IHavePropertyExtenders, _properties, attributes, editingContext);
                }
            }

            return PropertyWindowViewModelHelper.GetBrowsableProperties(_properties);
        }
    }
}
