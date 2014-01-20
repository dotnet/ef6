// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations
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

    [TreeGridDesignerRootBranch(typeof(EndScalarPropertyBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 3)]
    internal class MappingEndScalarProperty : MappingAssociationMappingRoot
    {
        // these are used to store 'property' (c-side key property) information
        // for when we don't yet have a ScalarProperty to link to
        private string _propertyName;
        private string _propertyType;

        public MappingEndScalarProperty(EditingContext context, ScalarProperty sp, MappingEFElement parent)
            : base(context, sp, parent)
        {
            _propertyType = Resources.MappingDetails_UnknownColumnType;
        }

        internal ScalarProperty ScalarProperty
        {
            get { return ModelItem as ScalarProperty; }
        }

        internal override string Name
        {
            get { return ColumnUtils.BuildPropertyDisplay(Property, PropertyType); }
        }

        internal string Value
        {
            get
            {
                if (MappingAssociationSet.AssociationSet.AssociationSetMapping == null)
                {
                    return Resources.Mapping_AssocMappingNoTable;
                }
                else
                {
                    return ColumnUtils.BuildPropertyDisplay(ColumnName, ColumnType);
                }
            }
        }

        internal string Property
        {
            get
            {
                // if we don't have a backing SP, then return our cached name
                if (ScalarProperty == null)
                {
                    return _propertyName;
                }
                else
                {
                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.Name.Target.LocalName.Value;
                    }
                    else
                    {
                        return ScalarProperty.Name.RefName;
                    }
                }
            }
            set
            {
                if (ScalarProperty == null)
                {
                    _propertyName = value;
                }
                else
                {
                    // you can't set the property into a scalar property, this is fixed for the row
                    throw new InvalidOperationException();
                }
            }
        }

        internal string PropertyType
        {
            get
            {
                // if we don't have a backing SP, then return our cached type
                if (ScalarProperty == null)
                {
                    return _propertyType;
                }
                else
                {
                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.Name.Target.TypeName;
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
                    _propertyType = value;
                }
                else
                {
                    // you can't set the property type into a scalar property; this is
                    // a facet of the underlying Property, not the scalar property
                    throw new InvalidOperationException();
                }
            }
        }

        internal string ColumnName
        {
            get
            {
                if (ScalarProperty != null)
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

                return string.Empty;
            }
            set
            {
                var newColumnName = value;

                if (ScalarProperty != null)
                {
                    // is it different than what we have already?
                    if (string.CompareOrdinal(ColumnName, newColumnName) != 0)
                    {
                        Debug.Assert(MappingAssociationSet != null, "Null MappingAssociationSet in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet != null, "Null MappingAssociationSet.AssociationSet in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet.AssociationSetMapping != null,
                            "Null MappingAssociationSet.AssociationSet.AssociationSetMapping in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet != null,
                            "Null MappingAssociationSet.AssociationSet.AssociationSetMapping.TableName in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target != null,
                            "Null MappingAssociationSet.AssociationSet.AssociationSetMapping.TableName.Target in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target.EntityType != null,
                            "Null MappingAssociationSet.AssociationSet.AssociationSetMapping.TableName.Target.EntityType in ColumnName setter");
                        Debug.Assert(
                            MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target.EntityType.Target != null,
                            "Null MappingAssociationSet.AssociationSet.AssociationSetMapping.TableName.Target.EntityType.Target in ColumnName setter");

                        if (MappingAssociationSet != null
                            && MappingAssociationSet.AssociationSet != null
                            && MappingAssociationSet.AssociationSet.AssociationSetMapping != null
                            && MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet != null
                            && MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target != null
                            && MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target.EntityType != null
                            && MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target.EntityType.Target != null)
                        {
                            var tableColumn =
                                MappingAssociationSet.AssociationSet.AssociationSetMapping.StoreEntitySet.Target.EntityType.Target
                                    .GetFirstNamedChildByLocalName(newColumnName) as Property;
                            Debug.Assert(tableColumn != null, "Could not find tableColumn property for newColumnName " + newColumnName);

                            if (tableColumn != null)
                            {
                                var cpc = new CommandProcessorContext(
                                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                                var cmd = new ChangeScalarPropertyCommand(ScalarProperty, null, tableColumn);
                                CommandProcessor.InvokeSingleCommand(cpc, cmd);
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
        }

        internal string ColumnType
        {
            get
            {
                if (ScalarProperty != null
                    &&
                    ScalarProperty.ColumnName.Status == BindingStatus.Known)
                {
                    return ScalarProperty.ColumnName.Target.TypeName;
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
                var table = MappingAssociationSet.GetTable();
                var properties = new List<Property>();
                if (table != null)
                {
                    properties.AddRange(table.Properties());
                }

                // we aren't filtering this list, show all columns of the table
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

        // <summary>
        //     The mapping view model contains a MappingEndScalarProperty for every key in each end.  The user can clear out the
        //     underlying scalar property, but that doesn't remove or add the MappingEndScalarProperty.  We need the placeholder
        //     in the view model to show the nodes in the Trid even if there isn't a mapping.  Thus, we don't need to call
        //     this.Parent.AddChild(this) since its already there.
        // </summary>
        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(underlyingModelItem != null, "The underlyingModelItem cannot be null");
            var tableColumn = underlyingModelItem as Property;
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(ScalarProperty == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(tableColumn != null, "The tableColumn cannot be null.");
            if (tableColumn == null)
            {
                return;
            }
            Debug.Assert(tableColumn.EntityType.EntityModel.IsCSDL == false, "tableColumn must be a Store-side Property");

            Context = context;

            // find the c-side property based on the passed in name
            var entityProperty = MappingAssociationSetEnd.ConceptualEntityType.GetFirstNamedChildByLocalName(Property) as Property;
            if (entityProperty == null)
            {
                // they might be trying to map a key from the base class
                EntityType topMostBaseType = MappingAssociationSetEnd.ConceptualEntityType.ResolvableTopMostBaseType;
                entityProperty = topMostBaseType.GetFirstNamedChildByLocalName(Property) as Property;
            }
            Debug.Assert(entityProperty != null, "Failed looking up entity property for ScalarProperty.");
            if (entityProperty == null)
            {
                return;
            }

            // create our context if we don't have one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateScalarProperty);
            }

            // create the right command
            CreateEndScalarPropertyCommand cmd = null;
            var end = MappingAssociationSetEnd.AssociationSetEnd.EndProperty;
            if (end == null)
            {
                // we don't have an end yet, this version will create an end as well as the scalar property
                cmd = new CreateEndScalarPropertyCommand(
                    MappingAssociationSet.AssociationSet.AssociationSetMapping, MappingAssociationSetEnd.AssociationSetEnd, entityProperty,
                    tableColumn);
            }
            else
            {
                cmd = new CreateEndScalarPropertyCommand(end, entityProperty, tableColumn);
            }

            // set up our post event to fix up the view model
            cmd.PostInvokeEvent += (o, eventArgs) =>
                {
                    var sp = cmd.ScalarProperty;
                    Debug.Assert(sp != null, "cmd failed to generate ScalarProperty");

                    // fix up our view model (we don't have to add this to the parent's children collection
                    // because we created a placeholder row already for every key in the entity)
                    ModelItem = sp;
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

        // <summary>
        //     The mapping view model contains a MappingEndScalarProperty for every key in each end.  The user can clear out the
        //     underlying scalar property, but that doesn't remove or add the MappingEndScalarProperty.  We need the placeholder
        //     in the view model to show the nodes in the Trid even if there isn't a mapping.  Thus, we don't need to call
        //     this.Parent.RemoveChild(this) as we want to leave the placeholder.
        // </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");
            if (IsModelItemDeleted() == false)
            {
                // since we are deleting the SP, we need to go back to our "base-less" mode
                // where the property info is returned as instance strings - cache off the data
                var property = Property;
                var propertyType = PropertyType;

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
                Property = property;
                PropertyType = propertyType;
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
