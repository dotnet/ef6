// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;

    // Event handler that knows about CommandProcessorContext
    internal delegate void CommandEventHandler(object sender, CommandEventArgs args);

    internal class CommandEventArgs : EventArgs
    {
        internal CommandEventArgs(CommandProcessorContext cpc)
        {
            CommandProcessorContext = cpc;
        }

        internal CommandProcessorContext CommandProcessorContext { get; set; }
    }
}
