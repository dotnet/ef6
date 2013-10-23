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
    ///     Dummy element which contains the EntityTypes
    /// </summary>
    internal class ExplorerTypes : EntityDesignExplorerEFElement
    {
        private readonly bool _isConceptual;

        private readonly TypedChildList<ExplorerEntityType> _entityTypes =
            new TypedChildList<ExplorerEntityType>();

        public ExplorerTypes(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }

            _isConceptual = (typeof(ExplorerConceptualEntityModel) == parent.GetType()) ? true : false;
        }

        public IList<ExplorerEntityType> EntityTypes
        {
            get { return _entityTypes.ChildList; }
        }

        public bool IsConceptualEntityTypesNode
        {
            get { return _isConceptual; }
        }

        private void LoadEntityTypesFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var entityModel = Parent.ModelItem as BaseEntityModel;
            if (entityModel != null)
            {
                foreach (var child in entityModel.EntityTypes())
                {
                    if (_isConceptual)
                    {
                        _entityTypes.Insert(
                            (ExplorerConceptualEntityType)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerConceptualEntityType)));
                    }
                    else
                    {
                        _entityTypes.Insert(
                            (ExplorerStorageEntityType)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerStorageEntityType)));
                    }
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadEntityTypesFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in EntityTypes)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var entityType = efElementToInsert as EntityType;
            if (entityType != null)
            {
                var explorerEntityType = AddEntityType(entityType);
                var index = _entityTypes.IndexOf(explorerEntityType);
                _children.Insert(index, explorerEntityType);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerEntityType = efElementToRemove as ExplorerEntityType;
            if (explorerEntityType == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _entityTypes.Remove(explorerEntityType);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerEntityType AddEntityType(EntityType entityType)
        {
            ExplorerEntityType explorerEntityType = null;
            if (_isConceptual)
            {
                explorerEntityType =
                    ModelToExplorerModelXRef.GetNew(_context, entityType, this, typeof(ExplorerConceptualEntityType)) as ExplorerEntityType;
            }
            else
            {
                explorerEntityType =
                    ModelToExplorerModelXRef.GetNew(_context, entityType, this, typeof(ExplorerStorageEntityType)) as ExplorerEntityType;
            }

            _entityTypes.Insert(explorerEntityType);
            return explorerEntityType;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
