// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [TreeGridDesignerRootBranch(typeof(ParameterBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 3)]
    [TreeGridDesignerColumn(typeof(UseOriginalValueColumn), Order = 4)]
    [TreeGridDesignerColumn(typeof(RowsAffectedParameterColumn), Order = 5)]
    internal class MappingFunctionScalarProperty : MappingFunctionMappingRoot
    {
        // these are used to store parameterinformation for when we don't yet have a ScalarProperty to link to
        private Parameter _storeParameter; // stores the S-side parameter to which this refers

        // used to store a navigation property that points to the actual property. We store this here because
        // we also need it for CreateModelItem
        private NavigationProperty _pointingNavProperty;

        public MappingFunctionScalarProperty(EditingContext context, FunctionScalarProperty scalarProperty, MappingEFElement parent)
            : base(context, scalarProperty, parent)
        {
        }

        internal FunctionScalarProperty ScalarProperty
        {
            get
            {
                var sp = ModelItem as FunctionScalarProperty;
                if (sp != null)
                {
                    Debug.Assert(sp.ParameterName.Status == BindingStatus.Known, "Why are we mapping an unresolved scalar?");
                    if (_storeParameter == null)
                    {
                        // this MappingFunctionScalarProperty was created based on an existing ScalarProperty,
                        // store off this information in case we need to re-attach our ModelItem later
                        if (sp.ParameterName.Status == BindingStatus.Known)
                        {
                            _storeParameter = sp.ParameterName.Target;
                        }
                    }
                    else
                    {
                        // quick check, no reason why a non-null parameter name shouldn't match the sp
                        Debug.Assert(
                            string.Compare(
                                _storeParameter.LocalName.Value, sp.ParameterName.RefName, StringComparison.CurrentCultureIgnoreCase) == 0,
                            "The ParameterName this scalar is pointing to has changed, this shouldn't happen. StoreParameterName = "
                            + _storeParameter.LocalName.Value + ", Scalar Property ParameterName =" + sp.ParameterName.RefName);
                    }
                }

                return sp;
            }
        }

        internal override string Name
        {
            get { return ColumnUtils.BuildPropertyDisplay(StoreParameter.LocalName.Value, StoreParameter.Type.Value); }
        }

        internal string Value
        {
            get { return ColumnUtils.BuildPropertyDisplay(Property, PropertyType); }
        }

        internal bool UseOriginalValue
        {
            get
            {
                if (ScalarProperty != null)
                {
                    return ScalarProperty.Version.Value == ModelConstants.FunctionScalarPropertyVersionOriginal;
                }

                return false;
            }
            set
            {
                if (ScalarProperty != null)
                {
                    var version = string.Empty;
                    if (value)
                    {
                        version = ModelConstants.FunctionScalarPropertyVersionOriginal;
                    }
                    else
                    {
                        version = ModelConstants.FunctionScalarPropertyVersionCurrent;
                    }

                    var cmd = new ChangeFunctionScalarPropertyCommand(ScalarProperty, version);
                    var cp = new CommandProcessor(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty, cmd);
                    cp.Invoke();
                }
                else
                {
                    // you can't set the version until we have a valid ModelItem
                    throw new InvalidOperationException();
                }
            }
        }

        internal Parameter StoreParameter
        {
            get
            {
                if (ScalarProperty == null)
                {
                    return _storeParameter;
                }
                else
                {
                    if (ScalarProperty.ParameterName.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.ParameterName.Target;
                    }
                    else
                    {
                        //TODO error!!
                        throw new InvalidOperationException();
                    }
                }
            }

            set
            {
                if (ScalarProperty == null)
                {
                    _storeParameter = value;
                }
                else
                {
                    // you can't set the parameter into a scalar property, this is fixed for the row
                    throw new InvalidOperationException();
                }
            }
        }

        internal string Property
        {
            get
            {
                if (ScalarProperty != null)
                {
                    if (ScalarProperty.ModificationFunction != null)
                    {
                        return ScalarProperty.Name.RefName;
                    }
                    else if (ScalarProperty.AssociationEnd != null)
                    {
                        var navProp = ModelHelper.FindNavigationPropertyForFunctionScalarProperty(ScalarProperty);
                        // navProp can be null if the NavProp has been deleted
                        if (navProp != null)
                        {
                            return string.Format(
                                CultureInfo.CurrentCulture, "{0}.{1}",
                                navProp.LocalName.Value,
                                ScalarProperty.Name.RefName);
                        }
                    }
                    else if (ScalarProperty.FunctionComplexProperty != null)
                    {
                        var name = ScalarProperty.Name.RefName;
                        for (var fcp = ScalarProperty.FunctionComplexProperty;
                             fcp != null;
                             fcp = (fcp.Parent as FunctionComplexProperty))
                        {
                            name = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", fcp.Name.RefName, name);
                        }

                        return name;
                    }
                }

                return string.Empty;
            }
        }

        internal void SetNavigationProperty(NavigationProperty navProp)
        {
            _pointingNavProperty = navProp;
        }

        internal void ChangeFunctionScalarProperty(EditingContext context, List<Property> newPropertiesChain)
        {
            if (ScalarProperty == null)
            {
                // if we don't have a scalar property, there is nothing to set this into;
                // create the scalar property first
                throw new InvalidOperationException();
            }
            else
            {
                // is the propertiesChain different from what we have already?
                var propertiesChain = ScalarProperty.GetMappedPropertiesList();
                var changeNeeded = false;
                if (propertiesChain.Count != newPropertiesChain.Count)
                {
                    changeNeeded = true;
                }
                else
                {
                    for (var i = 0; i < propertiesChain.Count; i++)
                    {
                        if (propertiesChain[i] != newPropertiesChain[i])
                        {
                            changeNeeded = true;
                            break;
                        }
                    }

                    // if no change needed yet, check NavProp as well
                    if (changeNeeded == false)
                    {
                        // if this property is pointed to by a navigation property, then check if the 
                        // new navigation property is different from the old one
                        if (ScalarProperty.AssociationEnd != null)
                        {
                            var currentNavProp = ModelHelper.FindNavigationPropertyForFunctionScalarProperty(ScalarProperty);
                            // currentNavProp can be null if the NavProp has been deleted
                            changeNeeded = (currentNavProp == null ? true : (currentNavProp != _pointingNavProperty));
                        }
                        else
                        {
                            // the previous property was not pointed to by a navigation property but the new one is
                            if (_pointingNavProperty != null)
                            {
                                changeNeeded = true;
                            }
                        }
                    }
                }

                if (changeNeeded)
                {
                    // delete old and create new FunctionScalarProperty in one transaction - this takes care of
                    // removing any old ComplexProperty or AssociationEnd parent nodes as necessary
                    var cpc = new CommandProcessorContext(
                        context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                    var version = (ScalarProperty.Version == null ? null : ScalarProperty.Version.Value);
                    // Version is used only for Update ModificationFunctions
                    var cmd = new ChangeFunctionScalarPropertyCommand(
                        ScalarProperty, newPropertiesChain, _pointingNavProperty, StoreParameter, version);
                    cmd.PostInvokeEvent += (o, eventsArgs) =>
                        {
                            var fsp = cmd.FunctionScalarProperty;
                            Debug.Assert(fsp != null, "ChangeFunctionScalarPropertyCommand failed to create a FunctionScalarProperty");
                            ModelItem = fsp;
                        };

                    var cp = new CommandProcessor(cpc, cmd);
                    try
                    {
                        cp.Invoke();
                    }
                    catch
                    {
                        ModelItem = null;

                        throw;
                    }
                }
            }
        }

        internal string PropertyType
        {
            get
            {
                if (ScalarProperty != null
                    &&
                    ScalarProperty.Name.Status == BindingStatus.Known)
                {
                    return ScalarProperty.Name.Target.TypeName;
                }

                return string.Empty;
            }
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            if (type == ListOfValuesCollection.ThirdColumn)
            {
                var entityType = MappingFunctionEntityType.EntityType;
                var cet = entityType as ConceptualEntityType;

                Debug.Assert(entityType == null || cet != null, "EntityType is not ConceptualEntityType");

                var propsFromSelf = new List<Property>();
                var propsFromNav = new Dictionary<NavigationProperty, HashSet<Property>>();

                // show keys for the top-most base type
                if (cet.HasResolvableBaseType)
                {
                    propsFromSelf.AddRange(entityType.ResolvableKeys);
                }

                // show all properties of the entity
                propsFromSelf.AddRange(cet.SafeInheritedAndDeclaredProperties);

                // bug 568863: we need to show any "inherited" navigation properties as well
                foreach (var selfOrBaseType in cet.SafeSelfAndBaseTypes)
                {
                    // add properties for every type referenced by a navigation property
                    foreach (var nav in selfOrBaseType.NavigationProperties())
                    {
                        if (nav.FromRole.Status == BindingStatus.Known
                            && nav.FromRole.Target.Type.Status == BindingStatus.Known
                            && nav.ToRole.Status == BindingStatus.Known
                            && nav.ToRole.Target.Type.Status == BindingStatus.Known
                            && nav.ToRole.Target.Multiplicity.Value != ModelConstants.Multiplicity_Many)
                        {
                            ConceptualEntityType other = null;
                            if (nav.FromRole.Target.Type.Target == selfOrBaseType)
                            {
                                other = nav.ToRole.Target.Type.Target as ConceptualEntityType;
                            }
                            else
                            {
                                other = nav.FromRole.Target.Type.Target as ConceptualEntityType;
                            }

                            if (!propsFromNav.ContainsKey(nav))
                            {
                                propsFromNav[nav] = new HashSet<Property>();
                            }

                            // bug 568863, only include keys from reference types
                            foreach (var key in other.ResolvableTopMostBaseType.ResolvableKeys)
                            {
                                if (!propsFromNav[nav].Contains(key))
                                {
                                    propsFromNav[nav].Add(key);
                                }
                            }
                        }
                    }
                }

                if (ScalarProperty != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (propsFromSelf.Count == 0
                    && propsFromNav.Count == 0)
                {
                    if (ScalarProperty == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    var displayName = String.Empty;

                    // add those remaining in our list
                    // Note: properties (even simple ScalarProperties that are not part of a ComplexProperty) are
                    //       added to the lov as a List<Property> whereas properties from the other end of a
                    //       NavigationProperty are added as a Property. This allows us to tell them apart (see
                    //       PropertyColumn.SetValue())
                    foreach (var prop in propsFromSelf)
                    {
                        ColumnUtils.AddPropertyToListOfValues(lov, prop, null);
                    }

                    var propsFromNavEnum = propsFromNav.GetEnumerator();
                    while (propsFromNavEnum.MoveNext())
                    {
                        foreach (var prop in propsFromNavEnum.Current.Value)
                        {
                            displayName = string.Format(
                                CultureInfo.CurrentCulture, "{0}.{1}",
                                propsFromNavEnum.Current.Key.LocalName.Value,
                                prop.LocalName.Value);
                            displayName = ColumnUtils.BuildPropertyDisplay(displayName, prop.TypeName);

                            lov.Add(new MappingLovEFElement(prop, displayName), displayName);
                        }
                    }
                }

                return lov;
            }
            else
            {
                Debug.Fail("Unsupported lov type (" + type.ToString() + ") was sent");
            }

            return base.GetListOfValues(type);
        }

        internal void CreateOrUpdateModelItem(EditingContext context, List<Property> propertiesChain)
        {
            if (ModelItem == null)
            {
                CreateModelItem(null, context, propertiesChain);
            }
            else
            {
                ChangeFunctionScalarProperty(context, propertiesChain);
            }
        }

        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(underlyingModelItem != null, "underlyingModelItem argument cannot be null");
            if (underlyingModelItem != null)
            {
                var entityProperty = underlyingModelItem as Property;
                Debug.Assert(
                    entityProperty != null,
                    "underlyingModelItem argument was of type " + underlyingModelItem.GetType().FullName + ", should be Property");
                if (entityProperty != null)
                {
                    var properties = new List<Property>(1);
                    properties.Add(entityProperty);
                    CreateModelItem(cpc, context, properties);
                }
            }
        }

        /// <summary>
        ///     The mapping view model contains a MappingFunctionScalarProperty for every parameter in the function.  The user can clear out the
        ///     underlying scalar property, but that doesn’t remove or add the MappingFunctionScalarProperty.  We need the placeholder
        ///     in the view model to show the nodes in the Trid even if there isn’t a mapping.  Thus, we don't need to call
        ///     this.Parent.AddChild(this) since its already there.
        /// </summary>
        internal void CreateModelItem(CommandProcessorContext cpc, EditingContext context, List<Property> propertiesChain)
        {
            Debug.Assert(propertiesChain != null, "The propertiesChain cannot be null");
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(ScalarProperty == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingModificationFunctionMapping.Function != null, "The parent item isn't set up correctly");

            if (propertiesChain == null
                || context == null
                || ScalarProperty != null
                || MappingModificationFunctionMapping == null)
            {
                return;
            }

            Debug.Assert(propertiesChain.Count > 0, "propertiesChain cannot be empty");
            if (propertiesChain.Count <= 0)
            {
                return;
            }

            Context = context;

            var mf = MappingModificationFunctionMapping.ModificationFunction;
            if (null == mf)
            {
                Debug.Fail("this.MappingModificationFunctionMapping.ModificationFunction is null");
                return;
            }

            // use the stored Parameter
            var parameter = StoreParameter;
            if (parameter == null)
            {
                return;
            }

            // create a context if we weren't passed one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateScalarProperty);
            }

            // create the FunctionScalarProperty command (including any intermediate ComplexProperty's or AssociationEnd's)
            var version = (MappingModificationFunctionMapping.ModificationFunctionType == ModificationFunctionType.Update
                               ? ModelConstants.FunctionScalarPropertyVersionCurrent
                               : null);
            var cmd =
                new CreateFunctionScalarPropertyTreeCommand(mf, propertiesChain, _pointingNavProperty, parameter, version);

            // set up our post event to fix up the view model
            cmd.PostInvokeEvent += (o, eventsArgs) =>
                {
                    var fsp = cmd.FunctionScalarProperty;
                    Debug.Assert(fsp != null, "CreateFunctionScalarPropertyTreeCommand did not create a FunctionScalarProperty");

                    // fix up our view model (we don't have to add this to the parent's children collection
                    // because we created a placeholder row already for every parameter in the function)
                    ModelItem = fsp;
                };

            try
            {
                // now make the change
                var cp = new CommandProcessor(cpc, cmd);
                cp.Invoke();
            }
            catch
            {
                ModelItem = null;
                throw;
            }
        }

        /// <summary>
        ///     The mapping view model contains a MappingFunctionScalarProperty for every parameter in the function.  The user can clear out the
        ///     underlying scalar property, but that doesn’t remove or add the MappingFunctionScalarProperty.  We need the placeholder
        ///     in the view model to show the nodes in the Trid even if there isn’t a mapping.  Thus, we don't need to call
        ///     this.Parent.RemoveChild(this) as we want to leave the placeholder.
        /// </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");

            if (IsModelItemDeleted() == false)
            {
                // since we are deleting the SP, we need to go back to our "base-less" mode
                // where the column info is returned as instance strings - cache off the data
                var storeParam = StoreParameter;

                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteScalarProperty);
                }

                // use the item's delete command
                var deleteCommand = ScalarProperty.GetDeleteCommand();
                deleteCommand.PostInvokeEvent += (o, eventsArgs) => { ModelItem = null; };

                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);

                // restore the data so the display is correct
                _storeParameter = storeParam;
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
