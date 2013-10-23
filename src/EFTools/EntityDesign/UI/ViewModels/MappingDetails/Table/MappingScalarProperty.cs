// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
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

    /// <summary>
    ///     Class that represents a scalar property.  This class has to be creatable without a ModelItem existing
    ///     since we want to be able to display every column in the mapped table, even if the column isn't mapped
    ///     yet.  So, this class has the ability to store s-side column information; and this is only used if
    ///     there isn't an associated ModelItem.
    /// </summary>
    [TreeGridDesignerRootBranch(typeof(ScalarPropertyBranch))]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ValueColumn), Order = 3)]
    internal class MappingScalarProperty : MappingEntityMappingRoot
    {
        // these are used to store 'column' (s-side property) information
        // for when we don't yet have a ScalarProperty to link to
        private string _columnName;
        private string _columnType;
        private bool _isKeyColumn;

        public MappingScalarProperty(EditingContext context, ScalarProperty scalarProperty, MappingEFElement parent)
            : base(context, scalarProperty, parent)
        {
            if (scalarProperty != null)
            {
                if (scalarProperty.ColumnName.Status == BindingStatus.Known)
                {
                    _columnName = scalarProperty.ColumnName.Target.LocalName.Value;
                }
                else
                {
                    _columnName = scalarProperty.ColumnName.RefName;
                }
            }

            _columnType = Resources.MappingDetails_UnknownColumnType;
        }

        internal ScalarProperty ScalarProperty
        {
            get
            {
                if (IsModelItemDeleted())
                {
                    // if the model item we are pointing to doesn't have an XLinq node, clear it out
                    ModelItem = null;

                    Debug.Assert(string.IsNullOrEmpty(_columnName) == false, "We are missing column name information");
                    Debug.Assert(MappingStorageEntityType != null, "We are missing our parent MappingStorageEntityType");
                    Debug.Assert(
                        MappingStorageEntityType != null && MappingStorageEntityType.StorageEntityType != null,
                        "Our parent MappingStorageEntityType is not mapped to a table");

                    if (string.IsNullOrEmpty(_columnName) == false
                        &&
                        MappingStorageEntityType != null
                        &&
                        MappingStorageEntityType.StorageEntityType != null)
                    {
                        // the underlying EFObject was deleted, probably because it got moved to a different 
                        // ETM, so go find it again
                        //
                        // see MappingCondition::Condition for more information
                        //
                        EntityType table = MappingStorageEntityType.StorageEntityType;

                        var tableColumn = table.GetFirstNamedChildByLocalName(_columnName) as Property;
                        Debug.Assert(tableColumn != null, "Failed looking up table column for ScalarProperty.");

                        if (tableColumn != null)
                        {
                            var spFound = ModelHelper.FindFragmentScalarProperty(
                                MappingConceptualEntityType.ConceptualEntityType,
                                tableColumn);

                            Debug.Assert(
                                ((spFound == null) || (spFound != null && spFound.XObject != null)),
                                "The found ScalarProperty has a null XObject pointer");

                            if (spFound != null
                                &&
                                spFound.XObject != null)
                            {
                                ModelItem = spFound;
                            }
                        }
                    }
                }

                var sp = ModelItem as ScalarProperty;
                if (sp != null)
                {
                    Debug.Assert(sp.ColumnName.Status == BindingStatus.Known, "Why are we mapping an unresolved scalar?");

                    if (_columnName == null)
                    {
                        // this MappingScalarProperty was created based on an existing ScalarProperty,
                        // store off this information in case we need to re-attach our ModelItem later
                        if (sp.ColumnName.Status == BindingStatus.Known)
                        {
                            _columnName = sp.ColumnName.Target.LocalName.Value;
                            _columnType = sp.ColumnName.Target.TypeName;
                        }
                        else
                        {
                            _columnName = sp.ColumnName.RefName;
                        }
                    }
                    else
                    {
                        // quick check, no reason why a non-null column name shouldn't match the sp
                        //
                        //  This assert isn't valid any longer when editing SSDL via the table designer.
                        //  Need to think if there is another assert here?
                        //
                        Debug.Assert(
                            string.Compare(_columnName, sp.ColumnName.RefName, StringComparison.CurrentCultureIgnoreCase) == 0,
                            "The column this scalar is pointing to has changed, this shouldn't happen");
                    }
                }

                return sp;
            }
        }

        internal override string Name
        {
            get { return ColumnUtils.BuildPropertyDisplay(ColumnName, ColumnType); }
        }

        internal string Value
        {
            get { return ColumnUtils.BuildPropertyDisplay(Property, PropertyType); }
        }

        internal string ColumnName
        {
            get
            {
                // if we don't have a backing SP, then return our cached name
                if (ScalarProperty == null)
                {
                    return _columnName;
                }
                else
                {
                    if (ScalarProperty.ColumnName.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.ColumnName.Target.LocalName.Value;
                    }
                    else
                    {
                        return ScalarProperty.ColumnName.RefName;
                    }
                }
            }
            set
            {
                if (ScalarProperty == null)
                {
                    _columnName = value;
                }
                else
                {
                    // you can't set the column into a scalar property, this is fixed for the row
                    throw new InvalidOperationException();
                }
            }
        }

        internal string ColumnType
        {
            get
            {
                // if we don't have a backing SP, then return our cached type
                if (ScalarProperty == null)
                {
                    return _columnType;
                }
                else
                {
                    if (ScalarProperty.ColumnName.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.ColumnName.Target.TypeName;
                    }
                    else
                    {
                        return Resources.MappingDetails_UnknownColumnType;
                    }
                }
            }
            set
            {
                if (ScalarProperty == null)
                {
                    _columnType = value;
                }
                else
                {
                    // you can't set the column type into a scalar property; this is
                    // a facet of the underlying Property, not the scalar property
                    throw new InvalidOperationException();
                }
            }
        }

        // This is only for dummy rows, so we know if we should show an icon with key
        internal bool IsKeyColumn
        {
            get
            {
                if (ScalarProperty == null
                    || ScalarProperty.ColumnName.Status != BindingStatus.Known)
                {
                    return _isKeyColumn;
                }

                return ScalarProperty.ColumnName.Target.IsKeyProperty;
            }
            set
            {
                if (ScalarProperty == null)
                {
                    _isKeyColumn = value;
                }
                else
                {
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
                    var sb = new StringBuilder();
                    foreach (var cp in ScalarProperty.GetParentComplexProperties())
                    {
                        if (cp.Name.Status == BindingStatus.Known)
                        {
                            sb.Append(cp.Name.Target.LocalName.Value);
                        }
                        else
                        {
                            sb.Append(cp.Name.RefName);
                        }
                        sb.Append(".");
                    }

                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        sb.Append(ScalarProperty.Name.Target.LocalName.Value);
                    }
                    else
                    {
                        sb.Append(ScalarProperty.Name.RefName);
                    }
                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        internal void ChangeScalarProperty(EditingContext context, List<Property> newPropertiesChain)
        {
            if (ModelItem != null)
            {
                var propertiesChain = ScalarProperty.GetMappedPropertiesList();
                var changeNeeded = false;
                // is it different than what we have already?
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
                }

                if (changeNeeded)
                {
                    Debug.Assert(ScalarProperty.ColumnName.Status == BindingStatus.Known, "Table column not found");
                    if (ScalarProperty.ColumnName.Status == BindingStatus.Known)
                    {
                        // delete old and create new ScalarProperty in one transaction
                        var cpc = new CommandProcessorContext(
                            context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                        var cmd1 = ScalarProperty.GetDeleteCommand();
                        var cmd2 = new CreateFragmentScalarPropertyTreeCommand(
                            MappingConceptualEntityType.ConceptualEntityType, newPropertiesChain, ScalarProperty.ColumnName.Target);
                        cmd2.PostInvokeEvent += (o, eventsArgs) =>
                            {
                                var sp = cmd2.ScalarProperty;
                                Debug.Assert(sp != null, "CreateFragmentScalarPropertyTreeCommand falied to create a ScalarProperty");
                                ModelItem = sp;
                            };

                        var cp = new CommandProcessor(cpc, cmd1, cmd2);
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
                var entityType = MappingConceptualEntityType.ConceptualEntityType;
                var properties = new List<Property>();

                // for TPT, show keys for the top-most base type
                if (entityType.HasResolvableBaseType)
                {
                    if (InheritanceMappingStrategy.TablePerType == ModelHelper.DetermineCurrentInheritanceStrategy(entityType))
                    {
                        // for TPT, show keys for the top-most base type
                        EntityType topMostBaseType = entityType.ResolvableTopMostBaseType;
                        properties.AddRange(topMostBaseType.ResolvableKeys);
                    }
                }

                // also show all properties of the current entity
                properties.AddRange(entityType.Properties());

                if (ScalarProperty != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (properties.Count == 0)
                {
                    if (ScalarProperty == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var prop in properties)
                    {
                        ColumnUtils.AddPropertyToListOfValues(lov, prop, null);
                    }
                }

                return lov;
            }
            else
            {
                Debug.Fail("Unsupported lov type was sent");
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
                ChangeScalarProperty(context, propertiesChain);
            }
        }

        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            var entityProperty = underlyingModelItem as Property;
            Debug.Assert(entityProperty != null, "entityProperty argument cannot be null");
            var properties = new List<Property>(1);
            properties.Add(entityProperty);
            CreateModelItem(cpc, context, properties);
        }

        /// <summary>
        ///     The mapping view model contains a MappingScalarProperty for every column in the table.  The user can clear out the
        ///     underlying scalar property, but that doesn’t remove or add the MappingScalarProperty.  We need the placeholder
        ///     in the view model to show the nodes in the Trid even if there isn’t a mapping.  Thus, we don't need to call
        ///     this.Parent.AddChild(this) since its already there.
        /// </summary>
        internal void CreateModelItem(CommandProcessorContext cpc, EditingContext context, List<Property> propertiesChain)
        {
            Debug.Assert(propertiesChain != null, "The propertiesChain cannot be null");
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(ScalarProperty == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingStorageEntityType.StorageEntityType != null, "The parent item isn't set up correctly");

            Debug.Assert(propertiesChain.Count > 0, "propertiesChain cannot be empty");

            Context = context;

            // local shortcuts
            EntityType entityType = MappingConceptualEntityType.ConceptualEntityType;
            EntityType table = MappingStorageEntityType.StorageEntityType;

            // find the s-side property based on the value stored in this.ColumnName
            var tableColumn = table.GetFirstNamedChildByLocalName(ColumnName) as Property;
            Debug.Assert(tableColumn != null, "Failed looking up table column for ScalarProperty.");
            if (tableColumn == null)
            {
                return;
            }

            try
            {
                // now make the change
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateScalarProperty);
                }
                var cmd = new CreateFragmentScalarPropertyTreeCommand(entityType, propertiesChain, tableColumn);
                cmd.PostInvokeEvent += (o, eventsArgs) =>
                    {
                        var sp = cmd.ScalarProperty;
                        Debug.Assert(sp != null, "CreateFragmentScalarPropertyTreeCommand failed to create ScalarProperty");

                        // fix up our view model (we don't have to add this to the parent's children collection
                        // because we created a placeholder row already for every column in the table)
                        ModelItem = sp;
                    };

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
        ///     The mapping view model contains a MappingScalarProperty for every column in the table.  The user can clear out the
        ///     underlying scalar property, but that doesn’t remove or add the MappingScalarProperty.  We need the placeholder
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
                var columnName = ColumnName;
                var columnType = ColumnType;

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
                _columnName = columnName;
                _columnType = columnType;
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
