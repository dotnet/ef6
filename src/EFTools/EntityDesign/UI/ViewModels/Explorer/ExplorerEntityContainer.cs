// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal abstract class ExplorerEntityContainer : EntityDesignExplorerEFElement
    {
        // Ghost nodes are grouping nodes in the EDM Browser which 
        // do not correspond to any underlying element in the model
        protected ExplorerEntityContainerEntitySets _entitySetsGhostNode;
        protected ExplorerEntityContainerAssociationSets _assocSetsGhostNode;

        public ExplorerEntityContainer(
            EditingContext context,
            BaseEntityContainer entityContainer, ExplorerEFElement parent)
            : base(context, entityContainer, parent)
        {
            _entitySetsGhostNode = new ExplorerEntityContainerEntitySets(
                Resources.EntitySetsGhostNodeName, context, this);
            _assocSetsGhostNode = new ExplorerEntityContainerAssociationSets(
                Resources.AssociationSetsGhostNodeName, context, this);
        }

        #region Properties

        public override string Name
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.EntityContainerNodeName, base.Name);
            }
        }

        #endregion

        public ExplorerEntityContainerEntitySets EntitySets
        {
            get { return _entitySetsGhostNode; }
        }

        public ExplorerEntityContainerAssociationSets AssociationSets
        {
            get { return _assocSetsGhostNode; }
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }
    }
}
