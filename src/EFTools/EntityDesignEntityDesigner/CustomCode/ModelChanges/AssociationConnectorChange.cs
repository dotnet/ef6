// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using AssociationConnector = Microsoft.Data.Entity.Design.EntityDesigner.View.AssociationConnector;

    internal class AssociationConnectorChange : AssociationConnectorModelChange
    {
        private readonly Guid _domainPropertyId;

        internal AssociationConnectorChange(AssociationConnector associationConnector, Guid domainPropertyId)
            : base(associationConnector)
        {
            _domainPropertyId = domainPropertyId;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, AssociationConnector, _domainPropertyId);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, AssociationConnector associationConnector, Guid domainPropertyId)
        {
            var viewModel = associationConnector.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from AssociationConnector: " + associationConnector);

            if (viewModel != null)
            {
                Connector modelAssociationConnector =
                    viewModel.ModelXRef.GetExisting(associationConnector) as Model.Designer.AssociationConnector;
                if (modelAssociationConnector == null)
                {
                    AssociationConnectorAdd.StaticInvoke(cpc, associationConnector);
                    modelAssociationConnector = viewModel.ModelXRef.GetExisting(associationConnector) as Model.Designer.AssociationConnector;
                }

                Debug.Assert(modelAssociationConnector != null);
                if (modelAssociationConnector != null)
                {
                    if (domainPropertyId == LinkShape.EdgePointsDomainPropertyId)
                    {
                        List<KeyValuePair<double, double>> points = null;
                        if (associationConnector.ManuallyRouted
                            && associationConnector.EdgePoints.Count > 0)
                        {
                            points = new List<KeyValuePair<double, double>>(associationConnector.EdgePoints.Count);
                            foreach (EdgePoint point in associationConnector.EdgePoints)
                            {
                                points.Add(new KeyValuePair<double, double>(point.Point.X, point.Point.Y));
                            }
                        }

                        if (points != null)
                        {
                            var cmd = new SetConnectorPointsCommand(modelAssociationConnector, points);
                            CommandProcessor.InvokeSingleCommand(cpc, cmd);
                        }
                    }
                    else if (domainPropertyId == LinkShape.ManuallyRoutedDomainPropertyId)
                    {
                        // if the connectors are not manually routed, we need to clean up all the connector points in the association connectors.
                        if (associationConnector.ManuallyRouted == false
                            && modelAssociationConnector.ConnectorPoints != null
                            && modelAssociationConnector.ConnectorPoints.Count > 0)
                        {
                            var points = new List<KeyValuePair<double, double>>();
                            var setConnectorPointCmd = new SetConnectorPointsCommand(modelAssociationConnector, points);
                            CommandProcessor.InvokeSingleCommand(cpc, setConnectorPointCmd);
                        }

                        var cmd = new UpdateDefaultableValueCommand<bool>(
                            modelAssociationConnector.ManuallyRouted, associationConnector.ManuallyRouted);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 240; }
        }
    }
}
