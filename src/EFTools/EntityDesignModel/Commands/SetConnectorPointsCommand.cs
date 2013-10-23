// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal class SetConnectorPointsCommand : Command
    {
        private readonly Connector _connector;
        private readonly List<KeyValuePair<double, double>> _edgePoints;

        internal SetConnectorPointsCommand(Connector connector, List<KeyValuePair<double, double>> edgePoints)
        {
            Debug.Assert(connector != null, "connector is null");

            _connector = connector;
            _edgePoints = edgePoints;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // recycle connectors to avoid having rebinding the model
            var connectorPoints = new List<ConnectorPoint>(_connector.ConnectorPoints);
            if (connectorPoints.Count > _edgePoints.Count)
            {
                var diff = _connector.ConnectorPoints.Count - _edgePoints.Count;
                for (var i = 0; i < diff; i++)
                {
                    var lastIdx = connectorPoints.Count - 1;
                    var cp = connectorPoints[lastIdx];
                    DeleteEFElementCommand.DeleteInTransaction(cpc, cp);
                    connectorPoints.RemoveAt(lastIdx);
                }
            }
            else if (connectorPoints.Count < _edgePoints.Count)
            {
                var diff = _edgePoints.Count - connectorPoints.Count;
                for (var i = 0; i < diff; i++)
                {
                    var cp = new ConnectorPoint(_connector, null);
                    _connector.AddConnectorPoint(cp);
                    XmlModelHelper.NormalizeAndResolve(cp);
                    connectorPoints.Add(cp);
                }
            }

            Debug.Assert(connectorPoints.Count == _edgePoints.Count, "ConnectorPoints.Count != edgePoints.Count");

            // be safe if the assert above fires, only go through min indices
            var min = Math.Min(_edgePoints.Count, connectorPoints.Count);
            for (var i = 0; i < min; i++)
            {
                var edgePoint = _edgePoints[i];
                var connectorPoint = connectorPoints[i];
                connectorPoint.PointX.Value = edgePoint.Key;
                connectorPoint.PointY.Value = edgePoint.Value;
            }
        }
    }
}
