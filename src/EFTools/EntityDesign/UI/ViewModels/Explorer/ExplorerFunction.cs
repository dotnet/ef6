// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerFunction : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerParameter> _parameters =
            new TypedChildList<ExplorerParameter>();

        public ExplorerFunction(EditingContext context, Function func, ExplorerEFElement parent)
            : base(context, func, parent)
        {
            // do nothing
        }

        public IList<ExplorerParameter> Parameters
        {
            get { return _parameters.ChildList; }
        }

        internal bool IsComposable
        {
            get
            {
                var func = ModelItem as Function;
                if (func != null)
                {
                    return func.IsComposable.Value;
                }

                return true; // assume the worst, that this can't be a function import
            }
        }

        private void LoadParametersFromModel()
        {
            // load children from model
            var function = ModelItem as Function;
            Debug.Assert(function != null, "Underlying Function is null for ExplorerFunction with name " + Name);
            if (function != null)
            {
                foreach (var child in function.Parameters())
                {
                    _parameters.Insert(
                        (ExplorerParameter)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerParameter)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadParametersFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in Parameters)
            {
                _children.Add(child);
            }
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "SprocPngIcon"; }
        }
    }
}
