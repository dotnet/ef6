// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class PropertyColumn : BaseColumn
    {
        public PropertyColumn()
            : base(Resources.MappingDetails_Property)
        {
        }

        internal override object GetInPlaceEdit(object component, ref string alternateText)
        {
            EnsureTypeConverters(component as MappingEFElement);
            return base.GetInPlaceEdit(component, ref alternateText);
        }

        /// <summary>
        ///     Overriding this allows the list-of-values dropdowns to use
        ///     the converter to convert back from a string to an object (in our
        ///     case a MappingLovEFElement object)
        /// </summary>
        public override Type /* PropertyDescriptor */ PropertyType
        {
            get { return typeof(PropertyColumnConverter); }
        }

        public override object /* PropertyDescriptor */ GetValue(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;
            if (mfsp != null)
            {
                EnsureTypeConverters(mfsp);
                return new MappingLovEFElement(mfsp, mfsp.Value);
            }

            var mrb = component as MappingResultBinding;
            if (mrb != null)
            {
                EnsureTypeConverters(mrb);
                return new MappingLovEFElement(mrb, mrb.Value);
            }

            var mfi = component as MappingFunctionImport;
            if (mfi != null)
            {
                EnsureTypeConverters(mfi);
                return new MappingLovEFElement(mfi, mfi.FunctionName);
            }

            var elem = component as MappingEFElement;
            if (elem != null)
            {
                EnsureTypeConverters(elem);
                return new MappingLovEFElement(elem, elem.Name);
            }

            return MappingEFElement.LovBlankPlaceHolder;
        }

        internal override bool IsDeleteSupported(object component)
        {
            if (component is MappingAssociationSet
                || component is MappingFunctionScalarProperty
                || component is MappingResultBinding)
            {
                var mappingElement = component as MappingEFElement;
                Debug.Assert(mappingElement != null, "The component should be a MappingEFElement");
                if (mappingElement.ModelItem != null)
                {
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override void SetValue(object component, object value)
        {
            // the user clicked off of the drop-down without choosing a value
            if (value == null
                || MappingEFElement.LovBlankPlaceHolder == value)
            {
                return;
            }

            // if they picked on the "Empty" placeholder, ignore it
            if (MappingEFElement.LovEmptyPlaceHolder == value)
            {
                return;
            }

            var lovElement = value as MappingLovEFElement;
            var valueAsString = value as string;
            Debug.Assert(
                lovElement != null || valueAsString != null,
                "value is not a MappingLovEFElement nor a string. Actual type is " + value.GetType().FullName);
            if (lovElement == null
                && valueAsString == null)
            {
                return;
            }

            // the trid will sometimes send an empty string when the user drops down a list
            // and then clicks away; if this happens just leave
            if (string.IsNullOrEmpty(valueAsString))
            {
                return;
            }

            var mas = component as MappingAssociationSet;
            if (mas != null)
            {
                lovElement = mas.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.FirstColumn);
                if (lovElement == null)
                {
                    return;
                }

                if (MappingEFElement.LovDeletePlaceHolder == lovElement)
                {
                    // they selected the delete me row, so clear out the item
                    mas.Delete(null);
                    OnValueChanged(this, new ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs.DeleteItemArgs));
                }
                else if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                {
                    return;
                }
                else
                {
                    var et = lovElement.ModelElement as EntityType;
                    Debug.Assert(
                        et != null, "component is a MappingAssociationSet but value.ModelElement is of type " + value.GetType().FullName);

                    if (mas.AssociationSet.AssociationSetMapping == null)
                    {
                        // this row is being created via the creator node
                        mas.CreateModelItem(null, Host.Context, et);
                        OnValueChanged(this, new ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs.InsertItemArgs));
                    }
                    else
                    {
                        // they switched to a different table so delete the old
                        // underlying model item and create a new one
                        var cpc = new CommandProcessorContext(
                            Host.Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_UpdateMappingFragment);
                        mas.SwitchModelItem(cpc, Host.Context, et, true);
                        OnValueChanged(this, new ColumnValueChangedEventArgs(new TreeGridDesignerBranchChangedArgs()));
                    }
                }

                return;
            }

            var mfsp = component as MappingFunctionScalarProperty;
            if (mfsp != null)
            {
                lovElement = mfsp.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.ThirdColumn);
                if (lovElement == null)
                {
                    return;
                }

                if (MappingEFElement.LovDeletePlaceHolder == lovElement)
                {
                    // they selected the delete me row, so clear out the item
                    // don't delete the view model row since we want one for every parameter
                    mfsp.DeleteModelItem(null);
                }
                else if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                {
                    return;
                }
                else
                {
                    // Note: properties (even simple ScalarProperties that are not part of a ComplexProperty) are
                    //       added to lovElement.Object as a List<Property> whereas properties from the other end
                    //       of a NavigationProperty are added as a Property (see 
                    //       MappingFunctionScalarProperty.GetListOfValues()).
                    var propertiesChain = lovElement.Object as List<Property>;
                    if (propertiesChain != null)
                    {
                        // there is no NavProp involved - clear out any old one
                        mfsp.SetNavigationProperty(null);

                        // create or update scalar property
                        mfsp.CreateOrUpdateModelItem(Host.Context, propertiesChain);
                    }
                    else
                    {
                        var property = lovElement.ModelElement as Property;

                        Debug.Assert(
                            property != null,
                            "component is a " + component.GetType().Name + " but value.ModelElement is of type "
                            + lovElement.ModelElement.GetType().FullName);
                        if (property != null)
                        {
                            // if the element's display name consists of "NavProp.Property" then figure out what
                            // the navigation property is
                            if (!String.IsNullOrEmpty(lovElement.DisplayName))
                            {
                                NavigationProperty navProp = null;
                                var dotSeparatorIndex = lovElement.DisplayName.IndexOf(Symbol.NORMALIZED_NAME_SEPARATOR_FOR_DISPLAY);
                                if (dotSeparatorIndex == -1)
                                {
                                    Debug.Fail(
                                        "For MappingFunctionScalarProperty component, lovElement is of type Property. This should indicate that the property was"
                                        +
                                        " reached via a NavigationProperty but the DisplayName (" + lovElement.DisplayName
                                        + ") does not have the right format.");
                                }
                                else
                                {
                                    var navPropRefName = lovElement.DisplayName.Substring(0, dotSeparatorIndex);
                                    Debug.Assert(
                                        mfsp.MappingFunctionEntityType != null,
                                        "Trying to get the navigation property, but where is the mapping function entity type tied to this mapping function scalar property?");
                                    Debug.Assert(
                                        mfsp.MappingFunctionEntityType.EntityType != null,
                                        "Trying to get the navigation property, but where is the entity type represented by the mapping function entity type?");

                                    if (mfsp.MappingFunctionEntityType != null
                                        && mfsp.MappingFunctionEntityType.EntityType != null)
                                    {
                                        var cet = mfsp.MappingFunctionEntityType.EntityType as ConceptualEntityType;
                                        Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                                        navProp = ModelHelper.FindNavigationPropertyByName(cet, navPropRefName);
                                        Debug.Assert(
                                            navProp != null,
                                            ""
                                            + String.Format(
                                                CultureInfo.CurrentCulture,
                                                "Could not find the navigation property {0} in the entity type {1}", navPropRefName,
                                                mfsp.MappingFunctionEntityType.EntityType.ToPrettyString()));
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                mfsp.SetNavigationProperty(navProp);
                            }

                            // create or update the item dependent on whether it already exists in the model
                            var propChain = new List<Property>(1);
                            propChain.Add(property);
                            mfsp.CreateOrUpdateModelItem(Host.Context, propChain);
                        }
                    }
                }

                OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                return;
            }

            var mrb = component as MappingResultBinding;
            if (mrb != null)
            {
                lovElement = mrb.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.ThirdColumn);
                if (lovElement == null)
                {
                    return;
                }

                if (MappingEFElement.LovDeletePlaceHolder == lovElement)
                {
                    // they selected the delete me row, so clear out the item
                    mrb.Delete(null);
                    OnValueChanged(this, new ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs.DeleteItemArgs));
                }
                else if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                {
                    return;
                }
                else
                {
                    var prop = lovElement.ModelElement as Property;
                    Debug.Assert(
                        prop != null,
                        "component is a " + component.GetType().Name + " but value.ModelElement is of type " + value.GetType().FullName);
                    Debug.Assert(mrb.ModelItem != null, "MappingResultBinding should not have null ModelItem");
                    mrb.SetProperty(prop);
                }

                OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                return;
            }

            var mfi = component as MappingFunctionImport;
            if (mfi != null)
            {
                lovElement = mfi.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.FirstColumn);
                if (lovElement != null)
                {
                    var function = lovElement.ModelElement as Function;
                    if (function != null)
                    {
                        mfi.ChangeModelItem(Host.Context, function);
                        OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                    }
                }
                return;
            }
        }

        internal override TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;
            if (component is MappingAssociationSetEnd
                || component is MappingEndScalarProperty
                || component is MappingFunctionEntityType
                || component is MappingModificationFunctionMapping
                || component is MappingFunctionScalarProperties
                || component is MappingResultBindings
                || component is MappingFunctionImportScalarProperty)
            {
                return TreeGridDesignerValueSupportedStates.None;
            }
            else if (null != mfsp)
            {
                if (null != mfsp.StoreParameter
                    && Parameter.InOutMode.Out == mfsp.StoreParameter.InOut)
                {
                    // do not support mapping to Out parameters
                    return TreeGridDesignerValueSupportedStates.None;
                }
            }

            return base.GetValueSupported(component);
        }

        internal override void EnsureTypeConverters(MappingEFElement element)
        {
            if (_converter == null
                || _currentElement != element)
            {
                // create initial type converter
                _currentElement = element;
                _converter = new PropertyColumnConverter();
            }
        }
    }

    internal class PropertyColumnConverter : BaseColumnConverter<PropertyColumn>
    {
        protected override void PopulateMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }

            Debug.Assert(_context != null, "Should have a context for the PopulateMapping call.");

            PopulateMappingForSelectedObject(_context.PropertyDescriptor as PropertyColumn);
        }

        protected override void PopulateMappingForSelectedObject(PropertyColumn selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null
                && selectedObject.Element != null)
            {
                if (selectedObject.Element is MappingAssociationSet
                    || selectedObject.Element is MappingFunctionImport)
                {
                    var lov = selectedObject.Element.GetListOfValues(ListOfValuesCollection.FirstColumn);
                    foreach (var key in lov.Keys)
                    {
                        AddMapping(key, lov[key]);
                    }
                    return;
                }

                if (selectedObject.Element is MappingFunctionScalarProperty
                    || selectedObject.Element is MappingResultBinding)
                {
                    var lov = selectedObject.Element.GetListOfValues(ListOfValuesCollection.ThirdColumn);
                    foreach (var key in lov.Keys)
                    {
                        AddMapping(key, lov[key]);
                    }
                    return;
                }
            }
        }
    }
}
