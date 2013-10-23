// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This is a wrapper for property descriptors that were obtained through reflection
    ///     by calling TypeDescriptor.GetProperties.
    ///     The reasons for having this wrapper are:
    ///     - be able to dynamically determine whether a property is browsable;
    ///     - be able to dynamically determine whether a property is read-only;
    ///     - be able to dynamically determine whether a property can have ResetValue called on it;
    ///     - be able to recycle cached type converters associated to a property;
    ///     The wrapped property descriptor already looks for methods called ShouldSerializeXXX () and
    ///     ResetXXX() for a property named XXX. (see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconshouldpersistresetmethods.asp )
    ///     This wrapper in addition looks for IsReadOnlyXXX() and IsBrowsableXXX () methods.
    /// </summary>
    internal class ReflectedPropertyDescriptor : CustomPropertyDescriptor
    {
        private readonly PropertyDescriptor _reflectedPropDescriptor;
        private readonly MethodInfo _descriptionMethod;
        private readonly MethodInfo _isReadOnlyMethod;
        private readonly MethodInfo _isBrowsableMethod;
        private readonly MethodInfo _canResetMethod;
        private TypeConverter _converter;

        public ReflectedPropertyDescriptor(EditingContext editingContext, PropertyDescriptor reflectedPropDescriptor, object component)
            : base(
                editingContext, component, reflectedPropDescriptor.Name,
                PropertyWindowViewModelHelper.GetArrayFromCollection<Attribute>(reflectedPropDescriptor.Attributes))
        {
            _reflectedPropDescriptor = reflectedPropDescriptor;
            var propertyName = reflectedPropDescriptor.Name;
            _descriptionMethod = FindMethod(
                _reflectedPropDescriptor.ComponentType, "Description" + propertyName, Type.EmptyTypes, typeof(string), false);
            _isReadOnlyMethod = FindMethod(
                _reflectedPropDescriptor.ComponentType, "IsReadOnly" + propertyName, Type.EmptyTypes, typeof(bool), false);
            _isBrowsableMethod = FindMethod(
                _reflectedPropDescriptor.ComponentType, "IsBrowsable" + propertyName, Type.EmptyTypes, typeof(bool), false);
            _canResetMethod = FindMethod(
                _reflectedPropDescriptor.ComponentType, "CanReset" + propertyName, Type.EmptyTypes, typeof(bool), false);
        }

        public override string Description
        {
            get
            {
                if (_descriptionMethod != null)
                {
                    return (string)_descriptionMethod.Invoke(Component, null);
                }
                return _reflectedPropDescriptor.Description;
            }
        }

        public override Type ComponentType
        {
            get { return _reflectedPropDescriptor.ComponentType; }
        }

        public override bool IsReadOnly
        {
            get
            {
                return
                    _isReadOnlyMethod != null
                        ? (bool)_isReadOnlyMethod.Invoke(Component, null)
                        : _reflectedPropDescriptor.IsReadOnly;
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return
                    _isBrowsableMethod != null
                        ? (bool)_isBrowsableMethod.Invoke(Component, null)
                        : _reflectedPropDescriptor.IsBrowsable;
            }
        }

        public override Type PropertyType
        {
            get { return _reflectedPropDescriptor.PropertyType; }
        }

        public override TypeConverter Converter
        {
            get
            {
                if (_converter == null)
                {
                    _converter = _reflectedPropDescriptor.Converter;

                    // The instance of the converter returned above may have been obtained from
                    // a cache of converter objects that System.ComponentModel maintains, so verifying
                    // if the converter needs to be reinitialized before being reused.
                    // Note: This could also have been accomplished by controlling the instantiation of the 
                    // converter ourselves, but there may be converters that rely on the fact that they get cached. 
                    var resettableConverter = _converter as IResettableConverter;
                    if (resettableConverter != null)
                    {
                        resettableConverter.Reset();
                    }
                }
                return _converter;
            }
        }

        public override object GetEditor(Type editorBaseType)
        {
            return _reflectedPropDescriptor.GetEditor(editorBaseType);
        }

        public override bool CanResetValue(object component)
        {
            if (_canResetMethod != null)
            {
                return (bool)_canResetMethod.Invoke(Component, null);
            }
            return _reflectedPropDescriptor.CanResetValue(Component);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _reflectedPropDescriptor.ShouldSerializeValue(Component);
        }

        protected override object GetEFElementValue()
        {
            return _reflectedPropDescriptor.GetValue(Component);
        }

        protected override void SetEFElementValue(object value)
        {
            _reflectedPropDescriptor.SetValue(Component, value);
        }

        protected override void ResetEFElementValue()
        {
            _reflectedPropDescriptor.ResetValue(Component);
        }

        protected internal override string UndoString
        {
            get { return string.Format(CultureInfo.CurrentCulture, Resources.Tx_PropertyChangeUndoString, _reflectedPropDescriptor.Name); }
        }
    }
}
