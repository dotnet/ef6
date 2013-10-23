// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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

    [TreeGridDesignerRootBranch(typeof(ModificationFunctionBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    internal class MappingModificationFunctionMapping : MappingFunctionMappingRoot
    {
        private ModificationFunctionType _functionType;
        private MappingFunctionScalarProperties _properties;
        private MappingResultBindings _resultBindings;

        public MappingModificationFunctionMapping(
            EditingContext context, ModificationFunction functionMapping, MappingEFElement parent, ModificationFunctionType functionType)
            : base(context, functionMapping, parent)
        {
            if (functionMapping == null)
            {
                _functionType = functionType;
            }
            else
            {
                _functionType = functionMapping.FunctionType;
                _properties = new MappingFunctionScalarProperties(context, functionMapping, this);
                _resultBindings = new MappingResultBindings(context, functionMapping, this);
            }
        }

        public MappingModificationFunctionMapping(EditingContext context, ModificationFunction functionMapping, MappingEFElement parent)
            : base(context, functionMapping, parent)
        {
            if (functionMapping != null)
            {
                _functionType = functionMapping.FunctionType;
                _properties = new MappingFunctionScalarProperties(context, functionMapping, this);
                _resultBindings = new MappingResultBindings(context, functionMapping, this);
            }
        }

        internal ModificationFunction ModificationFunction
        {
            get { return ModelItem as ModificationFunction; }
        }

        internal Function Function
        {
            get
            {
                if (ModificationFunction == null)
                {
                    return null;
                }
                else
                {
                    return ModificationFunction.FunctionName.Target;
                }
            }
        }

        internal ModificationFunctionType ModificationFunctionType
        {
            get { return _functionType; }
            set { _functionType = value; }
        }

        internal MappingFunctionScalarProperties MappingFunctionScalarProperties
        {
            get { return _properties; }
        }

        internal MappingResultBindings MappingResultBindings
        {
            get { return _resultBindings; }
        }

        internal override string Name
        {
            get
            {
                if (_functionType == ModificationFunctionType.Insert)
                {
                    if (Function == null)
                    {
                        return Resources.MappingDetails_InsertFunction_CreatorNode;
                    }
                    else
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.MappingDetails_InsertFunction_Display,
                            Function.LocalName.Value);
                    }
                }
                else if (_functionType == ModificationFunctionType.Update)
                {
                    if (Function == null)
                    {
                        return Resources.MappingDetails_UpdateFunction_CreatorNode;
                    }
                    else
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.MappingDetails_UpdateFunction_Display,
                            Function.LocalName.Value);
                    }
                }
                else if (_functionType == ModificationFunctionType.Delete)
                {
                    if (Function == null)
                    {
                        return Resources.MappingDetails_DeleteFunction_CreatorNode;
                    }
                    else
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.MappingDetails_DeleteFunction_Display,
                            Function.LocalName.Value);
                    }
                }

                return string.Empty;
            }
        }

        internal Parameter RowsAffectedParameter
        {
            get
            {
                if (null != ModificationFunction
                    &&
                    null != ModificationFunction.RowsAffectedParameter)
                {
                    return ModificationFunction.RowsAffectedParameter.Target;
                }

                return null;
            }

            set
            {
                Debug.Assert(ModificationFunction != null, "null ModificationFunction");

                if (null != ModificationFunction)
                {
                    var cmd = new SetRowsAffectedParameterCommand(ModificationFunction, value);
                    var cp = new CommandProcessor(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty, cmd);
                    cp.Invoke();
                }
            }
        }

        protected override void LoadChildrenCollection()
        {
            if (Function != null)
            {
                if (_properties != null)
                {
                    _children.Add(_properties);
                }

                if (_functionType != ModificationFunctionType.Delete
                    &&
                    _resultBindings != null)
                {
                    _children.Add(_resultBindings);
                }
            }
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            Debug.Assert(type == ListOfValuesCollection.FirstColumn, "Unsupported lov type was sent");

            if (type == ListOfValuesCollection.FirstColumn)
            {
                StorageEntityModel storageModel = null;
                if (Function != null)
                {
                    storageModel = Function.EntityModel;
                }
                else
                {
                    // this is a creator node, so get the list from the artifact
                    var entity = MappingFunctionEntityType.ModelItem as EntityType;
                    storageModel = entity.Artifact.StorageModel();
                }

                if (Function != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                var functions = new List<Function>();
                functions.AddRange(storageModel.Functions());
                if (functions.Count == 0)
                {
                    if (Function == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var func in functions)
                    {
                        // adding the Function to the list
                        lov.Add(new MappingLovEFElement(func, func.DisplayName), func.DisplayName);
                    }
                }

                return lov;
            }

            return base.GetListOfValues(type);
        }

        /// <summary>
        ///     The parent item for the function mapping view model always has 3 children; insert, update and delete items.  If there isn’t
        ///     a function mapped for any of these, then there is still a view model item since we want to display the ‘creator node’ text.
        ///     Calling this.Parent.AddChild(this) here would make the parent think it had a new child instead of updating the existing one -
        ///     so it is correct to _not_ call it here.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(context != null, "null context");

            Debug.Assert(Function == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingFunctionEntityType.EntityType != null, "The parent item isn't set up correctly");

            Debug.Assert(underlyingModelItem != null, "null underlyingModelItem");

            var function = underlyingModelItem as Function;
            Debug.Assert(
                function != null, "underlyingModelItem must be of type Function, actual type = " + underlyingModelItem.GetType().FullName);
            Debug.Assert(!function.EntityModel.IsCSDL, "The function must be in the SSDL");

            Context = context;

            // create a context if we weren't passed one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateFunctionMapping);
            }

            // create the commands
            var cmd = new CreateFunctionMappingCommand(MappingFunctionEntityType.EntityType, function, null, _functionType);
            // set up our post event to fix up the view model
            cmd.PostInvokeEvent += (o, eventsArgs) =>
                {
                    var mf = cmd.ModificationFunction;
                    Debug.Assert(mf != null, "null ModificationFunction");

                    // fix up our view model
                    ModelItem = mf;
                    // The parent item for the function mapping view model always has 3 children; insert, update and delete items.  If there isn’t 
                    // a function mapped for any of these, then there is still a view model item since we want to display the ‘creator node’ text.
                    // Calling this.Parent.AddChild(this) here would make the parent think it had a new child instead of updating the existing one - 
                    // so it is correct to _not_ call it here.
                };

            var cmd2 = new DelegateCommand(
                () =>
                    {
                        var mf = ModificationFunction;
                        Debug.Assert(
                            mf != null,
                            "Null ModificationFunction when trying to create view-model dummy nodes in MappingModificationFunctionMapping.CreateModelItem()");

                        if (mf != null)
                        {
                            //set up _properties and _resultBindings here as they are dummy ViewModel nodes
                            // (i.e. don't correspond to any underlying ModelItem)
                            _properties = new MappingFunctionScalarProperties(context, mf, this);
                            _resultBindings = new MappingResultBindings(context, mf, this);
                            _properties.Parent.AddChild(_properties);
                            _resultBindings.Parent.AddChild(_resultBindings);

                            // now ensure _properties scalar properties children have been calculated
                            // (this creates scalar properties with just the column info 
                            // since this ModificationFunction has not yet been mapped)
                            _properties.LoadScalarProperties();

                            // now try and do some match ups between the function and the entity
                            var mappedEntityType = MappingFunctionEntityType.EntityType as ConceptualEntityType;

                            Debug.Assert(
                                MappingFunctionEntityType.EntityType == null || mappedEntityType != null,
                                "EntityType is not ConceptualEntityType");

                            if (mappedEntityType != null)
                            {
                                foreach (var mfsp in _properties.ScalarProperties)
                                {
                                    // Try to do some auto-matching of the sproc's parameters to the EntityType's properties.
                                    // Search for a property in the mapped EntityType's inheritance hierarchy that matches the
                                    // parameter's name. First search this EntityType (both its scalar and complex properties),
                                    // then search its parents scalar and complex properties and so on up the hierarchy
                                    var propNameToSearchFor = mfsp.StoreParameter.LocalName.Value;
                                    var propList = new List<Property>();
                                    var entityTypeToSearch = mappedEntityType;
                                    // reset this back to the mapped EntityType each time through the loop
                                    while (entityTypeToSearch != null
                                           && false
                                           == ModelHelper.FindScalarPropertyPathByLocalName(
                                               entityTypeToSearch, propNameToSearchFor, out propList))
                                    {
                                        if (entityTypeToSearch.BaseType == null)
                                        {
                                            // safety code - this should not happen but will prevent an infinite loop if it does
                                            entityTypeToSearch = null;
                                        }
                                        else
                                        {
                                            entityTypeToSearch = entityTypeToSearch.BaseType.Target;
                                        }
                                    }

                                    // if propList is still empty that means we did not find a match - so leave the parameter unmapped
                                    if (propList.Count > 0)
                                    {
                                        mfsp.CreateModelItem(cpc, _context, propList);
                                    }
                                }
                            }
                        }
                    });

            try
            {
                // now make the change
                var cp = new CommandProcessor(cpc);
                cp.EnqueueCommand(cmd);
                cp.EnqueueCommand(cmd2);
                cp.Invoke();
            }
            catch
            {
                ModelItem = null;
                ClearChildren();

                throw;
            }
        }

        /// <summary>
        ///     The parent item for the function mapping view model always has 3 children; insert, update and delete.  If there isn’t
        ///     a function mapped for any of these, then there is still a view model item since we want to display the ‘creator node’ text.
        ///     Thus, we don't call this.Parent.RemoveChild(this).
        /// </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");
            if (IsModelItemDeleted() == false)
            {
                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteFunctionMapping);
                }

                // use the item's delete command
                var deleteCommand = ModificationFunction.GetDeleteCommand(MappingFunctionEntityType.EntityType, Function, _functionType);
                deleteCommand.PostInvokeEvent += (o, eventsArgs) =>
                    {
                        ModelItem = null;
                        ClearChildren();
                    };

                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }

        private void ClearChildren()
        {
            if (_children != null)
            {
                _children.Clear();
            }

            if (_properties != null)
            {
                _properties.Dispose();
                _properties = null;
            }

            if (_resultBindings != null)
            {
                _resultBindings.Dispose();
                _resultBindings = null;
            }
        }
    }
}
