// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class ExplorerDiagram : EntityDesignExplorerEFElement
    {
        private readonly TypedChildList<ExplorerEntityTypeShape> _explorerEntityTypeShapes = new TypedChildList<ExplorerEntityTypeShape>();

        public ExplorerDiagram(EditingContext context, Diagram diagram, ExplorerEFElement parent)
            : base(context, diagram, parent)
        {
            // do nothing
        }

        internal string DiagramMoniker
        {
            get
            {
                var diagram = ModelItem as Diagram;
                return diagram.Id.Value;
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var entityTypeShape = efElementToInsert as EntityTypeShape;
            if (entityTypeShape != null)
            {
                var explorerEntityTypeShape = AddEntityTypeShape(entityTypeShape);
                var index = _explorerEntityTypeShapes.IndexOf(explorerEntityTypeShape);
                _children.Insert(index, explorerEntityTypeShape);
            }
        }

        protected override void LoadChildrenFromModel()
        {
            var diagram = ModelItem as Diagram;

            Debug.Assert(diagram != null, "Underlying diagram is null calculating EntityTypeShape for ExplorerDiagram with name " + Name);
            if (diagram != null)
            {
                foreach (var child in diagram.EntityTypeShapes)
                {
                    // only show the EntityTypeShape that contains a valid EntityType.
                    if (child.EntityType.Target != null)
                    {
                        _explorerEntityTypeShapes.Insert(
                            (ExplorerEntityTypeShape)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerEntityTypeShape)));
                    }
                }
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerEntityTypeShape = efElementToRemove as ExplorerEntityTypeShape;
            if (explorerEntityTypeShape == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName, Name, GetType().FullName));
                return false;
            }
            // We can't use TypedChildList's Remove to delete the entity-type-shape because the it will fail if the underlying entity-type has been deleted.
            // When the entity-type is deleted, the name of the entity-type-shape will change to an empty string and deletion will fail 
            // until the list is re-sorted (see implementation of ExplorerEFElement's Remove).
            return _explorerEntityTypeShapes.ChildList.Remove(explorerEntityTypeShape);
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in _explorerEntityTypeShapes.ChildList)
            {
                _children.Add(child);
            }
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "DiagramPngIcon"; }
        }

        // the Diagram name is editable in Explorer window.
        public override bool IsEditableInline
        {
            get { return true; }
        }

        private ExplorerEntityTypeShape AddEntityTypeShape(EntityTypeShape entityTypeShape)
        {
            var explorerEntityTypeShape =
                ModelToExplorerModelXRef.GetNew(_context, entityTypeShape, this, typeof(ExplorerEntityTypeShape)) as ExplorerEntityTypeShape;
            _explorerEntityTypeShapes.Insert(explorerEntityTypeShape);
            return explorerEntityTypeShape;
        }
    }
}
