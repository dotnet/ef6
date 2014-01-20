// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.ComponentModel.Design;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.VisualStudio.Modeling.Shell;

    internal interface IEntityDesignCommandSet
    {
        // <summary>
        //     Enables callers from outside the package to dynamically add commands
        // </summary>
        // <returns>bool indicating whether the command was added</returns>
        bool AddCommand(CommandID commandIdNum, EntityDesignerCommand command, out DynamicStatusMenuCommand menuCommand);

        // <summary>
        //     Enables callers from outside the package to dynamically remove commands
        // </summary>
        // <returns>bool indicating whether the command was found and removed</returns>
        bool RemoveCommand(CommandID commandIdNum);
    }
}
