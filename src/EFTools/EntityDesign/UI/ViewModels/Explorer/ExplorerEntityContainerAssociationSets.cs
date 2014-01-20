// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    // <summary>
    //     Dummy element which contains the AssociationSets inside the EntityContainer
    // </summary>
    internal class ExplorerEntityContainerAssociationSets : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerAssociationSet> _associationSets =
            new TypedChildList<ExplorerAssociationSet>();

        public ExplorerEntityContainerAssociationSets(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }
        }

        public IList<ExplorerAssociationSet> AssociationSets
        {
            get { return _associationSets.ChildList; }
        }

        private void LoadAssociationSetsFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var container = Parent.ModelItem as BaseEntityContainer;
            if (container != null)
            {
                foreach (var child in container.AssociationSets())
                {
                    _associationSets.Insert(
                        (ExplorerAssociationSet)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerAssociationSet)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadAssociationSetsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in AssociationSets)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var assocSet = efElementToInsert as AssociationSet;
            if (assocSet != null)
            {
                var explorerAssocSet = AddAssociationSet(assocSet);
                var index = _associationSets.IndexOf(explorerAssocSet);
                _children.Insert(index, explorerAssocSet);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerAssocSet = efElementToRemove as ExplorerAssociationSet;
            if (explorerAssocSet == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _associationSets.Remove(explorerAssocSet);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerAssociationSet AddAssociationSet(AssociationSet entityType)
        {
            var explorerAssocSet =
                ModelToExplorerModelXRef.GetNew(_context, entityType, this, typeof(ExplorerAssociationSet)) as ExplorerAssociationSet;
            _associationSets.Insert(explorerAssocSet);
            return explorerAssocSet;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
