// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    internal partial class Property
    {
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

                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new PropertyDelete(this));
                }
            }
        }
    }
}
