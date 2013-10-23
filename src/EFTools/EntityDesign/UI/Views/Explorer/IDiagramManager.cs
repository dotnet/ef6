// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Collections.Generic;

    internal interface IDiagramManager
    {
        void OpenDiagram(string diagramMoniker, bool openInNewTab);
        void CloseDiagram(string diagramMoniker);
        void CloseAllDiagrams();
        IViewDiagram ActiveDiagram { get; }
        IViewDiagram FirstOpenDiagram { get; }
        IEnumerable<IViewDiagram> OpenDiagrams { get; }
    }
}
