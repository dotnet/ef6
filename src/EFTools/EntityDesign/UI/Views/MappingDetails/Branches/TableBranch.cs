// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     This branch shows a line for every table mapped for this entity type.  It also displays a
    ///     creator node so that users can add new tables.
    /// </summary>
    internal class TableBranch : TreeGridDesignerBranch
    {
        private MappingConceptualEntityType _mappingConceptualTypeMapping;
        private readonly List<IBranch> _expandedBranches = new List<IBranch>();
        //private bool _registeredEventHandlers = false;

        internal TableBranch(MappingConceptualEntityType mappingConceptualTypeMapping, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingConceptualTypeMapping, columns)
        {
            _mappingConceptualTypeMapping = mappingConceptualTypeMapping;
        }

        public TableBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingConceptualTypeMapping = component as MappingConceptualEntityType;
            if (mappingConceptualTypeMapping != null)
            {
                _mappingConceptualTypeMapping = mappingConceptualTypeMapping;
            }

            return true;
        }

        protected override string GetText(int row, int column)
        {
            if (column == 0)
            {
                return base.GetText(row, column);
            }
            else
            {
                return string.Empty;
            }
        }

        internal override object GetElement(int index)
        {
            return _mappingConceptualTypeMapping.Children[index];
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override object GetCreatorElement()
        {
            var mset = new MappingStorageEntityType(null, null, _mappingConceptualTypeMapping);
            return mset;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingConceptualTypeMapping.Children.Count; i++)
            {
                if (element == _mappingConceptualTypeMapping.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingConceptualTypeMapping.Children.Count; }
        }

        internal override int CreatorNodeCount
        {
            get { return 1; }
        }

        protected override string GetCreatorNodeText(int index)
        {
            return Resources.MappingDetails_TableCreatorNode;
        }

        protected override bool IsExpandable(int index)
        {
            return (index < ElementCount);
        }

        protected override IBranch GetExpandedBranch(int index)
        {
            if (index < ElementCount)
            {
                var mset = GetElement(index) as MappingStorageEntityType;
                if (mset != null)
                {
                    var branchList = new ArrayList(2);
                    branchList.Add(new ConditionBranch(mset, GetColumns()));
                    branchList.Add(new ColumnMappingsBranch(mset, GetColumns()));
                    var aggBranch = new AggregateBranch(branchList, 0);
                    if (_expandedBranches.Count <= index)
                    {
                        Debug.Assert
                            (
                                _expandedBranches.Count == index,
                                "_expandedBranches.Count (" + _expandedBranches.Count + ") should equal index (" + index + ")");
                        _expandedBranches.Add(aggBranch);
                    }
                    else
                    {
                        _expandedBranches[index] = aggBranch;
                    }
                    return aggBranch;
                }
            }

            return null;
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_TABLE;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }

        public override void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args)
        {
            if (!args.InsertingItem)
            {
                Debug.Assert(
                    args.Row < _expandedBranches.Count,
                    "args.Row(" + args.Row + ") should be < _expandedBranches.Count (" + _expandedBranches.Count + ")");
                if (args.DeletingItem)
                {
                    _expandedBranches.RemoveAt(args.Row);
                }
                else
                {
                    if (_expandedBranches[args.Row] != null)
                    {
                        DoBranchModification(BranchModificationEventArgs.RemoveBranch(_expandedBranches[args.Row]));
                        _expandedBranches[args.Row] = null;
                    }
                }
            }
            base.OnColumnValueChanged(args);
        }

        //protected override void AddEventHandlers()
        //{
        //    if (_registeredEventHandlers == false)
        //    {
        //        PackageManager.Package.ModelManager.ModelChangesCommitted += new EventHandler<EfiChangedEventArgs>(OnModelChangesCommitted);
        //        _registeredEventHandlers = true;
        //    }
        //}

        //protected override void RemoveEventHandlers()
        //{
        //    if (_registeredEventHandlers)
        //    {
        //        PackageManager.Package.ModelManager.ModelChangesCommitted -= new EventHandler<EfiChangedEventArgs>(OnModelChangesCommitted);
        //        _registeredEventHandlers = false;
        //    }
        //}

        //private void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        //{
        //    // if the EntitySetMapping is already there, then we will get 2 changes and we don't want
        //    // to update the trid twice; this list is there to keep track of which EntityTypes we have
        //    // processed (for instance, we'll get two MappingFragment creates, but these all tie back to
        //    // one EntityType on the C-Side)
        //    Dictionary<EntityType, bool> processedNewFragments = new Dictionary<EntityType, bool>();

        //    // same for deleting
        //    Dictionary<EntityType, bool> processedDeletedFragments = new Dictionary<EntityType, bool>();

        //    EditingContext editingContext = _mappingConceptualTypeMapping.Context;
        //    if (editingContext.Services.Contains<EFArtifactService>() == false)
        //    {
        //        // TODO: For some reason, RemoveEventHanlders() isn't being called; figure out why
        //        RemoveEventHandlers();
        //        return;
        //    }
        //    EFArtifactService service = editingContext.GetEFArtifactService();
        //    EFArtifact artifactFromContext = service.Artifact;

        //    foreach (EfiChange change in e.ChangeGroup.Changes)
        //    {
        //        // only process changes for the artifact this view is associated with
        //        if (change.Changed.Artifact != artifactFromContext)
        //        {
        //            continue;
        //        }

        //        // ignore if null ConceptualEntityType
        //        if (_mappingConceptualTypeMapping.ConceptualEntityType == null )
        //        {
        //            continue;
        //        }

        //        if (change.Type == EfiChange.EfiChangeType.Create)
        //        {
        //            // sometimes changes come through as one big change when the EntitySetMapping
        //            // has to be created; sometimes its 2 changes if only the EntityTypeMapping has to 
        //            // be created - regardless, resolve the item changed down to see if a mapping fragment
        //            // got created.
        //            EntitySetMapping esm = change.Changed as EntitySetMapping;
        //            EntityTypeMapping etm = null;
        //            if (esm != null && esm.EntityTypeMappings().Count > 0)
        //            {
        //                etm = esm.EntityTypeMappings()[0];
        //            }

        //            MappingFragment frag = null;
        //            if (etm == null)
        //            {
        //                etm = change.Changed as EntityTypeMapping;
        //            }
        //            if (etm != null && etm.MappingFragments().Count > 0)
        //            {
        //                frag = etm.MappingFragments()[0];
        //            }

        //            if (frag == null)
        //            {
        //                frag = change.Changed as MappingFragment;
        //            }

        //            if (frag != null)
        //            {
        //                etm = frag.Parent as EntityTypeMapping;
        //                if (etm.TypeName.Bindings.Count > 0 &&
        //                    etm.TypeName.Bindings[0].Target == _mappingConceptualTypeMapping.ConceptualEntityType &&
        //                    processedNewFragments.ContainsKey(_mappingConceptualTypeMapping.ConceptualEntityType) == false)
        //                {
        //                    processedNewFragments.Add(_mappingConceptualTypeMapping.ConceptualEntityType, true);
        //                    DoBranchModification(BranchModificationEventArgs.InsertItems(this, _mappingConceptualTypeMapping.Children.Count - 1, 1));
        //                }
        //            }
        //            else
        //            {
        //                // we can't figure what happened, so redo the whole branch
        //                DoBranchModification(BranchModificationEventArgs.DisplayDataChanged(new DisplayDataChangedData(this)));
        //            }
        //        }
        //        else if (change.Type == EfiChange.EfiChangeType.Update)
        //        {
        //            MappingEFElement melem = GetMappingElementFromChange(change);
        //            if (melem != null)
        //            {
        //                int indexOfChange = _mappingConceptualTypeMapping.IndexOfChild(melem);
        //                if (indexOfChange != -1)
        //                {
        //                    DisplayDataChangedData data = new DisplayDataChangedData(VirtualTreeDisplayDataChanges.All, this, indexOfChange, 0, 1);
        //                    DoBranchModification(BranchModificationEventArgs.DisplayDataChanged(data));
        //                }
        //            }
        //        }
        //        else if (change.Type == EfiChange.EfiChangeType.Delete)
        //        {
        //            MappingEFElement melem = GetMappingElementFromChange(change);
        //            if (melem != null &&
        //                processedDeletedFragments.ContainsKey(_mappingConceptualTypeMapping.ConceptualEntityType) == false)
        //            {
        //                int indexOfChange = _mappingConceptualTypeMapping.IndexOfChild(melem);
        //                if (indexOfChange != -1)
        //                {
        //                    processedDeletedFragments.Add(_mappingConceptualTypeMapping.ConceptualEntityType, true);
        //                    DoBranchModification(BranchModificationEventArgs.DeleteItems(this, indexOfChange, 1));

        //                    // this cause the creator node to come back
        //                    DoBranchModification(BranchModificationEventArgs.DisplayDataChanged(new DisplayDataChangedData(this)));
        //                }
        //            }
        //        }
        //    }
        //}

        //private MappingEFElement GetMappingElementFromChange(EfiChange change)
        //{
        //    // are we deleting a fragment?
        //    MappingFragment frag = change.Changed as MappingFragment;
        //    if (frag != null)
        //    {
        //        // make sure that this fragment is for the type we care about
        //        EntityTypeMapping etm = frag.Parent as EntityTypeMapping;
        //        if (etm != null &&
        //            _mappingConceptualTypeMapping != null &&
        //            etm.TypeName.Bindings.Count > 0 &&
        //            etm.TypeName.Bindings[0].Target == _mappingConceptualTypeMapping.ConceptualEntityType)
        //        {
        //            // make sure that we can get from the fragment to our table
        //            if (frag.TableName.Target != null &&
        //                frag.TableName.Target.EntityType.Target != null)
        //            {
        //                // find the mapping element in our Xref
        //                ModelToMappingModelXRef xref = ModelToMappingModelXRef.GetModelToMappingModelXRef(_mappingConceptualTypeMapping.Context);
        //                EFToolsTracer.AssertTraceEvent(xref != null);

        //                MappingEFElement melem = xref.GetExisting(frag.TableName.Target.EntityType.Target);
        //                return melem;
        //            }
        //        }
        //    }

        //    return null;
        //}
    }
}
