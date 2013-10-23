// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Designer
{
    using Microsoft.Data.Entity.Design.Model.Eventing;

    /// <summary>
    ///     This class contains the diagram information where the command transaction is originated.
    /// </summary>
    internal class DiagramContextItem : ITransactionContextItem
    {
        internal DiagramContextItem(string diagramId, bool containsDiagramChanges = true)
        {
            DiagramId = diagramId;
            ContainsDiagramChanges = containsDiagramChanges;
        }

        internal string DiagramId { get; private set; }

        internal bool ContainsDiagramChanges { get; private set; }
    }
}
