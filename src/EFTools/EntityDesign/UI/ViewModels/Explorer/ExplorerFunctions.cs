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
    ///     Dummy element which contains the Functions
    /// </summary>
    internal class ExplorerFunctions : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerFunction> _functions =
            new TypedChildList<ExplorerFunction>();

        public ExplorerFunctions(string name, EditingContext context, ExplorerEFElement parent)
            : base(context, null, parent)
        {
            if (name != null)
            {
                base.Name = name;
            }
        }

        public IList<ExplorerFunction> Functions
        {
            get { return _functions.ChildList; }
        }

        private void LoadFunctionsFromModel()
        {
            // load children from model
            // note: have to go to parent to get this as this is a dummy node
            var sem = Parent.ModelItem as StorageEntityModel;
            Debug.Assert(sem != null, "BrowserFunctions node should always have a StorageEntityModel as this.Parent.ModelItem");
            if (sem != null)
            {
                foreach (var child in sem.Functions())
                {
                    _functions.Insert(
                        (ExplorerFunction)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerFunction)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadFunctionsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in Functions)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var function = efElementToInsert as Function;
            if (function != null)
            {
                var explorerFunction = AddFunction(function);
                var index = _functions.IndexOf(explorerFunction);
                _children.Insert(index, explorerFunction);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerFunc = efElementToRemove as ExplorerFunction;
            if (explorerFunc == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _functions.Remove(explorerFunc);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerFunction AddFunction(Function function)
        {
            var explorerFunction =
                ModelToExplorerModelXRef.GetNew(_context, function, this, typeof(ExplorerFunction)) as ExplorerFunction;

            _functions.Insert(explorerFunction);
            return explorerFunction;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
