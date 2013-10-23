// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    /// <summary>
    ///     class with definition of miscellaneous helper methods for the property window ViewModel
    /// </summary>
    internal static class PropertyWindowViewModelHelper
    {
        private static CommandProcessorContext _cpc;

        internal static CommandProcessorContext CreateCommandProcessorContext(EditingContext editingContext, string transactionName)
        {
            Debug.Assert(_cpc == null, "The common handler in CustomPropertyDescriptor should have removed the last one");

            _cpc = new CommandProcessorContext(editingContext, EfiTransactionOriginator.PropertyWindowOriginatorId, transactionName);
            return _cpc;
        }

        internal static CommandProcessorContext GetCommandProcessorContext()
        {
            Debug.Assert(_cpc != null, "The common handler in CustomPropertyDescriptor should have created one");
            return _cpc;
        }

        internal static void RemoveCommandProcessorContext()
        {
            _cpc = null;
        }

        internal static T[] GetArrayFromCollection<T>(ICollection collection)
        {
            return (T[])new ArrayList(collection).ToArray(typeof(T));
        }

        internal static IEnumerable<TSelectedObj> GetObjectsFromSelection<TSelectedObj>(object selection) where TSelectedObj : class
        {
            // single selection
            var selectedObj = selection as TSelectedObj;
            if (selectedObj != null)
            {
                yield return selectedObj;
            }
            else
            {
                // multi selection
                var selectedItems = selection as object[];
                Debug.Assert(selectedItems != null);
                if (selectedItems != null)
                {
                    foreach (var item in selectedItems)
                    {
                        selectedObj = item as TSelectedObj;
                        Debug.Assert(selectedObj != null);
                        if (selectedObj != null)
                        {
                            yield return selectedObj;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     filter the specified collection of properties and
        ///     return only those properties that are browsable
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        internal static PropertyDescriptorCollection GetBrowsableProperties(PropertyDescriptorCollection properties)
        {
            // the IsBrowsable verification is not done as part of the caching in GetProperties because 
            // the IsBrowsable state can change when the values of other properties change.
            var browsableProperties = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor propDescriptor in properties)
            {
                if (propDescriptor.IsBrowsable)
                {
                    browsableProperties.Add(propDescriptor);
                }
            }

            return browsableProperties;
        }

        internal static void AddReflectedProperties(
            PropertyDescriptorCollection properties, object component, Attribute[] attributes, EditingContext editingContext)
        {
            var props = TypeDescriptor.GetProperties(component, attributes, true);

            // determine if the component can provide defaults and/or help keyword for its contained properties
            var defaultsProvider = component as IPropertyDescriptorDefaultsProvider;

            foreach (PropertyDescriptor prop in props)
            {
                var wrappedPropDescriptor = prop;

                // if the component can provide a default value for this property, then get it at runtime,
                // create a DefaultValueAttribute, create a copy of the property descriptor, attach the
                // attribute to it, and use this new descriptor
                if (defaultsProvider != null)
                {
                    var defaultValue = defaultsProvider.GetDescriptorDefaultValue(prop.Name);

                    var defaultValueAttribute = new DefaultValueAttribute(defaultValue);
                    wrappedPropDescriptor = TypeDescriptor.CreateProperty(
                        prop.ComponentType, prop, new Attribute[] { defaultValueAttribute });
                }

                var reflectedPropertyDescriptor = new ReflectedPropertyDescriptor(editingContext, wrappedPropDescriptor, component);
                properties.Add(reflectedPropertyDescriptor);
            }
        }

        internal static void AddExtendedProperties(
            IHavePropertyExtenders component, PropertyDescriptorCollection properties, Attribute[] attributes, EditingContext editingContext)
        {
            if (component != null)
            {
                var propExtenders = component.GetPropertyExtenders();
                if (propExtenders != null)
                {
                    foreach (var extenderObj in propExtenders)
                    {
                        if (extenderObj != null)
                        {
                            AddReflectedProperties(properties, extenderObj, attributes, editingContext);
                        }
                    }
                }
            }
        }
    }
}
