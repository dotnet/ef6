// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;

    /// <summary>
    ///     base class for a ICustomTypeDescriptor that describes the collection of properties of an EFElement
    /// </summary>
    /// <typeparam name="TEFElement"></typeparam>
    internal abstract class ElementDescriptor<TEFElement> : ObjectDescriptor, ICustomTypeDescriptor, IHavePropertyExtenders
        where TEFElement : EFElement
    {
        private TEFElement _element;
        private PropertyDescriptorCollection _properties;
        private EditingContext _editingContext;

        // this is true if the property window is running inside VS.  This was necessary because there were some
        // tests that ran outside of VS, and some logic needed to only run inside VS.  Gross hack?  Perhaps.  
        // Pragmatic?  Definitely.  
        private bool _runningInVS = true;

        /// <summary>
        ///     base implementation of the "name" property which is valid for most
        ///     of the EFElement derived types.
        /// </summary>
        [CommonLocCategory("PropertyWindow_Category_General")]
        [CommonLocDisplayName("PropertyWindow_DisplayName_Name")]
        [MergableProperty(false)]
        public virtual string Name
        {
            get
            {
                var efNameable = _element as EFNameableItem;
                if (efNameable != null)
                {
                    return efNameable.LocalName.Value;
                }
                return null;
            }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();

                Command c = _element.Artifact.ModelManager.CreateRenameCommand(_element as EFNameableItem, value, true);
                var cp = new CommandProcessor(cpc, c);
                cp.Invoke();
            }
        }

        internal virtual bool IsBrowsableName()
        {
            return _element is EFNameableItem;
        }

        internal virtual bool IsReadOnlyName()
        {
            return false;
        }

        /// <summary>
        ///     This property exposes _element as strongly typed (as opposed to the base EFElement class)
        ///     for the convenience of the derived descriptors, so they don't have to cast the WrappedItem
        ///     to the appropriate type.
        /// </summary>
        internal TEFElement TypedEFElement
        {
            get { return _element; }
        }

        /// <summary>
        ///     implementation of ObjectDescriptor
        /// </summary>

        #region ObjectDescriptor Members
        [Browsable(false)]
        public override EFObject WrappedItem
        {
            get { return _element; }
        }

        [Browsable(false)]
        public override EditingContext EditingContext
        {
            get { return _editingContext; }
        }

        internal override void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            _element = obj as TEFElement;
            _editingContext = editingContext;
            _runningInVS = runningInVS;

            // now perform any initialization on derived types
            OnTypeDescriptorInitialize();
        }

        #endregion

        IList<object> IHavePropertyExtenders.GetPropertyExtenders()
        {
            return GetPropertyExtenders();
        }

        /// <summary>
        ///     Overridden by derived classes to return a list of Property Extenders
        /// </summary>
        internal virtual IList<object> GetPropertyExtenders()
        {
            return null;
        }

        /// <summary>
        ///     Overridden by derived classes to perform initialization after _element is initialized
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
            return typeof(TEFElement).Name;
        }

        public virtual string GetComponentName()
        {
            return Name;
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
            // set the "name" property as the property that should be selected by default in the property window
            var nameProp = GetProperties().Find("Name", false);
            if (nameProp != null
                && nameProp.IsBrowsable)
            {
                return nameProp;
            }
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

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            // we are comparing method names here so we have to be strict...
            if (propertyDescriptorMethodName.Equals("Name"))
            {
                var efNameable = _element as EFNameableItem;
                if (efNameable != null)
                {
                    return efNameable.LocalName.DefaultValue;
                }
            }
            return null;
        }

        internal bool RunningInVS
        {
            get { return _runningInVS; }
        }
    }
}
