// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    // This class is used to change the property-descriptors that are passed in to Visual Studio. 
    internal class LinkedPropertyTypeDescriptor : ObjectDescriptor, ICustomTypeDescriptor
    {
        // The map between property descriptor and the object that contains the property described by property descriptor.
        private readonly Dictionary<PropertyDescriptor, Object> _propertyDescriptorToOwner =
            new Dictionary<PropertyDescriptor, Object>(new PropertyDescriptorEqualityComparer());

        private readonly ICustomTypeDescriptor _wrappedCustomTypeDescriptor;
        private PropertyDescriptorCollection _propertyDescriptorCollection; // List to cache property descriptors.

        // ContextItem that is shared by PropertyDescriptors; this enables bulk update operations to be grouped into a single undo scope.
        private readonly LinkedDescriptorContextItem _contextItem;

        internal LinkedPropertyTypeDescriptor(ICustomTypeDescriptor customTypeDescriptor, LinkedDescriptorContextItem contextItem)
        {
            _wrappedCustomTypeDescriptor = customTypeDescriptor;
            _contextItem = contextItem;
            _contextItem.RegisterDescriptor(this);
        }

        private ObjectDescriptor ObjectDescriptor
        {
            get { return _wrappedCustomTypeDescriptor as ObjectDescriptor; }
        }

        #region Override methods

        public override EFObject WrappedItem
        {
            get { return ObjectDescriptor.WrappedItem; }
        }

        public override EditingContext EditingContext
        {
            get { return ObjectDescriptor.EditingContext; }
        }

        internal override void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            ObjectDescriptor.Initialize(obj, editingContext, runningInVS);
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            return ObjectDescriptor.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }

        #endregion

        #region ICustomTypeDescriptor Implementation

        public AttributeCollection GetAttributes()
        {
            return _wrappedCustomTypeDescriptor.GetAttributes();
        }

        public string GetClassName()
        {
            return _wrappedCustomTypeDescriptor.GetClassName();
        }

        public string GetComponentName()
        {
            return _wrappedCustomTypeDescriptor.GetComponentName();
        }

        public TypeConverter GetConverter()
        {
            return _wrappedCustomTypeDescriptor.GetConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            return _wrappedCustomTypeDescriptor.GetDefaultEvent();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return _wrappedCustomTypeDescriptor.GetDefaultProperty();
        }

        public object GetEditor(Type editorBaseType)
        {
            return _wrappedCustomTypeDescriptor.GetEditor(editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return _wrappedCustomTypeDescriptor.GetEvents(attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return _wrappedCustomTypeDescriptor.GetEvents();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (_propertyDescriptorCollection == null)
            {
                var collection = _wrappedCustomTypeDescriptor.GetProperties(attributes);
                _propertyDescriptorCollection = new PropertyDescriptorCollection(null);
                foreach (PropertyDescriptor propertyDescriptor in collection)
                {
                    // Check if the property descriptor is ours.
                    var customPropertyDescriptor = propertyDescriptor as CustomPropertyDescriptor;
                    if (customPropertyDescriptor != null)
                    {
                        // Since we are changing the property descriptor type, we need to build propertyDescriptorToOwner map,
                        // so that the right value is returned when Visual-Studio calls GetPropertyOwner.
                        var linkedPropertyDescriptor = new LinkedPropertyDescriptor(customPropertyDescriptor, _contextItem);
                        var propertyOwner = _wrappedCustomTypeDescriptor.GetPropertyOwner(propertyDescriptor);
                        Debug.Assert(propertyOwner != null, "Could not find property owner for " + propertyDescriptor.Name);
                        if (propertyOwner != null)
                        {
                            _propertyDescriptorToOwner.Add(linkedPropertyDescriptor, propertyOwner);
                            _propertyDescriptorCollection.Add(linkedPropertyDescriptor);
                        }
                    }
                    else
                    {
                        _propertyDescriptorCollection.Add(propertyDescriptor);
                    }
                }
            }
            return _propertyDescriptorCollection;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        // This is called by VS to determine the instance to which the given property should be applied. 
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            if (pd != null)
            {
                Object val;
                if (_propertyDescriptorToOwner.TryGetValue(pd, out val))
                {
                    return val;
                }
            }
            return _wrappedCustomTypeDescriptor.GetPropertyOwner(pd);
        }

        #endregion
    }
}
