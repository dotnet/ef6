// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    // <summary>
    //     Abstract base class for all Mapping XyzColumn classes.
    // </summary>
    internal abstract class BaseColumn : TreeGridDesignerColumnDescriptor
    {
        internal TypeConverter _converter;
        internal MappingEFElement _currentElement; // cached element, used to know whether we need to recreate the type converters

        protected BaseColumn(string name)
            : base(name)
        {
        }

        protected override void OnValueChanged(object component, EventArgs e)
        {
            base.OnValueChanged(component, e);

            var columnArgs = e as ColumnValueChangedEventArgs;
            if (columnArgs != null)
            {
                var column = component as BaseColumn;

                var mappingDetailsWindow = column.Host as MappingDetailsWindow;
                Debug.Assert(mappingDetailsWindow != null, "MappingWindow is null");
                if (mappingDetailsWindow != null)
                {
                    // check if branch modifications are required
                    if (columnArgs.Args != null)
                    {
                        var treeItemInfo = mappingDetailsWindow.TreeControl.SelectedItemInfo;
                        var branch = treeItemInfo.Branch as ITreeGridDesignerBranch;
                        if (branch != null)
                        {
                            // need to set Row and Column values here
                            columnArgs.Args.Row = treeItemInfo.Row;
                            columnArgs.Args.Column = treeItemInfo.Column;
                            branch.OnColumnValueChanged(columnArgs.Args);
                            if (!columnArgs.Args.DeletingItem)
                            {
                                // expand added or changed branch here
                                mappingDetailsWindow.TreeControl.ExpandRecurse(mappingDetailsWindow.TreeControl.CurrentIndex, 0);
                            }
                        }
                    }

                    // calling this should null out the converter mappings and display an updated drop down
                    // whenever a descriptor value has changed.
                    var resettableConverter = _converter as IResettableConverter;
                    if (resettableConverter != null)
                    {
                        resettableConverter.Reset();
                    }

                    // refreshes the property window
                    mappingDetailsWindow.UpdateSelection();
                }
            }
        }

        internal override void Delete(object component)
        {
            if (IsDeleteSupported(component))
            {
                SetValue(component, MappingEFElement.LovDeletePlaceHolder);
            }
        }

        internal abstract bool IsDeleteSupported(object component);

        internal MappingEFElement Element
        {
            get { return _currentElement; }
        }

        public override TypeConverter /* PropertyDescriptor */ Converter
        {
            get { return _converter != null ? _converter : base.Converter; }
        }

        internal abstract void EnsureTypeConverters(MappingEFElement element);

        public override Type /* PropertyDescriptor */ ComponentType
        {
            get { return typeof(MappingLovEFElement); }
        }
    }

    internal abstract class BaseColumnConverter<T> : DynamicListConverter<MappingLovEFElement, T>
        where T : BaseColumn
    {
        // call EnsureTypeConverters() to ensure that we are resetting
        // the _currentElement
        protected override void InitializeMapping(ITypeDescriptorContext context)
        {
            Debug.Assert(context != null, "Null context");
            if (context != null)
            {
                var mappingElement = context.Instance as MappingEFElement;
                var propertyDescriptor = context.PropertyDescriptor as BaseColumn;
                if (mappingElement != null
                    && propertyDescriptor != null)
                {
                    propertyDescriptor.EnsureTypeConverters(mappingElement);
                }
            }

            base.InitializeMapping(context);
        }

        // editing using keyboard can pass in a string - allow this
        public override bool /* TypeConverter */ CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        // editing using keyboard can pass in a string - just return it unaltered
        public override object /* TypeConverter */ ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // if context is null we are passing in a string (possibly from dropdown) via keyboard entry
            var stringValue = value as string;
            if (context == null
                && stringValue != null)
            {
                return stringValue;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    internal class ColumnValueChangedEventArgs : EventArgs
    {
        internal TreeGridDesignerBranchChangedArgs Args { get; set; }

        internal ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs args)
        {
            Args = args;
        }

        internal static ColumnValueChangedEventArgs Default
        {
            get { return new ColumnValueChangedEventArgs(null); }
        }
    }
}
