// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class ExplorerConceptualEntityModel : ExplorerEntityModel
    {
        // Ghost nodes are grouping nodes in the EDM Browser which 
        // do not correspond to any underlying element in the model
        protected ExplorerComplexTypes _complexTypesGhostNode;
        protected ExplorerEnumTypes _enumTypesGhostNode;
        protected ExplorerFunctionImports _funcImportsGhostNode;

        public ExplorerConceptualEntityModel(EditingContext context, ConceptualEntityModel entityModel, ExplorerEFElement parent)
            : base(context, entityModel, parent)
        {
            _typesGhostNode = new ExplorerTypes(
                Resources.ConceptualTypesGhostNodeName, context, this);
            _complexTypesGhostNode = new ExplorerComplexTypes(Resources.ComplexTypesGhostNodeName, context, this);
            _assocsGhostNode = new ExplorerAssociations(
                Resources.ConceptualAssociationsGhostNodeName, context, this);
            _funcImportsGhostNode = new ExplorerFunctionImports(
                Resources.FunctionImportsGhostNodeName, context, this);

            _enumTypesGhostNode = null;

            if (EdmFeatureManager.GetEnumTypeFeatureState(entityModel.Artifact).IsEnabled())
            {
                _enumTypesGhostNode = new ExplorerEnumTypes(
                    Resources.EnumTypesGhostNodeName, context, this);
            }
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

        protected override void LoadChildrenFromModel()
        {
            LoadEntityContainersFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            if (_typesGhostNode != null)
            {
                _children.Add(_typesGhostNode);
            }

            if (_complexTypesGhostNode != null)
            {
                _children.Add(_complexTypesGhostNode);
            }

            if (_enumTypesGhostNode != null)
            {
                _children.Add(_enumTypesGhostNode);
            }

            if (_assocsGhostNode != null)
            {
                _children.Add(_assocsGhostNode);
            }

            if (_funcImportsGhostNode != null)
            {
                _children.Add(_funcImportsGhostNode);
            }

            foreach (var child in EntityContainers)
            {
                _children.Add(child);
            }
        }

        private void LoadEntityContainersFromModel()
        {
            // load entity container children from model
            var em = ModelItem as ConceptualEntityModel;
            if (em != null)
            {
                foreach (ConceptualEntityContainer child in em.EntityContainers())
                {
                    _entityContainers.Insert(
                        (ExplorerConceptualEntityContainer)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerConceptualEntityContainer)));
                }
            }
        }

        public ExplorerFunctionImports FunctionImports
        {
            get { return _funcImportsGhostNode; }
        }

        public ExplorerEnumTypes EnumTypes
        {
            get { return _enumTypesGhostNode; }
        }

        internal override ExplorerEFElement GetParentNodeForElement(EFElement childElement)
        {
            Debug.Assert(
                childElement != null,
                "GetParentNodeForElement on ExplorerEFElement with name " + Name + " received null child");

            var childElementType = childElement.GetType();
            if (typeof(ConceptualEntityType) == childElementType)
            {
                return _typesGhostNode;
            }
            if (typeof(ComplexType) == childElementType)
            {
                return _complexTypesGhostNode;
            }
            else if (typeof(EnumType) == childElementType)
            {
                return _enumTypesGhostNode;
            }
            else if (typeof(Association) == childElementType)
            {
                return _assocsGhostNode;
            }
            else if (typeof(FunctionImport) == childElementType)
            {
                return _funcImportsGhostNode;
            }
            else if (typeof(ConceptualEntityContainer) == childElementType)
            {
                return this;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.BadChildForParentException, GetType().FullName, childElementType.FullName));
            }
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "CsdlPngIcon"; }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var entityContainer = efElementToInsert as ConceptualEntityContainer;
            if (entityContainer != null)
            {
                _entityContainers.Insert(
                    (ExplorerConceptualEntityContainer)
                    ModelToExplorerModelXRef.GetNewOrExisting(_context, entityContainer, this, typeof(ExplorerConceptualEntityContainer)));
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerEntityContainer = efElementToRemove as ExplorerConceptualEntityContainer;
            if (explorerEntityContainer == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }

            var indexOfRemovedChild = _entityContainers.Remove(explorerEntityContainer);
            return (indexOfRemovedChild < 0) ? false : true;
        }
    }
}
