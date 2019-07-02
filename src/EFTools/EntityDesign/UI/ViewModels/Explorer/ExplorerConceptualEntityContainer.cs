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

    internal class ExplorerConceptualEntityContainer : ExplorerEntityContainer
    {
        public ExplorerConceptualEntityContainer(
            EditingContext context,
            ConceptualEntityContainer entityContainer, ExplorerEFElement parent)
            : base(context, entityContainer, parent)
        {
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing here since we have no corresponding nodes in the model
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            if (_entitySetsGhostNode != null)
            {
                _children.Add(_entitySetsGhostNode);
            }
            if (_assocSetsGhostNode != null)
            {
                _children.Add(_assocSetsGhostNode);
            }
        }

        internal override ExplorerEFElement GetParentNodeForElement(EFElement childElement)
        {
            Debug.Assert(childElement != null, "GetParentNodeForElement on ExplorerEFElement with name " + Name + " received null child");
            Debug.Assert(
                ModelItem == childElement.Parent, "GetParentNodeForElement - underlying model element with identity " +
                                                  ModelItem.Identity + " is not the same as the child element's parent, which has identity "
                                                  + childElement.Parent.Identity);
            var childElementType = childElement.GetType();
            if (typeof(ConceptualEntitySet) == childElementType)
            {
                return _entitySetsGhostNode;
            }
            else if (typeof(AssociationSet) == childElementType)
            {
                return _assocSetsGhostNode;
            }
            else if (typeof(FunctionImport) == childElementType)
            {
                // if asked what FunctionImport parent node is redirect to the parent (i.e. the ConceptualEntityModel)
                // This is because, for FunctionImports, we are not using the same parent-child relationship in the 
                // View-Model that we are in the Model
                return Parent.GetParentNodeForElement(childElement);
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
            get { return "EntityContainerPngIcon"; }
        }
    }
}
