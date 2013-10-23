// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.VisualStudio.Shell.Interop;

    internal interface IEntityDesignDocData
    {
        bool CreateAndLoadBuffer();
        string GetBufferTextForSaving();
        void EnableDiagramEdits(bool canEdit);
        void EnsureDiagramIsCreated(EFArtifact artifact);
        IVsHierarchy Hierarchy { get; }
        uint ItemId { get; }
        string BackupFileName { get; }
    }
}
