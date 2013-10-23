// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
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

    /// <summary>
    ///     This class represents a table that has been mapped to an entity.
    /// </summary>
    [TreeGridDesignerRootBranch(typeof(TableBranch))]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 1)]
    internal class MappingStorageEntityType : MappingEntityMappingRoot
    {
        private IList<MappingCondition> _conditions;
        private readonly MappingColumnMappings _columnMappings;

        public MappingStorageEntityType(EditingContext context, EntityType storageEntityType, MappingEFElement parent)
            : base(context, storageEntityType, parent)
        {
            _columnMappings = new MappingColumnMappings(context, storageEntityType, this);
        }

        internal StorageEntityType StorageEntityType
        {
            get { return ModelItem as StorageEntityType; }
        }

        internal override string Name
        {
            get
            {
                if (ModelItem == null)
                {
                    return string.Empty;
                }
                else
                {
                    var et = ModelItem as EntityType;
                    Debug.Assert(et != null, "ModelItem is of wrong type " + ModelItem.GetType().FullName);

                    return string.Format(
                        CultureInfo.CurrentCulture, Resources.MappingDetailsViewModel_StorageEntityTypeName, et.LocalName.Value);
                }
            }
        }

        internal MappingColumnMappings ColumnMappings
        {
            get { return _columnMappings; }
        }

        internal IList<MappingCondition> Conditions
        {
            get
            {
                _conditions = new List<MappingCondition>();

                if (StorageEntityType != null)
                {
                    // get the table's entity set
                    var ses = StorageEntityType.EntitySet as StorageEntitySet;
                    if (ses != null)
                    {
                        // get all of its fragments
                        foreach (var frag in ses.MappingFragments)
                        {
                            // make sure that this fragment is for the C-Entity we are referencing
                            var etm = frag.Parent as EntityTypeMapping;
                            Debug.Assert(
                                etm != null,
                                "fragment's parent is not an EntityTypeMapping, instead of type " + frag.Parent.GetType().FullName);

                            if (etm.TypeName.Bindings[0].Status == BindingStatus.Known
                                &&
                                etm.TypeName.Bindings[0].Target == MappingConceptualEntityType.ConceptualEntityType)
                            {
                                // add the conditions to our view model
                                foreach (var cond in frag.Conditions())
                                {
                                    var mcond = (MappingCondition)ModelToMappingModelXRef.GetNewOrExisting(_context, cond, this);
                                    _conditions.Add(mcond);
                                }
                            }
                        }
                    }
                }

                return _conditions;
            }
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var child in Conditions)
            {
                _children.Add(child);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingCondition;
            Debug.Assert(child != null, "Unknown child being deleted");
            if (child != null)
            {
                _children.Remove(child);
                return;
            }

            base.OnChildDeleted(melem);
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            if (type == ListOfValuesCollection.FirstColumn)
            {
                BaseEntityModel storageModel = null;
                if (ModelItem != null)
                {
                    var table = ModelItem as EntityType;
                    storageModel = table.EntityModel as StorageEntityModel;
                }
                else
                {
                    // this is a creator node, so get the list from the artifact
                    var entity = MappingConceptualEntityType.ModelItem as EntityType;
                    storageModel = entity.Artifact.StorageModel();
                }

                var entities = new List<EntityType>();
                entities.AddRange(storageModel.EntityTypes());

                // filter the list down to those tables that we aren't already mapping
                foreach (var child in MappingConceptualEntityType.Children)
                {
                    var mset = child as MappingStorageEntityType;
                    Debug.Assert(mset != null, "expected child of type MappingStorageEntityType, got type " + child.GetType().FullName);
                    if (mset.StorageEntityType != null)
                    {
                        entities.Remove(mset.StorageEntityType);
                    }
                }

                if (StorageEntityType != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (entities.Count == 0)
                {
                    if (StorageEntityType == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var entityType in entities)
                    {
                        // adding the EntityType to the list
                        lov.Add(new MappingLovEFElement(entityType, entityType.DisplayName), entityType.DisplayName);
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

        /// <summary>
        ///     When a new table is mapped, we also set up the MappingColumnMappings view model item.  This is a placeholder that
        ///     contains the scalar property mappings.  It is separated from the other "children", the conditions, simply because of
        ///     UI requirements that Conditions be UI peers of a "Column Mappings" node in the Trid.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(StorageEntityType == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingConceptualEntityType.ConceptualEntityType != null, "The parent item isn't set up correctly");
            Debug.Assert(underlyingModelItem != null, "The underlyingModelItem cannot be null");
            var storeEntityType = underlyingModelItem as EntityType;
            Debug.Assert(
                storeEntityType != null,
                "underlyingModelItem must be of type EntityType, actual type = " + underlyingModelItem.GetType().FullName);
            Debug.Assert(storeEntityType.EntityModel.IsCSDL == false, "The storageEntityType must not be a CSDL EntityType");

            Context = context;
            ColumnMappings.Context = context;

            // create a context if we weren't passed one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateMappingFragment);
            }

            // create the MappingFragment - if we already have a default EntityTypeMapping then just add
            // the MappingFragment to that mapping, otherwise if we already have an IsTypeOf
            // EntityTypeMapping then add the MappingFragment to that, otherwise create an IsTypeOf
            // EntityTypeMapping and add the MappingFragment to that
            var cet = MappingConceptualEntityType.ConceptualEntityType;
            var defaultEtm = ModelHelper.FindEntityTypeMapping(cpc, cet, EntityTypeMappingKind.Default, false);
            var etmKind = (defaultEtm == null ? EntityTypeMappingKind.IsTypeOf : EntityTypeMappingKind.Default);
            var cmd = new CreateMappingFragmentCommand(cet, storeEntityType, etmKind);

            // add post-invoke event to fix up our view model
            cmd.PostInvokeEvent += (o, eventsArgs) =>
                {
                    // fix up our view model
                    ModelItem = storeEntityType;
                    Parent.AddChild(this);

                    // assign the table to our container node as well
                    ColumnMappings.ModelItem = storeEntityType;

                    // now try and do some match ups between the entity and the table
                    var mappingStrategy = ModelHelper.DetermineCurrentInheritanceStrategy(cet);
                    var topMostBaseType = cet.ResolvableTopMostBaseType;
                    foreach (var child in ColumnMappings.Children)
                    {
                        var msp = child as MappingScalarProperty;
                        if (msp != null)
                        {
                            List<Property> properties;
                            if (ModelHelper.FindScalarPropertyPathByLocalName(cet, msp.ColumnName, out properties))
                            {
                                msp.CreateModelItem(cpc, _context, properties);
                            }
                            else if (InheritanceMappingStrategy.TablePerType == mappingStrategy
                                     &&
                                     ModelHelper.FindScalarPropertyPathByLocalName(topMostBaseType, msp.ColumnName, out properties))
                            {
                                msp.CreateModelItem(cpc, _context, properties);
                            }
                        }
                    }
                };

            try
            {
                // now update the model
                var cp = new CommandProcessor(cpc);
                cp.EnqueueCommand(cmd);
                cp.Invoke();
            }
            catch
            {
                ModelItem = null;
                ColumnMappings.ModelItem = null;
                Parent.RemoveChild(this);
                throw;
            }
        }

        /// <summary>
        ///     NOTE: We don't call this.Parent.RemoveChild(this) because this is always called from the MappingEFElement.Delete() method
        ///     which will remove this item from the parent.
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
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteMappingFragment);
                }

                var fragment = ModelHelper.FindMappingFragment(
                    cpc, MappingConceptualEntityType.ConceptualEntityType, StorageEntityType, false);
                Debug.Assert(fragment != null, "could not find MappingFragment for StorageEntityType " + StorageEntityType.ToPrettyString());
                if (fragment != null)
                {
                    // use the item's delete command
                    var deleteCommand = fragment.GetDeleteCommand();
                    deleteCommand.PostInvokeEvent += (o, eventsArgs) =>
                        {
                            ModelItem = null;
                            ColumnMappings.ModelItem = null;
                        };

                    DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);
                }
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
            ColumnMappings.ModelItem = null;
        }
    }
}
