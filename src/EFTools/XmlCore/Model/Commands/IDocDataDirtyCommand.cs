// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    /// <summary>
    ///     Interface that allows a command to report whether or not it has caused the artifact to become dirty.
    /// </summary>
    internal interface IDocDataDirtyCommand
    {
        bool IsDocDataDirty { get; }
    }
}
