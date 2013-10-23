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

    /// <summary>
    ///     Dummy element which contains the EntitySets inside the EntityContainer
    /// </summary>
    internal class ExplorerEntityContainerEntitySets : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerEntitySet> _entitySets =
            new TypedChildList<ExplorerEntitySet>();

        public ExplorerEntityContainerEntitySets(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }
        }

        public IList<ExplorerEntitySet> EntitySets
        {
            get { return _entitySets.ChildList; }
        }

        private void LoadEntitySetsFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var entityModel = Parent.ModelItem as BaseEntityContainer;
            if (entityModel != null)
            {
                foreach (var child in entityModel.EntitySets())
                {
                    _entitySets.Insert(
                        (ExplorerEntitySet)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerEntitySet)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadEntitySetsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();

            foreach (var child in EntitySets)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var entitySet = efElementToInsert as EntitySet;
            if (entitySet != null)
            {
                var explorerEntitySet = AddEntitySet(entitySet);
                var index = _entitySets.IndexOf(explorerEntitySet);
                _children.Insert(index, explorerEntitySet);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerEntitySet = efElementToRemove as ExplorerEntitySet;
            if (explorerEntitySet == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _entitySets.Remove(explorerEntitySet);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerEntitySet AddEntitySet(EntitySet entitySet)
        {
            var explorerEntitySet =
                ModelToExplorerModelXRef.GetNew(_context, entitySet, this, typeof(ExplorerEntitySet)) as ExplorerEntitySet;
            _entitySets.Insert(explorerEntitySet);
            return explorerEntitySet;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
