// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    // A decorator class to override the default behavior of CustomPropertyDescriptor class when a property value is updated.
    // CustomPropertyDescriptor will automatically close the transaction's undo-scope right when the update is done.
    // This class shift the responsibility of starting and ending the transaction's undo-scope to the LinkedDescriptorContextItem instance.
    internal class LinkedPropertyDescriptor : CustomPropertyDescriptor
    {
        private readonly CustomPropertyDescriptor _wrappedCustomPropertyDescriptor;
        private readonly LinkedDescriptorContextItem _contextItem;

        private readonly MethodInfo _setEFElementValueMethod;
        private readonly MethodInfo _resetEFElementValue;

        public LinkedPropertyDescriptor(CustomPropertyDescriptor propertyDescriptor, LinkedDescriptorContextItem contextItem)
            : base(propertyDescriptor.EditingContext, propertyDescriptor.Component, propertyDescriptor.Name
                , PropertyWindowViewModelHelper.GetArrayFromCollection<Attribute>(propertyDescriptor.Attributes))
        {
            _wrappedCustomPropertyDescriptor = propertyDescriptor;
            _contextItem = contextItem;

            // Since we need to call the protected methods but we still want the methods to be remained protected in other cases, use the reflector to do so.
            _setEFElementValueMethod = typeof(CustomPropertyDescriptor).GetMethod(
                "SetEFElementValue", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(_setEFElementValueMethod != null, "Type CustomPropertyDescriptor does not have 'SetEFElementValue' method.");

            _resetEFElementValue = typeof(CustomPropertyDescriptor).GetMethod(
                "ResetEFElementValue", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(_resetEFElementValue != null, "Type CustomPropertyDescriptor does not have 'ResetEFElementValue' method.");
        }

        #region CustomPropertyDescriptor overrides

        public override Type ComponentType
        {
            get { return _wrappedCustomPropertyDescriptor.ComponentType; }
        }

        public override bool IsReadOnly
        {
            get { return _wrappedCustomPropertyDescriptor.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return _wrappedCustomPropertyDescriptor.PropertyType; }
        }

        public override bool IsBrowsable
        {
            get { return _wrappedCustomPropertyDescriptor.IsBrowsable; }
        }

        public override bool CanResetValue(object component)
        {
            return _wrappedCustomPropertyDescriptor.CanResetValue(component);
        }

        public override void ResetValue(object component)
        {
            // reset the property value within a transaction context
            UpdatePropertyValue(ResetEFElementValue, UndoString);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _wrappedCustomPropertyDescriptor.ShouldSerializeValue(component);
        }

        public override object GetValue(object component)
        {
            return _wrappedCustomPropertyDescriptor.GetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            UpdatePropertyValue(delegate { SetEFElementValue(value); }, UndoString);
        }

        protected override object GetEFElementValue()
        {
            throw new NotImplementedException();
        }

        protected override void SetEFElementValue(object value)
        {
            // _setEFElementValueMethod should not be null here, don't need to Debug.Assert here since it is already done in the constructor. 
            if (_setEFElementValueMethod != null)
            {
                _setEFElementValueMethod.Invoke(_wrappedCustomPropertyDescriptor, new[] { value });
            }
        }

        protected override void ResetEFElementValue()
        {
            // _resetEFElementValueMethod should not be null here, don't need to Debug.Assert here since it is already done in the constructor. 
            if (_resetEFElementValue != null)
            {
                _resetEFElementValue.Invoke(_wrappedCustomPropertyDescriptor, null);
            }
        }

        protected internal override string UndoString
        {
            get { return _wrappedCustomPropertyDescriptor.UndoString; }
        }

        #endregion

        #region Helper Methods

        private delegate void UpdatePropertyValueCallback();

        /// <summary>
        ///     Update the property value within a shared transaction context.
        /// </summary>
        /// <param name="updateCallback"></param>
        private void UpdatePropertyValue(UpdatePropertyValueCallback updatePropertyValueCallback, string txName)
        {
            try
            {
                _contextItem.BeginPropertyValueUpdate(_wrappedCustomPropertyDescriptor.EditingContext, txName);
                updatePropertyValueCallback();
            }
            catch (Exception)
            {
                // We need to inform the context-item that an exception has been raised so that the undo-scope can be closed.
                _contextItem.OnPropertyValueUpdateException();
                throw;
            }
            finally
            {
                _contextItem.EndPropertyValueUpdate();
            }
        }

        #endregion
    }
}
