// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System.Collections.Generic;

    internal interface IEntityDesignerCommandFactory
    {
        /// <summary>
        ///     Commands that will be surfaced in the Entity Designer
        /// </summary>
        IList<EntityDesignerCommand> Commands { get; }
    }
}
