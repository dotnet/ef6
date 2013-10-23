// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Resources;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     base class for a PropertyDescriptor that describes a property of an EFElement
    /// </summary>
    internal abstract class CustomPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        ///     the component that owns the property
        /// </summary>
        private readonly object _component;

        private readonly EditingContext _editingContext;

        protected CustomPropertyDescriptor(EditingContext editingContext, object component, string name, Attribute[] attrs)
            : base(name, attrs)
        {
            _component = component;
            _editingContext = editingContext;
        }

        internal object Component
        {
            get { return _component; }
        }

        internal EditingContext EditingContext
        {
            get { return _editingContext; }
        }

        #region PropertyDescriptor implementation

        public override Type ComponentType
        {
            get { return _component.GetType(); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(string); }
        }

        public override bool IsBrowsable
        {
            get { return true; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
            // reset the property value within a transaction context
            UpdatePropertyValue(ResetEFElementValue, UndoString);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return GetEFElementValue();
        }

        public override void SetValue(object component, object value)
        {
            // set the new property value within a transaction context
            UpdatePropertyValue(delegate { SetEFElementValue(value); }, UndoString);
        }

        #endregion

        protected internal abstract string UndoString { get; }

        private delegate void UpdatePropertyValueCallback();

        /// <summary>
        ///     Update the property value within a transaction context
        /// </summary>
        /// <param name="updateCallback"></param>
        private void UpdatePropertyValue(UpdatePropertyValueCallback updatePropertyValueCallback, string txName)
        {
            try
            {
                PropertyWindowViewModelHelper.CreateCommandProcessorContext(_editingContext, txName);
                updatePropertyValueCallback();
            }
            finally
            {
                PropertyWindowViewModelHelper.RemoveCommandProcessorContext();
            }
        }

        protected abstract object GetEFElementValue();

        protected virtual void SetEFElementValue(object value)
        {
            Debug.Fail(
                "EFPropertyDescriptor.SetEFElementValue should never be invoked. Either it should be overriden in the derived class, or IsReadOnly should return true.");
        }

        protected virtual void ResetEFElementValue()
        {
            Debug.Fail(
                "EFPropertyDescriptor.ResetEFElementValue should never be invoked. Either it should be overriden in the derived class, or CanResetValue should return false.");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    internal class CommonLocDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string name;

        /// <include file='doc\PropertyPages.uex' path='docs/doc[@for="LocDisplayNameAttribute.DisplayNameAttribute"]/*' />
        public CommonLocDisplayNameAttribute(string name)
        {
            this.name = name;
        }

        protected virtual ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }

        /// <include file='doc\PropertyPages.uex' path='docs/doc[@for="LocDisplayNameAttribute.DisplayName"]/*' />
        public override string DisplayName
        {
            get
            {
                var result = ResourceManager.GetString(name, CultureInfo.CurrentUICulture);
                if (result == null)
                {
                    Debug.Assert(false, "String resource '" + name + "' is missing");
                    result = name;
                }
                return result;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    internal class CommonLocDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public CommonLocDescriptionAttribute(string description)
            : base(description)
        {
        }

        protected virtual ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }

        public override string Description
        {
            get
            {
                if (!replaced)
                {
                    replaced = true;
                    var result = ResourceManager.GetString(base.Description, CultureInfo.CurrentUICulture);
                    if (result == null)
                    {
                        Debug.Assert(false, "String resource '" + base.Description + "' is missing");
                        result = base.Description;
                    }
                    DescriptionValue = result;
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    internal class CommonLocCategoryAttribute : CategoryAttribute
    {
        public CommonLocCategoryAttribute(string category)
            : base(category)
        {
        }

        protected virtual ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }

        protected override string GetLocalizedString(string value)
        {
            var result = ResourceManager.GetString(value, CultureInfo.CurrentUICulture);
            if (result == null)
            {
                Debug.Assert(false, "String resource '" + value + "' is missing");
                result = value;
            }
            return result;
        }
    }
}
