// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     base class for a ICustomTypeDescriptor that describes the collection of properties of an EFAttribute
    /// </summary>
    /// <typeparam name="TEFAttribute"></typeparam>
    internal abstract class AttributeDescriptor<TEFAttribute> : ObjectDescriptor, ICustomTypeDescriptor, IHavePropertyExtenders
        where TEFAttribute : EFAttribute
    {
        private TEFAttribute _attribute;
        private PropertyDescriptorCollection _properties;
        private EditingContext _editingContext;

        // this is true if the property window is running inside VS.  This was necessary because there were some
        // tests that ran outside of VS, and some logic needed to only run inside VS.  Gross hack?  Perhaps.  
        // Pragmatic?  Definitely.  
        private bool _runningInVS = true;

        /// <summary>
        ///     This property exposes _attribute as strongly typed (as opposed to the base EFAttribute class)
        ///     for the convenience of the derived descriptors, so they don't have to cast the WrappedItem
        ///     to the appropriate type.
        /// </summary>
        internal TEFAttribute TypedEFAttribute
        {
            get { return _attribute; }
        }

        /// <summary>
        ///     implementation of ItemDescriptor
        /// </summary>

        #region InfoDescriptor Members
        [Browsable(false)]
        public override EFObject WrappedItem
        {
            get { return _attribute; }
        }

        [Browsable(false)]
        public override EditingContext EditingContext
        {
            get { return _editingContext; }
        }

        internal override void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            _attribute = obj as TEFAttribute;
            _editingContext = editingContext;
            _runningInVS = runningInVS;

            // now perform any initialization on derived types
            OnTypeDescriptorInitialize();
        }

        internal bool RunningInVS
        {
            get { return _runningInVS; }
        }

        #endregion

        IList<object> IHavePropertyExtenders.GetPropertyExtenders()
        {
            return GetPropertyExtenders();
        }

        internal virtual IList<object> GetPropertyExtenders()
        {
            return null;
        }

        /// <summary>
        ///     Overridden by derived classes to perform initialization after _attribute is initialized
        /// </summary>
        protected virtual void OnTypeDescriptorInitialize()
        {
        }

        /// <summary>
        ///     implementation of ICustomTypeDescriptor
        /// </summary>

        #region ICustomTypeDescriptor Members
        public virtual AttributeCollection GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        public virtual string GetClassName()
        {
            return typeof(TEFAttribute).Name;
        }

        public virtual string GetComponentName()
        {
            return TypedEFAttribute.XAttribute.Name.LocalName;
        }

        public virtual TypeConverter GetConverter()
        {
            return null;
        }

        public virtual EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public virtual PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            return null;
        }

        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        public virtual EventDescriptorCollection GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (_properties == null)
            {
                _properties = new PropertyDescriptorCollection(null);

                // get list of properties through reflection
                PropertyWindowViewModelHelper.AddReflectedProperties(_properties, this, attributes, _editingContext);

                // add properties from extender objects
                PropertyWindowViewModelHelper.AddExtendedProperties(this, _properties, attributes, _editingContext);
            }

            return PropertyWindowViewModelHelper.GetBrowsableProperties(_properties);
        }

        public virtual PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public virtual object GetPropertyOwner(PropertyDescriptor pd)
        {
            var efPropDesc = pd as CustomPropertyDescriptor;
            return (efPropDesc != null) ? efPropDesc.Component : this;
        }

        #endregion
    }
}
