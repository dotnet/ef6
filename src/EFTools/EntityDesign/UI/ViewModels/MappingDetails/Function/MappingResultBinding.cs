// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

    [TreeGridDesignerRootBranch(typeof(ResultBindingBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 3)]
    internal class MappingResultBinding : MappingFunctionMappingRoot
    {
        public MappingResultBinding(EditingContext context, ResultBinding binding, MappingEFElement parent)
            : base(context, binding, parent)
        {
        }

        internal ResultBinding ResultBinding
        {
            get { return ModelItem as ResultBinding; }
        }

        internal override string Name
        {
            get
            {
                if (ResultBinding != null
                    && ResultBinding.ColumnName.Value != null)
                {
                    return ResultBinding.ColumnName.Value;
                }

                return string.Empty;
            }
        }

        internal string Value
        {
            get { return ColumnUtils.BuildPropertyDisplay(Property, PropertyType); }
        }

        internal string ColumnName
        {
            get
            {
                if (ResultBinding != null)
                {
                    return ResultBinding.ColumnName.Value;
                }

                return string.Empty;
            }
            set
            {
                var newColumnName = value;

                if (ResultBinding != null)
                {
                    // is it different than what we have already?
                    if (string.CompareOrdinal(ColumnName, newColumnName) != 0)
                    {
                        var cmd = new ChangeResultBindingCommand(ResultBinding, null, newColumnName);
                        var cp = new CommandProcessor(
                            Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeResultBinding, cmd);
                        cp.Invoke();
                    }
                }
                else
                {
                    // if we don't have a scalar property, there is nothing to set this into;
                    // create the scalar property first
                    throw new InvalidOperationException();
                }
            }
        }

        internal string Property
        {
            get
            {
                if (ResultBinding != null)
                {
                    if (ResultBinding.Name.Status == BindingStatus.Known)
                    {
                        return ResultBinding.Name.Target.LocalName.Value;
                    }
                    else
                    {
                        return ResultBinding.Name.RefName;
                    }
                }

                return string.Empty;
            }
        }

        internal void SetProperty(Property property)
        {
            if (ResultBinding != null)
            {
                // is it different than what we have already?
                if (ResultBinding.Name.Target != property)
                {
                    var cmd = new ChangeResultBindingCommand(ResultBinding, property, null);
                    var cp = new CommandProcessor(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeResultBinding, cmd);
                    cp.Invoke();
                }
            }
            else
            {
                // if we don't have a scalar property, there is nothing to set this into;
                // create the scalar property first
                throw new InvalidOperationException();
            }
        }

        internal string PropertyType
        {
            get
            {
                if (ResultBinding != null
                    &&
                    ResultBinding.Name.Status == BindingStatus.Known)
                {
                    return ResultBinding.Name.Target.TypeName;
                }

                return string.Empty;
            }
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            Debug.Assert(type == ListOfValuesCollection.ThirdColumn, "Unsupported lov type was sent");

            if (type == ListOfValuesCollection.ThirdColumn)
            {
                var properties = new List<Property>();

                var entityType = MappingFunctionEntityType.EntityType;
                var conceptualEntityType = entityType as ConceptualEntityType;

                if (conceptualEntityType != null)
                {
                    properties.AddRange(conceptualEntityType.SafeInheritedAndDeclaredProperties);
                }
                else
                {
                    properties.AddRange(entityType.Properties());
                }

                if (ResultBinding != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (properties.Count == 0)
                {
                    if (ResultBinding == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var prop in properties)
                    {
                        var displayName = ColumnUtils.BuildPropertyDisplay(prop.LocalName.Value, prop.TypeName);
                        lov.Add(new MappingLovEFElement(prop, displayName), displayName);
                    }
                }

                return lov;
            }

            return base.GetListOfValues(type);
        }

        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            // you need to use the version below that takes a string
            throw new NotImplementedException();
        }

        internal bool CreateModelItem(CommandProcessorContext cpc, EditingContext context, string columnName)
        {
            Debug.Assert(context != null, "context must not be null");
            Debug.Assert(ResultBinding == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingFunctionEntityType.EntityType != null, "The parent item isn't set up correctly");
            Debug.Assert(!string.IsNullOrEmpty(columnName), "columnName must not be empty or null");

            Context = context;

            // local shortcuts
            var entityType = MappingFunctionEntityType.EntityType as ConceptualEntityType;
            Debug.Assert(MappingFunctionEntityType.EntityType == null || entityType != null, "EntityType is not ConceptualEntityType");
            var function = MappingModificationFunctionMapping.Function;

            // default a new binding to be bound to the first property of the entity
            Property property = null;
            if (entityType.PropertyCount > 0)
            {
                // if the entity has locally declared properties, use those
                foreach (var val in entityType.Properties())
                {
                    property = val;
                    break;
                }
            }
            else
            {
                // if not, go the more expensive route and get the first property from its ancestors
                foreach (var val in entityType.SafeInheritedProperties)
                {
                    property = val;
                    break;
                }
            }

            if (property != null)
            {
                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateResultBinding);
                }

                // create the command
                var cmd = new CreateResultBindingCommand(
                    entityType, function, MappingModificationFunctionMapping.ModificationFunctionType,
                    property, columnName);

                // set up our post event to fix up the view model
                cmd.PostInvokeEvent += (o, eventsArgs) =>
                    {
                        var binding = cmd.ResultBinding;
                        Debug.Assert(binding != null, "CreateResultBindingCommand resulted in null ResultBinding");

                        // fix up our view model
                        ModelItem = binding;
                        Parent.AddChild(this);
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
                    Parent.RemoveChild(this);

                    throw;
                }
            }
            else
            {
                // could not find any properties to which to map result
                // so should return false to indicate that attempt to create
                // the model item failed
                return false;
            }

            return true;
        }

        // <summary>
        //     NOTE: We don't call this.Parent.RemoveChild(this) here because this is always called from the MappingEFElement.Delete() method
        //     which will remove this item from the parent.
        // </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");
            if (IsModelItemDeleted() == false)
            {
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteResultBinding);
                }

                // use the item's delete command
                var deleteCommand = ResultBinding.GetDeleteCommand();
                deleteCommand.PostInvokeEvent += (o, eventsArgs) => { ModelItem = null; };

                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
