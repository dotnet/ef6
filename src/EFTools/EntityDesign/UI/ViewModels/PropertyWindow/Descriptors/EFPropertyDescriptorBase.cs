// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.TypeEditors;

    internal class EFPropertyDescriptorBase<T> : EFPropertyBaseDescriptor<T>
        where T : Property
    {
        [LocDescription("PropertyWindow_Description_PropertyName")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Nullable")]
        [LocDescription("PropertyWindow_Description_Nullable")]
        [TypeConverter(typeof(BoolOrNoneTypeConverter))]
        public BoolOrNone Nullable
        {
            get { return TypedEFElement.Nullable.Value; }
            set
            {
                var existingValue = TypedEFElement.Nullable.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if BoolOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet = (BoolOrNone.NoneValue.Equals(value) ? null : value);
                var cmd = new ChangePropertyTypeNullableCommand(TypedEFElement, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableNullable()
        {
            return false;
        }

        internal virtual bool IsReadOnlyNullable()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DefaultValue")]
        [LocDescription("PropertyWindow_Description_DefaultValue")]
        [TypeConverter(typeof(StringOrNoneTypeConverter))]
        [Editor(typeof(NoneOptionListBoxTypeEditor), typeof(UITypeEditor))]
        public StringOrNone Default
        {
            get { return TypedEFElement.DefaultValue.Value; }
            set
            {
                var existingValue = TypedEFElement.DefaultValue.Value;
                NoneOptionListBoxTypeEditor.UpdateDefaultableValueIfValuesDiffer(existingValue, value, TypedEFElement.DefaultValue);
            }
        }

        internal virtual bool IsBrowsableDefault()
        {
            return false;
        }

        internal virtual bool IsReadOnlyDefault()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_ConcurrencyMode")]
        [TypeConverter(typeof(ConcurrencyModeConverter))]
        public string ConcurrencyMode
        {
            get { return TypedEFElement.ConcurrencyMode.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.ConcurrencyMode, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableConcurrencyMode()
        {
            return false;
        }

        internal virtual bool IsReadOnlyConcurrencyMode()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_MaxLength")]
        [TypeConverter(typeof(MaxLengthConverter))]
        public StringOrPrimitive<UInt32> MaxLength
        {
            get { return TypedEFElement.MaxLength.Value; }
            set
            {
                var existingValue = TypedEFElement.MaxLength.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if DefaultableValueUIntOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet =
                    (DefaultableValueUIntOrNone.NoneValue.Equals(value) ? null : value);
                var cmd = new ChangePropertyTypeLengthCommand(TypedEFElement, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableMaxLength()
        {
            return false;
        }

        internal virtual bool IsReadOnlyMaxLength()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_FixedLength")]
        [TypeConverter(typeof(BoolOrNoneTypeConverter))]
        public BoolOrNone FixedLength
        {
            get { return TypedEFElement.FixedLength.Value; }
            set
            {
                var existingValue = TypedEFElement.FixedLength.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if BoolOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet = (BoolOrNone.NoneValue.Equals(value) ? null : value);
                var cmd =
                    new UpdateDefaultableValueCommand<BoolOrNone>(TypedEFElement.FixedLength, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableFixedLength()
        {
            return false;
        }

        internal virtual bool IsReadOnlyFixedLength()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_Precision")]
        [TypeConverter(typeof(UIntOrNoneTypeConverter))]
        public StringOrPrimitive<UInt32> Precision
        {
            get { return TypedEFElement.Precision.Value; }
            set
            {
                var existingValue = TypedEFElement.Precision.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if DefaultableValueUIntOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet = (DefaultableValueUIntOrNone.NoneValue.Equals(value) ? null : value);
                var cmd = new ChangePropertyTypePrecisionCommand(TypedEFElement, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsablePrecision()
        {
            return false;
        }

        internal virtual bool IsReadOnlyPrecision()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_Scale")]
        [TypeConverter(typeof(UIntOrNoneTypeConverter))]
        public StringOrPrimitive<UInt32> Scale
        {
            get { return TypedEFElement.Scale.Value; }
            set
            {
                var existingValue = TypedEFElement.Scale.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if DefaultableValueUIntOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet = (DefaultableValueUIntOrNone.NoneValue.Equals(value) ? null : value);
                var cmd = new ChangePropertyTypeScaleCommand(TypedEFElement, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableScale()
        {
            return false;
        }

        internal virtual bool IsReadOnlyScale()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_Unicode")]
        [TypeConverter(typeof(BoolOrNoneTypeConverter))]
        public BoolOrNone Unicode
        {
            get { return TypedEFElement.Unicode.Value; }
            set
            {
                var existingValue = TypedEFElement.Unicode.Value;
                if (existingValue.Equals(value))
                {
                    return;
                }

                // if BoolOrNone.NoneValue.Equals(value) then remove the attribute by sending null
                var valueToSet = (BoolOrNone.NoneValue.Equals(value) ? null : value);
                var cmd =
                    new UpdateDefaultableValueCommand<BoolOrNone>(TypedEFElement.Unicode, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableUnicode()
        {
            return false;
        }

        internal virtual bool IsReadOnlyUnicode()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_Facets")]
        [LocDisplayName("PropertyWindow_DisplayName_Collation")]
        [DisplayName("Collation")]
        [TypeConverter(typeof(StringOrNoneTypeConverter))]
        [Editor(typeof(NoneOptionListBoxTypeEditor), typeof(UITypeEditor))]
        public StringOrNone Collation
        {
            get { return TypedEFElement.Collation.Value; }
            set
            {
                var existingValue = TypedEFElement.Collation.Value;
                NoneOptionListBoxTypeEditor.UpdateDefaultableValueIfValuesDiffer(existingValue, value, TypedEFElement.Collation);
            }
        }

        internal virtual bool IsBrowsableCollation()
        {
            return false;
        }

        internal virtual bool IsReadOnlyCollation()
        {
            return false;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Property";
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("Nullable"))
            {
                return TypedEFElement.Nullable.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Collation"))
            {
                return TypedEFElement.Collation.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("ConcurrencyMode"))
            {
                return TypedEFElement.ConcurrencyMode.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Default"))
            {
                return TypedEFElement.DefaultValue.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("FixedLength"))
            {
                return TypedEFElement.FixedLength.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("MaxLength"))
            {
                return TypedEFElement.MaxLength.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Precision"))
            {
                return TypedEFElement.Precision.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Scale"))
            {
                return TypedEFElement.Scale.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Unicode"))
            {
                return TypedEFElement.Unicode.DefaultValue;
            }

            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
