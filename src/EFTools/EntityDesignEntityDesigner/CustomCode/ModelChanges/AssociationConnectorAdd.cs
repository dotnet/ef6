// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Association = Microsoft.Data.Entity.Design.Model.Entity.Association;
    using AssociationConnector = Microsoft.Data.Entity.Design.EntityDesigner.View.AssociationConnector;

    internal class AssociationConnectorAdd : AssociationConnectorModelChange
    {
        internal AssociationConnectorAdd(AssociationConnector associationConnector)
            : base(associationConnector)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, AssociationConnector);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, AssociationConnector associationConnector)
        {
            var viewModel = associationConnector.GetRootViewModel();
            Debug.Assert(
                viewModel != null, "Unable to find root view model from association connector: " + associationConnector.AccessibleName);

            if (viewModel != null)
            {
                var modelAssociation = viewModel.ModelXRef.GetExisting(associationConnector.ModelElement) as Association;
                var modelDiagram = viewModel.ModelXRef.GetExisting(associationConnector.Diagram) as Diagram;

                Debug.Assert(modelAssociation != null && modelDiagram != null);
                if (modelAssociation != null
                    && modelDiagram != null)
                {
                    var cmd = new CreateAssociationConnectorCommand(modelDiagram, modelAssociation);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    var modelAssociationConnector = cmd.AssociationConnector;
                    Debug.Assert(modelAssociationConnector != null);
                    viewModel.ModelXRef.Add(modelAssociationConnector, associationConnector, viewModel.EditingContext);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 150; }
        }
    }
}
