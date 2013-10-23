// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;

    internal delegate int ModelChangeEventHandler(object sender, ModelChangeEventArgs args);

    internal sealed class ModelChangeEventArgs : EventArgs
    {
        internal string OldFileName { get; set; }

        internal string NewFileName { get; set; }

        internal uint DocCookie { get; set; }

        internal string OldEntityContainerName { get; set; }

        internal string NewEntityContainerName { get; set; }

        internal string OldMetadataArtifactProcessing { get; set; }

        internal bool IsCurrentlyBuilding { get; set; }

        internal EFArtifact Artifact { get; set; }

        internal Project ProjectObj { get; set; }
    }
}
