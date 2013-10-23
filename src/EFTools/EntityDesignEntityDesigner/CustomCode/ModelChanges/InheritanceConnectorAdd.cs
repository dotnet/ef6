// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using InheritanceConnector = Microsoft.Data.Entity.Design.EntityDesigner.View.InheritanceConnector;

    internal class InheritanceConnectorAdd : InheritanceConnectorModelChange
    {
        internal InheritanceConnectorAdd(InheritanceConnector inheritanceConnector)
            : base(inheritanceConnector)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, InheritanceConnector);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, InheritanceConnector inheritanceConnector)
        {
            // if there was a circular inheritance, this connector will be deleted, if so, we just return
            if (inheritanceConnector.IsDeleted)
            {
                return;
            }

            var viewModel = inheritanceConnector.GetRootViewModel();
            Debug.Assert(
                viewModel != null, "Unable to find root view model from inheritance connector: " + inheritanceConnector.AccessibleName);

            if (viewModel != null)
            {
                var modelEntityTypeBase = viewModel.ModelXRef.GetExisting(inheritanceConnector.ModelElement) as EntityTypeBaseType;
                if (modelEntityTypeBase != null)
                {
                    var modelEntity = modelEntityTypeBase.Parent as EntityType;
                    var modelDiagram = viewModel.ModelXRef.GetExisting(inheritanceConnector.Diagram) as Diagram;
                    Debug.Assert(modelEntity != null && modelDiagram != null);
                    if (modelEntity != null
                        && modelDiagram != null)
                    {
                        var cmd = new CreateInheritanceConnectorCommand(modelDiagram, modelEntity);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                        var modelInheritanceConnector = cmd.InheritanceConnector;
                        viewModel.ModelXRef.Add(modelInheritanceConnector, inheritanceConnector, viewModel.EditingContext);
                    }
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 150; }
        }
    }
}
