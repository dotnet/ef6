// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Design;

    /// <summary>
    ///     This provider is wired to the NameableItem abstract DomainClass so that it can provide a mock
    ///     descriptor for DSL. When any property change occurs from the diagram, DSL asks the DomainClass
    ///     if there is a TypeDescriptionProvider attached to it. It then attempts to create the TypeDescriptor,
    ///     gets the properties exposed through the TypeDescriptor, and calls SetValue on the ElementTypeDescriptor
    ///     which sets the property directly on the DomainClass. This level of indirection is a result of TFS
    ///     Work Item #430446
    /// </summary>
    internal class NameableItemDescriptionProvider : ElementTypeDescriptionProvider
    {
        protected override ElementTypeDescriptor CreateTypeDescriptor(ICustomTypeDescriptor parent, ModelElement element)
        {
            var nameableItem = element as NameableItem;
            if (nameableItem != null)
            {
                return new NameableItemDescriptor(parent, nameableItem);
            }
            return base.CreateTypeDescriptor(parent, element);
        }
    }

    /// <summary>
    ///     The NameableItemDescriptor simply exposes the Name property since this is the only property that DSL
    ///     attempts to route through a PropertyDescriptor. DSL sets the property value on the
    ///     ElementPropertyDescriptor, which consequently sets it directly on the DomainClass.
    /// </summary>
    internal class NameableItemDescriptor : ElementTypeDescriptor
    {
        public NameableItemDescriptor(ICustomTypeDescriptor parent, NameableItem nameableItem)
            : base(parent, nameableItem)
        {
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var propertyCollection = new PropertyDescriptorCollection(null);

            var propertyInfo = ModelElement.Store.DomainDataDirectory.FindDomainProperty(NameableItem.NameDomainPropertyId);
            Debug.Assert(
                propertyInfo != null, "We should have found the NameableItem Name's DomainPropertyId to create the PropertyDescriptor");
            if (propertyInfo != null)
            {
                propertyCollection.Add(CreatePropertyDescriptor(ModelElement, propertyInfo, attributes));
            }

            return propertyCollection;
        }
    }
}
