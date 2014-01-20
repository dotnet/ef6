// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [TreeGridDesignerRootBranch(typeof(AssociationSetBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    internal class MappingAssociationSet : MappingAssociationMappingRoot
    {
        private IList<MappingAssociationSetEnd> _ends;

        public MappingAssociationSet(EditingContext context, AssociationSet assocSet, MappingEFElement parent)
            : base(context, assocSet, parent)
        {
        }

        internal AssociationSet AssociationSet
        {
            get { return ModelItem as AssociationSet; }
        }

        internal StorageEntityType StorageEntityType
        {
            get
            {
                var entitySet = AssociationSet.AssociationSetMapping.StoreEntitySet.Target;
                if (entitySet != null)
                {
                    var set = entitySet.EntityType.Target as StorageEntityType;
                    Debug.Assert(entitySet.EntityType.Target != null ? set != null : true, "EntityType is not StorageEntityType");
                    return set;
                }

                return null;
            }
        }

        internal override string Name
        {
            get
            {
                var table = GetTable();
                if (table != null)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture, Resources.MappingDetailsViewModel_StorageEntityTypeName, table.LocalName.Value);
                }
                else
                {
                    return Resources.MappingDetails_TableCreatorNode;
                }
            }
        }

        internal IList<MappingAssociationSetEnd> AssociationSetEnds
        {
            get
            {
                _ends = new List<MappingAssociationSetEnd>();

                if (AssociationSet != null)
                {
                    foreach (var end in AssociationSet.AssociationSetEnds())
                    {
                        var mend = (MappingAssociationSetEnd)ModelToMappingModelXRef.GetNewOrExisting(_context, end, this);
                        _ends.Add(mend);
                    }
                }

                return _ends;
            }
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var child in AssociationSetEnds)
            {
                _children.Add(child);
            }
        }

        internal EntityType GetTable()
        {
            if (AssociationSet != null)
            {
                Debug.Assert(AssociationSet.AssociationSetMappings.Count <= 1, "We assume that each AssociationSet will just have one ASM");

                var asm = AssociationSet.AssociationSetMapping;
                if (asm != null
                    &&
                    asm.StoreEntitySet.Status == BindingStatus.Known)
                {
                    var ses = asm.StoreEntitySet.Target as StorageEntitySet;
                    if (ses != null
                        &&
                        ses.EntityType.Status == BindingStatus.Known)
                    {
                        return ses.EntityType.Target;
                    }
                }
            }

            return null;
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            if (type == ListOfValuesCollection.FirstColumn)
            {
                BaseEntityModel storageModel = null;
                if (AssociationSet != null
                    &&
                    AssociationSet.AssociationSetMapping != null)
                {
                    var table = GetTable();
                    storageModel = table.EntityModel as StorageEntityModel;
                }
                else
                {
                    // this is a creator node, so get the list from the artifact
                    var assoc = MappingAssociation.Association;
                    storageModel = assoc.Artifact.StorageModel();
                }

                if (AssociationSet != null
                    &&
                    AssociationSet.AssociationSetMapping != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (storageModel.EntityTypeCount == 0)
                {
                    if (AssociationSet == null
                        ||
                        AssociationSet.AssociationSetMapping == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var entityType in storageModel.EntityTypes())
                    {
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

        // <summary>
        //     NOTE: The association set mapping view model doesn't keep a reference to the mapping model item. Instead, it
        //     keeps it to the AssociationSet and then it can find the AssociationSetMapping as an anti-dep.  We don't need to clear
        //     or set the ModelItem property.
        // </summary>
        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(AssociationSet.AssociationSetMapping == null, "Don't call this method if we already have a mapping");
            Debug.Assert(underlyingModelItem != null, "The underlyingModelItem cannot be null");
            var storeEntityType = underlyingModelItem as EntityType;
            Debug.Assert(
                storeEntityType != null,
                "underlyingModelItem must be of type EntityType, actual type = " + underlyingModelItem.GetType().FullName);
            Debug.Assert(!storeEntityType.EntityModel.IsCSDL, "The storageEntityType must not be a CSDL EntityType");

            Context = context;

            // create a context if we weren't passed one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateAssociationSetMapping);
            }

            // create the item
            var cmd1 = new CreateAssociationSetMappingCommand(MappingAssociation.Association, storeEntityType);

            // now try and do some match ups by name
            var cmd2 = new DelegateCommand(
                () =>
                    {
                        Parent.AddChild(this);

                        foreach (var child in Children)
                        {
                            var end = child as MappingAssociationSetEnd;
                            if (end != null)
                            {
                                foreach (var child2 in end.Children)
                                {
                                    var mesp = child2 as MappingEndScalarProperty;
                                    if (mesp != null)
                                    {
                                        var tableColumn =
                                            MappingAssociationSet.StorageEntityType.GetFirstNamedChildByLocalName(mesp.Property, true) as
                                            Property;
                                        if (tableColumn != null)
                                        {
                                            mesp.CreateModelItem(cpc, _context, tableColumn);
                                        }
                                    }
                                }
                            }
                        }
                    });

            // now make the change
            var cp = new CommandProcessor(cpc);
            cp.EnqueueCommand(cmd1);
            cp.EnqueueCommand(cmd2);
            cp.Invoke();
        }

        // <summary>
        //     NOTE: The association set mapping view model doesn't keep a reference to the mapping model item. Instead, it
        //     keeps it to the AssociationSet and then it can find the AssociationSetMapping as an anti-dep.  We don't need to clear
        //     or set the ModelItem property.
        // </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            if (IsModelItemDeleted() == false)
            {
                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteAssociationSetMapping);
                }
                DeleteEFElementCommand.DeleteInTransaction(cpc, AssociationSet.AssociationSetMapping);
            }
        }
    }
}
