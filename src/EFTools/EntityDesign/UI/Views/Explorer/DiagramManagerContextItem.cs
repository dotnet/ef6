// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;

    // <summary>
    //     Context Item that contains IDiagramManager instance.
    //     This is used by ModelBrowser/Explorer code to do diagram operation.
    //     (for example: OpenDiagram, CloseDiagram, CloseAllDiagrams).
    // </summary>
    internal class DiagramManagerContextItem : ContextItem
    {
        private IDiagramManager _diagramManager;

        internal void SetViewManager(IDiagramManager diagramManager)
        {
            _diagramManager = diagramManager;
        }

        internal IDiagramManager DiagramManager
        {
            get { return _diagramManager; }
        }

        internal override Type ItemType
        {
            get { return typeof(DiagramManagerContextItem); }
        }
    }
}
