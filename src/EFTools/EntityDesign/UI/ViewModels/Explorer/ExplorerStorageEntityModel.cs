// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class ExplorerStorageEntityModel : ExplorerEntityModel
    {
        // Ghost nodes are grouping nodes in the EDM Browser which 
        // do not correspond to any underlying element in the model
        protected ExplorerFunctions _funcsGhostNode;

        public ExplorerStorageEntityModel(EditingContext context, StorageEntityModel entityModel, ExplorerEFElement parent)
            : base(context, entityModel, parent)
        {
            _typesGhostNode = new ExplorerTypes(
                Resources.StorageTypesGhostNodeName, context, this);
            _funcsGhostNode = new ExplorerFunctions(
                Resources.StorageFunctionsGhostNodeName, context, this);
            _assocsGhostNode = new ExplorerAssociations(
                Resources.StorageAssociationsGhostNodeName, context, this);
        }

        public override string Name
        {
            get
            {
                if (ModelItem != null)
                {
                    return ModelItem.DisplayName;
                }

                return null;
            }
        }

        public ExplorerFunctions Functions
        {
            get { return _funcsGhostNode; }
        }

        protected override void LoadChildrenFromModel()
        {
            // LoadEntityContainersFromModel() should be called here
            // if we start displaying EntityContainer nodes for StorageEntityModels
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            if (_typesGhostNode != null)
            {
                _children.Add(_typesGhostNode);
            }

            if (_funcsGhostNode != null)
            {
                _children.Add(_funcsGhostNode);
            }

            if (_assocsGhostNode != null)
            {
                _children.Add(_assocsGhostNode);
            }
            // do not load ExplorerEntityContainer nodes 
            // (not visible in Storage Entity Model node)
        }

        internal override ExplorerEFElement GetParentNodeForElement(EFElement childElement)
        {
            Debug.Assert(childElement != null, "on ExplorerEFElement with name " + Name + " received null child");
            var childElementType = childElement.GetType();
            if (typeof(StorageEntityType) == childElementType)
            {
                return _typesGhostNode;
            }
            else if (typeof(Association) == childElementType)
            {
                return _assocsGhostNode;
            }
            else if (typeof(Function) == childElementType)
            {
                return _funcsGhostNode;
            }
            else if (typeof(StorageEntityContainer) == childElementType)
            {
                return this;
            }
            else
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.BadChildForParentException, GetType().FullName, childElementType.FullName));
                return null;
                // TODO: we need to provide a general exception-handling mechanism and replace the above Assert()
                // by e.g. the exception below
                // throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                //     Resources.BadChildForParentException, this.GetType().FullName, childElementType.FullName));
            }
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "SsdlPngIcon"; }
        }
    }
}
