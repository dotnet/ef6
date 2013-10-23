// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Commands
{
    using System.Windows.Input;

    internal static class WorkspaceCommands
    {
        public static RoutedCommand ZoomIn = new RoutedCommand("ZoomIn", typeof(WorkspaceCommands));
        public static RoutedCommand ZoomOut = new RoutedCommand("ZoomOut", typeof(WorkspaceCommands));
        public static RoutedCommand Activate = new RoutedCommand("Activate", typeof(WorkspaceCommands));
        public static RoutedCommand PutInRenameMode = new RoutedCommand("PutInRenameMode", typeof(WorkspaceCommands));
#if VIEWSOURCE
        public static RoutedCommand ViewSource = new RoutedCommand("ViewSource", typeof(WorkspaceCommands));
#endif
    }
}
