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
    ///     Dummy element which contains the Associations
    /// </summary>
    internal class ExplorerAssociations : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerAssociation> _associations =
            new TypedChildList<ExplorerAssociation>();

        public ExplorerAssociations(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }
        }

        #region Properties

        public IList<ExplorerAssociation> Associations
        {
            get { return _associations.ChildList; }
        }

        #endregion

        private void LoadAssociationsFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var entityModel = Parent.ModelItem as BaseEntityModel;
            if (entityModel != null)
            {
                foreach (var child in entityModel.Associations())
                {
                    if (child.EntityModel.IsCSDL)
                    {
                        _associations.Insert(
                            (ExplorerConceptualAssociation)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerConceptualAssociation)));
                    }
                    else
                    {
                        _associations.Insert(
                            (ExplorerStorageAssociation)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerStorageAssociation)));
                    }
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadAssociationsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();

            foreach (var child in Associations)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var assoc = efElementToInsert as Association;
            if (assoc != null)
            {
                var explorerAssociation = AddAssociation(assoc);
                var index = _associations.IndexOf(explorerAssociation);
                _children.Insert(index, explorerAssociation);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerAssociation = efElementToRemove as ExplorerAssociation;
            if (explorerAssociation == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _associations.Remove(explorerAssociation);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerAssociation AddAssociation(Association assoc)
        {
            ExplorerAssociation explorerAssociation = null;
            if (assoc.EntityModel.IsCSDL)
            {
                explorerAssociation =
                    ModelToExplorerModelXRef.GetNew(_context, assoc, this, typeof(ExplorerConceptualAssociation)) as
                    ExplorerConceptualAssociation;
            }
            else
            {
                explorerAssociation =
                    ModelToExplorerModelXRef.GetNew(_context, assoc, this, typeof(ExplorerStorageAssociation)) as ExplorerStorageAssociation;
            }
            _associations.Insert(explorerAssociation);
            return explorerAssociation;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
