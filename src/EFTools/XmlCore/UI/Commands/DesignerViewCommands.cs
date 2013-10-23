// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Commands
{
    using System.Windows.Input;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class DesignerViewCommands
    {
        public static readonly RoutedUICommand ChangeCenter =
            new RoutedUICommand(Resources.DesignerViewCommandsText, "ChangeCenter", typeof(DesignerViewCommands));
    }
}
