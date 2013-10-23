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
    using InheritanceConnector = Microsoft.Data.Entity.Design.EntityDesigner.View.InheritanceConnector;

    internal class InheritanceConnectorChange : InheritanceConnectorModelChange
    {
        private readonly Guid _domainPropertyId;

        internal InheritanceConnectorChange(InheritanceConnector inheritanceConnector, Guid domainPropertyId)
            : base(inheritanceConnector)
        {
            _domainPropertyId = domainPropertyId;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, InheritanceConnector, _domainPropertyId);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, InheritanceConnector inheritanceConnector, Guid domainPropertyId)
        {
            // The situation where inheritanceConnector.IsDeleted to be true is when the user is trying to create circular inheritance.
            // In that case, the inheritance connector creation is aborted but this rule could still be fired.
            if (inheritanceConnector.IsDeleted
                || inheritanceConnector.IsDeleting)
            {
                return;
            }

            Connector modelInheritanceConnector = null;

            var viewModel = inheritanceConnector.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from inheritance connector: " + viewModel);
            if (viewModel != null)
            {
                modelInheritanceConnector = viewModel.ModelXRef.GetExisting(inheritanceConnector) as Model.Designer.InheritanceConnector;
                if (modelInheritanceConnector == null)
                {
                    InheritanceConnectorAdd.StaticInvoke(cpc, inheritanceConnector);
                    modelInheritanceConnector = viewModel.ModelXRef.GetExisting(inheritanceConnector) as Model.Designer.InheritanceConnector;
                }
            }

            // we should have a connector unless its been deleted due to circular inheritance checks
            Debug.Assert(
                modelInheritanceConnector != null || (modelInheritanceConnector == null && inheritanceConnector.IsDeleted),
                "We could not locate an underlying model item to change for this Inheritance connector");
            if (modelInheritanceConnector != null)
            {
                if (domainPropertyId == LinkShape.EdgePointsDomainPropertyId)
                {
                    List<KeyValuePair<double, double>> points = null;
                    if (inheritanceConnector.ManuallyRouted
                        && inheritanceConnector.EdgePoints.Count > 0)
                    {
                        points = new List<KeyValuePair<double, double>>(inheritanceConnector.EdgePoints.Count);
                        foreach (EdgePoint point in inheritanceConnector.EdgePoints)
                        {
                            points.Add(new KeyValuePair<double, double>(point.Point.X, point.Point.Y));
                        }
                    }

                    if (points != null)
                    {
                        var cmd = new SetConnectorPointsCommand(modelInheritanceConnector, points);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
                else if (domainPropertyId == LinkShape.ManuallyRoutedDomainPropertyId)
                {
                    // if the connectors are not manually routed (the connectors are auto layout), we need to clear our the layout information in our model.
                    if (inheritanceConnector.ManuallyRouted == false
                        && modelInheritanceConnector.ConnectorPoints != null
                        && modelInheritanceConnector.ConnectorPoints.Count > 0)
                    {
                        var points = new List<KeyValuePair<double, double>>();
                        var setConnectorPointscmd = new SetConnectorPointsCommand(modelInheritanceConnector, points);
                        CommandProcessor.InvokeSingleCommand(cpc, setConnectorPointscmd);
                    }

                    var cmd = new UpdateDefaultableValueCommand<bool>(
                        modelInheritanceConnector.ManuallyRouted, inheritanceConnector.ManuallyRouted);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 240; }
        }
    }
}
