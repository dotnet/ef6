// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal partial class NavigationProperty : IContainRelatedElementsToEmphasizeWhenSelected
    {
        /// <summary>
        ///     Returns NavigationProperty's Association and the Association related elements minus itself.
        /// </summary>
        public IEnumerable<ModelElement> RelatedElementsToEmphasizeOnSelected
        {
            get
            {
                if (_association != null
                    && _association.IsDeleted == false)
                {
                    yield return _association;

                    IContainRelatedElementsToEmphasizeWhenSelected relatedElementsToEmphasize = _association;
                    Debug.Assert(
                        relatedElementsToEmphasize != null,
                        "Association class should implement IContainRelatedElementsToEmphasizeWhenSelected interface.");

                    if (relatedElementsToEmphasize != null)
                    {
                        foreach (var el in relatedElementsToEmphasize.RelatedElementsToEmphasizeOnSelected)
                        {
                            if (el != this)
                            {
                                yield return el;
                            }
                        }
                    }
                }
            }
        }

        private Association _association;

        public Association Association
        {
            get { return _association; }
            set { _association = value; }
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();

            if (EntityType != null
                && EntityType.EntityDesignerViewModel != null
                && EntityType.EntityDesignerViewModel.Reloading == false)
            {
                var viewModel = EntityType.EntityDesignerViewModel;
                var tx = ModelUtils.GetCurrentTx(Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    // deleting the property would select the Diagram, select parent Entity instead
                    var diagram = viewModel.GetDiagram();
                    if (diagram != null
                        && diagram.ActiveDiagramView != null)
                    {
                        var shape = diagram.FindShape(EntityType);
                        if (shape != null)
                        {
                            diagram.ActiveDiagramView.Selection.Set(new DiagramItem(shape));
                        }
                    }

                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new NavigationPropertyDelete(this));
                }
            }
        }
    }
}
