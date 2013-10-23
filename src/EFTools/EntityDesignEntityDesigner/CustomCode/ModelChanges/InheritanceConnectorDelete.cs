// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class InheritanceConnectorDelete : InheritanceConnectorModelChange
    {
        internal InheritanceConnectorDelete(InheritanceConnector inheritanceConnector)
            : base(inheritanceConnector)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = InheritanceConnector.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from inheritance connector: " + InheritanceConnector);

            if (viewModel != null)
            {
                var modelInheritanceConnector = viewModel.ModelXRef.GetExisting(InheritanceConnector) as Model.Designer.InheritanceConnector;
                if (modelInheritanceConnector != null)
                {
                    viewModel.ModelXRef.Remove(modelInheritanceConnector, InheritanceConnector);
                    DeleteEFElementCommand.DeleteInTransaction(cpc, modelInheritanceConnector);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 0; }
        }
    }
}
