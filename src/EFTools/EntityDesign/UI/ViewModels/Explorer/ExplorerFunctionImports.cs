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
    ///     Dummy element which contains the FunctionImports from the ConceptualEntityContainer
    /// </summary>
    internal class ExplorerFunctionImports : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerFunctionImport> _functionImports =
            new TypedChildList<ExplorerFunctionImport>();

        public ExplorerFunctionImports(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }
        }

        public IList<ExplorerFunctionImport> FunctionImports
        {
            get { return _functionImports.ChildList; }
        }

        private void LoadFunctionImportsFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var entityModel = Parent.ModelItem as ConceptualEntityModel;
            if (entityModel != null)
            {
                var entityContainer = entityModel.FirstEntityContainer as ConceptualEntityContainer;
                if (entityContainer != null)
                {
                    foreach (var child in entityContainer.FunctionImports())
                    {
                        _functionImports.Insert(
                            (ExplorerFunctionImport)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerFunctionImport)));
                    }
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadFunctionImportsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in FunctionImports)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var funcImport = efElementToInsert as FunctionImport;
            if (funcImport != null)
            {
                var explorerFuncImport = AddFunctionImport(funcImport);
                var index = _functionImports.IndexOf(explorerFuncImport);
                _children.Insert(index, explorerFuncImport);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerFuncImport = efElementToRemove as ExplorerFunctionImport;
            if (explorerFuncImport == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType, efElementToRemove.GetType().FullName, Name,
                        GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _functionImports.Remove(explorerFuncImport);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerFunctionImport AddFunctionImport(FunctionImport funcImport)
        {
            var explorerFuncImport =
                ModelToExplorerModelXRef.GetNew(_context, funcImport, this, typeof(ExplorerFunctionImport)) as ExplorerFunctionImport;
            _functionImports.Insert(explorerFuncImport);
            return explorerFuncImport;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
