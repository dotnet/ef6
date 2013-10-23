// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class ExplorerDiagrams : EntityDesignExplorerEFElement
    {
        // Ghost nodes are grouping nodes in the EDM Browser which 
        // do not correspond to any underlying element in the model
        protected ExplorerTypes _typesGhostNode;

        private readonly TypedChildList<ExplorerDiagram> _diagrams = new TypedChildList<ExplorerDiagram>();

        public ExplorerDiagrams(EditingContext context, Diagrams diagrams, ExplorerEFElement parent)
            : base(context, diagrams, parent)
        {
            var name = Resources.DiagramTypesGhostNodeName;
            base.Name = name;

            _typesGhostNode = new ExplorerTypes(name, context, this);
        }

        public IList<ExplorerDiagram> Diagrams
        {
            get { return _diagrams.ChildList; }
        }

        public ExplorerTypes Types
        {
            get { return _typesGhostNode; }
        }

        private void LoadDiagramsFromModel()
        {
            // load children from model
            var diagrams = ModelItem as Diagrams;
            if (diagrams != null)
            {
                foreach (var child in diagrams.Items)
                {
                    _diagrams.Insert(
                        (ExplorerDiagram)ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerDiagram)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadDiagramsFromModel();
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var child in Diagrams)
            {
                _children.Add(child);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var diagram = efElementToInsert as Diagram;
            if (diagram != null)
            {
                var explorerDiagram = AddDiagram(diagram);
                var index = _diagrams.IndexOf(explorerDiagram);
                _children.Insert(index, explorerDiagram);
            }
            else
            {
                base.InsertChild(efElementToInsert);
            }
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerDiagram = efElementToRemove as ExplorerDiagram;
            if (explorerDiagram == null)
            {
                Debug.Fail(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.BadRemoveBadChildType,
                        efElementToRemove.GetType().FullName,
                        Name,
                        GetType().FullName));

                return false;
            }

            var indexOfRemovedChild = _diagrams.Remove(explorerDiagram);
            return (indexOfRemovedChild < 0) ? false : true;
        }

        private ExplorerDiagram AddDiagram(Diagram diagram)
        {
            var explorerDiagram = ModelToExplorerModelXRef.GetNew(_context, diagram, this, typeof(ExplorerDiagram)) as ExplorerDiagram;
            _diagrams.Insert(explorerDiagram);
            return explorerDiagram;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "FolderPngIcon"; }
        }
    }
}
