// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class AssociationConnectorDelete : AssociationConnectorModelChange
    {
        internal AssociationConnectorDelete(AssociationConnector associationConnector)
            : base(associationConnector)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = AssociationConnector.GetRootViewModel();
            Debug.Assert(
                viewModel != null, "Unable to find root view model from association connector: " + AssociationConnector.AccessibleName);

            if (viewModel != null)
            {
                var modelAssociationConnector = viewModel.ModelXRef.GetExisting(AssociationConnector) as Model.Designer.AssociationConnector;
                if (modelAssociationConnector != null)
                {
                    DeleteEFElementCommand.DeleteInTransaction(cpc, modelAssociationConnector);
                    viewModel.ModelXRef.Remove(modelAssociationConnector, AssociationConnector);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 0; }
        }
    }
}
